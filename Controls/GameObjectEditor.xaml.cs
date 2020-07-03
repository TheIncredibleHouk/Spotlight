﻿using Newtonsoft.Json;
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
    /// Interaction logic for GameObjectEditor.xaml
    /// </summary>
    public partial class GameObjectEditor : UserControl
    {
        public GameObjectEditor()
        {
            InitializeComponent();
        }

        private ProjectService _projectService;
        private GraphicsAccessor _graphicsAccessor;
        private GraphicsService _graphicsService;
        private GameObjectService _gameObjectService;
        private GameObjectRenderer _renderer;
        private LevelObject viewObject = new LevelObject() { X = 8, Y = 7, Property = -1 };
        private List<LevelObject> viewObjects = new List<LevelObject>();
        private WriteableBitmap _bitmap;

        public GameObjectEditor(ProjectService projectService, GraphicsService graphicsService, GameObjectService gameObjectService)
        {
            InitializeComponent();

            _projectService = projectService;
            _gameObjectService = gameObjectService;
            _graphicsService = graphicsService;
            _graphicsAccessor = new GraphicsAccessor(graphicsService.GetGlobalTiles(), graphicsService.GetExtraTiles());
            viewObjects.Add(viewObject);


            _bitmap = new WriteableBitmap(256, 256, 96, 96, PixelFormats.Bgra32, null);
            _renderer = new GameObjectRenderer(_gameObjectService, _graphicsAccessor);
            _renderer.RenderGrid = true;

            GameObjectRenderer.Source = _bitmap;

            List<Palette> palettes = graphicsService.GetPalettes();

            ObjectSelector.Initialize(_gameObjectService, _graphicsAccessor, palettes[0]);

            PaletteSelector.ItemsSource = palettes;
            PaletteSelector.SelectedIndex = 0;
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
            Palette selectedPalette = _graphicsService.GetPalette(((Palette)PaletteSelector.SelectedItem).Id);
            _renderer.Update(selectedPalette);
            ObjectSelector.Update(selectedPalette);
            Update();
        }

        private GameObject _gameObject;
        private void ObjectSelector_GameObjectChanged(GameObject gameObject)
        {
            List<string> properties = gameObject.Properties.ToList();

            _gameObject = viewObject.GameObject = gameObject;
            ObjectDefinition.Text = JsonConvert.SerializeObject(gameObject, Formatting.Indented);
            Properties.ItemsSource = properties;
            Properties.SelectedIndex = 0;
            Update();
        }

        private void Update()
        {
            Int32Rect sourceArea = new Int32Rect(0, 0, 256, 256);
            _renderer.Clear();
            if (viewObject.GameObject != null)
            {
                _renderer.Update(viewObjects, _showOverlays);
            }

            _bitmap.Lock();
            _bitmap.WritePixels(sourceArea, _renderer.GetRectangle(sourceArea), sourceArea.Width * 4, sourceArea.X, sourceArea.Y);
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
                MessageBox.Show("Object data saved.");
                
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
    }
}

