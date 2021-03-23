using Spotlight.Models;
using Spotlight.Services;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;

namespace Spotlight.Renderers
{
    public class GameObjectRenderer : Renderer
    {
        private byte[] _buffer;
        private GameObjectService _gameObjectService;
        private PalettesService _palettesService;

        public GameObjectRenderer(GameObjectService gameObjectService, PalettesService palettesService, GraphicsAccessor graphicsAccessor) : base(graphicsAccessor)
        {
            BYTE_STRIDE = 256 * 4;
            _buffer = new byte[256 * 256 * 4];
            _gameObjectService = gameObjectService;
            _palettesService = palettesService;
        }

        private Color[][] _rgbPalette;

        public void Update(Palette palette)
        {
            _rgbPalette = palette.RgbColors;
            if (_buffer != null)
            {
                Update(_lastObjects, _lastWithOverlays);
            }
        }

        public void Update()
        {
            Update(_lastObjects, _lastWithOverlays);
        }

        public byte[] GetRectangle(Int32Rect rect)
        {
            return GetRectangle(rect, _buffer);
        }

        private List<LevelObject> _lastObjects;

        public void Clear()
        {
            for (int y = 0; y < 256; y++)
            {
                long yOffset = (256 * 4 * y);

                for (int x = 0; x < 256; x++)
                {
                    long xOffset = (x * 4) + yOffset;
                    Color color = _rgbPalette[0][0];

                    if (xOffset >= 0 && xOffset < _buffer.Length)
                    {
                        if (RenderGrid && (x % 16 == 0 || y % 16 == 0))
                        {
                            color = (y + x) % 2 == 1 ? Color.White : Color.Black;
                        }

                        _buffer[xOffset] = (byte)color.B;
                        _buffer[xOffset + 1] = (byte)color.G;
                        _buffer[xOffset + 2] = (byte)color.R;
                        _buffer[xOffset + 3] = 255;
                    }
                }
            }
        }

        private bool _lastWithOverlays;

        public void Update(List<LevelObject> _levelObjects, bool withOverlays)
        {
            if (_levelObjects == null)
            {
                return;
            }

            _lastObjects = _levelObjects;
            _lastWithOverlays = withOverlays;

            foreach (var levelObject in _levelObjects)
            {
                int baseX = levelObject.X * 16, baseY = levelObject.Y * 16;
                var visibleSprites = levelObject.GameObject.Sprites.Where(s => s.PropertiesAppliedTo == null ? true : s.PropertiesAppliedTo.Contains(levelObject.Property)).Where(s => withOverlays ? true : !s.Overlay).ToList();

                if (visibleSprites.Count == 0)
                {
                    visibleSprites = _gameObjectService.FallBackSprites(levelObject.GameObject);
                }

                foreach (var sprite in visibleSprites)
                {
                    int paletteIndex = sprite.PaletteIndex;

                    Tile topTile = sprite.Overlay ? _graphicsAccessor.GetOverlayTile(sprite.TileTableIndex, sprite.TileValueIndex) : _graphicsAccessor.GetAbsoluteTile(sprite.TileTableIndex, sprite.TileValueIndex);
                    Tile bottomTile = sprite.Overlay ? _graphicsAccessor.GetOverlayTile(sprite.TileTableIndex, sprite.TileValueIndex + 1) : _graphicsAccessor.GetAbsoluteTile(sprite.TileTableIndex, sprite.TileValueIndex + 1);
                    int x = baseX + sprite.X, y = baseY + sprite.Y;

                    RenderTile(x, y, sprite.VerticalFlip ? bottomTile : topTile, _buffer, sprite.CustomPalette != null ? _palettesService.GetRgbPalette(sprite.CustomPalette) : _rgbPalette[paletteIndex + 4], sprite.HorizontalFlip, sprite.VerticalFlip, true);
                    RenderTile(x, y + 8, sprite.VerticalFlip ? topTile : bottomTile, _buffer, sprite.CustomPalette != null ? _palettesService.GetRgbPalette(sprite.CustomPalette) : _rgbPalette[paletteIndex + 4], sprite.HorizontalFlip, sprite.VerticalFlip, true);
                }
            }
        }
    }
}