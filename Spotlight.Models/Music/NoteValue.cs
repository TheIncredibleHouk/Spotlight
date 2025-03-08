using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotlight.Models
{
    public class NoteValue : NoteData
    {
        public Channel Channel { get; set; }
        public bool Portmanto { get; set; }

        public NoteValue(int value, Channel channel) : base(value)
        {
            Value = value;
            Channel = channel;
        }

        public Note Note
        {
            get
            {
                switch (Channel)
                {
                    case Channel.Square1:
                        return Note.SquareChannel1.Where(note => note.Value == Value).FirstOrDefault();

                    case Channel.Square2:
                        return Note.SquareChannel2.Where(note => note.Value == Value).FirstOrDefault();

                    case Channel.Triangle:
                        return Note.Triangle.Where(note => note.Value == Value).FirstOrDefault();

                    case Channel.Noise:
                        return Note.Noise.Where(note => note.Value == Value).FirstOrDefault();

                    case Channel.DMC:
                        return Note.DMC.Where(note => note.Value == Value).FirstOrDefault();
                }


                return null;
            }
        }
    }
}
