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
    /// Interaction logic for PointerEditor.xaml
    /// </summary>
    public partial class WorldPointerEditor : UserControl
    {
        public WorldPointerEditor()
        {
            InitializeComponent();
        }

        private LevelService _levelService;
        private WorldInfo _worldInfo;
        public void Initialize(LevelService levelService, WorldInfo worldInfo)
        {
            _levelService = levelService;
            _worldInfo = worldInfo;

            LevelList.ItemsSource = _levelService.AllWorldsLevels();
        }

        private WorldPointer _pointer;
        public void SetPointer(WorldPointer pointer)
        {
            _pointer = pointer;
            LevelList.SelectedValue = pointer.LevelId;
        }

        private void LevelList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LevelList.SelectedValue != null)
            {
                _pointer.LevelId = (Guid)LevelList.SelectedValue;
            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (LevelList.SelectedItem != null)
            {
                if (LevelList.SelectedItem is LevelInfo)
                {
                    GlobalPanels.EditLevel((LevelInfo)LevelList.SelectedItem);
                }
            }
        }
        private LevelPanel _levelPanel;
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (LevelList.SelectedItem != null)
            {
                if (LevelList.SelectedItem is LevelInfo)
                {
                    _levelPanel = GlobalPanels.EditLevel((LevelInfo)LevelList.SelectedItem);
                    _levelPanel.LevelEditorExitSelected += LevelPanel_LevelEditorExitSelected;
                }
            }
        }

        private void LevelPanel_LevelEditorExitSelected(int x, int y)
        {
            _levelPanel.LevelEditorExitSelected -= LevelPanel_LevelEditorExitSelected;
            _levelPanel = null;
            
        }
    }
}
