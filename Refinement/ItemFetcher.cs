﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Blish_HUD;
using HtmlAgilityPack;
using System.Globalization;

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
                "metal" => "https://wiki.guildwars2.com/api.php?action=parse&page=Homestead_Refinement%E2%80%94Metal_Forge&format=json&prop=text&section=6",
                _ => null
            };

            if (string.IsNullOrWhiteSpace(url))
            {
                Logger.Error("Invalid refinement type provided.");
                return new Dictionary<string, List<Item>>();
            }

            if (DecorModule.DecorModuleInstance?.Client == null)
            {
                Logger.Error("HTTP client is not initialized.");
                return new Dictionary<string, List<Item>>();
            }

            string response;
            try
            {
                response = await RetryPolicyAsync(() => DecorModule.DecorModuleInstance.Client.GetStringAsync(url)) ?? string.Empty;
            }
            catch (HttpRequestException ex)
            {
                Logger.Error($"Failed to fetch items for type '{type}': {ex.Message}");
                return new Dictionary<string, List<Item>>();
            }

            if (string.IsNullOrWhiteSpace(response))
            {
                Logger.Error("Received an empty response from the server.");
                return new Dictionary<string, List<Item>>();
            }

            try
            {
                var jsonDoc = JsonDocument.Parse(response);

                if (!jsonDoc.RootElement.TryGetProperty("parse", out var parseElement) ||
                    !parseElement.TryGetProperty("text", out var textElement) ||
                    !textElement.TryGetProperty("*", out var contentElement))
                {
                    Logger.Error("Invalid response structure from the server.");
                    return new Dictionary<string, List<Item>>();
                }

                string htmlContent = contentElement.GetString() ?? string.Empty;
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(htmlContent);

                var rows = htmlDoc.DocumentNode.SelectNodes("//table//tr") ?? new HtmlNodeCollection(null);
                if (rows.Count == 0)
                {
                    Logger.Warn("No table rows found in the HTML content.");
                    return new Dictionary<string, List<Item>>();
                }

                List<Item> items = new List<Item>();
                foreach (var row in rows.Skip(1))
                {
                    var columns = row.SelectNodes("td") ?? new HtmlNodeCollection(null);
                    if (columns.Count < 10) continue;

                    string id = columns[2].SelectSingleNode(".//span[@data-id]")?.GetAttributeValue("data-id", "0") ?? "0";
                    string imageUrl = columns[0].SelectSingleNode(".//img")?.GetAttributeValue("src", "") ?? "";

                    int.TryParse(id, out var itemId);
                    int.TryParse(columns[1].InnerText?.Trim() ?? "0", out var defaultQty);

                    items.Add(new Item
                    {
                        Id = itemId,
                        Name = columns[0].InnerText.Trim(),
                        Icon = "https://wiki.guildwars2.com" + imageUrl,
                        DefaultQty = defaultQty,
                        DefaultBuy = columns[2].InnerText.Trim(),
                        DefaultSell = columns[3].InnerText.Trim(),
                        TradeEfficiency1Qty = int.TryParse(columns[4].InnerText.Trim(), out var te1Qty) ? te1Qty : 0,
                        TradeEfficiency1Buy = columns[5].InnerText.Trim(),
                        TradeEfficiency1Sell = columns[6].InnerText.Trim(),
                        TradeEfficiency2Qty = ParseDoubleInvariant(columns[7].InnerText.Trim(), 0.5),
                        TradeEfficiency2Buy = columns[8].InnerText.Trim(),
                        TradeEfficiency2Sell = columns[9].InnerText.Trim()
                    });
                }

                return await UpdateItemPrices(items)
                       .ContinueWith(t => t.Result.GroupBy(i => i.Name).ToDictionary(g => g.Key, g => g.ToList()));
            }
            catch (Exception ex)
            {
                Logger.Error($"Error parsing items for type '{type}': {ex.Message}");
                return new Dictionary<string, List<Item>>();
            }
        }

        public static async Task<List<Item>> UpdateItemPrices(List<Item> items)
        {
            if (items == null || items.Count == 0)
            {
                Logger.Warn("No items provided for price update.");
                return new List<Item>();
            }

            if (DecorModule.DecorModuleInstance == null || DecorModule.DecorModuleInstance.Client == null)
            {
                Logger.Error("DecorModuleInstance or its HTTP client is not initialized.");
                return items;
            }

            var itemIds = items.Select(i => i.Id).Distinct().ToList();
            int batchSize = 200;
            var batches = itemIds.Batch(batchSize) ?? new List<IEnumerable<int>>();

            foreach (var batch in batches)
            {
                string ids = string.Join(",", batch);
                string priceApiUrl = $"https://api.guildwars2.com/v2/commerce/prices?ids={ids}";
                try
                {
                    string priceResponse = await RetryPolicyAsync(() => DecorModule.DecorModuleInstance.Client.GetStringAsync(priceApiUrl));

                    if (string.IsNullOrWhiteSpace(priceResponse))
                    {
                        Logger.Error($"Received an empty response from the price API for batch {ids}.");
                        continue;
                    }

                    var priceData = JsonSerializer.Deserialize<List<ItemPrice>>(priceResponse, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<ItemPrice>();

                    foreach (var item in items)
                    {
                        var priceInfo = priceData.FirstOrDefault(p => p.Id == item.Id);
                        if (priceInfo != null)
                        {
                            item.DefaultBuy = ((priceInfo.Buys?.Unit_Price ?? 0) * item.DefaultQty).ToString();
                            item.DefaultSell = ((priceInfo.Sells?.Unit_Price ?? 0) * item.DefaultQty).ToString();

                            item.TradeEfficiency1Buy = ((priceInfo.Buys?.Unit_Price ?? 0) * item.TradeEfficiency1Qty).ToString();
                            item.TradeEfficiency1Sell = ((priceInfo.Sells?.Unit_Price ?? 0) * item.TradeEfficiency1Qty).ToString();

                            item.TradeEfficiency2Buy = Math.Ceiling((priceInfo.Buys?.Unit_Price ?? 0) * item.TradeEfficiency2Qty).ToString();
                            item.TradeEfficiency2Sell = Math.Ceiling((priceInfo.Sells?.Unit_Price ?? 0) * item.TradeEfficiency2Qty).ToString();
                        }
                    }
                }
                catch (HttpRequestException ex)
                {
                    Logger.Error($"HTTP request error while fetching prices for batch {ids}: {ex.Message}");
                }
                catch (JsonException ex)
                {
                    Logger.Error($"JSON parsing error while processing price data for batch {ids}: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Logger.Error($"Unexpected error in UpdateItemPrices for batch {ids}: {ex.Message}");
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
                catch (Exception ex)
                {
                    Logger.Error($"Unexpected error: {ex.Message}");
                    break;
                }
            }
            throw new HttpRequestException("Failed after multiple retries.");
        }

        public static double ParseDoubleInvariant(string value, double defaultValue = 0.5)
        {
            if (string.IsNullOrWhiteSpace(value))
                return defaultValue;

            value = value.Trim();

            bool hasComma = value.Contains(",");
            bool hasDot = value.Contains(".");

            if (hasComma && hasDot)
            {
                int lastComma = value.LastIndexOf(",");
                int lastDot = value.LastIndexOf(".");

                if (lastDot > lastComma)
                {
                    value = value.Replace(",", "");
                }
                else
                {
                    value = value.Replace(".", "").Replace(",", ".");
                }
            }
            else if (hasComma)
            {
                value = value.Replace(",", ".");
            }

            if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double parsedValue))
            {
                return parsedValue;
            }

            return defaultValue;
        }
    }
}