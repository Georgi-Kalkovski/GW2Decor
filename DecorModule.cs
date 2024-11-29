using System;
using System.ComponentModel.Composition;
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
        private Texture2D _homesteadSwitch;
        private Texture2D _scribeSwitch;
        private Texture2D _switchBackground;
        private Texture2D _info;
        private Texture2D _x;
        private LoadingSpinner _loadingSpinner;
        private StandardWindow _decorWindow;
        private Image _decorationIcon;
        private Image _decorationImage;
        private SignatureLabelManager _signatureLabelManager;
        private WikiLicenseLabelManager _wikiLicenseManager;

        public StandardWindow DecorWindow => _decorWindow;
        public Image DecorationImage => _decorationImage;
        public HttpClient Client => client;

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
            _homesteadSwitch = ContentsManager.GetTexture("test/homestead_switch.png");
            _scribeSwitch = ContentsManager.GetTexture("test/scribe_switch.png");
            _switchBackground = ContentsManager.GetTexture("test/switch_background.png");
            _info = ContentsManager.GetTexture("test/info.png");
            _x = ContentsManager.GetTexture("test/x.png");

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

            // Hide loading spinner when decorations are ready
            _loadingSpinner.Visible = false;

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
            _homesteadSwitch?.Dispose();
            _scribeSwitch?.Dispose();
            _switchBackground.Dispose();
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
                Emblem = _homesteadIconTexture,
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
                Location = new Point(searchTextBox.Right - 22, searchTextBox.Top+ 5),
                Size = new Point(16, 16),
                Visible = false,
                BackgroundTexture = _x,
            };

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

            var homesteadSwitchText = new Label
            {
                Parent = _decorWindow,
                Text = "Homestead",
                Location = new Point(searchTextBox.Right + 20, 3),
                Width = 80,
                Height = 20,
                ShowShadow = true,
                WrapText = true,
                StrokeText = true,
                TextColor = new Color(254, 219, 114),
                ShadowColor = new Color(165, 123, 0),
                Font = GameService.Content.DefaultFont16,
            };

            var toggleSwitch = new Panel
            {
                Parent = _decorWindow,
                Location = new Point(homesteadSwitchText.Right + 5, -2),
                Size = new Point(60, 30),
                BackgroundTexture = _switchBackground,
            };

            var toggleIcon = new Panel
            {
                Parent = toggleSwitch,
                Size = new Point(25, 25),
                BackgroundTexture = _homesteadSwitch,
                Location = new Point(6, 2),
            };

            var guildhallSwitchText = new Label
            {
                Parent = _decorWindow,
                Text = "Guild Hall",
                Location = new Point(toggleSwitch.Right + 5, 3),
                Width = 70,
                Height = 20,
                ShowShadow = true,
                WrapText = true,
                StrokeText = true,
                TextColor = Color.LightGray,
                ShadowColor = new Color(0, 0, 0),
                Font = GameService.Content.DefaultFont16,
            };

            var infoIconPanel = new Panel
            {
                Parent = _decorWindow,
                Size = new Point(25, 25),
                BackgroundTexture = _info,
                Location = new Point(_decorWindow.Width - 90, 5),
            };

            var infoIcon = new Image
            {
                Parent = infoIconPanel,
                Width = 25,
                Height = 25
            };

            var infoTextPanel = new Panel
            {
                Parent = _decorWindow,
                Size = new Point(180, 70),
                Location = new Point(_decorWindow.Width - 270, -5),
                ShowBorder = true,
                Visible = false,
                Opacity = 0,
            };

            var infoText = new Label
            {
                Parent = infoTextPanel,
                Text = "Click the name or the image of the decoration to copy its name.",
                Location = new Point(5, 0),
                Width = 170,
                Height = 60,
                ShowShadow = true,
                WrapText = true,
                StrokeText = true,
                TextColor = Color.LightGray,
                ShadowColor = new Color(0, 0, 0),
                Font = GameService.Content.DefaultFont14,
            };

            RightSideMethods.CenterTextInParent(decorationRightText, _decorWindow);

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

            await LeftSideMethods.PopulateDecorations(homesteadDecorationsFlowPanel, guildHallDecorationsFlowPanel);

            infoIconPanel.MouseEntered += (sender, args) =>
            {
                AnimatePanel(infoTextPanel, true);
            };

            infoIconPanel.MouseLeft += (sender, args) =>
            {
                AnimatePanel(infoTextPanel, false);
            };

            // Add click event for the toggle switch
            toggleSwitch.Click += (s, e) =>
            {
                _isHomestead = !_isHomestead;

                // Update the position of the circle and background color
                if (_isHomestead)
                {
                    _decorWindow.Subtitle = "Homestead Decorations";

                    toggleIcon.BackgroundTexture = _homesteadSwitch;
                    toggleIcon.Location = new Point(6, 2);
                    toggleIcon.Size = new Point(25, 25);
                    homesteadSwitchText.TextColor = new Color(254, 219, 114);
                    homesteadSwitchText.ShadowColor = new Color(165, 123, 0);
                    guildhallSwitchText.TextColor = Color.LightGray;
                    guildhallSwitchText.ShadowColor = Color.Black;
                    homesteadDecorationsFlowPanel.Visible = true;
                    guildHallDecorationsFlowPanel.Visible = false;
                }
                else
                {
                    _decorWindow.Subtitle = "Guild Hall Decorations";

                    toggleIcon.BackgroundTexture = _scribeSwitch;
                    toggleIcon.Location = new Point(toggleSwitch.Width - toggleIcon.Width - 4, 5);
                    toggleIcon.Size = new Point(22, 22);
                    guildhallSwitchText.TextColor = new Color(168, 178, 230);
                    guildhallSwitchText.ShadowColor = new Color(40, 47, 85);
                    homesteadSwitchText.TextColor = Color.LightGray;
                    homesteadSwitchText.ShadowColor = Color.Black;
                    guildHallDecorationsFlowPanel.Visible = true;
                    homesteadDecorationsFlowPanel.Visible = false;
                }
            };

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

            // Search functionality
            searchTextBox.TextChanged += async (sender, args) =>
            {
                string searchText = searchTextBox.Text.ToLower();
                await LeftSideMethods.FilterDecorations(homesteadDecorationsFlowPanel, searchText);
                await LeftSideMethods.FilterDecorations(guildHallDecorationsFlowPanel, searchText);

                clearButton.Visible = !string.IsNullOrEmpty(searchText);
            };

            clearButton.Click += (s, e) =>
            {
                searchTextBox.Text = string.Empty;
            };

            // Add click event to homesteadSwitchText
            homesteadSwitchText.Click += (s, e) =>
            {
                _isHomestead = true;

                _decorWindow.Subtitle = "Homestead Decorations";
                toggleIcon.BackgroundTexture = _homesteadSwitch;
                toggleIcon.Location = new Point(6, 2);
                toggleIcon.Size = new Point(25, 25);

                homesteadSwitchText.TextColor = new Color(254, 219, 114);
                homesteadSwitchText.ShadowColor = new Color(165, 123, 0);
                guildhallSwitchText.TextColor = Color.LightGray;
                guildhallSwitchText.ShadowColor = Color.Black;

                homesteadDecorationsFlowPanel.Visible = true;
                guildHallDecorationsFlowPanel.Visible = false;
            };

            // Add click event to guildhallSwitchText
            guildhallSwitchText.Click += (s, e) =>
            {
                if (!_isHomestead) return; // Prevent redundant state changes.
                _isHomestead = false;

                _decorWindow.Subtitle = "Guild Hall Decorations";
                toggleIcon.BackgroundTexture = _scribeSwitch;
                toggleIcon.Location = new Point(toggleSwitch.Width - toggleIcon.Width - 4, 5);
                toggleIcon.Size = new Point(22, 22);

                guildhallSwitchText.TextColor = new Color(168, 178, 230);
                guildhallSwitchText.ShadowColor = new Color(40, 47, 85);

                homesteadSwitchText.TextColor = Color.LightGray;
                homesteadSwitchText.ShadowColor = Color.Black;

                homesteadDecorationsFlowPanel.Visible = false;
                guildHallDecorationsFlowPanel.Visible = true;
            };

            searchTextBox.TextChanged += async (sender, args) =>
            {
                string searchText = searchTextBox.Text.ToLower();
                await LeftSideMethods.FilterDecorations(homesteadDecorationsFlowPanel, searchText);
                await LeftSideMethods.FilterDecorations(guildHallDecorationsFlowPanel, searchText);

                clearButton.Visible = !string.IsNullOrEmpty(searchText);
            };
        }
        private void AnimatePanel(Panel panel, bool fadeIn)
        {
            var targetOpacity = fadeIn ? 1.0f : 0.0f;

            if (fadeIn)
            {
                panel.Visible = true;
            }

            var timer = new System.Windows.Forms.Timer { Interval = 1 };
            timer.Tick += (s, e) =>
            {
                panel.Opacity += fadeIn ? 0.05f : -0.05f;
                panel.Opacity = MathHelper.Clamp(panel.Opacity, 0.0f, 1.0f);

                if (Math.Abs(panel.Opacity - targetOpacity) < 0.05)
                {
                    panel.Opacity = targetOpacity;

                    if (!fadeIn)
                    {
                        panel.Visible = false;
                    }

                    timer.Stop();
                }
            };

            timer.Start();
        }
    }
}