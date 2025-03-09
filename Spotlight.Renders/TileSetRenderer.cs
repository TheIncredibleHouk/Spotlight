using Spotlight.Abstractions;
using Spotlight.Models;
using Spotlight.Services;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;

namespace Spotlight.Renderers
{
    public class TileSetRenderer : Renderer
    {
        private byte[] _buffer;

        private List<TileTerrain> _terrain;
        private List<MapTileInteraction> _mapTileInteractions;

        public TileSetRenderer(IGraphicsManager graphicsManager) : base(graphicsManager)
        {
            _buffer = new byte[256 * 256 * BYTES_PER_BLOCK];
            BYTE_STRIDE = 16 * 16 * 4;
        }

        public byte[] GetRectangle(Rectangle rect)
        {
            return GetRectangle(rect, _buffer);
        }

        public void SetTilesAndTerrain(List<TileTerrain> terrain, List<MapTileInteraction> mapTileInteractions)
        {
            _terrain = terrain;
            _mapTileInteractions = mapTileInteractions;
        }

        private TileSet _tileSet;
        private Palette _palette;

        private bool _withTerrainOverlay,
                     _withInteractionOverlay,
                     _withMapInteractionOverlay,
                     _withProjectileInteractions;

        public void Update(TileSet tileSet = null, Palette palette = null, bool? withTerrainOverlay = null, bool? withInteractionOverlay = null, bool? withMapInteractionOverlay = null, bool? withProjectileInteractions = null)
        {
            _tileSet = tileSet ?? _tileSet;
            _palette = palette ?? _palette;
            _withProjectileInteractions = withProjectileInteractions ?? _withProjectileInteractions;
            _withTerrainOverlay = withTerrainOverlay ?? _withTerrainOverlay;
            _withInteractionOverlay = withInteractionOverlay ?? _withInteractionOverlay;
            _withMapInteractionOverlay = withMapInteractionOverlay ?? _withMapInteractionOverlay;

            Update();
        }

        public void Update()
        {
            if (_tileSet == null || _palette == null)
            {
                return;
            }

            int maxRow = 16;
            int maxCol = 16;

            for (int row = 0; row < maxRow; row++)
            {
                for (int col = 0; col < maxCol; col++)
                {
                    int tileValue = row * 16 + col;
                    int PaletteIndex = tileValue / 0x40;

                    TileBlock tile = _tileSet.TileBlocks[tileValue];
                    int paletteIndex = (tileValue & 0XC0) >> 6;
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

                    if (_withMapInteractionOverlay)
                    {
                        MapTileInteraction interaction = _mapTileInteractions.Where(i => i.HasInteraction(tile.Property)).FirstOrDefault();
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

                    if (_withProjectileInteractions)
                    {
                        if (_tileSet.FireBallInteractions.Contains(tileValue))
                        {
                            RenderTile(x + 4, y + 4, _graphicsManager.GetOverlayTile(0, 0xEE), _buffer, _palette.RgbColors[5], useTransparency: true);
                        }

                        if (_tileSet.IceBallInteractions.Contains(tileValue))
                        {
                            RenderTile(x + 4, y + 4, _graphicsManager.GetOverlayTile(0, 0xEF), _buffer, _palette.RgbColors[5], useTransparency: true);
                        }

                        PSwitchAlteration alteration = _tileSet.PSwitchAlterations.Where(p => p.From == tileValue).FirstOrDefault();
                        if (alteration != null)
                        {
                            TileBlock alternativeTile = PSwitchAlteration.GetAlterationBlocks(alteration.To);

                            RenderTile(x, y, _graphicsManager.GetOverlayTile(4, alternativeTile.UpperLeft), _buffer, _palette.RgbColors[paletteIndex], useTransparency: true, opacity: .85);
                            RenderTile(x + 8, y, _graphicsManager.GetOverlayTile(4, alternativeTile.UpperRight), _buffer, _palette.RgbColors[paletteIndex], useTransparency: true, opacity: .85);
                            RenderTile(x, y + 8, _graphicsManager.GetOverlayTile(4, alternativeTile.LowerLeft), _buffer, _palette.RgbColors[paletteIndex], useTransparency: true, opacity: .85);
                            RenderTile(x + 8, y + 8, _graphicsManager.GetOverlayTile(4, alternativeTile.LowerRight), _buffer, _palette.RgbColors[paletteIndex], useTransparency: true, opacity: .85);
                        }
                    }
                }
            }
        }
    }
}