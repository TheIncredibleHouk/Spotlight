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
    public partial class TileBlockEditor : UserControl, IDetachEvents
    {


        private GraphicsService _graphicsService;
        private TileService _tileService;
        private TextService _textService;
        private GraphicsAccessor _graphicsAccessor;
        private WorldService _worldService;
        private LevelService _levelService;
        private PalettesService _palettesService;
        private TileSetRenderer _tileSetRenderer;
        private GraphicsSetRender _graphicsSetRenderer;
        private WriteableBitmap _graphicsSetBitmap;
        private WriteableBitmap _tileBlockBitmap;

        public TileBlockEditor(WorldService worldService, LevelService levelService, GraphicsService graphicsService, PalettesService palettesService, TileService tileService, TextService textService)
        {
            _ignoreChanges = true;
            InitializeComponent();

            _palettesService = palettesService;
            _graphicsService = graphicsService;
            _worldService = worldService;
            _levelService = levelService;
            _tileService = tileService;
            _textService = textService;


            List<KeyValuePair<string, string>> tileSetText = _textService.GetTable("tile_sets");


            tileSetText.Insert(0, new KeyValuePair<string, string>("0", "Map"));

            TerrainList.ItemsSource = _localTileTerrain = _tileService.GetTerrainCopy();
            LevelList.ItemsSource = _levelService.AllWorldsLevels();
            MapInteractionList.ItemsSource = _localMapTileInteraction = _tileService.GetMapTileInteractionCopy();

            _graphicsAccessor = new GraphicsAccessor(_graphicsService.GetTileSection(0), _graphicsService.GetTileSection(0), _graphicsService.GetGlobalTiles(), _graphicsService.GetExtraTiles());

            _graphicsSetRenderer = new GraphicsSetRender(_graphicsAccessor);
            _tileSetRenderer = new TileSetRenderer(_graphicsAccessor, _localTileTerrain, _localMapTileInteraction);

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

            _graphicsService.GraphicsUpdated += _graphicsService_GraphicsUpdated;
            _graphicsService.ExtraGraphicsUpdated += _graphicsService_GraphicsUpdated;
        }

        public void DetachEvents()
        {
            _graphicsService.GraphicsUpdated -= _graphicsService_GraphicsUpdated;
            _graphicsService.ExtraGraphicsUpdated -= _graphicsService_GraphicsUpdated;
            BlockSelector.TileBlockSelected -= BlockSelector_TileBlockSelected;
        }

        private void _graphicsService_GraphicsUpdated()
        {
            if (_currentLevel != null)
            {
                _graphicsAccessor.SetAnimatedTable(_graphicsService.GetTileSection(_currentLevel.AnimationTileTableIndex));
                _graphicsAccessor.SetStaticTable(_graphicsService.GetTileSection(_currentLevel.StaticTileTableIndex));
            }
            else if (_currentWorld != null)
            {
                _graphicsAccessor.SetAnimatedTable(_graphicsService.GetTileSection(_currentWorld.AnimationTileTableIndex));
                _graphicsAccessor.SetStaticTable(_graphicsService.GetTileSection(_currentWorld.TileTableIndex));
            }

            _graphicsAccessor.SetGlobalTiles(_graphicsService.GetGlobalTiles(), _graphicsService.GetExtraTiles());
            UpdateTileBlock();
            UpdateGraphics();
            BlockSelector.Update();
        }

        public void SelectTileBlock(Guid levelId, int tileBlockValue)
        {
            LevelList.SelectedValue = levelId;
            BlockSelector.SelectedBlockValue = tileBlockValue;
        }

        private PSwitchAlteration _pSwitchAlteration = null;

        private void BlockSelector_TileBlockSelected(TileBlock tileBlock, int tileValue)
        {
            _ignoreChanges = true;


            if (_currentLevel != null)
            {
                TerrainList.SelectedValue = _localTileTerrain.Where(t => t.HasTerrain(tileBlock.Property)).FirstOrDefault()?.Value;
                InteractionList.SelectedValue = _localTileTerrain.Where(t => t.HasTerrain(tileBlock.Property)).FirstOrDefault()?.Interactions.Where(i => i.HasInteraction(tileBlock.Property)).FirstOrDefault()?.Value;
            }
            else if (_currentWorld != null)
            {
                MapInteractionList.SelectedValue = _localMapTileInteraction.Where(t => t.HasInteraction(tileBlock.Property)).FirstOrDefault()?.Value;
            }

            _graphicsSetRenderer.Update((tileValue & 0xC0) >> 6);
            UpdateTileBlock();
            UpdateGraphics();

            _ignoreChanges = false;

            if (_setInteractions)
            {
                if (Mouse.LeftButton == MouseButtonState.Pressed)
                {
                    if (_localTileSet.FireBallInteractions.Contains(BlockSelector.SelectedBlockValue))
                    {
                        _localTileSet.FireBallInteractions.Remove(BlockSelector.SelectedBlockValue);
                    }
                    else
                    {
                        if (_localTileSet.FireBallInteractions.Count < 8)
                        {
                            _localTileSet.FireBallInteractions.Add(BlockSelector.SelectedBlockValue);
                        }
                    }
                }
                else if (Mouse.RightButton == MouseButtonState.Pressed)
                {
                    if (_localTileSet.IceBallInteractions.Contains(BlockSelector.SelectedBlockValue))
                    {
                        _localTileSet.IceBallInteractions.Remove(BlockSelector.SelectedBlockValue);
                    }
                    else
                    {
                        if (_localTileSet.IceBallInteractions.Count < 8)
                        {
                            _localTileSet.IceBallInteractions.Add(BlockSelector.SelectedBlockValue);
                        }
                    }
                }
                else if (Mouse.MiddleButton == MouseButtonState.Pressed)
                {
                    if (_pSwitchAlteration != null)
                    {
                        _pSwitchAlteration.To = BlockSelector.SelectedBlockValue;
                        _localTileSet.PSwitchAlterations.Add(_pSwitchAlteration);
                        _pSwitchAlteration = null;
                    }
                    else
                    {
                        PSwitchAlteration existingAlteration = _localTileSet.PSwitchAlterations.Where(p => p.From == BlockSelector.SelectedBlockValue).FirstOrDefault();
                        if (existingAlteration != null)
                        {
                            _localTileSet.PSwitchAlterations.Remove(existingAlteration);
                        }
                        else
                        {
                            if (_localTileSet.PSwitchAlterations.Count < 16)
                            {
                                _pSwitchAlteration = new PSwitchAlteration();
                                _pSwitchAlteration.From = BlockSelector.SelectedBlockValue;
                            }
                        }
                    }
                }

                BlockSelector.Update();
                UpdateTileBlock();
            }
        }

        private void TerrainList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TileTerrain selectedTerrain = (TileTerrain)TerrainList.SelectedItem;
            InteractionList.ItemsSource = selectedTerrain.Interactions;
            InteractionList.SelectedIndex = (int)(BlockSelector.SelectedTileBlock.Property & TileInteraction.Mask);

            BlockSelector.SelectedTileBlock.Property = selectedTerrain.Value | ((TileInteraction)InteractionList.SelectedItem).Value;
            TerrainOverlay.Text = JsonConvert.SerializeObject(selectedTerrain.Overlay, Formatting.Indented);

            BlockSelector.Update(tileIndex: BlockSelector.SelectedBlockValue);
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

                BlockSelector.Update(tileIndex: BlockSelector.SelectedBlockValue);
                UpdateTileBlock();
                SetUnsaved();
            }
        }

        private TileSet _localTileSet;
        private List<TileTerrain> _localTileTerrain;
        private List<MapTileInteraction> _localMapTileInteraction;
        private World _currentWorld;
        private Level _currentLevel;
        private void LevelList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _ignoreChanges = true;
            if (LevelList.SelectedItem is LevelInfo)
            {
                _setInteractions = false;
                _pSwitchAlteration = null;
                SetProjectileInteractions.Content = "Set PSwitch/Fireball/Iceball Interactions";

                SetProjectileInteractions.Visibility = TileDefinitions.Visibility = LevelTileSection.Visibility = Visibility.Visible;
                MapTileDefinitions.Visibility = MapTileSection.Visibility = Visibility.Collapsed;

                LevelInfo levelInfo = (LevelInfo)LevelList.SelectedItem;
                _currentLevel = _levelService.LoadLevel(levelInfo);
                _currentWorld = null;

                Tile[] staticTiles = _graphicsService.GetTileSection(_currentLevel.StaticTileTableIndex);
                Tile[] animatedTiles = _graphicsService.GetTileSection(_currentLevel.AnimationTileTableIndex);
                Palette palette = _palettesService.GetPalette(_currentLevel.PaletteId);

                _graphicsAccessor.SetAnimatedTable(animatedTiles);
                _graphicsAccessor.SetStaticTable(staticTiles);

                _localTileSet = JsonConvert.DeserializeObject<TileSet>(JsonConvert.SerializeObject(_tileService.GetTileSet(_currentLevel.TileSetIndex)));

                _ignoreChanges = false;
                _graphicsSetRenderer.Update(palette);
                BlockSelector.Update(tileSet: _localTileSet, palette: palette, withProjectileInteractions: false);
                UpdateGraphics();
                UpdateTileBlock();
            }
            else if (LevelList.SelectedItem is WorldInfo)
            {
                _setInteractions = false;
                _pSwitchAlteration = null;
                SetProjectileInteractions.Content = "Set PSwitch/Fireball/Iceball Interactions";

                SetProjectileInteractions.Visibility = TileDefinitions.Visibility = LevelTileSection.Visibility = Visibility.Collapsed;
                MapTileDefinitions.Visibility = MapTileSection.Visibility = Visibility.Visible;

                if (MapInteractionList.SelectedIndex == -1)
                {
                    MapInteractionList.SelectedIndex = 0;
                }

                WorldInfo worldInfo = (WorldInfo)LevelList.SelectedItem;
                _currentWorld = _worldService.LoadWorld(worldInfo);
                _currentLevel = null;

                Tile[] staticTiles = _graphicsService.GetTileSection(_currentWorld.TileTableIndex);
                Tile[] animatedTiles = _graphicsService.GetTileSection(_currentWorld.AnimationTileTableIndex);
                Palette palette = _palettesService.GetPalette(_currentWorld.PaletteId);

                _graphicsAccessor.SetAnimatedTable(animatedTiles);
                _graphicsAccessor.SetStaticTable(staticTiles);

                _localTileSet = JsonConvert.DeserializeObject<TileSet>(JsonConvert.SerializeObject(_tileService.GetTileSet(0)));
                _graphicsSetRenderer.Update(palette);

                _ignoreChanges = false;

                _graphicsSetRenderer.Update(palette);
                BlockSelector.Update(tileSet: _localTileSet, palette: palette, withProjectileInteractions: false);
                UpdateGraphics();
                UpdateTileBlock();
            }

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

                    BlockSelector.Update(tileIndex: BlockSelector.SelectedBlockValue);
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

                    BlockSelector.Update(tileIndex: BlockSelector.SelectedBlockValue);
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
            BlockSelector.Update(withInteractionOverlay: _currentLevel != null ? ShowInteractions.IsChecked.Value : false,
                                 withMapInteractionOverlay: _currentWorld != null ? ShowInteractions.IsChecked.Value: false);
            UpdateTileBlock();
        }

        private void ShowTerrain_Click(object sender, RoutedEventArgs e)
        {
            BlockSelector.Update(withMapInteractionOverlay: _currentLevel != null ? ShowTerrain.IsChecked.Value : false);
            UpdateTileBlock();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _tileService.CommitTileSet(_currentLevel.TileSetIndex, _localTileSet, _localTileTerrain, _localMapTileInteraction);
            BlockSelector.Update();
            UpdateTileBlock();
            SetSaved();
        }

        private int _graphicTileIndexSelected = 0;
        private bool _isQuadSelected = false;
        private void GraphicsSetImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point clickPoint = Snap(e.GetPosition((Image)GraphicsSetImage));


            TileSelectionRectangle.Height = TileSelectionRectangle.Width = 16;
            _isQuadSelected = false;

            if (e.RightButton == MouseButtonState.Pressed)
            {
                _isQuadSelected = true;
                TileSelectionRectangle.Height = TileSelectionRectangle.Width = 32;
                if (clickPoint.X >= 16 * 15)
                {
                    clickPoint.X = 16 * 14;
                }

                if (clickPoint.Y >= 16 * 15)
                {
                    clickPoint.Y = 16 * 14;
                }
            }

            _graphicTileIndexSelected = (int)(clickPoint.Y + (clickPoint.X / 16));
            Canvas.SetLeft(TileSelectionRectangle, clickPoint.X);
            Canvas.SetTop(TileSelectionRectangle, clickPoint.Y);
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
                if (_isQuadSelected)
                {
                    BlockSelector.SelectedTileBlock.UpperRight = _graphicTileIndexSelected + 1;
                    BlockSelector.SelectedTileBlock.LowerLeft = _graphicTileIndexSelected + 0x10;
                    BlockSelector.SelectedTileBlock.LowerRight = _graphicTileIndexSelected + 0x11;
                }
            }
            else if (clickPoint.X <= 15 && clickPoint.Y >= 16)
            {
                BlockSelector.SelectedTileBlock.LowerLeft = _graphicTileIndexSelected;

                if (_isQuadSelected)
                {
                    BlockSelector.SelectedTileBlock.LowerRight = _graphicTileIndexSelected + 1;
                    BlockSelector.SelectedTileBlock.UpperLeft = _graphicTileIndexSelected + 0x10;
                    BlockSelector.SelectedTileBlock.UpperRight = _graphicTileIndexSelected + 0x11;
                }
            }
            else if (clickPoint.X >= 16 && clickPoint.Y <= 15)
            {
                BlockSelector.SelectedTileBlock.UpperRight = _graphicTileIndexSelected;

                if (_isQuadSelected)
                {
                    BlockSelector.SelectedTileBlock.UpperLeft = _graphicTileIndexSelected + 0x1;
                    BlockSelector.SelectedTileBlock.LowerRight = _graphicTileIndexSelected + 0x10;
                    BlockSelector.SelectedTileBlock.LowerLeft = _graphicTileIndexSelected + 0x11;
                }
            }
            else if (clickPoint.X >= 16 && clickPoint.Y >= 16)
            {
                BlockSelector.SelectedTileBlock.LowerRight = _graphicTileIndexSelected;

                if (_isQuadSelected)
                {
                    BlockSelector.SelectedTileBlock.LowerLeft = _graphicTileIndexSelected + 0x1;
                    BlockSelector.SelectedTileBlock.UpperRight = _graphicTileIndexSelected + 0x10;
                    BlockSelector.SelectedTileBlock.UpperLeft = _graphicTileIndexSelected + 0x11;
                }
            }

            BlockSelector.Update(tileIndex: BlockSelector.SelectedBlockValue);
            UpdateTileBlock();
            SetUnsaved();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var originalTerrain = _tileService.GetTerrain();
            for (var i = 0; i < _localTileTerrain.Count; i++)
            {
                _localTileTerrain[i].Overlay = originalTerrain[i].Overlay;
                for (var j = 0; j < _localTileTerrain[i].Interactions.Count; j++)
                {
                    _localTileTerrain[i].Interactions[j].Overlay = originalTerrain[i].Interactions[j].Overlay;
                }
            }

            var originalTileSet = _tileService.GetTileSet(_currentLevel.TileSetIndex);
            for (var i = 0; i < _localTileSet.TileBlocks.Length; i++)
            {
                _localTileSet.TileBlocks[i].LowerLeft = originalTileSet.TileBlocks[i].LowerLeft;
                _localTileSet.TileBlocks[i].LowerRight = originalTileSet.TileBlocks[i].LowerRight;
                _localTileSet.TileBlocks[i].UpperLeft = originalTileSet.TileBlocks[i].UpperLeft;
                _localTileSet.TileBlocks[i].UpperRight = originalTileSet.TileBlocks[i].UpperRight;
            }

            TerrainList_SelectionChanged(null, null);
            InteractionList_SelectionChanged(null, null);

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

        private void MapInteractionList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MapTileInteraction selectedIteraction = (MapTileInteraction)MapInteractionList.SelectedItem;

            if (selectedIteraction != null)
            {
                BlockSelector.SelectedTileBlock.Property = ((MapTileInteraction)MapInteractionList.SelectedItem).Value;
                MapInteractionOverlay.Text = JsonConvert.SerializeObject(selectedIteraction.Overlay, Formatting.Indented);

                BlockSelector.Update(tileIndex: BlockSelector.SelectedBlockValue);
                UpdateTileBlock();
                SetUnsaved();
            }
        }

        private void MapInteractionOverlay_TextChanged(object sender, TextChangedEventArgs e)
        {
            MapTileInteraction selectedInteraction = (MapTileInteraction)MapInteractionList.SelectedItem;
            if (selectedInteraction != null)
            {
                try
                {
                    TileBlockOverlay overlay = JsonConvert.DeserializeObject<TileBlockOverlay>(MapInteractionOverlay.Text);

                    if (overlay != null && (overlay.PaletteIndex < 0 || overlay.PaletteIndex >= 8))
                    {
                        throw new Exception();
                    }

                    MapInteractionOverlay.Foreground = new SolidColorBrush(Colors.White);
                    selectedInteraction.Overlay = overlay;

                    BlockSelector.Update(tileIndex: BlockSelector.SelectedBlockValue);
                    UpdateTileBlock();
                    SetUnsaved();
                }
                catch
                {
                    MapInteractionOverlay.Foreground = new SolidColorBrush(Colors.Red);
                }
            }
        }

        private void BlockSelectorBorder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            BlockSelectorBorder.Focus();
        }

        private TileBlock _copiedblock;
        private void BlockSelectorBorder_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (e.Key == Key.C)
                {
                    _copiedblock = BlockSelector.SelectedTileBlock;
                }

                if (e.Key == Key.V)
                {
                    BlockSelector.SelectedTileBlock.UpperLeft = _copiedblock.UpperLeft;
                    BlockSelector.SelectedTileBlock.UpperRight = _copiedblock.UpperRight;
                    BlockSelector.SelectedTileBlock.LowerLeft = _copiedblock.LowerLeft;
                    BlockSelector.SelectedTileBlock.LowerRight = _copiedblock.LowerRight;
                    BlockSelector.Update();
                    UpdateTileBlock();
                    SetUnsaved();
                }
            }
        }

        private bool _setInteractions;
        private void SetProjectileInteractions_Click(object sender, RoutedEventArgs e)
        {
            if (_setInteractions == false)
            {
                SetProjectileInteractions.Content = "Finish PSwitch/Fireball/Iceball Interactions";
                _setInteractions = true;
                BlockSelector.Update(withProjectileInteractions: true);
            }
            else
            {
                SetProjectileInteractions.Content = "Set PSwitch/Fireball/Iceball Interactions";
                _pSwitchAlteration = null;
                _setInteractions = false;
                BlockSelector.Update(withProjectileInteractions: false);
            }
        }

        private void GraphicsSetImage_MouseMove(object sender, MouseEventArgs e)
        {
            Point mousePoint = Snap(e.GetPosition(GraphicsSetImage));
            int value = (int)(mousePoint.Y + (mousePoint.X / 16));
            GraphicsCoordinate.Text = value.ToString("X2");
        }

        private void BlockSelector_MouseMove(object sender, MouseEventArgs e)
        {
            Point mousePoint = Snap(e.GetPosition(BlockSelector));
            int value = (int)(mousePoint.Y + (mousePoint.X / 16));
            TileSelectorCoordinate.Text = value.ToString("X2");
        }
    }
}
;