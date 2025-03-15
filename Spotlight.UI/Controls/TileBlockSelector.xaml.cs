using Microsoft.Extensions.DependencyInjection;
using Spotlight.Abstractions;
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
using Rectangle = System.Drawing.Rectangle;

namespace Spotlight
{
    /// <summary>
    /// Interaction logic for TileBlockSelector.xaml
    /// </summary>
    public partial class TileBlockSelector : UserControl
    {

        public Guid Id { get; set; }

        public TileBlock SelectedTileBlock { get; private set; }

        private IGraphicsManager _graphicsManager;
        private ITileSetRenderer _tileSetRenderer;
        private ITileService _tileService;
        private IEventService _eventService;

        private WriteableBitmap _bitmap;
        private TileSet _tileSet;
        private List<TileTerrain> _terrain;
        private List<MapTileInteraction> _mapTileInteractions;

        public TileBlockSelector()
        {
            Id = Guid.NewGuid();

            InitializeComponent();
            InitializeServices();
            InitializeUI();
        }


        public void Initialize(TileSet tileSet, Palette palette, TileSetRenderer tileSetRenderer = null)
        {
            _terrain = _tileService.GetTerrain();
            _mapTileInteractions = _tileService.GetMapTileInteractions();
            if (tileSetRenderer != null)
            {
                _tileSetRenderer = tileSetRenderer;
            }

            _tileSet = tileSet;

            Update(tileSet, palette);
        }

        private void InitializeServices()
        {
            _graphicsManager = App.Services.GetService<IGraphicsManager>();
            _tileSetRenderer = App.Services.GetService<ITileSetRenderer>();
            _tileService = App.Services.GetService<ITileService>();
            _eventService = App.Services.GetService<IEventService>();
        }

        private void InitializeUI()
        {
            Dpi dpi = this.GetDpi();
            _bitmap = new WriteableBitmap(256, 256, dpi.X, dpi.Y, PixelFormats.Bgra32, null);

            TileRenderSource.Source = _bitmap;
            SelectedBlockValue = 0;

            _tileSetRenderer.Initialize(_terrain, _mapTileInteractions);
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

            _bitmap.WritePixels(sourceArea, _tileSetRenderer.GetRectangle(sourceArea.AsRectangle()), sourceArea.Width * 4, sourceArea.X, sourceArea.Y);
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

                _eventService.Emit(SpotlightEventType.UIBlockSelected, Id, new TileBlockSelection()
                {
                    BlockId = _selectedBlockValue,
                    Block = SelectedTileBlock
                });
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
            Rectangle selectedArea = new Rectangle(((int)(_selectedBlockValue % 16)) * 16, ((int)(_selectedBlockValue / 16)) * 16, 16, 16);
            return _tileSetRenderer.GetRectangle(selectedArea);
        }
    }

    public class TileBlockSelection
    {
        public int BlockId { get; set; }
        public TileBlock Block { get; set; }
    }
}