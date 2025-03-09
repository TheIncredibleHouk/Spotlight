using Newtonsoft.Json;
using Spotlight.Abstractions;
using Spotlight.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Spotlight.Services
{
    public class TileService : ITileService
    {
        private readonly IErrorService _errorService;
        private readonly IProjectService _projectService;
        private readonly IEventService _eventService;

        public TileService(IErrorService errorService, IProjectService projectService, IEventService eventService)
        {
            _errorService = errorService;
            _projectService = projectService;
            _eventService = eventService;
        }

        public IEnumerable<TileSet> GetTileSets()
        {
            return _projectService.GetProject().TileSets;
        }

        public void CommitTileSet(int index, TileSet tileSet, List<TileTerrain> tileTerrain, List<MapTileInteraction> mapTileInterations)
        {
            Project project = _projectService.GetProject();

            project.TileSets[index].FireBallInteractions = tileSet.FireBallInteractions;
            project.TileSets[index].IceBallInteractions = tileSet.IceBallInteractions;
            project.TileSets[index].PSwitchAlterations = tileSet.PSwitchAlterations;

            for (int i = 0; i < 256; i++)
            {
                project.TileSets[index].TileBlocks[i] = tileSet.TileBlocks[i];
            }

            for (int i = 0; i < project.TileTerrain.Count; i++)
            {
                project.TileTerrain[i] = tileTerrain[i];
            }

            for (int i = 0; i < project.MapTileInteractions.Count; i++)
            {
                project.MapTileInteractions[i] = mapTileInterations[i];
            }

            _eventService.Emit(SpotlightEventType.TileSetUpdated, tileSet);
        }

        public byte[] GetTilePropertyData()
        {
            return _projectService.GetProject().TileSets.SelectMany(t => t.TileBlocks).Select(b => (byte)b.Property).ToArray();
        }

        public TileSet GetTileSet(int tileSetIndex)
        {
            return _projectService.GetProject().TileSets[tileSetIndex];
        }

        public List<TileTerrain> GetTerrain()
        {
            return _projectService.GetProject().TileTerrain;
        }

        public List<TileTerrain> GetTerrainCopy()
        {
            return JsonConvert.DeserializeObject<List<TileTerrain>>(JsonConvert.SerializeObject(_projectService.GetProject().TileTerrain));
        }

        public List<MapTileInteraction> GetMapTileInteractions()
        {
            return _projectService.GetProject().MapTileInteractions;
        }

        public List<MapTileInteraction> GetMapTileInteractionCopy()
        {
            return JsonConvert.DeserializeObject<List<MapTileInteraction>>(JsonConvert.SerializeObject(_projectService.GetProject().MapTileInteractions));
        }
    }
}