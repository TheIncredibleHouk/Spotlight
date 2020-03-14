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
    public partial class LevelPanel : UserControl
    {
        private Level _level;
        private LevelService _levelService;
        private LevelRenderer _renderer;
        private TextService _textService;
        private GraphicsService _graphicsService;
        private TileService _tileService;
        private GameObjectService _gameObjectService;
        private GraphicsAccessor _graphicsAccessor;
        private LevelDataAccessor _levelDataAccessor;
        private WriteableBitmap _bitmap;
        private HistoryService _historyService;
        public LevelPanel(GraphicsService graphicsService, TextService textService, TileService tileService, GameObjectService gameObjectService, LevelService levelService, Level level)
        {
            InitializeComponent();

            _textService = textService;
            _graphicsService = graphicsService;
            _gameObjectService = gameObjectService;
            _tileService = tileService;
            _levelService = levelService;
            _historyService = new HistoryService();
            _level = level;

            Tile[] staticSet = _graphicsService.GetTileSection(level.StaticTileTableIndex);
            Tile[] animationSet = _graphicsService.GetTileSection(level.AnimationTileTableIndex);
            _graphicsAccessor = new GraphicsAccessor(staticSet, animationSet, _graphicsService.GetGlobalTiles(), _graphicsService.GetExtraTiles());
            _levelDataAccessor = new LevelDataAccessor(level);

            _bitmap = new WriteableBitmap(LevelRenderer.BITMAP_WIDTH, LevelRenderer.BITMAP_HEIGHT, 96, 96, PixelFormats.Bgra32, null);
            _renderer = new LevelRenderer(_graphicsAccessor, _levelDataAccessor, _gameObjectService);

            TileSet tileSet = _tileService.GetTileSet(level.TileSetIndex);
            _renderer.SetTileSet(tileSet);

            Palette palette = _graphicsService.GetPalette(level.PaletteId);
            _renderer.SetPalette(palette);

            LevelRenderSource.Source = _bitmap;
            LevelRenderSource.Width = _bitmap.PixelWidth;
            LevelRenderSource.Height = _bitmap.PixelHeight;
            CanvasContainer.Width = RenderContainer.Width = level.ScreenLength * 16 * 16;
            level.ObjectData.ForEach(o =>
            {
                o.CalcBoundBox();
                o.CalcVisualBox(true);
            });

            EditMode = EditMode.Tiles;

            TileSelector.Initialize(_graphicsAccessor, tileSet, palette);
            ObjectSelector.Initialize(_gameObjectService, _graphicsAccessor, palette);

            Update();
            UpdateTextTables();

            gameObjectService.GameObjectUpdated += GameObjectService_GameObjectsUpdated;
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
            if (updateAreas.Count == 0)
            {
                return;
            }

            _bitmap.Lock();

            foreach (var updateArea in updateAreas.Select(r => new Int32Rect((int)r.X, (int)r.Y, (int)r.Width, (int)r.Height)))
            {
                Int32Rect safeRect = new Int32Rect(updateArea.X, updateArea.Y, updateArea.Width, updateArea.Height);

                if (safeRect.X + safeRect.Width > LevelRenderer.BITMAP_WIDTH)
                {
                    safeRect.Width -= LevelRenderer.BITMAP_WIDTH - (safeRect.X + safeRect.Width);
                }

                if (safeRect.Y + safeRect.Height > LevelRenderer.BITMAP_HEIGHT)
                {
                    safeRect.Height -= LevelRenderer.BITMAP_HEIGHT - (safeRect.Y + safeRect.Height);
                }

                Int32Rect sourceArea = new Int32Rect(0, 0, Math.Min(safeRect.Width, LevelRenderer.BITMAP_WIDTH), Math.Min(safeRect.Height, LevelRenderer.BITMAP_HEIGHT));

                _renderer.Update(safeRect, withOverlays);
                _bitmap.WritePixels(sourceArea, _renderer.GetRectangle(safeRect), safeRect.Width * 4, safeRect.X, safeRect.Y);
                _bitmap.AddDirtyRect(safeRect);
            }

            _bitmap.Unlock();
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

        private LevelObject selectedObject;
        private EditMode _editMode;
        private EditMode EditMode
        {
            get
            {
                return _editMode;
            }
            set
            {
                if (_editMode != value)
                {
                    _editMode = value;
                    switch (_editMode)
                    {
                        case EditMode.Objects:
                            EditSprites.IsChecked = true;
                            break;

                        case EditMode.Pointers:
                            EditPointers.IsChecked = true;
                            break;

                        case EditMode.Tiles:
                            EditTiles.IsChecked = true;
                            break;
                    }
                }
            }
        }
        private DrawMode DrawMode;
        private bool IsDragging = false;
        private Point DragStartPoint;
        private void LevelRenderSource_MouseDown(object sender, MouseButtonEventArgs e)
        {
            LevelRenderSource.Focusable = true;
            LevelRenderSource.Focus();

            switch (EditMode)
            {
                case EditMode.Tiles:
                    HandleTileClick(e);
                    break;

                case EditMode.Objects:
                    HandleSpriteClick(e);
                    break;

                case EditMode.Pointers:
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
                    TileSelector.SelectedTileValue = tileValue;
                    return;
                }

                _selectionMode = SelectionMode.SetTiles;

                if (DrawMode == DrawMode.Default)
                {
                    DragStartPoint = clickPoint;
                    IsDragging = true;
                    originalSpritePoint = clickPoint;
                }
                else if (DrawMode == DrawMode.Replace)
                {
                    if (tileValue != TileSelector.SelectedTileValue)
                    {
                        _levelDataAccessor.ReplaceValue(tileValue, TileSelector.SelectedTileValue);
                        Update();
                    }
                }
                else if (DrawMode == DrawMode.Fill)
                {
                    Stack<Point> stack = new Stack<Point>();
                    stack.Push(new Point(x, y));

                    int checkValue = _levelDataAccessor.GetData(x, y);
                    if (checkValue == TileSelector.SelectedTileValue)
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
                            _levelDataAccessor.SetData(i, j, TileSelector.SelectedTileValue);

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

                    Update(new Rect(lowestX * 16, lowestY * 16, (highestX - lowestX + 1) * 16, (highestY - lowestY + 1) * 16));
                }
            }
            else if (e.RightButton == MouseButtonState.Pressed)
            {
                _selectionMode = SelectionMode.SelectTiles;

                int x = (int)(clickPoint.X / 16), y = (int)(clickPoint.Y / 16);

                DragStartPoint = clickPoint;
                IsDragging = true;
                originalSpritePoint = clickPoint;

                SetSelectionRectangle(new Rect(x * 16, y * 16, 16, 16));
            }
        }

        private void HandleSpriteClick(MouseButtonEventArgs e)
        {
            Point tilePoint = e.GetPosition(LevelRenderSource);
            List<Rect> updatedRects = new List<Rect>();


            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (Keyboard.Modifiers == ModifierKeys.Shift)
                {
                    selectedObject = _level.ObjectData.Where(o => o.BoundRectangle.Contains(tilePoint.X, tilePoint.Y)).FirstOrDefault();
                    if (selectedObject != null)
                    {
                        _historyService.UndoLevelObjects.Push(new LevelObjectChange(selectedObject, selectedObject.X, selectedObject.Y, selectedObject.Property, selectedObject.GameObjectId, LevelObjectChangeType.Update));
                        ObjectSelector.SelectedObject = selectedObject.GameObject;
                    }
                    else
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

                selectedObject = _level.ObjectData.Where(o => o.BoundRectangle.Contains(tilePoint.X, tilePoint.Y)).FirstOrDefault();

                if (selectedObject != null)
                {
                    DragStartPoint = tilePoint;
                    SetSelectionRectangle(selectedObject.BoundRectangle);
                    originalSpritePoint = new Point(selectedObject.X * 16, selectedObject.Y * 16);
                    IsDragging = true;
                    _historyService.UndoLevelObjects.Push(new LevelObjectChange(selectedObject, selectedObject.X, selectedObject.Y, selectedObject.Property, selectedObject.GameObjectId, LevelObjectChangeType.Update));


                    if (selectedObject.GameObject.Properties != null && selectedObject.GameObject.Properties.Count > 0)
                    {
                        GameObjectProperty.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        GameObjectProperty.Visibility = Visibility.Collapsed;
                    }

                    Rect boundRect = selectedObject.BoundRectangle;
                    Canvas.SetTop(GameObjectProperty, boundRect.Bottom + 4);
                    Canvas.SetLeft(GameObjectProperty, boundRect.Left);

                    GameObjectProperty.ItemsSource = selectedObject.GameObject.Properties;
                    GameObjectProperty.SelectedIndex = selectedObject.Property;
                }
                else
                {
                    ClearSelectionRectangle();
                }

                Update(updatedRects);
            }
        }

        private void LevelRenderSource_MouseMove(object sender, MouseEventArgs e)
        {
            Point tilePoint = Snap(e.GetPosition(LevelRenderSource));

            switch (EditMode)
            {
                case EditMode.Tiles:
                    HandleTileMove(e);
                    break;

                case EditMode.Objects:
                    HandleSpriteMove(e);
                    break;
            }
        }

        private SelectionMode _selectionMode;
        private void HandleTileMove(MouseEventArgs e)
        {
            Point tilePoint = Snap(e.GetPosition(LevelRenderSource));
            if (IsDragging && ((_selectionMode == SelectionMode.SetTiles && DrawMode == DrawMode.Default) ||
                               _selectionMode == SelectionMode.SelectTiles))
            {
                int x = (int)Math.Min(tilePoint.X, DragStartPoint.X);
                int y = (int)Math.Min(tilePoint.Y, DragStartPoint.Y);
                int width = (int)(Math.Max(tilePoint.X, DragStartPoint.X)) - x;
                int height = (int)(Math.Max(tilePoint.Y, DragStartPoint.Y)) - y;

                SetSelectionRectangle(new Rect(x, y, width + 16, height + 16));
            }
            else if (_selectionMode != SelectionMode.SelectTiles)
            {
                SetSelectionRectangle(new Rect(tilePoint.X, tilePoint.Y, 16, 16));
            }
        }

        private Point originalSpritePoint;
        private void HandleSpriteMove(MouseEventArgs e)
        {
            Point movePoint = Snap(e.GetPosition(LevelRenderSource));

            if (selectedObject != null && IsDragging)
            {
                Point diffPoint = Snap(new Point(movePoint.X - originalSpritePoint.X, movePoint.Y - originalSpritePoint.Y));

                List<Rect> updateRects = new List<Rect>();

                Rect oldRect = selectedObject.VisualRectangle;

                if (oldRect.Right >= LevelRenderer.BITMAP_WIDTH)
                {
                    oldRect.Width = oldRect.Width - (oldRect.Right - LevelRenderer.BITMAP_WIDTH);
                }

                if (oldRect.Bottom >= LevelRenderer.BITMAP_HEIGHT)
                {
                    oldRect.Height = oldRect.Height - (oldRect.Bottom - LevelRenderer.BITMAP_HEIGHT);
                }

                updateRects.Add(oldRect);

                int newX = (int)((originalSpritePoint.X + diffPoint.X) / 16);
                int newY = (int)((originalSpritePoint.Y + diffPoint.Y) / 16);


                if (newX == selectedObject.X && newY == selectedObject.Y)
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

                selectedObject.X = newX;
                selectedObject.Y = newY;
                selectedObject.CalcBoundBox();
                selectedObject.CalcVisualBox(true);

                Rect updateArea = selectedObject.VisualRectangle;

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

                SetSelectionRectangle(selectedObject.BoundRectangle);
                Update(updateRects);

            }
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

        private void DeleteObject()
        {
            if (selectedObject != null)
            {
                _level.ObjectData.Remove(selectedObject);
                Update(selectedObject.VisualRectangle);
                ClearSelectionRectangle();
                _historyService.UndoLevelObjects.Push(new LevelObjectChange(selectedObject, selectedObject.X, selectedObject.Y, selectedObject.Property, selectedObject.GameObjectId, LevelObjectChangeType.Deletion));
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
            switch (EditMode)
            {
                case EditMode.Tiles:
                    HandleTileRelease(e);
                    break;

                case EditMode.Objects:
                    HandleSpriteRelease(e);
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
                    reverseTileChange.Data[i, j] = _levelDataAccessor.GetData(c, r);
                    _levelDataAccessor.SetData(c, r, tileChange.Data[i, j]);
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
                selectedObject = null;
                ClearSelectionRectangle();
            }
            else if (objectChange.ChangeType == LevelObjectChangeType.Deletion)
            {
                newChange = new LevelObjectChange(objectChange.OriginalObject, objectChange.OriginalObject.X, objectChange.OriginalObject.Y, objectChange.OriginalObject.Property, objectChange.OriginalObject.GameObjectId, LevelObjectChangeType.Addition);
                _level.ObjectData.Add(objectChange.OriginalObject);
                selectedObject = objectChange.OriginalObject;
                SetSelectionRectangle(selectedObject.CalcBoundBox());
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
                selectedObject = objectChange.OriginalObject;
                SetSelectionRectangle(selectedObject.CalcBoundBox());
                UpdateProperties();
            }

            Update(updateRects);
            return newChange;
        }

        private void HandleTileRelease(MouseButtonEventArgs e)
        {
            Point mousePoint = Snap(e.GetPosition(LevelRenderSource));

            if (DrawMode == DrawMode.Default && IsDragging)
            {
                IsDragging = false;

                if (_selectionMode == SelectionMode.SetTiles)
                {
                    int columnStart = (int)(Math.Min(originalSpritePoint.X, mousePoint.X) / 16);
                    int rowStart = (int)(Math.Min(originalSpritePoint.Y, mousePoint.Y) / 16);
                    int columnEnd = (int)(Math.Max(originalSpritePoint.X, mousePoint.X) / 16);
                    int rowEnd = (int)(Math.Max(originalSpritePoint.Y, mousePoint.Y) / 16);

                    TileChange tileChange = new TileChange(columnStart, rowStart, (columnEnd - columnStart) + 1, (rowEnd - rowStart) + 1);

                    for (int c = columnStart, i = 0; c <= columnEnd; c++, i++)
                    {
                        for (int r = rowStart, j = 0; r <= rowEnd; r++, j++)
                        {
                            tileChange.Data[i, j] = _levelDataAccessor.GetData(c, r);
                            _levelDataAccessor.SetData(c, r, TileSelector.SelectedTileValue);
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
            IsDragging = false;
            if (selectedObject != null)
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

        private void UpdateProperties()
        {
            if (selectedObject.GameObject.Properties != null && selectedObject.GameObject.Properties.Count > 0)
            {
                GameObjectProperty.Visibility = Visibility.Visible;
            }
            else
            {
                GameObjectProperty.Visibility = Visibility.Collapsed;
            }

            Rect boundRect = selectedObject.BoundRectangle;
            Canvas.SetTop(GameObjectProperty, boundRect.Bottom + 4);
            Canvas.SetLeft(GameObjectProperty, boundRect.Left);

            GameObjectProperty.ItemsSource = selectedObject.GameObject.Properties;
            GameObjectProperty.SelectedIndex = selectedObject.Property;
        }

        private void EditTiles_Checked(object sender, RoutedEventArgs e)
        {
            EditMode = EditMode.Tiles;
            if (SelectionRectangle != null)
            {
                SelectionRectangle.Visibility = Visibility.Collapsed;
            }

            selectedObject = null;
            if (GameObjectProperty != null)
            {
                GameObjectProperty.Visibility = Visibility.Collapsed;
            }
        }

        private void EditSprites_Checked(object sender, RoutedEventArgs e)
        {
            EditMode = EditMode.Objects;
            if (SelectionRectangle != null) SelectionRectangle.Visibility = Visibility.Collapsed;
        }

        private void EditPointers_Checked(object sender, RoutedEventArgs e)
        {
            EditMode = EditMode.Pointers;
            if (SelectionRectangle != null) SelectionRectangle.Visibility = Visibility.Collapsed;
        }


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
            PaletteIndex.ItemsSource = _graphicsService.GetPalettes();
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
            _level.PaletteId = ((Palette)PaletteIndex.SelectedItem).Id;

            Palette palette = _graphicsService.GetPalette(_level.PaletteId);
            _renderer.SetPalette(_graphicsService.GetPalette(_level.PaletteId));
            TileSelector.Update(palette);
            ObjectSelector.Update(palette);
            Update();
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

            TileSet tileSet = _tileService.GetTileSet(_level.TileSetIndex);
            _renderer.SetTileSet(tileSet);
            TileSelector.Update(tileSet);
            Update();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            _levelService.SaveLevel(_level);
            MessageBox.Show(_level.Name + " has been saved!", "Save succes");
        }

        private void EffectType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _level.Effects = int.Parse(EffectType.SelectedValue.ToString(), System.Globalization.NumberStyles.HexNumber);
        }

        private void ShowGrid_Checked(object sender, RoutedEventArgs e)
        {
            _renderer.RenderGrid = true;
            Update();
        }

        private void ShowGrid_Unchecked(object sender, RoutedEventArgs e)
        {
            _renderer.RenderGrid = false;
            Update();
        }

        private void ShowScreenLines_Checked(object sender, RoutedEventArgs e)
        {
            _renderer.ScreenBorders = true;
            Update();
        }

        private void ShowScreenLines_Unchecked(object sender, RoutedEventArgs e)
        {
            _renderer.ScreenBorders = false;
            Update();
        }

        private void DefaultDraw_Checked(object sender, RoutedEventArgs e)
        {
            DrawMode = DrawMode.Default;
        }

        private void FillDraw_Checked(object sender, RoutedEventArgs e)
        {
            DrawMode = DrawMode.Fill;
        }

        private void ReplaceDraw_Checked(object sender, RoutedEventArgs e)
        {
            DrawMode = DrawMode.Replace;
        }

        private void GameObjectProperty_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (selectedObject != null && GameObjectProperty.SelectedIndex > -1)
            {
                LevelObjectChange objectChange = new LevelObjectChange(selectedObject, selectedObject.X, selectedObject.Y, selectedObject.Property, selectedObject.GameObjectId, LevelObjectChangeType.Update);

                selectedObject.Property = GameObjectProperty.SelectedIndex;

                if (!objectChange.IsSame())
                {
                    _historyService.UndoLevelObjects.Push(objectChange);
                    _historyService.RedoLevelObjects.Clear();
                }

                Update(selectedObject.BoundRectangle);
                LevelRenderSource.Focus();
            }
        }

        private void ObjectSelector_MouseDown(object sender, MouseButtonEventArgs e)
        {
            EditMode = EditMode.Objects;
        }

        private void TileSelector_MouseDown(object sender, MouseButtonEventArgs e)
        {
            EditMode = EditMode.Tiles;
        }

        private bool withOverlays = false;
        private void ShowExtraSprites_Unchecked(object sender, RoutedEventArgs e)
        {
            withOverlays = ShowExtraSprites.IsChecked.Value;
            Update();
        }

        private void LevelRenderSource_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                switch (e.Key)
                {
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
                    break;
            }
        }

        private void LevelRenderSource_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (!GameObjectProperty.IsMouseOver)
            {
                LevelRenderSource.Focus();
            }
        }


        private bool _showTerrain;
        private void ShowTerrain_Unchecked(object sender, RoutedEventArgs e)
        {
            _showTerrain = ShowTerrain.IsChecked.Value;
            Update();
        }

        private void ShowTerrain_Checked(object sender, RoutedEventArgs e)
        {
            _showTerrain = ShowTerrain.IsChecked.Value;
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
