using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System;

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

            int currentYear = DateTime.Now.Year;

            var iconMapping = new Dictionary<Texture2D, List<string>>
            {
                // Lunar New Year events, including the current year variant
                { _homesteadIconMenuLunar, new List<string>
                    {
                        "id=\"Current_release:_“Lunar_New_Year”",
                        $"id=\"Current_release:_“Lunar_New_Year_{currentYear}”"
                    }
                },

                // Super Adventure Festival, no year variant but including it in case
                { _homesteadIconMenuSAB, new List<string>
                    {
                        "id=\"Current_release:_“Super_Adventure_Festival”",
                        $"id=\"Current_release:_“Super_Adventure_Festival_{currentYear}”"
                    }
                },

                // Dragon Bash events, same as above
                { _homesteadIconMenuDragonBash, new List<string>
                    {
                        "id=\"Current_release:_“Dragon_Bash”",
                        $"id=\"Current_release:_“Dragon_Bash_{currentYear}”"
                    }
                },

                // Festival of the Four Winds events
                { _homesteadIconMenuFOTFW, new List<string>
                    {
                        "id=\"Current_release:_“Festival_of_the_Four_Winds”",
                        $"id=\"Current_release:_“Festival_of_the_Four_Winds_{currentYear}”"
                    }
                },

                // Halloween events (with or without year)
                { _homesteadIconMenuHalloween, new List<string>
                    {
                        "id=\"Current_release:_“Halloween”",
                        "id=\"Current_release:_“Shadow_of_the_Mad_King”",
                        $"id=\"Current_release:_“Halloween_{currentYear}”",
                        $"id=\"Current_release:_“Shadow_of_the_Mad_King_{currentYear}”"
                    }
                },

                // Wintersday events, including the current year variant
                { wintersdayIcon, new List<string>
                    {
                        "id=\"Current_release:_“Wintersday”",
                        "id=\"Current_release:_“A_Very_Merry_Wintersday”",
                        $"id=\"Current_release:_“Wintersday_{currentYear}”",
                        $"id=\"Current_release:_“A_Very_Merry_Wintersday_{currentYear}”"
                    }
                },
            };

            try
            {
                DecorModule.DecorModuleInstance.Client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0");
                string jsonResponse = await DecorModule.DecorModuleInstance.Client.GetStringAsync(Url);

                // Parse the JSON response
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
