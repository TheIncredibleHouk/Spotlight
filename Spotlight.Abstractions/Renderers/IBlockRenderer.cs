using Spotlight.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotlight.Abstractions
{
    public interface IBlockRenderer
    {
        public void Update(Tile[,] tiles = null, Palette palette = null, int? paletteIndex = null);

        public byte[] GetRectangle();

        public void SetPixel(int x, int y, int value);

        public void Update();
    }
}
