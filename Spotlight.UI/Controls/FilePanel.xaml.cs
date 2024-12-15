using Spotlight.Models;
using Spotlight.Services;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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

        public delegate void LevelTreeUpdatedHandler();
        public event LevelTreeUpdatedHandler LevelTreeUpdated;

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
            _levelService.LevelUpdated += _levelService_LevelUpdated;
            _levelService.LevelsUpdated += _levelService_LevelsUpdated;

            Dpi dpi = this.GetDpi();
            _thumbnailBitmap = new WriteableBitmap(256, 256, dpi.X, dpi.Y, PixelFormats.Bgra32, null);
            ThumbnailPreview.Source = _thumbnailBitmap;
            BuildTree();
        }

        private void _levelService_LevelUpdated(LevelInfo levelInfo)
        {
            BuildTree(levelInfo);
        }

        private void _levelService_LevelsUpdated(LevelInfo levelInfo)
        {
            BuildTree(levelInfo);
        }

        public void BuildTree(LevelInfo defaultSelection = null)
        {
            WorldTree.Items.Clear();

            foreach (var worldInfo in _worldService.AllWorlds())
            {
                WorldTree.Items.Add(BuildTreeViewItem(worldInfo));
            }

            if (defaultSelection != null)
            {
                SetSelectedItem(defaultSelection);
            }
        }

        private TreeViewItem BuildTreeViewItem(WorldInfo worldInfo)
        {
            TreeViewItem treeViewItem = new TreeViewItem()
            {
                Header = worldInfo.Name,
                DataContext = worldInfo
            };
            foreach (var levelInfo in worldInfo.LevelsInfo ?? new List<LevelInfo>())
            {
                treeViewItem.Items.Add(BuildTreeViewItem(levelInfo));
            }
            return treeViewItem;
        }

        private TreeViewItem BuildTreeViewItem(LevelInfo levelInfo)
        {
            TreeViewItem treeViewItem = new TreeViewItem()
            {
                Header = levelInfo.Name + $" ({(levelInfo.Size / 1024.0).ToString("#.##")} kb)",
                DataContext = levelInfo
            };

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
                LevelMetaDisplay.Visibility = WorldTree.Visibility = ActionPanel.Visibility = Visibility.Visible;
                CollapseIcon.RenderTransform = null;
            }
            else
            {
                Width = 45;
                LevelMetaDisplay.Visibility = WorldTree.Visibility = ActionPanel.Visibility = Visibility.Collapsed;
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
                    if (newName != null)
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

        private void Move_Click(object sender, RoutedEventArgs e)
        {
            TreeViewItem parentWorldItem = (TreeViewItem)((TreeViewItem)WorldTree.SelectedItem).Parent;
            TreeViewItem parentLevelItem = (TreeViewItem)((TreeViewItem)WorldTree.SelectedItem).Parent;

            LevelInfo levelInfo = (LevelInfo)((TreeViewItem)WorldTree.SelectedItem).DataContext;
            IInfo parentInfo = (IInfo)parentLevelItem.DataContext;
            LevelInfo parentLevelInfo = parentLevelItem.DataContext is LevelInfo ? (LevelInfo)parentLevelItem.DataContext : null;

            while (parentWorldItem.Parent is TreeViewItem)
            {
                parentWorldItem = (TreeViewItem)parentWorldItem.Parent;
            }

            WorldInfo hostWorldInfo = (WorldInfo)parentWorldItem.DataContext;

            MoveLevelResult result = MoveLevelWindow.Show(_levelService, _worldService, hostWorldInfo, parentLevelInfo);

            if (result != null)
            {
                parentInfo.SublevelsInfo.Remove(levelInfo);
                if (result.InfoNode.SublevelsInfo == null)
                {
                    result.InfoNode.SublevelsInfo = new List<LevelInfo>();
                }

                result.InfoNode.SublevelsInfo.Add(levelInfo);

                BuildTree();
                LevelTreeUpdated();
                SetSelectedItem(levelInfo);
            }
        }

        private WriteableBitmap _thumbnailBitmap;
        private void WorldTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            MoveButton.IsEnabled = WorldTree.SelectedItem != null && ((TreeViewItem)WorldTree.SelectedItem).DataContext is LevelInfo;
            if (((TreeViewItem)WorldTree.SelectedItem)?.DataContext is LevelInfo levelInfo)
            {
                if (levelInfo.LevelMetaData != null)
                {
                    MetaCoins.Text = levelInfo.LevelMetaData?.MaxCoinCount.ToString() ?? "unknown";
                    MetaCherries.Text = levelInfo.LevelMetaData?.MaxCherryCount.ToString() ?? "unknown";
                    MetaPowerUps.Text = string.Join(", ", levelInfo.LevelMetaData?.PowerUps ?? new List<string>());

                    if (levelInfo.LevelMetaData.ThumbnailImage != null)
                    {
                        Int32Rect thumbnailRect = new Int32Rect(0, 0, 256, 256);

                        _thumbnailBitmap.Lock();
                        _thumbnailBitmap.WritePixels(thumbnailRect, levelInfo.LevelMetaData.ThumbnailImage, 256 * 4, 0, 0);
                        _thumbnailBitmap.AddDirtyRect(thumbnailRect);
                        _thumbnailBitmap.Unlock();
                    }
                }
            } else if(((TreeViewItem) WorldTree.SelectedItem)?.DataContext is WorldInfo worldInfo)
            {
                if (worldInfo.MetaData != null)
                {
                    if (worldInfo.MetaData.ThumbnailImage != null)
                    {
                        Int32Rect thumbnailRect = new Int32Rect(0, 0, 256, 256);

                        _thumbnailBitmap.Lock();
                        _thumbnailBitmap.WritePixels(thumbnailRect, worldInfo.MetaData.ThumbnailImage, 256 * 4, 0, 0);
                        _thumbnailBitmap.AddDirtyRect(thumbnailRect);
                        _thumbnailBitmap.Unlock();
                    }
                }
            }
        }

        private void SetSelectedItem(IInfo info)
        {
            SelectItem(WorldTree.Items, info);
        }

        private bool SelectItem(ItemCollection treeViewItems, IInfo info)
        {
            foreach (TreeViewItem item in treeViewItems)
            {
                if (item.DataContext == info)
                {
                    item.IsSelected = true;
                    return true;
                }

                if (item.IsExpanded = SelectItem(item.Items, info))
                {
                    return true; ;
                }
            }

            return false;
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            IInfo iInfo = (IInfo)((TreeViewItem)WorldTree.SelectedItem).DataContext;

            if (ConfirmationWindow.Confirm($"Are you sure you want to remove the level {iInfo.Name}?") == System.Windows.Forms.DialogResult.Yes)
            {
                if (iInfo is LevelInfo)
                {
                    _levelService.RemoveLevel((LevelInfo)iInfo);
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