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

        private CornerIcon _cornerIcon;
        private Texture2D _homesteadIconTexture;
        private Texture2D _homesteadBigIconTexture;
        private LoadingSpinner _loadingSpinner;
        private StandardWindow _decorWindow;
        private Image _decorationIcon;
        private Image _decorationImage;
        private SignatureLabelManager _signatureLabelManager;
        private WikiLicenseLabelManager _wikiLicenseManager;

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
            // Load assets
            _homesteadIconTexture = ContentsManager.GetTexture("test/homestead_icon.png");
            _homesteadBigIconTexture = ContentsManager.GetTexture("test/homestead_big_icon.png");

            // Create corner icon and show loading spinner
            _cornerIcon = CornerIconHelper.CreateLoadingIcon(_homesteadIconTexture, _homesteadBigIconTexture, _decorWindow, out _loadingSpinner);
            _loadingSpinner.Visible = true;

            // Window background
            var windowBackgroundTexture = AsyncTexture2D.FromAssetId(155997);
            await CreateGw2StyleWindowThatDisplaysAllDecorations(windowBackgroundTexture);

            // Homestead placeholder decoration
            var homesteadImagePlaceholder = new Decoration
            {
                Name = "Welcome to Decor. Enjoy your stay!",
                IconUrl = null,
                ImageUrl = "https://i.imgur.com/VBAy1WA.jpeg"
            };

            // Update the placeholder image
            await UpdateDecorationImageAsync(homesteadImagePlaceholder);

            await Task.Delay(8000);

            // Hide loading spinner when decorations are ready
            _loadingSpinner.Visible = false;

            // Toggle window visibility when corner icon is clicked
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

            // Set up signature and license labels
            _signatureLabelManager = new SignatureLabelManager(_decorWindow);
            _wikiLicenseManager = new WikiLicenseLabelManager(_decorWindow);
        }


        protected override void Unload()
        {
            _cornerIcon?.Dispose();
            _homesteadIconTexture?.Dispose();
            _homesteadBigIconTexture?.Dispose();
            _loadingSpinner?.Dispose();
            _decorWindow?.Dispose();
            _decorationIcon?.Dispose();
            _decorationImage?.Dispose();
            DecorModuleInstance = null;
        }

        // Style Window
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
                ShowBorder = true,
                Width = 500,
                Height = 660,
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

            RightSideMethods.CenterTextInParent(decorationRightText, _decorWindow);

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

            string url = "https://wiki.guildwars2.com/api.php?action=parse&page=Decoration/Homestead&format=json&prop=text";
            var decorationsByCategory = await DecorationFetcher.FetchDecorationsAsync(url);

            List<string> predefinedCategories = Categories.GetCategories();

            var caseInsensitiveCategories = decorationsByCategory
                .ToDictionary(kvp => kvp.Key.ToLower(), kvp => kvp.Value);

            foreach (var category in predefinedCategories)
            {
                string categoryLower = category.ToLower();

                if (caseInsensitiveCategories.ContainsKey(categoryLower))
                {
                    ReorderIconsInFlowPanel(category, caseInsensitiveCategories[categoryLower], decorationsFlowPanel);
                }
            }

            searchTextBox.TextChanged += async (sender, args) =>
            {
                string searchText = searchTextBox.Text.ToLower();
                await LeftSideMethods.FilterDecorations(decorationsFlowPanel, searchText);
            };
        }

        // Left Panel Operations
        private async void ReorderIconsInFlowPanel(string category, List<Decoration> decorations, FlowPanel decorationsFlowPanel)
        {
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

            foreach (var decoration in decorations)
            {
                await CreateDecorationIconAsync(decoration, categoryFlowPanel);
            }
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

        // Right Panel Operations
        private async Task UpdateDecorationImageAsync(Decoration decoration)
        {
            var decorationNameLabel = _decorWindow.Children.OfType<Label>().FirstOrDefault();
            decorationNameLabel.Text = "";

            RightSideMethods.CenterTextInParent(decorationNameLabel, _decorWindow);

            _decorationImage.Texture = null;
            RightSideMethods.AdjustImageSize(null, null);

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

                        RightSideMethods.AdjustImageSize(borderedTexture, _decorationImage);
                        RightSideMethods.CenterImageInParent(_decorationImage, _decorWindow);

                        decorationNameLabel.Text = decoration.Name.Replace(" ", " 🚪 ") ?? "Unknown Decoration";
                        RightSideMethods.CenterTextInParent(decorationNameLabel, _decorWindow);

                        RightSideMethods.PositionTextAboveImage(decorationNameLabel, _decorationImage);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warn($"Failed to load decoration image for '{decoration.Name}'. Error: {ex.ToString()}");

                    decorationNameLabel.Text = "Error Loading Decoration";
                    RightSideMethods.CenterTextInParent(decorationNameLabel, _decorWindow);
                }
            }
            else
            {
                _decorationImage.Texture = null;
                RightSideMethods.AdjustImageSize(null, null);

                decorationNameLabel.Text = decoration.Name ?? "Unknown Decoration";
                RightSideMethods.CenterTextInParent(decorationNameLabel, _decorWindow);
            }
        }
    }
}