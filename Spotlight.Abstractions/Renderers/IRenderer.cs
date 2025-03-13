using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotlight.Abstractions
{
    public interface IRenderer
    {
        public bool RenderGrid { get; set; }
        public bool ScreenBorders { get; set; }
        public void Initializing();

        public void Ready();

        public byte[] GetRectangle(Rectangle rect, byte[] buffer);

        public int BytesPerPixel { get; }
        public int PixelsPerTile { get; }
        public int PixelsPerBlockPerRow { get; }
        public int TilesPerRow { get; }
        public int TilesPerColumn { get; }
        public int PixelsPerBlock { get; }
        public int BytesPerBlock { get; }
        public int BlocksPerScreen { get; }
        public int ScreensPerLevel { get; }
        public int ByteStride { get; }
    }
}
