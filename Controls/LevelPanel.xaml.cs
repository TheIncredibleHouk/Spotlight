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
            _bitmap = new WriteableBitmap(LevelRenderer.BITMAP_WIDTH, LevelRenderer.BITMAP_HEIGHT, 96, 96, PixelFormats.Bgra32, null);

            _renderer = new LevelRenderer(_graphicsAccessor, new LevelDataAccessor(level));

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
                Int32Rect sourceArea = new Int32Rect(0, 0, updateArea.Width, updateArea.Height);
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
                DragStartPoint = clickPoint;
                IsDragging = true;
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

        private void HandleTileMove(MouseEventArgs e)
        {
            Point tilePoint = Snap(e.GetPosition(LevelRenderSource));
            if (IsDragging)
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

                List<Rect> updateRects = new List<Rect>();
                updateRects.Add(selectedObject.BoundRectangle);

                int newX = (int)((originalSpritePoint.X + diffPoint.X) / 16);
                int newY = (int)((originalSpritePoint.Y + diffPoint.Y) / 16);


                if (newX == selectedObject.X && newY == selectedObject.Y)
                {
                    return;
                }

                selectedObject.X = newX;
                selectedObject.Y = newY;
                selectedObject.CalcBoundBox();

                updateRects.Add(selectedObject.BoundRectangle);
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
            IsDragging = false;

            SetSelectionRectangle(new Rect(mousePoint.X, mousePoint.Y, 16, 16));
        }

        private void HandleSpriteRelease(MouseButtonEventArgs e)
        {
            IsDragging = false;
        }

        private void EditTiles_Checked(object sender, RoutedEventArgs e)
        {
            EditMode = EditMode.Tiles;
            if (SelectionRectangle != null) SelectionRectangle.Visibility = Visibility.Collapsed;
            selectedObject = null;
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
    }

    public enum EditMode
    {
        Tiles,
        Objects,
        Pointers
    }
}
