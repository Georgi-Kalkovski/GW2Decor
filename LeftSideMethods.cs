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

namespace DecorBlishhudModule
{
    public class LeftSideMethods
    {
        private static readonly Logger Logger = Logger.GetLogger<DecorModule>();

        private static Panel lastClickedIconPanel = null;

        public static async Task PopulateDecorations(FlowPanel homesteadDecorationsFlowPanel, FlowPanel guildHallDecorationsFlowPanel)
        {
            await PopulateHomesteadIconsInFlowPanel(homesteadDecorationsFlowPanel);
            await PopulateGuildHallIconsInFlowPanel(guildHallDecorationsFlowPanel);
        }

        //Homestead Logic
        public static async Task PopulateHomesteadIconsInFlowPanel(FlowPanel homesteadDecorationsFlowPanel)
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
                        Title = category,
                        FlowDirection = ControlFlowDirection.LeftToRight,
                        Width = homesteadDecorationsFlowPanel.Width - 20,
                        Height = calculatedHeight,
                        CanCollapse = false, // Maybe true in the future when Scrollbar jump to the top is fixed.
                        Parent = homesteadDecorationsFlowPanel,
                        ControlPadding = new Vector2(4, 4)
                    };

                    var tasks = decorations.Select(decoration => CreateDecorationIconAsync(decoration, categoryFlowPanel));
                    await Task.WhenAll(tasks);
                });

            await Task.WhenAll(categoryTasks);

            await OrderDecorations(homesteadDecorationsFlowPanel);
        }

        //Guild Hall Logic
        public static async Task PopulateGuildHallIconsInFlowPanel(FlowPanel decorationsFlowPanel)
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
                        Title = category,
                        FlowDirection = ControlFlowDirection.LeftToRight,
                        Width = decorationsFlowPanel.Width - 20,
                        Height = calculatedHeight,
                        CanCollapse = false, // Maybe true in the future when Scrollbar jump to the top is fixed.
                        Parent = decorationsFlowPanel,
                        ControlPadding = new Vector2(4, 4)
                    };

                    var iconTasks = categoryDecorations.Select(decoration => CreateDecorationIconAsync(decoration, categoryFlowPanel));
                    await Task.WhenAll(iconTasks);
                }).ToList();

            await Task.WhenAll(flowPanelTasks);

            await OrderDecorations(decorationsFlowPanel);
        }

        public static async Task CreateDecorationIconAsync(Decoration decoration, FlowPanel categoryFlowPanel)
        {
            try
            {
                var iconResponse = await DecorModule.DecorModuleInstance.Client.GetByteArrayAsync(decoration.IconUrl);

                var iconTexture = CreateIconTexture(iconResponse);

                if (iconTexture != null)
                {
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
                        await RightSideMethods.UpdateDecorationImageAsync(decoration, decorModule.DecorWindow, decorModule.DecorationImage);
                    };
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

        public static async Task FilterDecorations(FlowPanel decorationsFlowPanel, string searchText)
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

                    await AdjustCategoryHeightAsync(categoryFlowPanel);

                    categoryFlowPanel.Invalidate();
                }
            }

            decorationsFlowPanel.Invalidate();
        }

        public static Task AdjustCategoryHeightAsync(FlowPanel categoryFlowPanel)
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

        public static async Task OrderDecorations(FlowPanel decorationsFlowPanel)
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

                    await AdjustCategoryHeightAsync(categoryFlowPanel);
                }
            }
        }
    }
}