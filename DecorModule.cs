﻿using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using DecorBlishhudModule.CustomControls.CustomTab;
using DecorBlishhudModule.Model;
using DecorBlishhudModule.Sections;
using DecorBlishhudModule.Sections.LeftSideTasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
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
        private Texture2D _homesteadIconMenuLunar;
        private Texture2D _homesteadIconMenuSAB;
        private Texture2D _homesteadIconMenuDragonBash;
        private Texture2D _homesteadIconMenuFOTFW;
        private Texture2D _homesteadIconMenuHalloween;
        private Texture2D _homesteadIconMenuWintersday;
        private Texture2D _currentEmblem;
        private Texture2D _homesteadIconHover;
        private Texture2D _homesteadIconUnactive;
        private Texture2D _homesteadScreen;
        private Texture2D _guildhallScreen;
        private Texture2D _handiworkTab;
        private Texture2D _scribeTab;
        private Texture2D _iconsTab;
        private Texture2D _imagesTab;
        private Texture2D _info;
        private Texture2D _blackTexture;
        private Texture2D _x;
        private Texture2D _x2;
        private Texture2D _x2Active;
        private Texture2D _copy;
        private CustomTabbedWindow2 _decorWindow;
        private Image _decorationIcon;
        private Label _decorationRightText;
        private Image _decorationImage;
        private SignatureSection _signatureLabelManager;
        private WikiLicenseSection _wikiLicenseManager;
        private FlowPanel _homesteadDecorationsFlowPanel;
        private bool _loaded = false;

        public CustomTabbedWindow2 DecorWindow => _decorWindow;
        public Label DecorationRightText => _decorationRightText;
        public Image DecorationImage => _decorationImage;
        public Texture2D BlackTexture => _blackTexture;
        public Texture2D X => _x;
        public Texture2D X2 => _x2;
        public Texture2D X2Active => _x2Active;
        public Texture2D Info => _info;
        public new bool Loaded => _loaded;
        public Texture2D CopyIcon => _copy;


        public HttpClient Client => client;

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
            _homesteadIconMenuLunar = ContentsManager.GetTexture("test/homesteadIconMenuLunar.png");
            _homesteadIconMenuSAB = ContentsManager.GetTexture("test/homesteadIconMenuSAB.png");
            _homesteadIconMenuDragonBash = ContentsManager.GetTexture("test/homesteadIconMenuDragonBash.png");
            _homesteadIconMenuFOTFW = ContentsManager.GetTexture("test/homesteadIconMenuFOTFW.png");
            _homesteadIconMenuHalloween = ContentsManager.GetTexture("test/homesteadIconMenuHalloween.png");
            _homesteadIconMenuWintersday = ContentsManager.GetTexture("test/homesteadIconMenuWinterstay.png");
            _homesteadScreen = ContentsManager.GetTexture("test/homestead_screen.png");
            _guildhallScreen = ContentsManager.GetTexture("test/guildhall_screen.png");
            _handiworkTab = ContentsManager.GetTexture("test/handiwork.png");
            _scribeTab = ContentsManager.GetTexture("test/scribe.png");
            _iconsTab = ContentsManager.GetTexture("test/icons.png");
            _imagesTab = ContentsManager.GetTexture("test/images.png");
            _info = ContentsManager.GetTexture("test/info.png");
            _blackTexture = ContentsManager.GetTexture("test/black_texture.png");
            _x = ContentsManager.GetTexture("test/x.png");
            _x2 = ContentsManager.GetTexture("test/x2.png");
            _x2Active = ContentsManager.GetTexture("test/x2_active.png");
            _copy = ContentsManager.GetTexture("test/copy.png");

            // Create corner icon and show loading spinner
            _cornerIcon = new CornerIcon()
            {
                Icon = _homesteadIconUnactive,
                HoverIcon = _homesteadIconHover,
                IconName = "Decor",
                Priority = 1645843523,
                LoadingMessage = "Decor is fetching data...",
                Parent = GameService.Graphics.SpriteScreen
            };

            // Fetch theme-specific icon
            _currentEmblem = await MainIconTheme.GetThemeIconAsync(
                _homesteadIconMenu,
                _homesteadIconMenuLunar,
                _homesteadIconMenuSAB,
                _homesteadIconMenuDragonBash,
                _homesteadIconMenuFOTFW,
                _homesteadIconMenuHalloween,
                _homesteadIconMenuWintersday
                );

            // Window background
            var windowBackgroundTexture = AsyncTexture2D.FromAssetId(155997);
            await CreateGw2StyleWindowThatDisplaysAllDecorations(windowBackgroundTexture);

            // Spawn the corner info icon
            InfoSection.InitializeInfoPanel();

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

            // Set up signature and license labels
            _signatureLabelManager = new SignatureSection(_decorWindow);
            _wikiLicenseManager = new WikiLicenseSection(_decorWindow);
        }

        protected override void Unload()
        {
            _cornerIcon?.Dispose();
            _homesteadIconMenu?.Dispose();
            _homesteadIconMenuLunar?.Dispose();
            _homesteadIconMenuSAB?.Dispose();
            _homesteadIconMenuDragonBash?.Dispose();
            _homesteadIconMenuFOTFW.Dispose();
            _homesteadIconMenuHalloween.Dispose();
            _homesteadIconMenuWintersday?.Dispose();
            _homesteadIconHover?.Dispose();
            _homesteadIconUnactive?.Dispose();
            _homesteadScreen?.Dispose();
            _guildhallScreen?.Dispose();
            _decorWindow?.Dispose();
            _handiworkTab?.Dispose();
            _scribeTab?.Dispose();
            _iconsTab?.Dispose();
            _imagesTab?.Dispose();
            _info?.Dispose();
            _blackTexture?.Dispose();
            _x?.Dispose();
            _x2?.Dispose();
            _x2Active?.Dispose();
            _copy?.Dispose();
            _decorationIcon?.Dispose();
            _decorationImage?.Dispose();
            DecorModuleInstance = null;
        }

        // Style Window
        private async Task CreateGw2StyleWindowThatDisplaysAllDecorations(AsyncTexture2D windowBackgroundTexture)
        {
            _decorWindow = new CustomTabbedWindow2(
                windowBackgroundTexture,
                new Rectangle(20, 26, 560, 640),
                new Rectangle(70, 40, 550, 640),
                new Point(1150, 800))
            {
                Parent = GameService.Graphics.SpriteScreen,
                Title = "Decor",
                Emblem = _currentEmblem,
                Subtitle = "Homestead Decorations",
                Location = new Point(300, 300),
                SavesPosition = true,
                Id = $"{nameof(DecorModule)}_Decoration_Window",
            };

            // Background image setup
            var backgroundImage = new Image
            {
                Parent = _decorWindow,
                Size = new Point(_decorWindow.Size.X - 55, _decorWindow.Size.Y - 75),
                Location = new Point(0, 0),
                Opacity = 0.3f,
                BackgroundColor = new Color(0, 0, 0, 10),
                Texture = _homesteadScreen,
                Visible = true
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

            _homesteadDecorationsFlowPanel = new FlowPanel
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

            var homesteadDecorationsBigFlowPanel = new FlowPanel
            {
                Parent = _decorWindow,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                ShowBorder = true,
                Width = 1050,
                Height = 640,
                CanScroll = true,
                Location = new Point(10, searchTextBox.Bottom + 10),
                Visible = false
            };

            var guildHallDecorationsBigFlowPanel = new FlowPanel
            {
                Parent = _decorWindow,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                ShowBorder = true,
                Width = 1050,
                Height = 640,
                CanScroll = true,
                Location = new Point(10, searchTextBox.Bottom + 10),
                Visible = false
            };

            _decorationRightText = new Label
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

            _decorationIcon = new Image
            {
                Parent = _decorWindow,
                Size = new Point(40, 40),
                Location = new Point(_decorationRightText.Left, _decorationRightText.Bottom + 5)
            };

            _decorationImage = new Image
            {
                Parent = _decorWindow,
                Size = new Point(400, 400),
                Location = new Point(_decorationRightText.Left, _decorationIcon.Bottom + 5)
            };

            var customTab1 = new CustomTab(_handiworkTab, "Homestead Handiwork", 4);
            var customTab2 = new CustomTab(_scribeTab, "Guild Hall Scribe", 3);
            var customTab3 = new CustomTab(_iconsTab, "Icons Preview", 2);
            var customTab4 = new CustomTab(_imagesTab, "Images Preview", 1);

            _decorWindow.TabsGroup1.Add(customTab1);
            _decorWindow.TabsGroup1.Add(customTab2);
            _decorWindow.TabsGroup2.Add(customTab3);
            _decorWindow.TabsGroup2.Add(customTab4);

            _decorWindow.SelectedTabGroup1 = _decorWindow.TabsGroup1.FirstOrDefault();
            _decorWindow.SelectedTabGroup2 = _decorWindow.TabsGroup2.FirstOrDefault();

            CustomTab activeTabGroup1 = _decorWindow.SelectedTabGroup1;
            CustomTab activeTabGroup2 = _decorWindow.SelectedTabGroup2;

            _decorWindow.TabChanged += (s, e) =>
            {
                CustomTab activeTabGroup1 = _decorWindow.SelectedTabGroup1;
                CustomTab activeTabGroup2 = _decorWindow.SelectedTabGroup2;

                if (activeTabGroup1 == customTab1 && activeTabGroup2 == customTab3)
                {
                    _decorWindow.Subtitle = "Homestead Decorations";
                    backgroundImage.Texture = _homesteadScreen;
                    backgroundImage.Visible = true;
                    _decorationRightText.Visible = true;
                    _decorationImage.Visible = true;
                    _homesteadDecorationsFlowPanel.Visible = true;
                    guildHallDecorationsFlowPanel.Visible = false;
                    homesteadDecorationsBigFlowPanel.Visible = false;
                    guildHallDecorationsBigFlowPanel.Visible = false;
                    _wikiLicenseManager.UpdateWidthBasedOnFlowPanel(false);
                    InfoSection.UpdateInfoText("    Click on the name or the image\n            to copy its name.");
                }
                else if (activeTabGroup1 == customTab2 && activeTabGroup2 == customTab3)
                {
                    _decorWindow.Subtitle = "Guild Hall Decorations";
                    backgroundImage.Texture = _guildhallScreen;
                    backgroundImage.Visible = true;
                    _decorationRightText.Visible = true;
                    _decorationImage.Visible = true;
                    _homesteadDecorationsFlowPanel.Visible = false;
                    guildHallDecorationsFlowPanel.Visible = true;
                    homesteadDecorationsBigFlowPanel.Visible = false;
                    guildHallDecorationsBigFlowPanel.Visible = false;
                    _wikiLicenseManager.UpdateWidthBasedOnFlowPanel(false);
                    InfoSection.UpdateInfoText("    Click on the name or the image\n            to copy its name.");
                }
                else if (activeTabGroup1 == customTab1 && activeTabGroup2 == customTab4)
                {
                    _decorWindow.Subtitle = "Homestead Decorations";
                    backgroundImage.Texture = _homesteadScreen;
                    backgroundImage.Visible = true;
                    _decorationRightText.Visible = false;
                    _decorationImage.Visible = false;
                    _homesteadDecorationsFlowPanel.Visible = false;
                    guildHallDecorationsFlowPanel.Visible = false;
                    homesteadDecorationsBigFlowPanel.Visible = true;
                    guildHallDecorationsBigFlowPanel.Visible = false;
                    _wikiLicenseManager.UpdateWidthBasedOnFlowPanel(true);
                    InfoSection.UpdateInfoText("    Click on the image to zoom in.\nCopy icon copies the decoration name.");
                }
                else if (activeTabGroup1 == customTab2 && activeTabGroup2 == customTab4)
                {
                    _decorWindow.Subtitle = "Guild Hall Decorations";
                    backgroundImage.Texture = _guildhallScreen;
                    backgroundImage.Visible = true;
                    _decorationRightText.Visible = false;
                    _decorationImage.Visible = false;
                    _homesteadDecorationsFlowPanel.Visible = false;
                    guildHallDecorationsFlowPanel.Visible = false;
                    homesteadDecorationsBigFlowPanel.Visible = false;
                    guildHallDecorationsBigFlowPanel.Visible = true;
                    _wikiLicenseManager.UpdateWidthBasedOnFlowPanel(true);
                    InfoSection.UpdateInfoText("    Click on the image to zoom in.\nCopy icon copies the decoration name.");
                }
            };

            // Disable the tabs initially
            customTab2.Enabled = false;
            customTab4.Enabled = false;

            await LeftSideSection.PopulateHomesteadIconsInFlowPanel(_homesteadDecorationsFlowPanel, true);

            // Hide loading spinner when decorations are ready
            _cornerIcon.LoadingMessage = null;

            // Start background tasks
            var guildHallTask = Task.Run(async () =>
            {
                await LeftSideSection.PopulateGuildHallIconsInFlowPanel(guildHallDecorationsFlowPanel, true);
                customTab2.Enabled = true;
                if (customTab2.Enabled && customTab4.Enabled) { _loaded = true; }
            });

            var imagePreviewTask = Task.Run(async () =>
            {
                await LeftSideSection.PopulateHomesteadBigIconsInFlowPanel(homesteadDecorationsBigFlowPanel, false);
                await LeftSideSection.PopulateGuildHallBigIconsInFlowPanel(guildHallDecorationsBigFlowPanel, false);
                customTab4.Enabled = true;
                if (customTab2.Enabled && customTab4.Enabled) { _loaded = true; }
            });

            guildHallTask.ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    Logger.Warn(t.Exception, "Guild Hall task failed.");
                }
            });

            imagePreviewTask.ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    Logger.Warn(t.Exception, "Image Preview task failed.");
                }
            });

            // Search functionality
            searchTextBox.TextChanged += async (sender, args) =>
            {
                string searchText = searchTextBox.Text.ToLower();
                await FilterDecorations.FilterDecorationsAsync(_homesteadDecorationsFlowPanel, searchText, true);
                await FilterDecorations.FilterDecorationsAsync(guildHallDecorationsFlowPanel, searchText, true);
                await FilterDecorations.FilterDecorationsAsync(homesteadDecorationsBigFlowPanel, searchText, false);
                await FilterDecorations.FilterDecorationsAsync(guildHallDecorationsBigFlowPanel, searchText, false);

                clearButton.Visible = !string.IsNullOrEmpty(searchText);
            };

            clearButton.Click += (s, e) =>
            {
                searchTextBox.Text = string.Empty;
            };
        }
    }
}