using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Spotlight.Models
{
    public class WorldInfo : IInfo
    {
        public int Number { get; set; }
        public string Name { get; set; }
        public Guid Id { get; set; }
        public DateTime LastModified { get; set; }
        public List<LevelInfo> LevelsInfo { get; set; }

        public InfoType InfoType
        {
            get { return InfoType.World; }
        }

        public string DisplayName
        {
            get
            {
                return "(World) " + Name;
            }
        }
    }

    public class LegacyWorldInfo
    {
        [XmlAttribute]
        public string name { get; set; }

        [XmlAttribute]
        public string ordinal { get; set; }

        [XmlAttribute]
        public string worldguid { get; set; }

        [XmlAttribute]
        public string lastmodified { get; set; }

        [XmlAttribute]
        public string isnoworld { get; set; }
    }
}