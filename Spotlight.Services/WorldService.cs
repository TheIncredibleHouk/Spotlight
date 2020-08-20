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
    public class WorldService
    {
        private readonly ErrorService _errorService;
        private readonly Project _project;

        public WorldService(ErrorService errorService, Project project)
        {
            _errorService = errorService;
            _project = project;
        }

        public List<World> ConvertFromLegacy(List<LegacyWorld> legacyWorld, LegacyProject legacyProject)
        {
            return legacyWorld.Select(w => new World()
            {
                ClearTileIndex = int.Parse(w.clearvalue),
                Id = Guid.Parse(w.guid),
                Name = w.name,
                MusicValue = int.Parse(w.music, System.Globalization.NumberStyles.HexNumber),
                PaletteId = legacyProject.paletteinfo[int.Parse(w.palette)].guid,
                Pointers = w.pointers.Select(p => new WorldPointer()
                {
                    LevelId = Guid.Parse(w.guid),
                    X = int.Parse(p.x),
                    Y = int.Parse(p.y)
                }).ToList(),
                ScreenLength = int.Parse(w.length),
                TileTableIndex = int.Parse(w.graphicsbank),
                TileData = w.worlddata.Split(',').Select(d => int.Parse(d)).ToArray()
            }).ToList();
        }

        public List<LegacyWorld> GetLegacyWorlds(string basePath, List<LegacyWorldInfo> legacyWorldInfos)
        {
            List<LegacyWorld> legacyWorlds = new List<LegacyWorld>();
            foreach (var legacyWorldInfo in legacyWorldInfos)
            {
                string filePath = basePath + @"\" + legacyWorldInfo.worldguid + ".map";
                if (File.Exists(filePath))
                {
                    using (FileStream fileStream = File.OpenRead(filePath))
                    {
                        using (XmlReader xmlReader = XmlReader.Create(fileStream))
                        {
                            try
                            {
                                LegacyWorld legacyWorld = ((LegacyWorld)new XmlSerializer(typeof(LegacyWorld)).Deserialize(xmlReader));
                                legacyWorld.name = legacyWorldInfo.name;
                                legacyWorlds.Add(legacyWorld);
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

            return legacyWorlds;
        }

        public World LoadWorld(WorldInfo worldInfo)
        {
            try
            {
                string fileName = _project.DirectoryPath + @"\worlds\" + worldInfo.Name + ".json";
                return JsonConvert.DeserializeObject<World>(File.ReadAllText(fileName));
            }
            catch (Exception e)
            {
                _errorService.LogError(e);
                return null;
            }
        }

        public void SaveWorld(World world, string basePath = null)
        {
            try
            {

                if (basePath == null)
                {
                    basePath = _project.DirectoryPath;
                }

                string worldDirectory = basePath + @"\worlds";

                if (!Directory.Exists(worldDirectory))
                {
                    Directory.CreateDirectory(worldDirectory);
                }

                File.WriteAllText(string.Format(@"{0}\{1}.json", worldDirectory, world.Name), JsonConvert.SerializeObject(world));
            }
            catch (Exception e)
            {
                _errorService.LogError(e);
            }
        }
    }
}
