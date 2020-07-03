using Spotlight.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotlight
{
    public static class GlobalPanels
    {
        public static MainWindow MainWindow { get; set; }

        public static void EditGameObject(GameObject gameObject, Palette palette)
        {
            MainWindow.OpenGameObjectEditor(gameObject, palette);
        }

        public static void EditTileBlock(TileSet tileSet, TileBlock tileBlock)
        {

        }
    }
}
