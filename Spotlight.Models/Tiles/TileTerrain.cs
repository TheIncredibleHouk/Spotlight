using System.Collections.Generic;

namespace Spotlight.Models
{
    public class TileTerrain : TileInteraction
    {
        public static byte Background = 0x00;
        public static byte Foreground = 0x10;
        public static byte Water = 0x20;
        public static byte WaterForeground = 0x30;
        public static byte SemiSolid = 0x40;
        public static byte HiddenItemBlock = 0x80;
        public static byte Solid = 0xC0;
        public static byte ItemBlock = 0xF0;
        public static new byte Mask = 0xF0;

        public List<TileInteraction> Interactions { get; set; }

        public bool HasTerrain(int property)
        {
            return Value == (int)(property & Mask);
        }

        public TileTerrain()
        {
            Interactions = new List<TileInteraction>();
        }
    }
}