using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
            string url = type switch
            {
                "farm" => "https://wiki.guildwars2.com/api.php?action=parse&page=Homestead_Refinement%E2%80%94Farm&format=json&prop=text&section=6",
                "lumber" => "https://wiki.guildwars2.com/api.php?action=parse&page=Homestead_Refinement%E2%80%94Lumber_Mill&format=json&prop=text&section=6",
                "metal" => "https://wiki.guildwars2.com/api.php?action=parse&page=Homestead_Refinement%E2%80%94Metal_Forge&format=json&prop=text&section=3",
                _ => null
            };

            if (string.IsNullOrWhiteSpace(url))
            {
                Logger.Error("Invalid refinement type provided.");
                return new Dictionary<string, List<Item>>();
            }

            string response = null;

            try
            {
                response = await RetryPolicyAsync(() => DecorModule.DecorModuleInstance.Client.GetStringAsync(url));
            }
            catch (HttpRequestException ex)
            {
                Logger.Error($"Failed to fetch items for type '{type}': {ex.Message}");
                return new Dictionary<string, List<Item>>();
            }

            try
            {
                // Parse JSON response
                var jsonDoc = JsonDocument.Parse(response);

                if (!jsonDoc.RootElement.TryGetProperty("parse", out var parseElement) ||
                    !parseElement.TryGetProperty("text", out var textElement))
                {
                    Logger.Error("Invalid response structure from the server.");
                    return new Dictionary<string, List<Item>>();
                }

                string htmlContent = textElement.GetProperty("*").GetString();

                // Parse HTML content
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(htmlContent);

                var rows = htmlDoc.DocumentNode.SelectNodes("//table//tr") ?? new HtmlNodeCollection(null);

                List<Item> items = new List<Item>();
                foreach (var row in rows.Skip(1))
                {
                    var columns = row.SelectNodes("td") ?? new HtmlNodeCollection(null);
                    if (columns.Count < 10) continue;

                    // Extract item data safely
                    string id = columns[2].SelectSingleNode(".//span[@data-id]")?.GetAttributeValue("data-id", "0");
                    var imageNode = columns[0].SelectSingleNode(".//img");
                    string imageUrl = imageNode?.GetAttributeValue("src", "");

                    int.TryParse(id, out var itemId);

                    var item = new Item
                    {
                        Id = itemId,
                        Name = columns[0].InnerText.Trim(),
                        Icon = "https://wiki.guildwars2.com" + imageUrl,
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

                // Update item prices and group by name
                var updatedItems = await UpdateItemPrices(items);
                return updatedItems.GroupBy(i => i.Name)
                                   .ToDictionary(g => g.Key, g => g.ToList());
            }
            catch (Exception ex)
            {
                Logger.Error($"Error parsing items for type '{type}': {ex.Message}");
                return new Dictionary<string, List<Item>>();
            }
        }

        public static async Task<List<Item>> UpdateItemPrices(List<Item> items)
        {
            // Extract unique item IDs
            var itemIds = items.Select(i => i.Id).Distinct().ToList();
            int batchSize = 200; // Guild Wars 2 API supports up to 200 IDs per request
            var batches = itemIds.Batch(batchSize); // Helper method to split list into chunks

            foreach (var batch in batches)
            {
                string ids = string.Join(",", batch);
                string priceApiUrl = $"https://api.guildwars2.com/v2/commerce/prices?ids={ids}";

                try
                {
                    string priceResponse = await RetryPolicyAsync(() => DecorModule.DecorModuleInstance.Client.GetStringAsync(priceApiUrl));
                    var priceData = JsonSerializer.Deserialize<List<ItemPrice>>(priceResponse, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    foreach (var item in items)
                    {
                        var priceInfo = priceData?.FirstOrDefault(p => p.Id == item.Id);
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
                }
                catch (HttpRequestException ex)
                {
                    Logger.Error($"Error fetching prices for batch {ids}: {ex.Message}");
                }
            }

            return items;
        }

        async static Task<string> RetryPolicyAsync(Func<Task<string>> action, int retries = 3)
        {
            for (int i = 0; i < retries; i++)
            {
                try
                {
                    return await action();
                }
                catch (HttpRequestException ex) when (i < retries - 1)
                {
                    Logger.Warn($"Retrying due to error: {ex.Message}");
                    await Task.Delay(1000);
                }
            }
            throw new HttpRequestException("Failed after multiple retries.");
        }
    }
}