using Spotlight.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static Spotlight.Models.GameObject;

namespace Spotlight.Services
{
    public class GameObjectService
    {
        private readonly ErrorService _errorService;
        private readonly Project _project;

        public GameObjectService(ErrorService errorService, Project project)
        {
            _errorService = errorService;
            _project = project;
        }



        public List<GameObject> ConvertFromLegacy(string fileName)
        {
            List<GameObject> gameObjects = new List<GameObject>();

            XElement root = XDocument.Load(fileName).Element("sprites");
            foreach (var x in root.Elements("spritedefinition"))
            {
                SpriteDefinition sp = new SpriteDefinition();
                sp.LoadFromElement(x);
                try
                {
                    gameObjects.Add(new GameObject()
                    {
                        GameId = Int32.Parse(sp.InGameId, System.Globalization.NumberStyles.HexNumber),
                        Name = sp.Name,
                        Properties = sp.PropertyDescriptions,
                        Sprites = sp.Sprites.Select(s => new Sprite()
                        {
                            PropertiesAppliedTo = s.Property?.Select(p => int.Parse(p)).ToList(),
                            PaletteIndex = Int32.Parse(s.Palette),
                            HorizontalFlip = bool.Parse(s.HorizontalFlip),
                            VerticalFlip = bool.Parse(s.VerticalFlip),
                            Overlay = s.Table == "-1",
                            TileTableIndex = s.Table == "-1" ? -1 : Int32.Parse(s.Table, System.Globalization.NumberStyles.HexNumber),
                            TileValue = Int32.Parse(s.Value, System.Globalization.NumberStyles.HexNumber),
                            X = Int32.Parse(s.X),
                            Y = int.Parse(s.Y)
                        }).ToList()
                    });
                }
                catch (Exception e)
                {
                    _errorService.LogError(e, "Sprite name: " + sp.Name);
                }
            }

            return gameObjects;
        }
    }
}
