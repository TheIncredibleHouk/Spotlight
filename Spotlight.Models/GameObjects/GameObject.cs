using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Spotlight.Models
{
    public class GameObject
    {
        public GameObject()
        {
            Sprites = new List<Sprite>();
            Properties = new List<string>();
        }

        [JsonIgnore]
        public bool IsStartObject { get; set; }

        public int GameId { get; set; }
        public GameObjectType GameObjectType { get; set; }
        public string Name { get; set; }
        public List<Sprite> Sprites { get; set; }
        public List<string> Properties { get; set; }
        public string Group { get; set; }
    }

    public enum GameObjectType
    {
        Global = 1,
        TypeA = 2,
        TypeB = 3
    }

    public enum SpriteRotation
    {
        HorizontalFlip,
        VerticalFlip,
        HorizontalAndVerticalFlip
    }
}