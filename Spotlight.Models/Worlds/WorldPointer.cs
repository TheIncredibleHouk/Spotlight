using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace Spotlight.Models
{
    public class WorldPointer
    {
        public Guid LevelId { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        public Rect BoundRectangle
        {
            get
            {
                return new Rect()
                {
                    X = X * 16,
                    Y = Y * 16,
                    Width = 16,
                    Height = 16
                };
            }
        }


        private static List<Sprite> _overlay = new List<Sprite>()
        {
            new Sprite(){ X = 0, Y = 0, Overlay = true, PaletteIndex = 1, TileValueIndex = 0x56, TileTableIndex = 4 },
            new Sprite(){ X = 8, Y = 0, Overlay = true, PaletteIndex = 1, TileValueIndex = 0x58, TileTableIndex = 4 }
        };

        public static List<Sprite> Overlay
        {
            get
            {
                return _overlay;
            }
        }
    }

    public class LegacyWorldPointer
    {
        [XmlAttribute]
        public string levelguid { get; set; }

        [XmlAttribute]
        public string x { get; set; }

        [XmlAttribute]
        public string y { get; set; }

    }
}
