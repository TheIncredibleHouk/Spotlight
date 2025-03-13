using Spotlight.Abstractions;
using Spotlight.Models;
using Spotlight.Renderers;
using Spotlight.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Rectangle = System.Drawing.Rectangle;

namespace Spotlight
{
    /// <summary>
    /// Interaction logic for WorldPanel.xaml
    /// </summary>
    public partial class WorldPanel : UserControl, IKeyDownHandler
    {
        
        private IWorldService _worldService;
        private IPaletteService _palettesService;
        private IWorldRenderer _worldRenderer;
        
        private ITextService _textService;
        private IGraphicsService _graphicsService;
        private ICompressionService _compressionService;
        private IGameObjectService _gameObjectService;
        private IGraphicsManager _graphicsAccessor;
        private IWorldDataManager _worldDataAccessor;
        private IHistoryService _historyService;

        private World _world;
        private WorldInfo _worldInfo;
        private TileSet _tileSet;
        private TileService _tileService;
        private WriteableBitmap _bitmap;
        private List<MapTileInteraction> _interactions;

        public delegate void WorldEditorExitSelectedHandled(int x, int y);

        public event WorldEditorExitSelectedHandled WorldEditorExitSelected;

        private bool _initializing = true;

        public WorldPanel(IGraphicsService graphicsService, PaletteService palettesService, TextService textService, TileService tileService, WorldService worldService, ILevelService levelService, GameObjectService gameObjectService, WorldInfo worldInfo)
        {
            InitializeComponent();

            _worldInfo = worldInfo;
            _textService = textService;
            _graphicsService = graphicsService;
            _tileService = tileService;
            _palettesService = palettesService;
            _worldService = worldService;
            _gameObjectService = gameObjectService;

            _historyService = new HistoryService();
            _interactions = _tileService.GetMapTileInteractions();
            _world = _worldService.LoadWorld(_worldInfo);
            _compressionService = new CompressionService();

            Tile[] bottomTableSet = _graphicsService.GetTileSection(_world.TileTableIndex);
            Tile[] topTableSet = _graphicsService.GetTileSection(_world.AnimationTileTableIndex);

            _graphicsAccessor = new GraphicsManager(topTableSet, bottomTableSet, _graphicsService.GetGlobalTiles(), _graphicsService.GetExtraTiles());
            _worldDataAccessor = new WorldDataManager(_world);

            _bitmap = new WriteableBitmap(WorldRenderer.BITMAP_WIDTH, WorldRenderer.BITMAP_HEIGHT, 96, 96, PixelFormats.Bgra32, null);
            _worldRenderer = new WorldRenderer(_graphicsAccessor, _worldDataAccessor, _palettesService, _tileService.GetMapTileInteractions());
            _worldRenderer.Initializing();

            _tileSet = _tileService.GetTileSet(_world.TileSetIndex);

            Palette palette = _palettesService.GetPalette(_world.PaletteId);
            _worldRenderer.Update(tileSet: _tileSet, palette: palette);

            WorldRenderSource.Source = _bitmap;
            WorldRenderSource.Width = _bitmap.PixelWidth;
            WorldRenderSource.Height = _bitmap.PixelHeight;
            CanvasContainer.Width = RenderContainer.Width = _world.ScreenLength * 16 * 16;

            SelectedEditMode.SelectedIndex = 0;
            SelectedDrawMode.SelectedIndex = 0;

            TileSelector.Initialize(_graphicsAccessor, _tileService, _tileSet, palette);
            ObjectSelector.Initialize(_gameObjectService, _palettesService, _graphicsAccessor, palette);
            PointerEditor.Initialize(levelService, _worldInfo);

            _world.ObjectData.ForEach(o => o.GameObject = gameObjectService.GetObject(o.GameObjectId));


            UpdateTextTables();

            _graphicsService.GraphicsUpdated += _graphicsService_GraphicsUpdated;
            _graphicsService.ExtraGraphicsUpdated += _graphicsService_GraphicsUpdated;
            _palettesService.PalettesChanged += _palettesService_PalettesChanged;
            _tileService.TileSetUpdated += _tileService_TileSetUpdated;
            _worldService.WorldUpdated += _worldService_WorldUpdated;
            gameObjectService.GameObjectUpdated += GameObjectService_GameObjectsUpdated;


            _world.ObjectData.ForEach(o =>
            {
                o.CalcBoundBox();
                o.CalcVisualBox(true);
            });

            _initializing = false;
            _worldRenderer.Ready();
            Update();
        }

        private void _worldService_WorldUpdated(WorldInfo worldInfo)
        {
            if (worldInfo.Id == _world.Id)
            {
                _world.Name = worldInfo.Name;
            }
        }

        private void _tileService_TileSetUpdated(int index, TileSet tileSet)
        {
            Update();
            TileSelector.Update(_tileSet);
        }

        public void DetachEvents()
        {
            _graphicsService.GraphicsUpdated -= _graphicsService_GraphicsUpdated;
            _graphicsService.ExtraGraphicsUpdated -= _graphicsService_GraphicsUpdated;
            _palettesService.PalettesChanged -= _palettesService_PalettesChanged;
            _worldService.WorldUpdated -= _worldService_WorldUpdated;
            _gameObjectService.GameObjectUpdated -= GameObjectService_GameObjectsUpdated;
        }

        private void _palettesService_PalettesChanged()
        {
            PaletteIndex.ItemsSource = _palettesService.GetPalettes();
            if (PaletteIndex.SelectedItem == null)
            {
                PaletteIndex.SelectedIndex = 0;
            }

            _worldRenderer.Update(palette: (Palette)PaletteIndex.SelectedItem);
            TileSelector.Update(palette: (Palette)PaletteIndex.SelectedItem);
            ObjectSelector.Update(palette: (Palette)PaletteIndex.SelectedItem);
            Update();
        }

        private void _graphicsService_GraphicsUpdated()
        {
            _graphicsAccessor.SetTopTable(_graphicsService.GetTileSection(_world.AnimationTileTableIndex));
            _graphicsAccessor.SetBottomTable(_graphicsService.GetTileSection(_world.TileTableIndex));
            TileSelector.Update();
            Update();
        }

        private void ObjectSelector_GameObjectDoubleClicked(GameObject gameObject)
        {
            GlobalPanels.EditGameObject(gameObject, (Palette)PaletteIndex.SelectedItem);
        }

        private void Update(Rectangle updateRect)
        {
            Update(new List<Rectangle>() { updateRect });
        }

        private void Update(int x = 0, int y = 0, int width = WorldRenderer.BITMAP_WIDTH, int height = WorldRenderer.BITMAP_HEIGHT)
        {
            Update(new List<Rectangle>() { new Rectangle(x, y, width, height) });
        }

        private void Update(List<Rectangle> updateAreas)
        {
            if (updateAreas.Count == 0 || _initializing)
            {
                return;
            }

            DateTime speedTest = DateTime.Now;

            _bitmap.Lock();

            foreach (var updateArea in updateAreas.Select(r => new Int32Rect((int)r.X, (int)r.Y, (int)r.Width, (int)r.Height)))
            {
                Int32Rect safeRect = new Int32Rect(Math.Max(0, updateArea.X), Math.Max(0, updateArea.Y), updateArea.Width, updateArea.Height);

                if (safeRect.X + safeRect.Width > WorldRenderer.BITMAP_WIDTH)
                {
                    safeRect.Width -= WorldRenderer.BITMAP_WIDTH - (safeRect.X + safeRect.Width);
                }

                if (safeRect.Y + safeRect.Height > WorldRenderer.BITMAP_HEIGHT)
                {
                    safeRect.Height -= WorldRenderer.BITMAP_HEIGHT - (safeRect.Y + safeRect.Height);
                }

                Int32Rect sourceArea = new Int32Rect(0, 0, Math.Max(0, Math.Min(safeRect.Width, WorldRenderer.BITMAP_WIDTH)), Math.Max(0, Math.Min(safeRect.Height, WorldRenderer.BITMAP_HEIGHT)));
                Rectangle drawRect = safeRect.AsRectangle();

                _worldRenderer.Update(drawRect, withInteractionOverlay: ShowInteraction.IsChecked.Value, withPointers: ShowPointers.IsChecked);
                _bitmap.WritePixels(sourceArea, _worldRenderer.GetRectangle(drawRect), safeRect.Width * 4, safeRect.X, safeRect.Y);
                _bitmap.AddDirtyRect(safeRect);
            }

            _bitmap.Unlock();

            Console.WriteLine("Draw time " + (DateTime.Now - speedTest).TotalMilliseconds + " milliseconds.");
        }

        private void ClearSelectionRectangle()
        {
            SelectionRectangle.Visibility = Visibility.Collapsed;
        }

        private void SetSelectionRectangle(Rectangle rect)
        {
            Canvas.SetLeft(SelectionRectangle, rect.X);
            Canvas.SetTop(SelectionRectangle, rect.Y);

            SelectionRectangle.Width = rect.Width;
            SelectionRectangle.Height = rect.Height;
            SelectionRectangle.Visibility = Visibility.Visible;
        }

        private WorldPointer _selectedPointer;
        private WorldObject _selectedObject;

        private EditMode _editMode;
        private DrawMode _drawMode;

        private bool _isDragging = false;
        private Point _dragStartPoint;

        private void WorldRenderSource_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point clickPoint = Snap(e.GetPosition(WorldRenderSource));

            WorldRenderSource.Focusable = true;
            WorldRenderSource.Focus();

            if (WorldEditorExitSelected != null)
            {
                WorldEditorExitSelected((int)clickPoint.X / 16, (int)clickPoint.Y / 16);
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (_world.Pointers.Where(o => o.BoundRectangle.Contains((int) clickPoint.X, (int)clickPoint.Y)).FirstOrDefault() != null)
                {
                    if (SelectedEditMode.SelectedIndex != 2)
                    {
                        if (ShowPointers.IsChecked == false)
                        {
                            ShowPointers.IsChecked = true;
                            Update();
                        }

                        SelectedEditMode.SelectedIndex = 2;
                    }
                }
                else if (_world.ObjectData.Where(o => o.BoundRectangle.Contains((int)clickPoint.X, (int)clickPoint.Y)).FirstOrDefault() != null)
                {
                    SelectedEditMode.SelectedIndex = 1;
                }
                else
                {
                    SelectedEditMode.SelectedIndex = 0;
                }
            }

            switch (_editMode)
            {
                case EditMode.Tiles:
                    HandleTileClick(e);
                    break;

                case EditMode.Objects:
                    HandleObjectClick(e);
                    break;

                case EditMode.Pointers:
                    HandlePointerClick(e);
                    break;
            }
        }

        private Point originalTilePoint;

        private void HandleTileClick(MouseButtonEventArgs e)
        {
            Point clickPoint = Snap(e.GetPosition(WorldRenderSource));

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                int x = (int)(clickPoint.X / 16), y = (int)(clickPoint.Y / 16);
                int tileValue = _worldDataAccessor.GetData(x, y);

                if (Keyboard.Modifiers == ModifierKeys.Shift)
                {
                    TileSelector.SelectedBlockValue = tileValue;
                    return;
                }

                _selectionMode = SelectionMode.SetTiles;

                if (_drawMode == DrawMode.Default)
                {
                    _dragStartPoint = clickPoint;
                    _isDragging = true;
                    originalTilePoint = clickPoint;
                }
                else if (_drawMode == DrawMode.Replace)
                {
                    if (tileValue != TileSelector.SelectedBlockValue)
                    {
                        _worldDataAccessor.ReplaceValue(tileValue, TileSelector.SelectedBlockValue);
                        Update();
                    }
                }
                else if (_drawMode == DrawMode.Fill)
                {
                    Stack<Point> stack = new Stack<Point>();
                    stack.Push(new Point(x, y));

                    int checkValue = _worldDataAccessor.GetData(x, y);
                    if (checkValue == TileSelector.SelectedBlockValue)
                    {
                        return;
                    }

                    int lowestX, highestX;
                    int lowestY, highestY;

                    lowestX = highestX = x;
                    lowestY = highestY = y;

                    while (stack.Count > 0)
                    {
                        Point point = stack.Pop();

                        int i = (int)(point.X);
                        int j = (int)(point.Y);

                        if (checkValue == _worldDataAccessor.GetData(i, j))
                        {
                            _worldDataAccessor.SetData(i, j, TileSelector.SelectedBlockValue);

                            if (i < lowestX)
                            {
                                lowestX = i;
                            }
                            if (i > highestX)
                            {
                                highestX = i;
                            }
                            if (j < lowestY)
                            {
                                lowestY = j;
                            }
                            if (j > highestY)
                            {
                                highestY = j;
                            }

                            stack.Push(new Point(i + 1, j));
                            stack.Push(new Point(i - 1, j));
                            stack.Push(new Point(i, j + 1));
                            stack.Push(new Point(i, j - 1));
                        }
                    }

                    TileChange tileChange = new TileChange(lowestX, lowestY, highestX - lowestX + 1, highestY - lowestY + 1);

                    for (int j = 0, row = lowestY; row <= highestY; row++, j++)
                    {
                        for (int i = 0, col = lowestX; col <= highestX; col++, i++)
                        {
                            tileChange.Data[i, j] = _worldDataAccessor.GetData(col, row) == TileSelector.SelectedBlockValue ? checkValue : -1;
                        }
                    }

                    _historyService.UndoTiles.Push(tileChange);

                    Update(new Rectangle(lowestX * 16, lowestY * 16, (highestX - lowestX + 1) * 16, (highestY - lowestY + 1) * 16));
                }
            }
            else if (e.RightButton == MouseButtonState.Pressed)
            {
                _selectionMode = SelectionMode.SelectTiles;

                int x = (int)(clickPoint.X / 16), y = (int)(clickPoint.Y / 16);

                _dragStartPoint = clickPoint;
                _isDragging = true;
                originalTilePoint = clickPoint;

                SetSelectionRectangle(new Rectangle(x * 16, y * 16, 16, 16));
            }
        }

        private void HandleObjectClick(MouseButtonEventArgs e)
        {
            Point tilePoint = e.GetPosition(WorldRenderSource);
            List<Rectangle> updatedRects = new List<Rectangle>();

            if (e.LeftButton == MouseButtonState.Pressed || e.RightButton == MouseButtonState.Pressed)
            {
                if (e.RightButton == MouseButtonState.Pressed)
                {
                    _selectedObject = _world.ObjectData.Where(o => o.BoundRectangle.Contains((int)tilePoint.X, (int)tilePoint.Y)).FirstOrDefault();

                    if (_selectedObject != null && ObjectSelector.SelectedObject != null)
                    {
                        _historyService.UndoWorldObjects.Push(new WorldObjectChange(_selectedObject, _selectedObject.X, _selectedObject.Y, _selectedObject.GameObjectId, WorldObjectChangeType.Update));
                        updatedRects.Add(_selectedObject.VisualRectangle);
                        _selectedObject.GameObject = ObjectSelector.SelectedObject;
                        _selectedObject.CalcBoundBox();
                        updatedRects.Add(_selectedObject.CalcVisualBox(true));
                    }
                    else
                    {
                        if (ObjectSelector.SelectedObject != null && _world.ObjectData.Count < 14)
                        {
                            WorldObject newObject = new WorldObject();
                            newObject.X = (int)(tilePoint.X / 16);
                            newObject.Y = (int)(tilePoint.Y / 16);
                            newObject.GameObject = ObjectSelector.SelectedObject;
                            newObject.GameObjectId = ObjectSelector.SelectedObject.GameId;
                            newObject.CalcBoundBox();

                            _world.ObjectData.Add(newObject);
                            _historyService.UndoWorldObjects.Push(new WorldObjectChange(newObject, newObject.X, newObject.Y, newObject.GameObjectId, WorldObjectChangeType.Addition));
                            Update(newObject.CalcVisualBox(true));
                        }
                    }
                }
                else
                {
                    _selectedObject = _world.ObjectData.Where(o => o.BoundRectangle.Contains((int)tilePoint.X,(int) tilePoint.Y)).FirstOrDefault();

                    if ((Keyboard.Modifiers & ModifierKeys.Shift) > 0 || (Keyboard.Modifiers & ModifierKeys.Control) > 0)
                    {
                        if (_selectedObject != null && !_selectedObject.GameObject.IsStartObject)
                        {
                            ObjectSelector.SelectedObject = _selectedObject.GameObject;
                        }
                    }
                }

                if (_selectedObject != null)
                {
                    _dragStartPoint = tilePoint;
                    SetSelectionRectangle(_selectedObject.BoundRectangle);
                    originalTilePoint = new Point(_selectedObject.X * 16, _selectedObject.Y * 16);
                    _isDragging = true;
                    _historyService.UndoWorldObjects.Push(new WorldObjectChange(_selectedObject, _selectedObject.X, _selectedObject.Y, _selectedObject.GameObjectId, WorldObjectChangeType.Update));
                }
                else
                {
                    ClearSelectionRectangle();
                }

                Update(updatedRects);
            }

            //UpdateSpriteStatus();
        }

        private void HandlePointerClick(MouseButtonEventArgs e)
        {
            Point tilePoint = e.GetPosition(WorldRenderSource);
            List<Rectangle> updatedRects = new List<Rectangle>();

            if (e.LeftButton == MouseButtonState.Pressed || e.RightButton == MouseButtonState.Pressed)
            {
                if (e.RightButton == MouseButtonState.Pressed)
                {
                    if (_world.Pointers.Count < 25)
                    {
                        _selectedPointer = new WorldPointer()
                        {
                            X = (int)tilePoint.X / 16,
                            Y = (int)tilePoint.Y / 16
                        };

                        _world.Pointers.Add(_selectedPointer);
                        updatedRects.Add(_selectedPointer.BoundRectangle);
                    }
                }
                else
                {
                    _selectedPointer = _world.Pointers.Where(o => o.BoundRectangle.Contains((int)tilePoint.X, (int)tilePoint.Y)).FirstOrDefault();
                }

                if (_selectedPointer != null)
                {
                    _dragStartPoint = tilePoint;
                    SetSelectionRectangle(_selectedPointer.BoundRectangle);
                    originalPointerPoint = new Point(_selectedPointer.X * 16, _selectedPointer.Y * 16);
                    _isDragging = true;

                    PointerEditor.Visibility = Visibility.Visible;
                    PointerEditor.SetPointer(_selectedPointer);
                }
                else
                {
                    PointerEditor.Visibility = Visibility.Collapsed;
                    ClearSelectionRectangle();
                }

                Update(updatedRects);
            }
        }

        private void WorldRenderSource_MouseMove(object sender, MouseEventArgs e)
        {
            Point tilePoint = Snap(e.GetPosition(WorldRenderSource));

            switch (_editMode)
            {
                case EditMode.Tiles:
                    HandleTileMove(e);
                    break;

                case EditMode.Objects:
                    HandleObjectMove(e);
                    break;

                case EditMode.Pointers:
                    HandlePointerMove(e);
                    break;
            }
        }

        private SelectionMode _selectionMode;

        private void HandleObjectMove(MouseEventArgs e)
        {
            Point movePoint = Snap(e.GetPosition(WorldRenderSource));

            if (_selectedObject != null && _isDragging)
            {
                Point diffPoint = Snap(new Point(movePoint.X - originalTilePoint.X, movePoint.Y - originalTilePoint.Y));

                List<Rectangle> updateRects = new List<Rectangle>();

                Rectangle oldRect = _selectedObject.VisualRectangle;

                if (oldRect.Right >= LevelRenderer.BITMAP_WIDTH)
                {
                    oldRect.Width = oldRect.Width - (oldRect.Right - LevelRenderer.BITMAP_WIDTH);
                }

                if (oldRect.Bottom >= LevelRenderer.BITMAP_HEIGHT)
                {
                    oldRect.Height = oldRect.Height - (oldRect.Bottom - LevelRenderer.BITMAP_HEIGHT);
                }

                updateRects.Add(oldRect);

                int newX = (int)((originalTilePoint.X + diffPoint.X) / 16);
                int newY = (int)((originalTilePoint.Y + diffPoint.Y) / 16);

                if (newX == _selectedObject.X && newY == _selectedObject.Y)
                {
                    return;
                }

                if (newX >= Level.BLOCK_WIDTH)
                {
                    newX = Level.BLOCK_WIDTH - 1;
                }

                if (newY >= Level.BLOCK_HEIGHT)
                {
                    newY = Level.BLOCK_HEIGHT - 1;
                }

                _selectedObject.X = newX;
                _selectedObject.Y = newY;
                _selectedObject.CalcBoundBox();
                _selectedObject.CalcVisualBox(true);

                Rectangle updateArea = _selectedObject.VisualRectangle;

                if (updateArea.Right >= LevelRenderer.BITMAP_WIDTH)
                {
                    updateArea.Width = updateArea.Width - (updateArea.Right - LevelRenderer.BITMAP_WIDTH);
                }

                if (updateArea.Bottom >= LevelRenderer.BITMAP_HEIGHT)
                {
                    updateArea.Height = updateArea.Height - (updateArea.Bottom - LevelRenderer.BITMAP_HEIGHT);
                }

                updateRects.Add(updateArea);

                SetSelectionRectangle(_selectedObject.BoundRectangle);
                Update(updateRects);
            }

            int blockX = (int)movePoint.X / 16, blockY = (int)movePoint.Y / 16;

            PointerXY.Text = "X: " + blockX.ToString("X2") + " Y: " + blockY.ToString("X2");
        }

        private void HandleTileMove(MouseEventArgs e)
        {
            Point tilePoint = Snap(e.GetPosition(WorldRenderSource));

            if (_isDragging && ((_selectionMode == SelectionMode.SetTiles && _drawMode == DrawMode.Default) ||
                               _selectionMode == SelectionMode.SelectTiles))
            {
                int x = (int)Math.Min(tilePoint.X, _dragStartPoint.X);
                int y = (int)Math.Min(tilePoint.Y, _dragStartPoint.Y);
                int width = (int)(Math.Max(tilePoint.X, _dragStartPoint.X)) - x;
                int height = (int)(Math.Max(tilePoint.Y, _dragStartPoint.Y)) - y;

                SetSelectionRectangle(new Rectangle(x, y, width + 16, height + 16));
            }
            else if (_selectionMode != SelectionMode.SelectTiles)
            {
                SetSelectionRectangle(new Rectangle((int)tilePoint.X, (int)tilePoint.Y, 16, 16));
            }

            int blockX = (int)tilePoint.X / 16, blockY = (int)tilePoint.Y / 16;
            int tileValue = _worldDataAccessor.GetData(blockX, blockY);
            if (tileValue == -1)
            {
                InteractionDescription.Text = "";
                TerrainDescription.Text = "";
                PointerXY.Text = "";
                TileValue.Text = "";
            }
            else
            {
                TileBlock tileBlock = _tileSet.TileBlocks[tileValue];
                MapTileInteraction tileInteraction = _interactions.Where(t => t.Value == tileBlock.Property).FirstOrDefault();
                InteractionDescription.Text = tileInteraction.Name;
                PointerXY.Text = "X: " + blockX.ToString("X2") + " Y: " + blockY.ToString("X2");
                TileValue.Text = tileValue.ToString("X");
            }
        }

        private Point originalPointerPoint;

        private void HandlePointerMove(MouseEventArgs e)
        {
            Point movePoint = Snap(e.GetPosition(WorldRenderSource));

            if (_selectedPointer != null && _isDragging)
            {
                Point diffPoint = Snap(new Point(movePoint.X - originalPointerPoint.X, movePoint.Y - originalPointerPoint.Y));

                List<Rectangle> updateRects = new List<Rectangle>();

                Rectangle oldRect = _selectedPointer.BoundRectangle;

                if (oldRect.Right >= WorldRenderer.BITMAP_WIDTH)
                {
                    oldRect.Width = oldRect.Width - (oldRect.Right - WorldRenderer.BITMAP_WIDTH);
                }

                if (oldRect.Bottom >= WorldRenderer.BITMAP_HEIGHT)
                {
                    oldRect.Height = oldRect.Height - (oldRect.Bottom - WorldRenderer.BITMAP_HEIGHT);
                }

                updateRects.Add(oldRect);

                int newX = (int)((originalPointerPoint.X + diffPoint.X) / 16);
                int newY = (int)((originalPointerPoint.Y + diffPoint.Y) / 16);

                if (newX == _selectedPointer.X && newY == _selectedPointer.Y)
                {
                    return;
                }

                _selectedPointer.X = newX;
                _selectedPointer.Y = newY;

                Rectangle updateArea = _selectedPointer.BoundRectangle;

                if (updateArea.Right >= WorldRenderer.BITMAP_WIDTH)
                {
                    updateArea.Width = updateArea.Width - (updateArea.Right - WorldRenderer.BITMAP_WIDTH);
                }

                if (updateArea.Bottom >= WorldRenderer.BITMAP_HEIGHT)
                {
                    updateArea.Height = updateArea.Height - (updateArea.Bottom - WorldRenderer.BITMAP_HEIGHT);
                }

                updateRects.Add(updateArea);

                SetSelectionRectangle(_selectedPointer.BoundRectangle);
                Update(updateRects);
            }

            int blockX = (int)movePoint.X / 16, blockY = (int)movePoint.Y / 16;
            PointerXY.Text = "X: " + blockX.ToString("X2") + " Y: " + blockY.ToString("X2");
        }

        private int Snap(double value)
        {
            return (int)(Math.Floor(value / 16) * 16);
        }

        private Point Snap(Point value)
        {
            return new Point(Snap(value.X), Snap(value.Y));
        }

        private int[,] _copyBuffer;

        private void CutSelection()
        {
            if (_selectionMode == SelectionMode.SelectTiles)
            {
                _copyBuffer = new int[(int)(SelectionRectangle.Width / 16), (int)(SelectionRectangle.Height / 16)];

                int startX = (int)(Canvas.GetLeft(SelectionRectangle) / 16);
                int endX = (int)(SelectionRectangle.Width / 16) + startX;
                int startY = (int)(Canvas.GetTop(SelectionRectangle) / 16);
                int endY = (int)(SelectionRectangle.Height / 16) + startY;
                for (int r = startY, y = 0; r < endY; r++, y++)
                {
                    for (int c = startX, x = 0; c < endX; c++, x++)
                    {
                        _copyBuffer[x, y] = _worldDataAccessor.GetData(c, r);
                        _worldDataAccessor.SetData(c, r, 0x00);
                    }
                }

                Update(new Rectangle((int)Canvas.GetLeft(SelectionRectangle),
                                (int)Canvas.GetTop(SelectionRectangle),
                                (int)SelectionRectangle.Width,
                                (int)SelectionRectangle.Height));

                SetSelectionRectangle(new Rectangle((endX - 1) * 16, (endY - 1) * 16, 16, 16));

                _selectionMode = SelectionMode.SetTiles;
            }
        }

        private void CopySelection()
        {
            if (_selectionMode == SelectionMode.SelectTiles)
            {
                _copyBuffer = new int[(int)(SelectionRectangle.Width / 16), (int)(SelectionRectangle.Height / 16)];

                int startX = (int)(Canvas.GetLeft(SelectionRectangle) / 16);
                int endX = (int)(SelectionRectangle.Width / 16) + startX;
                int startY = (int)(Canvas.GetTop(SelectionRectangle) / 16);
                int endY = (int)(SelectionRectangle.Height / 16) + startY;
                for (int r = startY, y = 0; r < endY; r++, y++)
                {
                    for (int c = startX, x = 0; c < endX; c++, x++)
                    {
                        _copyBuffer[x, y] = _worldDataAccessor.GetData(c, r);
                    }
                }

                SetSelectionRectangle(new Rectangle((endX - 1) * 16, (endY - 1) * 16, 16, 16));

                _selectionMode = SelectionMode.SetTiles;
            }
        }

        private void PasteSelection()
        {
            if (_copyBuffer != null)
            {
                int startX = (int)((Canvas.GetLeft(SelectionRectangle) + 2) / 16);
                int endX = (int)((SelectionRectangle.Width - 4) / 16) + startX;
                int startY = (int)((Canvas.GetTop(SelectionRectangle) + 2) / 16);
                int endY = (int)((SelectionRectangle.Height - 4) / 16) + startY;
                int bufferWidth = _copyBuffer.GetLength(0);
                int bufferHeight = _copyBuffer.GetLength(1);

                TileChange tileChange;

                if (startX + bufferWidth > Level.BLOCK_WIDTH)
                {
                    bufferWidth = Level.BLOCK_WIDTH - startX;
                }

                if (startY + bufferHeight > Level.BLOCK_HEIGHT)
                {
                    bufferHeight = Level.BLOCK_HEIGHT - startY;
                }

                if (SelectionRectangle.Width == 20 &&
                    SelectionRectangle.Height == 20)
                {
                    tileChange = new TileChange(startX, startY, bufferWidth, bufferHeight);

                    for (int r = startY, y = 0; y < bufferHeight; r++, y++)
                    {
                        for (int c = startX, x = 0; x < bufferWidth; c++, x++)
                        {
                            tileChange.Data[x, y] = _worldDataAccessor.GetData(c, r);
                            _worldDataAccessor.SetData(c, r, _copyBuffer[x % bufferWidth, y % bufferHeight]);
                        }
                    }

                    Update(new Rectangle(startX * 16, startY * 16, bufferWidth * 16, bufferHeight * 16));
                }
                else
                {
                    tileChange = new TileChange(startX, startY, endX - startX, endY - startY);

                    for (int r = startY, y = 0; r < endY; r++, y++)
                    {
                        for (int c = startX, x = 0; c < endX; c++, x++)
                        {
                            tileChange.Data[x, y] = _worldDataAccessor.GetData(c, r);

                            _worldDataAccessor.SetData(c, r, _copyBuffer[x % bufferWidth, y % bufferHeight]);
                        }
                    }

                    Update(new Rectangle((int)Canvas.GetLeft(SelectionRectangle) + 2,
                                    (int)Canvas.GetTop(SelectionRectangle) + 2,
                                    (int)(SelectionRectangle.Width - 4),
                                    (int)(SelectionRectangle.Height - 4)));
                }

                _historyService.UndoTiles.Push(tileChange);

                _selectionMode = SelectionMode.SetTiles;
                SetSelectionRectangle(new Rectangle((int)_dragStartPoint.X, (int)_dragStartPoint.Y, 16, 16));
            }
        }

        private void UndoTiles()
        {
            if (_historyService.UndoTiles.Count > 0)
            {
                TileChange undoTiles = _historyService.UndoTiles.Pop();
                _historyService.RedoTiles.Push(ApplyTileChange(undoTiles));
            }
        }

        private void RedoTiles()
        {
            if (_historyService.RedoTiles.Count > 0)
            {
                TileChange redoTiles = _historyService.RedoTiles.Pop();
                _historyService.UndoTiles.Push(ApplyTileChange(redoTiles));
            }
        }

        private void DeleteObject()
        {
            if (_selectedObject != null)
            {
                _world.ObjectData.Remove(_selectedObject);
                Update(_selectedObject.VisualRectangle);
                ClearSelectionRectangle();
                _historyService.UndoWorldObjects.Push(new WorldObjectChange(_selectedObject, _selectedObject.X, _selectedObject.Y, _selectedObject.GameObjectId, WorldObjectChangeType.Deletion));
            }

            //UpdateSpriteStatus();
        }

        private void DeletePointer()
        {
            if (_selectedPointer != null)
            {
                _world.Pointers.Remove(_selectedPointer);
                Update(_selectedPointer.BoundRectangle);
                ClearSelectionRectangle();
                PointerEditor.Visibility = Visibility.Collapsed;
            }
        }

        private void UndoObjects()
        {
            if (_historyService.UndoLevelObjects.Count > 0)
            {
                WorldObjectChange undoObject = _historyService.UndoWorldObjects.Pop();
                _historyService.RedoWorldObjects.Push(ApplyObjectChange(undoObject));
            }

            //UpdateSpriteStatus();
        }

        private void RedoObjects()
        {
            if (_historyService.RedoLevelObjects.Count > 0)
            {
                WorldObjectChange redoObject = _historyService.RedoWorldObjects.Pop();
                _historyService.UndoWorldObjects.Push(ApplyObjectChange(redoObject));
            }

            //UpdateSpriteStatus();
        }

        private void WorldRenderSource_MouseUp(object sender, MouseButtonEventArgs e)
        {
            switch (_editMode)
            {
                case EditMode.Tiles:
                    HandleTileRelease(e);
                    break;

                case EditMode.Objects:
                    HandleObjectRelease(e);
                    break;

                case EditMode.Pointers:
                    HandlePointerRelease(e);
                    break;
            }
        }

        private TileChange ApplyTileChange(TileChange tileChange)
        {
            int columnStart = tileChange.X;
            int rowStart = tileChange.Y;
            int columnEnd = tileChange.X + tileChange.Width;
            int rowEnd = tileChange.Y + tileChange.Height;

            TileChange reverseTileChange = new TileChange(columnStart, rowStart, columnEnd - columnStart, rowEnd - rowStart);
            for (int c = columnStart, i = 0; c < columnEnd; c++, i++)
            {
                for (int r = rowStart, j = 0; r < rowEnd; r++, j++)
                {
                    if (tileChange.Data[i, j] > -1)
                    {
                        reverseTileChange.Data[i, j] = _worldDataAccessor.GetData(c, r);
                        _worldDataAccessor.SetData(c, r, tileChange.Data[i, j]);
                    }
                }
            }

            Update(new Rectangle(columnStart * 16, rowStart * 16, (columnEnd - columnStart) * 16, (rowEnd - rowStart) * 16));
            return reverseTileChange;
        }

        private WorldObjectChange ApplyObjectChange(WorldObjectChange objectChange)
        {
            WorldObjectChange newChange = null;
            List<Rectangle> updateRects = new List<Rectangle>() { objectChange.OriginalObject.VisualRectangle };

            if (objectChange.ChangeType == WorldObjectChangeType.Addition)
            {
                newChange = new WorldObjectChange(objectChange.OriginalObject, objectChange.OriginalObject.X, objectChange.OriginalObject.Y, objectChange.OriginalObject.GameObjectId, WorldObjectChangeType.Deletion);
                _world.ObjectData.Remove(objectChange.OriginalObject);
                _selectedObject = null;
                ClearSelectionRectangle();
            }
            else if (objectChange.ChangeType == WorldObjectChangeType.Deletion)
            {
                newChange = new WorldObjectChange(objectChange.OriginalObject, objectChange.OriginalObject.X, objectChange.OriginalObject.Y, objectChange.OriginalObject.GameObjectId, WorldObjectChangeType.Addition);
                _world.ObjectData.Add(objectChange.OriginalObject);
                _selectedObject = objectChange.OriginalObject;
                SetSelectionRectangle(_selectedObject.CalcBoundBox());
            }
            else if (objectChange.ChangeType == WorldObjectChangeType.Update)
            {
                newChange = new WorldObjectChange(objectChange.OriginalObject, objectChange.OriginalObject.X, objectChange.OriginalObject.Y, objectChange.OriginalObject.GameObjectId, WorldObjectChangeType.Update);
                objectChange.OriginalObject.X = objectChange.X;
                objectChange.OriginalObject.Y = objectChange.Y;
                objectChange.OriginalObject.GameObjectId = objectChange.GameId;
                objectChange.OriginalObject.GameObject = _gameObjectService.GetObject(objectChange.GameId);
                updateRects.Add(objectChange.OriginalObject.CalcVisualBox(true));
                _selectedObject = objectChange.OriginalObject;
                SetSelectionRectangle(_selectedObject.CalcBoundBox());
            }

            Update(updateRects);
            return newChange;
        }

        private void HandleTileRelease(MouseButtonEventArgs e)
        {
            Point mousePoint = Snap(e.GetPosition(WorldRenderSource));

            if (_drawMode == DrawMode.Default && _isDragging)
            {
                _isDragging = false;

                if (_selectionMode == SelectionMode.SetTiles)
                {
                    int columnStart = (int)(Math.Min(originalTilePoint.X, mousePoint.X) / 16);
                    int rowStart = (int)(Math.Min(originalTilePoint.Y, mousePoint.Y) / 16);
                    int columnEnd = (int)(Math.Max(originalTilePoint.X, mousePoint.X) / 16);
                    int rowEnd = (int)(Math.Max(originalTilePoint.Y, mousePoint.Y) / 16);

                    TileChange tileChange = new TileChange(columnStart, rowStart, (columnEnd - columnStart) + 1, (rowEnd - rowStart) + 1);

                    for (int c = columnStart, i = 0; c <= columnEnd; c++, i++)
                    {
                        for (int r = rowStart, j = 0; r <= rowEnd; r++, j++)
                        {
                            tileChange.Data[i, j] = _worldDataAccessor.GetData(c, r);
                            _worldDataAccessor.SetData(c, r, TileSelector.SelectedBlockValue);
                        }
                    }

                    _historyService.UndoTiles.Push(tileChange);
                    _historyService.RedoTiles.Clear();

                    Update(new Rectangle(columnStart * 16, rowStart * 16, (columnEnd - columnStart + 1) * 16, (rowEnd - rowStart + 1) * 16));
                    SetSelectionRectangle(new Rectangle((int)mousePoint.X, (int)mousePoint.Y, 16, 16));
                }
                else if (_selectionMode == SelectionMode.SelectTiles)
                {
                    if (SelectionRectangle.Width == 16 && SelectionRectangle.Height == 16)
                    {
                        _selectionMode = SelectionMode.SetTiles;
                    }
                }
            }
        }


        private void HandleObjectRelease(MouseButtonEventArgs e)
        {
            _isDragging = false;
            if (_selectedObject != null)
            {
                if (_historyService.UndoLevelObjects.Count > 0)
                {
                    if (_historyService.UndoLevelObjects.Peek().IsSame())
                    {
                        _historyService.UndoLevelObjects.Pop();
                    }
                    else
                    {
                        _historyService.RedoLevelObjects.Clear();
                    }
                }
            }
        }

        private void HandlePointerRelease(MouseButtonEventArgs e)
        {
            if (_selectedPointer != null && _isDragging)
            {
                PointerEditor.Visibility = Visibility.Visible;
            }

            _isDragging = false;
        }

        private void GameObjectService_GameObjectsUpdated(GameObject gameObject)
        {
            List<WorldObject> affectedObjects = _world.ObjectData.Where(l => l.GameObjectId == gameObject.GameId).ToList();
            List<Rectangle> affectedRects = new List<Rectangle>();

            foreach (WorldObject worldObject in affectedObjects)
            {
                worldObject.GameObject = gameObject;
                worldObject.GameObjectId = gameObject.GameId;
                worldObject.CalcBoundBox();
                worldObject.CalcVisualBox(true);
                affectedRects.Add(worldObject.VisualRectangle);
            }

            if (affectedRects.Count > 0)
            {
                Update(affectedRects);
            }
        }

        private void UpdateTextTables()
        {
            List<KeyValuePair<string, string>> _graphicsSetNames = new List<KeyValuePair<string, string>>();
            for (int i = 0; i < 256; i++)
            {
                _graphicsSetNames.Add(new KeyValuePair<string, string>(i.ToString("X"), "0x" + (i * 0x400).ToString("X")));
            }

            Music.ItemsSource = _textService.GetTable("music").OrderBy(kv => kv.Value);
            PaletteIndex.ItemsSource = _palettesService.GetPalettes();
            GraphicsSet.ItemsSource = _graphicsSetNames;
            Screens.ItemsSource = new int[4] { 1, 2, 3, 4 };

            Music.SelectedValue = _world.MusicValue.ToString("X");
            PaletteIndex.SelectedValue = _world.PaletteId;
            GraphicsSet.SelectedValue = _world.TileTableIndex.ToString("X");
            Screens.SelectedIndex = _world.ScreenLength - 1;
        }

        private void GraphicsSet_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _world.TileTableIndex = int.Parse(GraphicsSet.SelectedValue.ToString(), System.Globalization.NumberStyles.HexNumber);
            _graphicsAccessor.SetBottomTable(_graphicsService.GetTileSection(_world.TileTableIndex));
            TileSelector.Update();
            Update();
        }

        private void Music_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _world.MusicValue = int.Parse(Music.SelectedValue.ToString(), System.Globalization.NumberStyles.HexNumber);
        }

        private void PaletteIndex_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PaletteIndex.SelectedItem != null)
            {
                _world.PaletteId = ((Palette)PaletteIndex.SelectedItem).Id;

                Palette palette = _palettesService.GetPalette(_world.PaletteId);
                _worldRenderer.Update(palette: palette);
                TileSelector.Update(palette: palette);
                Update();
            }
        }

        private void Screens_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _world.ScreenLength = int.Parse(Screens.SelectedValue.ToString());
            CanvasContainer.Width = RenderContainer.Width = _world.ScreenLength * 16 * 16;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveWorld();
        }

        private void SaveWorld()
        {
            _world.CompressedData = _compressionService.CompressWorld(_world);
            _worldService.SaveWorld(_world);

            using (MemoryStream ms = new MemoryStream(_worldRenderer.GetRectangle(new Rectangle(16, 0, 256, 256))))
            {
                _worldService.GenerateMetaData(_tileService, _worldInfo, ms);
            }

            AlertWindow.Alert(_world.Name + " has been saved!");
        }

        private void TileSelector_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount >= 2)
            {
                GlobalPanels.EditTileBlock(_world.Id, TileSelector.SelectedBlockValue);
            }
        }

        private void WorldRenderSource_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                switch (e.Key)
                {
                    case Key.S:
                        SaveWorld();
                        break;

                    case Key.C:
                        if (_editMode == EditMode.Tiles)
                        {
                            CopySelection();
                        }
                        break;

                    case Key.V:
                        if (_editMode == EditMode.Tiles)
                        {
                            PasteSelection();
                        }
                        break;

                    case Key.X:
                        if (_editMode == EditMode.Tiles)
                        {
                            CutSelection();
                        }
                        break;

                    case Key.Z:
                        if (_editMode == EditMode.Tiles)
                        {
                            UndoTiles();
                        }
                        else if (_editMode == EditMode.Objects)
                        {
                            UndoObjects();
                        }
                        break;

                    case Key.Y:
                        if (_editMode == EditMode.Tiles)
                        {
                            RedoTiles();
                        }
                        else if (_editMode == EditMode.Objects)
                        {
                            RedoObjects();
                        }
                        break;
                }
            }

            switch (e.Key)
            {
                case Key.Delete:
                    if (_editMode == EditMode.Pointers)
                    {
                        DeletePointer();
                    }
                    else if (_editMode == EditMode.Objects)
                    {
                        DeleteObject();
                    }
                    break;
            }
        }

        private void WorldRenderSource_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (!PointerEditor.IsMouseOver && WorldScroller.IsMouseOver)
            {
                WorldRenderSource.Focus();
            }
        }

        private void DrawMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (SelectedDrawMode.SelectedIndex)
            {
                case 0:
                    _drawMode = DrawMode.Default;
                    break;

                case 1:
                    _drawMode = DrawMode.Fill;
                    break;

                case 2:
                    _drawMode = DrawMode.Replace;
                    break;
            }
        }

        private void SelectedEditMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectionRectangle != null) SelectionRectangle.Visibility = Visibility.Collapsed;

            switch (SelectedEditMode.SelectedIndex)
            {
                case 0:
                    _editMode = EditMode.Tiles;
                    PointerEditor.Visibility = Visibility.Collapsed;
                    break;

                case 1:
                    _editMode = EditMode.Objects;
                    PointerEditor.Visibility = Visibility.Collapsed;
                    break;

                case 2:
                    _editMode = EditMode.Pointers;
                    ShowPointers.IsChecked = true;
                    Update();
                    break;
            }
        }

        private void ShowPSwitch_Checked(object sender, RoutedEventArgs e)
        {
            Update();
        }

        private void ShowInteraction_Click(object sender, RoutedEventArgs e)
        {
            TileSelector.Update(withMapInteractionOverlay: ShowInteraction.IsChecked.Value);
            Update();
        }

        private void ShowGrid_Click(object sender, RoutedEventArgs e)
        {
            _worldRenderer.RenderGrid = ShowGrid.IsChecked.Value;
            Update();
        }

        private void ShowPointers_Click(object sender, RoutedEventArgs e)
        {
            Update();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            GlobalPanels.OpenPaletteEditor((Palette)PaletteIndex.SelectedItem);
        }

        private void ObjectSelector_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SelectedEditMode.SelectedIndex = 1;
        }

        public void HandleKeyDown(KeyEventArgs e)
        {
            WorldRenderSource_KeyDown(null, e);
        }
    }
}