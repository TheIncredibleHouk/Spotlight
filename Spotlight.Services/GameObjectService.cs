using Newtonsoft.Json;
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
        public delegate void GameObjectEventHandler(GameObject gameObject);
        public event GameObjectEventHandler GameObjectUpdated;

        private readonly ErrorService _errorService;
        private readonly Project _project;

        public readonly List<Sprite> InvisibleSprites = new List<Sprite>()
        {
            new Sprite() { X = 0, Y = 0, PaletteIndex = 1, TileValueIndex = 0, Overlay = true},
            new Sprite() { X = 8, Y = 0, PaletteIndex = 1, TileValueIndex = 2, Overlay = true}
        };

        public GameObjectTable GameObjectTable { get; private set; }
        private GameObject[] localGameObjects;

        public GameObjectService(ErrorService errorService, Project project)
        {
            _errorService = errorService;
            _project = project;
            GameObjectTable = new GameObjectTable();
            localGameObjects = JsonConvert.DeserializeObject<GameObject[]>(JsonConvert.SerializeObject(_project.GameObjects));

            for(int i = 0; i < localGameObjects.Length; i++)
            {
                if(localGameObjects[i] == null)
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

        public void RefreshGameObjectTable()
        {
            GameObjectTable.Clear();

            Dictionary<GameObjectType, Dictionary<string, int>> nextX = new Dictionary<GameObjectType, Dictionary<string, int>>();
            Dictionary<GameObjectType, Dictionary<string, int>> nextY = new Dictionary<GameObjectType, Dictionary<string, int>>();
            Dictionary<GameObjectType, Dictionary<string, int>> maxHeight = new Dictionary<GameObjectType, Dictionary<string, int>>();

            foreach (GameObject gameObject in localGameObjects.Where(g => g != null).OrderBy(g => g.Name))
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

                if (actualX < nextObject.X)
                {
                    nextObject.X += nextObject.X - actualX;
                    rect = nextObject.CalcBoundBox();
                }

                if(actualY < nextObject.Y)
                {
                    nextObject.Y += nextObject.Y - actualY;
                    rect = nextObject.CalcBoundBox();
                }

                if ((int)(rect.Height / 16) > maxHeight[gameObject.GameObjectType][gameObject.Group])
                {
                    maxHeight[gameObject.GameObjectType][gameObject.Group] = (int)(rect.Height / 16);
                }

                if (rect.X + rect.Width > 256)
                {
                    nextX[gameObject.GameObjectType][gameObject.Group] = nextObject.X = 0;
                    nextY[gameObject.GameObjectType][gameObject.Group] =  nextObject.Y += maxHeight[gameObject.GameObjectType][gameObject.Group] + 1;
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
            globalGameObject.Properties = gameObject.Properties;
            globalGameObject.Sprites = gameObject.Sprites;

            if(GameObjectUpdated != null)
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
