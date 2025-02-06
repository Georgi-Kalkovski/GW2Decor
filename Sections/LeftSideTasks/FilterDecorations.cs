using Blish_HUD.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DecorBlishhudModule.Sections.LeftSideTasks
{
    internal static class FilterDecorations
    {

        public static async Task FilterDecorationsAsync(FlowPanel decorationsFlowPanel, string searchText, bool _isIconView)
        {
            searchText = searchText.ToLower();

            foreach (var categoryFlowPanel in decorationsFlowPanel.Children.OfType<FlowPanel>())
            {
                bool hasVisibleDecoration = false;
                var visibleDecorations = new List<Panel>();

                foreach (var decorationIconPanel in categoryFlowPanel.Children.OfType<Panel>())
                {
                    var decorationIcon = decorationIconPanel.Children.OfType<Image>().FirstOrDefault();

                    if (decorationIcon != null && decorationIcon.Tooltip != null)
                    {
                        // Find the text from the Tooltip (specifically the Label containing the decoration name)
                        var tooltipLabel = decorationIcon.Tooltip.Children.OfType<Label>().FirstOrDefault();
                        bool matchesSearch = tooltipLabel != null && searchText.Split(' ').All(word => tooltipLabel.Text.ToLower().Contains(word));

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
                    // Sort visible decorations by name (from Tooltip)
                    visibleDecorations.Sort((a, b) =>
                        string.Compare(a.Children.OfType<Image>().FirstOrDefault().Tooltip.Children.OfType<Label>().FirstOrDefault()?.Text,
                                       b.Children.OfType<Image>().FirstOrDefault().Tooltip.Children.OfType<Label>().FirstOrDefault()?.Text,
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

                    categoryFlowPanel.Invalidate();
                }
            }

            decorationsFlowPanel.Invalidate();
        }
    }
}