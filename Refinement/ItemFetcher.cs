using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Blish_HUD;
using HtmlAgilityPack;

namespace DecorBlishhudModule.Refinement
{
    public class ItemFetcher
    {
        private static readonly Logger Logger = Logger.GetLogger<DecorModule>();
        public static async Task<Dictionary<string, List<Item>>> FetchItemsAsync(string type)
        {
            string url = null;
            if (type == "farm")
            url = "https://wiki.guildwars2.com/api.php?action=parse&page=Homestead_Refinement%E2%80%94Farm&format=json&prop=text&section=6";
            if (type == "lumber")
                url = "https://wiki.guildwars2.com/api.php?action=parse&page=Homestead_Refinement%E2%80%94Lumber_Mill&format=json&prop=text&section=6";
            if (type == "metal")
                url = "https://wiki.guildwars2.com/api.php?action=parse&page=Homestead_Refinement%E2%80%94Metal_Forge&format=json&prop=text&section=3";

            string response = await DecorModule.DecorModuleInstance.Client.GetStringAsync(url);

            // Parse the JSON to extract the HTML content
            var jsonDoc = JsonDocument.Parse(response);
            string htmlContent = jsonDoc.RootElement
                                        .GetProperty("parse")
                                        .GetProperty("text")
                                        .GetProperty("*")
                                        .GetString();

            // Load the HTML into HtmlAgilityPack
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlContent);

            // Extract the table rows
            var rows = htmlDoc.DocumentNode.SelectNodes("//table//tr");

            // Parse each row into the Item model
            List<Item> items = new List<Item>();
            foreach (var row in rows.Skip(1)) // Skip the header row
            {
                var columns = row.SelectNodes("td");
                if (columns == null || columns.Count < 10) continue; // Ensure sufficient columns exist

                // Extract item ID from the data-id attribute in the buy column
                string id = columns[2].SelectSingleNode(".//span[@data-id]")?.GetAttributeValue("data-id", "");

                var item = new Item
                {
                    Id = int.Parse(id),
                    Name = columns[0].InnerText.Trim(),
                    DefaultQty = int.TryParse(columns[1].InnerText.Trim(), out var defaultQty) ? defaultQty : 0,
                    DefaultBuy = columns[2].InnerText.Trim(),
                    DefaultSell = columns[3].InnerText.Trim(),
                    TradeEfficiency1Qty = int.TryParse(columns[4].InnerText.Trim(), out var te1xQty) ? te1xQty : 0,
                    TradeEfficiency1Buy = columns[5].InnerText.Trim(),
                    TradeEfficiency1Sell = columns[6].InnerText.Trim(),
                    TradeEfficiency2Qty = double.TryParse(columns[7].InnerText.Trim(), out var te2xQty) ? te2xQty : 0.5,
                    TradeEfficiency2Buy = columns[8].InnerText.Trim(),
                    TradeEfficiency2Sell = columns[9].InnerText.Trim()
                };

                items.Add(item);
            }

            // Fetch unit prices for all items
            var updatedItems = await UpdateItemPrices(items);

            // Group the items by name if needed
            var groupedItems = updatedItems.GroupBy(i => i.Name)
                                           .ToDictionary(g => g.Key, g => g.ToList());

            return groupedItems;
        }
        private static async Task<List<Item>> UpdateItemPrices(List<Item> items)
        {
            // Extract unique item IDs
            var itemIds = items.Select(i => i.Id).Distinct().ToList();

            // Create a comma-separated list of IDs for the API request
            var ids = string.Join(",", itemIds);

            // Fetch unit prices from the Guild Wars 2 API
            string priceApiUrl = $"https://api.guildwars2.com/v2/commerce/prices?ids={ids}";
              
            string priceResponse = await DecorModule.DecorModuleInstance.Client.GetStringAsync(priceApiUrl);

            // Parse the price response
            var priceData = JsonSerializer.Deserialize<List<ItemPrice>>(priceResponse, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // Ensures keys like "unit_price" match "UnitPrice"
            });

            // Update the item list with the fetched prices
            foreach (var item in items)
            {
                var priceInfo = priceData?.FirstOrDefault(p => p.Id == item.Id);

                Logger.Info($"item ID: {item.Id}");
                Logger.Info($"priceInfo: {priceInfo}");

                if (priceInfo != null)
                {
                    item.DefaultBuy = (priceInfo.Buys?.Unit_Price * item.DefaultQty).ToString();
                    item.DefaultSell = (priceInfo.Sells?.Unit_Price * item.DefaultQty).ToString();

                    item.TradeEfficiency1Buy = (priceInfo.Buys?.Unit_Price * item.TradeEfficiency1Qty).ToString();
                    item.TradeEfficiency1Sell = (priceInfo.Sells?.Unit_Price * item.TradeEfficiency1Qty).ToString();

                    item.TradeEfficiency2Buy = Math.Ceiling((priceInfo.Buys?.Unit_Price ?? 0) * item.TradeEfficiency2Qty).ToString();
                    item.TradeEfficiency2Sell = Math.Ceiling((priceInfo.Sells?.Unit_Price ?? 0) * item.TradeEfficiency2Qty).ToString();
                }
            }

            return items;
        }
    }
}