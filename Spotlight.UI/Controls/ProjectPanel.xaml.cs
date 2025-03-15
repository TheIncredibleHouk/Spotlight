using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using Spotlight.Abstractions;
using Spotlight.Models;
using Spotlight.Services;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Spotlight
{
    /// <summary>
    /// Interaction logic for ProjectPanel.xaml
    /// </summary>
    public partial class ProjectPanel : UserControl
    {
        private IEventService _eventService;
        private IProjectService _projectService;
        private IPaletteService _paletteService;
        private IWorldService _worldService;
        private ILevelService _levelService;
        private IConfigurationService _configurationService;

        public ProjectPanel()
        {
            InitializeComponent();
            InitializeServices();
        }

        private void InitializeServices()
        {
            if (DesignerProperties.GetIsInDesignMode(this)) return;

            _eventService = App.Services.GetService<IEventService>();
            _projectService = App.Services.GetService<IProjectService>();
            _worldService = App.Services.GetService<IWorldService>();
            _levelService = App.Services.GetService<ILevelService>();
            _configurationService = App.Services.GetService<IConfigurationService>();
            _paletteService = App.Services.GetService<IPaletteService>();
        }


        private void LoadProject(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                LoadProject(openFileDialog.FileName);
            }
        }

        public void LoadProject(string filePath)
        {
            _projectService.LoadProject(filePath);
            //MusicEditButton.IsEnabled
            ExportPaletteButton.IsEnabled = ObjectButton.IsEnabled = NewWorldButton.IsEnabled = NewLevelButton.IsEnabled = SaveRomButton.IsEnabled = PaletteButton.IsEnabled = TileSetButton.IsEnabled = TextEditButton.IsEnabled = true;
        }

        private void SaveProjectButton_Click(object sender, RoutedEventArgs e)
        {
            _projectService.SaveProject();
            AlertWindow.Alert("Project saved.");
        }

        private void NewWorldButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void NewLevelButton_Click(object sender, RoutedEventArgs e)
        {
            NewLevelResult newLevelResult = NewLevelWindow.Show();
            if (newLevelResult != null)
            {
                _levelService.AddLevel(newLevelResult.Level, newLevelResult.WorldInfo);
            }
            _projectService.SaveProject();
        }

        private void SaveRomButton_Click(object sender, RoutedEventArgs e)
        {
            _eventService.Emit(SpotlightEventType.RomSaved);
        }

        private void TextEditButton_Click(object sender, RoutedEventArgs e)
        {
            _eventService.Emit(SpotlightEventType.UIOpenTextEditor);
        }

        private void ObjectButton_Click(object sender, RoutedEventArgs e)
        {
            _eventService.Emit(SpotlightEventType.UIOpenGameObjectEditor);
        }

        private void TileSetButton_Click(object sender, RoutedEventArgs e)
        {
            _eventService.Emit(SpotlightEventType.UIOpenBlockEditor);
        }

        private void PaletteButton_Click(object sender, RoutedEventArgs e)
        {
            _eventService.Emit(SpotlightEventType.UIOpenPaletteEditor);
        }

        private void GraphicsEditButton_Click(object sender, RoutedEventArgs e)
        {
            _eventService.Emit(SpotlightEventType.UIOpenGraphicsEditor);
        }

        private void ExportPaletteButton_Click(object sender, RoutedEventArgs e)
        {
            Project project = _projectService.GetProject();
            Configuration config = _configurationService.GetConfiguration();

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.DefaultExt = ".pal";
            saveFileDialog.FileName = $"{project.Name}.pal";
            saveFileDialog.InitialDirectory = config.LastProjectPath;

            if (saveFileDialog.ShowDialog() == true)
            {
                _paletteService.ExportRgbPalette(saveFileDialog.FileName);
            }
        }

        private void UpdateMetaDataButton_Click(object sender, RoutedEventArgs e)
        {
            _eventService.Emit(SpotlightEventType.GenerateMetaData);
        }
    }
}