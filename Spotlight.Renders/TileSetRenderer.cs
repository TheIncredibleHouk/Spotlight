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
    public class TileSetRenderer : Renderer
    {

        private byte[] _buffer;

        private List<TileTerrain> _terrain;

        public TileSetRenderer(GraphicsAccessor graphicsAccessor, List<TileTerrain> terrain) : base(graphicsAccessor)
        {
            _buffer = new byte[256 * 256 * BYTES_PER_BLOCK];
            _graphicsAccessor = graphicsAccessor;
            _terrain = terrain;
        }

        public byte[] GetRectangle(Int32Rect rect)
        {
            return GetRectangle(rect, _buffer);
        }

        private TileSet _tileSet;
        private Palette _palette;

        public void Update(TileSet tileSet, Palette palette)
        {
            _tileSet = tileSet;
            _palette = palette;
            Update();
        }

        public void Update(TileSet tileSet)
        {
            _tileSet = tileSet;
            Update();
        }

        public void Update(Palette palette)
        {
            _palette = palette;
            Update();
        }

        private bool _withTerrainOverlay, _withInteractionOerlay;
        public void Update(bool withTerrainOverlay, bool withInteractionOverlay)
        {
            _withTerrainOverlay = withTerrainOverlay;
            _withInteractionOerlay = withInteractionOverlay;
            Update();
        }

        public void Update()
        {
            
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

                    RenderTile(x, y, _graphicsAccessor.GetRelativeTile(tile.UpperLeft),  _buffer, _palette.RgbColors[paletteIndex]);
                    RenderTile(x + 8, y, _graphicsAccessor.GetRelativeTile(tile.UpperRight),  _buffer, _palette.RgbColors[paletteIndex]);
                    RenderTile(x, y + 8, _graphicsAccessor.GetRelativeTile(tile.LowerLeft), _buffer, _palette.RgbColors[paletteIndex]);
                    RenderTile(x + 8, y + 8, _graphicsAccessor.GetRelativeTile(tile.LowerRight), _buffer, _palette.RgbColors[paletteIndex]);


                    if (_withTerrainOverlay)
                    {
                        TileTerrain terrain = _terrain.Where(t => t.HasTerrain(tile.Property)).FirstOrDefault();
                        if (terrain != null)
                        {
                            TileBlockOverlay overlay = terrain.Overlay;
                            if (overlay != null)
                            {
                                RenderTile(x, y, _graphicsAccessor.GetOverlayTile(overlay.UpperLeft), _buffer, _palette.RgbColors[overlay.PaletteIndex], useTransparency: true, opacity: .75);
                                RenderTile(x + 8, y, _graphicsAccessor.GetOverlayTile(overlay.UpperRight), _buffer, _palette.RgbColors[overlay.PaletteIndex], useTransparency: true, opacity: .75);
                                RenderTile(x, y + 8, _graphicsAccessor.GetOverlayTile(overlay.LowerLeft), _buffer, _palette.RgbColors[overlay.PaletteIndex], useTransparency: true, opacity: .75);
                                RenderTile(x + 8, y + 8, _graphicsAccessor.GetOverlayTile(overlay.LowerRight), _buffer, _palette.RgbColors[overlay.PaletteIndex], useTransparency: true, opacity: .75);
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
                                RenderTile(x, y, _graphicsAccessor.GetOverlayTile(overlay.UpperLeft), _buffer, _palette.RgbColors[overlay.PaletteIndex], useTransparency: true, opacity: .85);
                                RenderTile(x + 8, y, _graphicsAccessor.GetOverlayTile(overlay.UpperRight), _buffer, _palette.RgbColors[overlay.PaletteIndex], useTransparency: true, opacity: .85);
                                RenderTile(x, y + 8, _graphicsAccessor.GetOverlayTile(overlay.LowerLeft), _buffer, _palette.RgbColors[overlay.PaletteIndex], useTransparency: true, opacity: .85);
                                RenderTile(x + 8, y + 8, _graphicsAccessor.GetOverlayTile(overlay.LowerRight), _buffer, _palette.RgbColors[overlay.PaletteIndex], useTransparency: true, opacity: .85);
                            }
                        }
                    }
                }
            }
        }
    }
}
