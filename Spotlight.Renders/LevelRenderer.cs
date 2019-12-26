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
    public class LevelRenderer : Renderer
    {

        public const int BITMAP_HEIGHT = Level.BLOCK_HEIGHT * 16;
        public const int BITMAP_WIDTH = Level.BLOCK_WIDTH * 16;

        private byte[] _BackgroundLayer;

        private LevelDataAccessor _levelDataAccessor;

        public LevelRenderer(GraphicsAccessor graphicsAccessor, LevelDataAccessor levelDataAccessor) : base(graphicsAccessor)
        {
            _levelDataAccessor = levelDataAccessor;

            _BackgroundLayer = new byte[BITMAP_WIDTH * BITMAP_HEIGHT * BYTES_PER_BLOCK];
        }



        private TileSet _tileSet;
        public void SetTileSet(TileSet tileSet)
        {
            _tileSet = tileSet;
            if (_rgbPalette != null)
            {
                RenderTiles();
            }

        }

        private Color[][] _rgbPalette;
        public void SetPalette(Palette palette)
        {
            _rgbPalette = palette.RgbColors;
            if (_tileSet != null)
            {
                RenderTiles();
            }
        }

        public byte[] GetRectangle(Int32Rect rect)
        {
            return GetRectangle(rect, _BackgroundLayer);
        }

        public void Update()
        {
            Update(new Rect(0, 0, BITMAP_WIDTH, BITMAP_HEIGHT));
        }

        public void Update(Int32Rect rect)
        {
            Update(new Rect(rect.X, rect.Y, rect.Width, rect.Height));
        }

        public void Update(Rect updateRect)
        {
            int blockX = (int)(updateRect.X / 16),
                blockY = (int)(updateRect.Y / 16),
                blockWidth = (int)((updateRect.X + updateRect.Width) / 16) + 2,
                blockHeight = (int)((updateRect.Y + updateRect.Height) / 16) + 2;

            RenderTiles(blockX, blockY, blockWidth, blockHeight);
            RenderObjects(blockX, blockY, blockWidth, blockHeight);
        }

        public void RenderTiles(int blockX = 0, int blockY = 0, int blockWidth = Level.BLOCK_WIDTH, int blockHeight = Level.BLOCK_HEIGHT)
        {
            int maxRow = blockY + blockHeight;
            int maxCol = blockX + blockWidth;

            if(maxRow > Level.BLOCK_HEIGHT)
            {
                maxRow = Level.BLOCK_HEIGHT;
            }

            if(maxCol > Level.BLOCK_WIDTH)
            {
                maxCol = Level.BLOCK_WIDTH;
            }

            for (int row = blockY; row < maxRow; row++)
            {
                for (int col = blockX; col < maxCol; col++)
                {

                    int tileValue = _levelDataAccessor.GetData(col, row);
                    int PaletteIndex = tileValue / 0x40;

                    TileBlock tile = _tileSet.Tiles[tileValue];
                    int paletteIndex = (tileValue & 0XC0) >> 6;
                    int x = col * 16, y = row * 16;

                    RenderTile(x, y, _graphicsAccessor.GetRelativeTile(tile.UpperLeft), tile.UpperLeftPalette, _BackgroundLayer, _rgbPalette[paletteIndex]);
                    RenderTile(x + 8, y, _graphicsAccessor.GetRelativeTile(tile.UpperRight), tile.UpperRightPalette, _BackgroundLayer, _rgbPalette[paletteIndex]);
                    RenderTile(x, y + 8, _graphicsAccessor.GetRelativeTile(tile.LowerLeft), tile.LowerLeftPalette, _BackgroundLayer, _rgbPalette[paletteIndex]);
                    RenderTile(x + 8, y + 8, _graphicsAccessor.GetRelativeTile(tile.LowerRight), tile.LowerRightPalette, _BackgroundLayer, _rgbPalette[paletteIndex]);
                }
            }
        }

        public void RenderObjects(int blockX = 0, int blockY = 0, int blockWidth = Level.BLOCK_WIDTH, int blockHeight = Level.BLOCK_HEIGHT)
        {
            Rect updateReact = new Rect(blockX * 16, blockY * 16, blockWidth * 16, blockHeight * 16);

            foreach (var levelObject in _levelDataAccessor.GetLevelObjects().Where(o => o.BoundRectangle.IntersectsWith(updateReact)).ToList())
            {
                int baseX = levelObject.X * 16, baseY = levelObject.Y * 16;
                foreach (var sprite in levelObject.GameObject.Sprites.Where(s => s.PropertiesAppliedTo == null ? true : s.PropertiesAppliedTo.Contains(levelObject.Property)))
                {
                    if (sprite.Overlay)
                    {
                        continue;
                    }

                    int tileValue = sprite.TileValue;
                    int paletteIndex = sprite.PaletteIndex;

                    Tile topTile = _graphicsAccessor.GetAbsoluteTile(sprite.TileTableIndex, sprite.TileValue);
                    Tile bottomTile = _graphicsAccessor.GetAbsoluteTile(sprite.TileTableIndex, sprite.TileValue + 1);
                    int x = baseX + sprite.X, y = baseY + sprite.Y;

                    RenderTile(x, y, sprite.VerticalFlip ? bottomTile : topTile, paletteIndex, _BackgroundLayer, _rgbPalette[paletteIndex + 4], sprite.HorizontalFlip, sprite.VerticalFlip, true);
                    RenderTile(x, y + 8, sprite.VerticalFlip ? topTile : bottomTile, paletteIndex, _BackgroundLayer, _rgbPalette[paletteIndex + 4], sprite.HorizontalFlip, sprite.VerticalFlip, true);
                }
            }
        }
    }
}
