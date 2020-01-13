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
    /// Interaction logic for GameObjectSelector.xaml
    /// </summary>
    public partial class GameObjectSelector : UserControl
    {
        public GameObjectSelector()
        {
            InitializeComponent();
        }

        private GameObjectService _gameObjectService;
        private GraphicsAccessor _graphicsAccessor;
        private Palette _palette;

        public void Initialize(GameObjectService gameObjectService, GraphicsAccessor graphicsAccessor, Palette palette)
        {
            _gameObjectService = gameObjectService;
            _graphicsAccessor = graphicsAccessor;
            _palette = palette;
        }

        Dictionary<GameObjectType, List<LevelObject>> _gameObjectTable;

        public void RefreshGameObjects()
        {
            _gameObjectTable = _gameObjectService.GameObjectTable();
        }
    }
}
