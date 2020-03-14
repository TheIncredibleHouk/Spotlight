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
    /// Interaction logic for TileBlockEditor.xaml
    /// </summary>
    public partial class TileBlockEditor : UserControl
    {
        private GraphicsService _graphicsService;
        private TileService _tileService;
        private TextService _textService;
        public TileBlockEditor(GraphicsService graphicsService, TileService tileService, TextService textService)
        {
            InitializeComponent();
            
            _graphicsService = graphicsService;
            _tileService = tileService;
            _textService = textService;
            
            List<KeyValuePair<string, string>> tileSets = _textService.GetTable("tile_sets");
            tileSets.Insert(0, new KeyValuePair<string, string>("0", "Map"));

            PaletteList.ItemsSource = _graphicsService.GetPalettes();
            TileSetList.ItemsSource = tileSets;
            TerrainList.ItemsSource = _tileService.GetTerrain();
        }

        private bool _showTerrain;
        private void ShowTerrain_Changed(object sender, RoutedEventArgs e)
        {
            _showTerrain = ShowTerrain.IsChecked.Value;
            ShowInteractions.IsChecked = ShowItems.IsChecked = false;

        }

        private bool _showInteractions;
        private void ShowInteractions_Changed(object sender, RoutedEventArgs e)
        {
            _showItems = ShowInteractions.IsChecked.Value;
            ShowTerrain.IsChecked = ShowItems.IsChecked = false;
        }

        private bool _showItems;
        private void ShowItems_Changed(object sender, RoutedEventArgs e)
        {
            _showItems = ShowItems.IsChecked.Value;
            ShowTerrain.IsChecked = ShowInteractions.IsChecked = false;
        }

        private void TileSelector_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void TerrainList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            InteractionList.ItemsSource = ((TileTerrain)TerrainList.SelectedItem).Interactions;
        }
    }
}
