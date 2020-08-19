using Spotlight.Models;
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
        public MainWindow()
        {
            _errorService = new ErrorService();
            _projectService = new ProjectService(_errorService);

            InitializeComponent();

            _ProjectPanel.ProjectService = _projectService;
            _ProjectPanel.ProjectLoaded += _ProjectPanel_ProjectLoaded;
            _ProjectPanel.TextEditorOpened += _ProjectPanel_TextEditorOpened;
            _ProjectPanel.ObjectEditorOpened += OpenGameObjectEditor;
            _ProjectPanel.TileBlockEditorOpened += OpenTileBlockEditor;
            TabsOpen.SelectionChanged += TabsOpen_SelectionChanged;
            Activated += MainWindow_Activated;

            GlobalPanels.MainWindow = this;
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
            TileBlockEditor tileSetEditor = new TileBlockEditor(_worldService, _levelService, _graphicsService, _palettesService, _tileService, _textService);

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
            TextPanel textPanel = new TextPanel(_textService);

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
            _projectService = new ProjectService(new ErrorService(), project);
            _graphicsService = new GraphicsService(_errorService, project);
            _levelService = new LevelService(_errorService, project);
            _palettesService = new PalettesService(_errorService, project);
            _worldService = new WorldService(_errorService, project);
            _tileService = new TileService(_errorService, project);
            _textService = new TextService(_errorService, project);
            _gameObjectService = new GameObjectService(_errorService, project);

            List<WorldInfo> worldInfos = new List<WorldInfo>();
            worldInfos.AddRange(project.WorldInfo);
            worldInfos.Add(project.EmptyWorld);

            _WorldPanel.BuildTree(worldInfos);

            SplashText.Visibility = Visibility.Collapsed;
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

        public void OpenPaletteEditor()
        {
            var existingTab = OpenedTabs.Where(t => t.DataContext == _palettesService).FirstOrDefault();

            if (existingTab != null)
            {
                TabsOpen.SelectedItem = SelectedTabItem = existingTab;
            }

            TabItem tabItem = new TabItem();
            PaletteEditor paletteEditor = new PaletteEditor(_palettesService);

            tabItem.Header = "Palette Editor";
            tabItem.Content = paletteEditor;
            tabItem.DataContext = _palettesService;

            TabsOpen.Items.Add(tabItem);
            OpenedTabs.Add(tabItem);
            TabsOpen.Visibility = Visibility.Visible;
            TabsOpen.SelectedItem = SelectedTabItem = tabItem;
        }

        private void CloseTab(TabItem tabItem)
        {
            if (TabsOpen.Items.Count > 1)
            {
                TabsOpen.Items.Remove(tabItem);
                OpenedTabs.Remove(tabItem);
                if(tabItem.Content is IDetachEvents)
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
    }
}
