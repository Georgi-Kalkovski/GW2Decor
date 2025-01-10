using Blish_HUD;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Threading.Tasks;

namespace DecorBlishhudModule.Refinement
{
    public class CustomTable
    {
        private static FlowPanel _tablePanel;

        private static FlowPanel name;
        private static FlowPanel def;
        private static FlowPanel eff1;
        private static FlowPanel eff2;
        private static FlowPanel defQty;
        private static FlowPanel defBuy;
        private static FlowPanel defSell;
        private static FlowPanel eff1Qty;
        private static FlowPanel eff1Buy;
        private static FlowPanel eff1Sell;
        private static FlowPanel eff2Qty;
        private static FlowPanel eff2Buy;
        private static FlowPanel eff2Sell;
        private static Texture2D copper = DecorModule.DecorModuleInstance.CopperCoin;
        private static Texture2D silver = DecorModule.DecorModuleInstance.SilverCoin;

        public static async Task Initialize(FlowPanel tablePanel, string type)
        {
            _tablePanel = tablePanel;
            _tablePanel.Location = new Point(20, 10);
            _tablePanel.ShowBorder = true;
            _tablePanel.Width = 1050;
            _tablePanel.ControlPadding = new Vector2(2, 0);

            // Create headers
            name = CreateHeader("Name", 255, 565);
            def = CreateHeader("Default", 255, 600);
            eff1 = CreateHeader("Trade Efficiency (1x)", 255, 600);
            eff2 = CreateHeader("Trade Efficiency (2x)", 255, 600);

            // Create Default columns
            defQty = CreateInnerHeader("Qty", 60, def);
            defBuy = CreateInnerHeader("Buy", 97, def);
            defSell = CreateInnerHeader("Sell", 97, def);

            // Create Efficiency (1x) columns
            eff1Qty = CreateInnerHeader("Qty", 60, eff1);
            eff1Buy = CreateInnerHeader("Buy", 97, eff1);
            eff1Sell = CreateInnerHeader("Sell", 97, eff1);

            // Create Efficiency (2x) columns
            eff2Qty = CreateInnerHeader("Qty", 60, eff2);
            eff2Buy = CreateInnerHeader("Buy", 97, eff2);
            eff2Sell = CreateInnerHeader("Sell", 97, eff2);

            // Fetch and populate the table with items
            await PopulateTable(type);
        }

        private static FlowPanel CreateHeader(string text, int xSize, int ySize)
        {
            var headerPanel = new FlowPanel
            {
                Parent = _tablePanel,
                Title = text,
                Size = new Point(xSize, ySize),
                BackgroundColor = new Color(0, 0, 0, 75),
            };

            return headerPanel;
        }

        private static FlowPanel CreateInnerHeader(string text, int xSize, Panel flowPanel)
        {
            var headerPanel = new FlowPanel
            {
                Parent = flowPanel,
                Title = text,
                Width = xSize,
                Location = new Point(10, 0)
            };

            return headerPanel;
        }

        private static FlowPanel CreateCurrencyDisplay(FlowPanel parent, string value, Texture2D silverIcon, Texture2D copperIcon, Color backgroundColor)
        {
            var flowPanel = new FlowPanel
            {
                Parent = parent,
                Size = new Point(97, 30),
                Location = value.Length == 4 ? new Point(50, 0) : new Point(0, 0),
                BackgroundColor = backgroundColor
            };

            // Add silver part if applicable
            new Label
            {
                Parent = flowPanel,
                Text = value.Length >= 3
                    ? value.Length == 3
                        ? value.Substring(value.Length - 3, 1)
                        : value.Substring(value.Length - 4, 2)
                    : string.Empty,
                Size = value.Length > 2 ? new Point(13, 30) : new Point(40, 0),
                TextColor = Color.White,
                Font = GameService.Content.DefaultFont16,
                BackgroundColor = backgroundColor
            };

            new Image(silverIcon)
            {
                Parent = value.Length > 2 ? flowPanel : null,
            };

            // Add copper part
            new Label
            {
                Parent = flowPanel,
                Text = value.Length >= 2 ? value.Substring(value.Length - 2) : value,
                Size = new Point(20, 30),
                TextColor = Color.White,
                Font = GameService.Content.DefaultFont16,
                BackgroundColor = backgroundColor
            };

            new Image(copperIcon)
            {
                Parent = flowPanel,
            };

            return flowPanel;
        }

        private static async Task PopulateTable(string type)
        {
            // Fetch items
            var itemsByCategory = await ItemFetcher.FetchItemsAsync(type);

            int itemCount = 0;

            foreach (var category in itemsByCategory)
            {
                foreach (var item in category.Value)
                {
                    itemCount++;

                    // Determine if the current row is even
                    bool isEvenRow = itemCount % 2 == 0;
                    Color rowBackgroundColor = isEvenRow ? new Color(0, 0, 0, 100) : Color.Transparent;

                    // Add Name
                    new Label
                    {
                        Parent = name,
                        Text = item.Name,
                        Size = new Point(255, 30),
                        TextColor = Color.White,
                        Font = GameService.Content.DefaultFont16,
                        BackgroundColor = rowBackgroundColor,
                        Padding = new Thickness(10, 0)
                    };

                    // Add Default columns
                    new Label
                    {
                        Parent = defQty,
                        Text = item.DefaultQty.ToString(),
                        Size = new Point(60, 30),
                        TextColor = Color.White,
                        Font = GameService.Content.DefaultFont16,
                        BackgroundColor = rowBackgroundColor
                    };

                    CreateCurrencyDisplay(defBuy, item.DefaultBuy, silver, copper, rowBackgroundColor);
                    CreateCurrencyDisplay(defSell, item.DefaultSell, silver, copper, rowBackgroundColor);

                    // Add Trade Efficiency (1x) columns
                    new Label
                    {
                        Parent = eff1Qty,
                        Text = item.TradeEfficiency1Qty.ToString(),
                        Size = new Point(60, 30),
                        TextColor = Color.White,
                        Font = GameService.Content.DefaultFont16,
                        BackgroundColor = rowBackgroundColor
                    };

                    CreateCurrencyDisplay(eff1Buy, item.TradeEfficiency1Buy, silver, copper, rowBackgroundColor);
                    CreateCurrencyDisplay(eff1Sell, item.TradeEfficiency1Sell, silver, copper, rowBackgroundColor);

                    // Add Trade Efficiency (2x) columns
                    new Label
                    {
                        Parent = eff2Qty,
                        Text = item.TradeEfficiency2Qty.ToString(),
                        Size = new Point(60, 30),
                        TextColor = Color.White,
                        Font = GameService.Content.DefaultFont16,
                        BackgroundColor = rowBackgroundColor
                    };

                    CreateCurrencyDisplay(eff2Buy, item.TradeEfficiency2Buy, silver, copper, rowBackgroundColor);
                    CreateCurrencyDisplay(eff2Sell, item.TradeEfficiency2Sell, silver, copper, rowBackgroundColor);
                }
            }

            int labelHeight = 30;
            int padding = 40;
            int totalHeight = (labelHeight * itemCount) + padding;

            name.Size = new Point(name.Size.X, totalHeight);
            def.Size = new Point(def.Size.X, totalHeight + 35);
            eff1.Size = new Point(eff1.Size.X, totalHeight + 35);
            eff2.Size = new Point(eff2.Size.X, totalHeight + 35);
            name.Location = new Point(name.Location.X, name.Location.Y + 35);

            // Update the sizes of inner headers to match parent headers
            UpdateInnerPanelHeights();
        }
        private static void UpdateInnerPanelHeights()
        {
            // Match the height of the parent panels
            defQty.Size = new Point(defQty.Size.X, def.Size.Y);
            defBuy.Size = new Point(defBuy.Size.X, def.Size.Y);
            defSell.Size = new Point(defSell.Size.X, def.Size.Y);

            eff1Qty.Size = new Point(eff1Qty.Size.X, eff1.Size.Y);
            eff1Buy.Size = new Point(eff1Buy.Size.X, eff1.Size.Y);
            eff1Sell.Size = new Point(eff1Sell.Size.X, eff1.Size.Y);

            eff2Qty.Size = new Point(eff2Qty.Size.X, eff2.Size.Y);
            eff2Buy.Size = new Point(eff2Buy.Size.X, eff2.Size.Y);
            eff2Sell.Size = new Point(eff2Sell.Size.X, eff2.Size.Y);
        }
    }
}
