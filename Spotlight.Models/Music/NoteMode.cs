using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotlight.Models
{
    public enum NoteMode
    {
        Whole = 0xC,
        Half = 0xA,
        DottedHalf = 0x8,
        DottedQuarter = 0xB,
        DottedEight = 0x5,
        Quarter = 0x8,
        Eigth = 0x4,
        Sixteenth = 0x1,
        QuarterTriplet = 0x6,
        EightTriplet = 0x2
    }
}
