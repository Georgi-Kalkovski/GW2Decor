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

namespace DecorBlishhudModule
{
    public class LeftSideSection
    {
        private static readonly Logger Logger = Logger.GetLogger<DecorModule>();

        private static Panel lastClickedIconPanel = null;

        //Homestead Logic
        public static async Task PopulateHomesteadIconsInFlowPanel(FlowPanel homesteadDecorationsFlowPanel, bool _isIconView)
        {

            var decorationsByCategory = await HomesteadDecorationFetcher.FetchDecorationsAsync();

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
                        CanCollapse = false, // Maybe true in the future when Scrollbar jump to the top is fixed.
                        ControlPadding = new Vector2(4, 4),
                        OuterControlPadding = new Vector2(0, 4),
                    };

                    var tasks = decorations.Select(decoration => CreateDecorationIconAsync(decoration, categoryFlowPanel, _isIconView));
                    await Task.WhenAll(tasks);
                });

            await Task.WhenAll(categoryTasks);

            await OrderDecorations(homesteadDecorationsFlowPanel, _isIconView);
        }

        //Guild Hall Logic
        public static async Task PopulateGuildHallIconsInFlowPanel(FlowPanel decorationsFlowPanel, bool _isIconView)
        {
            var decorationsByCategory = await GuildHallDecorationFetcher.FetchDecorationsAsync();

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
                        CanCollapse = false, // Maybe true in the future when Scrollbar jump to the top is fixed.
                        ControlPadding = new Vector2(4, 4),
                        OuterControlPadding = new Vector2(0, 4),
                    };

                    var iconTasks = categoryDecorations.Select(decoration => CreateDecorationIconAsync(decoration, categoryFlowPanel, _isIconView));
                    await Task.WhenAll(iconTasks);
                }).ToList();

            await Task.WhenAll(flowPanelTasks);

            await OrderDecorations(decorationsFlowPanel, _isIconView);
        }

        //Homestead Big Logic
        public static async Task PopulateHomesteadBigIconsInFlowPanel(FlowPanel homesteadDecorationsFlowPanel, bool _isIconView)
        {

            var decorationsByCategory = await HomesteadDecorationFetcher.FetchDecorationsAsync();

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

                    var tasks = decorations.Select(decoration => CreateDecorationIconAsync(decoration, categoryFlowPanel, _isIconView));
                    await Task.WhenAll(tasks);
                });

            await Task.WhenAll(categoryTasks);

            await OrderDecorations(homesteadDecorationsFlowPanel, _isIconView);
        }

        //Guild Hall Big Logic
        public static async Task PopulateGuildHallBigIconsInFlowPanel(FlowPanel decorationsFlowPanel, bool _isIconView)
        {
            var decorationsByCategory = await GuildHallDecorationFetcher.FetchDecorationsAsync();

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

                    var iconTasks = categoryDecorations.Select(decoration => CreateDecorationIconAsync(decoration, categoryFlowPanel, _isIconView));
                    await Task.WhenAll(iconTasks);
                }).ToList();

            await Task.WhenAll(flowPanelTasks);

            await OrderDecorations(decorationsFlowPanel, _isIconView);
        }

        public static async Task CreateDecorationIconAsync(Decoration decoration, FlowPanel categoryFlowPanel, bool _isIconView)
        {
            try
            {
                var iconResponse = await DecorModule.DecorModuleInstance.Client.GetByteArrayAsync(decoration.IconUrl);
                var iconTexture = CreateIconTexture(iconResponse);
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
                            if (lastClickedIconPanel != null && lastClickedIconPanel.BackgroundColor == new Color(254, 254, 176))
                            {
                                lastClickedIconPanel.BackgroundColor = Color.Black;
                                decorationIconImage.Opacity = 1f;
                            }

                            borderPanel.BackgroundColor = new Color(254, 254, 176);
                            decorationIconImage.Opacity = 1f;

                            lastClickedIconPanel = borderPanel;
                            var decorModule = DecorModule.DecorModuleInstance;
                            await RightSideSection.UpdateDecorationImageAsync(decoration, decorModule.DecorWindow, decorModule.DecorationImage);
                        };
                    }
                }
                else
                {
                    var imageResponse = await DecorModule.DecorModuleInstance.Client.GetByteArrayAsync(decoration.ImageUrl);
                    var imageTexture = CreateIconTexture(imageResponse);
                    if (imageTexture != null && iconTexture != null)
                    {
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

                        //mainContainer.MouseEntered += (sender, e) =>
                        //{
                        //    if (lastClickedIconPanel != mainContainer)
                        //    {
                        //        mainContainer.BackgroundColor = Color.Gray;
                        //        decorationImage.Opacity = 0.75f;
                        //    }
                        //};

                        //mainContainer.MouseLeft += (sender, e) =>
                        //{
                        //    if (lastClickedIconPanel != mainContainer)
                        //    {
                        //        mainContainer.BackgroundColor = Color.Transparent;
                        //        decorationImage.Opacity = 1f;
                        //    }
                        //};
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Warn($"Failed to load decoration icon for '{decoration.Name}'. Error: {ex.Message}");
            }
        }

        private static Texture2D CreateIconTexture(byte[] iconResponse)
        {
            try
            {
                using (var memoryStream = new MemoryStream(iconResponse))
                using (var graphicsContext = GameService.Graphics.LendGraphicsDeviceContext())
                {
                    return Texture2D.FromStream(graphicsContext.GraphicsDevice, memoryStream);
                }
            }
            catch (Exception ex)
            {
                Logger.Warn($"Failed to create icon texture. Error: {ex.Message}");
                return null;
            }
        }

        public static async Task FilterDecorations(FlowPanel decorationsFlowPanel, string searchText, bool _isIconView)
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
                    // Sort visible decorations by name (BasicTooltipText)
                    visibleDecorations.Sort((a, b) =>
                        string.Compare(a.Children.OfType<Image>().FirstOrDefault().BasicTooltipText,
                                       b.Children.OfType<Image>().FirstOrDefault().BasicTooltipText,
                                       StringComparison.OrdinalIgnoreCase));

                    // Remove all visible decorations from the category panel first
                    foreach (var visibleDecoration in visibleDecorations)
                    {
                        categoryFlowPanel.Children.Remove(visibleDecoration);
                    }

                    // Reinsert them in the sorted order by appending at the end
                    foreach (var visibleDecoration in visibleDecorations)
                    {
                        categoryFlowPanel.Children.Add(visibleDecoration);
                    }

                    await AdjustCategoryHeightAsync(categoryFlowPanel, _isIconView);

                    categoryFlowPanel.Invalidate();
                }
            }

            decorationsFlowPanel.Invalidate();
        }

        public static Task AdjustCategoryHeightAsync(FlowPanel categoryFlowPanel, bool _isIconView)
        {
            int visibleDecorationCount = categoryFlowPanel.Children.OfType<Panel>().Count(p => p.Visible);

            if (visibleDecorationCount == 0)
            {
                categoryFlowPanel.Height = 45;
            }
            else
            {
                int baseHeight = 45;
                int heightIncrementPerDecorationSet = _isIconView ? 52 : 312;
                int numDecorationSets = (int)Math.Ceiling(visibleDecorationCount / (_isIconView ? 9.0 : 4.0));
                int calculatedHeight = baseHeight + numDecorationSets * heightIncrementPerDecorationSet;

                categoryFlowPanel.Height = calculatedHeight + (_isIconView ? 4 : 10);

                categoryFlowPanel.Invalidate();
            }
            return Task.CompletedTask;
        }

        public static async Task OrderDecorations(FlowPanel decorationsFlowPanel, bool _isIconView)
        {
            foreach (var categoryFlowPanel in decorationsFlowPanel.Children.OfType<FlowPanel>())
            {
                bool hasVisibleDecoration = false;

                var visibleDecorations = new List<Panel>();

                // Collect all decoration icon panels
                foreach (var decorationIconPanel in categoryFlowPanel.Children.OfType<Panel>())
                {
                    var decorationIcon = decorationIconPanel.Children.OfType<Image>().FirstOrDefault();

                    if (decorationIcon != null)
                    {
                        decorationIconPanel.Visible = true;
                        visibleDecorations.Add(decorationIconPanel);
                        hasVisibleDecoration = true;
                    }
                }

                categoryFlowPanel.Visible = hasVisibleDecoration;

                if (hasVisibleDecoration)
                {
                    // Sort visible decorations by name (BasicTooltipText)
                    visibleDecorations.Sort((a, b) =>
                        string.Compare(a.Children.OfType<Image>().FirstOrDefault().BasicTooltipText,
                                       b.Children.OfType<Image>().FirstOrDefault().BasicTooltipText,
                                       StringComparison.OrdinalIgnoreCase));

                    // Remove all visible decorations from the category panel first
                    foreach (var visibleDecoration in visibleDecorations)
                    {
                        categoryFlowPanel.Children.Remove(visibleDecoration);
                    }

                    // Reinsert them in the sorted order by appending at the end
                    foreach (var visibleDecoration in visibleDecorations)
                    {
                        categoryFlowPanel.Children.Add(visibleDecoration);
                    }

                    await AdjustCategoryHeightAsync(categoryFlowPanel, _isIconView);
                }
            }
        }
    }
}