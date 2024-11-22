using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Text.RegularExpressions;
using Blish_HUD;
using System.IO;
using Microsoft.Xna.Framework.Graphics;

namespace DecorBlishhudModule
{
    public class HomesteadDecorationFetcher
    {
        private static readonly HttpClient client = new HttpClient();
        private static readonly Dictionary<string, Texture2D> IconCache = new();  // Cache for icons

        public static async Task<Dictionary<string, List<Decoration>>> FetchDecorationsAsync(string url)
        {
            var decorationsByCategory = new Dictionary<string, List<Decoration>>();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0");

            // Fetch JSON data
            var json = await client.GetStringAsync(url);
            var parsedJson = JObject.Parse(json);

            // Extract HTML content
            string htmlContent = parsedJson["parse"]?["text"]?["*"]?.ToString();
            if (string.IsNullOrEmpty(htmlContent))
                return decorationsByCategory;

            var doc = new HtmlDocument();
            doc.LoadHtml(htmlContent);

            // Find all decorations with class "filter-plain"
            var decorationNodes = doc.DocumentNode.SelectNodes("//div[contains(@class, 'filter-plain')]");
            if (decorationNodes == null)
                return decorationsByCategory;

            // Iterate through each decoration node
            foreach (var node in decorationNodes)
            {
                var decoration = new Decoration();

                // Extract name
                decoration.Name = node?.InnerText
                   .Contains(".png") == true
                       ? node.InnerText
                           .Replace("&#160;", "")
                           .Replace("\n\n", "")
                           .Split(new[] { ".png" }, StringSplitOptions.None)
                           .ElementAtOrDefault(1) ?? "Unknown"
                       : node.InnerText
                           .Replace("\n&#160;", "")
                           .Replace("\n\n", "") ?? "Unknown";

                // Extract icon URL
                var headingDiv = node.SelectSingleNode(".//div[contains(@class, 'heading')]");
                var iconNode = headingDiv.SelectSingleNode(".//img");
                if (iconNode != null)
                {
                    decoration.IconUrl = "https://wiki.guildwars2.com" + iconNode.GetAttributeValue("src", "").Trim();
                }
                else
                {
                    decoration.IconUrl = "https://wiki.guildwars2.com/images/7/74/Skill.png"; // Default
                }

                var wrapperDiv = node.SelectSingleNode(".//div[contains(@class, 'wrapper')]");
                if (wrapperDiv != null)
                {
                    var wrapperImgNode = wrapperDiv.SelectSingleNode(".//img");
                    if (wrapperImgNode != null)
                    {
                        var imageUrl = "https://wiki.guildwars2.com" + wrapperImgNode.GetAttributeValue("src", "").Trim();
                        imageUrl = imageUrl.Replace("/images/thumb/", "/images/");
                        imageUrl = Regex.Replace(imageUrl, @"/\d+px-[^/]+$", "");
                        decoration.ImageUrl = imageUrl;
                    }
                }
                else
                {
                    decoration.ImageUrl = decoration.IconUrl; // Fallback to icon URL
                }

                // Assign to categories based on classes
                var classList = node.GetAttributeValue("class", "").Split(' ');
                foreach (var cls in classList)
                {
                    if (cls.StartsWith("f-", StringComparison.OrdinalIgnoreCase))
                    {
                        var category = cls.Substring(2)
                            .Replace("-", " ")
                            .Replace("table and seating", "table, seating, etc.")
                            .Replace("flags and signs", "flag, signs, markers, etc.")
                            .Trim();
                        if (!decorationsByCategory.ContainsKey(category))
                        {
                            decorationsByCategory[category] = new List<Decoration>();
                        }
                        decorationsByCategory[category].Add(decoration);
                    }
                }
            }

            return decorationsByCategory;
        }

        // Fetch icon and use cache
        public static async Task<Texture2D> GetIconTextureAsync(string iconUrl)
        {
            if (IconCache.ContainsKey(iconUrl))
            {
                return IconCache[iconUrl];
            }

            var iconResponse = await client.GetByteArrayAsync(iconUrl);
            using var memoryStream = new MemoryStream(iconResponse);
            using var graphicsContext = GameService.Graphics.LendGraphicsDeviceContext();
            var iconTexture = Texture2D.FromStream(graphicsContext.GraphicsDevice, memoryStream);

            // Cache the texture for future use
            IconCache[iconUrl] = iconTexture;

            return iconTexture;
        }
    }
}
