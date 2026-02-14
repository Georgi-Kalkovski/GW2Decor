using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;

namespace DecorBlishhudModule
{
    public static class MainIconTheme
    {
        private const string WikiUrl =
            "https://wiki.guildwars2.com/api.php?action=parse&page=Main_Page&format=json&prop=text";

        public static async Task<Texture2D> GetThemeIconAsync(
            Texture2D defaultIcon,
            Texture2D lunarIcon,
            Texture2D sabIcon,
            Texture2D dragonBashIcon,
            Texture2D fotfwIcon,
            Texture2D halloweenIcon,
            Texture2D wintersdayIcon)
        {
            if (DecorModule.DecorModuleInstance?.Client == null)
                return defaultIcon;

            int year = DateTime.Now.Year;

            var eventMap = BuildEventMap(
                year,
                lunarIcon,
                sabIcon,
                dragonBashIcon,
                fotfwIcon,
                halloweenIcon,
                wintersdayIcon);

            try
            {
                DecorModule.DecorModuleInstance.Client.DefaultRequestHeaders.UserAgent
                    .ParseAdd("Mozilla/5.0");

                string response = await DecorModule.DecorModuleInstance.Client.GetStringAsync(WikiUrl);
                string htmlText = ExtractHtml(response);

                if (string.IsNullOrWhiteSpace(htmlText))
                    return defaultIcon;

                htmlText = Normalize(htmlText);

                foreach (var entry in eventMap)
                {
                    foreach (var marker in entry.Value)
                    {
                        if (htmlText.Contains(marker))
                            return entry.Key;
                    }
                }
            }
            catch (HttpRequestException)
            {
                // network failure → fallback icon
            }
            catch (Exception)
            {
                // parsing or unexpected failure → fallback icon
            }

            return defaultIcon;
        }

        private static Dictionary<Texture2D, List<string>> BuildEventMap(
            int year,
            Texture2D lunar,
            Texture2D sab,
            Texture2D dragonBash,
            Texture2D fotfw,
            Texture2D halloween,
            Texture2D wintersday)
        {
            return new Dictionary<Texture2D, List<string>>
{
    {
        lunar, new List<string>
        {
            $"current_release:lunar_new_year",
            $"current_release:lunar_new_year_{year}",
            $"current_release:_\"lunar_new_year\"",
            $"current_release:_\"lunar_new_year_{year}\""
        }
    },
    {
        sab, new List<string>
        {
            $"current_release:super_adventure_festival",
            $"current_release:super_adventure_festival_{year}",
            $"current_release:_\"super_adventure_festival\"",
            $"current_release:_\"super_adventure_festival_{year}\""
        }
    },
    {
        dragonBash, new List<string>
        {
            $"current_release:dragon_bash",
            $"current_release:dragon_bash_{year}",
            $"current_release:_\"dragon_bash\"",
            $"current_release:_\"dragon_bash_{year}\""
        }
    },
    {
        fotfw, new List<string>
        {
            $"current_release:festival_of_the_four_winds",
            $"current_release:festival_of_the_four_winds_{year}",
            $"current_release:_\"festival_of_the_four_winds\"",
            $"current_release:_\"festival_of_the_four_winds_{year}\""
        }
    },
    {
        halloween, new List<string>
        {
            $"current_release:halloween",
            $"current_release:shadow_of_the_mad_king",
            $"current_release:halloween_{year}",
            $"current_release:shadow_of_the_mad_king_{year}",
            $"current_release:_\"halloween\"",
            $"current_release:_\"shadow_of_the_mad_king\"",
            $"current_release:_\"halloween_{year}\"",
            $"current_release:_\"shadow_of_the_mad_king_{year}\""
        }
    },
    {
        wintersday, new List<string>
        {
            $"current_release:wintersday",
            $"current_release:a_very_merry_wintersday",
            $"current_release:wintersday_{year}",
            $"current_release:a_very_merry_wintersday_{year}",
            $"current_release:_\"wintersday\"",
            $"current_release:_\"a_very_merry_wintersday\"",
            $"current_release:_\"wintersday_{year}\"",
            $"current_release:_\"a_very_merry_wintersday_{year}\""
        }
    }
};

        }

        private static string ExtractHtml(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return string.Empty;

            JObject parsed = JObject.Parse(json);
            return parsed["parse"]?["text"]?["*"]?.ToString() ?? string.Empty;
        }

        private static string Normalize(string input)
        {
            return input
                .ToLowerInvariant()
                .Replace("“", "\"")
                .Replace("”", "\"")
                .Replace(" ", "_");
        }
    }
}
