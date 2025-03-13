using Spotlight.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotlight.Abstractions
{
    public interface ILevelRenderer : IRenderer
    {
        void Initialize(List<TileTerrain> terrain, Palette palette = null, TileSet tileSet = null);

        public void Update(Palette palette = null, TileSet tileSet = null);

        public byte[] GetRectangle(Rectangle rect);
        public void Update(Rectangle updateRect);

        public void Update(bool withSpriteOverlays, bool withTerrainOverlay, bool withInteractionOverlay, bool asStrategy);

        public void Update(Rectangle rect, bool withSpriteOverlays, bool withTerrainOverlay, bool withInteractionOverlay);

        public void Update(Rectangle rect, bool withSpriteOverlays, bool withTerrainOverlay, bool withInteractionOverlay, int? tileHighlight, bool asStrategy);

        public void RenderTiles(int blockX = 0, int blockY = 0, int blockWidth = Level.BLOCK_WIDTH, int blockHeight = Level.BLOCK_HEIGHT);

        public void RenderObjects(int blockX = 0, int blockY = 0, int blockWidth = Level.BLOCK_WIDTH, int blockHeight = Level.BLOCK_HEIGHT, bool withOverlays = false);

        public void RenderPointers(int blockX = 0, int blockY = 0, int blockWidth = Level.BLOCK_WIDTH, int blockHeight = Level.BLOCK_HEIGHT);
    }
}
