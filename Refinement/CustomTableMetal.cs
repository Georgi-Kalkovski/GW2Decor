using Blish_HUD;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace DecorBlishhudModule.Refinement
{
    public class CustomTableMetal
    {
        private static FlowPanel _tablePanel;

        private static FlowPanel _nameTimer;
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

        private static Dictionary<Item, Tooltip> itemTooltips = new Dictionary<Item, Tooltip>();

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
            _name = await CreateHeader("Name", 255, 565, item => item.Name, type);
            _name.Icon = _arrowDown;
            _def = await CreateHeader("Default", 255, 600, null, type);
            _eff1 = await CreateHeader("Trade Efficiency (1x)", 255, 600, null, type);
            _eff1.Icon = _effiency;
            _eff2 = await CreateHeader("Trade Efficiency (2x)", 255, 600, null, type);
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
            _eff2Qty = CreateInnerHeader("Qty", 68, _eff2, item => ItemFetcher.ParseDoubleInvariant(item.TradeEfficiency2Qty.ToString()), type);
            _eff2Buy = CreateInnerHeader("Buy", 93, _eff2, item => ItemFetcher.ParseDoubleInvariant(item.TradeEfficiency2Buy), type);
            _eff2Sell = CreateInnerHeader("Sell", 93, _eff2, item => ItemFetcher.ParseDoubleInvariant(item.TradeEfficiency2Sell), type);

            // Fetch and populate the table with items
            await PopulateTable(type);
        }

        private static async Task<FlowPanel> CreateHeader(string text, int xSize, int ySize, Func<Item, IComparable> sortKeySelector = null, string type = null)
        {
            if (text == "Name")
            {
                await InitializeNameTimer(_tablePanel, type);
            };

            var headerPanel = new FlowPanel
            {
                Parent = text == "Name" ? _nameTimer : _tablePanel,
                Title = text,
                Size = new Point(xSize, ySize),
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
                Icon = _arrowNeutral,
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
            var sortedItems = _currentItemsByType[type]
                .OrderBy(item => _isAscending ? sortKeySelector(item) : null)
                .ThenByDescending(item => !_isAscending ? sortKeySelector(item) : null)
                .ToList();

            if (!sortedItems.SequenceEqual(_currentItemsByType[type]))
            {
                _currentItemsByType[type] = sortedItems;
                await PopulateTable(type);
            }
        }

        public static async Task PopulateTable(string type)
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

                // Determine if the current row is odd
                bool isOddRow = itemCount % 2 == 0;
                Color rowBackgroundColor = isOddRow ? new Color(0, 0, 0, 175) : new Color(0, 0, 0, 75);

                // Add Name
                var nameFlowPanel = new FlowPanel
                {
                    Parent = _name,
                    FlowDirection = ControlFlowDirection.TopToBottom,
                    Size = new Point(255, 30),
                    Padding = new Thickness(10, 10),
                    Tooltip = await CustomTooltip(item)
                };

                var texture = await LeftSideSection.GetOrCreateTextureAsync(item.Name, item.Icon);
                if (texture != null)
                {
                    new Image(texture)
                    {
                        Parent = nameFlowPanel,
                        Size = new Point(30, 30),
                        Location = new Point(0, 0),
                        Tooltip = nameFlowPanel.Tooltip
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
                    Padding = new Thickness(10, 10),
                    Tooltip = nameFlowPanel.Tooltip
                };

                // Add Default columns
                new Label
                {
                    Parent = _defQty,
                    Text = "    " + item.DefaultQty.ToString(),
                    Size = new Point(68, 30),
                    TextColor = Color.White,
                    Font = GameService.Content.DefaultFont16,
                    BackgroundColor = rowBackgroundColor,
                    Tooltip = nameFlowPanel.Tooltip
                };

                await CreateCurrencyDisplay(_defBuy, item, item.DefaultBuy, rowBackgroundColor);
                await CreateCurrencyDisplay(_defSell, item, item.DefaultSell, rowBackgroundColor);

                // Add Trade Efficiency (1x) columns
                new Label
                {
                    Parent = _eff1Qty,
                    Text = "    " + item.TradeEfficiency1Qty.ToString(),
                    Size = new Point(68, 30),
                    TextColor = Color.White,
                    Font = GameService.Content.DefaultFont16,
                    BackgroundColor = rowBackgroundColor,
                    Tooltip = nameFlowPanel.Tooltip
                };

                await CreateCurrencyDisplay(_eff1Buy, item, item.TradeEfficiency1Buy, rowBackgroundColor);
                await CreateCurrencyDisplay(_eff1Sell, item, item.TradeEfficiency1Sell, rowBackgroundColor);

                // Add Trade Efficiency (2x) columns
                new Label
                {
                    Parent = _eff2Qty,
                    Text = "    " + ItemFetcher.ParseDoubleInvariant(item.TradeEfficiency2Qty.ToString()).ToString(),
                    Size = new Point(68, 30),
                    TextColor = Color.White,
                    Font = GameService.Content.DefaultFont16,
                    BackgroundColor = rowBackgroundColor,
                    Tooltip = nameFlowPanel.Tooltip
                };

                await CreateCurrencyDisplay(_eff2Buy, item, item.TradeEfficiency2Buy, rowBackgroundColor);
                await CreateCurrencyDisplay(_eff2Sell, item, item.TradeEfficiency2Sell, rowBackgroundColor);
            }

            int labelHeight = 30;
            int padding = 125;
            int totalHeight = (labelHeight * itemCount) + padding;

            _name.Size = new Point(_name.Size.X, totalHeight - 35);
            _def.Size = new Point(_def.Size.X, totalHeight);
            _eff1.Size = new Point(_eff1.Size.X, totalHeight);
            _eff2.Size = new Point(_eff2.Size.X, totalHeight);

            // Update the sizes of inner headers to match parent headers
            UpdateInnerPanelHeights();
        }

        private static async Task<FlowPanel> CreateCurrencyDisplay(FlowPanel parent, Item item, string value, Color backgroundColor)
        {
            var flowPanel = new FlowPanel
            {
                Parent = parent,
                Size = new Point(98, 30),
                BackgroundColor = backgroundColor,
                Tooltip = await CustomTooltip(item)
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
                Tooltip = flowPanel.Tooltip
            };

            new Image(_silver)
            {
                Parent = value.Length > 2 ? flowPanel : null,
                Tooltip = flowPanel.Tooltip
            };

            // Add copper part
            new Label
            {
                Parent = flowPanel,
                Text = value.Length >= 2 ? value.Substring(value.Length - 2) : value,
                Size = new Point(22, 30),
                TextColor = Color.White,
                Font = GameService.Content.DefaultFont16,
                Tooltip = flowPanel.Tooltip
            };

            new Image(_copper)
            {
                Parent = flowPanel,
                Tooltip = flowPanel.Tooltip
            };

            return flowPanel;
        }

        private static void UpdateInnerPanelHeights()
        {
            _nameTimer.Size = new Point(_name.Size.X, _name.Size.Y - 15);
            _name.Location = new Point(0, 35);

            _defQty.Size = new Point(_defQty.Size.X, _def.Size.Y - 86);
            _defBuy.Size = new Point(_defBuy.Size.X, _def.Size.Y - 86);
            _defSell.Size = new Point(_defSell.Size.X, _def.Size.Y - 86);

            _eff1Qty.Size = new Point(_eff1Qty.Size.X, _eff1.Size.Y - 86);
            _eff1Buy.Size = new Point(_eff1Buy.Size.X, _eff1.Size.Y - 86);
            _eff1Sell.Size = new Point(_eff1Sell.Size.X, _eff1.Size.Y - 86);

            _eff2Qty.Size = new Point(_eff2Qty.Size.X, _eff2.Size.Y - 86);
            _eff2Buy.Size = new Point(_eff2Buy.Size.X, _eff2.Size.Y - 86);
            _eff2Sell.Size = new Point(_eff2Sell.Size.X, _eff2.Size.Y - 86);

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

        private static async Task<Tooltip> CustomTooltip(Item item)
        {
            if (itemTooltips.ContainsKey(item))
            {
                return itemTooltips[item];
            }

            var customTooltip = new Tooltip();

            byte[] imageResponse;
            string localImagePath = LeftSideSection.GetImageAndIconFilePath(item.Icon);
            var semaphore = LeftSideSection.GetFileSemaphore(localImagePath);

            await semaphore.WaitAsync();
            try
            {
                if (File.Exists(localImagePath))
                {
                    imageResponse = File.ReadAllBytes(localImagePath);
                }
                else
                {
                    imageResponse = await DecorModule.DecorModuleInstance.Client.GetByteArrayAsync(item.Icon);
                    File.WriteAllBytes(localImagePath, imageResponse);
                }
            }
            finally
            {
                semaphore.Release();
            }

            var icon = new Image
            {
                Parent = customTooltip,
                Texture = LeftSideSection.CreateIconTexture(imageResponse),
                Size = new Point(30, 30),
            };

            var titleLabel = new Label
            {
                Parent = customTooltip,
                Text = item.Name,
                TextColor = Color.White,
                Font = GameService.Content.DefaultFont18,
                Location = new Point(35, 3),
                AutoSizeWidth = true
            };

            itemTooltips[item] = customTooltip;
            return customTooltip;
        }

        private static async Task RefreshPrices(string type)
        {
            if (_currentItemsByType == null)
            {
                Console.WriteLine("Error: _currentItemsByType is null.");
                return;
            }

            if (!_currentItemsByType.ContainsKey(type) || _currentItemsByType[type] == null)
            {
                Console.WriteLine($"Error: _currentItemsByType does not contain key '{type}' or is null.");
                return;
            }

            List<Item> items = _currentItemsByType[type];

            if (items == null || items.Count == 0)
            {
                Console.WriteLine("Error: No items found to update prices.");
                return;
            }

            try
            {
                var updatedItems = await ItemFetcher.UpdateItemPrices(items);
                if (updatedItems == null)
                {
                    Console.WriteLine("Error: UpdateItemPrices returned null.");
                    return;
                }

                _currentItemsByType[type] = updatedItems;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating item prices: {ex}");
                return;
            }

            try
            {
                await UpdatePriceColumns(type);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdatePriceColumns: {ex}");
            }
        }

        private static async Task UpdatePriceColumns(string type)
        {
            if (_currentItemsByType == null)
            {
                Console.WriteLine("Error: _currentItemsByType is null.");
                return;
            }

            if (!_currentItemsByType.ContainsKey(type) || _currentItemsByType[type] == null)
            {
                Console.WriteLine($"Error: Cannot update price columns. _currentItemsByType[{type}] is null.");
                return;
            }

            List<Item> items = _currentItemsByType[type];

            if (items.Count == 0)
            {
                Console.WriteLine($"Error: _currentItemsByType[{type}] is empty.");
                return;
            }

            // Check if UI elements are null before clearing them
            if (_defBuy == null || _defSell == null || _eff1Buy == null || _eff1Sell == null || _eff2Buy == null || _eff2Sell == null)
            {
                Console.WriteLine("Error: One or more UI elements are null. Cannot update price columns.");
                return;
            }

            _defBuy.Children.Clear();
            _defSell.Children.Clear();
            _eff1Buy.Children.Clear();
            _eff1Sell.Children.Clear();
            _eff2Buy.Children.Clear();
            _eff2Sell.Children.Clear();

            foreach (var item in items)
            {
                if (item == null)
                {
                    Console.WriteLine("Warning: Found null item in _currentItemsByType[type]. Skipping...");
                    continue;
                }

                Color rowBackgroundColor = (items.IndexOf(item) % 2 != 0)
                    ? new Color(0, 0, 0, 175)
                    : new Color(0, 0, 0, 75);

                try
                {
                    // Ensure price values are not null before passing to CreateCurrencyDisplay
                    await CreateCurrencyDisplay(_defBuy, item, item.DefaultBuy ?? null, rowBackgroundColor);
                    await CreateCurrencyDisplay(_defSell, item, item.DefaultSell ?? null, rowBackgroundColor);

                    await CreateCurrencyDisplay(_eff1Buy, item, item.TradeEfficiency1Buy ?? null, rowBackgroundColor);
                    await CreateCurrencyDisplay(_eff1Sell, item, item.TradeEfficiency1Sell ?? null, rowBackgroundColor);

                    await CreateCurrencyDisplay(_eff2Buy, item, item.TradeEfficiency2Buy ?? null, rowBackgroundColor);
                    await CreateCurrencyDisplay(_eff2Sell, item, item.TradeEfficiency2Sell ?? null, rowBackgroundColor);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating currency display for item {item.Name ?? "Unknown"}: {ex.Message}");
                }
            }
        }

        private static async Task InitializeNameTimer(Panel parentPanel, string type)
        {
            if (parentPanel == null)
            {
                Console.WriteLine("Error: parentPanel is null. Cannot initialize name timer.");
                return;
            }

            if (string.IsNullOrEmpty(type))
            {
                Console.WriteLine("Error: type is null or empty. Cannot initialize name timer.");
                return;
            }

            try
            {
                _nameTimer = new FlowPanel
                {
                    Parent = parentPanel,
                    FlowDirection = ControlFlowDirection.TopToBottom,
                    Padding = new Thickness(10, 10)
                };

                if (_nameTimer == null)
                {
                    Console.WriteLine("Error: _nameTimer could not be created.");
                    return;
                }

                if (GameService.Content.DefaultFont16 == null)
                {
                    Console.WriteLine("Error: GameService.Content.DefaultFont16 is null. Cannot create label.");
                    return;
                }

                var updateTimerLabel = new Label
                {
                    Parent = _nameTimer,
                    Text = "      Prices will update in 60 s", // Initial countdown display
                    Font = GameService.Content.DefaultFont16, // No fallback
                    TextColor = Color.White,
                    Size = new Point(255, 30),
                    ShowShadow = true,
                    ShadowColor = new Color(0, 0, 0, 255)
                };

                if (updateTimerLabel == null)
                {
                    Console.WriteLine("Error: Failed to create updateTimerLabel.");
                    return;
                }

                int secondsCounter = 60; // Starting countdown value

                var secondsTimer = new Timer(1000); // Trigger every 1 second
                secondsTimer.Elapsed += (sender, e) =>
                {
                    if (secondsCounter > 0)
                    {
                        secondsCounter--;
                        updateTimerLabel.Text = $"      Prices will update in {secondsCounter} s";
                    }
                };
                secondsTimer.AutoReset = true;
                secondsTimer.Enabled = true;

                var updateTimer = new Timer(61000); // Trigger every 61 seconds
                updateTimer.Elapsed += async (sender, e) =>
                {
                    try
                    {
                        secondsCounter = 60; // Reset the countdown
                        await RefreshPrices(type);
                        updateTimerLabel.Text = $"      Prices will update in {secondsCounter} s"; // Reset label visually
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error in price update timer: {ex.Message}");
                    }
                };
                updateTimer.AutoReset = true;
                updateTimer.Enabled = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error in InitializeNameTimer: {ex.Message}");
            }
        }
    }
}