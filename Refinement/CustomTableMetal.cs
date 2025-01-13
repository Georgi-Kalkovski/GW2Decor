using Blish_HUD;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DecorBlishhudModule.Refinement
{
    public class CustomTableMetal
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

        private static Texture2D _copper = DecorModule.DecorModuleInstance.CopperCoin;
        private static Texture2D _silver = DecorModule.DecorModuleInstance.SilverCoin;

        private static bool _ascended = true;

        private static Dictionary<string, List<Item>> _currentItemsByType = new();
        private static Dictionary<string, Dictionary<string, List<Item>>> _itemsByCategoryByType = new();

        public static async Task Initialize(FlowPanel tablePanel, string type)
        {
            if (!_itemsByCategoryByType.ContainsKey(type))
            {
                _itemsByCategoryByType[type] = await ItemFetcher.FetchItemsAsync(type);
                _currentItemsByType[type] = null; // Initialize the current items for this type
            }

            _tablePanel = tablePanel;
            _tablePanel.Location = new Point(20, 10);
            _tablePanel.ShowBorder = true;
            _tablePanel.Width = 1050;
            _tablePanel.ControlPadding = new Vector2(2, 0);

            // Create headers
            name = CreateHeader("Name", 255, 565, item => item.Name, type);
            def = CreateHeader("Default", 255, 600, null, type);
            eff1 = CreateHeader("Trade Efficiency (1x)", 255, 600, null, type);
            eff2 = CreateHeader("Trade Efficiency (2x)", 255, 600, null, type);

            // Create Default columns
            defQty = CreateInnerHeader("Qty", 60, def, item => item.DefaultQty, type);
            defBuy = CreateInnerHeader("Buy", 97, def, item => item.DefaultBuy, type);
            defSell = CreateInnerHeader("Sell", 97, def, item => item.DefaultSell, type);

            // Create Efficiency (1x) columns
            eff1Qty = CreateInnerHeader("Qty", 60, eff1, item => item.TradeEfficiency1Qty, type);
            eff1Buy = CreateInnerHeader("Buy", 97, eff1, item => item.TradeEfficiency1Buy, type);
            eff1Sell = CreateInnerHeader("Sell", 97, eff1, item => item.TradeEfficiency1Sell, type);

            // Create Efficiency (2x) columns
            eff2Qty = CreateInnerHeader("Qty", 60, eff2, item => item.TradeEfficiency2Qty, type);
            eff2Buy = CreateInnerHeader("Buy", 97, eff2, item => item.TradeEfficiency2Buy, type);
            eff2Sell = CreateInnerHeader("Sell", 97, eff2, item => item.TradeEfficiency2Sell, type);

            // Fetch and populate the table with items
            await PopulateTable(type);
        }

        private static FlowPanel CreateHeader(string text, int xSize, int ySize, Func<Item, IComparable> sortKeySelector = null, string type = null)
        {
            var headerPanel = new FlowPanel
            {
                Parent = _tablePanel,
                Title = text,
                Size = new Point(xSize, ySize),
                BackgroundColor = new Color(0, 0, 0, 75),
            };

            if (sortKeySelector != null)
            {
                headerPanel.Click += (s, e) =>
                {
                    _ascended = !_ascended; // Toggle ascending/descending
                    SortAndPopulate(type, sortKeySelector); // Sort items and update the table
                };
            };

            return headerPanel;
        }

        private static FlowPanel CreateInnerHeader(string text, int xSize, Panel flowPanel, Func<Item, IComparable> sortKeySelector = null, string type = null)
        {
            var headerPanel = new FlowPanel
            {
                Parent = flowPanel,
                Title = text,
                Width = xSize,
                Location = new Point(10, 0)
            };

            if (sortKeySelector != null)
            {
                headerPanel.Click += (s, e) =>
                {
                    _ascended = !_ascended; // Toggle ascending/descending
                    SortAndPopulate(type, sortKeySelector); // Sort items and update the table
                };
            }

            return headerPanel;
        }

        private static async Task SortAndPopulate(string type, Func<Item, IComparable> sortKeySelector)
        {
            _currentItemsByType[type] = _currentItemsByType[type]
                .OrderBy(item => _ascended ? sortKeySelector(item) : null)
                .ThenByDescending(item => !_ascended ? sortKeySelector(item) : null)
                .ToList();

            await PopulateTable(type);
        }

        private static async Task PopulateTable(string type)
        {
            // Clear existing content from columns
            ClearColumnContent();

            // Flatten and sort items by name
            if (_currentItemsByType[type] == null)
            {
                _currentItemsByType[type] = _itemsByCategoryByType[type]
                    .SelectMany(category => category.Value)
                    .OrderBy(item => _ascended ? item.Name : null)
                    .ThenByDescending(item => !_ascended ? item.Name : null)
                    .ToList();
            }

            int itemCount = 0;

            foreach (var item in _currentItemsByType[type])
            {
                itemCount++;

                // Determine if the current row is even
                bool isEvenRow = itemCount % 2 == 0;
                Color rowBackgroundColor = isEvenRow ? new Color(0, 0, 0, 100) : Color.Transparent;

                // Add Name
                var nameFlowPanel = new FlowPanel
                {
                    Parent = name,
                    FlowDirection = ControlFlowDirection.TopToBottom,
                    Size = new Point(255, 30),
                };
                var texture = await LeftSideSection.GetOrCreateTextureAsync(item.Name, item.Icon);

                if (texture != null)
                {
                    new Image(texture)
                    {
                        Parent = nameFlowPanel,
                        Size = new Point(30, 30),
                        Location = new Point(0, 0),
                        BackgroundColor = Color.Transparent,
                    };
                }
                new Label
                {
                    Parent = nameFlowPanel,
                    Text = " " + item.Name,
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
                    Text = "    " + item.DefaultQty.ToString(),
                    Size = new Point(60, 30),
                    TextColor = Color.White,
                    Font = GameService.Content.DefaultFont16,
                    BackgroundColor = rowBackgroundColor
                };

                CreateCurrencyDisplay(defBuy, item.DefaultBuy, rowBackgroundColor);
                CreateCurrencyDisplay(defSell, item.DefaultSell, rowBackgroundColor);

                // Add Trade Efficiency (1x) columns
                new Label
                {
                    Parent = eff1Qty,
                    Text = "    " + item.TradeEfficiency1Qty.ToString(),
                    Size = new Point(60, 30),
                    TextColor = Color.White,
                    Font = GameService.Content.DefaultFont16,
                    BackgroundColor = rowBackgroundColor
                };

                CreateCurrencyDisplay(eff1Buy, item.TradeEfficiency1Buy, rowBackgroundColor);
                CreateCurrencyDisplay(eff1Sell, item.TradeEfficiency1Sell, rowBackgroundColor);

                // Add Trade Efficiency (2x) columns
                new Label
                {
                    Parent = eff2Qty,
                    Text = "    " + item.TradeEfficiency2Qty.ToString(),
                    Size = new Point(60, 30),
                    TextColor = Color.White,
                    Font = GameService.Content.DefaultFont16,
                    BackgroundColor = rowBackgroundColor
                };

                CreateCurrencyDisplay(eff2Buy, item.TradeEfficiency2Buy, rowBackgroundColor);
                CreateCurrencyDisplay(eff2Sell, item.TradeEfficiency2Sell, rowBackgroundColor);
            }

            int labelHeight = 30;
            int padding = 40;
            int totalHeight = (labelHeight * itemCount) + padding;

            name.Size = new Point(name.Size.X, totalHeight);
            def.Size = new Point(def.Size.X, totalHeight + 35);
            eff1.Size = new Point(eff1.Size.X, totalHeight + 35);
            eff2.Size = new Point(eff2.Size.X, totalHeight + 35);
            name.Location = new Point(name.Location.X, _tablePanel.Location.Y + 25);

            // Update the sizes of inner headers to match parent headers
            UpdateInnerPanelHeights();
        }

        private static FlowPanel CreateCurrencyDisplay(FlowPanel parent, string value, Color backgroundColor)
        {
            var flowPanel = new FlowPanel
            {
                Parent = parent,
                Size = new Point(98, 30),
                BackgroundColor = backgroundColor
            };

            // Add silver part if applicable
            new Label
            {
                Parent = flowPanel,
                Text = value.Length >= 3
                    ? value.Length == 3
                        ? " " + value.Substring(value.Length - 3, 1)
                        : value.Substring(value.Length - 4, 2)
                    : string.Empty,
                Size = value.Length > 2 ? new Point(18, 30) : new Point(45, 0),
                TextColor = Color.White,
                Font = GameService.Content.DefaultFont16,
            };

            new Image(_silver)
            {
                Parent = value.Length > 2 ? flowPanel : null,
            };

            // Add copper part
            new Label
            {
                Parent = flowPanel,
                Text = value.Length >= 2 ? value.Substring(value.Length - 2) : value,
                Size = new Point(25, 30),
                TextColor = Color.White,
                Font = GameService.Content.DefaultFont16,
            };

            new Image(_copper)
            {
                Parent = flowPanel,
            };

            return flowPanel;
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

        private static void ClearColumnContent()
        {
            name.Children.Clear();
            defQty.Children.Clear();
            defBuy.Children.Clear();
            defSell.Children.Clear();
            eff1Qty.Children.Clear();
            eff1Buy.Children.Clear();
            eff1Sell.Children.Clear();
            eff2Qty.Children.Clear();
            eff2Buy.Children.Clear();
            eff2Sell.Children.Clear();
        }
    }
}
