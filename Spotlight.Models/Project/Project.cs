using System.Collections.Generic;
using System.Drawing;
using System.Xml.Serialization;

namespace Spotlight.Models
{
    public class Project
    {
        public Project()
        {
          
        }

        public string Name { get; set; }
        public string DirectoryPath { get; set; }
        public List<Palette> Palettes { get; set; }
        public List<WorldInfo> WorldInfo { get; set; }
        public WorldInfo EmptyWorld { get; set; }
        public List<TileTerrain> TileTerrain { get; set; }
        public List<MapTileInteraction> MapTileInteractions { get; set; }
        public List<TileSet> TileSets { get; set; }
        public GameObject[] GameObjects { get; set; }
        public Dictionary<string, List<KeyValuePair<string, string>>> TextTable { get; set; }
        public Color[] RgbPalette { get; set; }
    }
}