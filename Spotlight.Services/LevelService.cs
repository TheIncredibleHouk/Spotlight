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

        public Level LoadLevel(LevelInfo levelInfo)
        {
            try
            {
                string fileName = _project.DirectoryPath + @"\levels\" + levelInfo.Name + ".json";
                var level = JsonConvert.DeserializeObject<Level>(File.ReadAllText(fileName));
                level.ObjectData.ForEach(o => o.GameObject = _project.GameObjects[o.GameObjectId]);
                return level;
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

                File.WriteAllText(string.Format(@"{0}\{1}.json", LevelDirectory, Level.Name), JsonConvert.SerializeObject(Level));
            }
            catch (Exception e)
            {
                _errorService.LogError(e);
            }
        }
    }
}
