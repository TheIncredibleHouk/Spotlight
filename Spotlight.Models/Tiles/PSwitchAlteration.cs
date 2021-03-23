using System;
using System.Collections.Generic;

namespace Spotlight.Models
{
    public class PSwitchAlteration
    {
        public PSwitchAlteration()
        {
        }

        public PSwitchAlteration(int fromTile, int toTile)
        {
            From = fromTile;
            To = toTile;
        }

        public int From { get; set; }
        public int To { get; set; }

        private static Dictionary<char, int> hexToSprite = new Dictionary<char, int>()
        {
            {'1', Int32.Parse("20", System.Globalization.NumberStyles.HexNumber) },
            {'2', Int32.Parse("22", System.Globalization.NumberStyles.HexNumber) },
            {'3', Int32.Parse("24", System.Globalization.NumberStyles.HexNumber) },
            {'4', Int32.Parse("26", System.Globalization.NumberStyles.HexNumber) },
            {'5', Int32.Parse("28", System.Globalization.NumberStyles.HexNumber) },
            {'6', Int32.Parse("40", System.Globalization.NumberStyles.HexNumber) },
            {'7', Int32.Parse("42", System.Globalization.NumberStyles.HexNumber) },
            {'8', Int32.Parse("44", System.Globalization.NumberStyles.HexNumber) },
            {'9', Int32.Parse("46", System.Globalization.NumberStyles.HexNumber) },
            {'0', Int32.Parse("48", System.Globalization.NumberStyles.HexNumber) },
            {'A', Int32.Parse("60", System.Globalization.NumberStyles.HexNumber) },
            {'B', Int32.Parse("62", System.Globalization.NumberStyles.HexNumber) },
            {'C', Int32.Parse("64", System.Globalization.NumberStyles.HexNumber) },
            {'D', Int32.Parse("66", System.Globalization.NumberStyles.HexNumber) },
            {'E', Int32.Parse("68", System.Globalization.NumberStyles.HexNumber) },
            {'F', Int32.Parse("6A", System.Globalization.NumberStyles.HexNumber) }
        };

        public static TileBlock GetAlterationBlocks(int tileValue)
        {
            TileBlock tileBlock = new TileBlock();
            string tileString = tileValue.ToString("X2");

            tileBlock.UpperLeft = hexToSprite[tileString[0]];
            tileBlock.LowerLeft = tileBlock.UpperLeft + 1;

            tileBlock.UpperRight = hexToSprite[tileString[1]];
            tileBlock.LowerRight = tileBlock.UpperRight + 1;
            return tileBlock;
        }
    }
}