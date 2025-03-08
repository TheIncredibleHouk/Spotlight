using Newtonsoft.Json;
using Spotlight.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using Spotlight.Abstractions;

namespace Spotlight.Services
{
    public class GameObjectService : IGameObjectService
    {
        public delegate void GameObjectEventHandler(GameObject gameObject);

        public event GameObjectEventHandler GameObjectUpdated;

        private readonly IErrorService _errorService;
        private readonly IProjectService _projectService;

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

        public GameObjectService(IErrorService errorService, IProjectService projectService)
        {
            _errorService = errorService;
            _projectService = projectService;
            GameObjectTable = new GameObjectTable();
            localGameObjects = JsonConvert.DeserializeObject<GameObject[]>(JsonConvert.SerializeObject(_projectService.GetProject().GameObjects));

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

        public List<string> GetObjectGroups(GameObjectType type)
        {
            return GameObjectTable.ObjectTable[type].Keys.ToList();
        }

        public List<LevelObject> GetObjectsByGroup(GameObjectType type, string group)
        {
            return GameObjectTable.ObjectTable[type][group];
        }

        public List<GameObject> GetObjectsByIds(IEnumerable<int> gameObjectIds)
        {
            return localGameObjects.Where(gameObject => gameObjectIds.Contains(gameObject.GameId)).ToList();
        }

        public GameObject GetObjectById(int gameObjectId)
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

            foreach (GameObject gameObject in _projectService.GetProject().GameObjects.Where(g => g != null).OrderBy(g => g.Name))
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

                    GameObject = gameObject,
                    X = nextX[gameObject.GameObjectType][gameObject.Group],
                    Y = nextY[gameObject.GameObjectType][gameObject.Group]
                };

                Rectangle Rectangle = nextObject.CalcBoundBox();

                int actualX = (int)(Rectangle.X / 16);
                int actualY = (int)(Rectangle.Y / 16);

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

                    Rectangle = nextObject.CalcBoundBox();
                }

                if (nextObject.Y - actualY >= 1)
                {
                    nextObject.Y += (nextObject.Y - actualY - 1);
                    Rectangle = nextObject.CalcBoundBox();
                }

                if ((int)(Rectangle.Height / 16) > maxHeight[gameObject.GameObjectType][gameObject.Group])
                {
                    maxHeight[gameObject.GameObjectType][gameObject.Group] = (int)(Rectangle.Height / 16);
                }

                if (Rectangle.X + Rectangle.Width > 256)
                {
                    nextX[gameObject.GameObjectType][gameObject.Group] = nextObject.X = 0;
                    nextY[gameObject.GameObjectType][gameObject.Group] = nextObject.Y += maxHeight[gameObject.GameObjectType][gameObject.Group] + 1;
                    Rectangle = nextObject.CalcBoundBox();
                }

                nextX[gameObject.GameObjectType][gameObject.Group] += (int)(Rectangle.Width / 16) + (Rectangle.Width % 16 > 0 ? 2 : 1);

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
            GameObject globalGameObject = _projectService.GetProject().GameObjects[gameObject.GameId];
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

        //public IEnumerable<LevelInfo> FindInLevels(GameObject gameObject)
        //{
        //    List<LevelInfo> foundLevels = new List<LevelInfo>();
        //    LevelService levelService = new LevelService(_errorService, _project, null);
        //    foreach(LevelInfo levelInfo in levelService.AllLevels())
        //    {
        //        if(levelService.LoadLevel(levelInfo).ObjectData.Exists(obj => obj.GameObjectId == gameObject.GameId))
        //        {
        //            foundLevels.Add(levelInfo);
        //        }
        //    }

        //    return foundLevels;
        //}
    }
}