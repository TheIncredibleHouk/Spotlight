using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Spotlight.Models
{
    public class LevelPointer
    {
        public Guid LevelId { get; set; }
        public int ExitActionType { get; set; }
        public int ExitX { get; set; }
        public int ExitY { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public bool ExitsLevel { get; set; }
        public bool RedrawsLevel { get; set; }
        public bool KeepObjects { get; set; }
        public bool DisableWeather { get; set; }
    }

    public class LegacyLevelPointer
    {
        [XmlAttribute]
        public string levelguid { get; set; }

        [XmlAttribute]
        public string exittype { get; set; }

        [XmlAttribute]
        public string xexit { get; set; }

        [XmlAttribute]
        public string yexit { get; set; }

        [XmlAttribute]
        public string xenter { get; set; }

        [XmlAttribute]
        public string yenter { get; set; }

        [XmlAttribute]
        public string exitslevel { get; set; }

        [XmlAttribute]
        public string world { get; set; }

        [XmlAttribute]
        public string redraw { get; set; }

        [XmlAttribute]
        public string keepobjects { get; set; }

        [XmlAttribute]
        public string disableweather { get; set; }
    }
}
