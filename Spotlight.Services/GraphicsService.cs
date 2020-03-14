using Spotlight.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
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
        private List<Tile> _extraTiles;
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
            
            string extraFileName = project.DirectoryPath + @"\" + project.Name + @".extra.chr";
            byte[] extraGraphicsData = File.ReadAllBytes(extraFileName);
            _extraTiles = new List<Tile>();

            dataPointer = 0;

            for (int i = 0; i < 12; i++)
            {

                for (int j = 0; j < 64; j++)
                {
                    byte[] nextTileChunk = new byte[16];
                    for (int k = 0; k < 16; k++)
                    {
                        nextTileChunk[k] = extraGraphicsData[dataPointer++];
                    }

                    _extraTiles.Add(new Tile(nextTileChunk));
                }
            }
        }

        public Tile[] GetGlobalTiles()
        {
            return _tiles.ToArray();
        }

        public Tile[] GetExtraTiles()
        {
            return _extraTiles.ToArray();
        }

        public Tile[] GetTileSection(int tileTableIndex)
        {
            return _tiles.Skip(tileTableIndex * 0x40).Take(0x80).ToArray();
        }

        public Tile GetTile(int tileTableIndex, int tileIndex)
        {
            return _tiles[tileTableIndex * 0x40 + tileIndex];   
        }

        public Palette GetPalette(int index)
        {
            return _project.Palettes[index];
        }

        public Palette GetPalette(Guid paletteId)
        {
            return _project.Palettes.Where(p => p.Id == paletteId).FirstOrDefault();
        }

        public List<Color> GetColors()
        {
            return _project.RgbPalette.ToList();
        }

        public List<Palette> GetPalettes()
        {
            return _project.Palettes.ToList();
        }
    }
}
