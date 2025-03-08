using Spotlight.Models;
using Spotlight.Renderers;
using Spotlight.Services;

namespace Spotlight
{
    public class GraphicsRenderer : Renderer
    {
        private byte[] _buffer;

        public GraphicsRenderer(GraphicsManager graphicsAccessor) : base(graphicsAccessor)
        {
            _buffer = new byte[256 * 256 * BYTES_PER_BLOCK];
        }

        public void Update(Palette palette, int paletteIndex)
        {
            for (var i = 0; i < 256; i++)
            {
                int x = i % 16;
                int y = i / 16;
                RenderTile(x, y, _graphicsAccessor.GetRelativeTile(i), _buffer, palette.RgbColors[paletteIndex]);
            }
        }
    }
}