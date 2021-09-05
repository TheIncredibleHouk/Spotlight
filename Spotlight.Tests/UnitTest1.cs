using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Spotlight.Models;
using Spotlight.Services;

namespace Spotlight.Tests
{
    [TestClass]
    public class Jobs
    {
        [TestMethod]
        public void ConvertPalettes()
        {
            
            ProjectService projectService = new ProjectService(new ErrorService());
            Project project = projectService.LoadProject(@"C:\Projects\Mario Adventure 3\Mushroom Mayhem\Mushroom Mayhem.json");
            LevelService levelService = new LevelService(new ErrorService(), project);
            WorldService worldService = new WorldService(new ErrorService(), project);
            PalettesService palettesService = new PalettesService(new ErrorService(), project);
            var palettes = palettesService.GetPalettes();

            foreach(IInfo info in levelService.AllWorldsLevels())
            {
                if(info.InfoType == InfoType.Level)
                {
                    LevelInfo levelInfo = info as LevelInfo;
                    Level level = levelService.LoadLevel(levelInfo);
                    Palette palette = palettes.Where(p => p.Id == level.PaletteId).FirstOrDefault();
                    if (!palette.Renamed)
                    {
                        palette.Name = level.Name;
                        palette.Renamed = true;
                        palettesService.CommitPalette(palette);
                    }
                }
                
                if(info.InfoType == InfoType.World)
                {
                    WorldInfo worldInfo = info as WorldInfo;
                    World world = worldService.LoadWorld(worldInfo);
                    Palette palette = palettes.Where(p => p.Id == world.PaletteId).FirstOrDefault();
                    if (!palette.Renamed)
                    {
                        palette.Name = world.Name;
                        palette.Renamed = true;
                        palettesService.CommitPalette(palette);
                    }
                }
            }

            projectService.SaveProject();
        }
    }
}
