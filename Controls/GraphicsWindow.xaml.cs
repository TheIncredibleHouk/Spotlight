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
        private BlockRenderer _blockRenderer;

        private WriteableBitmap _graphicsBitmap;
        private WriteableBitmap _editorBitmap;

        public GraphicsWindow(GraphicsService graphicsService, TileService tileService, PalettesService palettesService)
        {
            InitializeComponent();

            _graphicsService = graphicsService;
            _tileService = tileService;
            _paletteService = palettesService;

            _graphicsAccessor = new GraphicsAccessor(_graphicsService.GetTilesAtAddress(0));
            _graphicsRenderer = new GraphicsSetRender(_graphicsAccessor);
            _blockRenderer = new BlockRenderer();


            Dpi dpi = this.GetDpi();

            _graphicsBitmap = new WriteableBitmap(128, 128, dpi.X, dpi.Y, PixelFormats.Bgra32, null);
            _editorBitmap = new WriteableBitmap(16, 16, dpi.X, dpi.Y, PixelFormats.Bgra32, null);

            PatternTable.Source = _graphicsBitmap;
            EditorImage.Source = _editorBitmap;

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
            UpdateGraphicsFromAddress();
        }

        private void UpdateGraphicsFromAddress()
        {
            int graphicsAddress;

            if (Int32.TryParse(GraphicsAddress.Text, System.Globalization.NumberStyles.HexNumber, System.Threading.Thread.CurrentThread.CurrentCulture, out graphicsAddress))
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
            if(address < 0)
            {
                GraphicsAddress.Text = "0";
                address = 0;
            }

            if(showOverlays &&
                address > 0x2000)
            {
                GraphicsAddress.Text = "2000";
                address = 0x2000;
            }

            if (address > 0x3F000)
            {
                GraphicsAddress.Text = "3F000";
                address = 0x3F000;
            }

            if (showOverlays)
            {
                _graphicsAccessor.SetFullTable(_graphicsService.GetExtraTilesAtAddress(address));
            }
            else
            {
                _graphicsAccessor.SetFullTable(_graphicsService.GetTilesAtAddress(address));
            }

            _graphicsRenderer.Update();
            CopyTilesToEditor();
            UpdateGraphics();
            UpdateEditor();
        }
        
        private void CopyTilesToEditor()
        {
            Tile[,] sourceTiles = new Tile[2, 2];
            int graphicsAddress = GetGraphicsAddress();
            

            if (showOverlays)
            {
                switch (LayoutOrder.SelectedIndex)
                {
                    case 0:
                        {
                            int tileCol = (int)(Canvas.GetLeft(SelectionRectangle) / 16);
                            int tileRow = (int)(Canvas.GetTop(SelectionRectangle) / 16);

                            sourceTiles[0, 0] = _graphicsService.GetExtraTileAtAddress(graphicsAddress, tileCol, tileRow);
                            sourceTiles[1, 0] = _graphicsService.GetExtraTileAtAddress(graphicsAddress, tileCol + 1, tileRow);
                            sourceTiles[0, 1] = _graphicsService.GetExtraTileAtAddress(graphicsAddress, tileCol, tileRow + 1);
                            sourceTiles[1, 1] = _graphicsService.GetExtraTileAtAddress(graphicsAddress, tileCol + 1, tileRow + 1);
                        }
                        break;

                    case 1:
                        {
                            int tileCol = (int)(Canvas.GetLeft(SelectionRectangle) / 16);
                            int tileRow = (int)(Canvas.GetTop(SelectionRectangle) / 16);

                            sourceTiles[0, 0] = _graphicsService.GetExtraTileAtAddress(graphicsAddress, tileCol, tileRow);
                            sourceTiles[1, 0] = _graphicsService.GetExtraTileAtAddress(graphicsAddress, tileCol, tileRow + 1);
                            sourceTiles[0, 1] = _graphicsService.GetExtraTileAtAddress(graphicsAddress, tileCol + 1, tileRow);
                            sourceTiles[1, 1] = _graphicsService.GetExtraTileAtAddress(graphicsAddress, tileCol + 1, tileRow + 1);
                        }
                        break;
                }
            }
            else
            {
                int tileCol = (int)(Canvas.GetLeft(SelectionRectangle) / 16);
                int tileRow = (int)(Canvas.GetTop(SelectionRectangle) / 16);

                sourceTiles[0, 0] = _graphicsRenderer.GetMappedTile(tileCol, tileRow);
                sourceTiles[1, 0] = _graphicsRenderer.GetMappedTile(tileCol + 1, tileRow);
                sourceTiles[0, 1] = _graphicsRenderer.GetMappedTile(tileCol, tileRow + 1);
                sourceTiles[1, 1] = _graphicsRenderer.GetMappedTile(tileCol + 1, tileRow + 1);
            }

            _blockRenderer.Update(sourceTiles);
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

        private void UpdateEditor()
        {
            _editorBitmap.Lock();

            Int32Rect sourceRect = new Int32Rect(0, 0, 16, 16);
            Int32Rect destRect = new Int32Rect(0, 0, 16, 16);

            _editorBitmap.WritePixels(destRect, _blockRenderer.GetRectangle(), sourceRect.Width * 4, 0, 0);
            _editorBitmap.AddDirtyRect(destRect);
            _editorBitmap.Unlock();
        }

        private void PalettesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _graphicsRenderer.Update((Palette)PalettesList.SelectedItem);
            _blockRenderer.Update(palette: (Palette)PalettesList.SelectedItem);

            LoadColors();
            UpdateGraphics();
            UpdateEditor();
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

            CopyTilesToEditor();
            UpdateGraphics();
            UpdateEditor();
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
            if (_graphicsRenderer != null)
            {
                _graphicsRenderer.Update(paletteIndex: ColorChoices.SelectedIndex);
                _blockRenderer.Update(paletteIndex: ColorChoices.SelectedIndex);

                Palette selectedPalette = (Palette)PalettesList.SelectedItem;

                ColorsIndex.Colors = selectedPalette.RgbColors[ColorChoices.SelectedIndex].ToMediaColor();

                UpdateGraphics();
                UpdateEditor();
            }
        }

        private void PatternTable_MouseDown(object sender, MouseButtonEventArgs e)
        {

            Point clickPoint = Snap(e.GetPosition(PatternTable));
            Canvas.SetLeft(SelectionRectangle, clickPoint.X);
            Canvas.SetTop(SelectionRectangle, clickPoint.Y);
            CopyTilesToEditor();
            UpdateEditor();
        }

        private Point Snap(Point value, int max = 239)
        {
            return new Point(Snap(Math.Min(value.X, max)), Snap(Math.Min(value.Y, max)));
        }

        private int Snap(double value)
        {
            return (int)(Math.Floor(value / 16) * 16);
        }

        public int GetGraphicsAddress()
        {
            int graphicsAddress;

            if (Int32.TryParse(GraphicsAddress.Text, System.Globalization.NumberStyles.HexNumber, System.Threading.Thread.CurrentThread.CurrentCulture, out graphicsAddress) &&
                graphicsAddress >= 0 &&
                graphicsAddress <= (showOverlays ? 0x2000 : 0x3F000))
            {
                return graphicsAddress;
            }

            return -1;
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.PageDown:
                    {
                        int graphicsAddress = GetGraphicsAddress();
                        if (graphicsAddress >= 0)
                        {
                            GraphicsAddress.Text = (graphicsAddress + 0x1000).ToString("X");
                            UpdateGraphicsFromAddress();
                        }
                    }
                    break;

                case Key.PageUp:
                    {
                        int graphicsAddress = GetGraphicsAddress();
                        if (graphicsAddress >= 0)
                        {
                            if (graphicsAddress - 0x1000 < 0)
                            {
                                GraphicsAddress.Text = "0";
                            }
                            else
                            {
                                GraphicsAddress.Text = (graphicsAddress - 0x1000).ToString("X");
                            }

                            UpdateGraphicsFromAddress();
                        }
                    }
                    break;

                case Key.Home:
                    GraphicsAddress.Text = "0";
                    UpdateGraphicsFromAddress();
                    break;

                case Key.End:
                    GraphicsAddress.Text = "3F000";
                    UpdateGraphicsFromAddress();
                    break;

                case Key.Up:
                    {
                        int addressChange = Keyboard.Modifiers == ModifierKeys.Shift ? 0x800 : 0x400;
                        int graphicsAddress = GetGraphicsAddress();
                        if (graphicsAddress >= 0)
                        {
                            if (graphicsAddress - addressChange < 0)
                            {
                                GraphicsAddress.Text = "0";
                            }
                            else
                            {
                                GraphicsAddress.Text = (graphicsAddress - addressChange).ToString("X");
                            }

                            UpdateGraphicsFromAddress();
                        }
                    }
                    break;

                case Key.Down:
                    {
                        int addressChange = Keyboard.Modifiers == ModifierKeys.Shift ? 0x800 : 0x400;
                        int graphicsAddress = GetGraphicsAddress();
                        if (graphicsAddress >= 0)
                        {
                            GraphicsAddress.Text = (graphicsAddress + addressChange).ToString("X");
                            UpdateGraphicsFromAddress();
                        }
                    }
                    break;
            }
        }

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point clickPoint = Snap(e.GetPosition(ColorsIndex));
            Canvas.SetLeft(IndexRectangle, (int) (clickPoint.X / 32 ) * 32);
        }

        private void EditorCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point clickPoint = Snap(e.GetPosition(EditorCanvas), 247);
            Canvas.SetLeft(EditorRectangle, clickPoint.X);
            Canvas.SetTop(EditorRectangle, clickPoint.Y);
            EditorRectangle.Width = EditorRectangle.Height = 16;
        }
    }
}
