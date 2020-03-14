using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Spotlight.Models
{
    public class Sprite
    {
        public List<int> PropertiesAppliedTo { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public string TileTableAddress { get; set; }

        [JsonIgnore]
        public int TileTableIndex
        {
            get
            {
                return Int32.Parse(TileTableAddress.Substring(2), System.Globalization.NumberStyles.HexNumber) / 0x400;
            }
            set
            {
                TileTableAddress = "0x" + value.ToString("X2");
            }
        }

        public string TileValue { get; set; }

        [JsonIgnore]
        public int TileValueIndex
        {
            get
            {
                return Int32.Parse(TileValue.Substring(2), System.Globalization.NumberStyles.HexNumber);
            }
            set
            {
                TileValue = "0x" + value.ToString("X2");
            }
        }
        public bool HorizontalFlip { get; set; }
        public bool VerticalFlip { get; set; }
        public int PaletteIndex { get; set; }
        public bool Overlay { get; set; }
    }

    public class LegacySprite
    {
        [XmlAttribute]
        public string property { get; set; }

        [XmlAttribute]
        public string x { get; set; }

        [XmlAttribute]
        public string y { get; set; }

        [XmlAttribute]
        public string value { get; set; }

        [XmlAttribute]
        public string palette { get; set; }

        [XmlAttribute]
        public string horizontalflip { get; set; }

        [XmlAttribute]
        public string verticalflip { get; set; }

        [XmlAttribute]
        public string table { get; set; }
    }
}
