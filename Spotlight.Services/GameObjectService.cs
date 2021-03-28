using Newtonsoft.Json;
using Spotlight.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Xml.Linq;
using static Spotlight.Models.GameObject;

namespace Spotlight.Services
{
    public class GameObjectService
    {
        public delegate void GameObjectEventHandler(GameObject gameObject);

        public event GameObjectEventHandler GameObjectUpdated;

        private readonly ErrorService _errorService;
        private readonly Project _project;

        private static Dictionary<char, int> hexToSprite = new Dictionary<char, int>()
        {
            {'1', Int32.Parse("20", System.Globalization.NumberStyles.HexNumber) },
            {'2', Int32.Parse("22", System.Globalization.NumberStyles.HexNumber) },
            {'3', Int32.Parse("24", System.Globalization.NumberStyles.HexNumber) },
            {'4', Int32.Parse("26", System.Globalization.NumberStyles.HexNumber) },
            {'5', Int32.Parse("28", System.Globalization.NumberStyles.HexNumber) },
            {'6', Int32.Parse("40", System.Globalization.NumberStyles.HexNumber) },
            {'7', Int32.Parse("42", System.Globalization.NumberStyles.HexNumber) },
            {'8', Int32.Parse("44", System.Globalization.NumberStyles.HexNumber) },
            {'9', Int32.Parse("46", System.Globalization.NumberStyles.HexNumber) },
            {'0', Int32.Parse("48", System.Globalization.NumberStyles.HexNumber) },
            {'A', Int32.Parse("60", System.Globalization.NumberStyles.HexNumber) },
            {'B', Int32.Parse("62", System.Globalization.NumberStyles.HexNumber) },
            {'C', Int32.Parse("64", System.Globalization.NumberStyles.HexNumber) },
            {'D', Int32.Parse("66", System.Globalization.NumberStyles.HexNumber) },
            {'E', Int32.Parse("68", System.Globalization.NumberStyles.HexNumber) },
            {'F', Int32.Parse("6A", System.Globalization.NumberStyles.HexNumber) }
        };

        public List<Sprite> FallBackSprites(GameObject gameObject)
        {
            List<Sprite> sprites = new List<Sprite>();
            string objectId = gameObject.GameId.ToString("X2");
            sprites.Add(new Sprite()
            {
                X = 0,
                Y = 0,
                Overlay = true,
                TileTableAddress = "0x1000",
                TileValueIndex = hexToSprite[objectId[0]],
                PaletteIndex = 1
            });
            sprites.Add(new Sprite()
            {
                X = 8,
                Y = 0,
                Overlay = true,
                TileTableAddress = "0x1000",
                TileValueIndex = hexToSprite[objectId[1]],
                PaletteIndex = 1
            });

            return sprites;
        }

        public GameObjectTable GameObjectTable { get; private set; }
        private GameObject[] localGameObjects;

        public GameObjectService(ErrorService errorService, Project project)
        {
            _errorService = errorService;
            _project = project;
            GameObjectTable = new GameObjectTable();
            localGameObjects = JsonConvert.DeserializeObject<GameObject[]>(JsonConvert.SerializeObject(_project.GameObjects));

            for (int i = 0; i < localGameObjects.Length; i++)
            {
                if (localGameObjects[i] == null)
                {
                    localGameObjects[i] = new GameObject()
                    {
                        Group = "Unused",
                        GameId = i,
                        GameObjectType = GameObjectType.Global,
                        Name = "Unused x" + i.ToString("X"),
                        Sprites = new List<Sprite>()
                        {
                            new Sprite(){ Overlay = true, X = 0, Y = 0, TileValueIndex = 8},
                            new Sprite(){ Overlay = true, X = 0, Y = 0, TileValueIndex = 0x0A}
                        }
                    };
                }
            }
            RefreshGameObjectTable();
            _startPointPalette = new string[4] { "00", "0F", "36", "16" };
            _startPointGameObject = new GameObject()
            {
                GameId = 0,
                Name = "Start Point",
                Sprites = new List<Sprite>()
               {
                   new Sprite()
                   {
                       X = 0,
                       Y = 0,
                       TileValueIndex = 0x8,
                       TileTableIndex = 4,
                       CustomPalette = _startPointPalette,
                       Overlay = true
                   },
                   new Sprite()
                   {
                       X = 8,
                       Y = 0,
                       TileValueIndex = 0xA,
                       TileTableIndex = 4,
                       CustomPalette = _startPointPalette,
                       Overlay = true
                   }
               }
            };
        }

        public List<string> GetGroups(GameObjectType type)
        {
            return GameObjectTable.ObjectTable[type].Keys.ToList();
        }

        public List<LevelObject> GetObjects(GameObjectType type, string group)
        {
            return GameObjectTable.ObjectTable[type][group];
        }

        public GameObject GetObject(int gameObjectId)
        {
            return localGameObjects[gameObjectId];
        }

        private string[] _startPointPalette;
        private GameObject _startPointGameObject;

        public GameObject GetStartPointObject()
        {
            _startPointGameObject.IsStartObject = true;
            return _startPointGameObject;
        }

        public void RefreshGameObjectTable()
        {
            GameObjectTable.Clear();

            Dictionary<GameObjectType, Dictionary<string, int>> nextX = new Dictionary<GameObjectType, Dictionary<string, int>>();
            Dictionary<GameObjectType, Dictionary<string, int>> nextY = new Dictionary<GameObjectType, Dictionary<string, int>>();
            Dictionary<GameObjectType, Dictionary<string, int>> maxHeight = new Dictionary<GameObjectType, Dictionary<string, int>>();

            foreach (GameObject gameObject in _project.GameObjects.Where(g => g != null).OrderBy(g => g.Name))
            {
                if (!nextX.ContainsKey(gameObject.GameObjectType))
                {
                    nextX[gameObject.GameObjectType] = new Dictionary<string, int>();
                    nextY[gameObject.GameObjectType] = new Dictionary<string, int>();
                    maxHeight[gameObject.GameObjectType] = new Dictionary<string, int>();
                }

                if (!nextX[gameObject.GameObjectType].ContainsKey(gameObject.Group))
                {
                    nextX[gameObject.GameObjectType][gameObject.Group] = 0;
                    nextY[gameObject.GameObjectType][gameObject.Group] = 0;
                    maxHeight[gameObject.GameObjectType][gameObject.Group] = 0;
                }

                var nextObject = new LevelObject()
                {
                    GameObjectId = gameObject.GameId,
                    GameObject = gameObject,
                    X = nextX[gameObject.GameObjectType][gameObject.Group],
                    Y = nextY[gameObject.GameObjectType][gameObject.Group]
                };

                Rect rect = nextObject.CalcBoundBox();

                int actualX = (int)(rect.X / 16);
                int actualY = (int)(rect.Y / 16);

                if (nextObject.X - actualX >= 1)
                {
                    if (nextObject.X == 0)
                    {
                        nextObject.X += nextObject.X - actualX;
                    }
                    else
                    {
                        nextObject.X += (nextObject.X - actualX - 1);
                    }

                    rect = nextObject.CalcBoundBox();
                }

                if (nextObject.Y - actualY >= 1)
                {
                    nextObject.Y += (nextObject.Y - actualY - 1);
                    rect = nextObject.CalcBoundBox();
                }

                if ((int)(rect.Height / 16) > maxHeight[gameObject.GameObjectType][gameObject.Group])
                {
                    maxHeight[gameObject.GameObjectType][gameObject.Group] = (int)(rect.Height / 16);
                }

                if (rect.X + rect.Width > 256)
                {
                    nextX[gameObject.GameObjectType][gameObject.Group] = nextObject.X = 0;
                    nextY[gameObject.GameObjectType][gameObject.Group] = nextObject.Y += maxHeight[gameObject.GameObjectType][gameObject.Group] + 1;
                    rect = nextObject.CalcBoundBox();
                }

                nextX[gameObject.GameObjectType][gameObject.Group] += (int)(rect.Width / 16) + (rect.Width % 16 > 0 ? 2 : 1);

                GameObjectTable.AddObject(gameObject.GameObjectType, gameObject.Group, nextObject);
            }
        }

        public void UpdateGameTable(GameObject gameObject)
        {
            GameObjectTable.UpdateGameObject(gameObject);
            GameObjectUpdated(gameObject);
        }

        public void CommitGameObject(GameObject gameObject)
        {
            GameObject globalGameObject = _project.GameObjects[gameObject.GameId];
            globalGameObject.Name = gameObject.Name;
            globalGameObject.Group = gameObject.Group;
            globalGameObject.GameObjectType = gameObject.GameObjectType;
            globalGameObject.Properties = gameObject.Properties;
            globalGameObject.Sprites = gameObject.Sprites;
            RefreshGameObjectTable();

            if (GameObjectUpdated != null)
            {
                GameObjectUpdated(globalGameObject);
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
                        GameObjectType = (GameObjectType)int.Parse(sp.Class),
                        Properties = sp.PropertyDescriptions,
                        Group = sp.Group,
                        Sprites = sp.Sprites.Select(s => new Sprite()
                        {
                            PropertiesAppliedTo = s.Property?.Select(p => int.Parse(p)).ToList(),
                            PaletteIndex = Int32.Parse(s.Palette),
                            HorizontalFlip = bool.Parse(s.HorizontalFlip),
                            VerticalFlip = bool.Parse(s.VerticalFlip),
                            Overlay = s.Table == "-1",
                            TileTableAddress = s.Table == "-1" ? "0x0" : "0x" + (Int32.Parse(s.Table, System.Globalization.NumberStyles.HexNumber) * 0x400).ToString("X2"),
                            TileValue = "0x" + s.Value,
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