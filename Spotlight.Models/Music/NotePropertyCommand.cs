using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotlight.Models.Music
{
    public class NotePropertyCommand : NoteData
    {
        public NotePropertyCommand(int value) : base(value)
        {
            Value = value;
        }

        public NoteLength NoteLength { get => (NoteLength)(Value & 0x0F); }
        public int Timbre { get => (int)((Value & 0x70) >> 4); }
    }
}
