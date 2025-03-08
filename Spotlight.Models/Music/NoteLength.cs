using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotlight.Models
{
    public enum NoteLength
    {
        Whole = 0xC,
        Half = 0xA,
        DottedHalf = 0x8,
        DottedQuarter = 0xB,
        DottedEighth = 0x5,
        Quarter = 0x8,
        Eighth = 0x4,
        Sixteenth = 0x1,
        EigthTriplet = 0x6,
        SixteenthTriplet = 0x2,
        Invalid = 0
    }

    //public static class NoteLengthExtensions
    //{
    //    public static int MaxNotesPerMeasure(this NoteLength length)
    //    {
    //        switch (length)
    //        {
    //            case NoteLength.Whole: return 1;

    //            case NoteLength.Half: return 2;

    //            case NoteLength.Quarter: return 4;

    //            case NoteLength.Eighth: return 8;

    //            case NoteLength.Sixteenth: return 16;

    //            case NoteLength.EigthTriplet: return 3;

    //            case NoteLength.SixteenthTriplet: return 6;
    //        }

    //        return 1;
    //    }
    //}
}
