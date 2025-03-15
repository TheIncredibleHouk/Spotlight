using Microsoft.Extensions.DependencyInjection;
using Spotlight.Abstractions;
using Spotlight.Models;
using Spotlight.Services;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Spotlight
{
    /// <summary>
    /// Interaction logic for PointerEditor.xaml
    /// </summary>
    public partial class WorldPointerEditor : UserControl
    {
        private ILevelService _levelService;
        private IEventService _eventService;
        private WorldInfo _worldInfo;
        
        public WorldPointerEditor()
        {
            InitializeComponent();
            InitializeServices();
            InitializeUI();
        }

        private void InitializeServices()
        {
            _levelService = App.Services.GetService<ILevelService>();
            _eventService = App.Services.GetService<IEventService>();
        }

        private void InitializeUI()
        {
            LevelList.ItemsSource = _levelService.AllLevels();
        }

        public void Initialize(WorldInfo worldInfo)
        {
            _worldInfo = worldInfo;
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
                    _eventService.Emit(SpotlightEventType.UIOpenLevelEditor, (LevelInfo)LevelList.SelectedItem);
                    //GlobalPanels.EditLevel((LevelInfo)LevelList.SelectedItem);
                }
            }
        }

        //private void Button_Click_1(object sender, RoutedEventArgs e)
        //{
        //    if (LevelList.SelectedItem != null)
        //    {
        //        if (LevelList.SelectedItem is LevelInfo)
        //        {
        //            _eventService.Emit(SpotlightEventType.UIOpenLevelEditor, (LevelInfo)LevelList.SelectedItem);
        //            //_levelPanel = MainWindow.OpenLevelEditor(levelInfo) GlobalPanels.EditLevel((LevelInfo)LevelList.SelectedItem);
        //            //_levelPanel.LevelEditorExitSelected += LevelPanel_LevelEditorExitSelected;
        //        }
        //    }
        //}

    
    }
}