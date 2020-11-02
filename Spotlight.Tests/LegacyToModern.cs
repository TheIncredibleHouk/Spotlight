using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Spotlight.Services;
using Spotlight.Models;
using System.Collections.Generic;
using System.Drawing;

namespace Spotlight.Tests
{
    [TestClass]
    public class LegacyToModern
    {
        [TestMethod]
        public void ConvertService()
        {
            string legacyFilePath = @"F:\ROM Hacking\Mario Adventure 3\Mario Adventure 3 Project - Main\Reuben.rbn";
            //string tsaLegacyPath = @"F:\ROM Hacking\Mario Adventure 3\Mario Adventure 3 Project - Main\Reuben.tsa";
            //string spriteLegacyPath = @"F:\ROM Hacking\Mario Adventure 3\Mario Adventure 3 Project - Main\sprites.xml";
            //string paletteLegacyPath = @"F:\ROM Hacking\Mario Adventure 3\Mario Adventure 3 Project - Main\default.pal";
            string newFilePath = @"F:\ROM Hacking\Mario Adventure 3\Mushroom Mayhem";


            ErrorService es = new ErrorService();
            ProjectService ps = new ProjectService(es);


            LegacyProject legacyProject = ps.GetLegacyProject(legacyFilePath);
            //Project project = ps.ConvertProject(legacyProject);

            //TileService ts = new TileService(es, project);
            //GameObjectService gos = new GameObjectService(es, project);


            //foreach(var gameObject in gos.ConvertFromLegacy(spriteLegacyPath))
            //{
            //    project.GameObjects[gameObject.GameId] = gameObject;
            //}

            //project.TileSets = ts.ConvertLegacy(tsaLegacyPath);
            //project.Name = "Mushroom Mayhem";
            //project.RomFilePath = newFilePath;
            //project.RgbPalette = ps.GetLegacyPalette(paletteLegacyPath);


            ////ps.SaveProject(project, newFilePath);

            Project reloadProject = ps.LoadProject(newFilePath + @"\Mushroom Mayhem.json");
            //reloadProject.TileSets = ts.ConvertLegacy(tsaLegacyPath);
            //ps.SaveProject(reloadProject, newFilePath);
            //List<GameObject> gameObjects= new GameObjectService(es, reloadProject).ConvertFromLegacy(spriteLegacyPath);

            //reloadProject.GameObjects = new GameObject[256];

            //foreach(GameObject gameObject in gameObjects)
            //{
            //    reloadProject.GameObjects[gameObject.GameId] = gameObject;
            //}

            //ps.SaveProject(reloadProject, newFilePath);
            WorldService ws = new WorldService(es, reloadProject);
            foreach (World w in ws.ConvertFromLegacy(ws.GetLegacyWorlds(@"F:\ROM Hacking\Mario Adventure 3\Mario Adventure 3 Project - Main\Worlds", legacyProject.worldinfo), legacyProject))
            {
                ws.SaveWorld(w, newFilePath);
            }

            //LevelService ls = new LevelService(es, reloadProject);
            //foreach (Level l in ls.ConvertFromLegacy(ls.GetLegacyLevels(@"F:\ROM Hacking\Mario Adventure 3\Mario Adventure 3 Project - Main\Levels", legacyProject.levelinfo), legacyProject))
            //{
            //    ls.SaveLevel(l, newFilePath);
            //}
        }
    }
}
