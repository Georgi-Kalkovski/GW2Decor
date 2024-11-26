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

        public static async Task PopulateDecorations(FlowPanel homesteadDecorationsFlowPanel, FlowPanel guildHallDecorationsFlowPanel)
        {
            homesteadDecorationsFlowPanel.Children.Clear();
            string url = "https://wiki.guildwars2.com/api.php?action=parse&page=Decoration/Homestead&format=json&prop=text";
            var decorationsByCategory = await HomesteadDecorationFetcher.FetchDecorationsAsync(url);

            // Normalize category keys for case-insensitive lookups
            var caseInsensitiveCategories = new Dictionary<string, List<Decoration>>(StringComparer.OrdinalIgnoreCase);
            foreach (var kvp in decorationsByCategory)
            {
                caseInsensitiveCategories[kvp.Key] = kvp.Value;
            }

            // Get predefined categories and filter the ones that exist
            List<string> predefinedCategories = HomesteadCategories.GetCategories();
            foreach (var category in predefinedCategories.Where(category => caseInsensitiveCategories.ContainsKey(category)))
            {
                 PopulateHomesteadIconsInFlowPanel(category, caseInsensitiveCategories[category], homesteadDecorationsFlowPanel);
            }

            List<string> categories = GuildHallCategories.GetCategories();

            foreach (string category in categories)
            {
                await PopulateGuildHallIconsInFlowPanel(category, guildHallDecorationsFlowPanel);
            }
        }

        //Homestead Logic
        public static async void PopulateHomesteadIconsInFlowPanel(string category, List<Decoration> decorations, FlowPanel decorationsFlowPanel)
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

            var tasks = decorations.Select(decoration => CreateDecorationIconAsync(decoration, categoryFlowPanel)).ToList();
            await Task.WhenAll(tasks);

            await OrderDecorations(decorationsFlowPanel);
        }

        //Guild Hall Logic
        public static async Task PopulateGuildHallIconsInFlowPanel(string category, FlowPanel decorationsFlowPanel)
        {
            string formattedCategoryName = category.Replace(" ", "_");
            string categoryUrl = $"https://wiki.guildwars2.com/api.php?action=parse&page=Decoration/Guild_hall/{formattedCategoryName}&format=json&prop=text";
            var decorations = await GuildHallDecorationFetcher.FetchDecorationsAsync(categoryUrl);

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
                CanCollapse = false,
                Parent = decorationsFlowPanel,
                ControlPadding = new Vector2(4, 4)
            };

            var tasks = decorations.Select(decoration => CreateDecorationIconAsync(decoration, categoryFlowPanel)).ToArray();
            await Task.WhenAll(tasks);

            await OrderDecorations(decorationsFlowPanel);
        }

        public static async Task CreateDecorationIconAsync(Decoration decoration, FlowPanel categoryFlowPanel)
        {
            try
            {
                var iconResponse = await DecorModule.DecorModuleInstance.Client.GetByteArrayAsync(decoration.IconUrl);

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