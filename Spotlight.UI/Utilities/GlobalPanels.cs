using Spotlight.Models;
using System;

namespace Spotlight
{
    public static class GlobalPanels
    {
        public static MainWindow MainWindow { get; set; }

        public static void EditGameObject(GameObject gameObject, Palette palette)
        {
            MainWindow.OpenGameObjectEditor(gameObject, palette);
        }

        public static void EditTileBlock(Guid levelId, int tileBlockValue)
        {
            MainWindow.OpenTileBlockEditor(levelId, tileBlockValue);
        }

        public static LevelPanel EditLevel(LevelInfo levelInfo)
        {
            return MainWindow.OpenLevelEditor(levelInfo);
        }

        public static void OpenPaletteEditor(Palette palette = null)
        {
            MainWindow.OpenPaletteEditor(palette);
        }
    }
}