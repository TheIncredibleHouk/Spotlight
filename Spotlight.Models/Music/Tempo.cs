using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotlight.Models
{
    public class Tempo
    {
        public string Name { get; set; }
        public int Value { get; set; }
        public Tempo(string name, int value)
        {
            Name = name;
            Value = value;
        }


        private static List<Tempo> _all = new List<Tempo>()
        {
            new Tempo("112.5 BPM", 0x0),
            new Tempo("120 BPM", 0x10),
            new Tempo("128.6 BPM", 0x20),
            new Tempo("150 BPM", 0x30),
            new Tempo("180 BPM", 0x40),
            new Tempo("200 BPM", 0x50),
            new Tempo("225 BPM", 0x60),
            new Tempo("257.1 BPM", 0x70),
            new Tempo("300 BPM", 0x80),
            new Tempo("450 BPM", 0x90)
        };

        public static List<Tempo> All { get => _all; }
    }
}
