using Spotlight.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotlight.Services
{
    public class LevelDataAccessor
    {
        private int[] _tileData;
        private List<LevelObject> _gameObjects;
        private const int DATA_ROW_LENGTH = 15 * 16;

        public LevelDataAccessor(Level _level)
        {
            _tileData = _level.TileData;
            _gameObjects = _level.ObjectData;
        }

        public int GetData(int x, int y)
        {
            int dataOffset = y * DATA_ROW_LENGTH;
            return _tileData[dataOffset + x];
        }

        public void SetData(int x, int y, int tileValue)
        {
            int dataOffset = y * DATA_ROW_LENGTH;
            _tileData[dataOffset + x] = tileValue;
        }

        public List<LevelObject> GetLevelObjects()
        {
            return _gameObjects;
        }
    }
}
