using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace DecorBlishhudModule
{
    [Export(typeof(Module))]
    public class DecorModule : Module
    {
        private static readonly Logger Logger = Logger.GetLogger<DecorModule>();

        private static readonly HttpClient client = new HttpClient();

        private Image _decorationIcon;
        private Image _decorationImage;
        private Texture2D _homesteadIconTexture;
        private Texture2D _homesteadBigIconTexture;
        private CornerIcon _cornerIcon;
        private StandardWindow _decorWindow;
        private LoadingSpinner _loadingSpinner;
        private SignatureLabelManager _signatureLabelManager;

        internal static DecorModule DecorModuleInstance;

        internal SettingsManager SettingsManager => this.ModuleParameters.SettingsManager;
        internal ContentsManager ContentsManager => this.ModuleParameters.ContentsManager;
        internal DirectoriesManager DirectoriesManager => this.ModuleParameters.DirectoriesManager;
        internal Gw2ApiManager Gw2ApiManager => this.ModuleParameters.Gw2ApiManager;

        [ImportingConstructor]
        public DecorModule([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters)
        {
            DecorModuleInstance = this;
        }

        protected override async Task LoadAsync()
        {
            _homesteadIconTexture = ContentsManager.GetTexture("test/homestead_icon.png");
            _homesteadBigIconTexture = ContentsManager.GetTexture("test/homestead_big_icon.png");

            _cornerIcon = CornerIconHelper.CreateLoadingIcon(_homesteadIconTexture, _homesteadBigIconTexture, _decorWindow, out _loadingSpinner);

            _loadingSpinner.Visible = true;

            var windowBackgroundTexture = AsyncTexture2D.FromAssetId(155997);
            await CreateGw2StyleWindowThatDisplaysAllDecorations(windowBackgroundTexture);

            _loadingSpinner.Visible = false;

            _cornerIcon.Click += (s, e) =>
            {
                if (_decorWindow != null)
                {
                    if (_decorWindow.Visible)
                    {
                        _decorWindow.Hide();
                    }
                    else
                    {
                        _decorWindow.Show();
                    }
                }
            };

            _signatureLabelManager = new SignatureLabelManager(_decorWindow);
        }


        protected override void Unload()
        {
            _loadingSpinner?.Dispose();
            _homesteadIconTexture?.Dispose();
            _homesteadBigIconTexture?.Dispose();
            _cornerIcon?.Dispose();
            _decorWindow?.Dispose();
            _decorationIcon?.Dispose();
            _decorationImage?.Dispose();
            DecorModuleInstance = null;
        }

        // CREATE GW2 STYLE WINDOW
        private async Task CreateGw2StyleWindowThatDisplaysAllDecorations(AsyncTexture2D windowBackgroundTexture)
        {
            _decorWindow = new StandardWindow(
                windowBackgroundTexture,
                new Rectangle(25, 26, 555, 640),
                new Rectangle(40, 50, 550, 640),
                new Point(1100, 800))
            {
                Parent = GameService.Graphics.SpriteScreen,
                Title = "Decor",
                Emblem = _homesteadIconTexture,
                Subtitle = "Homestead Decorations",
                Location = new Point(300, 300),
                SavesPosition = true,
                Id = $"{nameof(DecorModule)}_Decoration_Window"
            };

            var searchTextBox = new TextBox
            {
                Parent = _decorWindow,
                Location = new Point(10, 0),
                Width = 200,
                PlaceholderText = "Search Decorations..."
            };

            var decorationsFlowPanel = new FlowPanel
            {
                FlowDirection = ControlFlowDirection.LeftToRight,
                Width = 500,
                Height = 640,
                CanScroll = true,
                Parent = _decorWindow,
                Location = new Point(10, searchTextBox.Bottom + 10)
            };

            var decorationRightText = new Label
            {
                Parent = _decorWindow,
                Width = 500,
                Height = 120,
                WrapText = true,
                StrokeText = true,
                ShowShadow = true,
                ShadowColor = new Color(0, 0, 0),
                Font = GameService.Content.DefaultFont18
            };

            CenterTextInParent(decorationRightText, _decorWindow);

            _decorationIcon = new Image
            {
                Size = new Point(40, 40),
                Parent = _decorWindow,
                Location = new Point(decorationRightText.Left, decorationRightText.Bottom + 5)
            };

            _decorationImage = new Image
            {
                Size = new Point(400, 400),
                Parent = _decorWindow,
                Location = new Point(decorationRightText.Left, _decorationIcon.Bottom + 5)
            };

            List<string> categories = Categories.GetCategories();
            foreach (string category in categories)
            {
                await ProcessCategoryAsync(category, decorationsFlowPanel);
            }

            searchTextBox.TextChanged += async (sender, args) =>
            {
                string searchText = searchTextBox.Text.ToLower();
                await FilterDecorations(decorationsFlowPanel, searchText);
            };

            _decorWindow.Show();
        }

        // Left Panel Operations
        private async Task ProcessCategoryAsync(string category, FlowPanel decorationsFlowPanel)
        {
            string formattedCategoryName = category.Replace(" ", "_");

            string categoryUrl = $"https://wiki.guildwars2.com/api.php?action=parse&page=Decoration/Homestead/{formattedCategoryName}&format=json&prop=text";
            var decorations = await DecorationFetcher.FetchDecorationsAsync(categoryUrl);

            if (!decorations.Any())
            {
                Logger.Info($"Category '{category}' has no decorations and will not be displayed.");
                return;
            }

            int baseHeight = 45;
            int heightIncrementPerDecorationSet = 52;
            int numDecorationSets = (int)Math.Ceiling(decorations.Count / 9.0);
            int calculatedHeight = baseHeight + (numDecorationSets * heightIncrementPerDecorationSet);

            var categoryFlowPanel = new FlowPanel
            {
                Title = category,
                FlowDirection = ControlFlowDirection.LeftToRight,
                Width = decorationsFlowPanel.Width - 20,
                Height = calculatedHeight,
                CanCollapse = false,
                Parent = decorationsFlowPanel,
                ControlPadding = new Vector2(4, 4)
            };

            var decorationIconTasks = decorations.Where(d => !string.IsNullOrEmpty(d.IconUrl))
                .Select(decoration => CreateDecorationIconAsync(decoration, categoryFlowPanel))
                .ToList();

            await Task.WhenAll(decorationIconTasks);
        }

        private async Task CreateDecorationIconAsync(Decoration decoration, FlowPanel categoryFlowPanel)
        {
            try
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0");
                var iconResponse = await client.GetByteArrayAsync(decoration.IconUrl);

                using (var memoryStream = new MemoryStream(iconResponse))
                using (var graphicsContext = GameService.Graphics.LendGraphicsDeviceContext())
                {
                    var iconTexture = Texture2D.FromStream(graphicsContext.GraphicsDevice, memoryStream);

                    var borderPanel = new Panel
                    {
                        Size = new Point(49, 49),
                        BackgroundColor = Color.Black,
                        Parent = categoryFlowPanel
                    };

                    var decorationIconImage = new Image(iconTexture)
                    {
                        BasicTooltipText = decoration.Name,
                        Size = new Point(45),
                        Location = new Point(2, 2),
                        Parent = borderPanel
                    };

                    decorationIconImage.Click += async (s, e) =>
                    {
                        await UpdateDecorationImageAsync(decoration);
                    };
                }
            }
            catch (Exception ex)
            {
                Logger.Warn($"Failed to load decoration icon for '{decoration.Name}'. Error: {ex.Message}");
            }
        }

        private async Task FilterDecorations(FlowPanel decorationsFlowPanel, string searchText)
        {
            searchText = searchText.ToLower();

            foreach (var categoryFlowPanel in decorationsFlowPanel.Children.OfType<FlowPanel>())
            {
                bool hasVisibleDecoration = false;

                var visibleDecorations = new List<Panel>();

                foreach (var decorationIconPanel in categoryFlowPanel.Children.OfType<Panel>())
                {
                    var decorationIcon = decorationIconPanel.Children.OfType<Image>().FirstOrDefault();

                    if (decorationIcon != null)
                    {
                        bool matchesSearch = decorationIcon.BasicTooltipText?.ToLower().Contains(searchText) ?? false;

                        decorationIconPanel.Visible = matchesSearch;

                        if (matchesSearch)
                        {
                            visibleDecorations.Add(decorationIconPanel);
                            hasVisibleDecoration = true;
                        }
                    }
                }

                categoryFlowPanel.Visible = hasVisibleDecoration;

                if (hasVisibleDecoration)
                {
                    foreach (var visibleDecoration in visibleDecorations)
                    {
                        categoryFlowPanel.Children.Remove(visibleDecoration);
                    }

                    foreach (var visibleDecoration in visibleDecorations)
                    {
                        categoryFlowPanel.Children.Insert(0, visibleDecoration);
                    }

                    await AdjustCategoryHeightAsync(categoryFlowPanel);

                    categoryFlowPanel.Invalidate();
                }
            }

            decorationsFlowPanel.Invalidate();
        }

        // Right Panel Operations
        private async Task UpdateDecorationImageAsync(Decoration decoration)
        {
            var decorationNameLabel = _decorWindow.Children.OfType<Label>().FirstOrDefault();
            decorationNameLabel.Text = "";

            CenterTextInParent(decorationNameLabel, _decorWindow);

            _decorationImage.Texture = null;
            AdjustImageSize(null);

            if (!string.IsNullOrEmpty(decoration.ImageUrl))
            {
                try
                {
                    var imageResponse = await client.GetByteArrayAsync(decoration.ImageUrl);

                    using (var memoryStream = new MemoryStream(imageResponse))
                    using (var graphicsContext = GameService.Graphics.LendGraphicsDeviceContext())
                    {
                        var originalTexture = Texture2D.FromStream(graphicsContext.GraphicsDevice, memoryStream);

                        int borderWidth = 15;
                        Color innerBorderColor = new Color(86, 76, 55);
                        Color outerBorderColor = Color.Black;

                        int borderedWidth = originalTexture.Width + 2 * borderWidth;
                        int borderedHeight = originalTexture.Height + 2 * borderWidth;

                        var borderedTexture = new Texture2D(graphicsContext.GraphicsDevice, borderedWidth, borderedHeight);
                        Color[] borderedColorData = new Color[borderedWidth * borderedHeight];

                        for (int y = 0; y < borderedHeight; y++)
                        {
                            for (int x = 0; x < borderedWidth; x++)
                            {
                                int distanceFromEdge = Math.Min(Math.Min(x, borderedWidth - x - 1), Math.Min(y, borderedHeight - y - 1));
                                if (distanceFromEdge < borderWidth)
                                {
                                    float gradientFactor = (float)distanceFromEdge / borderWidth;
                                    borderedColorData[y * borderedWidth + x] = Color.Lerp(outerBorderColor, innerBorderColor, gradientFactor);
                                }
                                else
                                {
                                    borderedColorData[y * borderedWidth + x] = Color.Transparent;
                                }
                            }
                        }

                        Color[] originalColorData = new Color[originalTexture.Width * originalTexture.Height];
                        originalTexture.GetData(originalColorData);

                        for (int y = 0; y < originalTexture.Height; y++)
                        {
                            for (int x = 0; x < originalTexture.Width; x++)
                            {
                                int borderedIndex = (y + borderWidth) * borderedWidth + (x + borderWidth);
                                borderedColorData[borderedIndex] = originalColorData[y * originalTexture.Width + x];
                            }
                        }

                        borderedTexture.SetData(borderedColorData);
                        _decorationImage.Texture = borderedTexture;

                        AdjustImageSize(borderedTexture);
                        CenterImageInParent(_decorationImage, _decorWindow);

                        decorationNameLabel.Text = decoration.Name ?? "Unknown Decoration";
                        CenterTextInParent(decorationNameLabel, _decorWindow);

                        PositionTextAboveImage(decorationNameLabel, _decorationImage);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warn($"Failed to load decoration image for '{decoration.Name}'. Error: {ex.ToString()}");

                    decorationNameLabel.Text = "Error Loading Decoration";
                    CenterTextInParent(decorationNameLabel, _decorWindow);
                }
            }
            else
            {
                _decorationImage.Texture = null;
                AdjustImageSize(null);

                decorationNameLabel.Text = decoration.Name ?? "Unknown Decoration";
                CenterTextInParent(decorationNameLabel, _decorWindow);
            }
        }

        private Task AdjustCategoryHeightAsync(FlowPanel categoryFlowPanel)
        {
            int visibleDecorationCount = categoryFlowPanel.Children.OfType<Panel>().Count(p => p.Visible);

            if (visibleDecorationCount == 0)
            {
                categoryFlowPanel.Height = 45;
            }
            else
            {
                int baseHeight = 45;
                int heightIncrementPerDecorationSet = 52;
                int numDecorationSets = (int)Math.Ceiling(visibleDecorationCount / 9.0);
                int calculatedHeight = baseHeight + numDecorationSets * heightIncrementPerDecorationSet;

                categoryFlowPanel.Height = calculatedHeight;

                categoryFlowPanel.Invalidate();
            }

            return Task.CompletedTask;
        }

        private void CenterTextInParent(Label label, Control parent)
        {
            var font = GameService.Content.DefaultFont18;
            var textSize = font.MeasureString(label.Text);
            float textWidth = textSize.Width;

            int parentWidth = parent.Width;

            int startX = (460 + parentWidth - (int)textWidth) / 2;
            if (label.Text == "Loading...")
            {
                label.Location = new Point(startX, label.Location.Y + 300);
            }
            else
            {
                label.Location = new Point(startX, label.Location.Y);
            }
        }

        private void PositionTextAboveImage(Label text, Image image)
        {
            int textY = image.Location.Y - text.Height + 30;
            text.Location = new Point(text.Location.X, textY);
        }

        private void CenterImageInParent(Image image, Control parent)
        {
            int centerX = (parent.Width - image.Size.X) / 2;
            int centerY = (parent.Height - image.Size.Y) / 2;

            image.Location = new Point(centerX + 230, centerY - 40);
        }

        private void AdjustImageSize(Texture2D loadedTexture)
        {
            if (loadedTexture == null)
            {
                return;
            }

            int maxDimension = 500;
            float aspectRatio = (float)loadedTexture.Width / loadedTexture.Height;

            int targetWidth, targetHeight;

            if (loadedTexture.Width > loadedTexture.Height)
            {
                targetWidth = maxDimension;
                targetHeight = (int)(maxDimension / aspectRatio);
            }
            else
            {
                targetHeight = maxDimension;
                targetWidth = (int)(maxDimension * aspectRatio);
            }

            _decorationImage.Size = new Point(targetWidth, targetHeight);
        }

    }
}