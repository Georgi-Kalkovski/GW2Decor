using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace Gw2DecorBlishhudModule
{
    // Decoration class for JSON deserialization
    public class Decoration
    {
        public string Name { get; set; }

        public string IconUrl { get; set; }

        public string ImageUrl { get; set; }

    }
}