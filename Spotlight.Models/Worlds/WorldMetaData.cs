using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotlight.Models
{
    public class WorldMetaData
    {
        public byte[] ThumbnailImage { get; set; }
        public List<int> TilesUsed { get; set; }
    }
}
