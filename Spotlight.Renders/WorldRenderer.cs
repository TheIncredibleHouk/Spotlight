using Spotlight.Abstractions;
using Spotlight.Models;
using Spotlight.Services;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;

namespace Spotlight.Renderers
{
    public class WorldRenderer : Renderer
    {
        public const int BITMAP_HEIGHT = World.BLOCK_HEIGHT * 16;
        public const int BITMAP_WIDTH = World.BLOCK_WIDTH * 16;

        private byte[] _buffer;

        private IWorldDataManager _worldDataManager;
        private IPaletteService _paletteService;
        private List<MapTileInteraction> _terrain;

        public WorldRenderer(IGraphicsManager graphicsManager, IWorldDataManager worldDataManager, IPaletteService paletteService, List<MapTileInteraction> terrain) : base(graphicsManager)
        {
            _worldDataManager = worldDataManager;
            _paletteService = paletteService;
            _buffer = new byte[BITMAP_WIDTH * BITMAP_HEIGHT * 4];

            BYTE_STRIDE = BYTES_PER_PIXEL * PIXELS_PER_BLOCK_ROW * BLOCKS_PER_SCREEN * 4;
        }

        public void SetTerrain(List<MapTileInteraction> terrain)
        {
            _terrain = terrain;
        }

        private Palette _palette;
        private TileSet _tileSet;
        private bool _withInteractionOverlay;
        private bool _withPointers;

        public void Update(Rectangle? rect = null, Palette palette = null, TileSet tileSet = null, bool? withInteractionOverlay = null, bool? withPointers = null)
        {
            _palette = palette ?? _palette;
            _tileSet = tileSet ?? _tileSet;
            _withInteractionOverlay = withInteractionOverlay ?? _withInteractionOverlay;
            _withPointers = withPointers ?? _withPointers;

            if (rect.HasValue)
            {
                Update(rect.Value);
            }
            else
            {
                Update();
            }
        }

        public byte[] GetRectangle(Rectangle rect)
        {
            return GetRectangle(rect, _buffer);
        }

        public void Update(Rectangle updateRect)
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
            RenderObjects(blockX, blockY, blockWidth, blockHeight);

            if (_withPointers)
            {
                RenderPointers(blockX, blockY, blockWidth, blockHeight);
            }
        }

        public void Update()
        {
            Update(new Rectangle(0, 0, BITMAP_WIDTH, BITMAP_HEIGHT));
        }

        public void RenderTiles(int blockX = 0, int blockY = 0, int blockWidth = World.BLOCK_WIDTH, int blockHeight = World.BLOCK_HEIGHT)
        {
            int maxRow = blockY + blockHeight;
            int maxCol = blockX + blockWidth;

            if (maxRow > World.BLOCK_HEIGHT)
            {
                maxRow = World.BLOCK_HEIGHT;
            }

            if (maxCol > World.BLOCK_WIDTH)
            {
                maxCol = World.BLOCK_WIDTH;
            }

            for (int row = blockY; row < maxRow; row++)
            {
                for (int col = blockX; col < maxCol; col++)
                {
                    int tileValue = _worldDataManager.GetData(col, row);

                    if (tileValue < 0)
                    {
                        continue;
                    }

                    int paletteIndex = (tileValue & 0XC0) >> 6;
                    TileBlock tile = _tileSet.TileBlocks[tileValue];
                    int x = col * 16, y = row * 16;

                    RenderTile(x, y, _graphicsManager.GetRelativeTile(tile.UpperLeft), _buffer, _palette.RgbColors[paletteIndex]);
                    RenderTile(x + 8, y, _graphicsManager.GetRelativeTile(tile.UpperRight), _buffer, _palette.RgbColors[paletteIndex]);
                    RenderTile(x, y + 8, _graphicsManager.GetRelativeTile(tile.LowerLeft), _buffer, _palette.RgbColors[paletteIndex]);
                    RenderTile(x + 8, y + 8, _graphicsManager.GetRelativeTile(tile.LowerRight), _buffer, _palette.RgbColors[paletteIndex]);

                    if (_withInteractionOverlay)
                    {
                        MapTileInteraction interaction = _terrain.Where(t => t.HasInteraction(tile.Property)).FirstOrDefault();

                        if (interaction != null)
                        {
                            TileBlockOverlay overlay = interaction.Overlay;
                            if (overlay != null)
                            {
                                RenderTile(x, y, _graphicsManager.GetOverlayTile(0, overlay.UpperLeft), _buffer, _palette.RgbColors[overlay.PaletteIndex], useTransparency: true, opacity: .85);
                                RenderTile(x + 8, y, _graphicsManager.GetOverlayTile(0, overlay.UpperRight), _buffer, _palette.RgbColors[overlay.PaletteIndex], useTransparency: true, opacity: .85);
                                RenderTile(x, y + 8, _graphicsManager.GetOverlayTile(0, overlay.LowerLeft), _buffer, _palette.RgbColors[overlay.PaletteIndex], useTransparency: true, opacity: .85);
                                RenderTile(x + 8, y + 8, _graphicsManager.GetOverlayTile(0, overlay.LowerRight), _buffer, _palette.RgbColors[overlay.PaletteIndex], useTransparency: true, opacity: .85);
                            }
                        }
                    }
                }
            }
        }

        public void RenderObjects(int blockX = 0, int blockY = 0, int blockWidth = Level.BLOCK_WIDTH, int blockHeight = Level.BLOCK_HEIGHT, bool withOverlays = false)
        {
            Rectangle updateRect = new Rectangle(blockX * 16, blockY * 16, blockWidth * 16, blockHeight * 16);

            foreach (var worldObject in _worldDataManager.GetWorldObjects(updateRect))
            {
                int baseX = worldObject.X * 16, baseY = worldObject.Y * 16;
                var visibleSprites = worldObject.GameObject.Sprites;


                foreach (var sprite in visibleSprites)
                {
                    int paletteIndex = sprite.PaletteIndex;

                    Tile topTile = sprite.Overlay ? _graphicsManager.GetOverlayTile(sprite.TileTableIndex, sprite.TileValueIndex) : _graphicsManager.GetAbsoluteTile(sprite.TileTableIndex, sprite.TileValueIndex);
                    Tile bottomTile = sprite.Overlay ? _graphicsManager.GetOverlayTile(sprite.TileTableIndex, sprite.TileValueIndex + 1) : _graphicsManager.GetAbsoluteTile(sprite.TileTableIndex, sprite.TileValueIndex + 1);
                    int x = baseX + sprite.X, y = baseY + sprite.Y;

                    RenderTile(x, y, sprite.VerticalFlip ? bottomTile : topTile, _buffer, sprite.CustomPalette != null ? _paletteService.GetRgbPalette(sprite.CustomPalette) : _palette.RgbColors[paletteIndex + 4], sprite.HorizontalFlip, sprite.VerticalFlip, true);
                    RenderTile(x, y + 8, sprite.VerticalFlip ? topTile : bottomTile, _buffer, sprite.CustomPalette != null ? _paletteService.GetRgbPalette(sprite.CustomPalette) : _palette.RgbColors[paletteIndex + 4], sprite.HorizontalFlip, sprite.VerticalFlip, true);
                }
            }
        }

        public void RenderPointers(int blockX = 0, int blockY = 0, int blockWidth = World.BLOCK_WIDTH, int blockHeight = World.BLOCK_HEIGHT)
        {
            Rectangle updateRect = new Rectangle(blockX * 16, blockY * 16, blockWidth * 16, blockHeight * 16);

            foreach (var pointerObject in _worldDataManager.GetPointers(updateRect))
            {
                int baseX = pointerObject.X * 16, baseY = pointerObject.Y * 16;

                foreach (var sprite in WorldPointer.Overlay)
                {
                    int paletteIndex = sprite.PaletteIndex;

                    Tile topTile = sprite.Overlay ? _graphicsManager.GetOverlayTile(sprite.TileTableIndex, sprite.TileValueIndex) : _graphicsManager.GetAbsoluteTile(sprite.TileTableIndex, sprite.TileValueIndex);
                    Tile bottomTile = sprite.Overlay ? _graphicsManager.GetOverlayTile(sprite.TileTableIndex, sprite.TileValueIndex + 1) : _graphicsManager.GetAbsoluteTile(sprite.TileTableIndex, sprite.TileValueIndex + 1);
                    int x = baseX + sprite.X, y = baseY + sprite.Y;

                    RenderTile(x, y, sprite.VerticalFlip ? bottomTile : topTile, _buffer, sprite.CustomPalette != null ? _paletteService.GetRgbPalette(sprite.CustomPalette) : _palette.RgbColors[paletteIndex + 4], sprite.HorizontalFlip, sprite.VerticalFlip, true);
                    RenderTile(x, y + 8, sprite.VerticalFlip ? topTile : bottomTile, _buffer, sprite.CustomPalette != null ? _paletteService.GetRgbPalette(sprite.CustomPalette) : _palette.RgbColors[paletteIndex + 4], sprite.HorizontalFlip, sprite.VerticalFlip, true);
                }
            }
        }
    }
}