using Spotlight.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Spotlight.Services
{
    public class GameObjectService
    {
        private readonly ErrorService _errorService;

        public GameObjectService(ErrorService errorService)
        {
            _errorService = errorService;
        }

        public List<GameObjectClass> ConvertFromLegacy(string fileName)
        {
            Dictionary<string, GameObjectClass> classTable = new Dictionary<string, GameObjectClass>();
            classTable.Add("1", new GameObjectClass() { Number = 1 });
            classTable.Add("2", new GameObjectClass() { Number = 2 });
            classTable.Add("3", new GameObjectClass() { Number = 3 });

            XElement root = XDocument.Load(fileName).Element("sprites");
            foreach (var x in root.Elements("spritedefinition"))
            {
                SpriteDefinition sp = new SpriteDefinition();
                sp.LoadFromElement(x);
                try
                {
                    GameObject gameObject = new GameObject()
                    {
                        GameId = Int32.Parse(sp.InGameId, System.Globalization.NumberStyles.HexNumber),
                        Name = sp.Name,
                        Properties = sp.PropertyDescriptions,
                        Sprites = sp.Sprites.Select(s => new Sprite()
                        {
                            PropertiesAppliesTo = s.Property.Select(p => int.Parse(p)).ToList(),
                            HorizontalFlip = bool.Parse(s.HorizontalFlip),
                            VerticalFlip = bool.Parse(s.VerticalFlip),
                            TileTableIndex = s.Table == "-1" ? -1 : Int32.Parse(s.Table, System.Globalization.NumberStyles.HexNumber),
                            TileValue = Int32.Parse(s.Value, System.Globalization.NumberStyles.HexNumber),
                            X = Int32.Parse(s.X),
                            Y = int.Parse(s.Y)
                        }).ToList()
                    };

                    var objectGroup = classTable[sp.Class].Groups.Where(g => g.Name == sp.Group).FirstOrDefault();
                    if (objectGroup == null)
                    {
                        objectGroup = new GameObjectGroup()
                        {
                            Name = sp.Group
                        };
                        classTable[sp.Class].Groups.Add(objectGroup);
                    }

                    objectGroup.GameObjects.Add(gameObject);
                }
                catch(Exception e)
                {
                    _errorService.LogError(e, "Sprite name: " + sp.Name);
                }
            }

            return new List<GameObjectClass>()
            {
                classTable["1"],
                classTable["2"],
                classTable["3"],
            };
        }
    }
}
