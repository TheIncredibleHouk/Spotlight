using Newtonsoft.Json;
using Spotlight.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace Spotlight.Services
{
    public class LevelService
    {
        private readonly ErrorService _errorService;
        private readonly Project _project;

        public delegate void LevelUpdatedEventHandler(LevelInfo levelInfo);
        public event LevelUpdatedEventHandler LevelUpdated;

        public delegate void LevelsUpdatedHandler(LevelInfo newLevel);
        public event LevelsUpdatedHandler LevelsUpdated;

        public LevelService(ErrorService errorService, Project project)
        {
            _errorService = errorService;
            _project = project;
        }

        public void AddLevel(Level level, WorldInfo worldInfo)
        {
            SaveLevel(level);

            LevelInfo levelInfo = new LevelInfo();
            levelInfo.Id = level.Id;
            levelInfo.Name = level.Name;
            levelInfo.SublevelsInfo = new List<LevelInfo>();

            worldInfo.LevelsInfo.Add(levelInfo);
            LevelsUpdated(levelInfo);
        }

        public List<IInfo> AllWorldsLevels()
        {
            List<IInfo> infos = new List<IInfo>();
            List<WorldInfo> worldInfos = _project.WorldInfo.ToList();

            worldInfos.Add(_project.EmptyWorld);

            foreach (var world in worldInfos)
            {
                infos.Add(world);

                foreach (var level in world.LevelsInfo)
                {
                    infos.Add(level);

                    foreach (var sublevel1 in level.SublevelsInfo ?? new List<LevelInfo>())
                    {
                        infos.Add(sublevel1);

                        foreach (var sublevel2 in sublevel1.SublevelsInfo ?? new List<LevelInfo>())
                        {
                            infos.Add(sublevel2);

                            foreach (var sublevel3 in sublevel2.SublevelsInfo ?? new List<LevelInfo>())
                            {
                                infos.Add(sublevel2);

                                foreach (var sublevel4 in sublevel3.SublevelsInfo ?? new List<LevelInfo>())
                                {
                                    infos.Add(sublevel4);

                                    foreach (var sublevel5 in sublevel4.SublevelsInfo ?? new List<LevelInfo>())
                                    {
                                        infos.Add(sublevel5);

                                        foreach (var sublevel6 in sublevel5.SublevelsInfo ?? new List<LevelInfo>())
                                        {
                                            infos.Add(sublevel6);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return infos;
        }

        public void NotifyUpdate(LevelInfo levelInfo)
        {
            if (LevelUpdated != null)
            {
                LevelUpdated(levelInfo);
            }
        }

        public List<LevelInfo> AllLevels()
        {
            List<LevelInfo> levelInfos = new List<LevelInfo>();
            List<WorldInfo> worldInfos = _project.WorldInfo.ToList();
            worldInfos.Add(_project.EmptyWorld);
            foreach (var worldInfo in worldInfos)
            {
                levelInfos.AddRange(FlattenLevelInfos(worldInfo.LevelsInfo));
            }

            return levelInfos;
        }

        public List<LevelInfo> FlattenLevelInfos(List<LevelInfo> levelInfos)
        {
            List<LevelInfo> returnLevelInfos = new List<LevelInfo>();
            foreach (var levelInfo in levelInfos)
            {
                if (levelInfo.SublevelsInfo != null && levelInfo.SublevelsInfo.Count > 0)
                {
                    returnLevelInfos.AddRange(levelInfo.SublevelsInfo);
                }

                returnLevelInfos.Add(levelInfo);
            }

            return returnLevelInfos;
        }

        public Level LoadLevel(LevelInfo levelInfo)
        {
            try
            {
                string safeFileName = levelInfo.Name.Replace("!", "").Replace("?", "");

                string fileName = _project.DirectoryPath + @"\levels\" + safeFileName + ".json";
                return JsonConvert.DeserializeObject<Level>(File.ReadAllText(fileName));
            }
            catch (Exception e)
            {
                _errorService.LogError(e);
                return null;
            }
        }

        public void RenameLevel(string previousName, string newName)
        {
            string safePriorLevelName = previousName.Replace("!", "").Replace("?", "");
            string priorFileName = string.Format(@"{0}\{1}.json", _project.DirectoryPath + @"\levels", safePriorLevelName);

            Level level = JsonConvert.DeserializeObject<Level>(File.ReadAllText(priorFileName));
            level.Name = newName;
            SaveLevel(level);

            File.Delete(priorFileName);
        }

        public void SaveLevel(Level Level, string basePath = null)
        {
            try
            {
                if (basePath == null)
                {
                    basePath = _project.DirectoryPath;
                }

                string LevelDirectory = basePath + @"\levels";

                if (!Directory.Exists(LevelDirectory))
                {
                    Directory.CreateDirectory(LevelDirectory);
                }

                string safeFileName = Level.Name.Replace("!", "").Replace("?", "");

                File.WriteAllText(string.Format(@"{0}\{1}.json", LevelDirectory, safeFileName), JsonConvert.SerializeObject(Level, Newtonsoft.Json.Formatting.Indented));
            }
            catch (Exception e)
            {
                _errorService.LogError(e);
            }
        }
    }
}