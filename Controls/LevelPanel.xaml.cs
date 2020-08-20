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
    /// Interaction logic for LevelPanel.xaml
    /// </summary>
    public partial class LevelPanel : UserControl, IDetachEvents
    {
        private Level _level;
        private LevelInfo _levelInfo;
        private LevelService _levelService;
        private PalettesService _palettesService;
        private LevelRenderer _levelRenderer;
        private TileSet _tileSet;
        private TextService _textService;
        private GraphicsService _graphicsService;
        private TileService _tileService;
        private GameObjectService _gameObjectService;
        private GraphicsAccessor _graphicsAccessor;
        private LevelDataAccessor _levelDataAccessor;
        private WriteableBitmap _bitmap;
        private HistoryService _historyService;
        private List<TileTerrain> _terrain;

        public delegate void LevelEditorExitSelectedHandled(int x, int y);
        public event LevelEditorExitSelectedHandled LevelEditorExitSelected;
        private bool _initializing = true;

        public LevelPanel(GraphicsService graphicsService, PalettesService palettesService, TextService textService, TileService tileService, GameObjectService gameObjectService, LevelService levelService, LevelInfo levelInfo)
        {
            InitializeComponent();

            _levelInfo = levelInfo;
            _textService = textService;
            _graphicsService = graphicsService;
            _gameObjectService = gameObjectService;
            _tileService = tileService;
            _palettesService = palettesService;
            _levelService = levelService;
            _historyService = new HistoryService();
            _terrain = _tileService.GetTerrain();
            _level = _levelService.LoadLevel(_levelInfo);

            _level.ObjectData.ForEach(o => o.GameObject = gameObjectService.GetObject(o.GameObjectId));
            _level.ObjectData.Insert(0, new LevelObject()
            {
                GameObject = _gameObjectService.GetStartPointObject(),
                X = _level.StartX,
                Y = _level.StartY
            });

            Tile[] staticSet = _graphicsService.GetTileSection(_level.StaticTileTableIndex);
            Tile[] animationSet = _graphicsService.GetTileSection(_level.AnimationTileTableIndex);
            _graphicsAccessor = new GraphicsAccessor(staticSet, animationSet, _graphicsService.GetGlobalTiles(), _graphicsService.GetExtraTiles());
            _levelDataAccessor = new LevelDataAccessor(_level);

            _bitmap = new WriteableBitmap(LevelRenderer.BITMAP_WIDTH, LevelRenderer.BITMAP_HEIGHT, 96, 96, PixelFormats.Bgra32, null);
            _levelRenderer = new LevelRenderer(_graphicsAccessor, _levelDataAccessor, _palettesService, _gameObjectService, _tileService.GetTerrain());
            _levelRenderer.Initializing();

            _tileSet = _tileService.GetTileSet(_level.TileSetIndex);
            _levelRenderer.SetTileSet(_tileSet);

            Palette palette = _palettesService.GetPalette(_level.PaletteId);
            _levelRenderer.Update(palette);

            LevelRenderSource.Source = _bitmap;
            LevelRenderSource.Width = _bitmap.PixelWidth;
            LevelRenderSource.Height = _bitmap.PixelHeight;
            CanvasContainer.Width = RenderContainer.Width = _level.ScreenLength * 16 * 16;
            _level.ObjectData.ForEach(o =>
            {
                o.CalcBoundBox();
                o.CalcVisualBox(true);
            });

            SelectedEditMode.SelectedIndex = SelectedDrawMode.SelectedIndex = 0;

            TileSelector.Initialize(_graphicsAccessor, _tileService, _tileSet, palette);
            ObjectSelector.Initialize(_gameObjectService, _palettesService, _graphicsAccessor, palette);
            PointerEditor.Initialize(_levelService, _levelInfo);

            UpdateTextTables();            

            gameObjectService.GameObjectUpdated += GameObjectService_GameObjectsUpdated;
            ObjectSelector.GameObjectDoubleClicked += ObjectSelector_GameObjectDoubleClicked;
            _graphicsService.GraphicsUpdated += _graphicsService_GraphicsUpdated;
            _graphicsService.ExtraGraphicsUpdated += _graphicsService_GraphicsUpdated;
            _palettesService.PalettesChanged += _palettesService_PalettesChanged;
            _tileService.TileSetUpdated += _tileService_TileSetUpdated;

            _initializing = false;
            _levelRenderer.Ready();
            Update();
        }

        private void _tileService_TileSetUpdated(int index, TileSet tileSet)
        {
            Update();
            TileSelector.Update(_tileSet);
        }

        public void DetachEvents()
        {
            _gameObjectService.GameObjectUpdated -= GameObjectService_GameObjectsUpdated;
            ObjectSelector.GameObjectDoubleClicked -= ObjectSelector_GameObjectDoubleClicked;
            _graphicsService.GraphicsUpdated -= _graphicsService_GraphicsUpdated;
            _graphicsService.ExtraGraphicsUpdated -= _graphicsService_GraphicsUpdated;
            _palettesService.PalettesChanged -= _palettesService_PalettesChanged;
            ObjectSelector.DetachEvents();
        }

        private void _palettesService_PalettesChanged()
        {
            PaletteIndex.ItemsSource = _palettesService.GetPalettes();
            if (PaletteIndex.SelectedItem == null)
            {
                PaletteIndex.SelectedIndex = 0;
            }
            else
            {
                _levelRenderer.Update((Palette)PaletteIndex.SelectedItem);
                TileSelector.Update(palette: (Palette)PaletteIndex.SelectedItem);
                ObjectSelector.Update((Palette)PaletteIndex.SelectedItem);
                Update();
            }
        }
     
        private void _graphicsService_GraphicsUpdated()
        {
            _graphicsAccessor.SetStaticTable(_graphicsService.GetTileSection(_level.StaticTileTableIndex));
            _graphicsAccessor.SetAnimatedTable(_graphicsService.GetTileSection(_level.AnimationTileTableIndex));
            _graphicsAccessor.SetGlobalTiles(_graphicsService.GetGlobalTiles(), _graphicsService.GetExtraTiles());
            TileSelector.Update();
            Update();
        }

        private void ObjectSelector_GameObjectDoubleClicked(GameObject gameObject)
        {
            GlobalPanels.EditGameObject(gameObject, (Palette)PaletteIndex.SelectedItem);
        }

        private void GameObjectService_GameObjectsUpdated(GameObject gameObject)
        {
            List<LevelObject> affectedObjects = _level.ObjectData.Where(l => l.GameObjectId == gameObject.GameId).ToList();
            List<Rect> affectedRects = new List<Rect>();
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

        private void Update(Rect updateRect)
        {
            Update(new List<Rect>() { updateRect });
        }

        private void Update(int x = 0, int y = 0, int width = LevelRenderer.BITMAP_WIDTH, int height = LevelRenderer.BITMAP_HEIGHT)
        {
            Update(new List<Rect>() { new Rect(x, y, width, height) });
        }

        private void Update(List<Rect> updateAreas)
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

                if (safeRect.X + safeRect.Width > LevelRenderer.BITMAP_WIDTH)
                {
                    safeRect.Width -= LevelRenderer.BITMAP_WIDTH - (safeRect.X + safeRect.Width);
                }

                if (safeRect.Y + safeRect.Height > LevelRenderer.BITMAP_HEIGHT)
                {
                    safeRect.Height -= LevelRenderer.BITMAP_HEIGHT - (safeRect.Y + safeRect.Height);
                }

                Int32Rect sourceArea = new Int32Rect(0, 0, Math.Max(0, Math.Min(safeRect.Width, LevelRenderer.BITMAP_WIDTH)), Math.Max(0, Math.Min(safeRect.Height, LevelRenderer.BITMAP_HEIGHT)));

                _levelRenderer.Update(safeRect, true, ShowTerrain.IsChecked.Value, ShowInteraction.IsChecked.Value);
                _bitmap.WritePixels(sourceArea, _levelRenderer.GetRectangle(safeRect), safeRect.Width * 4, safeRect.X, safeRect.Y);
                _bitmap.AddDirtyRect(safeRect);
            }

            _bitmap.Unlock();

            Console.WriteLine("Draw time " + (DateTime.Now - speedTest).TotalMilliseconds + " milliseconds.");
        }

        private void ClearSelectionRectangle()
        {
            GameObjectProperty.Visibility = SelectionRectangle.Visibility = Visibility.Collapsed;
        }

        private void SetSelectionRectangle(Rect rect)
        {
            Canvas.SetLeft(SelectionRectangle, rect.X);
            Canvas.SetTop(SelectionRectangle, rect.Y);

            SelectionRectangle.Width = rect.Width;
            SelectionRectangle.Height = rect.Height;
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
            Point clickPoint = Snap(e.GetPosition(LevelRenderSource));


            LevelRenderSource.Focusable = true;
            LevelRenderSource.Focus();

            if (LevelEditorExitSelected != null)
            {
                LevelEditorExitSelected((int)clickPoint.X / 16, (int)clickPoint.Y / 16);
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (_level.LevelPointers.Where(o => o.BoundRectangle.Contains(clickPoint.X, clickPoint.Y)).FirstOrDefault() != null)
                {
                    SelectedEditMode.SelectedIndex = 2;
                }
                else if (_level.ObjectData.Where(o => o.BoundRectangle.Contains(clickPoint.X, clickPoint.Y)).FirstOrDefault() != null)
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
                    HandleSpriteClick(e);
                    break;

                case EditMode.Pointers:
                    HandlePointerClick(e);
                    break;
            }

        }

        private void HandleTileClick(MouseButtonEventArgs e)
        {
            Point clickPoint = Snap(e.GetPosition(LevelRenderSource));

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                int x = (int)(clickPoint.X / 16), y = (int)(clickPoint.Y / 16);
                int tileValue = _levelDataAccessor.GetData(x, y);

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
                        _levelDataAccessor.ReplaceValue(tileValue, TileSelector.SelectedBlockValue);
                        Update();
                    }
                }
                else if (_drawMode == DrawMode.Fill)
                {
                    Stack<Point> stack = new Stack<Point>();
                    stack.Push(new Point(x, y));

                    int checkValue = _levelDataAccessor.GetData(x, y);
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

                        if (checkValue == _levelDataAccessor.GetData(i, j))
                        {
                            _levelDataAccessor.SetData(i, j, TileSelector.SelectedBlockValue);

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
                            tileChange.Data[i, j] = _levelDataAccessor.GetData(col, row) == TileSelector.SelectedBlockValue ? checkValue : -1;
                        }
                    }

                    _historyService.UndoTiles.Push(tileChange);

                    Update(new Rect(lowestX * 16, lowestY * 16, (highestX - lowestX + 1) * 16, (highestY - lowestY + 1) * 16));
                }
            }
            else if (e.RightButton == MouseButtonState.Pressed)
            {
                _selectionMode = SelectionMode.SelectTiles;

                int x = (int)(clickPoint.X / 16), y = (int)(clickPoint.Y / 16);

                _dragStartPoint = clickPoint;
                _isDragging = true;
                originalTilePoint = clickPoint;

                SetSelectionRectangle(new Rect(x * 16, y * 16, 16, 16));
            }
        }

        private void HandleSpriteClick(MouseButtonEventArgs e)
        {
            Point tilePoint = e.GetPosition(LevelRenderSource);
            List<Rect> updatedRects = new List<Rect>();


            if (e.LeftButton == MouseButtonState.Pressed || e.RightButton == MouseButtonState.Pressed)
            {
                if (e.RightButton == MouseButtonState.Pressed)
                {
                    _selectedObject = _level.ObjectData.Where(o => o.BoundRectangle.Contains(tilePoint.X, tilePoint.Y)).FirstOrDefault();
                    if (_selectedObject != _level.ObjectData[0])
                    {
                        if (_selectedObject != null)
                        {
                            _historyService.UndoLevelObjects.Push(new LevelObjectChange(_selectedObject, _selectedObject.X, _selectedObject.Y, _selectedObject.Property, _selectedObject.GameObjectId, LevelObjectChangeType.Update));
                            ObjectSelector.SelectedObject = _selectedObject.GameObject;
                        }
                        else
                        {
                            if (ObjectSelector.SelectedObject != null)
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
                    _selectedObject = _level.ObjectData.Where(o => o.BoundRectangle.Contains(tilePoint.X, tilePoint.Y)).FirstOrDefault();
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

                    Rect boundRect = _selectedObject.BoundRectangle;
                    Canvas.SetTop(GameObjectProperty, boundRect.Bottom + 4);
                    Canvas.SetLeft(GameObjectProperty, boundRect.Left);

                    GameObjectProperty.ItemsSource = _selectedObject.GameObject.Properties;
                    GameObjectProperty.SelectedIndex = _selectedObject.Property;
                }
                else
                {
                    ClearSelectionRectangle();
                }

                Update(updatedRects);
            }
        }

        private void HandlePointerClick(MouseButtonEventArgs e)
        {
            Point tilePoint = e.GetPosition(LevelRenderSource);
            List<Rect> updatedRects = new List<Rect>();

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
                    _selectedPointer = _level.LevelPointers.Where(o => o.BoundRectangle.Contains(tilePoint.X, tilePoint.Y)).FirstOrDefault();
                }

                if (_selectedPointer != null)
                {
                    _dragStartPoint = tilePoint;
                    SetSelectionRectangle(_selectedPointer.BoundRectangle);
                    originalPointerPoint = new Point(_selectedPointer.X * 16, _selectedPointer.Y * 16);
                    _isDragging = true;


                    PointerEditor.Visibility = Visibility;

                    Rect boundRect = _selectedPointer.BoundRectangle;
                    double leftEdge = boundRect.Left - PointerEditor.Width / 2;

                    if (leftEdge < 0)
                    {
                        leftEdge = 0;
                    }

                    if (leftEdge + PointerEditor.Width >= LevelRenderer.BITMAP_WIDTH)
                    {
                        leftEdge = LevelRenderer.BITMAP_WIDTH - PointerEditor.Width;
                    }

                    Canvas.SetTop(PointerEditor, boundRect.Bottom + 4);
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

            switch (_editMode)
            {
                case EditMode.Tiles:
                    HandleTileMove(e);
                    break;

                case EditMode.Objects:
                    HandleSpriteMove(e);
                    break;

                case EditMode.Pointers:
                    HandlePointerMove(e);
                    break;
            }
        }

        private SelectionMode _selectionMode;
        private void HandleTileMove(MouseEventArgs e)
        {
            Point tilePoint = Snap(e.GetPosition(LevelRenderSource));

            if (_isDragging && ((_selectionMode == SelectionMode.SetTiles && _drawMode == DrawMode.Default) ||
                               _selectionMode == SelectionMode.SelectTiles))
            {
                int x = (int)Math.Min(tilePoint.X, _dragStartPoint.X);
                int y = (int)Math.Min(tilePoint.Y, _dragStartPoint.Y);
                int width = (int)(Math.Max(tilePoint.X, _dragStartPoint.X)) - x;
                int height = (int)(Math.Max(tilePoint.Y, _dragStartPoint.Y)) - y;

                SetSelectionRectangle(new Rect(x, y, width + 16, height + 16));
            }
            else if (_selectionMode != SelectionMode.SelectTiles)
            {
                SetSelectionRectangle(new Rect(tilePoint.X, tilePoint.Y, 16, 16));
            }

            int blockX = (int)tilePoint.X / 16, blockY = (int)tilePoint.Y / 16;
            int tileValue = _levelDataAccessor.GetData(blockX, blockY);
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
                InteractionDescription.Text = tileTerrain.Name;
                TerrainDescription.Text = tileTerrain.Interactions.Where(t => t.Value == (tileBlock.Property & TileInteraction.Mask)).FirstOrDefault().Name;
                PointerXY.Text = "X: " + blockX.ToString("X2") + " Y: " + blockY.ToString("X2");
                TileValue.Text = tileValue.ToString("X");
            }
        }

        private Point originalTilePoint;
        private void HandleSpriteMove(MouseEventArgs e)
        {
            Point movePoint = Snap(e.GetPosition(LevelRenderSource));

            if (_selectedObject != null && _isDragging)
            {
                Point diffPoint = Snap(new Point(movePoint.X - originalTilePoint.X, movePoint.Y - originalTilePoint.Y));

                List<Rect> updateRects = new List<Rect>();

                Rect oldRect = _selectedObject.VisualRectangle;

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

                Rect updateArea = _selectedObject.VisualRectangle;

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
            LevelObject levelObject = _level.ObjectData.Where(o => o.BoundRectangle.Contains(movePoint.X, movePoint.Y)).FirstOrDefault();
            if (levelObject == null)
            {
                SpriteDescription.Text = "None";
            }
            else
            {
                SpriteDescription.Text = levelObject.GameObject.Name;
            }

            PointerXY.Text = "X: " + blockX.ToString("X2") + " Y: " + blockY.ToString("X2");
        }

        private Point originalPointerPoint;

        private void HandlePointerMove(MouseEventArgs e)
        {
            Point movePoint = Snap(e.GetPosition(LevelRenderSource));

            if (_selectedPointer != null && _isDragging)
            {
                Point diffPoint = Snap(new Point(movePoint.X - originalPointerPoint.X, movePoint.Y - originalPointerPoint.Y));

                List<Rect> updateRects = new List<Rect>();

                Rect oldRect = _selectedPointer.BoundRectangle;

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

                Rect updateArea = _selectedPointer.BoundRectangle;

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
            LevelObject levelObject = _level.ObjectData.Where(o => o.BoundRectangle.Contains(movePoint.X, movePoint.Y)).FirstOrDefault();
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
                        _copyBuffer[x, y] = _levelDataAccessor.GetData(c, r);
                        _levelDataAccessor.SetData(c, r, 0x41);
                    }
                }

                Update(new Rect(Canvas.GetLeft(SelectionRectangle),
                                Canvas.GetTop(SelectionRectangle),
                                SelectionRectangle.Width,
                                SelectionRectangle.Height));

                SetSelectionRectangle(new Rect((endX - 1) * 16, (endY - 1) * 16, 16, 16));

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
                        _copyBuffer[x, y] = _levelDataAccessor.GetData(c, r);
                    }
                }

                SetSelectionRectangle(new Rect((endX - 1) * 16, (endY - 1) * 16, 16, 16));

                _selectionMode = SelectionMode.SetTiles;
            }
        }

        private void PasteSelection()
        {
            if (_copyBuffer != null)
            {
                int startX = (int)(Canvas.GetLeft(SelectionRectangle) / 16);
                int endX = (int)(SelectionRectangle.Width / 16) + startX;
                int startY = (int)(Canvas.GetTop(SelectionRectangle) / 16);
                int endY = (int)(SelectionRectangle.Height / 16) + startY;
                int bufferWidth = _copyBuffer.GetLength(0);
                int bufferHeight = _copyBuffer.GetLength(1);

                if (startX + bufferWidth > Level.BLOCK_WIDTH)
                {
                    bufferWidth = Level.BLOCK_WIDTH - startX;
                }

                if (startY + bufferHeight > Level.BLOCK_HEIGHT)
                {
                    bufferHeight = Level.BLOCK_HEIGHT - startY;
                }

                if (SelectionRectangle.Width == 16 &&
                    SelectionRectangle.Height == 16)
                {
                    for (int r = startY, y = 0; y < bufferHeight; r++, y++)
                    {
                        for (int c = startX, x = 0; x < bufferWidth; c++, x++)
                        {
                            _levelDataAccessor.SetData(c, r, _copyBuffer[x % bufferWidth, y % bufferHeight]);
                        }
                    }

                    Update(new Rect(startX * 16, startY * 16, bufferWidth * 16, bufferHeight * 16));
                }
                else
                {
                    for (int r = startY, y = 0; r < endY; r++, y++)
                    {
                        for (int c = startX, x = 0; c < endX; c++, x++)
                        {
                            _levelDataAccessor.SetData(c, r, _copyBuffer[x % bufferWidth, y % bufferHeight]);
                        }
                    }

                    Update(new Rect(Canvas.GetLeft(SelectionRectangle),
                                    Canvas.GetTop(SelectionRectangle),
                                    SelectionRectangle.Width,
                                    SelectionRectangle.Height));
                }

                SetSelectionRectangle(new Rect((endX - 1) * 16, (endY - 1) * 16, 16, 16));

                _selectionMode = SelectionMode.SetTiles;
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
        }

        private void UndoObjects()
        {
            if (_historyService.UndoLevelObjects.Count > 0)
            {
                LevelObjectChange undoObject = _historyService.UndoLevelObjects.Pop();
                _historyService.RedoLevelObjects.Push(ApplyObjectChange(undoObject));
            }
        }

        private void RedoObjects()
        {
            if (_historyService.RedoLevelObjects.Count > 0)
            {
                LevelObjectChange redoObject = _historyService.RedoLevelObjects.Pop();
                _historyService.UndoLevelObjects.Push(ApplyObjectChange(redoObject));
            }
        }

        private void LevelRenderSource_MouseUp(object sender, MouseButtonEventArgs e)
        {
            switch (_editMode)
            {
                case EditMode.Tiles:
                    HandleTileRelease(e);
                    break;

                case EditMode.Objects:
                    HandleSpriteRelease(e);
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
                        reverseTileChange.Data[i, j] = _levelDataAccessor.GetData(c, r);
                        _levelDataAccessor.SetData(c, r, tileChange.Data[i, j]);
                    }
                }
            }

            Update(new Rect(columnStart * 16, rowStart * 16, (columnEnd - columnStart) * 16, (rowEnd - rowStart) * 16));
            return reverseTileChange;
        }

        private LevelObjectChange ApplyObjectChange(LevelObjectChange objectChange)
        {
            LevelObjectChange newChange = null;
            List<Rect> updateRects = new List<Rect>() { objectChange.OriginalObject.VisualRectangle };

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
                objectChange.OriginalObject.GameObject = _gameObjectService.GetObject(objectChange.GameId);
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
                            tileChange.Data[i, j] = _levelDataAccessor.GetData(c, r);
                            _levelDataAccessor.SetData(c, r, TileSelector.SelectedBlockValue);
                        }
                    }

                    _historyService.UndoTiles.Push(tileChange);
                    _historyService.RedoTiles.Clear();

                    Update(new Rect(columnStart * 16, rowStart * 16, (columnEnd - columnStart + 1) * 16, (rowEnd - rowStart + 1) * 16));
                    SetSelectionRectangle(new Rect(mousePoint.X, mousePoint.Y, 16, 16));
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

        private void HandleSpriteRelease(MouseButtonEventArgs e)
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
            }
        }

        private void HandlePointerRelease(MouseButtonEventArgs e)
        {
            if (_selectedPointer != null && _isDragging)
            {
                Rect boundRect = _selectedPointer.BoundRectangle;
                double leftEdge = boundRect.Left - PointerEditor.Width / 2;

                if (leftEdge < 0)
                {
                    leftEdge = 0;
                }

                if (leftEdge + PointerEditor.Width >= LevelRenderer.BITMAP_WIDTH)
                {
                    leftEdge = LevelRenderer.BITMAP_WIDTH - PointerEditor.Width;
                }

                Canvas.SetTop(PointerEditor, boundRect.Bottom + 4);
                Canvas.SetLeft(PointerEditor, leftEdge);

                PointerEditor.Visibility = Visibility.Visible;
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

            Rect boundRect = _selectedObject.BoundRectangle;
            Canvas.SetTop(GameObjectProperty, boundRect.Bottom + 4);
            Canvas.SetLeft(GameObjectProperty, boundRect.Left);

            GameObjectProperty.ItemsSource = _selectedObject.GameObject.Properties;
            GameObjectProperty.SelectedIndex = _selectedObject.Property;
        }

        //private void EditTiles_Checked(object sender, RoutedEventArgs e)
        //{
        //    EditMode = EditMode.Tiles;
        //    if (SelectionRectangle != null)
        //    {
        //        SelectionRectangle.Visibility = Visibility.Collapsed;
        //    }

        //    selectedObject = null;
        //    if (GameObjectProperty != null)
        //    {
        //        GameObjectProperty.Visibility = Visibility.Collapsed;
        //    }

        //    if (TileStatus != null)
        //    {
        //        TileStatus.Visibility = Visibility.Visible;
        //        SpriteStatus.Visibility = Visibility.Collapsed;
        //    }

        //}

        //private void EditSprites_Checked(object sender, RoutedEventArgs e)
        //{
        //    EditMode = EditMode.Objects;
        //    if (SelectionRectangle != null) SelectionRectangle.Visibility = Visibility.Collapsed;

        //    TileStatus.Visibility = Visibility.Collapsed;
        //    SpriteStatus.Visibility = Visibility.Visible;
        //    DrawModeStatus.Visibility = Visibility.Collapsed;
        //}

        //private void EditPointers_Checked(object sender, RoutedEventArgs e)
        //{
        //    EditMode = EditMode.Pointers;
        //    if (SelectionRectangle != null) SelectionRectangle.Visibility = Visibility.Collapsed;
        //}


        private void UpdateTextTables()
        {
            List<KeyValuePair<string, string>> _graphicsSetNames = new List<KeyValuePair<string, string>>();
            for (int i = 0; i < 256; i++)
            {
                _graphicsSetNames.Add(new KeyValuePair<string, string>(i.ToString(), i.ToString("X")));
            }

            foreach (var kv in _textService.GetTable("graphics"))
            {
                _graphicsSetNames[int.Parse(kv.Key, System.Globalization.NumberStyles.HexNumber)] = new KeyValuePair<string, string>(kv.Key, kv.Value);
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
            EffectType.SelectedValue = _level.Effects.ToString("X");
            EventType.SelectedValue = _level.EventType.ToString("X");
            PaletteIndex.SelectedValue = _level.PaletteId;
            GraphicsSet.SelectedValue = _level.GraphicsSet.ToString("X");
            PaletteEffect.SelectedValue = _level.PaletteEffect.ToString();
            Screens.SelectedIndex = _level.ScreenLength - 1;
            ScrollType.SelectedValue = _level.ScrollType.ToString("X");
            TileSet.SelectedValue = _level.TileSetIndex.ToString("X");
        }

        private void AnimationType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _level.AnimationType = int.Parse(AnimationType.SelectedValue.ToString());
            _graphicsAccessor.SetAnimatedTable(_graphicsService.GetTileSection(_level.AnimationTileTableIndex));
            TileSelector.Update();
            Update();
        }

        private void GraphicsSet_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _level.GraphicsSet = int.Parse(GraphicsSet.SelectedValue.ToString());
            _graphicsAccessor.SetStaticTable(_graphicsService.GetTileSection(_level.GraphicsSet));
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
            CanvasContainer.Width = RenderContainer.Width = _level.ScreenLength * 16 * 16;
        }

        private void ScrollType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _level.ScrollType = int.Parse(ScrollType.SelectedValue.ToString());
        }

        private void TileSet_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _level.TileSetIndex = int.Parse(TileSet.SelectedValue.ToString(), System.Globalization.NumberStyles.HexNumber);

            _tileSet = _tileService.GetTileSet(_level.TileSetIndex);
            _levelRenderer.SetTileSet(_tileSet);
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
            _levelService.SaveLevel(_level);
            _level.ObjectData.Insert(0, startPointObject);

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

                Update(Rect.Union(_selectedObject.VisualRectangle, _selectedObject.CalcVisualBox(true)));
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

        private void LevelRenderSource_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                switch (e.Key)
                {
                    case Key.S:
                        SaveLevel();
                        break;

                    case Key.C:
                        CopySelection();
                        break;

                    case Key.V:
                        PasteSelection();
                        break;

                    case Key.X:
                        CutSelection();
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

                    if (_editMode == EditMode.Pointers)
                    {
                        DeletePointer();
                    }
                    break;
            }
        }

        private void LevelRenderSource_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (!GameObjectProperty.IsMouseOver && !PointerEditor.IsMouseOver && LevelScroller.IsMouseOver)
            {
                LevelRenderSource.Focus();
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
                        TileStatus.Visibility = Visibility.Visible;
                        SpriteStatus.Visibility = Visibility.Collapsed;
                    }
                    break;

                case 1:
                    _editMode = EditMode.Objects;

                    if (TileStatus != null)
                    {
                        TileStatus.Visibility = Visibility.Collapsed;
                        SpriteStatus.Visibility = Visibility.Visible;
                    }
                    break;

                case 2:
                    _editMode = EditMode.Pointers;

                    if (GameObjectProperty != null)
                    {
                        GameObjectProperty.Visibility = Visibility.Collapsed;

                    }
                    _selectedObject = null;
                    break;
            }
        }

        private EditMode _previousEditMode;
        private void ShowPSwitch_Checked(object sender, RoutedEventArgs e)
        {
            Update();
        }

        private void ShowTerrain_Click(object sender, RoutedEventArgs e)
        {
            Update();
        }

        private void ShowInteraction_Click(object sender, RoutedEventArgs e)
        {
            Update();
        }

        private void ShowPSwitch_Click(object sender, RoutedEventArgs e)
        {
            if (_editMode != EditMode.PSwitchView)
            {
                for (int i = 0; i < _level.TileData.Length; i++)
                {
                    int tileValue = _level.TileData[i];
                    for (int j = 0; j < 8; j++)
                    {
                        if (_tileSet.PSwitchAlterations[j].From == tileValue)
                        {
                            _level.TileData[i] = _tileSet.PSwitchAlterations[j].To;
                        }
                    }
                }
                _previousEditMode = _editMode;
                _editMode = EditMode.PSwitchView;
                _graphicsAccessor.SetAnimatedTable(_graphicsService.GetTileSection(_level.PSwitchAnimationTileTableIndex));
                ShowPSwitch.IsChecked = true;
            }
            else
            {
                for (int i = 0; i < _level.TileData.Length; i++)
                {
                    int tileValue = _level.TileData[i];
                    for (int j = 0; j < 8; j++)
                    {
                        if (_tileSet.PSwitchAlterations[j].To == tileValue)
                        {
                            _level.TileData[i] = _tileSet.PSwitchAlterations[j].From;
                        }
                    }
                }

                _editMode = _previousEditMode;
                _graphicsAccessor.SetAnimatedTable(_graphicsService.GetTileSection(_level.AnimationTileTableIndex));
                ShowPSwitch.IsChecked = false;
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
        Pointers
    }

    public enum SelectionMode
    {
        SetTiles,
        SelectTiles
    }
}
