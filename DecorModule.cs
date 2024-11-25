using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using DecorBlishhudModule.Homestead;
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

        public StandardWindow DecorWindow => _decorWindow;
        public Image DecorationImage => _decorationImage;


        // Switch to turn Homestead preview to Guild Hall preview
        private bool _isHomestead = true;

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
            await RightSideMethods.UpdateDecorationImageAsync(homesteadImagePlaceholder, _decorWindow, _decorationImage);

            if (_isHomestead == true)
            {
                await Task.Delay(4000);
            }

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

            var toggleButton = new StandardButton
            {
                Parent = _decorWindow,
                Location = new Point(searchTextBox.Right + 10, 0),
                Text = "Guild Hall Decorations",
                Width = 150
            };

            var homesteadDecorationsFlowPanel = new FlowPanel
            {
                FlowDirection = ControlFlowDirection.LeftToRight,
                ShowBorder = true,
                Width = 500,
                Height = 660,
                CanScroll = true,
                Parent = _decorWindow,
                Location = new Point(10, searchTextBox.Bottom + 10),
                Visible = true // Initially visible
            };

            var guildHallDecorationsFlowPanel = new FlowPanel
            {
                FlowDirection = ControlFlowDirection.LeftToRight,
                ShowBorder = true,
                Width = 500,
                Height = 660,
                CanScroll = true,
                Parent = _decorWindow,
                Location = new Point(10, searchTextBox.Bottom + 10),
                Visible = false // Initially hidden
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

            if (_isHomestead)
            {
                string url = "https://wiki.guildwars2.com/api.php?action=parse&page=Decoration/Homestead&format=json&prop=text";
                var decorationsByCategory = await HomesteadDecorationFetcher.FetchDecorationsAsync(url);

                List<string> predefinedCategories = HomesteadCategories.GetCategories();

                var caseInsensitiveCategories = decorationsByCategory
                    .ToDictionary(kvp => kvp.Key.ToLower(), kvp => kvp.Value);

                foreach (var category in predefinedCategories)
                {
                    string categoryLower = category.ToLower();

                    if (caseInsensitiveCategories.ContainsKey(categoryLower))
                    {
                        LeftSideMethods.HomesteadIconsInFlowPanel(category, caseInsensitiveCategories[categoryLower], homesteadDecorationsFlowPanel);
                    }
                }
            }
            else
            {
                List<string> categories = GuildHallCategories.GetCategories();

                foreach (string category in categories)
                {
                    await LeftSideMethods.GuildHallIconsInFlowPanel(category, guildHallDecorationsFlowPanel);
                }
            }

            // Populate initially based on default (_isHomestead = true)
            await LeftSideMethods.PopulateDecorations(homesteadDecorationsFlowPanel, _isHomestead);
            await LeftSideMethods.PopulateDecorations(guildHallDecorationsFlowPanel, !_isHomestead);

            // Toggle button click event
            toggleButton.Click += (s, e) =>
            {
                _isHomestead = !_isHomestead;
                toggleButton.Text = _isHomestead ? "Guild Hall Decorations" : "Homestead Decorations";
                _decorWindow.Subtitle = _isHomestead ? "Homestead Decorations" : "Guild Hall Decorations";

                // Update the button width based on the new text
                if (toggleButton.Text == "Guild Hall Decorations") { toggleButton.Width = 150; }
                else if (toggleButton.Text == "Homestead Decorations") { toggleButton.Width = 170; }

                // Hide the current panel and show the other
                homesteadDecorationsFlowPanel.Visible = _isHomestead;
                guildHallDecorationsFlowPanel.Visible = !_isHomestead;
            };

            searchTextBox.TextChanged += async (sender, args) =>
            {
                string searchText = searchTextBox.Text.ToLower();
                await LeftSideMethods.FilterDecorations(homesteadDecorationsFlowPanel, searchText);
                await LeftSideMethods.FilterDecorations(guildHallDecorationsFlowPanel, searchText);
            };
        }
    }
}