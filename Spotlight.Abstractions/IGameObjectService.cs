
using Spotlight.Models;

namespace Spotlight.Abstractions
{
    public interface IGameObjectService
    {
        List<Sprite> FallBackSprites(GameObject gameObject);
        List<string> GetObjectGroups(GameObjectType type);
        List<LevelObject> GetObjectsByGroup(GameObjectType type, string group);
        List<GameObject> GetObjectsByIds(IEnumerable<int> gameObjectIds);
        GameObject GetObjectById(int gameObjectId);
        GameObject GetStartPointObject();
        void RefreshGameObjectTable();
        void UpdateGameTable(GameObject gameObject);
        void CommitGameObject(GameObject gameObject);
    }
}
