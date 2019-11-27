using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotlight.Models
{
    public class PSwitchAlteration
    {
        public PSwitchAlteration(int fromTile, int toTile)
        {
            From = fromTile;
            To = toTile;
        }

        public int From { get; set; }
        public int To { get; set; }
    }
}
