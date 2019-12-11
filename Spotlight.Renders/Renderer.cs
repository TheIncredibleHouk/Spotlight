using Spotlight.Models;
using Spotlight.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Spotlight.Renderers
{
    public class Renderer
    {

        protected GraphicsAccessor _graphicsAccessor;

        public Renderer(GraphicsAccessor graphicsAccessor)
        {
            _graphicsAccessor = graphicsAccessor;
        }


        public byte[] GetRectangle(Int32Rect rect, byte[] buffer)
        {
            byte[] copyData = new byte[rect.Width * rect.Height * 4];
            int copyDataPointer = 0;

            for (int y = rect.Y; y < rect.Height + rect.Y; y++)
            {
                int yOffset = y * BYTE_STRIDE;

                for (int x = rect.X; x < rect.Width + rect.X; x++)
                {
                    int xOffset = yOffset + (x * 4);

                    copyData[copyDataPointer++] = buffer[xOffset];
                    copyData[copyDataPointer++] = buffer[xOffset + 1];
                    copyData[copyDataPointer++] = buffer[xOffset + 2];
                    copyData[copyDataPointer++] = buffer[xOffset + 3];
                }
            }

            return copyData;
        }

        public const int BYTES_PER_PIXEL = 4;
        public const int PIXELS_PER_BLOCK = 16;
        public const int BYTES_PER_BLOCK = BYTES_PER_PIXEL * PIXELS_PER_BLOCK;
        public const int BLOCKS_PER_SCREEN = 16;
        public const int SCREENS_PER_LEVEL = 15;

        public const int BYTE_STRIDE = BYTES_PER_PIXEL * PIXELS_PER_BLOCK * BLOCKS_PER_SCREEN * SCREENS_PER_LEVEL;

        protected void RenderTile(int x, int y, Tile tile, int paletteIndex, byte[] buffer, Color[] palette, bool horizontalFlip = false, bool verticalFlip = false, bool useTransparency = false, double opacity = 1)
        {
            int pixelXChange = horizontalFlip ? -1 : 1,
                pixelYChange = verticalFlip ? -1 : 1;

            for (int i = 0, pixelY = verticalFlip ? 7 : 0; i < 8; i++, pixelY += pixelYChange)
            {
                long offset = (BYTE_STRIDE * (y + i)) + (x * 4);

                for (int j = 0, pixelX = horizontalFlip ? 7 : 0; j < 8; j++, pixelX += pixelXChange)
                {
                    long xOffset = (j * 4) + offset;
                    int colorIndex = tile[pixelX, pixelY];

                    Color color = palette[colorIndex];
                    double calcOpacity = (byte)(useTransparency && colorIndex == 0 ? 0 : opacity);

                    if (xOffset >= 0 && xOffset < buffer.Length)
                    {
                        buffer[xOffset] = (byte)((1 - calcOpacity) * buffer[xOffset] + (calcOpacity * color.B));
                        buffer[xOffset + 1] = (byte)((1 - calcOpacity) * buffer[xOffset + 1] + (calcOpacity * color.G));
                        buffer[xOffset + 2] = (byte)((1 - calcOpacity) * buffer[xOffset + 2] + (calcOpacity * color.R));
                        buffer[xOffset + 3] = 255;
                    }
                }
            }
        }

        protected void DrawHorizontalLine(int x, int y, int width, Color color, byte[] buffer)
        {
            for (int i = 0; i < width; x++, i++)
            {
                long xOffset = (BYTE_STRIDE * y) + (x * 4);

                if (xOffset >= 0 && xOffset < buffer.Length)
                {
                    buffer[xOffset] = (byte)color.B;
                    buffer[xOffset + 1] = (byte)color.G;
                    buffer[xOffset + 2] = (byte)color.R;
                    buffer[xOffset + 3] = 255;
                }
            }
        }

        protected void DrawVerticalLine(int x, int y, int height, Color color, byte[] buffer)
        {
            for (int i = 0; i < height; y++, i++)
            {
                long xOffset = (BYTE_STRIDE * y) + (x * 4);

                if (xOffset >= 0 && xOffset < buffer.Length)
                {
                    buffer[xOffset] = (byte)color.B;
                    buffer[xOffset + 1] = (byte)color.G;
                    buffer[xOffset + 2] = (byte)color.R;
                    buffer[xOffset + 3] = 255;
                }
            }
        }
    }
}
