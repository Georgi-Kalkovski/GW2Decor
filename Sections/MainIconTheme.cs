using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Microsoft.Xna.Framework.Graphics;

namespace DecorBlishhudModule
{
    public static class MainIconTheme
    {

        public static async Task<Texture2D> GetThemeIconAsync(Texture2D defaultIcon, Texture2D wintersdayIcon)
        {
            const string Url = "https://wiki.guildwars2.com/api.php?action=parse&page=Main_Page&format=json&prop=text";

            try
            {
                DecorModule.DecorModuleInstance.Client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0");
                string jsonResponse = await DecorModule.DecorModuleInstance.Client.GetStringAsync(Url);

                JObject parsedResponse = JObject.Parse(jsonResponse);

                string parsedText = parsedResponse["parse"]?["text"]?["*"]?.ToString() ?? string.Empty;
                if (parsedText.Contains("id=\"Current_release:_“Wintersday”"))
                {
                    return wintersdayIcon;
                }
            }
            catch { }

            return defaultIcon;
        }
    }
}
