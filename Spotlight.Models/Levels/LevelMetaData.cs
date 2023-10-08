using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotlight.Models
{
    public class LevelMetaData
    {
        public List<string> UniqueGameObjects { get; set; }
        public List<string> PowerUps { get; set; }
        public int MaxCoinCount { get; set; }
        public int MaxCherryCount { get; set; }
        public byte[] ThumbnailImage { get; set; }
        public List<int> TilesUsed { get; set; }
        public int TileSet { get; set; }
    }
}
