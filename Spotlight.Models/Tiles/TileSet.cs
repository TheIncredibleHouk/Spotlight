using System.Collections.Generic;

namespace Spotlight.Models
{
    public class TileSet
    {
        public TileSet()
        {
            FireBallInteractions = new List<int>();
            IceBallInteractions = new List<int>();
            PSwitchAlterations = new List<PSwitchAlteration>();
            TileBlocks = new TileBlock[256];
        }

        public List<int> FireBallInteractions { get; set; }
        public List<int> IceBallInteractions { get; set; }
        public List<PSwitchAlteration> PSwitchAlterations { get; set; }
        public TileBlock[] TileBlocks { get; set; }

        public byte[] GetTileData()
        {
            byte[] returnData = new byte[0x400];

            for (int i = 0; i < 256; i++)
            {
                returnData[i] = (byte)TileBlocks[i].UpperLeft;
                returnData[i + 0x100] = (byte)TileBlocks[i].LowerLeft;
                returnData[i + 0x200] = (byte)TileBlocks[i].UpperRight;
                returnData[i + 0x300] = (byte)TileBlocks[i].LowerRight;
            }

            return returnData;
        }
    }
}