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
    public class GameObjectRenderer : Renderer
    {

        public const int BITMAP_HEIGHT = Level.BLOCK_HEIGHT * 16;
        public const int BITMAP_WIDTH = Level.BLOCK_WIDTH * 16;

        private byte[] _BackgroundLayer;

        private GameObjectService _gameObjectService;
        private List<LevelObject> _levelObjects;

        public GameObjectRenderer(GraphicsAccessor graphicsAccessor, GameObjectService gameObjectService, List<LevelObject> levelObjects) : base(graphicsAccessor)
        {
            _gameObjectService = gameObjectService;
            _levelObjects = levelObjects;
        }

        public void Refresh()
        {
            _BackgroundLayer = new byte[BITMAP_WIDTH * BITMAP_HEIGHT * BYTES_PER_BLOCK];
        }

        private Color[][] _rgbPalette;
        public void SetPalette(Palette palette)
        {
            _rgbPalette = palette.RgbColors;
            if (_levelObjects != null)
            {
                Update();
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

            RenderObjects(blockX, blockY, blockWidth, blockHeight);
        }

        public void RenderObjects(int blockX = 0, int blockY = 0, int blockWidth = Level.BLOCK_WIDTH, int blockHeight = Level.BLOCK_HEIGHT)
        {
            Rect updateReact = new Rect(blockX * 16, blockY * 16, blockWidth * 16, blockHeight * 16);

            foreach (var levelObject in _levelObjects)
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
