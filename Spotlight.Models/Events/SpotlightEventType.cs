using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotlight.Models
{
    public enum SpotlightEventType
    {
        GraphicsUpdate,
        ExtraGraphicsUpdate,
        PaletteAdded,
        PaletteUpdate,
        PaletteRemoved,
        RgbColorsUpdated,
        ProjectUpdate,
        ProjectLoaded,
        GameObjectsUpdated,
        TileSetUpdated
    }
}
