using Newtonsoft.Json;
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
        [JsonIgnore]
        public IInfo ParentInfo { get; set; }
        public InfoType InfoType { get { return InfoType.Level; } }

        public string DisplayName
        {
            get
            {
                return "(Level) " + Name;
            }
        }
    }
}