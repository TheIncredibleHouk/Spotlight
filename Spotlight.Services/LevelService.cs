using Newtonsoft.Json;
using Spotlight.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Spotlight.Services
{
    public class LevelService
    {
        private readonly ErrorService _errorService;
        private readonly Project _project;

        public LevelService(ErrorService errorService, Project project)
        {
            _errorService = errorService;
            _project = project;
        }

        public List<Level> ConvertFromLegacy(List<LegacyLevel> legacyLevels, LegacyProject legacyProject)
        {
            return legacyLevels.Select(l => new Level()
            {
                AnimationType = int.Parse(l.animationtype),
                ClearTileIndex = int.Parse(l.clearvalue),
                EventType = int.Parse(l.misc1),
                GraphicsSet = int.Parse(l.graphicsbank, System.Globalization.NumberStyles.HexNumber),
                Id = Guid.Parse(l.guid),
                Name = l.name,
                MusicValue = int.Parse(LegacyLevel.MusicValues[int.Parse(l.music)], System.Globalization.NumberStyles.HexNumber),
                PaletteEffect = int.Parse(l.paletteeffect),
                PaletteId = legacyProject.paletteinfo[int.Parse(l.palette)].guid,
                Effects = (bool.Parse(l.invincibleenemies) ? 0x80 : 0) | (bool.Parse(l.tempprojeffects) ? 0x40 : 0) | (bool.Parse(l.rhythm) ? 0x20 : 0) | (bool.Parse(l.dpadtiles) ? 0x10 : 0),
                ScreenLength = int.Parse(l.length),
                ScrollType = int.Parse(l.scrolltype),
                StartX = int.Parse(l.xstart),
                StartY = int.Parse(l.ystart),
                StaticTileTableIndex = int.Parse(l.graphicsbank),
                TileSetIndex = int.Parse(l.type),
                TileData = l.leveldata.Split(',').Select(d => int.Parse(d)).ToArray(),
                LevelPointers = l.pointers.Select(p => new LevelPointer()
                {
                    DisableWeather = bool.Parse(p.disableweather),
                    ExitActionType = int.Parse(p.exittype),
                    ExitsLevel = bool.Parse(p.exitslevel),
                    ExitX = int.Parse(p.xexit),
                    ExitY = int.Parse(p.yexit),
                    KeepObjects = bool.Parse(p.keepobjects),
                    LevelId = Guid.Parse(p.levelguid),
                    RedrawsLevel = bool.Parse(p.redraw),
                    X = int.Parse(p.xenter),
                    Y = int.Parse(p.yenter)
                }).ToList(),
                ObjectData = l.spritedata.Select(s => new LevelObject()
                {
                    GameObjectId = int.Parse(s.value),
                    Property = int.Parse(s.property == "-1" ? "0" : s.property),
                    X = int.Parse(s.x),
                    Y = int.Parse(s.y)
                }).ToList()
            }).ToList();
        }

        public List<LegacyLevel> GetLegacyLevels(string basePath, List<LegacyLevelInfo> legacyLevelInfos)
        {
            List<LegacyLevel> legacyLevels = new List<LegacyLevel>();
            foreach (var legacyLevelInfo in legacyLevelInfos)
            {
                string filePath = basePath + @"\" + legacyLevelInfo.levelguid + ".lvl";
                if (File.Exists(filePath))
                {
                    using (FileStream fileStream = File.OpenRead(filePath))
                    {
                        using (XmlReader xmlReader = XmlReader.Create(fileStream))
                        {
                            try
                            {
                                LegacyLevel legacyLevel = ((LegacyLevel)new XmlSerializer(typeof(LegacyLevel)).Deserialize(xmlReader));
                                legacyLevel.name = legacyLevelInfo.name;
                                legacyLevels.Add(legacyLevel);
                            }
                            catch (Exception e)
                            {
                                _errorService.LogError(e);
                                return null;
                            }
                        }
                    }
                }
            }

            return legacyLevels;
        }

        public List<IInfo> AllWorldsLevels()
        {
            List<IInfo> infos = new List<IInfo>();
            foreach (var world in _project.WorldInfo)
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

        public List<WorldInfo> AllWorlds()
        {
            return _project.WorldInfo.ToList();
        }

        public List<LevelInfo> AllLevels()
        {
            List<LevelInfo> levelInfos = new List<LevelInfo>();
            foreach (var worldInfo in _project.WorldInfo)
            {
                levelInfos.AddRange(LevelInfoFromLevel(worldInfo.LevelsInfo));
            }

            return levelInfos;
        }

        private List<LevelInfo> LevelInfoFromLevel(List<LevelInfo> levelInfos)
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
                return  JsonConvert.DeserializeObject<Level>(File.ReadAllText(fileName));
            }
            catch (Exception e)
            {
                _errorService.LogError(e);
                return null;
            }
        }

        public void SaveLevel(Level Level, string basePath = null)
        {
            try
            {
                if(basePath == null)
                {
                    basePath = _project.DirectoryPath;
                }

                string LevelDirectory = basePath + @"\levels";

                if (!Directory.Exists(LevelDirectory))
                {
                    Directory.CreateDirectory(LevelDirectory);
                }

                string safeFileName = Level.Name.Replace("!", "").Replace("?", "");

                File.WriteAllText(string.Format(@"{0}\{1}.json", LevelDirectory, safeFileName), JsonConvert.SerializeObject(Level));
            }
            catch (Exception e)
            {
                _errorService.LogError(e);
            }
        }
    }
}
