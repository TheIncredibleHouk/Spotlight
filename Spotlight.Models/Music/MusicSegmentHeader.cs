using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotlight.Models.Music
{
    public class MusicSegmentHeader
    {
        public Tempo Tempo { get; set; }
        public UInt16 MusicSegmentAddress { get; set; }
        public byte TriangleOffset { get; set; }
        public byte Square1Offset { get; set; }
        public byte NoiseOffset { get; set; }
        public byte DMCOffset { get; set; }

    }
}
