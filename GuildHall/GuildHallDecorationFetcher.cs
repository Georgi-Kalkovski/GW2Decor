using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Text.RegularExpressions;
using DecorBlishhudModule.Homestead;
using DecorBlishhudModule.Model;

namespace DecorBlishhudModule
{
    public class GuildHallDecorationFetcher
    {
        public static async Task<Dictionary<string, List<Decoration>>> FetchDecorationsAsync(bool _isIconView)
        {
            // Dictionary to store decorations by category
            var decorationsByCategory = new Dictionary<string, List<Decoration>>();

            // Get the categories (assuming this is already defined)
            List<string> categories = GuildHallCategories.GetCategories();

            // Create a list of tasks for each category
            var fetchTasks = categories.Select(async category =>
            {
                string formattedCategoryName = category.Replace(" ", "_");
                string categoryUrl = $"https://wiki.guildwars2.com/api.php?action=parse&page=Decoration/Guild_hall/{formattedCategoryName}&format=json&prop=text";

                // Fetch decorations for this category
                var decorations = await FetchDecorationsForCategoryAsync(categoryUrl, _isIconView);
                return new KeyValuePair<string, List<Decoration>>(category, decorations);
            }).ToList();

            // Await all tasks
            var results = await Task.WhenAll(fetchTasks);

            // Populate the dictionary with results
            foreach (var result in results)
            {
                decorationsByCategory[result.Key] = result.Value;
            }

            return decorationsByCategory;
        }

        private static async Task<List<Decoration>> FetchDecorationsForCategoryAsync(string baseUrl, bool _isIconView)
        {
            var decorations = new List<Decoration>();

            // Combine sections 1 and 2
            string[] sections = { "1", "2" };
            string combinedHtmlContent = string.Empty;

            foreach (var section in sections)
            {
                // Fetch JSON data for each section
                var url = $"{baseUrl}&section={section}";
                var json = await DecorModule.DecorModuleInstance.Client.GetStringAsync(url);
                var parsedJson = JObject.Parse(json);

                // Extract HTML content for the section
                string htmlContent = parsedJson["parse"]?["text"]?["*"]?.ToString();
                if (!string.IsNullOrEmpty(htmlContent))
                {
                    combinedHtmlContent += htmlContent;
                }
            }

            var doc = new HtmlDocument();
            doc.LoadHtml(combinedHtmlContent);

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

                    if (imageUrl != null)
                    {
                        imageUrl = imageUrl.Replace("/images/thumb/", "/images/");
                        imageUrl = Regex.Replace(imageUrl, @"/\d+px-[^/]+$", "");
                    }

                    // Match with existing decoration based on name
                    var matchedDecoration = decorations.FirstOrDefault(d =>
                            d.Name.Trim().Equals(galleryName?.Trim(), StringComparison.OrdinalIgnoreCase));
                    if (matchedDecoration != null && imageUrl != null)
                    {
                        matchedDecoration.ImageUrl = imageUrl;
                    }
                }
            }
            return decorations;
        }
    }
}