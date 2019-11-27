using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Spotlight.Models
{
    public class LevelInfo
    {
        public string Name { get; set; }
        public Guid Id { get; set; }
        public int TileSet { get; set; }
        public DateTime LastModified { get; set; }
        public List<LevelInfo> SublevelsInfo { get; set; }
    }

    public class LegacyLevelInfo
    {
        [XmlAttribute]
        public string name { get; set; }

        [XmlAttribute]
        public string worldguid { get; set; }

        [XmlAttribute]
        public string levelguid { get; set; }

        [XmlAttribute]
        public string leveltype { get; set; }

        [XmlAttribute]
        public string lastmodified { get; set; }

        [XmlAttribute]
        public string bonusfor { get; set; }
    }
}
