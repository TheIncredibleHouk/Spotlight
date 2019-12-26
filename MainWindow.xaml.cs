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
        private LevelService _levelService;
        private TileService _tileService;
        private TextService _textService;

        public MainWindow()
        {
            _errorService = new ErrorService();
            _projectService = new ProjectService(_errorService);

            InitializeComponent();

            _ProjectPanel.ProjectService = _projectService;
            _ProjectPanel.ProjectLoaded += _ProjectPanel_ProjectLoaded;
            _ProjectPanel.TextEditorOpened += _ProjectPanel_TextEditorOpened;
        }

        private void _ProjectPanel_TextEditorOpened()
        {
            var existingTab = OpenedTabs.Where(t => t.DataContext == _textService).FirstOrDefault();

            if (existingTab != null)
            {
                TabsOpen.SelectedItem = existingTab;
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
            TabsOpen.SelectedItem = tabItem;
        }

        private void _ProjectPanel_ProjectLoaded(Project project)
        {
            _projectService = new ProjectService(new ErrorService(), project);
            _graphicsService = new GraphicsService(_errorService, project);
            _levelService = new LevelService(_errorService, project);
            _tileService = new TileService(_errorService, project);
            _textService = new TextService(_errorService, project);

            List<WorldInfo> worldInfos = new List<WorldInfo>();
            worldInfos.AddRange(project.WorldInfo);
            worldInfos.Add(project.EmptyWorld);

            _WorldPanel.BuildTree(worldInfos);
            
            SplashText.Visibility = Visibility.Collapsed;
        }

        public List<TabItem> OpenedTabs = new List<TabItem>();
        private void _WorldPanel_LevelOpened(LevelInfo levelInfo)
        {
            var existingTab = OpenedTabs.Where(t => t.DataContext == levelInfo).FirstOrDefault();

            if (existingTab != null)
            {
                TabsOpen.SelectedItem = existingTab;
                return;
            }

            TabItem tabItem = new TabItem();
            LevelPanel levelPanel = new LevelPanel(_graphicsService, _textService, _tileService, _levelService, _levelService.LoadLevel(levelInfo));

            tabItem.Header = levelInfo.Name;
            tabItem.Content = levelPanel;
            tabItem.DataContext = levelInfo;

            TabsOpen.Items.Add(tabItem);
            OpenedTabs.Add(tabItem);
            TabsOpen.Visibility = Visibility.Visible;
            TabsOpen.SelectedItem = tabItem;
        }

        private void CloseTab(TabItem tabItem)
        {
            if (TabsOpen.Items.Count > 1)
            {
                TabsOpen.Items.Remove(tabItem);
                OpenedTabs.Remove(tabItem);
            }
        }

        private void TabItemHeader_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Middle)
            {
                if (TabsOpen.Items.Count > 1)
                {
                    CloseTab((TabItem)TabsOpen.SelectedItem);
                }
            }
        }

        private void CloseButton_Clicked(object sender, RoutedEventArgs e)
        {

            if (TabsOpen.Items.Count > 1)
            {
                CloseTab((TabItem)((Button)sender).DataContext);
            }
        }
    }
}
