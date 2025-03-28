﻿using Microsoft.Extensions.DependencyInjection;
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
    public partial class LevelPointerEditor : UserControl
    {
        private IEventService _eventService;
        private ILevelService _levelService;
        private LevelInfo _levelInfo;

        public LevelPointerEditor()
        {
            InitializeComponent();
            InitializeServices();
            InitializeUI();
        }
        
        public void Initialize(LevelInfo levelInfo)
        {

            _levelInfo = levelInfo;
        }

        private void InitializeServices()
        {
            _levelService = App.Services.GetService<ILevelService>();
            _eventService = App.Services.GetService<IEventService>();
        }

        private void InitializeUI()
        {
            LevelList.ItemsSource = _levelService.GetAllWorldsAndLevels();
        }

        private LevelPointer _pointer;

        public void SetPointer(LevelPointer pointer)
        {
            _pointer = pointer;
            LevelList.SelectedValue = pointer.LevelId;
            ExitX.Text = pointer.ExitX.ToString("X");
            ExitY.Text = pointer.ExitY.ToString("X");
            ExitAction.SelectedIndex = pointer.ExitActionType;
            RedrawLevel.IsChecked = pointer.RedrawsLevel;
            KeepObjectData.IsChecked = pointer.KeepObjects;
        }

        private void LevelList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LevelList.SelectedValue != null)
            {
                _pointer.LevelId = (Guid)LevelList.SelectedValue;
            }

            _pointer.ExitsLevel = LevelList.SelectedItem is WorldInfo;

            ExitActionLabel.Visibility =
            ExitAction.Visibility =
            RedrawLevelLabel.Visibility =
            RedrawLevel.Visibility =
            DisableWeatherLabel.Visibility =
            DisableWeather.Visibility =
            KeepObjectDataLabel.Visibility =
            KeepObjectData.Visibility = _pointer.ExitsLevel ? Visibility.Hidden : Visibility.Visible;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (LevelList.SelectedItem != null)
            {
                if (LevelList.SelectedItem is LevelInfo)
                {
                    _eventService.Emit(SpotlightEventType.UIOpenLevelEditor, LevelList.SelectedItem as LevelInfo);
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
                    _eventService.Emit(SpotlightEventType.UIOpenLevelEditor, LevelList.SelectedItem as LevelInfo);
                    //_levelPanel = GlobalPanels.EditLevel((LevelInfo)LevelList.SelectedItem);
                    //_levelPanel.LevelEditorExitSelected += LevelPanel_LevelEditorExitSelected;
                }
            }
        }

        //private void LevelPanel_LevelEditorExitSelected(int x, int y)
        //{
        //    _levelPanel.LevelEditorExitSelected -= LevelPanel_LevelEditorExitSelected;
        //    _levelPanel = null;

        //    ExitX.Text = x.ToString("X");
        //    ExitY.Text = y.ToString("X");

        //    _eventService.Emit(SpotlightEventType.UIOpenLevelEditor, _levelInfo);
        //}

        private void ExitX_TextChanged(object sender, TextChangedEventArgs e)
        {
            int x;
            if (Int32.TryParse(ExitX.Text, System.Globalization.NumberStyles.HexNumber, System.Threading.Thread.CurrentThread.CurrentCulture, out x))
            {
                _pointer.ExitX = x;
            }
        }

        private void ExitY_TextChanged(object sender, TextChangedEventArgs e)
        {
            int y;
            if (Int32.TryParse(ExitY.Text, System.Globalization.NumberStyles.HexNumber, System.Threading.Thread.CurrentThread.CurrentCulture, out y))
            {
                _pointer.ExitY = y;
            }
        }

        private void Pointer_CheckBoxChanged(object sender, RoutedEventArgs e)
        {
            _pointer.DisableWeather = DisableWeather.IsChecked.Value;
            _pointer.RedrawsLevel = RedrawLevel.IsChecked.Value;
            _pointer.KeepObjects = KeepObjectData.IsChecked.Value;
        }

        private void ExitAction_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _pointer.ExitActionType = ExitAction.SelectedIndex;
        }
    }
}