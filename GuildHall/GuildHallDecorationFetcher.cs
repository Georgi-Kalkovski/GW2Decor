using DecorBlishhudModule.Homestead;
using DecorBlishhudModule.Model;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DecorBlishhudModule
{
    public class GuildHallDecorationFetcher
    {
        private static readonly Dictionary<string, string> IngredientIconCache = new();

        public static async Task<Dictionary<string, List<Decoration>>> FetchDecorationsAsync()
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
                var decorations = await FetchDecorationsForCategoryAsync(categoryUrl);
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

        private static async Task<List<Decoration>> FetchDecorationsForCategoryAsync(string baseUrl)
        {
            var decorations = new List<Decoration>();

            string combinedHtmlContent = string.Empty;

            var json = await DecorModule.DecorModuleInstance.Client.GetStringAsync(baseUrl);
            var parsedJson = JObject.Parse(json);

            // Extract HTML content for the section
            string htmlContent = parsedJson["parse"]?["text"]?["*"]?.ToString();
            if (!string.IsNullOrEmpty(htmlContent))
            {
                combinedHtmlContent += htmlContent;
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
            else if (baseUrl == "https://wiki.guildwars2.com/api.php?action=parse&page=Decoration/Guild_hall/Monuments&format=json&prop=text")
            {
                var div1 = doc.DocumentNode.SelectSingleNode("//h2/span[@id='List_of_monument_decorations']/following::div[1]//ul");
                var div2 = doc.DocumentNode.SelectSingleNode("//h2/span[@id='List_of_monument_decorations']/following::div[2]//ul");

                var combinedListItems = new List<HtmlNode>();
                if (div1 != null)
                {
                    combinedListItems.AddRange(div1.SelectNodes(".//li"));
                }
                if (div2 != null)
                {
                    combinedListItems.AddRange(div2.SelectNodes(".//li"));
                }

                foreach (var item in combinedListItems)
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

            var recipesNode = doc.DocumentNode.SelectNodes("//table[@class='recipe sortable table']//tr[position()>1]");
            if (recipesNode != null)
            {
                foreach (var item in recipesNode)
                {
                    var cells = item.SelectNodes("td");
                    if (cells != null && cells.Count >= 5)
                    {
                        var recipeName = cells[0].InnerText.Trim().Split(new string[] { "  " }, StringSplitOptions.None)[0];
                        var matchedDecoration = decorations.FirstOrDefault(d =>
                        d.Name.Trim().Equals(recipeName, StringComparison.OrdinalIgnoreCase));
                        if (matchedDecoration != null)
                        {
                            matchedDecoration.Book = cells[0].InnerText.Trim().Split(new string[] { "  " }, StringSplitOptions.None).Length > 1
                                ? cells[0].InnerText.Trim().Split(new string[] { "  " }, StringSplitOptions.None)
                                    .ElementAtOrDefault(1)?
                                    .Replace("(Learned from: ", "")
                                    .Replace(")", "")
                                    .Trim()
                                : null;

                            matchedDecoration.CraftingRating = cells[3]?.InnerText.Trim();

                            var ingredientsNode = cells[4].SelectSingleNode(".//dl");
                            if (ingredientsNode != null)
                            {
                                ParseIngredients(ingredientsNode, matchedDecoration);
                            }
                        }
                    }
                }
            }
            return decorations;
        }

        private static void ParseIngredients(HtmlNode ingredientsNode, Decoration decoration)
        {
            var dtNodes = ingredientsNode.SelectNodes(".//dt");
            var ddNodes = ingredientsNode.SelectNodes(".//dd");

            if (dtNodes == null || ddNodes == null || dtNodes.Count != ddNodes.Count)
                return;

            for (int i = 0; i < dtNodes.Count; i++)
            {
                var ingredientQuantity = dtNodes[i]?.InnerText.Trim();
                var ingredientName = ddNodes[i]?.InnerText.Trim();

                var ingredientIconNode = ddNodes[i]?.SelectSingleNode(".//img");
                string ingredientIconUrl = null;

                if (ingredientIconNode != null)
                {
                    string rawIconUrl = "https://wiki.guildwars2.com" + ingredientIconNode.GetAttributeValue("src", "").Trim();
                    if (!IngredientIconCache.TryGetValue(ingredientName, out ingredientIconUrl))
                    {
                        ingredientIconUrl = rawIconUrl;
                        IngredientIconCache[ingredientName] = ingredientIconUrl; // Cache the icon URL
                    }
                }

                switch (i + 1)
                {
                    case 1:
                        decoration.CraftingIngredientName1 = ingredientName;
                        decoration.CraftingIngredientIcon1 = ingredientIconUrl;
                        decoration.CraftingIngredientQty1 = ingredientQuantity;
                        break;
                    case 2:
                        decoration.CraftingIngredientName2 = ingredientName;
                        decoration.CraftingIngredientIcon2 = ingredientIconUrl;
                        decoration.CraftingIngredientQty2 = ingredientQuantity;
                        break;
                    case 3:
                        decoration.CraftingIngredientName3 = ingredientName;
                        decoration.CraftingIngredientIcon3 = ingredientIconUrl;
                        decoration.CraftingIngredientQty3 = ingredientQuantity;
                        break;
                    case 4:
                        decoration.CraftingIngredientName4 = ingredientName;
                        decoration.CraftingIngredientIcon4 = ingredientIconUrl;
                        decoration.CraftingIngredientQty4 = ingredientQuantity;
                        break;
                }
            }
        }
    }
}