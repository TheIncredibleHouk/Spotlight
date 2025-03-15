using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotlight.Models
{
    public enum SpotlightEventType
    {
        GraphicsUpdated,
        ExtraGraphicsUpdated,
        PaletteAdded,
        PaletteUpdated,
        PaletteRemoved,
        RgbColorsUpdated,
        ProjectUpdated,
        ProjectLoaded,
        GameObjectUpdated,
        TileSetUpdated,
        LevelAdded,
        LevelUpdated,
        LevelRenamed,
        LevelRemoved,
        LevelOpened,
        WorldOpened,
        WorldRenamed,
        RomSaved,
        GenerateMetaData,
        UIGameObjectSelected,
        UIOpenTextEditor,
        UIOpenPaletteEditor,
        UIOpenBlockEditor,
        UIOpenGraphicsEditor,
        UIOpenGameObjectEditor,
        UIOpenLevelEditor,
        UIExportPalette,
        UIBlockSelected
    }
}
