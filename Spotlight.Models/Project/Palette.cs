using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Spotlight.Models
{

    public class Palette
    {
        public Palette()
        {
            Colors = new List<int>();
        }
        public Guid Id { get; set; }
        public List<int> Colors { get; set; }
        public string Name { get; set; }


        [JsonIgnore]
        public int BackgroundColor
        {
            get
            {
                return Colors[0];
            }
        }
    }

    public class LegacyPalette
    {
        [XmlAttribute]
        public string background { get; set; }

        [XmlAttribute]
        public string data { get; set; }

        [XmlAttribute]
        public string name { get; set; }

        [XmlAttribute]
        public string guid { get; set; }
    }
}
