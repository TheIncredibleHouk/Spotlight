using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotlight.Models
{
    public class TileBlock
    {
        public int Property { get; set; }
        public int UpperLeft { get; set; }
        public int UpperLeftPalette
        {
            get
            {
                return (UpperLeft >> 6) & 0x02;
            }
        }
        public int UpperRight { get; set; }
        public int UpperRightPalette
        {
            get
            {
                return (UpperLeft >> 6) & 0x02;
            }
        }
        public int LowerLeft { get; set; }
        public int LowerLeftPalette
        {
            get
            {
                return (UpperLeft >> 6) & 0x02;
            }
        }
        public int LowerRight { get; set; }
        public int LowerRightPalette
        {
            get
            {
                return (UpperLeft >> 6) & 0x02;
            }
        }
    }
}
