using Spotlight.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotlight.Abstractions
{
    public interface IWorldRenderer : IRenderer
    {
        public void Initialize(List<MapTileInteraction> terrain);

        public void Update(Rectangle? rect = null, Palette palette = null, TileSet tileSet = null, bool? withInteractionOverlay = null, bool? withPointers = null);

        public byte[] GetRectangle(Rectangle rect);

        public void Update(Rectangle updateRect);

        public void Update();

        public void RenderTiles(int blockX = 0, int blockY = 0, int blockWidth = World.BLOCK_WIDTH, int blockHeight = World.BLOCK_HEIGHT);

        public void RenderObjects(int blockX = 0, int blockY = 0, int blockWidth = Level.BLOCK_WIDTH, int blockHeight = Level.BLOCK_HEIGHT, bool withOverlays = false);

        public void RenderPointers(int blockX = 0, int blockY = 0, int blockWidth = World.BLOCK_WIDTH, int blockHeight = World.BLOCK_HEIGHT);
    }
}
