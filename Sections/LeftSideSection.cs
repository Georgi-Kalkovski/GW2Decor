using Blish_HUD;
using Blish_HUD.Controls;
using DecorBlishhudModule.Homestead;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Point = Microsoft.Xna.Framework.Point;
using DecorBlishhudModule.Model;
using DecorBlishhudModule.CustomControls;
using DecorBlishhudModule.Sections;
using DecorBlishhudModule.Sections.LeftSideTasks;
using DecorBlishhudModule.CustomControls.CustomTab;

namespace DecorBlishhudModule
{
    public class LeftSideSection
    {
        private static readonly Logger Logger = Logger.GetLogger<DecorModule>();

        private static readonly Dictionary<string, Texture2D> _sharedTextureCache = new();
        private static Dictionary<string, List<Decoration>> _homesteadDecorationsCache;
        private static Dictionary<string, List<Decoration>> _guildHallDecorationsCache;
        private static Panel lastClickedIconPanel = null;
        private static bool isOperationRunning = false;

        private static LoadingSpinner _loaderSpinner = null;
        private static Label _loadingLabel = null;
        private static Label _loadingLabel2 = null;

        private static Task<Dictionary<string, List<Decoration>>> FetchHomesteadDecorationsAsync()
        {
            return _homesteadDecorationsCache != null
                ? Task.FromResult(_homesteadDecorationsCache)
                : RefreshHomesteadDecorationsAsync();
        }

        private static Task<Dictionary<string, List<Decoration>>> FetchGuildHallDecorationsAsync()
        {
            return _guildHallDecorationsCache != null
                ? Task.FromResult(_guildHallDecorationsCache)
                : RefreshGuildHallDecorationsAsync();
        }

        private static async Task<Dictionary<string, List<Decoration>>> RefreshHomesteadDecorationsAsync()
        {
            _homesteadDecorationsCache = await HomesteadDecorationFetcher.FetchDecorationsAsync();
            return _homesteadDecorationsCache;
        }

        private static async Task<Dictionary<string, List<Decoration>>> RefreshGuildHallDecorationsAsync()
        {
            _guildHallDecorationsCache = await GuildHallDecorationFetcher.FetchDecorationsAsync();
            return _guildHallDecorationsCache;
        }

        public static async Task PopulateHomesteadIconsInFlowPanel(FlowPanel homesteadDecorationsFlowPanel, bool _isIconView)
        {
            var decorationsByCategory = await FetchHomesteadDecorationsAsync();

            var caseInsensitiveCategories = new Dictionary<string, List<Decoration>>(StringComparer.OrdinalIgnoreCase);
            foreach (var kvp in decorationsByCategory)
            {
                caseInsensitiveCategories[kvp.Key] = kvp.Value;
            }

            List<string> predefinedCategories = HomesteadCategories.GetCategories();

            var categoryTasks = predefinedCategories
                .Where(category => caseInsensitiveCategories.ContainsKey(category))
                .Select(async category =>
                {
                    int baseHeight = 45;
                    int heightIncrementPerDecorationSet = 52;
                    var decorations = caseInsensitiveCategories[category];
                    int numDecorationSets = (int)Math.Ceiling(decorations.Count / 9.0);
                    int calculatedHeight = baseHeight + (numDecorationSets * heightIncrementPerDecorationSet);

                    var categoryFlowPanel = new FlowPanel
                    {
                        Parent = homesteadDecorationsFlowPanel,
                        Title = category,
                        FlowDirection = ControlFlowDirection.LeftToRight,
                        Width = homesteadDecorationsFlowPanel.Width - 20,
                        Height = calculatedHeight,
                        CanCollapse = false,
                        ControlPadding = new Vector2(4, 4),
                        OuterControlPadding = new Vector2(6, 4),
                    };

                    var tasks = decorations.Select(decoration => CreateDecorationIconsImagesAsync(decoration, categoryFlowPanel, _isIconView));
                    await Task.WhenAll(tasks);
                });

            await Task.WhenAll(categoryTasks);

            await OrderDecorations.OrderDecorationsAsync(homesteadDecorationsFlowPanel, _isIconView);
        }

        public static async Task PopulateGuildHallIconsInFlowPanel(FlowPanel decorationsFlowPanel, bool _isIconView)
        {
            var decorationsByCategory = await FetchGuildHallDecorationsAsync();

            var flowPanelTasks = decorationsByCategory
                .Where(entry => entry.Value != null && entry.Value.Count > 0)
                .Select(async entry =>
                {
                    string category = entry.Key;
                    var categoryDecorations = entry.Value;

                    int baseHeight = 45;
                    int heightIncrementPerDecorationSet = 52;
                    int numDecorationSets = (int)Math.Ceiling(categoryDecorations.Count / 9.0);
                    int calculatedHeight = baseHeight + (numDecorationSets * heightIncrementPerDecorationSet);

                    var categoryFlowPanel = new FlowPanel
                    {
                        Parent = decorationsFlowPanel,
                        Title = category,
                        FlowDirection = ControlFlowDirection.LeftToRight,
                        Width = decorationsFlowPanel.Width - 20,
                        Height = calculatedHeight,
                        CanCollapse = false,
                        ControlPadding = new Vector2(4, 4),
                        OuterControlPadding = new Vector2(6, 4),
                    };

                    var iconTasks = categoryDecorations.Select(decoration => CreateDecorationIconsImagesAsync(decoration, categoryFlowPanel, _isIconView));
                    await Task.WhenAll(iconTasks);
                }).ToList();

            await Task.WhenAll(flowPanelTasks);

            await OrderDecorations.OrderDecorationsAsync(decorationsFlowPanel, _isIconView);
        }

        public static async Task PopulateHomesteadBigIconsInFlowPanel(FlowPanel homesteadDecorationsFlowPanel, bool _isIconView)
        {
            var decorationsByCategory = await FetchHomesteadDecorationsAsync();

            var caseInsensitiveCategories = new Dictionary<string, List<Decoration>>(StringComparer.OrdinalIgnoreCase);
            foreach (var kvp in decorationsByCategory)
            {
                caseInsensitiveCategories[kvp.Key] = kvp.Value;
            }

            List<string> predefinedCategories = HomesteadCategories.GetCategories();

            var categoryTasks = predefinedCategories
                .Where(category => caseInsensitiveCategories.ContainsKey(category))
                .Select(async category =>
                {
                    int baseHeight = 45;
                    int heightIncrementPerDecorationSet = 312;
                    var decorations = caseInsensitiveCategories[category];
                    int numDecorationSets = (int)Math.Ceiling(decorations.Count / 4.0);
                    int calculatedHeight = baseHeight + (numDecorationSets * heightIncrementPerDecorationSet);

                    var categoryFlowPanel = new FlowPanel
                    {
                        Parent = homesteadDecorationsFlowPanel,
                        Title = category,
                        FlowDirection = ControlFlowDirection.LeftToRight,
                        Width = homesteadDecorationsFlowPanel.Width - 20,
                        Height = calculatedHeight,
                        CanCollapse = true,
                        ControlPadding = new Vector2(8, 10),
                        OuterControlPadding = new Vector2(10, 10),
                    };

                    var tasks = decorations.Select(decoration => CreateDecorationIconsImagesAsync(decoration, categoryFlowPanel, _isIconView));
                    await Task.WhenAll(tasks);
                });

            await Task.WhenAll(categoryTasks);

            await OrderDecorations.OrderDecorationsAsync(homesteadDecorationsFlowPanel, _isIconView);
        }

        public static async Task PopulateGuildHallBigIconsInFlowPanel(FlowPanel decorationsFlowPanel, bool _isIconView)
        {
            var decorationsByCategory = await FetchGuildHallDecorationsAsync();

            var flowPanelTasks = decorationsByCategory
                .Where(entry => entry.Value != null && entry.Value.Count > 0)
                .Select(async entry =>
                {
                    string category = entry.Key;
                    var categoryDecorations = entry.Value;

                    int baseHeight = 45;
                    int heightIncrementPerDecorationSet = 312;
                    int numDecorationSets = (int)Math.Ceiling(categoryDecorations.Count / 4.0);
                    int calculatedHeight = baseHeight + (numDecorationSets * heightIncrementPerDecorationSet);

                    var categoryFlowPanel = new FlowPanel
                    {
                        Parent = decorationsFlowPanel,
                        Title = category,
                        FlowDirection = ControlFlowDirection.LeftToRight,
                        Width = decorationsFlowPanel.Width - 20,
                        Height = calculatedHeight,
                        CanCollapse = true,
                        ControlPadding = new Vector2(8, 10),
                        OuterControlPadding = new Vector2(10, 10),
                    };

                    var iconTasks = categoryDecorations.Select(decoration => CreateDecorationIconsImagesAsync(decoration, categoryFlowPanel, _isIconView));
                    await Task.WhenAll(iconTasks);
                }).ToList();

            await Task.WhenAll(flowPanelTasks);

            await OrderDecorations.OrderDecorationsAsync(decorationsFlowPanel, _isIconView);
        }

        public static async Task CreateDecorationIconsImagesAsync(Decoration decoration, FlowPanel categoryFlowPanel, bool _isIconView)
        {
            try
            {
                // Fetch or create the icon texture
                var iconTexture = await GetOrCreateTextureAsync(decoration.Name, decoration.IconUrl);
                if (iconTexture == null)
                {
                    Logger.Warn($"Icon texture for '{decoration.Name}' could not be loaded.");
                    return;
                }
                var tooltip = await DecorationCustomTooltip.CreateTooltipWithIconsAsync(decoration, iconTexture);
                if (_isIconView == true)
                {
                    if (iconTexture != null)
                    {
                        var borderPanel = new Panel
                        {
                            Parent = categoryFlowPanel,
                            Size = new Point(49, 49),
                            BackgroundColor = Color.Black,
                        };

                        var decorationIconImage = new Image(iconTexture)
                        {
                            Parent = borderPanel,
                            Size = new Point(45),
                            Location = new Point(2, 2),
                            BasicTooltipText = decoration.Name,
                            Tooltip = tooltip
                        };

                        borderPanel.MouseEntered += (sender, e) =>
                        {
                            if (lastClickedIconPanel != borderPanel)
                            {
                                borderPanel.BackgroundColor = Color.LightGray;
                                decorationIconImage.Opacity = 0.75f;
                            }
                        };

                        borderPanel.MouseLeft += (sender, e) =>
                        {
                            if (lastClickedIconPanel != borderPanel)
                            {
                                borderPanel.BackgroundColor = Color.Black;
                                decorationIconImage.Opacity = 1f;
                            }
                        };
                        var decorWindow = DecorModule.DecorModuleInstance.DecorWindow;

                        CustomTab lastSelectedTab = null;

                        decorWindow.TabChanged += (s, e) =>
                        {
                            if (decorWindow.SelectedTabGroup3 != null)
                            {
                                if (_loaderSpinner != null) _loaderSpinner.Visible = false;
                                if (_loadingLabel != null) _loadingLabel.Visible = false;
                                if (_loadingLabel2 != null) _loadingLabel2.Visible = false;
                            }
                            if (decorWindow.SelectedTabGroup2 != null && decorWindow.SelectedTabGroup2 != lastSelectedTab)
                            {
                                if (_loaderSpinner != null && _loadingLabel != null && _loadingLabel2 != null)
                                {
                                    if (decorWindow.SelectedTabGroup2.Name == "Icons Preview")
                                    {
                                        _loaderSpinner.Visible = true;
                                        _loadingLabel.Visible = true;
                                        _loadingLabel2.Visible = true;
                                    }
                                    else
                                    {
                                        _loaderSpinner.Visible = false;
                                        _loadingLabel.Visible = false;
                                        _loadingLabel2.Visible = false;
                                    }
                                }
                            }
                            lastSelectedTab = decorWindow.SelectedTabGroup2;
                        };

                        decorationIconImage.Click += async (s, e) =>
                        {
                            if (isOperationRunning)
                            {
                                return;
                            }

                            var loaded = DecorModule.DecorModuleInstance.Loaded;

                            if (lastClickedIconPanel != null && lastClickedIconPanel.BackgroundColor == new Color(254, 254, 176))
                            {
                                lastClickedIconPanel.BackgroundColor = Color.Black;
                                decorationIconImage.Opacity = 1f;
                            }

                            borderPanel.BackgroundColor = new Color(254, 254, 176);
                            decorationIconImage.Opacity = 1f;

                            lastClickedIconPanel = borderPanel;

                            _loaderSpinner = new LoadingSpinner
                            {
                                Parent = decorWindow,
                                Size = new Point(32, 32),
                                Location = new Point(727, 320),
                            };

                            _loadingLabel = new Label
                            {
                                Parent = decorWindow,
                                Text = "Loading...",
                                Font = GameService.Content.DefaultFont16,
                                Location = new Point(762, 325),
                                HorizontalAlignment = HorizontalAlignment.Center,
                                AutoSizeWidth = true,
                            };

                            _loadingLabel2 = new Label();
                            if (!loaded)
                            {
                                _loadingLabel2 = new Label
                                {
                                    Parent = decorWindow,
                                    Text = "The image may take longer as the full data is fetched.",
                                    Font = GameService.Content.DefaultFont16,
                                    Location = new Point(620, 350),
                                    HorizontalAlignment = HorizontalAlignment.Center,
                                    AutoSizeWidth = true,
                                };
                            }

                            isOperationRunning = true;

                            try
                            {
                                var decorModule = DecorModule.DecorModuleInstance;
                                decorModule.DecorationImage.Tooltip = tooltip;
                                await Task.Run(async () =>
                                {
                                    try
                                    {
                                        await RightSideSection.UpdateDecorationImageAsync(decoration, decorModule.DecorWindow, decorModule.DecorationImage);
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine("Error occurred during decoration image update: " + ex.Message);
                                    }
                                });
                            }
                            finally
                            {
                                isOperationRunning = false;
                                _loaderSpinner.Dispose();
                                _loadingLabel.Dispose();
                                _loadingLabel2.Dispose();
                            }
                        };
                    };
                }
                else
                {
                    // Fetch or create the larger image texture
                    var imageTexture = await GetOrCreateTextureAsync(decoration.Name + "_Image", decoration.ImageUrl);

                    if (imageTexture == null) return;

                    // Main container for the decoration
                    var mainContainer = new BorderPanel
                    {
                        Parent = categoryFlowPanel,
                        Size = new Point(254, 300),
                        BackgroundColor = new Color(0, 0, 0, 36),
                        Tooltip = tooltip
                    };

                    // Icon and text container (horizontal layout)
                    var iconTextContainer = new Panel
                    {
                        Parent = mainContainer,
                        Location = new Point(0, 0),
                        Size = new Point(256, 50),
                        Tooltip = tooltip,
                        BackgroundTexture = DecorModule.DecorModuleInstance.BlackTexture,
                    };

                    // Icon
                    var iconImage = new Image(iconTexture)
                    {
                        Parent = iconTextContainer,
                        Location = new Point(3, 3),
                        Size = new Point(44, 44),
                        BasicTooltipText = decoration.Name,
                        Tooltip = tooltip
                    };

                    // Decoration Name Label
                    var nameLabel = new Label
                    {
                        Parent = iconTextContainer,
                        Size = new Point(190, 40),
                        Location = new Point(iconImage.Location.X + iconImage.Size.X + 8, 5),
                        Text = decoration.Name,
                        Font = decoration.Name.ToString().Length > 30 ? GameService.Content.DefaultFont12 : GameService.Content.DefaultFont14,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Middle,
                        Tooltip = tooltip
                    };

                    // Image Panel
                    int imageWidth = imageTexture.Width;
                    int imageHeight = imageTexture.Height;
                    float aspectRatio = (float)imageWidth / imageHeight;

                    int width = 245;
                    int height = (int)(245 / aspectRatio);

                    if (height > 245)
                    {
                        height = 245;
                        width = (int)(245 * aspectRatio);
                    }

                    int xOffset = (mainContainer.Size.X - width) / 2;
                    int yOffset = iconTextContainer.Location.Y + iconTextContainer.Size.Y;

                    int remainingHeight = mainContainer.Size.Y - iconTextContainer.Size.Y;
                    int centeredYOffset = (remainingHeight - height) / 2 + yOffset;

                    var decorationImage = new Image(imageTexture)
                    {
                        Parent = mainContainer,
                        Location = new Point(xOffset + 2, centeredYOffset - 1),
                        Size = new Point(width - 3, height),
                        Tooltip = tooltip
                    };

                    mainContainer.MouseEntered += (sender, e) =>
                    {
                        if (lastClickedIconPanel != mainContainer)
                        {
                            mainContainer.BackgroundColor = new Color(101, 101, 84, 36);
                        }
                    };

                    mainContainer.MouseLeft += (sender, e) =>
                    {
                        if (lastClickedIconPanel != mainContainer)
                        {
                            mainContainer.BackgroundColor = new Color(0, 0, 0, 36);
                        }
                    };

                    var decorWindow = DecorModule.DecorModuleInstance.DecorWindow;
                    var borderedTexture = BorderCreator.CreateBorderedTexture(imageTexture);

                    decorationImage.Click += async (s, e) =>
                    {

                        decorWindow = DecorModule.DecorModuleInstance.DecorWindow;

                        decorationImage = new Image(borderedTexture) { Parent = decorWindow };

                        await BigImageSection.UpdateDecorationImageAsync(decoration, decorWindow, decorationImage);
                    };

                    // Copy icon panel
                    var copyPanelContainer = new Panel
                    {
                        Parent = mainContainer,
                        Size = new Point(24, 24),
                        Location = new Point(mainContainer.Size.X - 24, 0),
                    };

                    // Copy icon
                    var copyIcon = new Image(DecorModule.DecorModuleInstance?.CopyIcon)
                    {
                        Parent = copyPanelContainer,
                        Size = copyPanelContainer.Size,
                        BasicTooltipText = "Copy Name"
                    };

                    var savePanel = new Panel
                    {
                        Parent = mainContainer,
                        Location = new Point(90, 150),
                        Title = "Copied !",
                        Width = 80,
                        Height = 45,
                        ShowBorder = true,
                        Opacity = 0f,
                        Visible = false,
                    };

                    var copyBrightnessOverlay = new Panel
                    {
                        Parent = copyPanelContainer,
                        Size = copyPanelContainer.Size,
                        Location = Point.Zero,
                        BackgroundColor = Color.White * 0.3f,
                        Visible = false,
                    };

                    copyIcon.MouseEntered += (s, e) =>
                    {
                        copyBrightnessOverlay.Visible = true;
                    };

                    copyIcon.MouseLeft += (s, e) =>
                    {
                        copyBrightnessOverlay.Visible = false;
                    };

                    copyBrightnessOverlay.Click += (sender, e) =>
                    {
                        if (savePanel.Visible == false)
                        {
                            SaveTasks.CopyTextToClipboard(decoration.Name);
                            SaveTasks.ShowSavedPanel(savePanel);
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                Logger.Warn($"Failed to load decoration icon for '{decoration.Name}'. Error: {ex.Message}");
            }
        }

        public static async Task<Texture2D> GetOrCreateTextureAsync(string key, string iconUrl)
        {
            if (_sharedTextureCache.TryGetValue(key, out var existingTexture))
            {
                return existingTexture;
            }

            try
            {
                var iconResponse = await DecorModule.DecorModuleInstance.Client.GetByteArrayAsync(iconUrl);
                var newTexture = CreateIconTexture(iconResponse);

                if (newTexture != null)
                {
                    _sharedTextureCache[key] = newTexture;
                }

                return newTexture;
            }
            catch (Exception ex)
            {
                Logger.Warn($"Failed to load texture for '{key}'. Error: {ex.Message}");
                return null;
            }
        }

        public static Texture2D CreateIconTexture(byte[] iconResponse)
        {
            try
            {
                using (var memoryStream = new MemoryStream(iconResponse))
                using (var originalImage = System.Drawing.Image.FromStream(memoryStream))
                {
                    int maxDimension = 200;

                    int newWidth = originalImage.Width;
                    int newHeight = originalImage.Height;

                    if (originalImage.Width > maxDimension || originalImage.Height > maxDimension)
                    {
                        float scale = Math.Min((float)maxDimension / originalImage.Width, (float)maxDimension / originalImage.Height);
                        newWidth = (int)(originalImage.Width * scale);
                        newHeight = (int)(originalImage.Height * scale);
                    }

                    using (var resizedImage = new System.Drawing.Bitmap(originalImage, newWidth, newHeight))
                    using (var resizedStream = new MemoryStream())
                    using (var graphicsContext = GameService.Graphics.LendGraphicsDeviceContext())
                    {
                        resizedImage.Save(resizedStream, System.Drawing.Imaging.ImageFormat.Png);
                        resizedStream.Seek(0, SeekOrigin.Begin);

                        return Texture2D.FromStream(graphicsContext.GraphicsDevice, resizedStream);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Warn($"Failed to create icon texture. Error: {ex.Message}");
                return null;
            }
        }
        private static void CleanupSharedTextureCache()
        {
            foreach (var texture in _sharedTextureCache.Values)
            {
                texture.Dispose();
            }

            _sharedTextureCache.Clear();
            _homesteadDecorationsCache.Clear();
            _guildHallDecorationsCache.Clear();
            Logger.Info("Shared texture cache cleaned up.");
        }
        protected void Unload()
        {
            CleanupSharedTextureCache();
            this.Unload();
        }
    }
}