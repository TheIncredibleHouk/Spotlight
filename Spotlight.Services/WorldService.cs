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
    public class WorldService
    {
        private readonly ErrorService _errorService;
        private readonly Project _project;

        public delegate void WorldUpdatedEventHandler(WorldInfo worldInfo);

        public event WorldUpdatedEventHandler WorldUpdated;

        public WorldService(ErrorService errorService, Project project)
        {
            _errorService = errorService;
            _project = project;
        }

        public List<WorldInfo> AllWorlds()
        {
            List<WorldInfo> worldList = _project.WorldInfo.ToList();
            worldList.Add(_project.EmptyWorld);
            return worldList;
        }

        public void NotifyUpdate(WorldInfo worldInfo)
        {
            if (WorldUpdated != null)
            {
                WorldUpdated(worldInfo);
            }
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

                File.WriteAllText(string.Format(@"{0}\{1}.json", worldDirectory, world.Name), JsonConvert.SerializeObject(world, Newtonsoft.Json.Formatting.Indented));
            }
            catch (Exception e)
            {
                _errorService.LogError(e);
            }
        }

        public void RenameWorld(string previousName, string newName)
        {
            string safePriorLevelName = previousName.Replace("!", "").Replace("?", "");
            string priorFileName = string.Format(@"{0}\{1}.json", _project.DirectoryPath + @"\worlds", safePriorLevelName);

            World world = JsonConvert.DeserializeObject<World>(File.ReadAllText(priorFileName));
            world.Name = newName;
            SaveWorld(world);

            File.Delete(priorFileName);
        }

        public void GenerateMetaData(TileService tileService, WorldInfo worldInfo, MemoryStream thumbnailStream = null)
        {
            World world = LoadWorld(worldInfo);
            TileSet tileSet = tileService.GetTileSet(0);
            List<TileTerrain> tileTerrain = tileService.GetTerrain();
            WorldMetaData worldMetaData = new WorldMetaData();


            worldMetaData.TilesUsed = world.TileData.Distinct().ToList();
            worldMetaData.ThumbnailImage = thumbnailStream?.ToArray() ?? worldInfo.MetaData.ThumbnailImage;

            worldInfo.MetaData = worldMetaData;
        }
    }
}