using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Windows;
using System.Xml.Serialization;

namespace Spotlight.Models
{
    public class GameObject
    {
        public GameObject()
        {
            Sprites = new List<Sprite>();
            Properties = new List<string>();
        }

        public int GameId { get; set; }
        public string Name { get; set; }
        public List<Sprite> Sprites { get; set; }
        public List<string> Properties
        {
            get; set;
        }

        public class SpriteDefinition
        {
            public List<SpriteInfo> Sprites { get; private set; }
            public string ProxyId { get; private set; }
            public string InGameId { get; private set; }
            public string Width { get; private set; }
            public string Height { get; private set; }
            public string Name { get; private set; }
            public string Class { get; private set; }
            public string Group { get; private set; }
            public string MaxLeftX { get; private set; }
            public string MaxRightX { get; private set; }
            public string MaxTopY { get; private set; }
            public string MaxBottomY { get; private set; }
            public List<String> PropertyDescriptions { get; private set; }

            public SpriteDefinition()
            {
                Sprites = new List<SpriteInfo>();
                MaxBottomY = MaxLeftX = MaxRightX = MaxTopY = "0";
                PropertyDescriptions = new List<string>();
            }

            public bool LoadFromElement(XElement e)
            {
                foreach (XAttribute a in e.Attributes())
                {
                    switch (a.Name.LocalName.ToLower())
                    {
                        case "id":
                            InGameId = a.Value;
                            break;

                        case "width":
                            Width = a.Value;
                            break;

                        case "height":
                            Height = a.Value;
                            break;

                        case "name":
                            Name = a.Value;
                            break;

                        case "class":
                            Class = a.Value;
                            break;

                        case "group":
                            Group = a.Value;
                            break;

                        case "proxy":
                            ProxyId = a.Value;
                            break;
                    }
                }

                foreach (var x in e.Elements("sprite"))
                {
                    SpriteInfo s = new SpriteInfo();
                    s.LoadFromElement(x);
                    Sprites.Add(s);

                }

                foreach (var p in e.Elements("property"))
                {
                    PropertyDescriptions.Add(p.Value);
                }

                return true;
            }
        }

        public class SpriteInfo
        {
            public string X { get; set; }
            public string Y { get; set; }
            public string Value { get; set; }
            public string Palette { get; set; }
            public string HorizontalFlip { get; set; }
            public string VerticalFlip { get; set; }
            public string Table { get; set; }
            public string Name { get; private set; }
            public List<string> Property { get; private set; }

            #region IXmlIO Members


            public bool LoadFromElement(XElement e)
            {
                X = e.Attribute("x").Value;
                Y = e.Attribute("y").Value;
                Value = e.Attribute("value").Value;
                Palette = e.Attribute("palette").Value;
                HorizontalFlip = e.Attribute("horizontalflip").Value;
                VerticalFlip = e.Attribute("verticalflip").Value;
                if (e.Attribute("table").Value.Contains('-')) Table = e.Attribute("table").Value;
                else Table = e.Attribute("table").Value;
                if (e.Attribute("property") != null)
                {
                    Property = e.Attribute("property").Value.Split(',').Select(s => s).ToList();
                }

                return true;
            }

            #endregion
        }

        public enum SpriteRotation
        {
            HorizontalFlip,
            VerticalFlip,
            HorizontalAndVerticalFlip
        }
    }
}
