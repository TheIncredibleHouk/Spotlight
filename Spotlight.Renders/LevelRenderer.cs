using Spotlight.Abstractions;
using Spotlight.Models;
using Spotlight.Services;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;

namespace Spotlight.Renderers
{
    public class LevelRenderer : Renderer
    {
        public const int BITMAP_HEIGHT = Level.BLOCK_HEIGHT * 16;
        public const int BITMAP_WIDTH = Level.BLOCK_WIDTH * 16;

        private byte[] _buffer;

        private ILevelDataManager _levelDataManager;
        private IGameObjectService _gameObjectService;
        private IPaletteService _paletteService;
        private List<TileTerrain> _terrain;

        public LevelRenderer(IGraphicsManager graphicsManager,
                            ILevelDataManager levelDataManager,
                            IPaletteService paletteService,
                            IGameObjectService gameObjectService,
                            List<TileTerrain> terrain) : base(graphicsManager)
        {
            _levelDataManager = levelDataManager;
            _gameObjectService = gameObjectService;
            _paletteService = paletteService;
            _terrain = terrain;
            _buffer = new byte[BITMAP_WIDTH * BITMAP_HEIGHT * 4];

            _highlightPalette[1] = _paletteService.RgbPalette[0x0F];
            _highlightPalette[2] = _paletteService.RgbPalette[0x30];
        }

        private TileSet _tileSet;
        private Palette _palette;

        public void Update(Palette palette = null, TileSet tileSet = null)
        {
            _tileSet = tileSet ?? _tileSet;
            _palette = palette ?? _palette;
            if (_tileSet != null)
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
            if (!_asStrategy)
            {
                RenderPointers(blockX, blockY, blockWidth, blockHeight);
            }
        }

        private bool _withSpriteOverlays, _withTerrainOverlay, _withInteractionOverlay, _asStrategy;
        private int? _withTileHighlight;

        public void Update(bool withSpriteOverlays, bool withTerrainOverlay, bool withInteractionOverlay, bool asStrategy)
        {
            _withSpriteOverlays = withSpriteOverlays;
            _withTerrainOverlay = withTerrainOverlay;
            _withInteractionOverlay = withInteractionOverlay;
            _asStrategy = asStrategy;
        }

        public void Update(Rectangle rect, bool withSpriteOverlays, bool withTerrainOverlay, bool withInteractionOverlay)
        {
            _withSpriteOverlays = withSpriteOverlays;
            _withTerrainOverlay = withTerrainOverlay;
            _withInteractionOverlay = withInteractionOverlay;
            Update(rect);
        }

        public void Update(Rectangle rect, bool withSpriteOverlays, bool withTerrainOverlay, bool withInteractionOverlay, int? tileHighlight, bool asStrategy)
        {
            _withSpriteOverlays = withSpriteOverlays;
            _withTerrainOverlay = withTerrainOverlay;
            _withInteractionOverlay = withInteractionOverlay;
            _withTileHighlight = tileHighlight;
            _asStrategy = asStrategy;
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
                    int tileValue = _levelDataManager.GetData(col, row);

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

                    if (_withTerrainOverlay)
                    {
                        TileTerrain terrain = _terrain.Where(t => t.HasTerrain(tile.Property)).FirstOrDefault();

                        if (terrain != null)
                        {
                            TileBlockOverlay overlay = terrain.Overlay;
                            if (overlay != null)
                            {
                                RenderTile(x, y, _graphicsManager.GetOverlayTile(0, overlay.UpperLeft), _buffer, _palette.RgbColors[overlay.PaletteIndex], useTransparency: true, opacity: .75);
                                RenderTile(x + 8, y, _graphicsManager.GetOverlayTile(0, overlay.UpperRight), _buffer, _palette.RgbColors[overlay.PaletteIndex], useTransparency: true, opacity: .75);
                                RenderTile(x, y + 8, _graphicsManager.GetOverlayTile(0, overlay.LowerLeft), _buffer, _palette.RgbColors[overlay.PaletteIndex], useTransparency: true, opacity: .75);
                                RenderTile(x + 8, y + 8, _graphicsManager.GetOverlayTile(0, overlay.LowerRight), _buffer, _palette.RgbColors[overlay.PaletteIndex], useTransparency: true, opacity: .75);
                            }
                        }
                    }

                    if (_withInteractionOverlay)
                    {
                        TileInteraction interaction = _terrain.Where(t => t.HasTerrain(tile.Property)).FirstOrDefault()?.Interactions.Where(i => i.HasInteraction(tile.Property)).FirstOrDefault();

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


                    if (_asStrategy)
                    {
                        if (tile.Property.IsTerrain(TileTerrain.ItemBlock) ||
                            tile.Property.IsTerrain(TileTerrain.HiddenItemBlock))
                        {

                            TileInteraction interaction = _terrain.Where(t => t.HasTerrain(tile.Property)).FirstOrDefault()?.Interactions.Where(i => i.HasInteraction(tile.Property)).FirstOrDefault();
                            if (interaction.Name.Contains("P-Switch"))
                            {
                                int flippedTile = _levelDataManager.GetData(col, row - 1) - 1;
                                TileBlock flippedBlock = _tileSet.TileBlocks[flippedTile];
                                int flippedPaletteIndex = (flippedTile & 0XC0) >> 6;

                                RenderTile(x, y - 16, _graphicsManager.GetRelativeTile(flippedBlock.UpperLeft), _buffer, _palette.RgbColors[flippedPaletteIndex], useTransparency: true, opacity: .75);
                                RenderTile(x + 8, y - 16, _graphicsManager.GetRelativeTile(flippedBlock.UpperRight), _buffer, _palette.RgbColors[flippedPaletteIndex], useTransparency: true, opacity: .75);
                                RenderTile(x, y - 8, _graphicsManager.GetRelativeTile(flippedBlock.LowerLeft), _buffer, _palette.RgbColors[flippedPaletteIndex], useTransparency: true, opacity: .75);
                                RenderTile(x + 8, y - 8, _graphicsManager.GetRelativeTile(flippedBlock.LowerRight), _buffer, _palette.RgbColors[flippedPaletteIndex], useTransparency: true, opacity: .75);
                            }
                            else if (tile.Property.IsTerrain(TileTerrain.HiddenItemBlock))
                            {
                                TileBlockOverlay overlay = interaction.Overlay;
                                if (overlay != null)
                                {
                                    RenderTile(x, y, _graphicsManager.GetOverlayTile(0, overlay.UpperLeft), _buffer, _palette.RgbColors[overlay.PaletteIndex], useTransparency: true, opacity: .75);
                                    RenderTile(x + 8, y, _graphicsManager.GetOverlayTile(0, overlay.UpperRight), _buffer, _palette.RgbColors[overlay.PaletteIndex], useTransparency: true, opacity: .75);
                                    RenderTile(x, y + 8, _graphicsManager.GetOverlayTile(0, overlay.LowerLeft), _buffer, _palette.RgbColors[overlay.PaletteIndex], useTransparency: true, opacity: .75);
                                    RenderTile(x + 8, y + 8, _graphicsManager.GetOverlayTile(0, overlay.LowerRight), _buffer, _palette.RgbColors[overlay.PaletteIndex], useTransparency: true, opacity: .75);
                                }
                            }
                            else if (interaction != null && !interaction.Name.Contains("Brick"))
                            {
                                TileBlockOverlay overlay = interaction.Overlay;
                                if (overlay != null)
                                {
                                    RenderTile(x, y - 16, _graphicsManager.GetOverlayTile(0, overlay.UpperLeft), _buffer, _palette.RgbColors[overlay.PaletteIndex], useTransparency: true, opacity: .75);
                                    RenderTile(x + 8, y - 16, _graphicsManager.GetOverlayTile(0, overlay.UpperRight), _buffer, _palette.RgbColors[overlay.PaletteIndex], useTransparency: true, opacity: .75);
                                    RenderTile(x, y - 8, _graphicsManager.GetOverlayTile(0, overlay.LowerLeft), _buffer, _palette.RgbColors[overlay.PaletteIndex], useTransparency: true, opacity: .75);
                                    RenderTile(x + 8, y - 8, _graphicsManager.GetOverlayTile(0, overlay.LowerRight), _buffer, _palette.RgbColors[overlay.PaletteIndex], useTransparency: true, opacity: .75);
                                }
                            }
                        }
                    }

                    if (_withTileHighlight == tileValue)
                    {
                        int upperLeft = 0xFF;
                        int upperRight = 0xFF;
                        int lowerLeft = 0xFF;
                        int lowerRight = 0xFF;

                        RenderTile(x, y, _graphicsManager.GetOverlayTile(0, upperLeft), _buffer, _highlightPalette, useTransparency: true, opacity: .5);
                        RenderTile(x + 8, y, _graphicsManager.GetOverlayTile(0, upperRight), _buffer, _highlightPalette, useTransparency: true, opacity: .5);
                        RenderTile(x, y + 8, _graphicsManager.GetOverlayTile(0, lowerLeft), _buffer, _highlightPalette, useTransparency: true, opacity: .5);
                        RenderTile(x + 8, y + 8, _graphicsManager.GetOverlayTile(0, lowerRight), _buffer, _highlightPalette, useTransparency: true, opacity: .5);
                    }
                }
            }
        }


        private Color[] _highlightPalette = new Color[4];


        public void RenderObjects(int blockX = 0, int blockY = 0, int blockWidth = Level.BLOCK_WIDTH, int blockHeight = Level.BLOCK_HEIGHT, bool withOverlays = false)
        {
            Rectangle updateRect = new Rectangle(blockX * 16, blockY * 16, blockWidth * 16, blockHeight * 16);

            foreach (var levelObject in _levelDataManager.GetLevelObjects(updateRect))
            {
                if (_asStrategy)
                {
                    if(levelObject.GameObject.IsStartObject || levelObject.GameObject.Name.Contains("Magic Star") && levelObject.Property == 7)
                    {
                        continue;
                    }
                }

                int baseX = levelObject.X * 16, baseY = levelObject.Y * 16;
                var visibleSprites = levelObject.GameObject.Sprites.Where(s => (s.PropertiesAppliedTo == null ? true : s.PropertiesAppliedTo.Contains(levelObject.Property)) && (_withSpriteOverlays ? true : !s.Overlay)).ToList();

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

                    RenderTile(x, y, sprite.VerticalFlip ? bottomTile : topTile, _buffer, sprite.CustomPalette != null ? _paletteService.GetRgbPalette(sprite.CustomPalette) : _palette.RgbColors[paletteIndex + 4], sprite.HorizontalFlip, sprite.VerticalFlip, true);
                    RenderTile(x, y + 8, sprite.VerticalFlip ? topTile : bottomTile, _buffer, sprite.CustomPalette != null ? _paletteService.GetRgbPalette(sprite.CustomPalette) : _palette.RgbColors[paletteIndex + 4], sprite.HorizontalFlip, sprite.VerticalFlip, true);
                }
            }
        }

        public void RenderPointers(int blockX = 0, int blockY = 0, int blockWidth = Level.BLOCK_WIDTH, int blockHeight = Level.BLOCK_HEIGHT)
        {
            Rectangle updateRect = new Rectangle(blockX * 16, blockY * 16, blockWidth * 16, blockHeight * 16);

            foreach (var pointerObject in _levelDataManager.GetPointers(updateRect))
            {
                int baseX = pointerObject.X * 16, baseY = pointerObject.Y * 16;

                foreach (var sprite in LevelPointer.Overlay)
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