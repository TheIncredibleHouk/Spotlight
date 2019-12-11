using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Spotlight.Models
{
    public class WorldPointer
    {
        public Guid LevelId { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
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
