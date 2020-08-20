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
    public class WorldRenderer : Renderer
    {

        public const int BITMAP_HEIGHT = Level.BLOCK_HEIGHT * 16;
        public const int BITMAP_WIDTH = Level.BLOCK_WIDTH * 16;

        private byte[] _buffer;

        private WorldDataAccessor _worldDataAccessor;
        private PalettesService _paletteService;
        private List<TileTerrain> _terrain;
        public WorldRenderer(GraphicsAccessor graphicsAccessor, WorldDataAccessor worldDataAccessor, PalettesService paletteService, List<TileTerrain> terrain) : base(graphicsAccessor)
        {
            _worldDataAccessor = worldDataAccessor;
            _paletteService = paletteService;
            _terrain = terrain;
            _buffer = new byte[BITMAP_WIDTH * BITMAP_HEIGHT * 4];
        }


        private Palette _palette;
        private TileSet _tileSet;
        private bool _withInteractionOverlay;
        public void Update(Palette palette = null, TileSet tileSet = null, bool? withInteractionOverlay = null)
        {
            _palette = palette ?? _palette;
            _tileSet = tileSet ?? _tileSet;
            _withInteractionOverlay = withInteractionOverlay ?? _withInteractionOverlay;
           
        }

        public byte[] GetRectangle(Int32Rect rect)
        {
            return GetRectangle(rect, _buffer);
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

            if (_initializing || _tileSet == null || _palette == null)
            {
                return;
            }

            RenderTiles(blockX, blockY, blockWidth, blockHeight);
            RenderPointers(blockX, blockY, blockWidth, blockHeight);
        }

        public void Update()
        {
            Update(new Rect(0, 0, BITMAP_WIDTH, BITMAP_HEIGHT));
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


                    int tileValue = _worldDataAccessor.GetData(col, row);

                    if(tileValue < 0)
                    {
                        continue;
                    }

                    int paletteIndex = (tileValue & 0XC0) >> 6;
                    TileBlock tile = _tileSet.TileBlocks[tileValue];
                    int x = col * 16, y = row * 16;

                    RenderTile(x, y, _graphicsAccessor.GetRelativeTile(tile.UpperLeft), _buffer, _palette.RgbColors[paletteIndex]);
                    RenderTile(x + 8, y, _graphicsAccessor.GetRelativeTile(tile.UpperRight), _buffer, _palette.RgbColors[paletteIndex]);
                    RenderTile(x, y + 8, _graphicsAccessor.GetRelativeTile(tile.LowerLeft), _buffer, _palette.RgbColors[paletteIndex]);
                    RenderTile(x + 8, y + 8, _graphicsAccessor.GetRelativeTile(tile.LowerRight), _buffer, _palette.RgbColors[paletteIndex]);


                    if (_withInteractionOverlay)
                    {
                        TileInteraction interaction = _terrain.Where(t => t.HasTerrain(tile.Property)).FirstOrDefault()?.Interactions.Where(i => i.HasInteraction(tile.Property)).FirstOrDefault();

                        if (interaction != null)
                        {
                            TileBlockOverlay overlay = interaction.Overlay;
                            if (overlay != null)
                            {
                                RenderTile(x, y, _graphicsAccessor.GetOverlayTile(0, overlay.UpperLeft), _buffer, _palette.RgbColors[overlay.PaletteIndex], useTransparency: true, opacity: .85);
                                RenderTile(x + 8, y, _graphicsAccessor.GetOverlayTile(0, overlay.UpperRight), _buffer, _palette.RgbColors[overlay.PaletteIndex], useTransparency: true, opacity: .85);
                                RenderTile(x, y + 8, _graphicsAccessor.GetOverlayTile(0, overlay.LowerLeft), _buffer, _palette.RgbColors[overlay.PaletteIndex], useTransparency: true, opacity: .85);
                                RenderTile(x + 8, y + 8, _graphicsAccessor.GetOverlayTile(0, overlay.LowerRight), _buffer, _palette.RgbColors[overlay.PaletteIndex], useTransparency: true, opacity: .85);
                            }
                        }
                    }
                }
            }
        }


        public void RenderPointers(int blockX = 0, int blockY = 0, int blockWidth = Level.BLOCK_WIDTH, int blockHeight = Level.BLOCK_HEIGHT)
        {
            Rect updateRect = new Rect(blockX * 16, blockY * 16, blockWidth * 16, blockHeight * 16);

            foreach (var pointerObject in _worldDataAccessor.GetPointers(updateRect))
            {
                int baseX = pointerObject.X * 16, baseY = pointerObject.Y * 16;

                foreach (var sprite in LevelPointer.Overlay)
                {
                    int paletteIndex = sprite.PaletteIndex;

                    Tile topTile = sprite.Overlay ? _graphicsAccessor.GetOverlayTile(sprite.TileTableIndex, sprite.TileValueIndex) : _graphicsAccessor.GetAbsoluteTile(sprite.TileTableIndex, sprite.TileValueIndex);
                    Tile bottomTile = sprite.Overlay ? _graphicsAccessor.GetOverlayTile(sprite.TileTableIndex, sprite.TileValueIndex + 1) : _graphicsAccessor.GetAbsoluteTile(sprite.TileTableIndex, sprite.TileValueIndex + 1);
                    int x = baseX + sprite.X, y = baseY + sprite.Y;

                    RenderTile(x, y, sprite.VerticalFlip ? bottomTile : topTile, _buffer, sprite.CustomPalette != null ? _paletteService.GetRgbPalette(sprite.CustomPalette) :  _palette.RgbColors[paletteIndex + 4], sprite.HorizontalFlip, sprite.VerticalFlip, true);
                    RenderTile(x, y + 8, sprite.VerticalFlip ? topTile : bottomTile, _buffer, sprite.CustomPalette != null ? _paletteService.GetRgbPalette(sprite.CustomPalette) : _palette.RgbColors[paletteIndex + 4], sprite.HorizontalFlip, sprite.VerticalFlip, true);
                }
            }
        }
    }
}
