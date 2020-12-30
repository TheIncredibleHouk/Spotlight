using Spotlight.Models;
using Spotlight.Services;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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

        public delegate void NameUpdatedHandler(NameUpdate nameUpdate);
        public event NameUpdatedHandler NameUpdated;

        public FilePanel()
        {
            InitializeComponent();
            
        }

        private WorldService _worldService;
        private LevelService _levelService;
        public void Initialize(LevelService levelService, WorldService worldService)
        {
            _levelService = levelService;
            _worldService = worldService;
            _levelService.LevelsUpdated += _levelService_LevelsUpdated;
            BuildTree();
        }

        private void _levelService_LevelsUpdated()
        {
            BuildTree();
        }

        public void BuildTree()
        {
            WorldTree.Items.Clear();
            Expanded = true;

            UpdateCollapsedState();

            foreach (var worldInfo in _worldService.AllWorlds())
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
                else if (dataContext is WorldInfo)
                {
                    WorldOpened((WorldInfo)dataContext);
                    Expanded = false;
                    UpdateCollapsedState();
                }
            }
        }

        private void Rename_Click(object sender, RoutedEventArgs e)
        {
            if (WorldTree.SelectedItem != null)
            {
                object dataContext = ((TreeViewItem)WorldTree.SelectedItem).DataContext;

                if (dataContext is LevelInfo)
                {
                    LevelInfo levelInfo = dataContext as LevelInfo;
                    string previousName = levelInfo.Name;

                    string newName = InputWindow.GetInput("Rename level", levelInfo.Name);
                    if (newName != null)
                    {

                        LevelInfo existingInfo = _levelService.AllLevels().Where(l => l.Name.Equals(newName, System.StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                        if (existingInfo == null)
                        {
                            _levelService.RenameLevel(levelInfo.Name, newName);
                            levelInfo.Name = newName;
                            
                            _levelService.NotifyUpdate(levelInfo);
                            NameUpdated(new NameUpdate((IInfo)levelInfo, previousName, levelInfo.Name));
                            ((TreeViewItem)WorldTree.SelectedItem).Header = levelInfo.Name;
                        }
                        else
                        {
                            AlertWindow.Alert(newName + " already exists!");
                        }
                    }
                }
                else if (dataContext is WorldInfo)
                {
                    WorldInfo worldInfo = dataContext as WorldInfo;
                    string previousName = worldInfo.Name;

                    string newName = InputWindow.GetInput("Rename map", worldInfo.Name);
                    if(newName != null)
                    {
                        WorldInfo existingInfo = _worldService.AllWorlds().Where(w => w.Name.Equals(newName, System.StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                        if (existingInfo == null)
                        {
                            _worldService.RenameWorld(worldInfo.Name, newName);
                            worldInfo.Name = newName;
                            
                            NameUpdated(new NameUpdate((IInfo)worldInfo, previousName, worldInfo.Name));
                            ((TreeViewItem)WorldTree.SelectedItem).Header = worldInfo.Name;
                        }
                        else
                        {
                            AlertWindow.Alert(newName + " already exists!");
                        }
                    }
                }
            }
        }
    }

    public class NameUpdate
    {
        public IInfo Info { get; set; }
        public string PreviousName { get; set; }
        public string NewName { get; set; }

        public NameUpdate(IInfo info, string previousName, string newName)
        {
            Info = info;
            PreviousName = previousName;
            NewName = newName;
        }
    }
}