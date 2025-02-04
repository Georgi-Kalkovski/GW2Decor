namespace DecorBlishhudModule.Model
{
    // Decoration class for JSON deserialization
    public class Decoration
    {
        public string Name { get; set; }
        public string IconUrl { get; set; }
        public string ImageUrl { get; set; }
        public string Book { get; set; }
        public string CraftingRating { get; set; }

        // Crafting Ingredient 1
        public string CraftingIngredientName1 { get; set; }
        public string CraftingIngredientQty1 { get; set; }
        public string CraftingIngredientIcon1 { get; set; }

        // Crafting Ingredient 2
        public string CraftingIngredientName2 { get; set; }
        public string CraftingIngredientQty2 { get; set; }
        public string CraftingIngredientIcon2 { get; set; }

        // Crafting Ingredient 3
        public string CraftingIngredientName3 { get; set; }
        public string CraftingIngredientQty3 { get; set; }
        public string CraftingIngredientIcon3 { get; set; }

        // Crafting Ingredient 4
        public string CraftingIngredientName4 { get; set; }
        public string CraftingIngredientQty4 { get; set; }
        public string CraftingIngredientIcon4 { get; set; }
    }
}