using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotlight.Models
{
    public class TileInteraction
    {
        public string Name { get;  set; }
        public int Value { get; set; }
        public TileBlock[] Overlays { get; set; }

        public TileInteraction()
        {

        }

    }
}
