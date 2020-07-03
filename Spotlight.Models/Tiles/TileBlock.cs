using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotlight.Models
{
    public class TileBlock
    {
        public int Property { get; set; }
        public int UpperLeft { get; set; }
 
        public int UpperRight { get; set; }
       
        public int LowerLeft { get; set; }
        
        public int LowerRight { get; set; }
        
    }
}
