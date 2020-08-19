using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Spotlight.Models
{
    public class World
    {
        public World()
        {
            TileData = new int[0x40 * 0x1B];
            Pointers = new List<WorldPointer>();
        }

        public Guid Id { get; set; }
        public int ClearTileIndex { get; set; }
        public int TileTableIndex { get; set; }
        public int MusicValue { get; set; }
        public int ScreenLength { get; set; }
        public Guid PaletteId { get; set; }
        public string Name { get; set; }
        public int[] TileData { get; set; }
        public List<WorldPointer> Pointers { get; set; }

        [JsonIgnore]

        public int AnimationTileTableIndex
        {
            get { return 0x70; }
        }
    }

    [XmlRoot("world")]
    public class LegacyWorld
    {
        [XmlAttribute]
        public string guid { get; set; }

        [XmlAttribute]
        public string clearvalue { get; set; }

        [XmlAttribute]
        public string graphicsbank { get; set; }

        [XmlAttribute]
        public string length { get; set; }

        [XmlAttribute]
        public string music { get; set; }

        [XmlAttribute]
        public string palette { get; set; }

        [XmlAttribute]
        public string worlddata { get; set; }

        [XmlArray("pointers")]
        [XmlArrayItem("pointer")]
        public List<LegacyWorldPointer> pointers { get; set; }

        [XmlIgnore]
        public string name { get; set; }
    }

}
