using Microsoft.Win32;
using Spotlight.Models;
using Spotlight.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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


        public delegate void ObjectEditorEventHandler(GameObject gameObject, Palette palette);
        public event ObjectEditorEventHandler ObjectEditorOpened;

        public ProjectService ProjectService { get; set; }

        public ProjectPanel()
        {
            InitializeComponent();
        }

        private void LoadProject(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                Project project = ProjectService.LoadProject(openFileDialog.FileName);
                ProjectLoaded(project);
                ObjectButton.IsEnabled = NewWorldButton.IsEnabled = NewLevelButton.IsEnabled = SaveProjectButton.IsEnabled = SaveRomButton.IsEnabled = PaletteButton.IsEnabled = TileSetButton.IsEnabled = TextEditButton.IsEnabled = true ;
            }
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

        }

        private void SaveRomButton_Click(object sender, RoutedEventArgs e)
        {
            GlobalPanels.OpenPaletteEditor();
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
    }
}
