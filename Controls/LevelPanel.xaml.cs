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
        private GraphicsAccessor _graphicsAccessor;
        private LevelDataAccessor _levelDataAccessor;
        private WriteableBitmap _bitmap;
        public LevelPanel(GraphicsService graphicsService, TextService textService, TileService tileService, LevelService levelService, Level level)
        {
            InitializeComponent();

            _textService = textService;
            _graphicsService = graphicsService;
            _tileService = tileService;
            _levelService = levelService;
            _level = level;

            Tile[] staticSet = _graphicsService.GetTileSection(level.StaticTileTableIndex);
            Tile[] animationSet = _graphicsService.GetTileSection(level.AnimationTileTableIndex);
            _graphicsAccessor = new GraphicsAccessor(staticSet, animationSet, _graphicsService.GetGlobalTiles());
            _levelDataAccessor = new LevelDataAccessor(level);

            _bitmap = new WriteableBitmap(LevelRenderer.BITMAP_WIDTH, LevelRenderer.BITMAP_HEIGHT, 96, 96, PixelFormats.Bgra32, null);
            _renderer = new LevelRenderer(_graphicsAccessor, _levelDataAccessor);

            TileSet tileSet = _tileService.GetTileSet(level.TileSetIndex);
            _renderer.SetTileSet(tileSet);

            Palette palette = _graphicsService.GetPalette(level.PaletteIndex);
            _renderer.SetPalette(palette);

            LevelRenderSource.Source = _bitmap;
            LevelRenderSource.Width = _bitmap.PixelWidth;
            LevelRenderSource.Height = _bitmap.PixelHeight;
            CanvasContainer.Width = RenderContainer.Width = level.ScreenLength * 16 * 16;
            level.ObjectData.ForEach(o => o.CalcBoundBox());

            EditMode = EditMode.Tiles;

            Update();
            UpdateTextTables();

            TileSelector.Initialize(_graphicsAccessor, tileSet, palette);
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
                Int32Rect sourceArea = new Int32Rect(0, 0, Math.Min(updateArea.Width, LevelRenderer.BITMAP_WIDTH), Math.Min(updateArea.Height, LevelRenderer.BITMAP_HEIGHT));
                _renderer.Update(updateArea);
                _bitmap.WritePixels(sourceArea, _renderer.GetRectangle(updateArea), updateArea.Width * 4, updateArea.X, updateArea.Y);
                _bitmap.AddDirtyRect(updateArea);
            }

            _bitmap.Unlock();
        }

        private void ClearSelectionRectangle()
        {
            SelectionRectangle.Visibility = Visibility.Collapsed;

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
        private EditMode EditMode;
        private DrawMode DrawMode;
        private bool IsDragging = false;
        private Point DragStartPoint;
        private void LevelRenderSource_MouseDown(object sender, MouseButtonEventArgs e)
        {

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
                if (DrawMode == DrawMode.Default)
                {
                    DragStartPoint = clickPoint;
                    IsDragging = true;
                    originalSpritePoint = clickPoint;
                }
                else if (DrawMode == DrawMode.Replace)
                {
                    int x = (int)(clickPoint.X / 16), y = (int)(clickPoint.Y / 16);
                    int tileValue = _levelDataAccessor.GetData(x, y);

                    if (tileValue != TileSelector.SelectedTileValue)
                    {
                        _levelDataAccessor.ReplaceValue(tileValue, TileSelector.SelectedTileValue);
                        Update();
                    }
                }
                else if (DrawMode == DrawMode.Fill)
                {
                    int x = (int)(clickPoint.X / 16), y = (int)(clickPoint.Y / 16);

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
        }

        private void HandleSpriteClick(MouseButtonEventArgs e)
        {
            Point tilePoint = e.GetPosition(LevelRenderSource);
            List<Rect> updatedRects = new List<Rect>();


            if (e.LeftButton == MouseButtonState.Pressed)
            {
                selectedObject = _level.ObjectData.Where(o => o.BoundRectangle.Contains(tilePoint.X, tilePoint.Y)).FirstOrDefault();

                if (selectedObject != null)
                {
                    DragStartPoint = tilePoint;
                    SetSelectionRectangle(selectedObject.BoundRectangle);
                    originalSpritePoint = new Point(selectedObject.X * 16, selectedObject.Y * 16);
                    IsDragging = true;

                    GameObjectProperty.Visibility = Visibility.Visible;
                    Rect boundRect = selectedObject.BoundRectangle;
                    Canvas.SetTop(GameObjectProperty, boundRect.Bottom + 4);
                    Canvas.SetLeft(GameObjectProperty, boundRect.Left);

                    GameObjectProperty.ItemsSource = selectedObject.GameObject.Properties;
                    GameObjectProperty.SelectedIndex = selectedObject.Property;
                }
                else
                {
                    GameObjectProperty.Visibility = Visibility.Collapsed;
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

        private void HandleTileMove(MouseEventArgs e)
        {
            Point tilePoint = Snap(e.GetPosition(LevelRenderSource));
            if (IsDragging && DrawMode == DrawMode.Default)
            {
                int x = (int)Math.Min(tilePoint.X, DragStartPoint.X);
                int y = (int)Math.Min(tilePoint.Y, DragStartPoint.Y);
                int width = (int)(Math.Max(tilePoint.X, DragStartPoint.X)) - x;
                int height = (int)(Math.Max(tilePoint.Y, DragStartPoint.Y)) - y;

                SetSelectionRectangle(new Rect(x, y, width + 16, height + 16));
            }
            else
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


                if (diffPoint.X >= 16 || diffPoint.X <= -16 || diffPoint.Y >= 16  || diffPoint.Y <= -16)
                {
                    List<Rect> updateRects = new List<Rect>();

                    Rect oldRect = selectedObject.BoundRectangle;
                    
                    if(oldRect.Right >= LevelRenderer.BITMAP_WIDTH)
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

                    if(newX >= Level.BLOCK_WIDTH)
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

                    Rect updateArea = selectedObject.BoundRectangle;

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
        }

        private int Snap(double value)
        {
            return (int)(Math.Floor(value / 16) * 16);
        }

        private Point Snap(Point value)
        {
            return new Point(Snap(value.X), Snap(value.Y));
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

        private void HandleTileRelease(MouseButtonEventArgs e)
        {
            Point mousePoint = Snap(e.GetPosition(LevelRenderSource));

            if (DrawMode == DrawMode.Default)
            {
                IsDragging = false;

                int columnStart = (int)(Math.Min(originalSpritePoint.X, mousePoint.X) / 16);
                int rowStart = (int)(Math.Min(originalSpritePoint.Y, mousePoint.Y) / 16);
                int columnEnd = (int)(Math.Max(originalSpritePoint.X, mousePoint.X) / 16) + 1;
                int rowEnd = (int)(Math.Max(originalSpritePoint.Y, mousePoint.Y) / 16) + 1;

                for (var c = columnStart; c <= columnEnd; c++)
                {
                    for (var r = rowStart; r <= rowEnd; r++)
                    {
                        _levelDataAccessor.SetData(c, r, TileSelector.SelectedTileValue);
                    }
                }

                Update(new Rect(columnStart * 16, rowStart * 16, (columnEnd - columnStart) * 16, (rowEnd - rowStart) * 16));
                SetSelectionRectangle(new Rect(mousePoint.X, mousePoint.Y, 16, 16));
            }
        }

        private void HandleSpriteRelease(MouseButtonEventArgs e)
        {
            IsDragging = false;
            if (selectedObject != null)
            {
                GameObjectProperty.Visibility = Visibility.Visible;
                Rect boundRect = selectedObject.BoundRectangle;
                Canvas.SetTop(GameObjectProperty, boundRect.Bottom + 4);
                Canvas.SetLeft(GameObjectProperty, boundRect.Left);

                GameObjectProperty.ItemsSource = selectedObject.GameObject.Properties;
                GameObjectProperty.SelectedIndex = selectedObject.Property;
            }
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
            PaletteIndex.ItemsSource = _graphicsService.GetPaletteNames();
            GraphicsSet.ItemsSource = _graphicsSetNames;
            PaletteEffect.ItemsSource = _textService.GetTable("palette_effect");
            Screens.ItemsSource = new int[15] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };
            ScrollType.ItemsSource = _textService.GetTable("scroll_types");
            TileSet.ItemsSource = _textService.GetTable("tile_sets");

            Music.SelectedValue = _level.MusicValue.ToString("X");
            AnimationType.SelectedValue = _level.AnimationType.ToString();
            EffectType.SelectedValue = _level.Effects.ToString("X");
            EventType.SelectedValue = _level.EventType.ToString("X");
            PaletteIndex.SelectedIndex = _level.PaletteIndex;
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
            _level.PaletteIndex = int.Parse(PaletteIndex.SelectedIndex.ToString());

            Palette palette = _graphicsService.GetPalette(_level.PaletteIndex);
            _renderer.SetPalette(_graphicsService.GetPalette(_level.PaletteIndex));
            TileSelector.Update(palette);
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
            if(selectedObject != null && GameObjectProperty.SelectedIndex > -1)
            {
                selectedObject.Property = GameObjectProperty.SelectedIndex;
                Update(selectedObject.BoundRectangle);
            }
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
}
