using Spotlight.Models;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Spotlight.Services
{
    public class LevelDataAccessor
    {
        private int[] _tileData;
        private List<LevelObject> _gameObjects;
        private List<LevelPointer> _pointers;
        private const int DATA_ROW_LENGTH = 15 * 16;

        public LevelDataAccessor(Level _level)
        {
            _tileData = _level.TileData;
            _gameObjects = _level.ObjectData;
            _pointers = _level.LevelPointers;
        }

        public int GetData(int x, int y)
        {
            if (x < 0 || y < 0 || x >= 240 || y >= 27)
            {
                return -1;
            }

            int dataOffset = (y * DATA_ROW_LENGTH) + x;
            if (dataOffset >= _tileData.Length)
            {
                return -1;
            }

            return _tileData[dataOffset];
        }

        public void SetData(int x, int y, int tileValue)
        {
            int dataOffset = y * DATA_ROW_LENGTH;
            int offset = dataOffset + x;

            if (offset < _tileData.Length)
            {
                _tileData[offset] = tileValue;
            }
        }

        public void ReplaceValue(int existingValue, int newValue)
        {
            for (int i = 0; i < _tileData.Length; i++)
            {
                if (_tileData[i] == existingValue)
                {
                    _tileData[i] = newValue;
                }
            }
        }

        public List<LevelObject> GetLevelObjects(Rect area)
        {
            return _gameObjects.Where(o => o.BoundRectangle.IntersectsWith(area)).OrderBy(o => o.X).ThenBy(o => o.Y).ToList();
        }

        public List<LevelPointer> GetPointers(Rect area)
        {
            return _pointers.Where(p => p.BoundRectangle.IntersectsWith(area)).OrderBy(o => o.X).ThenBy(o => o.Y).ToList();
        }
    }
}