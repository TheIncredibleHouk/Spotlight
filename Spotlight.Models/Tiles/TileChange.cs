using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotlight.Models
{
    public class TileChange
    {
        public TileChange(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Data = new int[width, height];
        }

        public int X { get; set; }
        public int Y { get; set; }
        public int Width
        {
            get
            {
                return Data.GetLength(0);
            }
        }
        public int Height
        {
            get
            {
                return Data.GetLength(1);
            }
        }
        public int[,] Data { get; private set; }
    }
}
