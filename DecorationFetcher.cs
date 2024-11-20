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
    public class DecorationFetcher
    {
        private static readonly HttpClient client = new HttpClient();
        private static readonly Dictionary<string, Texture2D> IconCache = new();  // Cache for icons

        public static async Task<List<Decoration>> FetchDecorationsAsync(string url)
        {
            var decorations = new List<Decoration>();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0");

            // Fetch JSON data
            var json = await client.GetStringAsync(url);
            var parsedJson = JObject.Parse(json);

            // Extract HTML content
            string htmlContent = parsedJson["parse"]?["text"]?["*"]?.ToString();
            if (string.IsNullOrEmpty(htmlContent))
                return decorations;

            var doc = new HtmlDocument();
            doc.LoadHtml(htmlContent);

            // Find the decorations list container more generally
            var listNode = doc.DocumentNode.SelectSingleNode("ancestor::h2/following-sibling::div[@class='smw-ul-columns'] | //div[contains(@class, 'smw-ul-columns')]");
            if (listNode != null)
            {
                var listItems = listNode.SelectNodes(".//li[@class='smw-row']");
                foreach (var item in listItems)
                {
                    var decoration = new Decoration();

                    // Extract name using text between '>' and '</a>'
                    var nameNode = item.InnerText;
                    if (nameNode != null)
                    {
                        decoration.Name = nameNode.Trim().Replace("\u00A0", " ").Replace("&nbsp;", "").Trim();
                    }

                    // Extract icon URL
                    var iconNode = item.SelectSingleNode(".//img");
                    if (iconNode != null)
                    {
                        decoration.IconUrl = "https://wiki.guildwars2.com" + iconNode.GetAttributeValue("src", "").Trim();
                    }
                    else
                    {
                        // Use default icon URL if iconNode is null
                        decoration.IconUrl = "https://wiki.guildwars2.com/images/7/74/Skill.png";
                        var nameParts = nameNode?.Split(new[] { ".png" }, StringSplitOptions.None);
                        if (nameParts != null && nameParts.Length > 1)
                        {
                            decoration.Name = nameParts[1].Trim();
                        }
                    }
                    // Add to list
                    decorations.Add(decoration);
                }
            }

            // Parse the "Gallery" section for images
            var galleryNode = doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'srf-gallery')]");
            if (galleryNode != null)
            {
                var galleryItems = galleryNode.SelectNodes(".//li[@class='gallerybox']");
                foreach (var galleryItem in galleryItems)
                {
                    var nameNode = galleryItem.SelectSingleNode(".//div[@class='gallerytext']//a");
                    string galleryName = nameNode?.InnerText.Trim();

                    var imgNode = galleryItem.SelectSingleNode(".//img");
                    string imageUrl = imgNode != null ? "https://wiki.guildwars2.com" + imgNode.GetAttributeValue("src", "").Trim() : null;

                    // Modify image URL to use 200px version
                    if (imageUrl != null)
                    {
                        imageUrl = imageUrl.Replace("/images/thumb/", "/images/");
                        imageUrl = Regex.Replace(imageUrl, @"/\d+px-[^/]+$", "");
                    }

                    // Match with existing decoration based on name
                    var matchedDecoration = decorations.FirstOrDefault(d => d.Name == galleryName);
                    if (matchedDecoration != null && imageUrl != null)
                    {
                        matchedDecoration.ImageUrl = imageUrl;
                    }
                }
            }

            return decorations;
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
