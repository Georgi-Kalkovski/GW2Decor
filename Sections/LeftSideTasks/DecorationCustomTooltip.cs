using Blish_HUD;
using Blish_HUD.Controls;
using DecorBlishhudModule.Model;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Color = Microsoft.Xna.Framework.Color;

namespace DecorBlishhudModule.Sections.LeftSideTasks
{
    public class DecorationCustomTooltip
    {
        private static readonly Dictionary<Decoration, Tooltip> decorationTooltips = new();
        public static async Task<Tooltip> CreateTooltipWithIconsAsync(Decoration decoration, Texture2D mainIcon)
        {
            // Fetch icons for ingredients
            var icon1 = await GetIconTextureAsync(decoration?.CraftingIngredientIcon1);
            var icon2 = await GetIconTextureAsync(decoration?.CraftingIngredientIcon2);
            var icon3 = await GetIconTextureAsync(decoration?.CraftingIngredientIcon3);
            var icon4 = await GetIconTextureAsync(decoration?.CraftingIngredientIcon4);

            // Now pass these icons to the tooltip creation method
            return await CustomTooltip(decoration, mainIcon, icon1, icon2, icon3, icon4);
        }

        public static async Task<Tooltip> CustomTooltip(Decoration decoration, Texture2D mainIcon, Texture2D icon1, Texture2D icon2, Texture2D icon3, Texture2D icon4)
        {
            if (decorationTooltips.TryGetValue(decoration, out var cachedTooltip))
            {
                return cachedTooltip;
            }

            // Create tooltip
            var customTooltip = new Tooltip
            {
                Width = 300 // Set width to avoid dynamic resizing
            };

            // Add main icon
            new Image
            {
                Parent = customTooltip,
                Texture = mainIcon,
                Size = new Point(40, 40),
            };

            // Add title (decoration.Name is the text we want to search for)
            var nameLabel = new Label
            {
                Parent = customTooltip,
                Text = decoration.Name,
                TextColor = Color.White,
                Font = GameService.Content.DefaultFont16,
                Location = new Point(50, 5),
                AutoSizeWidth = true
            };

            // Add book title
            new Label
            {
                Parent = customTooltip,
                Text = decoration.Book,
                TextColor = new Color(255, 164, 5),
                Font = GameService.Content.DefaultFont12,
                Location = new Point(50, 22),
                AutoSizeWidth = true
            };

            // Add recipe section with pre-fetched icons
            if (!string.IsNullOrEmpty(decoration.CraftingIngredientQty1))
            {
                AddIngredientSection(customTooltip, new[]
                {
            (decoration.CraftingIngredientQty1, icon1, decoration.CraftingIngredientName1),
            (decoration.CraftingIngredientQty2, icon2, decoration.CraftingIngredientName2),
            (decoration.CraftingIngredientQty3, icon3, decoration.CraftingIngredientName3),
            (decoration.CraftingIngredientQty4, icon4, decoration.CraftingIngredientName4)
        });
            }

            // Cache and return
            decorationTooltips[decoration] = customTooltip;

            // Return tooltip with the nameLabel reference for searching
            return customTooltip;
        }

        private static void AddIngredientSection(Tooltip parent, IEnumerable<(string qty, Texture2D icon, string name)> ingredients)
        {
            var yOffset = 50; // Starting Y offset for ingredients
            new Label
            {
                Parent = parent,
                Text = "Recipe:",
                TextColor = Color.White,
                Font = GameService.Content.DefaultFont16,
                Location = new Point(5, yOffset),
                AutoSizeWidth = true
            };

            foreach (var (qty, icon, name) in ingredients.Where(i => !string.IsNullOrEmpty(i.qty)))
            {
                yOffset += 30;

                // Quantity
                new Label
                {
                    Parent = parent,
                    Text = qty,
                    TextColor = Color.White,
                    Font = GameService.Content.DefaultFont16,
                    Location = new Point(5, yOffset),
                    AutoSizeWidth = true
                };

                // Icon
                new Image
                {
                    Parent = parent,
                    Texture = icon,
                    Size = new Point(25, 25),
                    Location = new Point(35, yOffset)
                };

                // Ingredient name
                new Label
                {
                    Parent = parent,
                    Text = name,
                    TextColor = Color.White,
                    Font = GameService.Content.DefaultFont14,
                    Location = new Point(70, yOffset),
                    AutoSizeWidth = true
                };
            }
        }
        private static async Task<Texture2D> GetIconTextureAsync(string iconUrl)
        {
            if (string.IsNullOrEmpty(iconUrl)) return null;
            var byteArray = await DecorModule.DecorModuleInstance.Client.GetByteArrayAsync(iconUrl);
            return LeftSideSection.CreateIconTexture(byteArray);
        }
    }
}
