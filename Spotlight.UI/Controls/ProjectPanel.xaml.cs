using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using Spotlight.Abstractions;
using Spotlight.Models;
using Spotlight.Services;
using System;
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
        private IWorldService _worldService;
        private ILevelService _levelService;

        public ProjectPanel()
        {
            InitializeComponent();
        }

        public void Initialize(IServiceProvider serviceProvider)
        {
            _eventService = serviceProvider.GetService<IEventService>();
            _projectService = serviceProvider.GetService<IProjectService>();
            _worldService = serviceProvider.GetService<IWorldService>();
            _levelService = serviceProvider.GetService<ILevelService>();
        }

        public void Configure(IServiceProvider services)
        {
            _eventService = services.GetService<IEventService>();
            _projectService = services.GetService<IProjectService>();
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
            ExportPaletteButton.IsEnabled = MusicEditButton.IsEnabled = ObjectButton.IsEnabled = NewWorldButton.IsEnabled = NewLevelButton.IsEnabled = SaveRomButton.IsEnabled = PaletteButton.IsEnabled = TileSetButton.IsEnabled = TextEditButton.IsEnabled = true;
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
            NewLevelResult newLevelResult = NewLevelWindow.Show(_levelService, _worldService);
            if (newLevelResult != null)
            {
                _levelService.AddLevel(newLevelResult.Level, newLevelResult.WorldInfo);
            }
            _projectService.SaveProject();
        }

        private void SaveRomButton_Click(object sender, RoutedEventArgs e)
        {
            if (RomSaved != null)
            {
                RomSaved();
            }
        }

        private void TextEditButton_Click(object sender, RoutedEventArgs e)
        {
            TextEditorOpened();
        }

        private void ObjectButton_Click(object sender, RoutedEventArgs e)
        {
            ObjectEditorOpened(null, null);
        }

        private void TileSetButton_Click(object sender, RoutedEventArgs e)
        {
            TileBlockEditorOpened();
        }

        private void PaletteButton_Click(object sender, RoutedEventArgs e)
        {
            GlobalPanels.OpenPaletteEditor();
        }

        private void GraphicsEditButton_Click(object sender, RoutedEventArgs e)
        {
            GraphicsEditorClicked();
        }

        private void ExportPaletteButton_Click(object sender, RoutedEventArgs e)
        {
            ExportPaletteClicked();
        }

        private void UpdateMetaDataButton_Click(object sender, RoutedEventArgs e)
        {
            GenerateMetaDataClicked();
        }
    }
}