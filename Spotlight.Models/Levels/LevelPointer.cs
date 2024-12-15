using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Xml.Serialization;

namespace Spotlight.Models
{
    public class LevelPointer
    {
        public Guid LevelId { get; set; }
        public int ExitActionType { get; set; }
        public int ExitX { get; set; }
        public int ExitY { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public bool ExitsLevel { get; set; }
        public bool RedrawsLevel { get; set; }
        public bool KeepObjects { get; set; }
        public bool DisableWeather { get; set; }

        private static List<Sprite> _overlay = new List<Sprite>()
        {
            new Sprite(){ X = 0, Y = 0, Overlay = true, PaletteIndex = 2, TileValueIndex = 0, TileTableIndex = 4 },
            new Sprite(){ X = 8, Y = 0, Overlay = true, PaletteIndex = 2, TileValueIndex = 0x2, TileTableIndex = 4 },
            new Sprite(){ X = 16, Y = 0, Overlay = true, PaletteIndex = 2, HorizontalFlip = true, TileValueIndex = 0x2, TileTableIndex = 4 },
            new Sprite(){ X = 24, Y = 0, Overlay = true, PaletteIndex = 2, HorizontalFlip = true, TileValueIndex = 0x0, TileTableIndex = 4 },
            new Sprite(){ X = 0, Y = 16, Overlay = true, PaletteIndex = 2, VerticalFlip = true, TileValueIndex = 0, TileTableIndex = 4 },
            new Sprite(){ X = 8, Y = 16, Overlay = true, PaletteIndex = 2, VerticalFlip = true, TileValueIndex = 0x2, TileTableIndex = 4 },
            new Sprite(){ X = 16, Y = 16, Overlay = true, PaletteIndex = 2, HorizontalFlip = true, VerticalFlip = true, TileValueIndex = 0x2, TileTableIndex = 4 },
            new Sprite(){ X = 24, Y = 16, Overlay = true, PaletteIndex = 2, HorizontalFlip = true, VerticalFlip = true, TileValueIndex = 0x0, TileTableIndex = 4 },
            new Sprite(){ X = 8, Y = 8, Overlay = true, PaletteIndex = 1, HorizontalFlip = false, VerticalFlip = false, TileValueIndex = 0x4, TileTableIndex = 4 },
            new Sprite(){ X = 16, Y = 8, Overlay = true, PaletteIndex = 1, HorizontalFlip = false, VerticalFlip = false, TileValueIndex = 0x6, TileTableIndex = 4 }
        };

        public static List<Sprite> Overlay
        {
            get
            {
                return _overlay;
            }
        }

        public Rectangle BoundRectangle
        {
            get
            {
                return new Rectangle()
                {
                    X = X * 16,
                    Y = Y * 16,
                    Width = 32,
                    Height = 32
                };
            }
        }
    }
}