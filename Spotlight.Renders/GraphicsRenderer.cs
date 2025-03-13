using Spotlight.Abstractions;
using Spotlight.Models;
using Spotlight.Renderers;
using Spotlight.Services;

namespace Spotlight
{
    public class GraphicsRenderer : Renderer, IGraphicsRenderer
    {
        private byte[] _buffer;

        public GraphicsRenderer(IGraphicsManager graphicsManager) : base(graphicsManager)
        {
            _buffer = new byte[256 * 256 * BYTES_PER_BLOCK];
        }

        public void Update(Palette palette, int paletteIndex)
        {
            for (var i = 0; i < 256; i++)
            {
                int x = i % 16;
                int y = i / 16;
                RenderTile(x, y, _graphicsManager.GetRelativeTile(i), _buffer, palette.RgbColors[paletteIndex]);
            }
        }
    }
}