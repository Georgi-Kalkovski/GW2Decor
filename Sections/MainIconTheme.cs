using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace DecorBlishhudModule
{
    public static class MainIconTheme
    {
        public static async Task<Texture2D> GetThemeIconAsync(
            Texture2D defaultIcon,
            Texture2D _homesteadIconMenuLunar,
            Texture2D _homesteadIconMenuSAB,
            Texture2D _homesteadIconMenuDragonBash,
            Texture2D _homesteadIconMenuFOTFW,
            Texture2D _homesteadIconMenuHalloween,
            Texture2D wintersdayIcon
            )
        {
            const string Url = "https://wiki.guildwars2.com/api.php?action=parse&page=Main_Page&format=json&prop=text";

            var iconMapping = new Dictionary<Texture2D, string[]>
            {
                { _homesteadIconMenuLunar, new[] { "id=\"Current_release:_“Lunar_New_Year”" } },
                { _homesteadIconMenuSAB, new[] { "id=\"Current_release:_“Super_Adventure_Festival”" } },
                { _homesteadIconMenuDragonBash, new[] { "id=\"Current_release:_“Dragon_Bash”" } },
                { _homesteadIconMenuFOTFW, new[] { "id=\"Current_release:_“Festival_of_the_Four_Winds”" } },
                { _homesteadIconMenuHalloween, new[] { "id=\"Current_release:_“Halloween”", "id=\"Current_release:_“Shadow_of_the_Mad_King”" } },
                { wintersdayIcon, new[] { "id=\"Current_release:_“Wintersday”", "id=\"Current_release:_“A_Very_Merry_Wintersday”" } },
            };

            try
            {
                DecorModule.DecorModuleInstance.Client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0");
                string jsonResponse = await DecorModule.DecorModuleInstance.Client.GetStringAsync(Url);

                JObject parsedResponse = JObject.Parse(jsonResponse);
                string parsedText = parsedResponse["parse"]?["text"]?["*"]?.ToString() ?? string.Empty;

                foreach (var entry in iconMapping)
                {
                    foreach (var keyword in entry.Value)
                    {
                        if (parsedText.Contains(keyword))
                        {
                            return entry.Key;
                        }
                    }
                }
            }
            catch { }

            return defaultIcon;
        }
    }
}
