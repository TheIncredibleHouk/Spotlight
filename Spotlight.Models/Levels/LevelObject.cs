using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace Spotlight.Models
{
    public class LevelObject
    {
        public int GameObjectId { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Property { get; set; }

        [JsonIgnore]
        public GameObject GameObject { get; set; }

        public void CalcBoundBox()
        {
            int minX = 1000, minY = 10000;
            int maxX = 0, maxY = 0;

            List<Sprite> visibleSprites = GameObject.Sprites.Where(s => s.PropertiesAppliedTo == null || s.PropertiesAppliedTo.Count == 0 ? true : s.PropertiesAppliedTo.Contains(Property) && !s.Overlay).ToList();

            if (visibleSprites.Count == 0)
            {
                minY = minX = 0;
            }

            foreach (var sprite in visibleSprites)
            {
                minX = sprite.X < minX ? sprite.X : minX;
                maxX = sprite.X + 8 > maxX ? sprite.X + 8 : maxX;
                minY = sprite.Y < minY ? sprite.Y : minY;
                maxY = sprite.Y + 16 > maxY ? sprite.Y + 16 : maxY;
            }

            BoundRectangle = new Rect(X * 16 + minX, Y * 16 + minY, maxX - minX, maxY - minY);
        }

        [JsonIgnore]
        public Rect BoundRectangle { get; private set; }
    }

    public class LegacyLevelObject
    {
        [XmlAttribute]
        public string x { get; set; }

        [XmlAttribute]
        public string y { get; set; }
        [XmlAttribute]
        public string value { get; set; }
        [XmlAttribute]
        public string property { get; set; }
    }
}
