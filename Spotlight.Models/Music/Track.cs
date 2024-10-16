using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotlight.Models.Music
{
    public class Track
    {
        public byte FirstBlockIndex { get; set; }
        public byte LastBlockIndex { get; set; }
        public byte LoopBlockIndex { get; set; }
        public byte MusicBank { get; set; }
    }
}
