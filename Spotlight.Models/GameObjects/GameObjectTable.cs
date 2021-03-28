using System.Collections.Generic;
using System.Linq;

namespace Spotlight.Models
{
    public class GameObjectTable
    {
        public Dictionary<GameObjectType, Dictionary<string, List<LevelObject>>> ObjectTable { get; set; }
        public List<LevelObject> AllObjects { get; set; }

        public GameObjectTable()
        {
            Clear();
        }

        public void UpdateGameObject(GameObject gameObject)
        {
            LevelObject existingLevelObject = AllObjects.Where(g => g.GameObjectId == gameObject.GameId).FirstOrDefault();
            
            if (existingLevelObject != null)
            {
                if (gameObject.GameObjectType != existingLevelObject.GameObject.GameObjectType ||
                    gameObject.Group != existingLevelObject.GameObject.Group)
                {
                    ObjectTable[existingLevelObject.GameObject.GameObjectType][existingLevelObject.GameObject.Group].Remove(existingLevelObject);
                    AddObject(gameObject.GameObjectType, gameObject.Group, existingLevelObject);
                }

                existingLevelObject.GameObject = gameObject;
            }
        }

        public void Clear()
        {
            ObjectTable = new Dictionary<GameObjectType, Dictionary<string, List<LevelObject>>>();
            AllObjects = new List<LevelObject>();
        }

        public void AddObject(GameObjectType type, string group, LevelObject levelObject)
        {
            if (!ObjectTable.ContainsKey(type))
            {
                ObjectTable[type] = new Dictionary<string, List<LevelObject>>();
            }

            if (!ObjectTable[type].ContainsKey(group))
            {
                ObjectTable[type][group] = new List<LevelObject>();
            }

            ObjectTable[type][group].Add(levelObject);
            AllObjects.Add(levelObject);
        }
    }
}