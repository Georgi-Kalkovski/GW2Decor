using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace Gw2DecorBlishhudModule
{
    // Decoration class for JSON deserialization
    public class Decoration
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("icon")]
        public string IconUrl { get; set; }

        [JsonProperty("image")]
        public string Image { get; set; }

        [JsonProperty("categories")]
        public List<int> Categories { get; set; }

        public Decoration()
        {
            Categories = new List<int>();
        }

        public int IconAssetId => int.Parse(Path.GetFileNameWithoutExtension(new Uri(IconUrl).AbsolutePath));
    }
}
