using Microsoft.Win32;
using Newtonsoft.Json;
using Spotlight.Models;
using Spotlight.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Spotlight
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ProjectService _projectService;
        private ErrorService _errorService;
        private GraphicsService _graphicsService;
        private WorldService _worldService;
        private LevelService _levelService;
        private PalettesService _palettesService;
        private TileService _tileService;
        private TextService _textService;
        private GameObjectService _gameObjectService;
        private RomService _romService;
        private Project _project;
        private Configuration _config;

        public MainWindow()
        {
            _errorService = new ErrorService();
            _projectService = new ProjectService(_errorService);
            _config = new Configuration();

            LoadConfiguration();

            InitializeComponent();

            _ProjectPanel.ProjectService = _projectService;
            _ProjectPanel.ProjectLoaded += _ProjectPanel_ProjectLoaded;
            _ProjectPanel.TextEditorOpened += _ProjectPanel_TextEditorOpened;
            _ProjectPanel.ObjectEditorOpened += OpenGameObjectEditor;
            _ProjectPanel.TileBlockEditorOpened += OpenTileBlockEditor;
            _ProjectPanel.RomSaved += _ProjectPanel_RomSaved;
            TabsOpen.SelectionChanged += TabsOpen_SelectionChanged;
            Activated += MainWindow_Activated;

            GlobalPanels.MainWindow = this;

            if (_config.LastProjectPath != null && File.Exists(_config.LastProjectPath))
            {
                _ProjectPanel.LoadProject(_config.LastProjectPath);
            }
        }

        private void _worldService_WorldUpdated(WorldInfo worldInfo)
        {
            var existingTab = OpenedTabs.Where(t => t.DataContext == worldInfo).FirstOrDefault();

            if (existingTab != null)
            {
                existingTab.Header = worldInfo.Name;
            }
        }

        private void _levelService_LevelUpdated(LevelInfo levelInfo)
        {
            var existingTab = OpenedTabs.Where(t => t.DataContext == levelInfo).FirstOrDefault();

            if (existingTab != null)
            {
                existingTab.Header = levelInfo.Name;
            }
        }

        private void _ProjectPanel_RomSaved()
        {
            if (_projectService.RomFileName == null || !File.Exists(_projectService.RomFileName))
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                if (openFileDialog.ShowDialog() == true)
                {
                    _projectService.RomFileName = openFileDialog.FileName;
                }
            }

            if (_projectService.RomFileName != null)
            {
                try
                {
                    _romService.CompileRom(_projectService.RomFileName);

                    if (_errorService.CurrentLog.Count > 0)
                    {
                        AlertWindow.Alert("Rom compiled but with the following errors: " + string.Join("\n-", _errorService.CurrentLog));
                    }
                    else
                    {
                        AlertWindow.Alert("Rom compiled!");
                    }
                }
                catch (Exception e)
                {
                    AlertWindow.Alert("Error occurred when attempting to compile rom. \nException:\n" + e.Message);
                }
            }
        }

        private void MainWindow_Activated(object sender, EventArgs e)
        {
            if (_graphicsService != null)
            {
                _graphicsService.CheckGraphics();
            }
        }

        private TabItem SelectedTabItem = null;

        private void TabsOpen_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectedTabItem != null && TabsOpen.SelectedItem != SelectedTabItem)
            {
                TabsOpen.SelectedItem = SelectedTabItem;
            }
        }

        public void OpenTileBlockEditor()
        {
            OpenTileBlockEditor(Guid.Empty, -1);
        }

        public void OpenTileBlockEditor(Guid levelId, int tileBlockValue)
        {
            var existingTab = OpenedTabs.Where(t => t.DataContext == _tileService).FirstOrDefault();

            if (existingTab != null)
            {
                if (levelId != Guid.Empty && tileBlockValue > -1)
                {
                    ((TileBlockEditor)existingTab.Content).SelectTileBlock(levelId, tileBlockValue);
                }

                TabsOpen.SelectedItem = SelectedTabItem = existingTab;
                return;
            }

            TabItem tabItem = new TabItem();
            TileBlockEditor tileSetEditor = new TileBlockEditor(_projectService, _worldService, _levelService, _graphicsService, _palettesService, _tileService, _textService);

            tabItem.Header = "Tile Set Editor";
            tabItem.Content = tileSetEditor;
            tabItem.DataContext = _tileService;

            TabsOpen.Items.Add(tabItem);
            OpenedTabs.Add(tabItem);
            TabsOpen.Visibility = Visibility.Visible;
            TabsOpen.SelectedItem = SelectedTabItem = tabItem;

            if (levelId != Guid.Empty && tileBlockValue > -1)
            {
                tileSetEditor.SelectTileBlock(levelId, tileBlockValue);
            }
        }

        public void OpenGameObjectEditor(GameObject gameObject, Palette palette = null)
        {
            var existingTab = OpenedTabs.Where(t => t.DataContext == _gameObjectService).FirstOrDefault();

            if (existingTab != null)
            {
                ((GameObjectEditor)existingTab.Content).SelectObject(gameObject, palette);
                TabsOpen.SelectedItem = SelectedTabItem = existingTab;
                return;
            }

            TabItem tabItem = new TabItem();
            GameObjectEditor objectEditor = new GameObjectEditor(_projectService, _palettesService, _graphicsService, _gameObjectService);

            tabItem.Header = "Object Editor";
            tabItem.Content = objectEditor;
            tabItem.DataContext = _gameObjectService;

            objectEditor.SelectObject(gameObject, palette);

            TabsOpen.Items.Add(tabItem);
            OpenedTabs.Add(tabItem);
            TabsOpen.Visibility = Visibility.Visible;
            TabsOpen.SelectedItem = SelectedTabItem = tabItem;
        }

        private void _ProjectPanel_TextEditorOpened()
        {
            var existingTab = OpenedTabs.Where(t => t.DataContext == _textService).FirstOrDefault();

            if (existingTab != null)
            {
                TabsOpen.SelectedItem = SelectedTabItem = existingTab;
                return;
            }

            TabItem tabItem = new TabItem();
            TextPanel textPanel = new TextPanel(_projectService, _textService);

            tabItem.Header = "Text Editor";
            tabItem.Content = textPanel;
            tabItem.DataContext = _textService;

            TabsOpen.Items.Add(tabItem);
            OpenedTabs.Add(tabItem);
            TabsOpen.Visibility = Visibility.Visible;
            TabsOpen.SelectedItem = SelectedTabItem = tabItem;
        }

        private void _ProjectPanel_ProjectLoaded(Project project)
        {
            _project = project;
            _projectService = new ProjectService(new ErrorService(), project);
            _graphicsService = new GraphicsService(_errorService, project);
            _levelService = new LevelService(_errorService, project);
            _palettesService = new PalettesService(_errorService, project);
            _worldService = new WorldService(_errorService, project);
            _tileService = new TileService(_errorService, project);
            _textService = new TextService(_errorService, project);
            _romService = new RomService(_errorService, _graphicsService, _palettesService, _tileService, _levelService, _worldService, _textService);
            _gameObjectService = new GameObjectService(_errorService, project);

            _levelService.LevelUpdated += _levelService_LevelUpdated;
            _worldService.WorldUpdated += _worldService_WorldUpdated;
            

            List<WorldInfo> worldInfos = new List<WorldInfo>();
            worldInfos.AddRange(project.WorldInfo);
            worldInfos.Add(project.EmptyWorld);

            FilePanel.Initialize(_levelService, _worldService);

            SplashText.Visibility = Visibility.Collapsed;
            _config.LastProjectPath = _project.DirectoryPath + "\\" + _project.Name + ".json";
        }

        public List<TabItem> OpenedTabs = new List<TabItem>();

        public LevelPanel OpenLevelEditor(LevelInfo levelInfo)
        {
            var existingTab = OpenedTabs.Where(t => t.DataContext == levelInfo).FirstOrDefault();

            if (existingTab != null)
            {
                TabsOpen.SelectedItem = SelectedTabItem = existingTab;
                return (LevelPanel)existingTab.Content;
            }

            TabItem tabItem = new TabItem();
            LevelPanel levelPanel = new LevelPanel(_graphicsService, _palettesService, _textService, _tileService, _gameObjectService, _levelService, levelInfo);

            tabItem.Header = levelInfo.Name;
            tabItem.Content = levelPanel;
            tabItem.DataContext = levelInfo;

            TabsOpen.Items.Add(tabItem);
            OpenedTabs.Add(tabItem);
            TabsOpen.Visibility = Visibility.Visible;
            TabsOpen.SelectedItem = SelectedTabItem = tabItem;
            return levelPanel;
        }

        public WorldPanel OpenWorldEditor(WorldInfo worldInfo)
        {
            var existingTab = OpenedTabs.Where(t => t.DataContext == worldInfo).FirstOrDefault();

            if (existingTab != null)
            {
                TabsOpen.SelectedItem = SelectedTabItem = existingTab;
                return (WorldPanel)existingTab.Content;
            }

            TabItem tabItem = new TabItem();
            WorldPanel worldPanel = new WorldPanel(_graphicsService, _palettesService, _textService, _tileService, _worldService, _levelService, worldInfo);

            tabItem.Header = worldInfo.Name;
            tabItem.Content = worldPanel;
            tabItem.DataContext = worldInfo;

            TabsOpen.Items.Add(tabItem);
            OpenedTabs.Add(tabItem);
            TabsOpen.Visibility = Visibility.Visible;
            TabsOpen.SelectedItem = SelectedTabItem = tabItem;
            return worldPanel;
        }

        public void OpenPaletteEditor(Palette palette = null)
        {
            var existingTab = OpenedTabs.Where(t => t.DataContext == _palettesService).FirstOrDefault();

            if (existingTab != null)
            {
                TabsOpen.SelectedItem = SelectedTabItem = existingTab;
            }
            else
            {
                TabItem tabItem = new TabItem();
                PaletteEditor paletteEditor = new PaletteEditor(_projectService, _palettesService);

                tabItem.Header = "Palette Editor";
                tabItem.Content = paletteEditor;
                tabItem.DataContext = _palettesService;
                TabsOpen.Items.Add(tabItem);
                OpenedTabs.Add(tabItem);
                existingTab = tabItem;
            }

            if (palette != null)
            {
                ((PaletteEditor)existingTab.Content).SetPalette(palette);
            }

            TabsOpen.Visibility = Visibility.Visible;
            TabsOpen.SelectedItem = SelectedTabItem = existingTab;
        }

        private void CloseTab(TabItem tabItem)
        {
            if (TabsOpen.Items.Count > 1)
            {
                TabsOpen.Items.Remove(tabItem);
                OpenedTabs.Remove(tabItem);
                if (tabItem.Content is IDetachEvents)
                {
                    ((IDetachEvents)tabItem.Content).DetachEvents();
                }

                if (SelectedTabItem == tabItem)
                {
                    SelectedTabItem = (TabItem)TabsOpen.SelectedItem;
                }
            }
        }

        private void TabItemHeader_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SelectedTabItem = null;
        }

        private void CloseButton_Clicked(object sender, RoutedEventArgs e)
        {
            TabItem tabItem = (TabItem)((Button)sender).DataContext;
            if (TabsOpen.Items.Count > 1)
            {
                if (((string)(tabItem.Header)).EndsWith("*"))
                {
                    if (ConfirmationWindow.Confirm("You have unsaved changes, are you sure you want to close this tab?") == System.Windows.Forms.DialogResult.Yes)
                    {
                        CloseTab(tabItem);
                    }
                }
                else
                {
                    CloseTab(tabItem);
                }
            }
        }

        public void SetUnsavedTab(string tabHeader)
        {
            foreach (TabItem item in TabsOpen.Items)
            {
                if (((string)item.Header).Equals(tabHeader, StringComparison.OrdinalIgnoreCase))
                {
                    item.Header = tabHeader + "*";
                }
            }
        }

        public void SetSavedTab(string tabHeader)
        {
            foreach (TabItem item in TabsOpen.Items)
            {
                if (((string)item.Header).Equals(tabHeader + "*", StringComparison.OrdinalIgnoreCase))
                {
                    item.Header = tabHeader;
                }
            }
        }

        private void TabItemHeader_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Middle)
            {
                if (TabsOpen.Items.Count > 1)
                {
                    CloseTab((TabItem)TabsOpen.SelectedItem);
                }
            }
        }

        private void _OpenLevelEditor(LevelInfo levelInfo)
        {
            OpenLevelEditor(levelInfo);
        }

        private void FilePanel_WorldOpened(WorldInfo worldInfo)
        {
            OpenWorldEditor(worldInfo);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) > 0 &&
               (Keyboard.Modifiers & ModifierKeys.Shift) > 0)
            {
                if (e.Key == Key.S)
                {
                    _ProjectPanel_RomSaved();
                }
            }
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            _config.WindowLocation = new System.Drawing.Rectangle((int)this.Left, (int)this.Top, (int)this.Width, (int)this.Height);
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _config.WindowLocation = new System.Drawing.Rectangle((int)this.Left, (int)this.Top, (int)this.Width, (int)this.Height);
        }

        private void LoadConfiguration()
        {
            if (File.Exists("config.json"))
            {
                _config = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText("config.json"));
                this.Left = _config.WindowLocation.X;
                this.Top = _config.WindowLocation.Y;
                this.Width = _config.WindowLocation.Width;
                this.Height = _config.WindowLocation.Height;
            }
        }

        private void SaveConfiguration()
        {
            File.WriteAllText("config.json", JsonConvert.SerializeObject(_config, Newtonsoft.Json.Formatting.Indented));
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            SaveConfiguration();
        }

        private void FilePanel_NameUpdated(NameUpdate nameUpdate)
        {
            if (nameUpdate.Info is LevelInfo)
            {
                _levelService.NotifyUpdate((LevelInfo)nameUpdate.Info);
            }
            else if (nameUpdate.Info is WorldInfo)
            {
                _worldService.NotifyUpdate((WorldInfo)nameUpdate.Info);
            }

            _projectService.SaveProject();
            
        }

        private void _ProjectPanel_NewLevelClicked()
        {
            NewLevelResult newLevelResult = NewLevelWindow.Show(_levelService, _worldService);
            if(newLevelResult != null)
            {
                _levelService.AddLevel(newLevelResult.Level, newLevelResult.WorldInfo);
            }
            _projectService.SaveProject();
        }

        private void FilePanel_LevelTreeUpdated()
        {
            _projectService.SaveProject();
        }
    }
}