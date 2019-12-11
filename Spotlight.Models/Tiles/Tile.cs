using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotlight.Models
{
    public class Tile
    {
        private byte[,] _pixels;

        public Tile(byte[] pixelData)
        {
            _pixels = new byte[8, 8];
            byte leftBit, rightBit;

            BitArray leftBitPlane = new BitArray(new byte[] { pixelData[0], pixelData[1], pixelData[2], pixelData[3], pixelData[4], pixelData[5], pixelData[6], pixelData[7] });
            BitArray rightBitPlane = new BitArray(new byte[] { pixelData[8], pixelData[9], pixelData[10], pixelData[11], pixelData[12], pixelData[13], pixelData[14], pixelData[15] });

            for (int j = 0; j < 8; j++)
            {
                for (int k = 0; k < 8; k++)
                {
                    rightBit = (byte)(leftBitPlane[(j * 8) + k] ? 0x01 : 0x00);
                    leftBit = (byte)(rightBitPlane[(j * 8) + k] ? 0x01 : 0x00);

                    _pixels[7 - k, j] = (byte)((leftBit << 1) | rightBit);
                }
            }
        }

        public byte this[int x, int y]
        {
            get { return _pixels[x, y]; }
            set
            {
                _pixels[x, y] = value;
            }
        }
    }
}
