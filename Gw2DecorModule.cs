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
using Blish_HUD.Settings;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Gw2DecorBlishhudModule
{
    [Export(typeof(Module))]
    public class Gw2DecorModule : Module
    {
        private static readonly Logger Logger = Logger.GetLogger<Gw2DecorModule>();

        private static readonly HttpClient client = new HttpClient();

        // Images and other resources
        private Image _decorationIcon;
        private Image _decorationImage;
        private Texture2D _mugTexture;
        private CornerIcon _cornerIcon;
        private ContextMenuStrip _contextMenuStrip;
        private StandardWindow _gw2DecorWindow;

        // Settings entries
        private SettingEntry<bool> _boolGw2DecorSetting;
        private SettingEntry<int> _valueRangeGw2DecorSetting;
        private SettingEntry<int> _hiddenIntGw2DecorSetting;
        private SettingEntry<int> _hiddenIntGw2DecorSetting2;
        private SettingEntry<string> _stringGw2DecorSetting;
        private SettingEntry<ColorType> _enumGw2DecorSetting;
        private SettingCollection _internalGw2DecorSettingSubCollection;

        internal static Gw2DecorModule Gw2DecorModuleInstance;

        internal SettingsManager SettingsManager => this.ModuleParameters.SettingsManager;
        internal ContentsManager ContentsManager => this.ModuleParameters.ContentsManager;
        internal DirectoriesManager DirectoriesManager => this.ModuleParameters.DirectoriesManager;
        internal Gw2ApiManager Gw2ApiManager => this.ModuleParameters.Gw2ApiManager;

        [ImportingConstructor]
        public Gw2DecorModule([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters)
        {
            Gw2DecorModuleInstance = this;
        }

        protected override void DefineSettings(SettingCollection settings)
        {
            settings.DefineSetting(
                "Gw2DecorSetting",
                "This is the default value of the setting",
                () => "Display name of setting",
                () => "Tooltip text of setting");

            _boolGw2DecorSetting = settings.DefineSetting(
                "bool gw2decor",
                true,
                () => "This is a bool setting (checkbox)",
                () => "Settings can be many different types");

            _stringGw2DecorSetting = settings.DefineSetting(
                "string gw2decor",
                "myText",
                () => "This is an string setting (textbox)",
                () => "Settings can be many different types");

            _valueRangeGw2DecorSetting = settings.DefineSetting(
                "int gw2decor",
                20,
                () => "This is an int setting (slider)",
                () => "Settings can be many different types");

            _valueRangeGw2DecorSetting.SetRange(0, 255);

            _enumGw2DecorSetting = settings.DefineSetting(
                "enum gw2decor",
                ColorType.Blue,
                () => "This is an enum setting (drop down menu)",
                () => "...");

            _boolGw2DecorSetting.Value = false;

            SettingEntry<string> setting1 = SettingsManager.ModuleSettings["Gw2DecorSetting"] as SettingEntry<string>;

            _internalGw2DecorSettingSubCollection = settings.AddSubCollection("internal settings (not visible in UI)");
            _hiddenIntGw2DecorSetting = _internalGw2DecorSettingSubCollection.DefineSetting("gw2decor window x position", 50);
            _hiddenIntGw2DecorSetting2 = _internalGw2DecorSettingSubCollection.DefineSetting("gw2decor window y position", 50);
        }

        protected override async Task LoadAsync()
        {
            foreach (string directoryName in DirectoriesManager.RegisteredDirectories)
            {
                string fullDirectoryPath = DirectoriesManager.GetFullDirectoryPath(directoryName);
                var allFiles = Directory.EnumerateFiles(fullDirectoryPath, "*", SearchOption.AllDirectories).ToList();

                Logger.Info($"'{directoryName}' can be found at '{fullDirectoryPath}' and has {allFiles.Count} total files within it.");
            }

            _mugTexture = ContentsManager.GetTexture("test/homestead_icon.png");

            var windowBackgroundTexture = AsyncTexture2D.FromAssetId(155997);

            await CreateGw2StyleWindowThatDisplaysAllDecorations(windowBackgroundTexture);
            _cornerIcon = CornerIconHelper.CreateCornerIconWithContextMenu(_mugTexture, _gw2DecorWindow);
        }

        protected override void Unload()
        {
            _cornerIcon?.Dispose();
            _contextMenuStrip?.Dispose();
            _decorationIcon?.Dispose();
            _decorationImage?.Dispose();
            _mugTexture?.Dispose();
            Gw2DecorModuleInstance = null;
        }

        // CREATE GW2 STYLE WINDOW
        private async Task CreateGw2StyleWindowThatDisplaysAllDecorations(AsyncTexture2D windowBackgroundTexture)
        {
            _gw2DecorWindow = new StandardWindow(
                windowBackgroundTexture,
                new Rectangle(25, 26, 560, 640),
                new Rectangle(40, 50, 540, 590),
                new Point(1200, 800))
            {
                Parent = GameService.Graphics.SpriteScreen,
                Title = "GW2Decor",
                Emblem = _mugTexture,
                Subtitle = "Homestead Decorations",
                Location = new Point(300, 300),
                SavesPosition = true,
                Id = $"{nameof(Gw2DecorModule)}_Decoration_Window"
            };

            var searchTextBox = new TextBox
            {
                Parent = _gw2DecorWindow,
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
                Parent = _gw2DecorWindow,
                Location = new Point(10, searchTextBox.Bottom + 10)
            };

            var decorationRightText = new Label
            {
                Text = "Select a decoration",
                Parent = _gw2DecorWindow,
                Location = new Point(decorationsFlowPanel.Right + 10, 20),
                Width = 200,
                Height = 40,
            };

            _decorationIcon = new Image
            {
                Size = new Point(40, 40),
                Parent = _gw2DecorWindow,
                Location = new Point(decorationRightText.Left, decorationRightText.Bottom + 5)
            };

            _decorationImage = new Image
            {
                Size = new Point(400, 400),
                Parent = _gw2DecorWindow,
                Location = new Point(decorationRightText.Left, _decorationIcon.Bottom + 5)
            };

            // Fetch categories asynchronously (parallelizing)
            List<string> categories = Categories.GetCategories();
            List<Task> categoryTasks = new List<Task>();

            foreach (string category in categories)
            {
                categoryTasks.Add(ProcessCategoryAsync(category, decorationsFlowPanel));
            }

            // Wait for all categories to be processed
            await Task.WhenAll(categoryTasks);

            // Handle search text box input to filter decorations
            searchTextBox.TextChanged += (sender, args) =>
            {
                string searchText = searchTextBox.Text.ToLower();
                FilterDecorations(decorationsFlowPanel, searchText);
            };

            _gw2DecorWindow.Show();
        }

        private async Task ProcessCategoryAsync(string category, FlowPanel decorationsFlowPanel)
        {
            string formattedCategoryName = category.Replace(" ", "_");

            string categoryUrl = $"https://wiki.guildwars2.com/api.php?action=parse&page=Decoration/Homestead/{formattedCategoryName}&format=json&prop=text";
            var decorations = await DecorationFetcher.FetchDecorationsAsync(categoryUrl);

            // If there are no decorations, skip creating a panel for this category
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
                CanCollapse = true,
                CanScroll = false,
                Parent = decorationsFlowPanel,
                ControlPadding = new Vector2(4, 4)
            };

            // Create decoration icons concurrently
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

                    // Create a border panel to wrap the icon
                    var borderPanel = new Panel
                    {
                        Size = new Point(49, 49),
                        BackgroundColor = Color.Black,
                        Parent = categoryFlowPanel
                    };

                    // Create the decoration icon with padding inside the border panel
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


        private async Task UpdateDecorationImageAsync(Decoration decoration)
        {
            var decorationNameLabel = _gw2DecorWindow.Children.OfType<Label>().FirstOrDefault();
            decorationNameLabel.Text = decoration.Name ?? "Unknown Decoration";

            if (!string.IsNullOrEmpty(decoration.ImageUrl))
            {
                try
                {
                    var imageResponse = await client.GetByteArrayAsync(decoration.ImageUrl);
                    using (var memoryStream = new MemoryStream(imageResponse))
                    using (var graphicsContext = GameService.Graphics.LendGraphicsDeviceContext())
                    {
                        var loadedTexture = Texture2D.FromStream(graphicsContext.GraphicsDevice, memoryStream);
                        _decorationImage.Texture = loadedTexture;

                        // Adjust size while maintaining aspect ratio
                        AdjustImageSize(loadedTexture);

                        // Center the image in its parent container (e.g., _gw2DecorWindow)
                        CenterImageInParent(_decorationImage, _gw2DecorWindow);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warn($"Failed to load decoration image for '{decoration.Name}'. Error: {ex.ToString()}");
                }
            }
        }

        private void CenterImageInParent(Image image, Control parent)
        {
            // Get the center position of the parent container
            int centerX = (parent.Width - image.Size.X) / 2;
            int centerY = (parent.Height - image.Size.Y) / 2;

            // Set the location of the image to the calculated center
            image.Location = new Point(centerX  + 230, centerY - 40);
        }

        private void AdjustImageSize(Texture2D loadedTexture)
        {
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

        private void FilterDecorations(FlowPanel decorationsFlowPanel, string searchText)
        {
            foreach (var categoryFlowPanel in decorationsFlowPanel.Children.OfType<FlowPanel>())
            {
                bool hasVisibleDecoration = false;

                foreach (var decorationIcon in categoryFlowPanel.Children.OfType<Image>())
                {
                    bool matchesSearch = decorationIcon.BasicTooltipText.ToLower().Contains(searchText);
                    decorationIcon.Visible = matchesSearch;

                    if (matchesSearch) hasVisibleDecoration = true;
                }

                categoryFlowPanel.Visible = hasVisibleDecoration;
                categoryFlowPanel.Invalidate();
            }

            decorationsFlowPanel.Invalidate();


            _gw2DecorWindow.Show();
        }

    }
}