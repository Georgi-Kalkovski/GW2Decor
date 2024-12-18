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
                        OuterControlPadding = new Vector2(0, 4),
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
                        OuterControlPadding = new Vector2(0, 4),
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
                        ControlPadding = new Vector2(4, 10),
                        OuterControlPadding = new Vector2(0, 10),
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
                        ControlPadding = new Vector2(4, 10),
                        OuterControlPadding = new Vector2(0, 10),
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

                        decorationIconImage.Click += async (s, e) =>
                        {
                            if (isOperationRunning)
                            {
                                return;
                            }

                            var decorWindow = DecorModule.DecorModuleInstance.DecorWindow;
                            var loaded = DecorModule.DecorModuleInstance.Loaded;

                            if (lastClickedIconPanel != null && lastClickedIconPanel.BackgroundColor == new Color(254, 254, 176))
                            {
                                lastClickedIconPanel.BackgroundColor = Color.Black;
                                decorationIconImage.Opacity = 1f;
                            }

                            borderPanel.BackgroundColor = new Color(254, 254, 176);
                            decorationIconImage.Opacity = 1f;

                            lastClickedIconPanel = borderPanel;

                            var loaderSpinner = new LoadingSpinner
                            {
                                Parent = decorWindow,
                                Size = new Point(32, 32),
                                Location = new Point(727, 320),
                            };

                            var loadingLabel = new Label
                            {
                                Parent = decorWindow,
                                Text = "Loading...",
                                Font = GameService.Content.DefaultFont16,
                                Location = new Point(762, 325),
                                HorizontalAlignment = HorizontalAlignment.Center,
                                AutoSizeWidth = true,
                            };

                            var loadingLabel2 = new Label();
                            if (!loaded)
                            {
                                loadingLabel2 = new Label
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

                                loaderSpinner.Dispose();
                                loadingLabel.Dispose();
                                loadingLabel2.Dispose();
                            }
                        };
                    }
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
                        BasicTooltipText = decoration.Name,

                    };

                    // Icon and text container (horizontal layout)
                    var iconTextContainer = new Panel
                    {
                        Parent = mainContainer,
                        Location = new Point(0, 0),
                        Size = new Point(254, 50),
                        BackgroundColor = Color.Black,
                        BasicTooltipText = decoration.Name,
                    };

                    // Icon
                    var iconImage = new Image(iconTexture)
                    {
                        Parent = iconTextContainer,
                        Location = new Point(3, 3),
                        Size = new Point(44, 44),
                        BasicTooltipText = decoration.Name,
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
                        BasicTooltipText = decoration.Name,
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
                        Location = new Point(xOffset, centeredYOffset),
                        Size = new Point(width - 3, height),
                        BasicTooltipText = decoration.Name,
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

                    // Panel behind big image
                    var bigImagePanel = new Panel
                    {
                        Parent = decorWindow,
                        Size = new Point(1045, 632),
                        Location = new Point(10, 40),
                        BackgroundColor = Color.Black,
                        Opacity = 0.5f,
                        Visible = false,
                        ZIndex = 100,
                    };

                    // Big image
                    var bigImage = new Image(borderedTexture)
                    {
                        Parent = decorWindow,
                        Visible = false,
                        ZIndex = 101,
                    };

                    bool bigImageIsVisible = false;

                    float aspectRatioBig = (float)imageTexture.Width / imageTexture.Height;

                    int maxWidth = decorWindow.Size.X - 400;
                    int maxHeight = decorWindow.Size.Y - 300;

                    int targetWidth, targetHeight;

                    if (aspectRatioBig > 1)
                    {
                        targetWidth = maxWidth;
                        targetHeight = (int)(maxWidth / aspectRatioBig);

                        if (targetHeight > maxHeight)
                        {
                            targetHeight = maxHeight;
                            targetWidth = (int)(maxHeight * aspectRatioBig);
                        }
                    }
                    else
                    {
                        targetHeight = maxHeight;
                        targetWidth = (int)(maxHeight * aspectRatioBig);

                        if (targetWidth > maxWidth)
                        {
                            targetWidth = maxWidth;
                            targetHeight = (int)(maxWidth / aspectRatioBig);
                        }
                    }
                    bigImage.Size = new Point(targetWidth, targetHeight);

                    bigImage.Location = new Point(
                        (decorWindow.Size.X - bigImage.Size.X - 90) / 2,
                        (decorWindow.Size.Y - bigImage.Size.Y - 80) / 2
                    );

                    // X image on the top right corner of the big image
                    var textureX = DecorModule.DecorModuleInstance.X;

                    int textureWidth = 30;
                    int textureHeight = 30;

                    float aspectRatioX = (float)textureX.Width / textureX.Height;

                    if (aspectRatioX > 1)
                    {
                        textureWidth = Math.Min(textureWidth, bigImage.Size.X / 5);
                        textureHeight = (int)(textureWidth / aspectRatioX);
                    }
                    else
                    {
                        textureHeight = Math.Min(textureHeight, bigImage.Size.Y / 5);
                        textureWidth = (int)(textureHeight * aspectRatioX);
                    }

                    var textureXImage = new Image(textureX)
                    {
                        Parent = decorWindow,
                        Size = new Point(textureWidth, textureHeight),
                        Location = new Point(
                            bigImage.Location.X + bigImage.Size.X - textureWidth - 10,
                            bigImage.Location.Y + 10
                        ),
                        ZIndex = 102,
                        Visible = false
                    };

                    decorationImage.Click += (s, e) =>
                    {
                        if (!bigImageIsVisible)
                        {
                            bigImage.Visible = true;
                            bigImagePanel.Visible = true;
                            bigImageIsVisible = true;
                            textureXImage.Visible = true;
                        }
                        else
                        {
                            bigImage.Visible = false;
                            bigImagePanel.Visible = false;
                            bigImageIsVisible = false;
                            textureXImage.Visible = false;
                        }
                    };

                    bigImage.Click += (s, e) =>
                    {
                        if (bigImageIsVisible)
                        {
                            bigImage.Visible = false;
                            bigImagePanel.Visible = false;
                            bigImageIsVisible = false;
                            textureXImage.Visible = false;
                        }
                    };

                    decorWindow.Click += (s, e) =>
                    {
                        if (bigImage.Visible)
                        {
                            bigImage.Visible = false;
                            bigImagePanel.Visible = false;
                            bigImageIsVisible = false;
                            textureXImage.Visible = false;
                        }
                    };

                    // Copy icon panel
                    var copyPanelContainer = new Panel
                    {
                        Parent = mainContainer,
                        Size = new Point(24, 24),
                        Location = new Point(mainContainer.Size.X - 24, -2),
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

        private static async Task<Texture2D> GetOrCreateTextureAsync(string key, string iconUrl)
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

        private static Texture2D CreateIconTexture(byte[] iconResponse)
        {
            try
            {
                using (var memoryStream = new MemoryStream(iconResponse))
                using (var originalImage = System.Drawing.Image.FromStream(memoryStream))
                {
                    int maxDimension = 600;

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
            Logger.Info("Shared texture cache cleaned up.");
        }
        protected void Unload()
        {
            CleanupSharedTextureCache();
            this.Unload();
        }
    }
}