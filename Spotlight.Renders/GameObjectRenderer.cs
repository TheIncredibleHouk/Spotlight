using Spotlight.Models;
using Spotlight.Services;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using Spotlight.Abstractions;

namespace Spotlight.Renderers
{
    public class GameObjectRenderer : Renderer, IGameObjectRenderer
    {
        private byte[] _buffer;
        private IGameObjectService _gameObjectService;
        private IPaletteService _palettesService;

        public GameObjectRenderer(IGameObjectService gameObjectService, IPaletteService palettesService, IGraphicsManager graphicsManager) : base(graphicsManager)
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

        public byte[] GetRectangle(Rectangle rect)
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

                    Tile topTile = sprite.Overlay ? _graphicsManager.GetOverlayTile(sprite.TileTableIndex, sprite.TileValueIndex) : _graphicsManager.GetAbsoluteTile(sprite.TileTableIndex, sprite.TileValueIndex);
                    Tile bottomTile = sprite.Overlay ? _graphicsManager.GetOverlayTile(sprite.TileTableIndex, sprite.TileValueIndex + 1) : _graphicsManager.GetAbsoluteTile(sprite.TileTableIndex, sprite.TileValueIndex + 1);
                    int x = baseX + sprite.X, y = baseY + sprite.Y;

                    RenderTile(x, y, sprite.VerticalFlip ? bottomTile : topTile, _buffer, sprite.CustomPalette != null ? _palettesService.GetRgbPalette(sprite.CustomPalette) : _rgbPalette[paletteIndex + 4], sprite.HorizontalFlip, sprite.VerticalFlip, true);
                    RenderTile(x, y + 8, sprite.VerticalFlip ? topTile : bottomTile, _buffer, sprite.CustomPalette != null ? _palettesService.GetRgbPalette(sprite.CustomPalette) : _rgbPalette[paletteIndex + 4], sprite.HorizontalFlip, sprite.VerticalFlip, true);
                }
            }
        }
    }
}