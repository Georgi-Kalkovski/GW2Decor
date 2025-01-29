using Blish_HUD.Controls;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DecorBlishhudModule.Sections.LeftSideTasks
{
    internal static class AdjustCategoryHeight
    {

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
                int heightIncrementPerDecorationSet = _isIconView ? 53 : 312;
                int numDecorationSets = (int)Math.Ceiling(visibleDecorationCount / (_isIconView ? 9.0 : 4.0));
                int calculatedHeight = baseHeight + numDecorationSets * heightIncrementPerDecorationSet;

                categoryFlowPanel.Height = calculatedHeight + (_isIconView ? 4 : 10);

                categoryFlowPanel.Invalidate();
            }
            return Task.CompletedTask;
        }
    }
}