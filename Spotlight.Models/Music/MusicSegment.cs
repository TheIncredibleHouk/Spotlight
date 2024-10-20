using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Spotlight.Models.Music
{
    public class MusicSegment
    {
        public UInt16 Address { get; set; }
        public byte[] SquareChannel2 { get; set; }
        public byte[] TriangeChannel { get; set; }
        public byte[] Square1Channel { get; set; }
        public byte[] NoiseChannel { get; set; }
        public byte[] DMC {  get; set; }
    }
}
