using Spotlight.Models;
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
    /// Interaction logic for WorldPanel.xaml
    /// </summary>
    public partial class FilePanel : UserControl
    {
        public delegate void LevelOpenEventHandler(LevelInfo levelInfo);
        public event LevelOpenEventHandler LevelOpened;

        public delegate void WorldOpenEventHandler(WorldInfo worldInfo);
        public event WorldOpenEventHandler WorldOpened;
        public FilePanel()
        {
            InitializeComponent();
            Expanded = false;
            UpdateCollapsedState();
        }

        public void BuildTree(List<WorldInfo> worldInfos)
        {
            WorldTree.Items.Clear();
            Expanded = true;

            UpdateCollapsedState();

            foreach (var worldInfo in worldInfos)
            {
                WorldTree.Items.Add(BuildTreeViewItem(worldInfo));
            }
        }

        private TreeViewItem BuildTreeViewItem(WorldInfo worldInfo)
        {
            TreeViewItem treeViewItem = new TreeViewItem() { Header = worldInfo.Name, DataContext = worldInfo };
            foreach (var levelInfo in worldInfo.LevelsInfo ?? new List<LevelInfo>())
            {
                treeViewItem.Items.Add(BuildTreeViewItem(levelInfo));
            }
            return treeViewItem;
        }

        private TreeViewItem BuildTreeViewItem(LevelInfo levelInfo)
        {
            TreeViewItem treeViewItem = new TreeViewItem() { Header = levelInfo.Name, DataContext = levelInfo };

            if (levelInfo.SublevelsInfo != null)
            {
                foreach (LevelInfo subLevelInfo in levelInfo.SublevelsInfo)
                {
                    treeViewItem.Items.Add(BuildTreeViewItem(subLevelInfo));
                }
            }

            return treeViewItem;
        }

        public bool Expanded { get; set; }

        private void Expander_Click(object sender, RoutedEventArgs e)
        {
            if (WorldTree.Items == null || WorldTree.Items.Count == 0)
            {
                return;
            }

            Expanded = !Expanded;
            UpdateCollapsedState();
        }

        public void UpdateCollapsedState()
        {
            if (Expanded)
            {
                Width = 300;
                WorldTree.Visibility = ActionPanel.Visibility = Visibility.Visible;
                CollapseIcon.RenderTransform = null;
            }
            else
            {
                Width = 45;
                WorldTree.Visibility = ActionPanel.Visibility = Visibility.Collapsed;
                CollapseIcon.RenderTransform = new RotateTransform(180, 5, 5);
            }
        }

        private void Open_Clicked(object sender, RoutedEventArgs e)
        {
            if (WorldTree.SelectedItem != null)
            {
                object dataContext = ((TreeViewItem)WorldTree.SelectedItem).DataContext;

                if (dataContext is LevelInfo)
                {
                    LevelOpened((LevelInfo)dataContext);
                    Expanded = false;
                    UpdateCollapsedState();
                }
                else if(dataContext is WorldInfo)
                {
                    WorldOpened((WorldInfo)dataContext);
                    Expanded = false;
                    UpdateCollapsedState();
                }
            }
        }
    }
}
