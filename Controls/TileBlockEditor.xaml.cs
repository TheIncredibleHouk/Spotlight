using Newtonsoft.Json;
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
    /// Interaction logic for TileBlockEditor.xaml
    /// </summary>
    public partial class TileBlockEditor : UserControl
    {


        private GraphicsService _graphicsService;
        private TileService _tileService;
        private TextService _textService;
        private GraphicsAccessor _graphicsAccessor;
        private ProjectService _projectService;
        private WorldService _worldService;
        private LevelService _levelService;
        private TileSetRenderer _tileSetRenderer;
        private GraphicsSetRender _graphicsSetRenderer;
        private WriteableBitmap _graphicsSetBitmap;
        private WriteableBitmap _tileBlockBitmap;

        public TileBlockEditor(ProjectService projectService, WorldService worldService, LevelService levelService, GraphicsService graphicsService, TileService tileService, TextService textService)
        {
            _ignoreChanges = true;
            InitializeComponent();

            _projectService = projectService;
            _graphicsService = graphicsService;
            _worldService = worldService;
            _levelService = levelService;
            _tileService = tileService;
            _textService = textService;


            List<KeyValuePair<string, string>> tileSetText = _textService.GetTable("tile_sets");


            tileSetText.Insert(0, new KeyValuePair<string, string>("0", "Map"));

            TerrainList.ItemsSource = _localTerrain = _tileService.GetTerrainCopy();
            LevelList.ItemsSource = _projectService.AllWorldsLevels();

            _graphicsAccessor = new GraphicsAccessor(_graphicsService.GetTileSection(0), _graphicsService.GetTileSection(0), _graphicsService.GetGlobalTiles(), _graphicsService.GetExtraTiles());

            _graphicsSetRenderer = new GraphicsSetRender(_graphicsAccessor);
            _tileSetRenderer = new TileSetRenderer(_graphicsAccessor, _localTerrain);

            _graphicsSetBitmap = new WriteableBitmap(128, 128, 96, 96, PixelFormats.Bgra32, null);
            _tileBlockBitmap = new WriteableBitmap(16, 16, 96, 96, PixelFormats.Bgra32, null);

            GraphicsSetImage.Source = _graphicsSetBitmap;
            GraphicsSetImage.Width = _graphicsSetBitmap.PixelWidth * 2;
            GraphicsSetImage.Height = _graphicsSetBitmap.PixelHeight * 2;

            TileBlockImage.Source = _tileBlockBitmap;
            TileBlockImage.Width = _tileBlockBitmap.PixelWidth * 2;
            TileBlockImage.Height = _tileBlockBitmap.PixelHeight * 2;

            BlockSelector.Initialize(_graphicsAccessor, _tileService, _tileService.GetTileSet(0), _graphicsService.GetPalette(0), _tileSetRenderer);
            BlockSelector.TileBlockSelected += BlockSelector_TileBlockSelected;

            LevelList.SelectedIndex = 1;
            BlockSelector.SelectedBlockValue = 0;
            _ignoreChanges = false;
        }

        private void BlockSelector_TileBlockSelected(TileBlock tileBlock, int tileValue)
        { 
            _ignoreChanges = true;
            _graphicsSetRenderer.Update((tileValue & 0xC0) >> 6);
            UpdateTileBlock();
            UpdateGraphics();

            TerrainList.SelectedValue = _localTerrain.Where(t => t.HasTerrain(tileBlock.Property)).FirstOrDefault()?.Value;
            InteractionList.SelectedValue = _localTerrain.Where(t => t.HasTerrain(tileBlock.Property)).FirstOrDefault()?.Interactions.Where(i => i.HasInteraction(tileBlock.Property)).FirstOrDefault()?.Value;
            _ignoreChanges = false;
        }

        private void TerrainList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TileTerrain selectedTerrain = (TileTerrain)TerrainList.SelectedItem;
            InteractionList.ItemsSource = selectedTerrain.Interactions;
            InteractionList.SelectedIndex = (int)(BlockSelector.SelectedTileBlock.Property & TileInteraction.Mask);

            BlockSelector.SelectedTileBlock.Property = selectedTerrain.Value | ((TileInteraction)InteractionList.SelectedItem).Value;
            TerrainOverlay.Text = JsonConvert.SerializeObject(selectedTerrain.Overlay, Formatting.Indented);

            BlockSelector.Update(ShowTerrain.IsChecked.Value, ShowInteractions.IsChecked.Value);
            UpdateTileBlock();
            SetUnsaved();
        }

        private void InteractionList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TileInteraction selectedIteraction = (TileInteraction)InteractionList.SelectedItem;

            if (selectedIteraction != null)
            {
                BlockSelector.SelectedTileBlock.Property = ((TileTerrain)TerrainList.SelectedItem).Value | selectedIteraction.Value;
                InteractionOverlay.Text = JsonConvert.SerializeObject(selectedIteraction.Overlay, Formatting.Indented);

                BlockSelector.Update(ShowTerrain.IsChecked.Value, ShowInteractions.IsChecked.Value);
                UpdateTileBlock();
                SetUnsaved();
            }
        }

        private TileSet _localTileSet;
        private List<TileTerrain> _localTerrain;
        private Level _currentLevel;
        private void LevelList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _ignoreChanges = true;
            if (LevelList.SelectedItem is LevelInfo)
            {
                LevelInfo levelInfo = (LevelInfo)LevelList.SelectedItem;
                _currentLevel = _levelService.LoadLevel(levelInfo);

                Tile[] staticTiles = _graphicsService.GetTileSection(_currentLevel.StaticTileTableIndex);
                Tile[] animatedTiles = _graphicsService.GetTileSection(_currentLevel.AnimationTileTableIndex);
                Palette palette = _graphicsService.GetPalette(_currentLevel.PaletteId);

                _graphicsAccessor.SetAnimatedTable(animatedTiles);
                _graphicsAccessor.SetStaticTable(staticTiles);

                _localTileSet = JsonConvert.DeserializeObject<TileSet>(JsonConvert.SerializeObject(_tileService.GetTileSet(_currentLevel.TileSetIndex)));
                _graphicsSetRenderer.Update(palette);

                BlockSelector.Update(_localTileSet, palette);

                UpdateTileBlock();
                UpdateGraphics();
            }
            _ignoreChanges = false;
        }

        private void UpdateGraphics()
        {
            _graphicsSetBitmap.Lock();

            Int32Rect sourceRect = new Int32Rect(0, 0, 128, 128);
            Int32Rect destRect = new Int32Rect(0, 0, 128, 128);

            _graphicsSetBitmap.WritePixels(destRect, _graphicsSetRenderer.GetRectangle(sourceRect), sourceRect.Width * 4, 0, 0);
            _graphicsSetBitmap.AddDirtyRect(sourceRect);
            _graphicsSetBitmap.Unlock();
        }

        private void UpdateTileBlock()
        {
            _tileBlockBitmap.Lock();

            Int32Rect sourceRect = new Int32Rect((int)(BlockSelector.SelectedBlockValue % 16) * 16, (int)(BlockSelector.SelectedBlockValue / 16) * 16, 16, 16);
            Int32Rect destRect = new Int32Rect(0, 0, 16, 16);

            _tileBlockBitmap.WritePixels(destRect, _tileSetRenderer.GetRectangle(sourceRect), sourceRect.Width * 4, 0, 0);
            _tileBlockBitmap.AddDirtyRect(destRect);
            _tileBlockBitmap.Unlock();
        }

        private void TerrainOverlay_TextChanged(object sender, TextChangedEventArgs e)
        {
            TileTerrain selectedTerrain = (TileTerrain)TerrainList.SelectedItem;
            if (selectedTerrain != null)
            {
                try
                {
                    TileBlockOverlay overlay = JsonConvert.DeserializeObject<TileBlockOverlay>(TerrainOverlay.Text);

                    if (overlay != null && (overlay.PaletteIndex < 0 || overlay.PaletteIndex >= 8))
                    {
                        throw new Exception();
                    }

                    TerrainOverlay.Foreground = new SolidColorBrush(Colors.White);
                    selectedTerrain.Overlay = overlay;

                    BlockSelector.Update(ShowTerrain.IsChecked.Value, ShowInteractions.IsChecked.Value);
                    UpdateTileBlock();
                    SetUnsaved();
                }
                catch
                {
                    TerrainOverlay.Foreground = new SolidColorBrush(Colors.Red);
                }
            }
        }

        private void InteractionOverlay_TextChanged(object sender, TextChangedEventArgs e)
        {
            TileInteraction selectedInteraction = (TileInteraction)InteractionList.SelectedItem;
            if (selectedInteraction != null)
            {
                try
                {
                    TileBlockOverlay overlay = JsonConvert.DeserializeObject<TileBlockOverlay>(InteractionOverlay.Text);

                    if (overlay != null && (overlay.PaletteIndex < 0 || overlay.PaletteIndex >= 8))
                    {
                        throw new Exception();
                    }

                    InteractionOverlay.Foreground = new SolidColorBrush(Colors.White);
                    selectedInteraction.Overlay = overlay;
                    
                    BlockSelector.Update(ShowTerrain.IsChecked.Value, ShowInteractions.IsChecked.Value);
                    UpdateTileBlock();
                    SetUnsaved();
                }
                catch
                {
                    InteractionOverlay.Foreground = new SolidColorBrush(Colors.Red);
                }
            }
        }

        private void ShowInteractions_Click(object sender, RoutedEventArgs e)
        {
            BlockSelector.Update(ShowTerrain.IsChecked.Value, ShowInteractions.IsChecked.Value);
            UpdateTileBlock();
        }

        private void ShowTerrain_Click(object sender, RoutedEventArgs e)
        {
            BlockSelector.Update(ShowTerrain.IsChecked.Value, ShowInteractions.IsChecked.Value);
            UpdateTileBlock();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _tileService.CommitTerrain(_localTerrain);
            _tileService.CommitTileSet(_currentLevel.TileSetIndex, _localTileSet);
            BlockSelector.Update(ShowTerrain.IsChecked.Value, ShowInteractions.IsChecked.Value);
            UpdateTileBlock();
            SetSaved();
        }

        private int _graphicTileIndexSelected = 0;
        private void GraphicsSetImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point clickPoint = Snap(e.GetPosition((Image)sender));
            Canvas.SetLeft(TileSelectionRectangle, clickPoint.X);
            Canvas.SetTop(TileSelectionRectangle, clickPoint.Y);

            _graphicTileIndexSelected = (int)(clickPoint.Y + (clickPoint.X / 16));
        }

        private Point Snap(Point value)
        {
            return new Point(Snap(value.X), Snap(value.Y));
        }

        private int Snap(double value)
        {
            return (int)(Math.Floor(value / 16) * 16);
        }

        private void TileBlockImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point clickPoint = Snap(e.GetPosition((Image)sender));

            if (clickPoint.X <= 15 && clickPoint.Y <= 15)
            {
                BlockSelector.SelectedTileBlock.UpperLeft = _graphicTileIndexSelected;
            }
            else if(clickPoint.X <= 15 && clickPoint.Y >= 16)
            {
                BlockSelector.SelectedTileBlock.LowerLeft = _graphicTileIndexSelected;
            }
            else if (clickPoint.X >= 16 && clickPoint.Y <= 15)
            {
                BlockSelector.SelectedTileBlock.UpperRight = _graphicTileIndexSelected;
            }
            else if (clickPoint.X >= 16 && clickPoint.Y >= 16)
            {
                BlockSelector.SelectedTileBlock.LowerRight = _graphicTileIndexSelected;
            }

            BlockSelector.Update(ShowTerrain.IsChecked.Value, ShowInteractions.IsChecked.Value);
            UpdateTileBlock();
            SetUnsaved();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var originalTerrain = _tileService.GetTerrain();
            for (var i = 0; i < _localTerrain.Count; i++)
            {
                _localTerrain[i].Overlay = originalTerrain[i].Overlay;
                for(var j = 0; j < _localTerrain[i].Interactions.Count; j++)
                {
                    _localTerrain[i].Interactions[j].Overlay = originalTerrain[i].Interactions[j].Overlay;
                }
            }

            var originalTileSet = _tileService.GetTileSet(_currentLevel.TileSetIndex);
            for(var i = 0; i < _localTileSet.TileBlocks.Length; i++)
            {
                _localTileSet.TileBlocks[i].LowerLeft = originalTileSet.TileBlocks[i].LowerLeft;
                _localTileSet.TileBlocks[i].LowerRight = originalTileSet.TileBlocks[i].LowerRight;
                _localTileSet.TileBlocks[i].UpperLeft = originalTileSet.TileBlocks[i].UpperLeft;
                _localTileSet.TileBlocks[i].UpperRight = originalTileSet.TileBlocks[i].UpperRight;
            }

            TerrainList_SelectionChanged(null, null);
            InteractionList_SelectionChanged(null, null);

            BlockSelector.Update(ShowTerrain.IsChecked.Value, ShowInteractions.IsChecked.Value);
            UpdateTileBlock();
            SetSaved();
        }

        private bool _ignoreChanges = false;
        private void SetUnsaved()
        {
            if (!_ignoreChanges)
            {
                GlobalPanels.MainWindow.SetUnsavedTab("Tile Set Editor");
            }
        }

        private void SetSaved()
        {
            GlobalPanels.MainWindow.SetSavedTab("Tile Set Editor");
        }
    }
}
