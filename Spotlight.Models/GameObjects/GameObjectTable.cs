using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Spotlight.Models
{
    public class GameObjectTable
    {
        public Dictionary<GameObjectType, Dictionary<string, List<LevelObject>>> ObjectTable { get; set; }

        public GameObjectTable()
        {
            Clear();
        }

        public void UpdateGameObject(GameObject gameObject)
        {
            LevelObject existingLevelObject = ObjectTable[gameObject.GameObjectType][gameObject.Group].Where(g => g.GameObjectId == gameObject.GameId).FirstOrDefault();
            if (existingLevelObject != null)
            {
                existingLevelObject.GameObject = gameObject;
            }
        }

        public void Clear()
        {
            ObjectTable = new Dictionary<GameObjectType, Dictionary<string, List<LevelObject>>>();
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
        }
    }
}
