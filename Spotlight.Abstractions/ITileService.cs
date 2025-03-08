using Spotlight.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotlight.Abstractions
{
    public interface ITileService
    {
        IEnumerable<TileSet> GetTileSets();
        void CommitTileSet(int index, TileSet tileSet, List<TileTerrain> tileTerrain, List<MapTileInteraction> mapTileInterations);
        byte[] GetTilePropertyData();
        TileSet GetTileSet(int tileSetIndex);
        List<TileTerrain> GetTerrain();
        List<TileTerrain> GetTerrainCopy();
        List<MapTileInteraction> GetMapTileInteractions();
        List<MapTileInteraction> GetMapTileInteractionCopy();

    }
}
