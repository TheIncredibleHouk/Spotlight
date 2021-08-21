using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Spotlight.Models
{
    public class World
    {
        public const int BLOCK_HEIGHT = 9;
        public const int BLOCK_WIDTH = 4 * 16;

        public World()
        {
            TileData = new int[0x40 * 0x1B];
            Pointers = new List<WorldPointer>();
            ObjectData = new List<WorldObject>();
        }

        public int TileSetIndex
        {
            get
            {
                return 0;
            }
        }

        public Guid Id { get; set; }
        public int ClearTileIndex { get; set; }
        public int TileTableIndex { get; set; }
        public int MusicValue { get; set; }
        public int ScreenLength { get; set; }
        public Guid PaletteId { get; set; }
        public string Name { get; set; }
        public int[] TileData { get; set; }
        public List<WorldPointer> Pointers { get; set; }
        public List<WorldObject> ObjectData { get; set; }

        public byte[] CompressedData { get; set; }

        [JsonIgnore]
        public int AnimationTileTableIndex
        {
            get { return 0x70; }
        }
    }
}