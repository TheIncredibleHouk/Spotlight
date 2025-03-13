using Spotlight.Abstractions;
using Spotlight.Models;
using Spotlight.Renderers;
using Spotlight.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using Rectangle = System.Drawing.Rectangle;

namespace Spotlight
{
    /// <summary>
    /// Interaction logic for LevelPanel.xaml
    /// </summary>
    public partial class LevelPanel : UserControl, IKeyDownHandler
    {
        private ILevelService _levelService;
        private IPaletteService _palettesService;
        private ITextService _textService;
        private IGraphicsService _graphicsService;
        private ITileService _tileService;
        private ICompressionService _compressionService;
        private IGameObjectService _gameObjectService;
        private IGraphicsManager _graphicsManager;
        private ILevelDataManager _levelDataManager;
        private IHistoryService _historyService;
        private IClipboardService _clipBoardService;
        private IEventService _eventService;
        private ILevelRenderer _levelRenderer;

        private Level _level;
        private LevelInfo _levelInfo;
        private TileSet _tileSet;
        private WriteableBitmap _bitmap;
        private WriteableBitmap _cursorBitmap;
        private List<TileTerrain> _terrain;

        public delegate void LevelEditorExitSelectedHandled(int x, int y);

        public event LevelEditorExitSelectedHandled LevelEditorExitSelected;

        private bool _initializing = true;
        private List<Guid> _subscriptions;

        public LevelPanel(IProjectService projectService,
                        IGraphicsService graphicsService,
                        IPaletteService palettesService,
                        ITextService textService,
                        ITileService tileService,
                        IGameObjectService gameObjectService,
                        ILevelService levelService,
                        IClipboardService clipBoardService,
                        IHistoryService historyService,
                        ICompressionService compressionService,
                        IEventService eventService,
                        IGraphicsManager graphicsManager,
                        ILevelDataManager levelDataManager,
                        ILevelRenderer levelRenderer)
        {
            InitializeComponent();


            _textService = textService;
            _graphicsService = graphicsService;
            _gameObjectService = gameObjectService;
            _tileService = tileService;
            _palettesService = palettesService;
            _levelService = levelService;
            _historyService = historyService;
            _compressionService = compressionService;
            _clipBoardService = clipBoardService;
            _eventService = eventService;
            _graphicsManager = graphicsManager;
            _levelDataManager = levelDataManager;
            _levelRenderer = levelRenderer;

            _subscriptions = new List<Guid>();
            InitializeUI();
        }
        
        public void Initialize(LevelInfo levelInfo)
        {
            _terrain = _tileService.GetTerrain();
            _level = _levelService.LoadLevel(levelInfo);
            _tileSet = _tileService.GetTileSet(_level.TileSetIndex);
            _initializing = false;
        }

        private void InitializeUI()
        {
            Dpi dpi = this.GetDpi();
            _bitmap = new WriteableBitmap(LevelRenderer.BITMAP_WIDTH, LevelRenderer.BITMAP_HEIGHT, dpi.X, dpi.Y, PixelFormats.Bgra32, null);

            LevelRenderSource.Source = _bitmap;
            CanvasContainer.Width = LevelRenderSource.Width = _bitmap.PixelWidth;
            CanvasContainer.Height = LevelRenderSource.Height = _bitmap.PixelHeight;

            LevelClip.Width = _level.ScreenLength * 16 * 16;

            _cursorBitmap = new WriteableBitmap(16, 16, dpi.X, dpi.Y, PixelFormats.Bgra32, null);
            CursorImage.ImageSource = _cursorBitmap;

            SelectedEditMode.SelectedIndex = SelectedDrawMode.SelectedIndex = 0;

            PaletteIndex.ItemsSource = _palettesService.GetPalettes();
            PaletteIndex.SelectedItem = _palettesService.GetPalette(_level.PaletteId);

            NoStars.SelectedIndex = _level.NoStars ? 1 : 0;
            ExtendedSpace.SelectedIndex = _levelInfo.SaveToExtendedSpace ? 1 : 0;

            UpdateTextTables();
        }

        private void InitialRender()
        {
            _levelRenderer.Ready();
            _levelRenderer.Update(tileSet: _tileSet, palette: _palettesService.GetPalette(_level.PaletteId));
            Update();
        }

        private void InitializeGameObjectData()
        {
            _level.FirstObjectData.ForEach(o =>
            {
                o.GameObject = _gameObjectService.GetObjectById(o.GameObjectId);
                o.CalcBoundBox();
                o.CalcVisualBox(true);
            });

            _level.SecondObjectData.ForEach(o =>
            {
                o.GameObject = _gameObjectService.GetObjectById(o.GameObjectId);
                o.CalcBoundBox();
                o.CalcVisualBox(true);
            });

            _level.FirstObjectData.Insert(0, new LevelObject()
            {
                GameObject = _gameObjectService.GetStartPointObject(),
                X = _level.StartX,
                Y = _level.StartY
            });
        }

        private void Initialize()
        {

            Tile[] staticSet = _graphicsService.GetTileSection(_level.StaticTileTableIndex);
            Tile[] animationSet = _graphicsService.GetTileSection(_level.AnimationTileTableIndex);
            Palette palette = _palettesService.GetPalette(_level.PaletteId);

            _graphicsManager.Initialize(staticSet, animationSet, _graphicsService.GetGlobalTiles(), _graphicsService.GetExtraTiles());
            _levelRenderer.Initialize(_tileService.GetTerrain());
            _levelDataManager.Initialize(_level, _tileSet);

            TileSelector.Initialize(_graphicsManager, _tileService, _tileSet, palette);
            ObjectSelector.Initialize(_gameObjectService, _palettesService, _eventService, _graphicsManager, palette);
            PointerEditor.Initialize(_levelService, _levelInfo);
        }

        private void SubscribeToEvents()
        {
            _subscriptions.Add(_eventService.Subscribe<GameObject>(SpotlightEventType.GameObjectsUpdated, GameObjectsUpdated));
            _subscriptions.Add(_eventService.Subscribe(SpotlightEventType.GraphicsUpdated, GraphicsUpdated));
            _subscriptions.Add(_eventService.Subscribe(SpotlightEventType.ExtraGraphicsUpdated, GraphicsUpdated));
            _subscriptions.Add(_eventService.Subscribe(SpotlightEventType.PaletteAdded, PaletteAdded));
            _subscriptions.Add(_eventService.Subscribe<Palette>(SpotlightEventType.PaletteUpdated, _level.PaletteId, PaletteUpdated));
            _subscriptions.Add(_eventService.Subscribe<Palette>(SpotlightEventType.PaletteRemoved, PaletteRemoved));
            _subscriptions.Add(_eventService.Subscribe<TileSet>(SpotlightEventType.TileSetUpdated, _level.TileSetIndex, TileSetUpdated));
            _subscriptions.Add(_eventService.Subscribe<TileSet>(SpotlightEventType.LevelUpdated, _level.Id, TileSetUpdated));
        }

        private void UnsubscribeFromEvents()
        {
            foreach(Guid subscriptionId in _subscriptions)
            {
                _eventService.Unsubscribe(subscriptionId);
            }
        }

        public void LoadLevel(LevelInfo levelInfo)
        {
            _levelInfo = levelInfo;
        }

        private void LevelUpdated(LevelInfo levelInfo)
        {
            if (levelInfo.Id == _level.Id)
            {
                _level.Name = levelInfo.Name;
            }
        }

        private void TileSetUpdated(TileSet tileSet)
        {
            Update();
            TileSelector.Update(_tileSet);
        }

        //public void DetachEvents()
        //{
        //    _gameObjectService.GameObjectUpdated -= GameObjectService_GameObjectsUpdated;
        //    ObjectSelector.GameObjectDoubleClicked -= ObjectSelector_GameObjectDoubleClicked;
        //    _graphicsService.GraphicsUpdated -= _graphicsService_GraphicsUpdated;
        //    _graphicsService.ExtraGraphicsUpdated -= _graphicsService_GraphicsUpdated;
        //    _palettesService.PalettesChanged -= _palettesService_PalettesChanged;
        //    _levelService.LevelUpdated -= _levelService_LevelUpdated;
        //    ObjectSelector.DetachEvents();
        //}

        private void PaletteAdded()
        {

            //_levelRenderer.Update(palette: (Palette)PaletteIndex.SelectedItem);
            //TileSelector.Update(palette: (Palette)PaletteIndex.SelectedItem);
            //ObjectSelector.Update((Palette)PaletteIndex.SelectedItem);
            //Update();
        }

        private void PaletteRemoved(Palette palette)
        {
            if (palette.Id == _level.PaletteId)
            {
                Palette newPalette = _palettesService.GetPalettes().FirstOrDefault();
                _level.PaletteId = newPalette.Id;
                PaletteUpdated(newPalette);
            }
        }

        private void PaletteUpdated(Palette palette)
        {
            TileSelector.Update(palette: (Palette)PaletteIndex.SelectedItem);
            ObjectSelector.Update((Palette)PaletteIndex.SelectedItem);
            Update();
        }

        private void GraphicsUpdated()
        {
            _graphicsManager.SetTopTable(_graphicsService.GetTileSection(_level.StaticTileTableIndex));
            _graphicsManager.SetBottomTable(_graphicsService.GetTileSection(_level.AnimationTileTableIndex));
            _graphicsManager.SetGlobalTiles(_graphicsService.GetGlobalTiles(), _graphicsService.GetExtraTiles());
            TileSelector.Update();
            Update();
        }

        private void GameObjectsUpdated(GameObject gameObject)
        {
            List<LevelObject> affectedObjects = _level.ObjectData.Where(l => l.GameObjectId == gameObject.GameId).ToList();
            List<Rectangle> affectedRects = new List<Rectangle>();

            foreach (LevelObject levelObject in affectedObjects)
            {
                levelObject.GameObject = gameObject;
                levelObject.GameObjectId = gameObject.GameId;
                levelObject.CalcBoundBox();
                levelObject.CalcVisualBox(true);
                affectedRects.Add(levelObject.VisualRectangle);
            }

            if (affectedRects.Count > 0)
            {
                Update(affectedRects);
            }
        }

        private void Update(Rectangle updateRect)
        {
            Update(new List<Rectangle>() { updateRect });
        }

        private void Update(int x = 0, int y = 0, int width = LevelRenderer.BITMAP_WIDTH, int height = LevelRenderer.BITMAP_HEIGHT)
        {
            Update(new List<Rectangle>() { new Rectangle(x, y, width, height) });
        }

        private void Update(List<Rectangle> updateAreas)
        {
            if (updateAreas.Count == 0 || _initializing)
            {
                return;
            }

            _bitmap.Lock();

            foreach (var updateArea in updateAreas.Select(r => new Int32Rect((int)r.X, (int)r.Y, (int)r.Width, (int)r.Height)))
            {
                Int32Rect safeRect = new Int32Rect(Math.Max(0, updateArea.X), Math.Max(0, updateArea.Y), updateArea.Width, updateArea.Height);

                if (safeRect.X + safeRect.Width > LevelRenderer.BITMAP_WIDTH)
                {
                    safeRect.Width = LevelRenderer.BITMAP_WIDTH - safeRect.X;
                }

                if (safeRect.Y + safeRect.Height > LevelRenderer.BITMAP_HEIGHT)
                {
                    safeRect.Height = LevelRenderer.BITMAP_HEIGHT - safeRect.Y;
                }

                Int32Rect sourceArea = new Int32Rect(0, 0, Math.Max(0, Math.Min(safeRect.Width, LevelRenderer.BITMAP_WIDTH)), Math.Max(0, Math.Min(safeRect.Height, LevelRenderer.BITMAP_HEIGHT)));

                Rectangle drawRectangle = safeRect.AsRectangle();
                _levelRenderer.Update(drawRectangle, true, ShowTerrain.IsChecked.Value, ShowInteraction.IsChecked.Value, _highlightedTile, ShowStrategy.IsChecked.Value);
                _bitmap.WritePixels(sourceArea, _levelRenderer.GetRectangle(drawRectangle), safeRect.Width * 4, safeRect.X, safeRect.Y);
                _bitmap.AddDirtyRect(safeRect);
            }

            _bitmap.Unlock();
        }

        private void ClearSelectionRectangle()
        {
            GameObjectProperty.Visibility = SelectionRectangle.Visibility = Visibility.Collapsed;
        }

        private void SetSelectionRectangle(Rectangle rect)
        {
            Canvas.SetLeft(SelectionRectangle, rect.X - 2);
            Canvas.SetTop(SelectionRectangle, rect.Y - 2);

            SelectionRectangle.Width = rect.Width + 4;
            SelectionRectangle.Height = rect.Height + 4;
            SelectionRectangle.Visibility = Visibility.Visible;
        }

        private LevelObject _selectedObject;
        private LevelPointer _selectedPointer;

        private EditMode _editMode;
        private DrawMode _drawMode;

        private bool _isDragging = false;
        private Point _dragStartPoint;

        private void LevelRenderSource_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point clickPoint = e.GetPosition(LevelRenderSource);

            LevelRenderSource.Focusable = true;
            LevelRenderSource.Focus();

            if (LevelEditorExitSelected != null)
            {
                LevelEditorExitSelected((int)clickPoint.X / 16, (int)clickPoint.Y / 16);
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (_level.LevelPointers.Where(o => o.BoundRectangle.Contains((int)clickPoint.X, (int)clickPoint.Y)).FirstOrDefault() != null)
                {
                    SelectedEditMode.SelectedIndex = 2;
                    CursorImage.Opacity = 0;
                    _editMode = EditMode.Pointers;
                }
                else if (_level.ObjectData.Where(o => o.BoundRectangle.Contains((int)clickPoint.X, (int)clickPoint.Y)).FirstOrDefault() != null)
                {
                    SelectedEditMode.SelectedIndex = 1;
                    CursorImage.Opacity = 0;
                    _editMode = EditMode.Objects;
                    PointerEditor.Visibility = Visibility.Collapsed;
                }
                else
                {
                    SelectedEditMode.SelectedIndex = 0;
                    _selectionMode = SelectionMode.SetTiles;
                    CursorImage.Opacity = .75;
                    _editMode = EditMode.Tiles;
                    SelectionRectangle.Visibility = Visibility.Visible;
                    PointerEditor.Visibility = Visibility.Collapsed;
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

            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                LevelObject startObject = _level.ObjectData.Where(o => o.GameObject.IsStartObject).First();
                List<Rectangle> updateRects = new List<Rectangle>();

                updateRects.Add(startObject.VisualRectangle);

                startObject.X = (int)(clickPoint.X / 16);
                startObject.Y = (int)(clickPoint.Y / 16);

                startObject.CalcBoundBox();
                startObject.CalcVisualBox(true);

                _level.StartX = startObject.X;
                _level.StartY = startObject.Y;

                updateRects.Add(startObject.VisualRectangle);
                Update(updateRects);

            }
        }

        private void HandleTileClick(MouseButtonEventArgs e)
        {
            Point clickPoint = Snap(e.GetPosition(LevelRenderSource));

            if (e.LeftButton == MouseButtonState.Pressed && Keyboard.Modifiers != ModifierKeys.Alt)
            {
                CursorImage.Opacity = .75;

                int x = (int)(clickPoint.X / 16), y = (int)(clickPoint.Y / 16);
                int tileValue = _levelDataManager.GetData(x, y);

                if (Keyboard.Modifiers == ModifierKeys.Shift || Keyboard.Modifiers == ModifierKeys.Control)
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
                        TileChange tileChange = new TileChange(0, 0, Level.BLOCK_WIDTH, Level.BLOCK_HEIGHT);
                        for (int row = 0; row < Level.BLOCK_HEIGHT; row++)
                        {
                            for (int col = 0; col < Level.BLOCK_WIDTH; col++)
                            {
                                tileChange.Data[col, row] = _levelDataManager.GetData(col, row);
                            }
                        }

                        _historyService.UndoTiles.Push(tileChange);

                        _levelDataManager.ReplaceValue(tileValue, TileSelector.SelectedBlockValue);
                        Update();
                    }
                }
                else if (_drawMode == DrawMode.Fill)
                {
                    Stack<Point> stack = new Stack<Point>();
                    stack.Push(new Point(x, y));

                    int checkValue = _levelDataManager.GetData(x, y);
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
                        int currentTileValue;

                        if (checkValue == (currentTileValue = _levelDataManager.GetData(i, j)) && currentTileValue > -1)
                        {
                            _levelDataManager.SetData(i, j, TileSelector.SelectedBlockValue);

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
                            tileChange.Data[i, j] = _levelDataManager.GetData(col, row) == TileSelector.SelectedBlockValue ? checkValue : -1;
                        }
                    }

                    _historyService.UndoTiles.Push(tileChange);

                    Update(new Rectangle(lowestX * 16, lowestY * 16, (highestX - lowestX + 1) * 16, (highestY - lowestY + 1) * 16));
                }
            }
            else if (e.RightButton == MouseButtonState.Pressed || (e.LeftButton == MouseButtonState.Pressed && Keyboard.Modifiers == ModifierKeys.Alt))
            {
                _selectionMode = SelectionMode.SelectTiles;

                int x = (int)(clickPoint.X / 16), y = (int)(clickPoint.Y / 16);

                _dragStartPoint = clickPoint;
                _isDragging = true;
                originalTilePoint = clickPoint;
                CursorImage.Opacity = 0;

                SetSelectionRectangle(new Rectangle(x * 16, y * 16, 16, 16));
            }
        }

        private void HandleObjectClick(MouseButtonEventArgs e)
        {
            Point tilePoint = e.GetPosition(LevelRenderSource);
            List<Rectangle> updatedRects = new List<Rectangle>();

            if (e.LeftButton == MouseButtonState.Pressed || e.RightButton == MouseButtonState.Pressed)
            {
                if (e.RightButton == MouseButtonState.Pressed)
                {
                    _selectedObject = _level.ObjectData.Where(o => o.BoundRectangle.Contains((int)tilePoint.X, (int)tilePoint.Y)).FirstOrDefault();
                    if (_selectedObject != _level.ObjectData[0])
                    {
                        if (_selectedObject != null && ObjectSelector.SelectedObject != null)
                        {
                            _historyService.UndoLevelObjects.Push(new LevelObjectChange(_selectedObject, _selectedObject.X, _selectedObject.Y, _selectedObject.Property, _selectedObject.GameObjectId, LevelObjectChangeType.Update));
                            updatedRects.Add(_selectedObject.VisualRectangle);
                            _selectedObject.GameObject = ObjectSelector.SelectedObject;
                            _selectedObject.CalcBoundBox();
                            updatedRects.Add(_selectedObject.CalcVisualBox(true));
                        }
                        else
                        {
                            if (ObjectSelector.SelectedObject != null && _level.ObjectData.Count < 48)
                            {
                                LevelObject newObject = new LevelObject();
                                newObject.X = (int)(tilePoint.X / 16);
                                newObject.Y = (int)(tilePoint.Y / 16);
                                newObject.GameObject = ObjectSelector.SelectedObject;
                                newObject.GameObjectId = ObjectSelector.SelectedObject.GameId;
                                newObject.CalcBoundBox();

                                _level.ObjectData.Add(newObject);
                                _historyService.UndoLevelObjects.Push(new LevelObjectChange(newObject, newObject.X, newObject.Y, newObject.Property, newObject.GameObjectId, LevelObjectChangeType.Addition));
                                Update(newObject.CalcVisualBox(true));
                            }
                        }
                    }
                }
                else
                {
                    _selectedObject = _level.ObjectData.Where(o => o.BoundRectangle.Contains((int)tilePoint.X, (int)tilePoint.Y)).FirstOrDefault();

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
                    _historyService.UndoLevelObjects.Push(new LevelObjectChange(_selectedObject, _selectedObject.X, _selectedObject.Y, _selectedObject.Property, _selectedObject.GameObjectId, LevelObjectChangeType.Update));

                    if (_selectedObject.GameObject.Properties != null && _selectedObject.GameObject.Properties.Count > 0)
                    {
                        GameObjectProperty.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        GameObjectProperty.Visibility = Visibility.Collapsed;
                    }

                    Rectangle boundRect = _selectedObject.BoundRectangle;
                    if (boundRect.Bottom <= 200)
                    {
                        Canvas.SetTop(GameObjectProperty, boundRect.Bottom + 4);
                    }
                    else
                    {
                        Canvas.SetTop(GameObjectProperty, boundRect.Top - 48);
                    }

                    Canvas.SetLeft(GameObjectProperty, boundRect.Left - 10);

                    GameObjectProperty.ItemsSource = _selectedObject.GameObject.Properties;
                    GameObjectProperty.SelectedIndex = _selectedObject.Property;
                    if (_selectedObject.GameObject.Properties != null && _selectedObject.GameObject.Properties.Count > 0)
                    {
                        GameObjectProperty.Width = _selectedObject.GameObject.Properties.Max(prop => prop.Length) * 5 + 80;
                    }
                }
                else
                {
                    ClearSelectionRectangle();
                }

                Update(updatedRects);
            }

            UpdateSpriteStatus();
        }

        private void UpdateSpriteStatus()
        {
            if (_selectedObject == null)
            {
                SpriteDescription.Text = "None";
            }
            else
            {
                SpriteDescription.Text = _selectedObject.GameObject.Name;
            }
        }
        private void HandlePointerClick(MouseButtonEventArgs e)
        {
            Point tilePoint = e.GetPosition(LevelRenderSource);
            List<Rectangle> updatedRects = new List<Rectangle>();

            if (e.LeftButton == MouseButtonState.Pressed || e.RightButton == MouseButtonState.Pressed)
            {
                if (e.RightButton == MouseButtonState.Pressed)
                {
                    if (_level.LevelPointers.Count < 10)
                    {
                        _selectedPointer = new LevelPointer()
                        {
                            X = (int)tilePoint.X / 16,
                            Y = (int)tilePoint.Y / 16
                        };

                        _level.LevelPointers.Add(_selectedPointer);
                        updatedRects.Add(_selectedPointer.BoundRectangle);
                    }
                }
                else
                {
                    _selectedPointer = _level.LevelPointers.Where(o => o.BoundRectangle.Contains((int)tilePoint.X, (int)tilePoint.Y)).FirstOrDefault();
                }

                if (_selectedPointer != null)
                {
                    _dragStartPoint = tilePoint;
                    SetSelectionRectangle(_selectedPointer.BoundRectangle);
                    originalPointerPoint = new Point(_selectedPointer.X * 16, _selectedPointer.Y * 16);
                    _isDragging = true;

                    PointerEditor.Visibility = Visibility;

                    Rectangle boundRect = _selectedPointer.BoundRectangle;
                    double leftEdge = (boundRect.Left - PointerEditor.ActualWidth / 2) + 16;

                    if (leftEdge < 0)
                    {
                        leftEdge = 0;
                    }

                    if (leftEdge + PointerEditor.Width >= LevelRenderer.BITMAP_WIDTH)
                    {
                        leftEdge = LevelRenderer.BITMAP_WIDTH - PointerEditor.ActualWidth;
                    }

                    if (boundRect.Bottom <= 200)
                    {
                        Canvas.SetTop(PointerEditor, boundRect.Bottom + 4);
                    }
                    else
                    {
                        Canvas.SetTop(PointerEditor, boundRect.Top - 160);
                    }
                    Canvas.SetLeft(PointerEditor, leftEdge);

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

        private void LevelRenderSource_MouseMove(object sender, MouseEventArgs e)
        {
            Point tilePoint = Snap(e.GetPosition(LevelRenderSource));

            if (_isDragging && Mouse.LeftButton != MouseButtonState.Pressed && Mouse.RightButton != MouseButtonState.Pressed)
            {
                MouseButtonEventArgs args = new MouseButtonEventArgs(e.MouseDevice, e.Timestamp, MouseButton.Left);
                LevelRenderSource_MouseUp(sender, args);
            }
            else
            {
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
        }

        private SelectionMode _selectionMode;

        private void HandleTileMove(MouseEventArgs e)
        {
            Point tilePoint = Snap(e.GetPosition(LevelRenderSource));

            if (_isDragging && ((_selectionMode == SelectionMode.SetTiles && _drawMode == DrawMode.Default) || _selectionMode == SelectionMode.SelectTiles))
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
            int tileValue = _levelDataManager.GetData(blockX, blockY);
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
                TileTerrain tileTerrain = _terrain.Where(t => t.Value == (tileBlock.Property & TileTerrain.Mask)).FirstOrDefault();
                InteractionDescription.Text = tileTerrain.Interactions.Where(t => t.Value == (tileBlock.Property & TileInteraction.Mask)).FirstOrDefault().Name;
                TerrainDescription.Text = tileTerrain.Name;
                PointerXY.Text = "X: " + blockX.ToString("X2") + " Y: " + blockY.ToString("X2");
                TileValue.Text = tileValue.ToString("X");
            }
        }

        private Point originalTilePoint;

        private void HandleObjectMove(MouseEventArgs e)
        {
            Point movePoint = Snap(e.GetPosition(LevelRenderSource));

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

                GameObjectProperty.Visibility = Visibility.Collapsed;

                updateRects.Add(updateArea);

                SetSelectionRectangle(_selectedObject.BoundRectangle);
                Update(updateRects);
            }

            int blockX = (int)movePoint.X / 16, blockY = (int)movePoint.Y / 16;

            PointerXY.Text = "X: " + blockX.ToString("X2") + " Y: " + blockY.ToString("X2");
        }

        private Point originalPointerPoint;

        private void HandlePointerMove(MouseEventArgs e)
        {
            Point movePoint = Snap(e.GetPosition(LevelRenderSource));

            if (_selectedPointer != null && _isDragging)
            {
                Point diffPoint = Snap(new Point(movePoint.X - originalPointerPoint.X, movePoint.Y - originalPointerPoint.Y));

                List<Rectangle> updateRects = new List<Rectangle>();

                Rectangle oldRect = _selectedPointer.BoundRectangle;

                if (oldRect.Right >= LevelRenderer.BITMAP_WIDTH)
                {
                    oldRect.Width = oldRect.Width - (oldRect.Right - LevelRenderer.BITMAP_WIDTH);
                }

                if (oldRect.Bottom >= LevelRenderer.BITMAP_HEIGHT)
                {
                    oldRect.Height = oldRect.Height - (oldRect.Bottom - LevelRenderer.BITMAP_HEIGHT);
                }

                updateRects.Add(oldRect);

                int newX = (int)((originalPointerPoint.X + diffPoint.X) / 16);
                int newY = (int)((originalPointerPoint.Y + diffPoint.Y) / 16);

                if (newX == _selectedPointer.X && newY == _selectedPointer.Y)
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

                _selectedPointer.X = newX;
                _selectedPointer.Y = newY;

                Rectangle updateArea = _selectedPointer.BoundRectangle;

                if (updateArea.Right >= LevelRenderer.BITMAP_WIDTH)
                {
                    updateArea.Width = updateArea.Width - (updateArea.Right - LevelRenderer.BITMAP_WIDTH);
                }

                if (updateArea.Bottom >= LevelRenderer.BITMAP_HEIGHT)
                {
                    updateArea.Height = updateArea.Height - (updateArea.Bottom - LevelRenderer.BITMAP_HEIGHT);
                }

                PointerEditor.Visibility = Visibility.Collapsed;

                updateRects.Add(updateArea);

                SetSelectionRectangle(_selectedPointer.BoundRectangle);
                Update(updateRects);
            }

            int blockX = (int)movePoint.X / 16, blockY = (int)movePoint.Y / 16;
            LevelObject levelObject = _level.ObjectData.Where(o => o.BoundRectangle.Contains((int)movePoint.X, (int)movePoint.Y)).FirstOrDefault();
            if (levelObject == null)
            {
                SpriteDescription.Text = "None";
            }
            else
            {
                SpriteDescription.Text = _selectedObject == null ? levelObject.GameObject.Name : _selectedObject.GameObject.Name;
            }

            PointerXY.Text = "X: " + blockX.ToString("X2") + " Y: " + blockY.ToString("X2");
        }

        private int Snap(double value)
        {
            return (int)(Math.Floor(value / 16) * 16);
        }

        private Point Snap(Point value)
        {
            return new Point(Snap(Math.Min(value.X, LevelRenderer.BITMAP_WIDTH - 1)), Snap(Math.Min(value.Y, LevelRenderer.BITMAP_HEIGHT - 1)));
        }

        private void CutSelection()
        {
            if (_selectionMode == SelectionMode.SelectTiles)
            {
                int[,] _copyBuffer = new int[(int)((SelectionRectangle.Width - 4) / 16), (int)((SelectionRectangle.Height - 4) / 16)];


                int startX = (int)((Canvas.GetLeft(SelectionRectangle) + 2) / 16);
                int endX = (int)((SelectionRectangle.Width - 4) / 16) + startX;
                int startY = (int)((Canvas.GetTop(SelectionRectangle) + 2) / 16);
                int endY = (int)((SelectionRectangle.Height - 4) / 16) + startY;

                TileChange tileChange = new TileChange(startX, startY, endX - startX, endY - startY);

                for (int r = startY, y = 0; r < endY; r++, y++)
                {
                    for (int c = startX, x = 0; c < endX; c++, x++)
                    {
                        tileChange.Data[x, y] = _copyBuffer[x, y] = _levelDataManager.GetData(c, r);
                        _levelDataManager.SetData(c, r, 0x41);
                    }
                }

                _historyService.UndoTiles.Push(tileChange);

                Update(new Rectangle((int)Canvas.GetLeft(SelectionRectangle) + 2,
                                (int)Canvas.GetTop(SelectionRectangle) + 2,
                                (int)(SelectionRectangle.Width - 4),
                                (int)(SelectionRectangle.Height - 4)));

                SetSelectionRectangle(new Rectangle((endX - 1) * 16, (endY - 1) * 16, 16, 16));

                _selectionMode = SelectionMode.SetTiles;
                CursorImage.Opacity = .75;
                _clipBoardService.SetClipboard(new ClipboardItem(_copyBuffer, ClipBoardItemType.TileBuffer));
            }
        }

        private void DeleteSection()
        {
            if (_selectionMode == SelectionMode.SelectTiles)
            {
                int startX = (int)((Canvas.GetLeft(SelectionRectangle) + 2) / 16);
                int endX = (int)((SelectionRectangle.Width - 4) / 16) + startX;
                int startY = (int)((Canvas.GetTop(SelectionRectangle) + 2) / 16);
                int endY = (int)((SelectionRectangle.Height - 4) / 16) + startY;

                TileChange tileChange = new TileChange(startX, startY, endX - startX, endY - startY);

                for (int r = startY, y = 0; r < endY; r++, y++)
                {
                    for (int c = startX, x = 0; c < endX; c++, x++)
                    {
                        tileChange.Data[x, y] = _levelDataManager.GetData(c, r);
                        _levelDataManager.SetData(c, r, 0x41);
                    }
                }

                _historyService.UndoTiles.Push(tileChange);

                Update(new Rectangle((int)Canvas.GetLeft(SelectionRectangle),
                                (int)Canvas.GetTop(SelectionRectangle),
                                (int)(SelectionRectangle.Width - 4),
                                (int)(SelectionRectangle.Height - 4)));

                _selectionMode = SelectionMode.SetTiles;
            }
        }

        private void CopySelection()
        {
            if (_selectionMode == SelectionMode.SelectTiles)
            {
                int[,] _copyBuffer = new int[(int)((SelectionRectangle.Width - 4) / 16), (int)((SelectionRectangle.Height - 4) / 16)];

                int startX = (int)((Canvas.GetLeft(SelectionRectangle) + 2) / 16);
                int endX = (int)((SelectionRectangle.Width - 4) / 16) + startX;
                int startY = (int)((Canvas.GetTop(SelectionRectangle) + 2) / 16);
                int endY = (int)((SelectionRectangle.Height - 4) / 16) + startY;
                for (int r = startY, y = 0; r < endY; r++, y++)
                {
                    for (int c = startX, x = 0; c < endX; c++, x++)
                    {
                        _copyBuffer[x, y] = _levelDataManager.GetData(c, r);
                    }
                }

                _selectionMode = SelectionMode.SetTiles;
                CursorImage.Opacity = .75;
                SetSelectionRectangle(new Rectangle((int)_dragStartPoint.X, (int)_dragStartPoint.Y, 16, 16));
                _clipBoardService.SetClipboard(new ClipboardItem(_copyBuffer, ClipBoardItemType.TileBuffer));
            }
        }

        private void PasteSelection()
        {
            ClipboardItem clipboardItem = _clipBoardService.GetClipboard();

            if (clipboardItem.Type == ClipBoardItemType.TileBuffer)
            {
                int[,] _copyBuffer = (int[,])clipboardItem.Data;
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
                            tileChange.Data[x, y] = _levelDataManager.GetData(c, r);
                            _levelDataManager.SetData(c, r, _copyBuffer[x % bufferWidth, y % bufferHeight]);
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
                            tileChange.Data[x, y] = _levelDataManager.GetData(c, r);

                            _levelDataManager.SetData(c, r, _copyBuffer[x % bufferWidth, y % bufferHeight]);
                        }
                    }

                    Update(new Rectangle((int)Canvas.GetLeft(SelectionRectangle) + 2,
                                    (int)Canvas.GetTop(SelectionRectangle) + 2,
                                    (int)(SelectionRectangle.Width - 4),
                                    (int)(SelectionRectangle.Height - 4)));
                }

                _historyService.UndoTiles.Push(tileChange);

                _selectionMode = SelectionMode.SetTiles;
                CursorImage.Opacity = .75;
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

        private void DeletePointer()
        {
            if (_selectedPointer != null)
            {
                _level.LevelPointers.Remove(_selectedPointer);
                Update(_selectedPointer.BoundRectangle);
                ClearSelectionRectangle();
                PointerEditor.Visibility = Visibility.Collapsed;
            }
        }

        private void DeleteObject()
        {
            if (_selectedObject != null && _selectedObject != _level.ObjectData[0])
            {
                _level.ObjectData.Remove(_selectedObject);
                Update(_selectedObject.VisualRectangle);
                ClearSelectionRectangle();
                _historyService.UndoLevelObjects.Push(new LevelObjectChange(_selectedObject, _selectedObject.X, _selectedObject.Y, _selectedObject.Property, _selectedObject.GameObjectId, LevelObjectChangeType.Deletion));
            }

            UpdateSpriteStatus();
        }

        private void UndoObjects()
        {
            if (_historyService.UndoLevelObjects.Count > 0)
            {
                LevelObjectChange undoObject = _historyService.UndoLevelObjects.Pop();
                _historyService.RedoLevelObjects.Push(ApplyObjectChange(undoObject));
            }

            UpdateSpriteStatus();
        }

        private void RedoObjects()
        {
            if (_historyService.RedoLevelObjects.Count > 0)
            {
                LevelObjectChange redoObject = _historyService.RedoLevelObjects.Pop();
                _historyService.UndoLevelObjects.Push(ApplyObjectChange(redoObject));
            }

            UpdateSpriteStatus();
        }

        private void LevelRenderSource_MouseUp(object sender, MouseButtonEventArgs e)
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
                        reverseTileChange.Data[i, j] = _levelDataManager.GetData(c, r);
                        _levelDataManager.SetData(c, r, tileChange.Data[i, j]);
                    }
                }
            }

            Update(new Rectangle(columnStart * 16, rowStart * 16, (columnEnd - columnStart) * 16, (rowEnd - rowStart) * 16));
            return reverseTileChange;
        }

        private LevelObjectChange ApplyObjectChange(LevelObjectChange objectChange)
        {
            LevelObjectChange newChange = null;
            List<Rectangle> updateRects = new List<Rectangle>() { objectChange.OriginalObject.VisualRectangle };

            if (objectChange.ChangeType == LevelObjectChangeType.Addition)
            {
                newChange = new LevelObjectChange(objectChange.OriginalObject, objectChange.OriginalObject.X, objectChange.OriginalObject.Y, objectChange.OriginalObject.Property, objectChange.OriginalObject.GameObjectId, LevelObjectChangeType.Deletion);
                _level.ObjectData.Remove(objectChange.OriginalObject);
                _selectedObject = null;
                ClearSelectionRectangle();
            }
            else if (objectChange.ChangeType == LevelObjectChangeType.Deletion)
            {
                newChange = new LevelObjectChange(objectChange.OriginalObject, objectChange.OriginalObject.X, objectChange.OriginalObject.Y, objectChange.OriginalObject.Property, objectChange.OriginalObject.GameObjectId, LevelObjectChangeType.Addition);
                _level.ObjectData.Add(objectChange.OriginalObject);
                _selectedObject = objectChange.OriginalObject;
                SetSelectionRectangle(_selectedObject.CalcBoundBox());
                UpdateProperties();
            }
            else if (objectChange.ChangeType == LevelObjectChangeType.Update)
            {
                newChange = new LevelObjectChange(objectChange.OriginalObject, objectChange.OriginalObject.X, objectChange.OriginalObject.Y, objectChange.OriginalObject.Property, objectChange.OriginalObject.GameObjectId, LevelObjectChangeType.Update);
                objectChange.OriginalObject.X = objectChange.X;
                objectChange.OriginalObject.Y = objectChange.Y;
                objectChange.OriginalObject.GameObjectId = objectChange.GameId;
                objectChange.OriginalObject.GameObject = _gameObjectService.GetObjectById(objectChange.GameId);
                objectChange.OriginalObject.Property = objectChange.Property;
                updateRects.Add(objectChange.OriginalObject.CalcVisualBox(true));
                _selectedObject = objectChange.OriginalObject;
                SetSelectionRectangle(_selectedObject.CalcBoundBox());
                UpdateProperties();
            }

            Update(updateRects);
            return newChange;
        }

        private void HandleTileRelease(MouseButtonEventArgs e)
        {
            Point mousePoint = Snap(e.GetPosition(LevelRenderSource));

            if (_drawMode == DrawMode.Default && _isDragging)
            {
                _isDragging = false;

                if (_selectionMode == SelectionMode.SetTiles && (Keyboard.Modifiers & ModifierKeys.Control) == 0)
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
                            tileChange.Data[i, j] = _levelDataManager.GetData(c, r);
                            _levelDataManager.SetData(c, r, TileSelector.SelectedBlockValue);
                        }
                    }

                    _historyService.UndoTiles.Push(tileChange);
                    _historyService.RedoTiles.Clear();

                    Update(new Rectangle(columnStart * 16, rowStart * 16, (columnEnd - columnStart + 1) * 16, (rowEnd - rowStart + 1) * 16));
                    SetSelectionRectangle(new Rectangle((int)mousePoint.X, (int)mousePoint.Y, 16, 16));
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
                    UpdateProperties();
                    if (_historyService.UndoLevelObjects.Peek().IsSame())
                    {
                        _historyService.UndoLevelObjects.Pop();
                    }
                    else
                    {
                        _historyService.RedoLevelObjects.Clear();
                    }
                }

                if (_selectedObject.GameObject.IsStartObject)
                {
                    _level.StartX = _selectedObject.X;
                    _level.StartY = _selectedObject.Y;
                }
            }
        }

        private void HandlePointerRelease(MouseButtonEventArgs e)
        {
            if (_selectedPointer != null && _isDragging)
            {
                PointerEditor.Visibility = Visibility.Visible;

                Rectangle boundRect = _selectedPointer.BoundRectangle;
                double leftEdge = (boundRect.Left - PointerEditor.ActualWidth / 2) + 16;

                if (leftEdge < 0)
                {
                    leftEdge = 0;
                }

                if (leftEdge + PointerEditor.Width >= LevelRenderer.BITMAP_WIDTH)
                {
                    leftEdge = LevelRenderer.BITMAP_WIDTH - PointerEditor.ActualWidth;
                }

                if (boundRect.Bottom <= 200)
                {
                    Canvas.SetTop(PointerEditor, boundRect.Bottom + 4);
                }
                else
                {
                    Canvas.SetTop(PointerEditor, boundRect.Top - 160);
                }

                Canvas.SetLeft(PointerEditor, leftEdge);
            }

            _isDragging = false;
        }

        private void UpdateProperties()
        {
            if (_selectedObject.GameObject.Properties != null && _selectedObject.GameObject.Properties.Count > 0)
            {
                GameObjectProperty.Visibility = Visibility.Visible;
            }
            else
            {
                GameObjectProperty.Visibility = Visibility.Collapsed;
            }

            Rectangle boundRect = _selectedObject.BoundRectangle;

            if (boundRect.Bottom <= 200)
            {
                Canvas.SetTop(GameObjectProperty, boundRect.Bottom + 4);
            }
            else
            {
                Canvas.SetTop(GameObjectProperty, boundRect.Top - 48);
            }

            Canvas.SetLeft(GameObjectProperty, boundRect.Left - 10);

            GameObjectProperty.ItemsSource = _selectedObject.GameObject.Properties;
            GameObjectProperty.SelectedIndex = _selectedObject.Property;
        }


        private void UpdateTextTables()
        {
            List<KeyValuePair<string, string>> _graphicsSetNames = new List<KeyValuePair<string, string>>();
            for (int i = 0; i < 256; i++)
            {
                _graphicsSetNames.Add(new KeyValuePair<string, string>(i.ToString("X"), "0x" + (i * 0x400).ToString("X")));
            }

            Music.ItemsSource = _textService.GetTable("music").OrderBy(kv => kv.Value);
            AnimationType.ItemsSource = _textService.GetTable("animation_type");
            EffectType.ItemsSource = _textService.GetTable("effects");
            EventType.ItemsSource = _textService.GetTable("event_type");
            PaletteIndex.ItemsSource = _palettesService.GetPalettes();
            GraphicsSet.ItemsSource = _graphicsSetNames;
            PaletteEffect.ItemsSource = _textService.GetTable("palette_effect");
            Screens.ItemsSource = new int[15] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };
            ScrollType.ItemsSource = _textService.GetTable("scroll_types");
            TileSet.ItemsSource = _textService.GetTable("tile_sets");

            Music.SelectedValue = _level.MusicValue.ToString("X");
            AnimationType.SelectedValue = _level.AnimationType.ToString();
            EffectType.SelectedValue = _level.Effects.ToString("X2");
            EventType.SelectedValue = _level.EventType.ToString("X");
            PaletteIndex.SelectedValue = _level.PaletteId;
            GraphicsSet.SelectedValue = _level.StaticTileTableIndex.ToString("X");
            PaletteEffect.SelectedValue = _level.PaletteEffect.ToString();
            Screens.SelectedIndex = _level.ScreenLength - 1;
            ScrollType.SelectedValue = _level.ScrollType.ToString("X");
            TileSet.SelectedValue = _level.TileSetIndex.ToString("X");
        }

        private void AnimationType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _level.AnimationType = int.Parse(AnimationType.SelectedValue.ToString());
            _graphicsManager.SetBottomTable(_graphicsService.GetTileSection(_level.AnimationTileTableIndex));
            TileSelector.Update();
            Update();
        }

        private void GraphicsSet_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _level.StaticTileTableIndex = int.Parse(GraphicsSet.SelectedValue.ToString(), System.Globalization.NumberStyles.HexNumber);
            _graphicsManager.SetTopTable(_graphicsService.GetTileSection(_level.StaticTileTableIndex));
            TileSelector.Update();
            Update();
        }

        private void EventType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _level.EventType = int.Parse(EventType.SelectedValue.ToString(), System.Globalization.NumberStyles.HexNumber);
        }

        private void Music_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _level.MusicValue = int.Parse(Music.SelectedValue.ToString(), System.Globalization.NumberStyles.HexNumber);
        }

        private void PaletteIndex_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PaletteIndex.SelectedItem != null)
            {
                _level.PaletteId = ((Palette)PaletteIndex.SelectedItem).Id;

                Palette palette = _palettesService.GetPalette(_level.PaletteId);
                _levelRenderer.Update(_palettesService.GetPalette(_level.PaletteId));
                TileSelector.Update(palette: palette);
                ObjectSelector.Update(palette);
                Update();
            }
        }

        private void PaletteEffect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _level.PaletteEffect = int.Parse(PaletteEffect.SelectedValue.ToString());
        }

        private void Screens_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _level.ScreenLength = int.Parse(Screens.SelectedValue.ToString());
            LevelClip.Width = _level.ScreenLength * 16 * 16;
        }

        private void ScrollType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _level.ScrollType = int.Parse(ScrollType.SelectedValue.ToString());
        }

        private void TileSet_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _level.TileSetIndex = int.Parse(TileSet.SelectedValue.ToString(), System.Globalization.NumberStyles.HexNumber);

            _tileSet = _tileService.GetTileSet(_level.TileSetIndex);
            _levelRenderer.Update(tileSet: _tileSet);
            TileSelector.Update(_tileSet);
            Update();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveLevel();
        }

        private void SaveLevel()
        {
            LevelObject startPointObject = _level.ObjectData[0];
            _level.ObjectData.RemoveAt(0);

            int mostCommonTile = -1;
            int[] tileCount = new int[256];
            foreach (var data in _level.TileData)
            {
                if (data != 0xFF)
                {
                    tileCount[data]++;
                }
            }

            int highestTileCount = -1;
            for (int i = 0; i < 256; i++)
            {
                if (tileCount[i] > highestTileCount)
                {
                    mostCommonTile = i;
                    highestTileCount = tileCount[i];
                }
            }

            _level.MostCommonTile = mostCommonTile;

            _level.CompressedData = _compressionService.CompressLevel(_level);
            _levelInfo.Size = _level.CompressedData.Length;

            _levelService.SaveLevel(_level);

            _level.ObjectData.Insert(0, startPointObject);

            int thumbnailX = startPointObject.X * 16 - 120;
            int thumbnailY = startPointObject.Y * 16 - 120;

            if (thumbnailX < 0)
            {
                thumbnailX = 0;
            }

            if (thumbnailY < 0)
            {
                thumbnailY = 0;
            }

            if (thumbnailX + 256 > 256 * _level.ScreenLength)
            {
                thumbnailX = 256 * _level.ScreenLength - 256;
            }

            if (thumbnailY + 256 > LevelRenderer.BITMAP_HEIGHT)
            {
                thumbnailY = LevelRenderer.BITMAP_HEIGHT - 256;
            }

            Rectangle thumbnailReact = new Rectangle(thumbnailX, thumbnailY, 256, 256);

            using (MemoryStream ms = new MemoryStream(_levelRenderer.GetRectangle(thumbnailReact)))
            {
                _levelService.GenerateMetaData(_levelInfo, ms);
            }

            AlertWindow.Alert(_level.Name + " has been saved!");
        }

        private void EffectType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _level.Effects = int.Parse(EffectType.SelectedValue.ToString(), System.Globalization.NumberStyles.HexNumber);
        }

        private void GameObjectProperty_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_selectedObject != null && GameObjectProperty.SelectedIndex > -1)
            {
                LevelObjectChange objectChange = new LevelObjectChange(_selectedObject, _selectedObject.X, _selectedObject.Y, _selectedObject.Property, _selectedObject.GameObjectId, LevelObjectChangeType.Update);

                _selectedObject.Property = GameObjectProperty.SelectedIndex;

                if (!objectChange.IsSame())
                {
                    _historyService.UndoLevelObjects.Push(objectChange);
                    _historyService.RedoLevelObjects.Clear();
                }

                Update(Rectangle.Union(_selectedObject.VisualRectangle, _selectedObject.CalcVisualBox(true)));
                SetSelectionRectangle(_selectedObject.CalcBoundBox());
            }
        }

        private void ObjectSelector_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SelectedEditMode.SelectedIndex = 1;
        }

        private void TileSelector_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SelectedEditMode.SelectedIndex = 0;

            if (e.ClickCount >= 2)
            {
                GlobalPanels.EditTileBlock(_level.Id, TileSelector.SelectedBlockValue);
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
                    _selectedObject = null;

                    if (GameObjectProperty != null)
                    {
                        GameObjectProperty.Visibility = Visibility.Collapsed;
                    }

                    if (TileStatus != null)
                    {
                        TileStatus.Visibility = TerrainStatus.Visibility = InteractionStatus.Visibility = Visibility.Visible;
                        SpriteStatus.Visibility = Visibility.Collapsed;
                    }

                    CursorImage.Opacity = .75;
                    SelectedDrawMode.IsEnabled = true;
                    SelectedDrawMode.Opacity = 1;
                    PointerEditor.Visibility = Visibility.Hidden;
                    GameObjectProperty.Visibility = Visibility.Hidden;
                    break;

                case 1:
                    _editMode = EditMode.Objects;

                    if (TileStatus != null)
                    {
                        TileStatus.Visibility = TerrainStatus.Visibility = InteractionStatus.Visibility = Visibility.Collapsed;
                        SpriteStatus.Visibility = Visibility.Visible;
                    }

                    CursorImage.Opacity = 0;
                    SelectedDrawMode.IsEnabled = false;
                    SelectedDrawMode.Opacity = .5;
                    PointerEditor.Visibility = Visibility.Hidden;
                    break;

                case 2:
                    _editMode = EditMode.Pointers;

                    if (GameObjectProperty != null)
                    {
                        GameObjectProperty.Visibility = Visibility.Collapsed;
                    }
                    _selectedObject = null;
                    CursorImage.Opacity = 0;
                    SelectedDrawMode.IsEnabled = false;
                    SelectedDrawMode.Opacity = .5;
                    GameObjectProperty.Visibility = Visibility.Hidden;
                    break;

                case 3:
                    _editMode = EditMode.Tips;
                    if (GameObjectProperty != null)
                    {
                        GameObjectProperty.Visibility = Visibility.Hidden;
                    }

                    _selectedObject = null;
                    CursorImage.Opacity = 0;
                    SelectedDrawMode.IsEnabled = false;
                    SelectedDrawMode.Opacity = .5;
                    GameObjectProperty.Visibility = Visibility.Hidden;
                    break;
            }
        }

        private void ShowPSwitch_Checked(object sender, RoutedEventArgs e)
        {
            Update();
        }

        private void ShowTerrain_Click(object sender, RoutedEventArgs e)
        {
            Update();
            TileSelector.Update(withInteractionOverlay: ShowInteraction.IsChecked.Value, withTerrainOverlay: ShowTerrain.IsChecked.Value);
        }

        private void ShowInteraction_Click(object sender, RoutedEventArgs e)
        {
            Update();
            TileSelector.Update(withInteractionOverlay: ShowInteraction.IsChecked.Value, withTerrainOverlay: ShowTerrain.IsChecked.Value);
        }

        private void ShowStrategy_Click(object sender, RoutedEventArgs e)
        {
            Update();
        }

        private void ShowPSwitch_Click(object sender, RoutedEventArgs e)
        {
            if (_editMode != EditMode.PSwitchView)
            {
                _editMode = EditMode.PSwitchView;
                _levelDataManager.PSwitchActive = true;
                ShowPSwitch.IsChecked = true;
            }
            else
            {
                _editMode = EditMode.Tiles;
                _levelDataManager.PSwitchActive = false;
                ShowPSwitch.IsChecked = false;
            }

            Update();
        }

        private int? _highlightedTile = null;
        private void ShowHighlight_Click(object sender, RoutedEventArgs e)
        {
            if (_highlightedTile == null)
            {
                _highlightedTile = TileSelector.SelectedBlockValue;
                ShowHighlight.IsChecked = true;
            }
            else
            {
                _highlightedTile = null;
                ShowHighlight.IsChecked = false;
            }

            Update();
        }

        private void ShowScreenLines_Click(object sender, RoutedEventArgs e)
        {
            _levelRenderer.ScreenBorders = ShowScreenLines.IsChecked.Value;
            Update();
        }

        private void ShowGrid_Click(object sender, RoutedEventArgs e)
        {
            _levelRenderer.RenderGrid = ShowGrid.IsChecked.Value;
            Update();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            GlobalPanels.OpenPaletteEditor((Palette)PaletteIndex.SelectedItem);
        }

        private void TileSelector_TileBlockSelected(TileBlock tileBlock, int tileBlockValue)
        {
            Int32Rect updateRect = new Int32Rect(0, 0, 16, 16);
            _cursorBitmap.Lock();
            _cursorBitmap.WritePixels(updateRect, TileSelector.GetTileBlockImage(), 16 * 4, 0, 0);
            _cursorBitmap.AddDirtyRect(updateRect);
            _cursorBitmap.Unlock();

            if (ShowHighlight.IsChecked == true)
            {
                _highlightedTile = TileSelector.SelectedBlockValue;
                Update();
            }
        }

        public void HandleKeyDown(KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                switch (e.Key)
                {
                    case Key.S:
                        SaveLevel();
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
                    if (_editMode == EditMode.Objects)
                    {
                        DeleteObject();
                    }
                    else if (_editMode == EditMode.Pointers)
                    {
                        DeletePointer();
                    }
                    else
                    {
                        DeleteSection();
                    }
                    break;

                case Key.Escape:
                    if (_editMode == EditMode.Tiles)
                    {
                        _isDragging = false;
                        _selectionMode = SelectionMode.SetTiles;
                        CursorImage.Opacity = .75;
                        SetSelectionRectangle(new Rectangle((int)_dragStartPoint.X, (int)_dragStartPoint.Y, 16, 16));
                    }
                    break;
            }
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            var encoder = new PngBitmapEncoder();
            RenderTargetBitmap bitmap = new RenderTargetBitmap((int)_level.ScreenLength * 16 * 16, (int)LevelRenderSource.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(LevelRenderSource);

            BitmapFrame frame = BitmapFrame.Create(bitmap);
            encoder.Frames.Add(frame);

            //_levelService.ExportLevelToPng(encoder, _level);
        }

        private Level.LevelQuest _currentQuest = Level.LevelQuest.First;
        private void ToggleQuest_Click(object sender, RoutedEventArgs e)
        {
            LevelObject startObject = _level.ObjectData.Where(o => o.GameObject.IsStartObject).First();
            _level.ObjectData.RemoveAt(0);
            _currentQuest = _currentQuest == Level.LevelQuest.First ? Level.LevelQuest.Second : Level.LevelQuest.First;
            _level.SwitchQuest(_currentQuest);
            _level.ObjectData.Insert(0, startObject);
            Update();
        }

        private void NoStars_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _level.NoStars = NoStars.SelectedIndex == 1;
        }

        private void ExtendedSpace_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _levelInfo.SaveToExtendedSpace = ExtendedSpace.SelectedIndex == 1;
        }
    }

    public enum DrawMode
    {
        Default,
        Fill,
        Replace
    }

    public enum EditMode
    {
        PSwitchView,
        Tiles,
        Objects,
        Pointers,
        Tips
    }

    public enum SelectionMode
    {
        SetTiles,
        SelectTiles
    }
}