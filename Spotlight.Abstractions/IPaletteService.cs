using Spotlight.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotlight.Abstractions
{
    public interface IPaletteService
    {
        void CommitPalette(Palette palette);
        Palette NewPalette(string name, Palette basePalette);
        void RemovePalette(Palette palette);
        List<Palette> GetPalettes();
        void CacheRgbPalettes();
        void CacheRgbPalettes(Palette palette);
        Palette GetPalette(Guid paletteId);
        Color[] RgbPalette { get; }
        Color[] GetRgbPalette(string[] paletteIndex);
        void CommitRgbPalette(Color[] rgbPalette);
        void ExportRgbPalette(string fileName);
    }
}
