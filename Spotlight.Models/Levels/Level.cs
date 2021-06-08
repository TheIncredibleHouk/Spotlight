using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Spotlight.Models
{
    public class Level
    {
        public const int BLOCK_HEIGHT = 27;
        public const int BLOCK_WIDTH = 15 * 16;

        public Level()
        {
            TileData = new int[240 * 27];
            LevelPointers = new List<LevelPointer>();
            ObjectData = new List<LevelObject>();
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public int TileSetIndex { get; set; }
        public int ClearTileIndex { get; set; }
        public int MusicValue { get; set; }
        public int ScreenLength { get; set; }
        public int StartX { get; set; }
        public int StartY { get; set; }
        public int PaletteEffect { get; set; }
        public Guid PaletteId { get; set; }
        public int StaticTileTableIndex { get; set; }

        public int MostCommonTile { get; set; }

        [JsonIgnore]
        public int AnimationTileTableIndex
        {
            get
            {
                switch (AnimationType)
                {
                    case 1:
                        return 0xD0;

                    case 2:
                        return 0xF0;

                    case 3:
                        return 0x5C;

                    default:
                        return 0x80;
                }
            }
        }

        [JsonIgnore]
        public int PSwitchAnimationTileTableIndex
        {
            get
            {
                switch (AnimationType)
                {
                    case 1:
                        return 0xE0;

                    case 2:
                        return 0xF0;

                    case 3:
                        return 0x5C;

                    default:
                        return 0xC0;
                }
            }
        }

        public int AnimationType { get; set; }
        public int ScrollType { get; set; }
        public int Effects { get; set; }
        public int EventType { get; set; }
        public int[] TileData { get; set; }
        public List<LevelObject> ObjectData { get; set; }
        public List<LevelPointer> LevelPointers { get; set; }
        public byte[] CompressedData { get; set; }
    }
}