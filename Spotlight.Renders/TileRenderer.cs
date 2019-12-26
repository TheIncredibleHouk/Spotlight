using Spotlight.Models;
using Spotlight.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Spotlight.Renderers
{
    public class TileRenderer : Renderer
    {

        public const int BITMAP_HEIGHT = Level.BLOCK_HEIGHT * 16;
        public const int BITMAP_WIDTH = Level.BLOCK_WIDTH * 16;

        private byte[] _BackgroundLayer;

        public TileRenderer(GraphicsAccessor graphicsAccessor) : base(graphicsAccessor)
        {
            _BackgroundLayer = new byte[256 * 256 * BYTES_PER_BLOCK];
            _graphicsAccessor = graphicsAccessor;
        }

        public byte[] GetRectangle(Int32Rect rect)
        {
            return GetRectangle(rect, _BackgroundLayer);
        }

        private TileSet _tileSet;
        private Palette _palette;

        public void Update(TileSet tileSet, Palette palette)
        {
            _tileSet = tileSet;
            _palette = palette;
            Update();
        }

        public void Update(TileSet tileSet)
        {
            _tileSet = tileSet;
            Update();
        }

        public void Update(Palette palette)
        {
            _palette = palette;
            Update();
        }

        public void Update()
        {
            
            int maxRow = 16;
            int maxCol = 16;

            for (int row = 0; row < maxRow; row++)
            {
                for (int col = 0; col < maxCol; col++)
                {

                    int tileValue = row * 16 + col;
                    int PaletteIndex = tileValue / 0x40;

                    TileBlock tile = _tileSet.Tiles[tileValue];
                    int paletteIndex = (tileValue & 0XC0) >> 6;
                    int x = col * 16, y = row * 16;

                    RenderTile(x, y, _graphicsAccessor.GetRelativeTile(tile.UpperLeft), tile.UpperLeftPalette, _BackgroundLayer, _palette.RgbColors[paletteIndex]);
                    RenderTile(x + 8, y, _graphicsAccessor.GetRelativeTile(tile.UpperRight), tile.UpperRightPalette, _BackgroundLayer, _palette.RgbColors[paletteIndex]);
                    RenderTile(x, y + 8, _graphicsAccessor.GetRelativeTile(tile.LowerLeft), tile.LowerLeftPalette, _BackgroundLayer, _palette.RgbColors[paletteIndex]);
                    RenderTile(x + 8, y + 8, _graphicsAccessor.GetRelativeTile(tile.LowerRight), tile.LowerRightPalette, _BackgroundLayer, _palette.RgbColors[paletteIndex]);
                }
            }
        }
    }
}
