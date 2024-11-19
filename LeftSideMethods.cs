using Blish_HUD.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DecorBlishhudModule
{
    public class LeftSideMethods
    {

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
                        string.Compare(((Image)a.Children.OfType<Image>().FirstOrDefault()).BasicTooltipText,
                                       ((Image)b.Children.OfType<Image>().FirstOrDefault()).BasicTooltipText,
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
    }
}
