using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Spotlight.Abstractions;
using Spotlight.Models;
using Spotlight.Renderers;

namespace Spotlight.Renderers
{
    public class BlockRenderer : Renderer, IBlockRenderer
    {
        private byte[] _buffer;
        private Tile[,] _localTiles;

        public BlockRenderer() : base(null)
        {
            _buffer = new byte[BYTES_PER_PIXEL * PIXELS_PER_TILE * 4];
            _localTiles = new Tile[2, 2];

            _localTiles[0, 0] = new Tile();
            _localTiles[1, 0] = new Tile();
            _localTiles[0, 1] = new Tile();
            _localTiles[1, 1] = new Tile();

            BYTE_STRIDE = 16* 4;
        }

        private Palette _palette;
        private int _paletteIndex = 0;

        public void Update(Tile[,] tiles = null, Palette palette = null, int? paletteIndex = null)
        {
            _palette = palette ?? _palette;
            _paletteIndex = paletteIndex ?? _paletteIndex;

            if(tiles != null)
            {
                _localTiles[0, 0].ApplyTile(tiles[0, 0]);
                _localTiles[0, 1].ApplyTile(tiles[0, 1]);
                _localTiles[1, 0].ApplyTile(tiles[1, 0]);
                _localTiles[1, 1].ApplyTile(tiles[1, 1]);
            }

            Update();
        }

        public byte[] GetRectangle()
        {
            return GetRectangle(new Rectangle(0, 0, 16, 16), _buffer);
        }

        public void SetPixel(int x, int y, int value)
        {
            int tileX = x / 8;
            int tileY = y / 8;
            int relativeX = x % 8;
            int relativeY = y % 8;

            _localTiles[x, y][relativeX, relativeY] = (byte) value;
        }

        public void Update()
        {
            if (_palette == null)
            {
                return;
            }

            RenderTile(0, 0, _localTiles[0, 0], _buffer, _palette.RgbColors[_paletteIndex]);
            RenderTile(8, 0, _localTiles[1, 0], _buffer, _palette.RgbColors[_paletteIndex]);
            RenderTile(0, 8, _localTiles[0, 1], _buffer, _palette.RgbColors[_paletteIndex]);
            RenderTile(8, 8, _localTiles[1, 1], _buffer, _palette.RgbColors[_paletteIndex]);
        }
    }
}