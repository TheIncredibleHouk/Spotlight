using Microsoft.Win32;
using Spotlight.Models;
using Spotlight.Services;
using System.Windows;
using System.Windows.Controls;

namespace Spotlight
{
    /// <summary>
    /// Interaction logic for ProjectPanel.xaml
    /// </summary>
    public partial class ProjectPanel : UserControl
    {
        public delegate void ProjectLoadEventHandler(Project project);

        public event ProjectLoadEventHandler ProjectLoaded;

        public delegate void TileBlockEditorOpenEventHandler();

        public event TileBlockEditorOpenEventHandler TileBlockEditorOpened;

        public delegate void TextEditorOpenEventHandler();

        public event TextEditorOpenEventHandler TextEditorOpened;

        public delegate void RomSavedEventHandler();

        public event RomSavedEventHandler RomSaved;

        public delegate void ObjectEditorEventHandler(GameObject gameObject, Palette palette);

        public event ObjectEditorEventHandler ObjectEditorOpened;

        public delegate void NewLevelEventHandler();

        public event NewLevelEventHandler NewLevelClicked;
        
        public delegate void GraphicsEditorEventHandlder();
        public event GraphicsEditorEventHandlder GraphicsEditorClicked;

        public delegate void ExportPaletteEventHandler();
        public event ExportPaletteEventHandler ExportPaletteClicked;

        public delegate void GenerateMetaDataEventHandler();
        public event GenerateMetaDataEventHandler GenerateMetaDataClicked;

        public ProjectService ProjectService { get; set; }
        public RomService RomService { get; set; }

        public ProjectPanel()
        {
            InitializeComponent();
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
            Project project = ProjectService.LoadProject(filePath);
            ProjectLoaded(project);
            ExportPaletteButton.IsEnabled = GraphicsEditButton.IsEnabled = ObjectButton.IsEnabled = NewWorldButton.IsEnabled = NewLevelButton.IsEnabled = SaveRomButton.IsEnabled = PaletteButton.IsEnabled = TileSetButton.IsEnabled = TextEditButton.IsEnabled = true;
        }

        private void SaveProjectButton_Click(object sender, RoutedEventArgs e)
        {
            ProjectService.SaveProject();
            AlertWindow.Alert("Project saved.");
        }

        private void NewWorldButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void NewLevelButton_Click(object sender, RoutedEventArgs e)
        {
            if (NewLevelClicked != null)
            {
                NewLevelClicked();
            }
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