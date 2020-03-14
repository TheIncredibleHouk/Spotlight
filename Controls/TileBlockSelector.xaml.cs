using Spotlight.Models;
using Spotlight.Renderers;
using Spotlight.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Spotlight
{
    /// <summary>
    /// Interaction logic for TileBlockSelector.xaml
    /// </summary>
    public partial class TileBlockSelector : UserControl
    {
        public TileBlock SelectedTileBlock { get; private set; }
        public TileBlockSelector()
        {
            InitializeComponent();
        }

        private GraphicsAccessor _graphicsAccessor;
        private TileRenderer _tileRenderer;
        private WriteableBitmap _bitmap;
        public void Initialize(GraphicsAccessor graphicsAccessor, TileSet tileSet, Palette palette)
        {
            _graphicsAccessor = graphicsAccessor;

            _tileRenderer = new TileRenderer(graphicsAccessor);

            TileRenderSource.Width = 256;
            TileRenderSource.Height = 256;

            _bitmap = new WriteableBitmap(256, 256, 96, 96, PixelFormats.Bgra32, null);

            TileRenderSource.Source = _bitmap;

            Update(tileSet, palette);
            SelectedTileValue = 0;
        }

        public void Update()
        {
            if (_tileRenderer != null)
            {
                _tileRenderer.Update();
                Render();
            }
        }
        public void Update(TileSet tileSet)
        {
            if (_tileRenderer != null)
            {
                _tileRenderer.Update(tileSet);
                Render();
            }
        }

        public void Update(Palette palette)
        {
            if (_tileRenderer != null)
            {
                _tileRenderer.Update(palette);
                Render();
            }
        }

        public void Update(TileSet tileSet, Palette palette)
        {
            if (_tileRenderer != null)
            {
                _tileRenderer.Update(tileSet, palette);
                Render();
            }
        }

        private void Render()
        {
            if (_bitmap == null || _tileRenderer == null)
            {
                return;
            }

            _bitmap.Lock();


            Int32Rect sourceArea = new Int32Rect(0, 0, 256, 256);
            _bitmap.WritePixels(sourceArea, _tileRenderer.GetRectangle(sourceArea), sourceArea.Width * 4, sourceArea.X, sourceArea.Y);
            _bitmap.AddDirtyRect(sourceArea);

            _bitmap.Unlock();
        }


        private void TileRenderSource_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point clickPoint = Snap(e.GetPosition(this));

            SelectedTileValue = (int)(clickPoint.Y + (clickPoint.X / 16));
        }

        private int _selectedTileValue;
        public int SelectedTileValue
        {
            get
            {
                return _selectedTileValue;
            }
            set
            {
                _selectedTileValue = value;
                UpdateSelectionRectangle();
            }
        }
        private void UpdateSelectionRectangle()
        {
            Canvas.SetTop(SelectionRectangle, ((int)(_selectedTileValue / 16)) * 16);
            Canvas.SetLeft(SelectionRectangle, ((int)(_selectedTileValue % 16)) * 16);
        }

        private Point Snap(Point value)
        {
            return new Point(Snap(value.X), Snap(value.Y));
        }

        private int Snap(double value)
        {
            return (int)(Math.Floor(value / 16) * 16);
        }
    }
}
