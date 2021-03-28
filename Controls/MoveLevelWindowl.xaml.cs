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
using System.Windows.Shapes;

namespace Spotlight
{
    /// <summary>
    /// Interaction logic for NewLevel.xaml
    /// </summary>
    public partial class MoveLevelWindow : Window
    {
        public static MoveLevelResult Show(LevelService levelService, WorldService worldService, WorldInfo hostWorld, LevelInfo parentLevel)
        {
            MoveLevelResult moveLevelResult = null;

            MoveLevelWindow window = new MoveLevelWindow(levelService, worldService, hostWorld, parentLevel);
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            window.ShowDialog();


            if (window.DialogResult == true)
            {
                moveLevelResult = new MoveLevelResult();
                if(window.ParentLevel != null)
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


        LevelService _levelService;
        WorldService _worldService;
        LevelInfo _defaultParentLevel;

        public MoveLevelWindow(LevelService levelService, WorldService worldService, WorldInfo hostWorld, LevelInfo parentLevel)
        {
            InitializeComponent();

            _levelService = levelService;
            _worldService = worldService;
            _defaultParentLevel = parentLevel;

            WorldList.ItemsSource = _worldService.AllWorlds();
            
            if(parentLevel == null)
            {
                LevelList.SelectedIndex = 0;
            }
            else
            {
                LevelList.SelectedItem = parentLevel;
            }

            if(hostWorld != null)
            {
                WorldList.SelectedItem = hostWorld;
            }
        }

        public LevelInfo ParentLevel
        {
            get
            {
                if(LevelList.SelectedIndex == 0)
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

            List<LevelInfo> availableLevels = _levelService.FlattenLevelInfos(HostWorld.LevelsInfo);
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
