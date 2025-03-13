using Spotlight.Abstractions;
using Spotlight.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace Spotlight.Services
{
    public class PaletteService : IPaletteService
    {
        private IErrorService _errorService;
        private IEventService _eventService;
        private IProjectService _projectService;

        public PaletteService(IErrorService errorService, IProjectService projectService, IEventService eventService)
        {
            _errorService = errorService;
            _projectService = projectService;
            _eventService = eventService;

            CacheRgbPalettes();
        }

        public void CommitPalette(Palette palette)
        {
            Palette commitPalette = _projectService.GetProject().Palettes.Where(p => p.Id == palette.Id).FirstOrDefault();

            if (commitPalette != null)
            {
                for (int i = 0; i < 32; i++)
                {
                    commitPalette.IndexedColors[i] = palette.IndexedColors[i];
                }

                CacheRgbPalettes(commitPalette);

                _eventService.Emit(SpotlightEventType.PaletteUpdated, palette.Id, palette);
            }
        }

        public Palette NewPalette(string name, Palette basePalette)
        {
            Palette palette = new Palette();
            for (int i = 0; i < 32; i++)
            {
                palette.IndexedColors[i] = basePalette.IndexedColors[i];
            }

            palette.Name = name;
            palette.Id = Guid.NewGuid();
            CacheRgbPalettes(palette);

            _projectService.GetProject().Palettes.Add(palette);

            _eventService.Emit(SpotlightEventType.PaletteAdded, palette);

            return palette;
        }

        public void RemovePalette(Palette palette)
        {
            Project project = _projectService.GetProject();
            Palette foundPalette = project.Palettes.Where(p => p.Id == palette.Id).FirstOrDefault();
            if (foundPalette != null)
            {
                project.Palettes.Remove(foundPalette);
            }

            _eventService.Emit(SpotlightEventType.PaletteRemoved, palette);
        }

        public List<Palette> GetPalettes()
        {
            return _projectService.GetProject().Palettes.ToList();
        }

        public void CacheRgbPalettes()
        {
            foreach (var palette in _projectService.GetProject().Palettes)
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
            Project project = _projectService.GetProject();
            palette.IndexedColors[0x11] = 0x0F;
            palette.IndexedColors[0x12] = 0x36;
            palette.IndexedColors[0x13] = 0x06;

            int paletteIndex = 0;

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    palette.RgbColors[i][j] = project.RgbPalette[palette.IndexedColors[paletteIndex++]];
                }
            }
        }

        public Palette GetPalette(Guid paletteId)
        {
            Project project = _projectService.GetProject();

            Palette returnedPalette = project.Palettes.Where(p => p.Id == paletteId).FirstOrDefault();
            if (returnedPalette == null)
            {
                returnedPalette = project.Palettes[0];
            }

            return returnedPalette;
        }

        public Color[] RgbPalette
        {
            get
            {
                return _projectService.GetProject().RgbPalette;
            }
        }

        public Color[] GetRgbPalette(string[] paletteIndex)
        {
            Project project = _projectService.GetProject();

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

                rgbPalette[j] = project.RgbPalette[colorIndex];
            }

            return rgbPalette;
        }

        public void CommitRgbPalette(Color[] rgbPalette)
        {
            _projectService.GetProject().RgbPalette = rgbPalette;
            CacheRgbPalettes();
            _eventService.Emit(SpotlightEventType.RgbColorsUpdated);
        }

        public void ExportRgbPalette(string fileName)
        {
            byte[] outputBytes = new byte[3 * 0x40];
            int byteCounter = 0;
            foreach (Color c in _projectService.GetProject().RgbPalette)
            {
                outputBytes[byteCounter++] = c.R;
                outputBytes[byteCounter++] = c.G;
                outputBytes[byteCounter++] = c.B;
            }

            File.WriteAllBytes(fileName, outputBytes);
        }
    }
}