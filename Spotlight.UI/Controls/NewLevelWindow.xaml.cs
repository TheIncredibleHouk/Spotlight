﻿using Microsoft.Extensions.DependencyInjection;
using Spotlight.Abstractions;
using Spotlight.Models;
using Spotlight.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Spotlight
{
    /// <summary>
    /// Interaction logic for NewLevel.xaml
    /// </summary>
    public partial class NewLevelWindow : Window
    {
        private ILevelService _levelService;
        private IWorldService _worldService;

        public NewLevelWindow()
        {
            InitializeComponent();
            InitializeServices();
        }

        private void InitializeUI()
        {
            WorldList.ItemsSource = _worldService.AllWorlds();
        }

        private void InitializeServices()
        {
            _levelService = App.Services.GetService<ILevelService>();
            _worldService = App.Services.GetService<IWorldService>();
        }

        public string LevelName
        {
            get
            {
                return LevelNameText.Text;
            }
        }

        public Level BaseLevel
        {
            get
            {
                return _levelService.LoadLevel((LevelInfo)LevelList.SelectedItem);
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
            LevelList.ItemsSource = _levelService.FlattenLevelInfos(HostWorld.LevelsInfo).OrderBy(l => l.Name).ToList();
        }

        public static NewLevelResult Show()
        {
            Level newLevel = null;
            NewLevelResult newLevelResult = null;

            NewLevelWindow window = new NewLevelWindow();
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            window.ShowDialog();


            if (window.DialogResult == true)
            {
                newLevel = new Level();
                newLevel.AnimationType = window.BaseLevel.AnimationType;
                newLevel.Effects = window.BaseLevel.Effects;
                newLevel.EventType = window.BaseLevel.EventType;
                newLevel.StaticTileTableIndex = window.BaseLevel.StaticTileTableIndex;
                newLevel.Id = Guid.NewGuid();
                newLevel.MusicValue = window.BaseLevel.MusicValue;
                newLevel.Name = window.LevelName;
                newLevel.PaletteEffect = window.BaseLevel.PaletteEffect;
                newLevel.PaletteId = window.BaseLevel.PaletteId;
                newLevel.ScreenLength = window.BaseLevel.ScreenLength;
                newLevel.ScrollType = window.BaseLevel.ScrollType;
                newLevel.StartX = window.BaseLevel.StartX;
                newLevel.StartY = window.BaseLevel.StartY;
                newLevel.TileSetIndex = window.BaseLevel.TileSetIndex;

                newLevelResult = new NewLevelResult();
                newLevelResult.Level = newLevel;
                newLevelResult.LevelInfo = new LevelInfo() { Name = newLevel.Name, SublevelsInfo = new List<LevelInfo>() };
                newLevelResult.WorldInfo = window.HostWorld;

                return newLevelResult;
            }

            return newLevelResult;

        }

    }

    public class NewLevelResult
    {
        public LevelInfo LevelInfo { get; set; }
        public Level Level { get; set; }
        public WorldInfo WorldInfo { get; set; }
    }

}
