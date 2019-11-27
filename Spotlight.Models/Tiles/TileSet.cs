using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotlight.Models
{
    public class TileSet
    {
        public TileSet()
        {
            FireBallInteractions = new List<int>();
            IceBallInteractions = new List<int>();
            PSwitchAlterations = new List<PSwitchAlteration>();
            Tiles = new List<Tile>();
        }

        public List<int> FireBallInteractions { get; set; }
        public List<int> IceBallInteractions { get; set; }
        public List<PSwitchAlteration> PSwitchAlterations { get; set; }
        public List<Tile> Tiles { get; set; }
    }
}
