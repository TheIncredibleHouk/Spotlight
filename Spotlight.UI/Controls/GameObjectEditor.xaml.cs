using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Spotlight.Abstractions;
using Spotlight.Models;
using Spotlight.Renderers;
using Spotlight.Services;
using Spotlight.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Spotlight
{
    /// <summary>
    /// Interaction logic for GameObjectEditor.xaml
    /// </summary>
    public partial class GameObjectEditor : UserControl, IUnsubscribe
    {
        private IProjectService _projectService;
        private IGraphicsManager _graphicsManager;
        private IGraphicsService _graphicsService;
        private IGameObjectService _gameObjectService;
        private IPaletteService _palettesService;
        private IGameObjectRenderer _gameObjectRenderer;
        private IEventService _eventService;

        private LevelObject viewObject = new LevelObject() { X = 8, Y = 7, Property = -1 };
        private List<LevelObject> viewObjects = new List<LevelObject>();
        private WriteableBitmap _bitmap;

        private List<Guid> _subscriptionIds = new List<Guid>();
        public GameObjectEditor()
        {
            InitializeComponent();

            _projectService = App.Services.GetService<IProjectService>();
            _gameObjectService = App.Services.GetService<IGameObjectService>();
            _graphicsService = App.Services.GetService<IGraphicsService>();
            _palettesService = App.Services.GetService<IPaletteService>();
            _graphicsManager = App.Services.GetService<IGraphicsManager>();
            _eventService = App.Services.GetService<IEventService>();
            
            GameObjectRenderer.Source = _bitmap;

            _subscriptionIds.Add(_eventService.Subscribe(SpotlightEventType.GraphicsUpdated, GraphicsUpdated));
            _subscriptionIds.Add(_eventService.Subscribe(SpotlightEventType.ExtraGraphicsUpdated, GraphicsUpdated));
        }

        private void Initialize()
        {
            _graphicsManager.Initialize(_graphicsService.GetGlobalTiles(), _graphicsService.GetExtraTiles());
            _gameObjectRenderer = new GameObjectRenderer(_gameObjectService, _palettesService, _graphicsManager);
            _gameObjectRenderer.RenderGrid = true;
            viewObjects.Add(viewObject);

        }

        private void InitializeUI()
        {
            Dpi dpi = this.GetDpi();
            _bitmap = new WriteableBitmap(256, 256, dpi.X, dpi.Y, PixelFormats.Bgra32, null);
            ObjectSelector.Initialize(_gameObjectService, _palettesService, App.Services.GetService<IEventService>(), _graphicsManager, _palettesService.GetPalettes()[0]);

            PaletteSelector.ItemsSource = _palettesService.GetPalettes();
            PaletteSelector.SelectedIndex = 0;
        }
       

        private void GraphicsUpdated()
        {
            _graphicsManager.SetGlobalTiles(_graphicsService.GetGlobalTiles(), _graphicsService.GetExtraTiles());
            ObjectSelector.Update();
            Update();
        }

        public void SelectObject(GameObject gameObject, Palette palette)
        {
            if (gameObject != null)
            {
                ObjectSelector.SelectedObject = gameObject;
                PaletteSelector.SelectedItem = palette ?? PaletteSelector.Items[0];
                ObjectSelector_GameObjectChanged(gameObject);
            }
        }

        private void PaletteSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Palette selectedPalette = _palettesService.GetPalette(((Palette)PaletteSelector.SelectedItem).Id);
            _gameObjectRenderer.Update(selectedPalette);
            ObjectSelector.Update(selectedPalette);
            Update();
        }

        private GameObject _gameObject;

        private void ObjectSelector_GameObjectChanged(GameObject gameObject)
        {
            List<string> properties = (gameObject.Properties ?? new List<string>()).ToList();

            _gameObject = viewObject.GameObject = gameObject;
            ObjectDefinition.Text = JsonConvert.SerializeObject(gameObject, Formatting.Indented);
            Properties.ItemsSource = properties;
            Properties.SelectedIndex = 0;
            Update();
        }

        private void Update()
        {
            Int32Rect sourceArea = new Int32Rect(0, 0, 256, 256);
            _gameObjectRenderer.Clear();
            if (viewObject.GameObject != null)
            {
                _gameObjectRenderer.Update(viewObjects, _showOverlays);
            }

            _bitmap.Lock();
            _bitmap.WritePixels(sourceArea, _gameObjectRenderer.GetRectangle(sourceArea.AsRectangle()), sourceArea.Width * 4, sourceArea.X, sourceArea.Y);
            _bitmap.AddDirtyRect(sourceArea);
            _bitmap.Unlock();
        }

        private void ObjectDefinition_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                GameObject gameObject = JsonConvert.DeserializeObject<GameObject>(ObjectDefinition.Text);
                viewObject.GameObject = gameObject;
                Update();
                ObjectDefinition.Foreground = new SolidColorBrush(Colors.White);
                SaveButton.IsEnabled = true;
            }
            catch
            {
                ObjectDefinition.Foreground = new SolidColorBrush(Colors.Red);
                SaveButton.IsEnabled = false;
            }
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            viewObject.GameObject = _gameObject;
            ObjectDefinition.Text = JsonConvert.SerializeObject(_gameObject, Formatting.Indented);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (viewObject.GameObject != null)
            {
                _gameObjectService.UpdateGameTable(viewObject.GameObject);
                _gameObjectService.CommitGameObject(viewObject.GameObject);
                _projectService.SaveProject();
            }
        }

        private bool _showOverlays;

        private void ShowOverlays_Checked(object sender, RoutedEventArgs e)
        {
            _showOverlays = ((CheckBox)sender).IsChecked.Value;
            Update();
        }

        private void Properties_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            viewObject.Property = Properties.SelectedIndex;
            Update();
        }

        public void Unsubscribe()
        {
            _eventService.Unsubscribe(_subscriptionIds);
        }

    }
}