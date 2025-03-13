using Spotlight.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotlight.Abstractions
{
    public interface ITileSetRenderer
    {
        public byte[] GetRectangle(Rectangle rect);
        public void Initialize(List<TileTerrain> terrain, List<MapTileInteraction> mapTileInteractions);
        public void Update(TileSet tileSet = null, Palette palette = null, bool? withTerrainOverlay = null, bool? withInteractionOverlay = null, bool? withMapInteractionOverlay = null, bool? withProjectileInteractions = null);
        public void Update();
    }
}
