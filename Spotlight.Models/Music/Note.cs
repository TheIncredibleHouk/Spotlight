using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace Spotlight.Models
{
    public class Note
    {
        public int Octave { get; set; }
        public string Name { get; set; }
        public int Value { get; set; }
        public string Display { get => (Octave > 0 ? Octave.ToString() : "") + ":" + Name; }

        public Note(int octave, string name, int value)
        {
            Octave = octave;
            Name = name;
            Value = value;
        }

        private static List<Note> _all = new List<Note>()
        {
            new Note(1, "C#", 0x1),
            new Note(1, "D", 0x2),
            new Note(1, "D#", 0x3),
            new Note(1, "E", 0x4),
            new Note(1, "F",0x5),
            new Note(1, "F#",0x6),
            new Note(1, "G",0x7),
            new Note(1, "G#",0x8),
            new Note(1, "A",0x9),
            new Note(1, "A#",0xA),
            new Note(1, "B",0xB),
            new Note(2, "C",0xC),
            new Note(2, "C#",0xD),
            new Note(2, "D",0xE),
            new Note(2, "D#",0XF),
            new Note(2, "E",0x10),
            new Note(2, "F",0x11),
            new Note(2, "F#",0x12),
            new Note(2, "G",0x13),
            new Note(2, "G#",0x14),
            new Note(2, "A",0x15),
            new Note(2, "A#",0x16),
            new Note(2, "B",0x17),
            new Note(3, "C",0x18),
            new Note(3, "C#",0x19),
            new Note(3, "D",0x1A),
            new Note(3, "D#",0x1B),
            new Note(3, "E",0x1C),
            new Note(3, "F",0x1D),
            new Note(3, "F#",0x1E),
            new Note(3, "G",0x1F),
            new Note(3, "G#",0x20),
            new Note(3, "A",0x21),
            new Note(3, "A#",0x22),
            new Note(3, "B",0x23),
            new Note(4, "C",0x24),
            new Note(4, "C#",0x25),
            new Note(4, "D",0x26),
            new Note(4, "D#",0x27),
            new Note(4, "E",0x28),
            new Note(4, "F",0x29),
            new Note(4, "F#",0x2A),
            new Note(4, "G",0x2B),
            new Note(4, "G#",0x2C),
            new Note(4, "A",0x2D),
            new Note(4, "A#",0x2E),
            new Note(4, "B",0x2F),
            new Note(5, "C",0x30),
            new Note(5, "C#",0x31),
            new Note(5, "D",0x32),
            new Note(5, "D#",0x33),
            new Note(5, "E",0x34),
            new Note(5, "F",0x35),
            new Note(5, "F#",0x36),
            new Note(5, "G",0x37),
            new Note(5, "G#",0x38),
            new Note(5, "A",0x39),
            new Note(5, "A#",0x3A),
            new Note(5, "B",0x3B),
            new Note(6, "C",0x3C),
            new Note(6, "C#",0x3D),
            new Note(6, "D",0x3E),
            new Note(0, "𝄽", 0x7E)
        };


        private static List<Note> _percussion = new List<Note>()
        {
            new Note(0, "Bass drum", 0x1),
            new Note(0, "Snare drum", 0x2),
            new Note(0, "Snare rim", 0x3),
            new Note(0, "Wood block", 0x5),
            new Note(0, "Bongo high", 0x6),
            new Note(0, "Bongo mid", 0x7),
            new Note(0, "Timbales high", 0x8),
            new Note(0, "Timbales low", 0x9),
            new Note(0, "Synth pad high", 0xA),
            new Note(0, "Synth pad low", 0xB),
            new Note(0, "Bongo low", 0xC),
            new Note(0, "Clap", 0xD),
            new Note(0, "Timpani high", 0xE),
            new Note(0, "Timpani mid", 0xF),
            new Note(0, "Timpani low", 0x10),
            new Note(0, "𝄽", 0x7E)
        };

        public static List<Note> SquareChannel1 { get => _all; }
        public static List<Note> SquareChannel2 { get => _all; }
        public static List<Note> Triangle { get => _all; }
        public static List<Note> Noise { get => _all; }
        public static List<Note> DMC { get => _percussion; }

        public static Note Rest { get => _all.Last(); }


        public static Note Parse(Channel channel, int value)
        {
            switch (channel)
            {
                case Channel.Square1:
                    return Note.SquareChannel1.Where(note => note.Value == value).FirstOrDefault();

                case Channel.Square2:
                    return Note.SquareChannel2.Where(note => note.Value == value).FirstOrDefault();

                case Channel.Triangle:
                    return Note.Triangle.Where(note => note.Value == value).FirstOrDefault();

                case Channel.Noise:
                    return Note.Noise.Where(note => note.Value == value).FirstOrDefault();

                case Channel.DMC:
                    return Note.DMC.Where(note => note.Value == value).FirstOrDefault();
            }

            return null;
        }
    }
}
