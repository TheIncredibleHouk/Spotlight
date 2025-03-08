using Spotlight.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotlight.Abstractions
{
    public interface IGraphicsService
    {
        void CheckGraphics();
        Color[][] GetRgbPalette(Palette palette);
        Color[] GetRgbPalette(string[] paletteIndex);
        byte[] GetData();
        void LoadGraphics();
        void LoadExtraGraphics();
        Tile[] GetGlobalTiles();
        Tile[] GetExtraTiles();
        Tile[] GetExtraTilesAtAddress(int address);
        Tile GetExtraTileAtAddress(int address, int col, int row);
        Tile[] GetTileSection(int tileTableIndex);
        Tile GetTile(int tileTableIndex, int tileIndex);
        Palette GetPalette(int index);
        List<Color> GetColors();
        Tile[] GetTilesAtAddress(int address);
        Tile GetTileAtAddress(int address, int col, int row);

    }
}
