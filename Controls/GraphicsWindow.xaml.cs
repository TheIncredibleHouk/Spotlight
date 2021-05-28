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
using System.Windows.Shapes;

namespace Spotlight
{
    /// <summary>
    /// Interaction logic for GraphicsWindow.xaml
    /// </summary>
    public partial class GraphicsWindow : Window
    {
        private readonly GraphicsService _graphicsService;
        private readonly TileService _tileService;
        private readonly PalettesService _paletteService;

        private GraphicsAccessor _graphicsAccessor;
        private GraphicsSetRender _graphicsRenderer;

        private WriteableBitmap _graphicsBitmap;

        public GraphicsWindow(GraphicsService graphicsService, TileService tileService, PalettesService palettesService)
        {
            InitializeComponent();

            _graphicsService = graphicsService;
            _tileService = tileService;
            _paletteService = palettesService;

            _graphicsAccessor = new GraphicsAccessor(_graphicsService.GetTilesAtAddress(0));
            _graphicsRenderer = new GraphicsSetRender(_graphicsAccessor);


            Dpi dpi = this.GetDpi();

            _graphicsBitmap = new WriteableBitmap(128, 128, dpi.X, dpi.Y, PixelFormats.Bgra32, null);

            PatternTable.Source = _graphicsBitmap;

            LoadPalettes();
            GraphicsType.SelectedIndex = LayoutOrder.SelectedIndex = 0;

            _paletteService.PalettesChanged += _paletteService_PalettesChanged;
        }

        private void _paletteService_PalettesChanged()
        {
            LoadPalettes();
        }

        private void LoadColors()
        {
            Palette selectedPalette = (Palette)PalettesList.SelectedItem;
            if (selectedPalette != null)
            {
                Colors1.Colors = selectedPalette.RgbColors[0].ToMediaColor();
                Colors2.Colors = selectedPalette.RgbColors[1].ToMediaColor();
                Colors3.Colors = selectedPalette.RgbColors[2].ToMediaColor();
                Colors4.Colors = selectedPalette.RgbColors[3].ToMediaColor();
                Colors5.Colors = selectedPalette.RgbColors[4].ToMediaColor();
                Colors6.Colors = selectedPalette.RgbColors[5].ToMediaColor();
                Colors7.Colors = selectedPalette.RgbColors[6].ToMediaColor();
                Colors8.Colors = selectedPalette.RgbColors[7].ToMediaColor();
            }
        }

        private void LoadPalettes()
        {
            Palette selectedPalette = (Palette)PalettesList.SelectedItem;

            PalettesList.Items.Clear();
            foreach (Palette palette in _paletteService.GetPalettes())
            {
                PalettesList.Items.Add(palette);
            }

            PalettesList.SelectedItem = selectedPalette ?? PalettesList.Items[0];
        }

        private void GraphicsAddress_KeyUp(object sender, KeyEventArgs e)
        {
            int graphicsAddress;

            if (Int32.TryParse(GraphicsAddress.Text, System.Globalization.NumberStyles.HexNumber, System.Threading.Thread.CurrentThread.CurrentCulture, out graphicsAddress) &&
                graphicsAddress >= 0 &&
                graphicsAddress <= (showOverlays ? 0x2000 : 0x3F000))
            {
                GraphicsAddress.Foreground = new SolidColorBrush(Colors.White);
                UpdateGraphics(graphicsAddress);
            }
            else
            {
                GraphicsAddress.Foreground = new SolidColorBrush(Colors.Red);
            }
        }

        private void UpdateGraphics(int address)
        {
            if (showOverlays)
            {
                _graphicsAccessor.SetFullTable(_graphicsService.GetExtraTilesAtAddress(address));
            }
            else
            {
                _graphicsAccessor.SetFullTable(_graphicsService.GetTilesAtAddress(address));
            }
       
            _graphicsRenderer.Update();
            UpdateGraphics();
        }

        private void UpdateGraphics()
        {
            _graphicsBitmap.Lock();

            Int32Rect sourceRect = new Int32Rect(0, 0, 128, 128);
            Int32Rect destRect = new Int32Rect(0, 0, 128, 128);

            _graphicsBitmap.WritePixels(destRect, _graphicsRenderer.GetRectangle(sourceRect), sourceRect.Width * 4, 0, 0);
            _graphicsBitmap.AddDirtyRect(sourceRect);
            _graphicsBitmap.Unlock();
        }

        private void PalettesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _graphicsRenderer.Update((Palette)PalettesList.SelectedItem);
            LoadColors();
            UpdateGraphics();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _paletteService.PalettesChanged -= _paletteService_PalettesChanged;
        }

        private void LayoutOrder_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (LayoutOrder.SelectedIndex)
            {
                case 0:
                    _graphicsRenderer.Update(tileFormat: TileFormat._8x8);
                    break;

                case 1:
                    _graphicsRenderer.Update(tileFormat: TileFormat._8x16);
                    break;
            }

            UpdateGraphics();
        }

        private bool showOverlays = false;

        private void GraphicsType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            showOverlays = GraphicsType.SelectedIndex == 1;
            GraphicsAddress.Text = "0";
            GraphicsAddress_KeyUp(null, null);
        }

        private void ColorChoices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _graphicsRenderer.Update(paletteIndex: ColorChoices.SelectedIndex);
            UpdateGraphics();
        }
    }
}
