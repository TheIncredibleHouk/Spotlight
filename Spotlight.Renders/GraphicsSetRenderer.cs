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
    public class GraphicsSetRender : Renderer
    {

        private byte[] _buffer;

        public GraphicsSetRender(GraphicsAccessor graphicsAccessor) : base(graphicsAccessor)
        {
            _buffer = new byte[BYTES_PER_PIXEL * PIXELS_PER_TILE * TILES_PER_COLUMN * TILES_PER_ROW];
            _graphicsAccessor = graphicsAccessor;

            BYTE_STRIDE = 128 * 4;
        }

        public byte[] GetRectangle(Int32Rect rect)
        {
            return GetRectangle(rect, _buffer);
        }

        private Palette _palette;
        private int _paletteIndex = 0;

        public void Update(Palette palette = null, int? paletteIndex = null)
        {
            _palette = palette ?? _palette;
            _paletteIndex = paletteIndex ?? _paletteIndex;
            Update();
        }

        public void Update()
        {
            if(_palette == null)
            {
                return;
            }

            int maxRow = 16;
            int maxCol = 16;

            for (int row = 0; row < maxRow; row++)
            {
                for (int col = 0; col < maxCol; col++)
                {

                    int tileValue = row * 16 + col;
                    int x = col * 8, y = row * 8;

                    RenderTile(x, y, _graphicsAccessor.GetRelativeTile(tileValue), _buffer, _palette.RgbColors[_paletteIndex]);
                }
            }
        }
    }
}
