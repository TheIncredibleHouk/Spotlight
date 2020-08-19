using Spotlight.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotlight.Services
{
    public class GraphicsAccessor
    {
        private Tile[] _relativeTable;
        private Tile[] _globalTable;
        private Tile[] _overlayTable;
        public GraphicsAccessor(Tile[] staticTable, Tile[] animatedTable, Tile[] globalTable, Tile[] overlayTable)
        {
            _relativeTable = new Tile[256];
            for (int i = 0; i < 128; i++)
            {
                _relativeTable[i] = staticTable[i];
                _relativeTable[i + 128] = animatedTable[i];
            }

            _globalTable = globalTable;
            _overlayTable = overlayTable;
        }

        public GraphicsAccessor(Tile[] globalTable, Tile[] extraTable)
        {
            _globalTable = globalTable;
            _overlayTable = extraTable;
        }

        public Tile GetAbsoluteTile(int tileTableIndex, int tileIndex)
        {
            int absoluteTileIndex = tileTableIndex * 0x40 + tileIndex;
            if (absoluteTileIndex > _globalTable.Length - 1)
            {
                absoluteTileIndex = _globalTable.Length - 1;
            }
            return _globalTable[absoluteTileIndex];
        }

        public Tile GetRelativeTile(int tileIndex)
        {
            return _relativeTable[tileIndex];
        }

        public Tile GetOverlayTile(int bank, int tileIndex)
        {
            return _overlayTable[bank * 0x40 + tileIndex];
        }

        public void SetStaticTable(Tile[] staticTable)
        {
            for (int i = 0; i < 128; i++)
            {
                _relativeTable[i] = staticTable[i];
            }
        }

        public void SetAnimatedTable(Tile[] animatedTable)
        {
            for (int i = 0; i < 128; i++)
            {
                _relativeTable[i + 128] = animatedTable[i];

            }
        }

        public void SetGlobalTiles(Tile[] globalTable, Tile[] extraTable)
        {
            _globalTable = globalTable;
            _overlayTable = extraTable;
        }
    }
}
