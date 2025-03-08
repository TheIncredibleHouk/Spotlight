using Spotlight.Models;
using Spotlight.Services;
using System.Drawing;
using System.Windows;

namespace Spotlight.Renderers
{
    public enum PaletteType
    {
        Section,
        Full,
    }

    public class PaletteRenderer : Renderer
    {
        private byte[] _buffer;

        private PaletteService _palettesService;
        private PaletteType _paletteType;

        public PaletteRenderer(PaletteService paletteService, PaletteType paletteType) : base(null)
        {
            _buffer = new byte[256 * (paletteType == PaletteType.Full ? 64 : 32) * BYTES_PER_BLOCK];
            _paletteType = paletteType;
            _palettesService = paletteService;
            BYTE_STRIDE = 256 * 4;
        }

        public byte[] GetRectangle(Rectangle rect)
        {
            return GetRectangle(rect, _buffer);
        }

        private Palette _palette;

        public void Update(Palette palette)
        {
            _palette = palette;
            Update();
        }

        public void Update()
        {
            int maxRow = _paletteType == PaletteType.Full ? 4 : 2;
            int maxCol = 16;

            for (int row = 0; row < maxRow; row++)
            {
                for (int col = 0; col < maxCol; col++)
                {
                    int x = col * 16, y = row * 16;

                    if (_paletteType == PaletteType.Full)
                    {
                        DrawColorTile(x, y, _palettesService.RgbPalette[row * 16 + col], _buffer);
                    }
                    else
                    {
                        DrawColorTile(x, y, _palette.RgbColors[row * 4 + (col / 4)][col % 4], _buffer);
                    }
                }
            }
        }
    }
}