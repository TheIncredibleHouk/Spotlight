using Spotlight.Models;
using Spotlight.Renderers;
using Spotlight.Services;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Spotlight
{
    /// <summary>
    /// Interaction logic for GameObjectSelector.xaml
    /// </summary>
    public partial class GameObjectSelector : UserControl, IDetachEvents
    {
        public delegate void GameObjectSelectorEventHandler(GameObject gameObject);

        public event GameObjectSelectorEventHandler GameObjectChanged;

        public event GameObjectSelectorEventHandler GameObjectDoubleClicked;

        public GameObjectSelector()
        {
            InitializeComponent();
        }

        private GameObjectService _gameObjectService;
        private GraphicsAccessor _graphicsAccessor;
        private PalettesService _palettesService;
        private Palette _palette;
        private GameObjectRenderer _renderer;
        private WriteableBitmap _bitmap;

        public void Initialize(GameObjectService gameObjectService, PalettesService palettesService, GraphicsAccessor graphicsAccessor, Palette palette)
        {
            _gameObjectService = gameObjectService;
            _graphicsAccessor = graphicsAccessor;
            _palette = palette;
            _palettesService = palettesService;

            _renderer = new GameObjectRenderer(gameObjectService, _palettesService, graphicsAccessor);

            Dpi dpi = this.GetDpi();
            _bitmap = new WriteableBitmap(256, 256, dpi.X, dpi.Y, PixelFormats.Bgra32, null);

            _selectedGroup = new Dictionary<GameObjectType, string>();
            _selectedGroup[GameObjectType.Global] = null;
            _selectedGroup[GameObjectType.TypeA] = null;
            _selectedGroup[GameObjectType.TypeB] = null;

            _selectedObject = null;

            GameObjectImage.Source = _bitmap;
            GameObjectTypes.ItemsSource = new List<GameObjectType>() { GameObjectType.Global, GameObjectType.TypeA, GameObjectType.TypeB };

            _renderer.Update(palette);

            CanvasArea.Background = GameObjectImageBorder.BorderBrush = new SolidColorBrush(palette.RgbColors[0][0].ToMediaColor());
            GameObjectTypes.SelectedIndex = 0;

            _gameObjectService.GameObjectUpdated += GameObjectsUpdated;
        }

        public void DetachEvents()
        {
            _gameObjectService.GameObjectUpdated -= GameObjectsUpdated;
        }

        private void GameObjectsUpdated(GameObject gameObject)
        {
            GameObjectType_SelectionChanged(null, null);
            Update();
        }

        public void Update(Palette palette)
        {
            _palette = palette;
            _renderer.Update(palette);
            CanvasArea.Background = GameObjectImageBorder.Background = GameObjectImageBorder.BorderBrush = new SolidColorBrush(palette.RgbColors[0][0].ToMediaColor());
            Update();
        }

        public void Update()
        {
            Int32Rect sourceArea = new Int32Rect(0, 0, 256, 256);
            _renderer.Clear();
            _renderer.Update(_gameObjectService.GetObjects((GameObjectType)GameObjectTypes.SelectedItem, (string)GameObjectGroups.SelectedItem), false);

            _bitmap.Lock();
            _bitmap.WritePixels(sourceArea, _renderer.GetRectangle(sourceArea), sourceArea.Width * 4, sourceArea.X, sourceArea.Y);
            _bitmap.AddDirtyRect(sourceArea);
            _bitmap.Unlock();
        }

        private LevelObject _selectedObject;

        public GameObject SelectedObject
        {
            get
            {
                if (_selectedObject == null)
                {
                    return null;
                }

                return _selectedObject.GameObject;
            }
            set
            {
                if (value != null)
                {
                    GameObjectTypes.SelectedItem = value.GameObjectType;
                    GameObjectGroups.SelectedItem = value.Group;
                    _selectedObject = _gameObjectService.GetObjects((GameObjectType)GameObjectTypes.SelectedItem, (string)GameObjectGroups.SelectedItem).Where(o => o.GameObject == value).FirstOrDefault();
                    UpdateSelectedObject();
                }
            }
        }

        private Dictionary<GameObjectType, string> _selectedGroup;

        private void GameObjectType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GameObjectGroups.ItemsSource = _gameObjectService.GetGroups((GameObjectType)GameObjectTypes.SelectedItem);
            GameObjectGroups.SelectedItem = _selectedGroup[(GameObjectType)GameObjectTypes.SelectedItem];

            if (GameObjectGroups.SelectedItem == null)
            {
                GameObjectGroups.SelectedIndex = 0;
            }

            _selectedObject = null;
            Update();
            UpdateSelectedObject();
        }

        private void GameObjectGroups_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GameObjectGroups.SelectedItem != null)
            {
                _selectedGroup[(GameObjectType)GameObjectTypes.SelectedItem] = (string)GameObjectGroups.SelectedItem;

                _selectedObject = null;
                Update();
                UpdateSelectedObject();
            }
        }

        private void UpdateSelectedObject()
        {
            if (_selectedObject == null)
            {
                SelectionRectangle.Visibility = Visibility.Collapsed;
                GameObjectName.Text = "None selected";
            }
            else
            {
                var selectedObjectRect = _selectedObject.BoundRectangle;
                Canvas.SetTop(SelectionRectangle, selectedObjectRect.Top);
                Canvas.SetLeft(SelectionRectangle, selectedObjectRect.Left);
                SelectionRectangle.Width = selectedObjectRect.Width + 1;
                SelectionRectangle.Height = selectedObjectRect.Height + 1;
                SelectionRectangle.Visibility = Visibility.Visible;

                GameObjectName.Text = _selectedObject.GameObject.Name;
            }
        }

        private void GameObjectImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (GameObjectTypes.SelectedItem != null && GameObjectGroups.SelectedItem != null)
            {
                var position = e.GetPosition(GameObjectImage);
                LevelObject newObject = _gameObjectService.GetObjects((GameObjectType)GameObjectTypes.SelectedItem, (string)GameObjectGroups.SelectedItem).Where(o => o.BoundRectangle.Contains(position.X, position.Y)).FirstOrDefault();

                if (newObject != null)
                {
                    _selectedObject = newObject;
                    UpdateSelectedObject();
                    if (GameObjectChanged != null)
                    {
                        GameObjectChanged(_selectedObject.GameObject);
                    }
                }

                if (e.ClickCount > 1)
                {
                    if (GameObjectDoubleClicked != null)
                    {
                        GameObjectDoubleClicked(_selectedObject.GameObject);
                    }
                }
            }
        }
    }
}