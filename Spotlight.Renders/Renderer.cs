using Spotlight.Models;
using Spotlight.Services;
using System.Drawing;
using System.Windows;

namespace Spotlight.Renderers
{
    public class Renderer
    {
        public bool RenderGrid { get; set; }
        public bool ScreenBorders { get; set; }

        protected GraphicsAccessor _graphicsAccessor;

        public Renderer(GraphicsAccessor graphicsAccessor)
        {
            _graphicsAccessor = graphicsAccessor;
        }

        protected bool _initializing;

        public void Initializing()
        {
            _initializing = true;
        }

        public void Ready()
        {
            _initializing = false;
        }

        public byte[] GetRectangle(Rectangle rect, byte[] buffer)
        {
            byte[] copyData = new byte[rect.Width * rect.Height * 4];
            int copyDataPointer = 0;

            for (int y = rect.Y; y < rect.Height + rect.Y; y++)
            {
                int yOffset = y * BYTE_STRIDE;

                for (int x = rect.X; x < rect.Width + rect.X; x++)
                {
                    int xOffset = yOffset + (x * 4);

                    if (xOffset >= buffer.Length)
                    {
                        copyData[copyDataPointer++] = 0;
                        copyData[copyDataPointer++] = 0;
                        copyData[copyDataPointer++] = 0;
                        copyData[copyDataPointer++] = 0;
                    }
                    else
                    {
                        copyData[copyDataPointer++] = buffer[xOffset];
                        copyData[copyDataPointer++] = buffer[xOffset + 1];
                        copyData[copyDataPointer++] = buffer[xOffset + 2];
                        copyData[copyDataPointer++] = buffer[xOffset + 3];
                    }
                }
            }

            return copyData;
        }

        public const int BYTES_PER_PIXEL = 4;
        public const int PIXELS_PER_TILE = 8 * 8;
        public const int PIXELS_PER_BLOCK_ROW = 16;
        public const int TILES_PER_ROW = 16;
        public const int TILES_PER_COLUMN = 16;
        public const int PIXELS_PER_BLOCK = 16 * 16;
        public const int BYTES_PER_BLOCK = BYTES_PER_PIXEL * PIXELS_PER_BLOCK;
        public const int BLOCKS_PER_SCREEN = 16;
        public const int SCREENS_PER_LEVEL = 15;

        public int BYTE_STRIDE = BYTES_PER_PIXEL * PIXELS_PER_BLOCK_ROW * BLOCKS_PER_SCREEN * SCREENS_PER_LEVEL;

        protected void RenderTile(int x, int y, Tile tile, byte[] buffer, Color[] palette, bool horizontalFlip = false, bool verticalFlip = false, bool useTransparency = false, double opacity = 1)
        {
            int pixelXChange = horizontalFlip ? -1 : 1,
                pixelYChange = verticalFlip ? -1 : 1;

            for (int i = 0, pixelY = verticalFlip ? 7 : 0; i < 8; i++, pixelY += pixelYChange)
            {
                long yOffset = (BYTE_STRIDE * (y + i)) + (x * 4);

                for (int j = 0, pixelX = horizontalFlip ? 7 : 0; j < 8; j++, pixelX += pixelXChange)
                {
                    long xOffset = (j * 4) + yOffset;
                    int colorIndex = tile[pixelX, pixelY];

                    Color color = palette[colorIndex];
                    double calcOpacity = (double)(useTransparency && (colorIndex == 0) ? 0 : opacity);

                    if (xOffset >= 0 && xOffset < buffer.Length)
                    {
                        if (RenderGrid && ((j == 0 && x % 16 == 0) || (i == 0 && y % 16 == 0)))
                        {
                            color = (j + i) % 2 == 1 ? Color.White : Color.Black;
                        }

                        if (ScreenBorders)
                        {
                            if (x > 0)
                            {
                                if (xOffset % 1024 == 1023 || xOffset % 1024 == 0)
                                {
                                    color = (j + i) % 2 == 1 ? Color.Red : Color.Green;
                                }
                            }
                        }

                        buffer[xOffset] = (byte)((1 - calcOpacity) * buffer[xOffset] + (calcOpacity * color.B));
                        buffer[xOffset + 1] = (byte)((1 - calcOpacity) * buffer[xOffset + 1] + (calcOpacity * color.G));
                        buffer[xOffset + 2] = (byte)((1 - calcOpacity) * buffer[xOffset + 2] + (calcOpacity * color.R));
                        buffer[xOffset + 3] = 255;
                    }
                }
            }
        }

        protected void DrawColorTile(int x, int y, Color color, byte[] buffer)
        {
            for (int i = 0, pixelY = 0; i < 16; i++, pixelY++)
            {
                long yOffset = (BYTE_STRIDE * (y + i)) + (x * 4);

                for (int j = 0, pixelX = 0; j < 16; j++, pixelX++)
                {
                    long xOffset = (j * 4) + yOffset;

                    if (xOffset >= 0 && xOffset < buffer.Length)
                    {
                        buffer[xOffset] = color.B;
                        buffer[xOffset + 1] = color.G;
                        buffer[xOffset + 2] = color.R;
                        buffer[xOffset + 3] = 255;
                    }
                }
            }
        }

        protected void Clear(Color color, byte[] buffer)
        {
            for (int i = 0; i < buffer.Length; i += 4)
            {
                buffer[i] = (byte)color.B;
                buffer[i + 1] = (byte)color.G;
                buffer[i + 2] = (byte)color.R;
                buffer[i + 3] = (byte)color.A;
            }
        }
    }
}