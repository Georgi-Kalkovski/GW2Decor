using Blish_HUD.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DecorBlishhudModule.Sections.LeftSideTasks
{
    internal static class OrderDecorations
    {

        public static async Task OrderDecorationsAsync(FlowPanel decorationsFlowPanel, bool _isIconView)
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

                    await AdjustCategoryHeight.AdjustCategoryHeightAsync(categoryFlowPanel, _isIconView);
                }
            }
        }
    }
}