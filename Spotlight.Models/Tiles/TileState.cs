using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotlight.Models
{
    public class TileState
    {

        public string Name { get; set; }
        public int Value { get; set; }

        public static byte Background = 0x00;
        public static byte Foreground = 0x10;
        public static byte Water = 0x20;
        public static byte WaterForeground = 0x30;
        public static byte SemiSolid = 0x40;
        public static byte HiddenItemBlock = 0x80;
        public static byte Solid = 0xC0;
        public static byte ItemBlock = 0xF0;

        public TileState(string name, int value)
        {
            Name = name;
            Value = value;
        }
    }
}
