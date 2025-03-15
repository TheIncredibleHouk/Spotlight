using System.Linq;
using Spotlight.Models;
using Spotlight.Services;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Spotlight.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Spotlight
{
    /// <summary>
    /// Interaction logic for NewLevel.xaml
    /// </summary>
    public partial class MoveLevelWindow : Window
    {
        private ILevelService _levelService;
        private IWorldService _worldService;
        private LevelInfo _defaultParentLevel;

        public static MoveLevelResult Show(WorldInfo hostWorld, LevelInfo parentLevel)
        {
            MoveLevelResult moveLevelResult = null;

            MoveLevelWindow window = new MoveLevelWindow();
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            window.ShowDialog();


            if (window.DialogResult == true)
            {
                moveLevelResult = new MoveLevelResult();
                if (window.ParentLevel != null)
                {
                    moveLevelResult.InfoNode = window.ParentLevel;
                }
                else
                {
                    moveLevelResult.InfoNode = window.HostWorld;
                }

                return moveLevelResult;
            }

            return moveLevelResult;

        }

        public MoveLevelWindow()
        {
            InitializeComponent();
            InitializeServices();
            InitializeUI();
        }

        private void InitializeServices()
        {
            _levelService = App.Services.GetService<ILevelService>();
            _worldService = App.Services.GetService<IWorldService>();
        }

        private void InitializeUI()
        {

            WorldList.ItemsSource = _worldService.AllWorlds();
        }

        public void Initialize(WorldInfo hostWorld, LevelInfo parentLevel)
        {
            _defaultParentLevel = parentLevel;

            if (parentLevel == null)
            {
                LevelList.SelectedIndex = 0;
            }
            else
            {
                LevelList.SelectedItem = parentLevel;
            }

            if (hostWorld != null)
            {
                WorldList.SelectedItem = hostWorld;
            }
        }

        public LevelInfo ParentLevel
        {
            get
            {
                if (LevelList.SelectedIndex == 0)
                {
                    return null;
                }

                return (LevelInfo)LevelList.SelectedItem;
            }
        }

        public WorldInfo HostWorld
        {
            get
            {
                return (WorldInfo)WorldList.SelectedItem;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();

        }

        private void WorldList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            List<LevelInfo> availableLevels = _levelService.FlattenLevelInfos(HostWorld.LevelsInfo).OrderBy(l => l.Name).ToList();
            availableLevels.Insert(0, new LevelInfo()
            {
                Name = "<none>"
            });

            LevelList.ItemsSource = availableLevels;
            LevelList.SelectedItem = _defaultParentLevel;

            if (LevelList.SelectedItem == null)
            {
                LevelList.SelectedIndex = 0;
            }
        }
    }

    public class MoveLevelResult
    {
        public IInfo InfoNode { get; set; }
    }
}
