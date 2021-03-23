using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
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

        public Rect CalcBoundBox()
        {
            int minX = 1000, minY = 10000;
            int maxX = 0, maxY = 0;

            List<Sprite> visibleSprites = GameObject.Sprites.Where(s => s.PropertiesAppliedTo == null || s.PropertiesAppliedTo.Count == 0 ? true : s.PropertiesAppliedTo.Contains(Property)).Where(s => !s.Overlay).ToList();

            if (visibleSprites.Count == 0)
            {
                minY = minX = 0;
                maxX = maxY = 16;
            }
            else
            {
                foreach (var sprite in visibleSprites)
                {
                    minX = sprite.X < minX ? sprite.X : minX;
                    maxX = sprite.X + 8 > maxX ? sprite.X + 8 : maxX;
                    minY = sprite.Y < minY ? sprite.Y : minY;
                    maxY = sprite.Y + 16 > maxY ? sprite.Y + 16 : maxY;
                }
            }

            BoundRectangle = new Rect(X * 16 + minX, Y * 16 + minY, maxX - minX, maxY - minY);
            return BoundRectangle;
        }

        public Rect CalcVisualBox(bool withOverlays)
        {
            int minX = 1000, minY = 10000;
            int maxX = 0, maxY = 0;

            List<Sprite> visibleSprites = GameObject.Sprites.Where(s => s.PropertiesAppliedTo == null || s.PropertiesAppliedTo.Count == 0 ? true : s.PropertiesAppliedTo.Contains(Property)).Where(s => (withOverlays ? true : !s.Overlay)).ToList();

            if (visibleSprites.Count == 0)
            {
                minY = minX = 0;
                maxX = maxY = 16;
            }
            else
            {
                foreach (var sprite in visibleSprites)
                {
                    minX = sprite.X < minX ? sprite.X : minX;
                    maxX = sprite.X + 8 > maxX ? sprite.X + 8 : maxX;
                    minY = sprite.Y < minY ? sprite.Y : minY;
                    maxY = sprite.Y + 16 > maxY ? sprite.Y + 16 : maxY;
                }
            }

            if (minX % 16 != 0)
            {
                if (minX < 0)
                {
                    minX = ((minX / 16) - 1) * 16;
                }
                else
                {
                    minX = (minX / 16) * 16;
                }
            }

            if (minY % 16 != 0)
            {
                if (minY < 0)
                {
                    minY = ((minY / 16) - 1) * 16;
                }
                else
                {
                    minY = (minY / 16) * 16;
                }
            }

            if (maxX % 16 != 0)
            {
                maxX = ((maxX / 16) + 1) * 16;
            }

            if (maxY % 16 != 0)
            {
                maxY = ((maxY / 16) + 1) * 16;
            }

            VisualRectangle = new Rect(X * 16 + minX, Y * 16 + minY, maxX - minX, maxY - minY);
            return VisualRectangle;
        }

        [JsonIgnore]
        public Rect BoundRectangle { get; private set; }

        [JsonIgnore]
        public Rect VisualRectangle { get; private set; }
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