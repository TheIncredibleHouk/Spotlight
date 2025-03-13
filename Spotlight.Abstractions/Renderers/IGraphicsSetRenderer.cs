using Spotlight.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotlight.Abstractions
{
    public interface IGraphicsSetRenderer
    {
        public byte[] GetRectangle(Rectangle rect);

        public void Update(Palette palette = null, int? paletteIndex = null, TileFormat? tileFormat = null);
        public void Update();
        public Tile GetMappedTile(int col, int row);
    }
}
