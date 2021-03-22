using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Spotlight.Models
{
    public class LevelInfo : IInfo
    {
        public string Name { get; set; }
        public Guid Id { get; set; }
        public DateTime LastModified { get; set; }
        public List<LevelInfo> SublevelsInfo { get; set; }
        public InfoType InfoType { get { return InfoType.Level; } }

        public string DisplayName
        {
            get
            {
                return "(Level) " + Name;
            }
        }
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