using Newtonsoft.Json;
using Spotlight.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
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

        private GameObjectService _gameObjectService;

        public LevelService(ErrorService errorService, Project project, GameObjectService gameObjectService)
        {
            _errorService = errorService;
            _project = project;
            _gameObjectService = gameObjectService;
        }

        public void AddLevel(Level level, WorldInfo worldInfo)
        {
            SaveLevel(level);

            LevelInfo levelInfo = new LevelInfo();
            levelInfo.Id = level.Id;
            levelInfo.Name = level.Name;
            levelInfo.SublevelsInfo = new List<LevelInfo>();
            levelInfo.ParentInfo = worldInfo;

            worldInfo.LevelsInfo.Add(levelInfo);

            LevelsUpdated(levelInfo);
        }

        public void RemoveLevel(LevelInfo info)
        {
            int levelIndex = info.ParentInfo.SublevelsInfo.IndexOf(info);
            info.ParentInfo.SublevelsInfo.Remove(info);
            info.ParentInfo.SublevelsInfo.InsertRange(levelIndex, info.SublevelsInfo ?? new List<LevelInfo>());

            string fileName = _project.DirectoryPath + SafeFileName(info);
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            if (LevelsUpdated != null)
            {
                LevelsUpdated(null);
            }
        }

        public List<IInfo> AllWorldsLevels(LevelInfo currentLevel = null)
        {
            List<IInfo> infos = new List<IInfo>();
            List<WorldInfo> worldInfos = _project.WorldInfo.ToList();

            worldInfos.Add(_project.EmptyWorld);

            foreach (var world in worldInfos)
            {
                if (world == _project.EmptyWorld)
                {
                    continue;
                }

                infos.Add(world);

                foreach (var level in world.LevelsInfo)
                {
                    infos.Add(level);
                    AddLevelInfos(infos, level);
                }
            }

            if (currentLevel != null)
            {
                infos.Remove(currentLevel);
            }

            return infos;
        }

        private void AddLevelInfos(List<IInfo> infos, LevelInfo level)
        {
            foreach (var sublevel in level.SublevelsInfo ?? new List<LevelInfo>())
            {
                infos.Add(sublevel);
                AddLevelInfos(infos, sublevel);
            }
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
                levelInfos.AddRange(FlattenLevelInfos(worldInfo.LevelsInfo).OrderBy(l => l.Name));
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
                    returnLevelInfos.AddRange(FlattenLevelInfos(levelInfo.SublevelsInfo));
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
                Level level = JsonConvert.DeserializeObject<Level>(File.ReadAllText(fileName));
                level.SwitchQuest(Level.LevelQuest.First);
                level.LevelPointers = level.LevelPointers.Where(pointer => pointer.LevelId != Guid.Empty).ToList();
                return level;
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

        private string SafeFileName(Level level)
        {
            return level.Name.Replace("!", "").Replace("?", "");
        }

        private string SafeFileName(LevelInfo levelInfo)
        {
            return levelInfo.Name.Replace("!", "").Replace("?", "");
        }

        public void SaveLevel(Level level, string basePath = null, bool asTemp = false)
        {
            try
            {
                if (basePath == null)
                {
                    basePath = _project.DirectoryPath;
                }

                string levelDirectory = basePath + @"\levels";

                if (!Directory.Exists(levelDirectory))
                {
                    Directory.CreateDirectory(levelDirectory);
                }

                string safeFileName = SafeFileName(level);

                if (asTemp)
                {
                    safeFileName = "~" + safeFileName;
                }

                File.WriteAllText(string.Format(@"{0}\{1}.json", levelDirectory, safeFileName), JsonConvert.SerializeObject(level, Newtonsoft.Json.Formatting.Indented));
            }
            catch (Exception e)
            {
                _errorService.LogError(e);
            }
        }

        public void ExportLevelToPng(PngBitmapEncoder encoder, Level level, string basePath = null)
        {
            try
            {
                if (basePath == null)
                {
                    basePath = _project.DirectoryPath;
                }

                string imageDirectory = basePath + @"\images";
                
                if (!Directory.Exists(imageDirectory))
                {
                    Directory.CreateDirectory(imageDirectory);
                }

                string safeFileName = SafeFileName(level);

                using (var stream = File.Create(string.Format(@"{0}\{1}.png", imageDirectory, safeFileName)))
                {
                    encoder.Save(stream);
                }
            }
            catch (Exception e)
            {
                _errorService.LogError(e);
            }
        }

        public void CleanUpTemp(Level level)
        {
            string tempFile = _project.DirectoryPath + SafeFileName(level);

            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }

        public void CleanUpTemps()
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(_project.DirectoryPath + @"\levels\");
            foreach (FileInfo fileInfo in directoryInfo.GetFiles())
            {
                if (fileInfo.Name.StartsWith("~"))
                {
                    File.Delete(fileInfo.FullName);
                }
            }
        }

        public IEnumerable<FileInfo> FindTemps()
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(_project.DirectoryPath + @"\levels\");
            return directoryInfo.GetFiles().Where(file => file.Name.StartsWith("~"));
        }

        public void SwapTemp(FileInfo tempFile)
        {
            string originalFile = _project.DirectoryPath + @"\levels\" + tempFile.Name.Substring(1);
            if (File.Exists(originalFile))
            {
                string backupFile = _project.DirectoryPath + @"\levels\" + tempFile.Name.Substring(1) + ".bak";
                if (File.Exists(backupFile))
                {
                    File.Delete(backupFile);
                }

                File.Move(originalFile, backupFile);
            }

            File.Move(tempFile.FullName, originalFile);
        }

        public void GenerateMetaData(TileService tileService, LevelInfo levelInfo, MemoryStream thumbnailStream = null)
        {
            Level level = LoadLevel(levelInfo);
            TileSet tileSet = tileService.GetTileSet(level.TileSetIndex);
            List<TileTerrain> tileTerrain = tileService.GetTerrain();
            LevelMetaData levelMeta = new LevelMetaData();

            List<int> gameObjectIds = level.ObjectData.Select(obj => obj.GameObjectId).Distinct().ToList();

            levelMeta.UniqueGameObjects = _gameObjectService.GetObjects(gameObjectIds).Select(gameObject => gameObject.Name).ToList();

            List<int> coinValues = new List<int>();
            List<int> cherryValues = new List<int>();
            List<int> itemBlockValues = new List<int>();
            Dictionary<int, string> itemBlockDescriptions = new Dictionary<int, string>();

            foreach (TileTerrain terrain in tileService.GetTerrain())
            {
                foreach (TileInteraction interaction in terrain.Interactions)
                {
                    if (interaction.Name.ToLower().Contains("coin"))
                    {
                        for (int index = 0; index < 256; index++)
                        {
                            if (tileSet.TileBlocks[index].Property == (terrain.Value | interaction.Value))
                            {
                                coinValues.Add(index);
                            }
                        }
                    }
                    else if (interaction.Name.ToLower().Contains("cherry"))
                    {
                        for (int index = 0; index < 256; index++)
                        {
                            if (tileSet.TileBlocks[index].Property == (terrain.Value | interaction.Value))
                            {
                                cherryValues.Add(index);
                            }
                        }
                    }
                    else if (terrain.Name.ToLower().Contains("item block") &&
                             !interaction.Name.ToLower().Contains("toggle") &&
                             !interaction.Name.ToLower().Contains("checkpoint") &&
                             !interaction.Name.ToLower().Contains("event") &&
                             !interaction.Name.ToLower().Contains("brick") &&
                             !interaction.Name.ToLower().Contains("vine") &&
                             !interaction.Name.ToLower().Contains("spinner") &&
                             !interaction.Name.ToLower().Contains("key"))
                    {
                        for (int index = 0; index < 256; index++)
                        {
                            if (tileSet.TileBlocks[index].Property == (terrain.Value | interaction.Value))
                            {
                                itemBlockValues.Add(index);
                                itemBlockDescriptions[index] = interaction.Name;
                            }
                        }
                    }
                }
            }

            List<string> powerUpList = new List<string>();

            foreach (int tileValue in level.TileData.Where(tile => itemBlockValues.Contains(tile)).Distinct().ToList())
            {
                powerUpList.Add(itemBlockDescriptions[tileValue]);
            }

            levelMeta.MaxCoinCount = level.TileData.Where(tile => coinValues.Contains(tile)).Count();
            levelMeta.PowerUps = powerUpList.Distinct().ToList();
            levelMeta.MaxCherryCount = level.TileData.Where(tile => cherryValues.Contains(tile)).Count();
            levelMeta.TilesUsed = level.TileData.Distinct().ToList();
            levelMeta.TileSet = level.TileSetIndex;
            levelMeta.ThumbnailImage = thumbnailStream?.ToArray() ?? levelInfo.LevelMetaData.ThumbnailImage;

            levelInfo.LevelMetaData = levelMeta;
        }
    }
}