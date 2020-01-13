using Spotlight.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using static Spotlight.Models.GameObject;

namespace Spotlight.Services
{
    public class GameObjectService
    {
        private readonly ErrorService _errorService;
        private readonly Project _project;
        private Dictionary<GameObjectType, List<LevelObject>> _gameObjectTable;

        public Dictionary<GameObjectType, Rect> GameObjectRenderAreas { get; private set; }

        public GameObjectService(ErrorService errorService, Project project)
        {
            _errorService = errorService;
            _project = project;

            RefreshGameObjectTable();
            GameObjectRenderAreas = new Dictionary<GameObjectType, Rect>();

            GameObjectRenderAreas[GameObjectType.Global] = new Rect();
            GameObjectRenderAreas[GameObjectType.TypeA] = new Rect();
            GameObjectRenderAreas[GameObjectType.TypeB] = new Rect();
        }

        public Dictionary<GameObjectType, List<LevelObject>> GameObjectTable()
        {
            return _gameObjectTable;
        }

        public void RefreshGameObjectTable()
        {
            _gameObjectTable = new Dictionary<GameObjectType, List<LevelObject>>();
            _gameObjectTable[GameObjectType.Global] = new List<LevelObject>();
            _gameObjectTable[GameObjectType.TypeA] = new List<LevelObject>();
            _gameObjectTable[GameObjectType.TypeB] = new List<LevelObject>();

            foreach (GameObject gameObject in _project.GameObjects.OrderBy(g => g.GameId))
            {
                switch (gameObject.GameObjectType)
                {
                    case GameObjectType.Global:
                        {
                            var nextObject = new LevelObject()
                            {
                                GameObjectId = gameObject.GameId,
                                GameObject = gameObject,
                                X = 0,
                                Y = (int)GameObjectRenderAreas[GameObjectType.Global].Height
                            };

                            _gameObjectTable[GameObjectType.Global].Add(nextObject);
                            nextObject.CalcBoundBox();

                            Rect boundRect = nextObject.BoundRectangle;


                        }
                        break;

                    case GameObjectType.TypeA:
                        {
                            var nextObject = new LevelObject()
                            {
                                GameObjectId = gameObject.GameId,
                                GameObject = gameObject,
                                X = 0,
                                Y = (int)GameObjectRenderAreas[GameObjectType.TypeA].Height
                            };

                            _gameObjectTable[GameObjectType.Global].Add(nextObject);
                            nextObject.CalcBoundBox();
                        }
                        break;

                    case GameObjectType.TypeB:
                        {
                            var nextObject = new LevelObject()
                            {
                                GameObjectId = gameObject.GameId,
                                GameObject = gameObject,
                                X = 0,
                                Y = (int)GameObjectRenderAreas[GameObjectType.TypeB].Height
                            };

                            _gameObjectTable[GameObjectType.Global].Add(nextObject);
                            nextObject.CalcBoundBox();
                        }
                        break;
                }
            }
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
                        GameObjectType = (GameObjectType)Enum.Parse(typeof(GameObjectType), sp.Class),
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
