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
            Project project = projectService.LoadProject(@"E:\Mushroom Mayhem\Game\Mushroom Mayhem\Mushroom Mayhem.json");
            LevelService levelService = new LevelService(new ErrorService(), project);

            foreach (IInfo info in levelService.AllWorldsLevels())
            {
                if (info.InfoType == InfoType.Level)
                {
                    LevelInfo levelInfo = info as LevelInfo;
                    Level level = levelService.LoadLevel(levelInfo);
                    level.FirstObjectData = level.ObjectData;
                    level.SecondObjectData = new List<LevelObject>();
                    levelService.SaveLevel(level);
                }
            }

            projectService.SaveProject();
        }
    }
}
