using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotlight.Models
{
    public class NoteEndCommand : NoteData
    {
        public NoteEndCommand(int value) : base(value)
        {
            Value = 0x0;
        }
    }
}
