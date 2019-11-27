using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Spotlight.Services;
using Spotlight.Models;

namespace Spotlight.Tests
{
    [TestClass]
    public class LegacyToModern
    {
        [TestMethod]
        public void ConvertService()
        {
            string legacyFilePath = @"F:\ROM Hacking\Mario Adventure 3\Mario Adventure 3 Project - Main\Reuben.rbn";
            string tsaLegacyPath = @"F:\ROM Hacking\Mario Adventure 3\Mario Adventure 3 Project - Main\Reuben.tsa";
            string spriteLegacyPath = @"F:\ROM Hacking\Mario Adventure 3\Mario Adventure 3 Project - Main\sprites.xml";
            string newFilePath = @"F:\ROM Hacking\Mario Adventure 3\Mushroom Mayhem\Mushroom Mayhem.json";
            

            ErrorService es = new ErrorService();
            ProjectService ps = new ProjectService(es);
            

            LegacyProject legacyProject = ps.GetLegacyProject(legacyFilePath);
            Project project = ps.ConvertProject(legacyProject);

            TileService ts = new TileService(es, project);
            GameObjectService gos = new GameObjectService(es);


            project.ObjectClasses = gos.ConvertFromLegacy(spriteLegacyPath);
            project.TileSets = ts.ConvertLegacy(tsaLegacyPath);
            project.Name = "Mushroom Mayhem";
            project.Path = newFilePath;

            ps.SaveProject(project);

            Project reloadProject = ps.LoadProject(newFilePath);

            Assert.IsTrue(reloadProject.Name == project.Name);
            Assert.IsTrue(reloadProject.Palettes.Count == project.Palettes.Count);
        }
    }
}
