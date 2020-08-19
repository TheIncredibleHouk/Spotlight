using Spotlight.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotlight.Services
{
    public class PalettesService
    {
        private ErrorService _errorService;
        private Project _project;

        public delegate void PaletteServiceEventHandler();
        public event PaletteServiceEventHandler PalettesChanged;

        public PalettesService(ErrorService errorService, Project project)
        {
            _errorService = errorService;
            _project = project;

            CacheRgbPalettes();
        }

        public void CommitPalette(Palette palette)
        {
            Palette commitPalette = _project.Palettes.Where(p => p.Id == palette.Id).FirstOrDefault();

            if (commitPalette != null)
            {
                for (int i = 0; i < 32; i++)
                {
                    commitPalette.IndexedColors[i] = palette.IndexedColors[i];
                }

                CacheRgbPalettes(commitPalette);

                if(PalettesChanged != null)
                {
                    PalettesChanged();
                }
            }
        }

        public Palette NewPalette(string name, Palette basePalette)
        {
            Palette palette = new Palette();
            for(int i =0; i < 32; i++)
            {
                palette.IndexedColors[i] = basePalette.IndexedColors[i];
            }

            palette.Name = name;
            palette.Id = Guid.NewGuid();
            CacheRgbPalettes(palette);

            _project.Palettes.Add(palette);

            if (PalettesChanged != null)
            {
                PalettesChanged();
            }

            return palette;
        }

        public void RemovePalette(Palette palette)
        {
            Palette foundPalette = _project.Palettes.Where(p => p.Id == palette.Id).FirstOrDefault();
            if (foundPalette != null)
            {
                _project.Palettes.Remove(foundPalette);
            }

            if (PalettesChanged != null)
            {
                PalettesChanged();
            }
        }

        public List<Palette> GetPalettes()
        {
            return _project.Palettes.ToList();
        }

        public void CacheRgbPalettes()
        {
            foreach (var palette in _project.Palettes)
            {
                for (int i = 0; i < 8; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        CacheRgbPalettes(palette);
                    }
                }
            }
        }

        public void CacheRgbPalettes(Palette palette)
        {
            palette.IndexedColors[0x11] = 0x0F;
            palette.IndexedColors[0x12] = 0x36;
            palette.IndexedColors[0x13] = 0x06;

            int paletteIndex = 0;

            for (int i = 0; i < 8; i++)
            {    
                for (int j = 0; j < 4; j++)
                {
                    palette.RgbColors[i][j] = _project.RgbPalette[palette.IndexedColors[paletteIndex++]];
                }
            }
        }


        public Palette GetPalette(Guid paletteId)
        {
            Palette returnedPalette =  _project.Palettes.Where(p => p.Id == paletteId).FirstOrDefault();
            if(returnedPalette == null)
            {
                returnedPalette = _project.Palettes[0];
            }

            return returnedPalette;
        }
        public Color[] RgbPalette
        {
            get
            {
                return _project.RgbPalette;
            }
        }

        public Color[] GetRgbPalette(string[] paletteIndex)
        {
            Color[] rgbPalette = new Color[4];
            for (int j = 0; j < 4; j++)
            {
                int colorIndex = 0;
                try
                {
                    colorIndex = Int32.Parse(paletteIndex[j], System.Globalization.NumberStyles.HexNumber);
                }
                catch
                {

                }

                rgbPalette[j] = _project.RgbPalette[colorIndex];
            }

            return rgbPalette;
        }
    }
}
