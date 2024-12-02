using System.ComponentModel.Composition;
using System.Net.Http;
using System.Threading.Tasks;
using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using DecorBlishhudModule.Model;
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
        private Texture2D _homesteadIconMenu;
        private Texture2D _homesteadIconHover;
        private Texture2D _homesteadIconUnactive;
        private Texture2D _homesteadSwitch;
        private Texture2D _scribeSwitch;
        private Texture2D _backgroundSwitch;
        private Texture2D _info;
        private Texture2D _x;
        private LoadingSpinner _loadingSpinner;
        private StandardWindow _decorWindow;
        private Image _decorationIcon;
        private Image _decorationImage;
        private SignatureSection _signatureLabelManager;
        private WikiLicenseSection _wikiLicenseManager;

        public StandardWindow DecorWindow => _decorWindow;
        public Image DecorationImage => _decorationImage;
        public HttpClient Client => client;
        public Texture2D HomesteadSwitch => _homesteadSwitch;
        public Texture2D ScribeSwitch => _scribeSwitch;
        public Texture2D BackgroundSwitch => _backgroundSwitch;

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
            _homesteadIconUnactive = ContentsManager.GetTexture("test/homesteadIconUnactive.png");
            _homesteadIconHover = ContentsManager.GetTexture("test/homesteadIconHover.png");
            _homesteadIconMenu = ContentsManager.GetTexture("test/homesteadIconMenu.png");
            _homesteadSwitch = ContentsManager.GetTexture("test/homestead_switch.png");
            _scribeSwitch = ContentsManager.GetTexture("test/scribe_switch.png");
            _backgroundSwitch = ContentsManager.GetTexture("test/switch_background.png");
            _info = ContentsManager.GetTexture("test/info.png");
            _x = ContentsManager.GetTexture("test/x.png");

            // Create corner icon and show loading spinner
            _cornerIcon = CornerIconHelper.CreateLoadingIcon(_homesteadIconUnactive, _homesteadIconHover, _decorWindow, out _loadingSpinner);
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
            await RightSideSection.UpdateDecorationImageAsync(homesteadImagePlaceholder, _decorWindow, _decorationImage);

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

            // Hide loading spinner when decorations are ready
            _loadingSpinner.Visible = false;

            // Set up signature and license labels
            _signatureLabelManager = new SignatureSection(_decorWindow);
            _wikiLicenseManager = new WikiLicenseSection(_decorWindow);
        }


        protected override void Unload()
        {
            _cornerIcon?.Dispose();
            _homesteadIconMenu?.Dispose();
            _homesteadIconHover?.Dispose();
            _homesteadIconUnactive?.Dispose();
            _loadingSpinner?.Dispose();
            _decorWindow?.Dispose();
            _homesteadSwitch?.Dispose();
            _scribeSwitch?.Dispose();
            _backgroundSwitch.Dispose();
            _info.Dispose();
            _x.Dispose();
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
                new Point(1050, 800))
            {
                Parent = GameService.Graphics.SpriteScreen,
                Title = "Decor",
                Emblem = _homesteadIconMenu,
                Subtitle = "Homestead Decorations",
                Location = new Point(300, 300),
                SavesPosition = true,
                Id = $"{nameof(DecorModule)}_Decoration_Window"
            };

            var searchTextBox = new TextBox
            {
                Parent = _decorWindow,
                Location = new Point(20, 0),
                Width = 240,
                PlaceholderText = "Search Decorations..."
            };

            var clearButton = new Panel
            {
                Parent = _decorWindow,
                Location = new Point(searchTextBox.Right - 22, searchTextBox.Top + 5),
                Size = new Point(16, 16),
                Visible = false,
                BackgroundTexture = _x,
            };

            var infoText = new InfoSection(_decorWindow, _info);

            var homesteadDecorationsFlowPanel = new FlowPanel
            {
                Parent = _decorWindow,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                ShowBorder = true,
                Width = 500,
                Height = 660,
                CanScroll = true,
                Location = new Point(10, searchTextBox.Bottom + 10),
                Visible = true
            };

            var guildHallDecorationsFlowPanel = new FlowPanel
            {
                Parent = _decorWindow,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                ShowBorder = true,
                Width = 500,
                Height = 660,
                CanScroll = true,
                Location = new Point(10, searchTextBox.Bottom + 10),
                Visible = false
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


            RightSideSection.CenterTextInParent(decorationRightText, _decorWindow);

            _decorationIcon = new Image
            {
                Parent = _decorWindow,
                Size = new Point(40, 40),
                Location = new Point(decorationRightText.Left, decorationRightText.Bottom + 5)
            };

            _decorationImage = new Image
            {
                Parent = _decorWindow,
                Size = new Point(400, 400),
                Location = new Point(decorationRightText.Left, _decorationIcon.Bottom + 5)
            };

            //await LeftSideMethods.PopulateDecorations(homesteadDecorationsFlowPanel, guildHallDecorationsFlowPanel);

            // Search functionality
            searchTextBox.TextChanged += async (sender, args) =>
            {
                string searchText = searchTextBox.Text.ToLower();
                await LeftSideSection.FilterDecorations(homesteadDecorationsFlowPanel, searchText);
                await LeftSideSection.FilterDecorations(guildHallDecorationsFlowPanel, searchText);

                clearButton.Visible = !string.IsNullOrEmpty(searchText);
            };

            clearButton.Click += (s, e) =>
            {
                searchTextBox.Text = string.Empty;
            };

            SwitchSection switchLogic = new SwitchSection(searchTextBox, homesteadDecorationsFlowPanel, guildHallDecorationsFlowPanel);
        }
    }
}