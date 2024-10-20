using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotlight.Models.Music
{
    public class Note
    {
        public NoteLength Length { get; set; }
        public bool Dotted { get; set; }
        public bool Triplet { get; set; }
        public byte Timber { get; set; }

        public byte Value
        {
            get
            {
                byte length = 0;
                if (Dotted)
                {
                    switch (Length)
                    {
                        case NoteLength.Half:
                            length = 0x0B;
                            break;

                        case NoteLength.Quarter:
                            length = 0x09;
                            break;

                        case NoteLength.Eighth:
                            length = 0x05;
                            break;
                    }
                }
                else if (Triplet)
                {
                    switch (Length)
                    {
                        case NoteLength.Quarter:
                            length = 0x06;
                            break;

                        case NoteLength.Eighth:
                            length = 0x02;
                            break;
                    }
                }
                else
                {
                    switch (Length)
                    {
                        case NoteLength.Whole:
                            length = 0x0C;
                            break;

                        case NoteLength.Half:
                            length = 0x0A;
                            break;

                        case NoteLength.Quarter:
                            length = 0x08;
                            break;

                        case NoteLength.Eighth:
                            length = 0x04;
                            break;

                        case NoteLength.Sixteenth:
                            length = 0x01;
                            break;
                    }
                }

                return (byte)((Timber << 4) | length);
            }
        }
    }
}
