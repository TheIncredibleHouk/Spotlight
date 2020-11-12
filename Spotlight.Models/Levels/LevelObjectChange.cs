namespace Spotlight.Models
{
    public class LevelObjectChange
    {
        public LevelObjectChange(LevelObject levelObject, int x, int y, int property, int gameId, LevelObjectChangeType changeType)
        {
            OriginalObject = levelObject;
            Property = property;
            X = x;
            Y = y;
            ChangeType = changeType;
            GameId = gameId;
        }

        public LevelObject OriginalObject { get; private set; }
        public int Property { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int GameId { get; set; }
        public LevelObjectChangeType ChangeType { get; set; }

        public bool IsSame()
        {
            if (OriginalObject.X == X &&
                OriginalObject.Y == Y &&
                OriginalObject.Property == Property &&
                OriginalObject.GameObjectId == GameId &&
                ChangeType == LevelObjectChangeType.Update)
            {
                return true;
            }

            return false;
        }
    }

    public enum LevelObjectChangeType
    {
        Addition,
        Deletion,
        Update
    }
}