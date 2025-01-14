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
    public class CustomTableFarm
    {
        private static FlowPanel _tablePanel;

        private static FlowPanel _name;
        private static FlowPanel _def;
        private static FlowPanel _eff1;
        private static FlowPanel _eff2;
        private static FlowPanel _defQty;
        private static FlowPanel _defBuy;
        private static FlowPanel _defSell;
        private static FlowPanel _eff1Qty;
        private static FlowPanel _eff1Buy;
        private static FlowPanel _eff1Sell;
        private static FlowPanel _eff2Qty;
        private static FlowPanel _eff2Buy;
        private static FlowPanel _eff2Sell;

        private static Texture2D _copper = DecorModule.DecorModuleInstance.CopperCoin;
        private static Texture2D _silver = DecorModule.DecorModuleInstance.SilverCoin;

        private static Texture2D _effiency = DecorModule.DecorModuleInstance.Efficiency;

        private static Texture2D _arrowUp = DecorModule.DecorModuleInstance.ArrowUp;
        private static Texture2D _arrowDown = DecorModule.DecorModuleInstance.ArrowDown;
        private static Texture2D _arrowNeutral = DecorModule.DecorModuleInstance.ArrowNeutral;


        private static string _activeColumn = "Name";
        private static bool _isAscending = true;

        private static Dictionary<string, List<Item>> _currentItemsByType = new();
        private static Dictionary<string, Dictionary<string, List<Item>>> _itemsByCategoryByType = new();

        public static async Task Initialize(FlowPanel tablePanel, string type)
        {
            if (!_itemsByCategoryByType.ContainsKey(type))
            {
                _itemsByCategoryByType[type] = await ItemFetcher.FetchItemsAsync(type);
                _currentItemsByType[type] = null;
            }

            _tablePanel = tablePanel;
            _tablePanel.Location = new Point(20, 0);
            _tablePanel.ShowBorder = true;
            _tablePanel.Width = 1050;
            _tablePanel.ControlPadding = new Vector2(2, 0);

            // Create headers
            _name = CreateHeader("Name", 255, 565, item => item.Name, type);
            _name.Icon = _arrowDown;
            _def = CreateHeader("Default", 255, 600, null, type);
            _eff1 = CreateHeader("Trade Efficiency (1x)", 255, 600, null, type);
            _eff1.Icon = _effiency;
            _eff2 = CreateHeader("Trade Efficiency (2x)", 255, 600, null, type);
            _eff2.Icon = _effiency;

            // Create Default columns
            _defQty = CreateInnerHeader("Qty", 68, _def, item => item.DefaultQty, type);
            _defBuy = CreateInnerHeader("Buy", 93, _def, item => item.DefaultBuy, type);
            _defSell = CreateInnerHeader("Sell", 93, _def, item => item.DefaultSell, type);

            // Create Efficiency (1x) columns
            _eff1Qty = CreateInnerHeader("Qty", 68, _eff1, item => item.TradeEfficiency1Qty, type);
            _eff1Buy = CreateInnerHeader("Buy", 93, _eff1, item => item.TradeEfficiency1Buy, type);
            _eff1Sell = CreateInnerHeader("Sell", 93, _eff1, item => item.TradeEfficiency1Sell, type);

            // Create Efficiency (2x) columns
            _eff2Qty = CreateInnerHeader("Qty", 68, _eff2, item => item.TradeEfficiency2Qty, type);
            _eff2Buy = CreateInnerHeader("Buy", 93, _eff2, item => item.TradeEfficiency2Buy, type);
            _eff2Sell = CreateInnerHeader("Sell", 93, _eff2, item => item.TradeEfficiency2Sell, type);

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
                    ResetHeaderIcons();

                    if (_activeColumn == text)
                    {
                        _isAscending = !_isAscending;
                    }
                    else
                    {
                        _activeColumn = text;
                        _isAscending = true;
                    }

                    headerPanel.Icon = _isAscending ? _arrowDown : _arrowUp;

                    SortAndPopulate(type, sortKeySelector);
                };
            }

            return headerPanel;
        }

        private static FlowPanel CreateInnerHeader(string text, int xSize, Panel flowPanel, Func<Item, IComparable> sortKeySelector = null, string type = null)
        {

            var headerPanel = new FlowPanel
            {
                Parent = flowPanel,
                Title = text,
                Width = xSize,
                Location = new Point(10, 0),
                Icon = _arrowNeutral
            };

            if (sortKeySelector != null)
            {
                headerPanel.Click += (s, e) =>
                {
                    ResetHeaderIcons();

                    if (_activeColumn == text)
                    {
                        _isAscending = !_isAscending;
                    }
                    else
                    {
                        _activeColumn = text;
                        _isAscending = true;
                    }

                    headerPanel.Icon = _activeColumn == text ? (_isAscending ? _arrowDown : _arrowUp) : null;

                    _ = SortAndPopulate(type, item =>
                    {
                        try
                        {
                            if (text == "Buy" || text == "Sell")
                            {
                                return int.Parse(sortKeySelector(item).ToString());
                            }
                        }
                        catch
                        {
                            return 0;
                        }

                        return sortKeySelector(item);
                    });
                };
            }

            return headerPanel;
        }

        private static async Task SortAndPopulate(string type, Func<Item, IComparable> sortKeySelector)
        {
            _currentItemsByType[type] = _currentItemsByType[type]
                .OrderBy(item => _isAscending ? sortKeySelector(item) : null)
                .ThenByDescending(item => !_isAscending ? sortKeySelector(item) : null)
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
                    .OrderBy(item => _isAscending ? item.Name : null)
                    .ThenByDescending(item => !_isAscending ? item.Name : null)
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
                    Parent = _name,
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
                    Parent = _defQty,
                    Text = "    " + item.DefaultQty.ToString(),
                    Size = new Point(68, 30),
                    TextColor = Color.White,
                    Font = GameService.Content.DefaultFont16,
                    BackgroundColor = rowBackgroundColor
                };

                CreateCurrencyDisplay(_defBuy, item.DefaultBuy, rowBackgroundColor);
                CreateCurrencyDisplay(_defSell, item.DefaultSell, rowBackgroundColor);

                // Add Trade Efficiency (1x) columns
                new Label
                {
                    Parent = _eff1Qty,
                    Text = "    " + item.TradeEfficiency1Qty.ToString(),
                    Size = new Point(68, 30),
                    TextColor = Color.White,
                    Font = GameService.Content.DefaultFont16,
                    BackgroundColor = rowBackgroundColor
                };

                CreateCurrencyDisplay(_eff1Buy, item.TradeEfficiency1Buy, rowBackgroundColor);
                CreateCurrencyDisplay(_eff1Sell, item.TradeEfficiency1Sell, rowBackgroundColor);

                // Add Trade Efficiency (2x) columns
                new Label
                {
                    Parent = _eff2Qty,
                    Text = "    " + item.TradeEfficiency2Qty.ToString(),
                    Size = new Point(68, 30),
                    TextColor = Color.White,
                    Font = GameService.Content.DefaultFont16,
                    BackgroundColor = rowBackgroundColor
                };

                CreateCurrencyDisplay(_eff2Buy, item.TradeEfficiency2Buy, rowBackgroundColor);
                CreateCurrencyDisplay(_eff2Sell, item.TradeEfficiency2Sell, rowBackgroundColor);
            }

            int labelHeight = 30;
            int padding = 40;
            int totalHeight = (labelHeight * itemCount) + padding;

            _name.Size = new Point(_name.Size.X, totalHeight);
            _def.Size = new Point(_def.Size.X, totalHeight + 35);
            _eff1.Size = new Point(_eff1.Size.X, totalHeight + 35);
            _eff2.Size = new Point(_eff2.Size.X, totalHeight + 35);
            _name.Location = new Point(_name.Location.X, _tablePanel.Location.Y + 35);

            // Update the sizes of inner headers to match parent headers
            UpdateInnerPanelHeights();
        }

        private static FlowPanel CreateCurrencyDisplay(FlowPanel parent, string value, Color backgroundColor)
        {
            var flowPanel = new FlowPanel
            {
                Parent = parent,
                Size = new Point(98, 30),
                BackgroundColor = backgroundColor,
            };

            // Add silver part if applicable
            new Label
            {
                Parent = flowPanel,
                Text = value.Length >= 3
                    ? value.Length == 3
                        ? "  " + value.Substring(value.Length - 3, 1)
                        : " " + value.Substring(value.Length - 4, 2)
                    : string.Empty,
                Size = value.Length > 2 ? new Point(25, 30) : new Point(48, 30),
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
                Size = new Point(22, 30),
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
            _defQty.Size = new Point(_defQty.Size.X, _def.Size.Y);
            _defBuy.Size = new Point(_defBuy.Size.X, _def.Size.Y);
            _defSell.Size = new Point(_defSell.Size.X, _def.Size.Y);

            _eff1Qty.Size = new Point(_eff1Qty.Size.X, _eff1.Size.Y);
            _eff1Buy.Size = new Point(_eff1Buy.Size.X, _eff1.Size.Y);
            _eff1Sell.Size = new Point(_eff1Sell.Size.X, _eff1.Size.Y);

            _eff2Qty.Size = new Point(_eff2Qty.Size.X, _eff2.Size.Y);
            _eff2Buy.Size = new Point(_eff2Buy.Size.X, _eff2.Size.Y);
            _eff2Sell.Size = new Point(_eff2Sell.Size.X, _eff2.Size.Y);
        }

        private static void ClearColumnContent()
        {
            _name.Children.Clear();
            _defQty.Children.Clear();
            _defBuy.Children.Clear();
            _defSell.Children.Clear();
            _eff1Qty.Children.Clear();
            _eff1Buy.Children.Clear();
            _eff1Sell.Children.Clear();
            _eff2Qty.Children.Clear();
            _eff2Buy.Children.Clear();
            _eff2Sell.Children.Clear();
        }
        private static void ResetHeaderIcons()
        {
            _name.Icon = _arrowNeutral;
            _defQty.Icon = _arrowNeutral;
            _defBuy.Icon = _arrowNeutral;
            _defSell.Icon = _arrowNeutral;
            _eff1Qty.Icon = _arrowNeutral;
            _eff1Buy.Icon = _arrowNeutral;
            _eff1Sell.Icon = _arrowNeutral;
            _eff2Qty.Icon = _arrowNeutral;
            _eff2Buy.Icon = _arrowNeutral;
            _eff2Sell.Icon = _arrowNeutral;
        }

    }
}
