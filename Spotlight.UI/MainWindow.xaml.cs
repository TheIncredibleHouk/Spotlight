using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Spotlight.Abstractions;
using Spotlight.Controls;
using Spotlight.Models;
using Spotlight.Renderers;
using Spotlight.Services;
using Spotlight.UI;
using Spotlight.UI.Events;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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

        private IEventService _eventService;
        private ILevelService _levelService;
        private IProjectService _projectService;
        private IConfigurationService _configurationService;
        private IRomService _romService;
        private IGraphicsService _graphicsService;
        //private ILevelService _levelService;
        //private IWorldService _worldService;

        public MainWindow()
        {
            InitializeComponent();
            InitializeUI();
            InitializeServices();

            _eventService.Subscribe<Project>(SpotlightEventType.ProjectLoaded, ProjectLoaded);
            _eventService.Subscribe(SpotlightEventType.UIOpenTextEditor, OpenTextEditor);
            _eventService.Subscribe<OpenGameObjectEditorEvent>(SpotlightEventType.UIOpenGameObjectEditor, OpenGameObjectEditor);
            _eventService.Subscribe(SpotlightEventType.UIOpenBlockEditor, OpenTileBlockEditor);
            _eventService.Subscribe(SpotlightEventType.UIOpenGraphicsEditor, OpenGraphicsEditor);

            //GlobalPanels.MainWindow = this;


        }

        private void InitializeServices()
        {
            _levelService = App.Services.GetService<ILevelService>();
            _eventService = App.Services.GetService<IEventService>();
            _projectService = App.Services.GetService<IProjectService>();
            _configurationService = App.Services.GetService<IConfigurationService>();
            _romService = App.Services.GetService<IRomService>();
            _graphicsService = App.Services.GetService<IGraphicsService>();
        }

        private void InitializeUI()
        {
            TabsOpen.SelectionChanged += TabsOpen_SelectionChanged;
            Activated += MainWindow_Activated;
        }

        private void Initialize()
        {
            Configuration config = _configurationService.GetConfiguration();

            if (config.LastProjectPath != null && File.Exists(config.LastProjectPath))
            {
                _projectService.LoadProject(config.LastProjectPath);
            }
        }

        //private MusicEditor _musicEditor;
        //private void _ProjectPanel_MusicEditorClicked()
        //{
        //    if (_musicEditor == null)
        //    {
        //        _musicEditor = new MusicEditor();
        //        _musicEditor.Closed += _musicEditor_Closed;
        //        _musicEditor.Show();
        //    }

        //    _musicEditor.Focus();
        //}

        //private void _musicEditor_Closed(object sender, EventArgs e)
        //{
        //    _musicEditor.Closed -= _musicEditor_Closed;
        //    _musicEditor = null;
        //}

        //private void _ProjectPanel_GenerateMetaDataClicked()
        //{
        //    foreach (LevelInfo levelInfo in _levelService.AllLevels())
        //    {
        //        Level level = _levelService.LoadLevel(levelInfo);
        //        if (level.ObjectData.Count == 0)
        //        {
        //            continue;
        //        }

        //        level.FirstObjectData.ForEach(o => o.GameObject = _gameObjectService.GetObject(o.GameObjectId));
        //        level.SecondObjectData.ForEach(o => o.GameObject = _gameObjectService.GetObject(o.GameObjectId));

        //        Tile[] staticSet = _graphicsService.GetTileSection(level.StaticTileTableIndex);
        //        Tile[] animationSet = _graphicsService.GetTileSection(level.AnimationTileTableIndex);
        //        LevelRenderer levelRenderer = new LevelRenderer(
        //            new GraphicsManager(staticSet, animationSet, _graphicsService.GetGlobalTiles(), _graphicsService.GetExtraTiles()),
        //            new LevelDataManager(level),
        //            _palettesService,
        //            _gameObjectService,
        //            _tileService.GetTerrain());

        //        levelRenderer.Update(palette: _palettesService.GetPalette(level.PaletteId), tileSet: _tileService.GetTileSet(level.TileSetIndex));

        //        var startPointObject = level.ObjectData[0];

        //        int thumbnailX = startPointObject.X * 16 - 120;
        //        int thumbnailY = startPointObject.Y * 16 - 120;

        //        if (thumbnailX < 0)
        //        {
        //            thumbnailX = 0;
        //        }

        //        if (thumbnailY < 0)
        //        {
        //            thumbnailY = 0;
        //        }

        //        if (thumbnailX + 256 > 256 * level.ScreenLength)
        //        {
        //            thumbnailX = 256 * level.ScreenLength - 256;
        //        }

        //        if (thumbnailY + 256 > LevelRenderer.BITMAP_HEIGHT)
        //        {
        //            thumbnailY = LevelRenderer.BITMAP_HEIGHT - 256;
        //        }

        //        Rectangle thumbnailReact = new Rectangle(thumbnailX, thumbnailY, 256, 256);
        //        levelRenderer.Update();

        //        using (MemoryStream ms = new MemoryStream(levelRenderer.GetRectangle(thumbnailReact)))
        //        {
        //            _levelService.GenerateMetaData(_tileService, levelInfo, ms);
        //        }
        //    }

        //    AlertWindow.Alert("Meta data generated for all levels.");
        //}

        private GraphicsWindow _graphicsWindow;
        private void OpenGraphicsEditor()
        {
            if (_graphicsWindow == null)
            {
                _graphicsWindow = new GraphicsWindow();
                _graphicsWindow.Closed += GraphicsWindow_Closed;
                _graphicsWindow.Show();
            }

            _graphicsWindow.Focus();
        }

        private void GraphicsWindow_Closed(object sender, EventArgs e)
        {
            _graphicsWindow.Closed -= GraphicsWindow_Closed;
            _graphicsWindow = null;
        }

        private void WorldRenamed(WorldInfo worldInfo)
        {
            var existingTab = OpenedTabs.Where(t => t.DataContext.Equals(worldInfo.Id)).FirstOrDefault();

            if (existingTab != null)
            {
                existingTab.Header = worldInfo.Name;
            }
        }

        private void LevelRenamed(LevelInfo levelInfo)
        {
            var existingTab = OpenedTabs.Where(t => t.DataContext.Equals(levelInfo.Id)).FirstOrDefault();

            if (existingTab != null)
            {
                existingTab.Header = levelInfo.Name;
            }
        }

        private void SaveRom()
        {
            Configuration config = _configurationService.GetConfiguration();

            if (config.LastRomPath == null || !File.Exists(config.LastRomPath) || (Keyboard.Modifiers == ModifierKeys.Control))
            {
                System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog()
                {
                    Filter = "NES ROM|*.nes"
                };

                if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    config.LastRomPath = saveFileDialog.FileName;
                }
            }

            if (config.LastRomPath != null)
            {
                try
                {
                    RomInfo romInfo = _romService.CompileRom(config.LastRomPath);

                    //if (_errorService.CurrentLog.Count > 0)
                    //{
                    //    AlertWindow.Alert("Rom compiled but with the following errors: " + string.Join("\n-", _errorService.CurrentLog));
                    //}
                    //else
                    //{
                    //    AlertWindow.Alert($"Rom compiled!\nLevels used: {romInfo.LevelsUsed}\nRemaining space: {romInfo.SpaceRemaining} bytes\nRemaining extended space: {romInfo.ExtendedSpaceRemaining} bytes");
                    //}

                    _configurationService.UpdateConfiguration(config);
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
            OpenTileBlockEditor(null);
        }

        private Guid _tileBlockEditorId;
        public void OpenTileBlockEditor(OpenTileBlockEditorEvent openTileBlockEditorEvent)
        {
            var existingTab = OpenedTabs.Where(t => t.DataContext.Equals(_tileBlockEditorId)).FirstOrDefault();

            if (existingTab != null)
            {
                if (openTileBlockEditorEvent != null)
                {
                    if (openTileBlockEditorEvent.LevelId != Guid.Empty && openTileBlockEditorEvent.BlockId > -1)
                    {
                        ((TileBlockEditor)existingTab.Content).SelectTileBlock(openTileBlockEditorEvent.LevelId, openTileBlockEditorEvent.BlockId);
                    }
                }

                TabsOpen.SelectedItem = SelectedTabItem = existingTab;
                return;
            }

            TabItem tabItem = new TabItem();
            TileBlockEditor tileSetEditor = new TileBlockEditor();

            tabItem.Header = "Tile Set Editor";
            tabItem.Content = tileSetEditor;
            tabItem.DataContext = _tileBlockEditorId = tileSetEditor.Id;

            TabsOpen.Items.Add(tabItem);
            OpenedTabs.Add(tabItem);
            TabsOpen.Visibility = Visibility.Visible;
            TabsOpen.SelectedItem = SelectedTabItem = tabItem;

            if (openTileBlockEditorEvent != null)
            {
                if (openTileBlockEditorEvent.LevelId != Guid.Empty && openTileBlockEditorEvent.BlockId > -1)
                {
                    tileSetEditor.SelectTileBlock(openTileBlockEditorEvent.LevelId, openTileBlockEditorEvent.BlockId);
                }
            }
        }

        private Guid _gameObjectEditorId;

        public void OpenGameObjectEditor(OpenGameObjectEditorEvent openGameObjectEditorEvent)
        {
            var existingTab = OpenedTabs.Where(t => t.DataContext.Equals(_gameObjectEditorId)).FirstOrDefault();

            if (existingTab != null)
            {
                ((GameObjectEditor)existingTab.Content).SelectObject(openGameObjectEditorEvent.SelectedGameObject, openGameObjectEditorEvent.SelectedPalette);
                TabsOpen.SelectedItem = SelectedTabItem = existingTab;
                return;
            }

            TabItem tabItem = new TabItem();
            GameObjectEditor objectEditor = new GameObjectEditor();

            tabItem.Header = "Object Editor";
            tabItem.Content = objectEditor;
            tabItem.DataContext = _gameObjectEditorId = objectEditor.Id;

            objectEditor.SelectObject(openGameObjectEditorEvent.SelectedGameObject, openGameObjectEditorEvent.SelectedPalette);

            TabsOpen.Items.Add(tabItem);
            OpenedTabs.Add(tabItem);

            TabsOpen.Visibility = Visibility.Visible;
            TabsOpen.SelectedItem = SelectedTabItem = tabItem;
        }

        private Guid _textEditorId;
        private void OpenTextEditor()
        {
            var existingTab = OpenedTabs.Where(t => t.DataContext.Equals(_textEditorId)).FirstOrDefault();

            if (existingTab != null)
            {
                TabsOpen.SelectedItem = SelectedTabItem = existingTab;
                return;
            }

            TabItem tabItem = new TabItem();
            TextPanel textPanel = new TextPanel();

            tabItem.Header = "Text Editor";
            tabItem.Content = textPanel;
            tabItem.DataContext = _textEditorId = textPanel.Id;

            TabsOpen.Items.Add(tabItem);
            OpenedTabs.Add(tabItem);

            TabsOpen.Visibility = Visibility.Visible;
            TabsOpen.SelectedItem = SelectedTabItem = tabItem;
        }

        private void ProjectLoaded(Project project)
        {


            List<WorldInfo> worldInfos = new List<WorldInfo>();
            worldInfos.AddRange(project.WorldInfo);
            worldInfos.Add(project.EmptyWorld);

            SplashText.Visibility = Visibility.Collapsed;

            Configuration config = _configurationService.GetConfiguration();
            config.LastProjectPath = project.DirectoryPath + "\\" + project.Name + ".json";
            _configurationService.UpdateConfiguration(config);
        }

        public List<TabItem> OpenedTabs = new List<TabItem>();

        public void OpenLevelEditor(LevelInfo levelInfo)
        {
            var existingTab = OpenedTabs.Where(t => t.DataContext.Equals(levelInfo.Id)).FirstOrDefault();

            if (existingTab != null)
            {
                TabsOpen.SelectedItem = SelectedTabItem = existingTab;
                return;
            }

            TabItem tabItem = new TabItem();
            LevelPanel levelPanel = new LevelPanel();

            levelPanel.Initialize(levelInfo);

            tabItem.Header = levelInfo.Name;
            tabItem.Content = levelPanel;
            tabItem.DataContext = levelInfo.Id;

            TabsOpen.Items.Add(tabItem);
            OpenedTabs.Add(tabItem);

            TabsOpen.Visibility = Visibility.Visible;
            TabsOpen.SelectedItem = SelectedTabItem = tabItem;
        }

        public WorldPanel OpenWorldEditor(WorldInfo worldInfo)
        {
            var existingTab = OpenedTabs.Where(t => t.DataContext.Equals(worldInfo.Id)).FirstOrDefault();

            if (existingTab != null)
            {
                TabsOpen.SelectedItem = SelectedTabItem = existingTab;
                return (WorldPanel)existingTab.Content;
            }

            TabItem tabItem = new TabItem();
            WorldPanel worldPanel = new WorldPanel();

            worldPanel.Initialize(worldInfo);

            tabItem.Header = worldInfo.Name;
            tabItem.Content = worldPanel;
            tabItem.DataContext = worldInfo.Id;

            TabsOpen.Items.Add(tabItem);
            OpenedTabs.Add(tabItem);
            TabsOpen.Visibility = Visibility.Visible;
            TabsOpen.SelectedItem = SelectedTabItem = tabItem;
            return worldPanel;
        }

        private Guid _paletteEditorId;
        public void OpenPaletteEditor(Palette palette = null)
        {
            var existingTab = OpenedTabs.Where(t => t.DataContext.Equals(_paletteEditorId)).FirstOrDefault();

            if (existingTab != null)
            {
                TabsOpen.SelectedItem = SelectedTabItem = existingTab;
            }
            else
            {
                TabItem tabItem = new TabItem();
                PaletteEditor paletteEditor = new PaletteEditor();

                tabItem.Header = "Palette Editor";
                tabItem.Content = paletteEditor;
                tabItem.DataContext = _paletteEditorId = Guid.NewGuid();
                TabsOpen.Items.Add(tabItem);
                OpenedTabs.Add(tabItem);
                existingTab = tabItem;
            }

            if (palette != null)
            {
                ((PaletteEditor)existingTab.Content).Initialize(palette);
            }

            TabsOpen.Visibility = Visibility.Visible;
            TabsOpen.SelectedItem = SelectedTabItem = existingTab;
        }

        private void CloseTab(TabItem tabItem)
        {
            TabsOpen.Items.Remove(tabItem);
            OpenedTabs.Remove(tabItem);
            if (tabItem.Content is IUnsubscribe)
            {
                ((IUnsubscribe)tabItem.Content).Unsubscribe();
            }

            if (SelectedTabItem == tabItem)
            {
                SelectedTabItem = (TabItem)TabsOpen.SelectedItem;
            }
        }

        private void TabItemHeader_MouseDown(object sender, MouseButtonEventArgs e)
        {
            TabsOpen.SelectedItem = SelectedTabItem = (TabItem)((Border)sender).DataContext; ;
        }

        private void CloseButton_Clicked(object sender, RoutedEventArgs e)
        {
            TabItem tabItem = (TabItem)((Button)sender).DataContext;
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
                    _projectService.SaveProject();
                }
            }
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            Configuration configuration = _configurationService.GetConfiguration();
            configuration.WindowLocation = new System.Drawing.Rectangle((int)this.Left, (int)this.Top, (int)this.Width, (int)this.Height);
            _configurationService.UpdateConfiguration(configuration);
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Configuration config = _configurationService.GetConfiguration();
            config.WindowLocation = new System.Drawing.Rectangle((int)this.Left, (int)this.Top, (int)this.Width, (int)this.Height);
            _configurationService.UpdateConfiguration(config);
        }


        private void Window_Closed(object sender, EventArgs e)
        {
            _projectService.SaveProject();
        }

        //private void FilePanel_NameUpdated(NameUpdate nameUpdate)
        //{


        //    _projectService.SaveProject();

        //}

        private void _ProjectPanel_NewLevelClicked()
        {
            NewLevelResult newLevelResult = NewLevelWindow.Show();
            if (newLevelResult != null)
            {
                _levelService.AddLevel(newLevelResult.Level, newLevelResult.WorldInfo);
            }
            _projectService.SaveProject();
        }

        private void FilePanel_LevelTreeUpdated()
        {
            _projectService.SaveProject();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_levelService == null)
            {
                return;
            }

            //IEnumerable<FileInfo> residualTempFiles = _levelService.FindTemps();
            //foreach (FileInfo fileInfo in residualTempFiles)
            //{

            //    //if (ConfirmationWindow.Confirm($"Unsaved level data {fileInfo.Name}, would you like to swap this out?") == System.Windows.Forms.DialogResult.Yes)
            //    //{
            //    //    _levelService.SwapTemp(fileInfo);
            //    //}
            //}

            _levelService.CleanUpTemps();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (TabsOpen.SelectedItem != null)
            {
                object selectedContent = ((TabItem)TabsOpen.SelectedItem).Content;
                if (selectedContent is IKeyDownHandler)
                {
                    ((IKeyDownHandler)selectedContent).HandleKeyDown(e);
                }
            }
        }
    }
}