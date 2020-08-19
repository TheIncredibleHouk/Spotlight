using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotlight.Models
{
    public class TileBlockOverlay
    {
        public int PaletteIndex { get; set; }

        public string UpperLeftTile { get; set; }

        [JsonIgnore]
        public int UpperLeft
        {
            get { return Int32.Parse(UpperLeftTile, System.Globalization.NumberStyles.HexNumber); }
            set { UpperLeftTile = value.ToString("X"); }
        }

        public string UpperRightTile { get; set; }

        [JsonIgnore]
        public int UpperRight
        {
            get { return Int32.Parse(UpperRightTile, System.Globalization.NumberStyles.HexNumber); }
            set { UpperRightTile = value.ToString("X"); }
        }
        public string LowerLeftTile { get; set; }

        [JsonIgnore]
        public int LowerLeft
        {
            get { return Int32.Parse(LowerLeftTile, System.Globalization.NumberStyles.HexNumber); }
            set { LowerLeftTile = value.ToString("X"); }
        }
        public string LowerRightTile { get; set; }

        [JsonIgnore]
        public int LowerRight
        {
            get { return Int32.Parse(LowerRightTile, System.Globalization.NumberStyles.HexNumber); }
            set { LowerRightTile = value.ToString("X"); }
        }
    }
}
