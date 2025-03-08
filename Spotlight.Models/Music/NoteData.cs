using Spotlight.Models.Music;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotlight.Models
{
    public class NoteData
    {
        public int Value { get; set; }
        public NoteData(int value)
        {
            Value = value;
        }

        public bool IsEnd()
        {
            return Value == 0;
        }

        public bool IsProperty()
        {
            return (Value & 0x80) > 0;
        }

        public bool IsValue()
        {
            return Value > 1 && Value < 0x80;
        }

        public bool IsPortmanto(Channel channel)
        {
            if (channel == Channel.Square1 || channel == Channel.Square2)
            {
                return Value == 0xFF;
            }

            return false;
        }

        public NotePropertyCommand AsPropertyCommand()
        {
            return new NotePropertyCommand(Value);
        }

        public NotePortamantoCommand AsNotePortmantoCommand()
        {
            return new NotePortamantoCommand(Value);
        }

        public NoteEndCommand AsNoteEndCommand()
        {
            return new NoteEndCommand(Value);
        }

        public NoteValue AsNoteValue(Channel channel)
        {
            return new NoteValue(Value, channel);
        }
    }
   
}
