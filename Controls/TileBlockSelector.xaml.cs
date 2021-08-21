using Spotlight.Models;
using Spotlight.Renderers;
using Spotlight.Services;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Spotlight
{
    /// <summary>
    /// Interaction logic for TileBlockSelector.xaml
    /// </summary>
    public partial class TileBlockSelector : UserControl
    {
        public delegate void TileBlockSelectorEventHandler(TileBlock tileBlock, int tileBlockValue);

        public event TileBlockSelectorEventHandler TileBlockSelected;

        public TileBlock SelectedTileBlock { get; private set; }

        public TileBlockSelector()
        {
            InitializeComponent();
        }

        private GraphicsAccessor _graphicsAccessor;
        private TileSetRenderer _tileSetRenderer;
        private WriteableBitmap _bitmap;
        private TileSet _tileSet;
        private List<TileTerrain> _terrain;
        private List<MapTileInteraction> _mapTileInteractions;

        public void Initialize(GraphicsAccessor graphicsAccessor, TileService tileService, TileSet tileSet, Palette palette, TileSetRenderer tileSetRenderer = null)
        {
            _graphicsAccessor = graphicsAccessor;
            _tileSet = tileSet;
            _terrain = tileService.GetTerrain();
            _mapTileInteractions = tileService.GetMapTileInteractions();
            _tileSetRenderer = tileSetRenderer ?? new TileSetRenderer(graphicsAccessor, _terrain, _mapTileInteractions);

            Dpi dpi = this.GetDpi();
            _bitmap = new WriteableBitmap(256, 256, dpi.X, dpi.Y, PixelFormats.Bgra32, null);

            TileRenderSource.Source = _bitmap;

            Update(tileSet, palette);
            SelectedBlockValue = 0;
        }

        public void Update(TileSet tileSet = null, Palette palette = null, int? tileIndex = null, bool? withTerrainOverlay = null, bool? withInteractionOverlay = null, bool? withMapInteractionOverlay = null, bool? withProjectileInteractions = null)
        {
            if (_tileSetRenderer != null)
            {
                _tileSet = tileSet ?? _tileSet;
                _tileSetRenderer.Update(tileSet: tileSet, palette: palette, withTerrainOverlay: withTerrainOverlay, withInteractionOverlay: withInteractionOverlay, withMapInteractionOverlay: withMapInteractionOverlay, withProjectileInteractions: withProjectileInteractions);
                Update();
            }
        }

        public void Update()
        {
            _tileSetRenderer.Update();
            Update(new Rect(0, 0, 256, 256));
        }

        private void Update(Rect rect)
        {
            if (_bitmap == null || _tileSetRenderer == null)
            {
                return;
            }

            _bitmap.Lock();

            Int32Rect sourceArea = new Int32Rect((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);

            _bitmap.WritePixels(sourceArea, _tileSetRenderer.GetRectangle(sourceArea), sourceArea.Width * 4, sourceArea.X, sourceArea.Y);
            _bitmap.AddDirtyRect(sourceArea);
            _bitmap.Unlock();
        }

        private void TileRenderSource_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point clickPoint = Snap(e.GetPosition(this));
            SelectedBlockValue = (int)(clickPoint.Y + (clickPoint.X / 16));
        }

        private int _selectedBlockValue;

        public int SelectedBlockValue
        {
            get
            {
                return _selectedBlockValue;
            }
            set
            {
                if (value > 256)
                {
                    return;
                }

                _selectedBlockValue = value;
                SelectedTileBlock = _tileSet.TileBlocks[value];

                UpdateSelectionRectangle();
                if (TileBlockSelected != null)
                {
                    TileBlockSelected(_tileSet.TileBlocks[value], value);
                }
            }
        }

        private void UpdateSelectionRectangle()
        {
            Canvas.SetTop(SelectionRectangle, ((int)(_selectedBlockValue / 16)) * 16);
            Canvas.SetLeft(SelectionRectangle, ((int)(_selectedBlockValue % 16)) * 16);
        }

        private Point Snap(Point value)
        {
            return new Point(Snap(value.X), Snap(value.Y));
        }

        private int Snap(double value)
        {
            return (int)(Math.Floor(value / 16) * 16);
        }

        public byte[] GetTileBlockImage()
        {
            Int32Rect selectedArea = new Int32Rect(((int)(_selectedBlockValue % 16)) * 16, ((int)(_selectedBlockValue / 16)) * 16, 16, 16);
            return _tileSetRenderer.GetRectangle(selectedArea);
        }
    }
}