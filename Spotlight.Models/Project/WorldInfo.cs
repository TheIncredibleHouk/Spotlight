using Newtonsoft.Json;
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
        [JsonIgnore]
        public List<LevelInfo> SublevelsInfo
        {
            get
            {
                return LevelsInfo;
            }
            set
            {
                LevelsInfo = value;
            }
        }
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
}