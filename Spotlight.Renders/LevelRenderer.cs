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

        private byte[] _buffer;

        private LevelDataAccessor _levelDataAccessor;
        private GameObjectService _gameObjectService;
        private List<TileTerrain> _terrain;
        public LevelRenderer(GraphicsAccessor graphicsAccessor, LevelDataAccessor levelDataAccessor, GameObjectService gameObjectService, List<TileTerrain> terrain) : base(graphicsAccessor)
        {
            _levelDataAccessor = levelDataAccessor;
            _gameObjectService = gameObjectService;
            _terrain = terrain;
            _buffer = new byte[BITMAP_WIDTH * BITMAP_HEIGHT * 4];
        }



        private TileSet _tileSet;
        public void SetTileSet(TileSet tileSet)
        {
            _tileSet = tileSet;
            if (_rgbPalette != null)
            {
                Update();
            }

        }

        private Color[][] _rgbPalette;
        public void SetPalette(Palette palette)
        {
            _rgbPalette = palette.RgbColors;
            if (_tileSet != null)
            {
                Update();
            }
        }

        public byte[] GetRectangle(Int32Rect rect)
        {
            return GetRectangle(rect, _buffer);
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
                blockWidth = (int)(updateRect.Width / 16),
                blockHeight = (int)(updateRect.Height / 16);

            RenderTiles(blockX, blockY, blockWidth, blockHeight);
            RenderObjects(blockX, blockY, blockWidth, blockHeight);
        }

        private bool _withSpriteOverlays, _withTerrainOverlay, _withInteractionOerlay;
        public void Update(bool withSpriteOverlays, bool withTerrainOverlay, bool withInteractionOverlay)
        {
            _withSpriteOverlays = withSpriteOverlays;
            _withTerrainOverlay = withTerrainOverlay;
            _withInteractionOerlay = withInteractionOverlay;
        }

        public void Update(Int32Rect rect, bool withSpriteOverlays, bool withTerrainOverlay, bool withInteractionOverlay)
        {
            _withSpriteOverlays = withSpriteOverlays;
            _withTerrainOverlay = withTerrainOverlay;
            _withInteractionOerlay = withInteractionOverlay;
            Update(rect);
        }

        public void RenderTiles(int blockX = 0, int blockY = 0, int blockWidth = Level.BLOCK_WIDTH, int blockHeight = Level.BLOCK_HEIGHT)
        {
            int maxRow = blockY + blockHeight;
            int maxCol = blockX + blockWidth;

            if (maxRow > Level.BLOCK_HEIGHT)
            {
                maxRow = Level.BLOCK_HEIGHT;
            }

            if (maxCol > Level.BLOCK_WIDTH)
            {
                maxCol = Level.BLOCK_WIDTH;
            }

            for (int row = blockY; row < maxRow; row++)
            {
                for (int col = blockX; col < maxCol; col++)
                {


                    int tileValue = _levelDataAccessor.GetData(col, row);
                    int paletteIndex = (tileValue & 0XC0) >> 6;
                    TileBlock tile = _tileSet.TileBlocks[tileValue];
                    int x = col * 16, y = row * 16;

                    RenderTile(x, y, _graphicsAccessor.GetRelativeTile(tile.UpperLeft), _buffer, _rgbPalette[paletteIndex]);
                    RenderTile(x + 8, y, _graphicsAccessor.GetRelativeTile(tile.UpperRight), _buffer, _rgbPalette[paletteIndex]);
                    RenderTile(x, y + 8, _graphicsAccessor.GetRelativeTile(tile.LowerLeft), _buffer, _rgbPalette[paletteIndex]);
                    RenderTile(x + 8, y + 8, _graphicsAccessor.GetRelativeTile(tile.LowerRight), _buffer, _rgbPalette[paletteIndex]);

                    if (_withTerrainOverlay)
                    {
                        TileTerrain terrain = _terrain.Where(t => t.HasTerrain(tile.Property)).FirstOrDefault();

                        if (terrain != null)
                        {
                            TileBlockOverlay overlay = terrain.Overlay;
                            if (overlay != null)
                            {
                                RenderTile(x + overlay.OffsetX, y + overlay.OffsetY, _graphicsAccessor.GetOverlayTile(overlay.UpperLeft), _buffer, _rgbPalette[overlay.PaletteIndex], useTransparency: true, opacity: .75);
                                RenderTile(x + 8 + overlay.OffsetX, y + overlay.OffsetY, _graphicsAccessor.GetOverlayTile(overlay.UpperRight), _buffer, _rgbPalette[overlay.PaletteIndex], useTransparency: true, opacity: .75);
                                RenderTile(x + overlay.OffsetX, y + 8 + overlay.OffsetY, _graphicsAccessor.GetOverlayTile(overlay.LowerLeft), _buffer, _rgbPalette[overlay.PaletteIndex], useTransparency: true, opacity: .75);
                                RenderTile(x + 8 + overlay.OffsetX, y + 8 + overlay.OffsetY, _graphicsAccessor.GetOverlayTile(overlay.LowerRight), _buffer, _rgbPalette[overlay.PaletteIndex], useTransparency: true, opacity: .75);
                            }
                        }
                    }

                    if (_withInteractionOerlay)
                    {
                        TileInteraction interaction = _terrain.Where(t => t.HasTerrain(tile.Property)).FirstOrDefault()?.Interactions.Where(i => i.HasInteraction(tile.Property)).FirstOrDefault();

                        if (interaction != null)
                        {
                            TileBlockOverlay overlay = interaction.Overlay;
                            if (overlay != null)
                            {
                                RenderTile(x + overlay.OffsetX, y + overlay.OffsetY, _graphicsAccessor.GetOverlayTile(overlay.UpperLeft), _buffer, _rgbPalette[overlay.PaletteIndex], useTransparency: true, opacity: .85);
                                RenderTile(x + 8 + overlay.OffsetX, y + overlay.OffsetY, _graphicsAccessor.GetOverlayTile(overlay.UpperRight), _buffer, _rgbPalette[overlay.PaletteIndex], useTransparency: true, opacity: .85);
                                RenderTile(x + overlay.OffsetX, y + 8 + overlay.OffsetY, _graphicsAccessor.GetOverlayTile(overlay.LowerLeft), _buffer, _rgbPalette[overlay.PaletteIndex], useTransparency: true, opacity: .85);
                                RenderTile(x + 8 + overlay.OffsetX, y + 8 + overlay.OffsetY, _graphicsAccessor.GetOverlayTile(overlay.LowerRight), _buffer, _rgbPalette[overlay.PaletteIndex], useTransparency: true, opacity: .85);
                            }
                        }
                    }
                }
            }
        }

        public void RenderObjects(int blockX = 0, int blockY = 0, int blockWidth = Level.BLOCK_WIDTH, int blockHeight = Level.BLOCK_HEIGHT, bool withOverlays = false)
        {
            Rect updateReact = new Rect(blockX * 16, blockY * 16, blockWidth * 16, blockHeight * 16);

            foreach (var levelObject in _levelDataAccessor.GetLevelObjects().Where(o => o.BoundRectangle.IntersectsWith(updateReact)).OrderBy(o => o.X).ThenBy(o => o.Y).ToList())
            {
                int baseX = levelObject.X * 16, baseY = levelObject.Y * 16;
                var visibleSprites = levelObject.GameObject.Sprites.Where(s => (s.PropertiesAppliedTo == null ? true : s.PropertiesAppliedTo.Contains(levelObject.Property)) && (_withSpriteOverlays ? true : !s.Overlay)).ToList();

                if (visibleSprites.Count == 0)
                {
                    visibleSprites = _gameObjectService.InvisibleSprites;
                }

                foreach (var sprite in visibleSprites)
                {
                    int paletteIndex = sprite.PaletteIndex;

                    Tile topTile = sprite.Overlay ? _graphicsAccessor.GetOverlayTile(sprite.TileValueIndex) : _graphicsAccessor.GetAbsoluteTile(sprite.TileTableIndex, sprite.TileValueIndex);
                    Tile bottomTile = sprite.Overlay ? _graphicsAccessor.GetOverlayTile(sprite.TileValueIndex + 1) : _graphicsAccessor.GetAbsoluteTile(sprite.TileTableIndex, sprite.TileValueIndex + 1);
                    int x = baseX + sprite.X, y = baseY + sprite.Y;

                    RenderTile(x, y, sprite.VerticalFlip ? bottomTile : topTile, _buffer, _rgbPalette[paletteIndex + 4], sprite.HorizontalFlip, sprite.VerticalFlip, true);
                    RenderTile(x, y + 8, sprite.VerticalFlip ? topTile : bottomTile, _buffer, _rgbPalette[paletteIndex + 4], sprite.HorizontalFlip, sprite.VerticalFlip, true);
                }
            }
        }
    }
}
