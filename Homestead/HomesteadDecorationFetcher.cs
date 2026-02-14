using DecorBlishhudModule.Homestead;
using DecorBlishhudModule.Model;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Net.Http;

namespace DecorBlishhudModule
{
    public class HomesteadDecorationFetcher
    {
        private static readonly Dictionary<string, string> IngredientIconCache = new();

        public static async Task<Dictionary<string, List<Decoration>>> FetchDecorationsAsync()
        {
            var decorationsByCategory = new Dictionary<string, List<Decoration>>();
            List<string> categories = HomesteadCategories.GetCategories();

            foreach (var category in categories)
            {
                string formattedCategoryName = category.Replace(" ", "_");

                string categoryUrl =
                    "https://wiki.guildwars2.com/api.php" +
                    "?action=parse" +
                    $"&page=Decoration/Homestead/{formattedCategoryName}" +
                    "&prop=text" +
                    "&format=json" +
                    "&origin=*";

                var decorations = await FetchDecorationsForCategoryAsync(categoryUrl);
                decorationsByCategory[category] = decorations;

                // Prevent 403 / rate limiting
                await Task.Delay(300);
            }

            return decorationsByCategory;
        }

        private static async Task<List<Decoration>> FetchDecorationsForCategoryAsync(string url)
        {
            var decorations = new List<Decoration>();

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Accept.ParseAdd("application/json");

            using var response = await DecorModule.DecorModuleInstance.Client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var parsedJson = JObject.Parse(json);

            string htmlContent = parsedJson["parse"]?["text"]?["*"]?.ToString();
            if (string.IsNullOrWhiteSpace(htmlContent))
                return decorations;

            var doc = new HtmlDocument();
            doc.LoadHtml(htmlContent);

            var listNode = doc.DocumentNode.SelectSingleNode(
                "//div[contains(@class,'smw-ul-columns')]"
            );

            if (listNode != null)
            {
                var listItems = listNode.SelectNodes(".//li[contains(@class,'smw-row')]");
                if (listItems != null)
                {
                    foreach (var item in listItems)
                    {
                        var decoration = new Decoration();

                        decoration.Name = item.InnerText
                            .Replace("\u00A0", " ")
                            .Replace("&nbsp;", "")
                            .Trim();

                        var iconNode = item.SelectSingleNode(".//img");
                        decoration.IconUrl = iconNode != null
                            ? "https://wiki.guildwars2.com" + iconNode.GetAttributeValue("src", "").Trim()
                            : "https://wiki.guildwars2.com/images/7/74/Skill.png";

                        decorations.Add(decoration);
                    }
                }
            }

            // Gallery images
            var galleryNode = doc.DocumentNode.SelectSingleNode("//div[contains(@class,'srf-gallery')]");
            if (galleryNode != null)
            {
                var galleryItems = galleryNode.SelectNodes(".//li[contains(@class,'gallerybox')]");
                if (galleryItems != null)
                {
                    foreach (var galleryItem in galleryItems)
                    {
                        var nameNode = galleryItem.SelectSingleNode(".//div[@class='gallerytext']//a");
                        var imgNode = galleryItem.SelectSingleNode(".//img");

                        if (nameNode == null || imgNode == null)
                            continue;

                        string galleryName = nameNode.InnerText.Trim();

                        string imageUrl = "https://wiki.guildwars2.com" +
                            imgNode.GetAttributeValue("src", "")
                                .Replace("/images/thumb/", "/images/");

                        imageUrl = Regex.Replace(imageUrl, @"/\d+px-[^/]+$", "");

                        var matchedDecoration = decorations.FirstOrDefault(d =>
                            d.Name.Equals(galleryName, StringComparison.OrdinalIgnoreCase));

                        if (matchedDecoration != null)
                            matchedDecoration.ImageUrl = imageUrl;
                    }
                }
            }

            // Recipes
            var recipesNode = doc.DocumentNode.SelectNodes(
                "//table[contains(@class,'recipe')]//tr[position()>1]"
            );

            if (recipesNode != null)
            {
                foreach (var item in recipesNode)
                {
                    var cells = item.SelectNodes("td");
                    if (cells == null || cells.Count < 5)
                        continue;

                    var recipeName = cells[0].InnerText
                        .Split(new[] { "  " }, StringSplitOptions.None)[0]
                        .Trim();

                    var matchedDecoration = decorations.FirstOrDefault(d =>
                        d.Name.Equals(recipeName, StringComparison.OrdinalIgnoreCase));

                    if (matchedDecoration == null)
                        continue;

                    matchedDecoration.Book = cells[0].InnerText.Split(new[] { "  " }, StringSplitOptions.None).Length > 1
                        ? cells[0].InnerText.Split(new[] { "  " }, StringSplitOptions.None)[1]
                            .Replace("(Learned from: ", "")
                            .Replace(")", "")
                            .Trim()
                        : null;

                    matchedDecoration.CraftingRating = cells[3]?.InnerText.Trim();

                    var ingredientsNode = cells[4].SelectSingleNode(".//dl");
                    if (ingredientsNode != null)
                        ParseIngredients(ingredientsNode, matchedDecoration);
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
                    string rawIconUrl =
                        "https://wiki.guildwars2.com" +
                        ingredientIconNode.GetAttributeValue("src", "").Trim();

                    if (!IngredientIconCache.TryGetValue(ingredientName, out ingredientIconUrl))
                    {
                        ingredientIconUrl = rawIconUrl;
                        IngredientIconCache[ingredientName] = ingredientIconUrl;
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
