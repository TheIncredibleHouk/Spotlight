namespace Spotlight.Models
{
    public class WorldObjectChange
    {
        public WorldObjectChange(WorldObject worldObject, int x, int y, int gameId, WorldObjectChangeType changeType)
        {
            OriginalObject = worldObject;
            X = x;
            Y = y;
            ChangeType = changeType;
            GameId = gameId;
        }

        public WorldObject OriginalObject { get; private set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int GameId { get; set; }
        public WorldObjectChangeType ChangeType { get; set; }

        public bool IsSame()
        {
            if (OriginalObject.X == X &&
                OriginalObject.Y == Y &&
                OriginalObject.GameObjectId == GameId &&
                ChangeType == WorldObjectChangeType.Update)
            {
                return true;
            }

            return false;
        }
    }

    public enum WorldObjectChangeType
    {
        Addition,
        Deletion,
        Update
    }
}