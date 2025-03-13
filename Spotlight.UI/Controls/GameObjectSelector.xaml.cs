using Spotlight.Abstractions;
using Spotlight.Models;
using Spotlight.Renderers;
using Spotlight.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Spotlight
{

    public enum GameObjectGroup
    {
        Level,
        World,
        All
    }

    /// <summary>
    /// Interaction logic for GameObjectSelector.xaml
    /// </summary>
    public partial class GameObjectSelector : UserControl
    {
        public delegate void GameObjectSelectorEventHandler(GameObject gameObject);

        public event GameObjectSelectorEventHandler GameObjectChanged;

        public event GameObjectSelectorEventHandler GameObjectDoubleClicked;

        public GameObjectGroup ObjectGroup { get; set; }

        public GameObjectSelector()
        {
            InitializeComponent();
        }

        private IGameObjectService _gameObjectService;
        private IGraphicsManager _graphicsAccessor;
        private IPaletteService _palettesService;
        private IEventService _eventService;
        private Palette _palette;
        private GameObjectRenderer _renderer;
        private WriteableBitmap _bitmap;
        private List<GameObjectType> _objectTypes;

        private Guid _gameObjectsServiceSubId;

        public void Initialize(IGameObjectService gameObjectService, IPaletteService palettesService, IEventService eventService, IGraphicsManager graphicsManager, Palette palette)
        {
            _gameObjectService = gameObjectService;
            _eventService = eventService;
            _graphicsAccessor = graphicsManager;
            _palette = palette;
            _palettesService = palettesService;

            _objectTypes = new List<GameObjectType>();
            _selectedGroup = new Dictionary<GameObjectType, string>();

            _renderer = new GameObjectRenderer(gameObjectService, _palettesService, graphicsManager);

            Dpi dpi = this.GetDpi();
            _bitmap = new WriteableBitmap(256, 256, dpi.X, dpi.Y, PixelFormats.Bgra32, null);

            

            switch (ObjectGroup)
            {
                case GameObjectGroup.Level:
                    _objectTypes.Add(GameObjectType.Global);
                    _objectTypes.Add(GameObjectType.TypeA);
                    _objectTypes.Add(GameObjectType.TypeB);
                    
                    break;

                case GameObjectGroup.World:
                    _objectTypes.Add(GameObjectType.World);
                    break;

                case GameObjectGroup.All:
                    _objectTypes.Add(GameObjectType.Global);
                    _objectTypes.Add(GameObjectType.TypeA);
                    _objectTypes.Add(GameObjectType.TypeB);
                    _objectTypes.Add(GameObjectType.World);
                    break;
            }

            foreach(var objectType in _objectTypes)
            {
                _selectedGroup[objectType] = null;
            }
            

            _selectedObject = null;
            _gameObjectsServiceSubId = _eventService.Subscribe(SpotlightEventType.GameObjectsUpdated, GameObjectsUpdated);
        }

        private void InitializeUI()
        {

            GameObjectImage.Source = _bitmap;
            GameObjectTypes.ItemsSource = _objectTypes;

            CanvasArea.Background = new SolidColorBrush(_palette.RgbColors[0][0].ToMediaColor());
            GameObjectTypes.SelectedIndex = 0;

            _renderer.Update(_palette);

        }

        public void DetachEvents()
        {
            _eventService.Unsubscribe(_gameObjectsServiceSubId);
        }

        private void GameObjectsUpdated()
        {
            UpdateSelectedObject();
            Update();
        }

        public void Update(Palette palette)
        {
            _palette = palette;
            _renderer.Update(palette);
            CanvasArea.Background = new SolidColorBrush(palette.RgbColors[0][0].ToMediaColor());
            Update();
        }

        public void Update()
        {
            Int32Rect sourceArea = new Int32Rect(0, 0, 256, 256);
            _renderer.Clear();
            _renderer.Update(_gameObjectService.GetObjectsByGroup((GameObjectType)GameObjectTypes.SelectedItem, (string)GameObjectGroups.SelectedItem), false);

            _bitmap.Lock();
            _bitmap.WritePixels(sourceArea, _renderer.GetRectangle(sourceArea.AsRectangle()), sourceArea.Width * 4, sourceArea.X, sourceArea.Y);
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
                    _selectedObject = _gameObjectService.GetObjectsByGroup((GameObjectType)GameObjectTypes.SelectedItem, (string)GameObjectGroups.SelectedItem).Where(o => o.GameObject.GameId == value.GameId).FirstOrDefault();
                    UpdateSelectedObject();
                }
            }
        }

        private Dictionary<GameObjectType, string> _selectedGroup;

        private void GameObjectType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateGameObjectUI();
            
        }

        private void UpdateGameObjectUI()
        {
            GameObjectGroups.ItemsSource = _gameObjectService.GetObjectGroups((GameObjectType)GameObjectTypes.SelectedItem).OrderBy(group => group);
            GameObjectGroups.SelectedItem = _selectedGroup[(GameObjectType)GameObjectTypes.SelectedItem];

            if (GameObjectGroups.SelectedItem == null)
            {
                GameObjectGroups.SelectedIndex = 0;
            }

            _selectedObject = null;
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
                LevelObject newObject = _gameObjectService.GetObjectsByGroup((GameObjectType)GameObjectTypes.SelectedItem, (string)GameObjectGroups.SelectedItem).Where(o => o.BoundRectangle.Contains((int) position.X, (int) position.Y)).FirstOrDefault();

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
                    GlobalPanels.EditGameObject(_selectedObject.GameObject, _palette);
                    //if (GameObjectDoubleClicked != null && _selectedObject != null)
                    //{
                    //    GameObjectDoubleClicked(_selectedObject.GameObject);
                    //}
                }
            }
        }
    }
}