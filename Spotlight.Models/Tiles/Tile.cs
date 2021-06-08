using System.Collections;

namespace Spotlight.Models
{
    public class Tile
    {
        private byte[,] _pixels;
        public Tile()
        {
            _pixels = new byte[8, 8];
            for (int j = 0; j < 8; j++)
            {
                for (int k = 0; k < 8; k++)
                {
                    _pixels[j, k] = (byte)0;
                }
            }
        }


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

        public void ApplyTile(Tile tile)
        {
            for(int x = 0; x < 8; x++)
            {
                for(int y = 0; y < 8; y++)
                {
                    _pixels[x, y] = tile[x, y];
                }
            }
        }

        public byte[] GetData()
        {
            byte[] outputData = new byte[16];
            byte currentByteData = (byte) 0;
            int byteIndex = 0, outputIndex = 0;

            for(int x = 0; x < 8; x++)
            {
                for(int y = 0; y < 8; y++)
                {
                    currentByteData = (byte) ((currentByteData | _pixels[x, y]) << 2);
                    byteIndex++;

                    if (byteIndex == 4)
                    {
                        outputData[outputIndex] = currentByteData;
                        outputIndex++;
                        byteIndex = 0;
                    }
                }
            }

            return outputData;
        }

        public byte this[int x, int y]
        {
            get
            {
                return _pixels[x, y];
            }
            set
            {
                _pixels[x, y] = value;
            }
        }
    }
}