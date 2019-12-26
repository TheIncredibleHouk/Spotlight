using Spotlight.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotlight.Services
{
    public class GraphicsService
    {
        private ErrorService _errorService;
        private List<Tile> _tiles;
        private Project _project;

        public GraphicsService(ErrorService errorService, Project project)
        {
            _errorService = errorService;
            _project = project;
            LoadGraphics(project);
        }

        public void LoadGraphics(Project project)
        {
            string fileName = project.DirectoryPath + @"\" + project.Name + @".chr";

            byte[] graphicsData = File.ReadAllBytes(fileName);
            _tiles = new List<Tile>();


            int dataPointer = 0;

            for (int i = 0; i < 256; i++)
            {

                for (int j = 0; j < 64; j++)
                {
                    byte[] nextTileChunk = new byte[16];
                    for (int k = 0; k < 16; k++)
                    {
                        nextTileChunk[k] = graphicsData[dataPointer++];
                    }

                    _tiles.Add(new Tile(nextTileChunk));
                }
            }
        }

        public Tile[] GetGlobalTiles()
        {
            return _tiles.ToArray();
        }

        public Tile[] GetTileSection(int tileTableIndex)
        {
            return _tiles.Skip(tileTableIndex * 0x40).Take(0x80).ToArray();
        }

        public Tile GetTile(int tileTableIndex, int tileIndex)
        {
            return _tiles[tileTableIndex * 0x40 + tileIndex];   
        }

        public Palette GetPalette(int paletteIndex)
        {
            return _project.Palettes[paletteIndex];
        }

        public List<string> GetPaletteNames()
        {
            return _project.Palettes.Select(p => p.Name).ToList();
        }
    }
}
