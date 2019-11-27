using Spotlight.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotlight.Services
{
    public class TileService
    {

        private readonly ErrorService _errorService;
        private readonly Project _project;

        public TileService(ErrorService errorService, Project project)
        {
            _errorService = errorService;
            _project = project;
        }

        public string GetTileDescription(byte tileInteractionValue)
        {
            string tileDescription = _project.TileInteractions.Where(t => t.Value == tileInteractionValue).Select(t => t.Name).FirstOrDefault();
            if (string.IsNullOrWhiteSpace(tileDescription))
            {
                int stateValue = 0xF0 & tileInteractionValue;
                int interactionValue = 0x0F & tileInteractionValue;

                tileDescription = _project.TileStates.Where(s => s.Value == stateValue).Select(s => s.Name).FirstOrDefault() + " " + _project.TileInteractions.Where(s => s.Value == interactionValue).Select(s => s.Name).FirstOrDefault();
            }

            return tileDescription ?? "";
        }

        public List<TileSet> ConvertLegacy(string fileName)
        {
            try
            {
                var data = File.ReadAllBytes(fileName);
                var tileSets = new List<TileSet>();

                for (int i = 0; i < 16; i++)
                {
                    int bankOffset = i * 0x400;
                    var tileSet = new TileSet();

                    for (int j = 0; j < 256; j++)
                    {
                        Tile tile = new Tile();

                        tile.UpperLeft = data[bankOffset + j];
                        tile.UpperRight = data[bankOffset + 0x100 + j];
                        tile.LowerLeft = data[bankOffset + 0x200 + j];
                        tile.LowerRight = data[bankOffset + 0x300 + j];

                        tileSet.Tiles.Add(tile);
                    }

                    tileSets.Add(tileSet);
                }

                var propertyOffset = 0x4000;
                for (int i = 0; i < 16; i++)
                {
                    for (int j = 0; j < 256; j++)
                    {
                        tileSets[i].Tiles[j].Property = data[propertyOffset++];
                    }
                }


                for (int i = 0; i < 16; i++)
                {
                    for (int k = 0; k < 8; k++)
                    {
                        tileSets[i].FireBallInteractions.Add(data[propertyOffset++]);
                    }

                    for (int k = 0; k < 8; k++)
                    {
                        tileSets[i].IceBallInteractions.Add(data[propertyOffset++]);
                    }

                    for (int k = 0; k < 8; k++)
                    {
                        tileSets[i].PSwitchAlterations.Add(new PSwitchAlteration(data[propertyOffset++], data[propertyOffset++]));
                    }
                }

                return tileSets;
            }
            catch (Exception e)
            {
                _errorService.LogError(e);
                return null;
            }
        }
    }
}
