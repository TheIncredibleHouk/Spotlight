using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotlight.Models.Music
{
    public class TrackDefinition
    {
        // 0x01 - 0x0F and 0x10 - 0xF0
        public int MusicTrackId { get; set; }  
        public byte StartMusicSegmentHeaderIndex { get; set; } // points to MusicSegmentHeaderTable
        public byte EndMusicSegmentHeaderIndex { get; set; } // points to MusicSegmentHeaderTable
        public byte LoopMusicSegmentHeaderIndex { get; set; } // points to MusicSegmentHeaderTable
    }
}
