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
    public partial class NewLevelWindow : Window
    {
        public static NewLevelResult Show(LevelService levelService, WorldService worldService)
        {
            Level newLevel = null;
            NewLevelResult newLevelResult = null;

            NewLevelWindow window = new NewLevelWindow(levelService, worldService);
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
                newLevelResult.LevelInfo = new LevelInfo() { Name = newLevel.Name,SublevelsInfo = new List<LevelInfo>() };
                newLevelResult.WorldInfo = window.HostWorld;

                return newLevelResult;
            }

            return newLevelResult;
            
        }


        LevelService _levelService;
        WorldService _worldService;

        public NewLevelWindow(LevelService levelService, WorldService worldService)
        {
            InitializeComponent();

            _levelService = levelService;
            _worldService = worldService;

            LevelList.ItemsSource = _levelService.AllLevels();
            WorldList.ItemsSource = _worldService.AllWorlds();
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
    }

    public class NewLevelResult
    {
        public LevelInfo LevelInfo { get; set; }
        public Level Level { get; set; }
        public WorldInfo WorldInfo { get; set; }
    }
}
