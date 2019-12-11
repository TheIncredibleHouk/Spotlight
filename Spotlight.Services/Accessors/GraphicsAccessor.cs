using Spotlight.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotlight.Services
{
    public class GraphicsAccessor
    {
        private Tile[] _relativeTable;
        private Tile[] _globalTable;
        public GraphicsAccessor(Tile[] staticTable, Tile[] animatedTable, Tile[] globalTable)
        {
            _relativeTable = new Tile[256];
            for(int i = 0; i < 128; i++)
            {
                _relativeTable[i] = staticTable[i];
                _relativeTable[i + 128] = animatedTable[i];
            }

            _globalTable = globalTable;
        }

        public Tile GetAbsoluteTile(int tileTableIndex, int tileIndex)
        {
            return _globalTable[tileTableIndex * 0x40 + tileIndex];
        }

        public Tile GetRelativeTile(int tileIndex)
        {
            return _relativeTable[tileIndex];
        }
    }
}
