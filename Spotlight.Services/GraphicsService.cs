using Spotlight.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Spotlight.Abstractions;

namespace Spotlight.Services
{
    public class GraphicsService : IGraphicsService
    {
        private List<Tile> _tiles;
        private List<Tile> _extraTiles;
        private IProjectService _projectService;
        private byte[] _graphicsData;

        private DateTime _lastGraphicsUpdate;
        private DateTime _lastExtraGraphicsUpdated;

        public delegate void GraphicsUpdatedHandler();

        public event GraphicsUpdatedHandler GraphicsUpdated;

        public event GraphicsUpdatedHandler ExtraGraphicsUpdated;

        public GraphicsService(IProjectService projectService)
        {
            _projectService = projectService;
            LoadGraphics();
            LoadExtraGraphics();
        }

        public void CheckGraphics()
        {
            Project project = _projectService.GetProject();
            string fileName = project.DirectoryPath + @"\" + project.Name + @".chr";
            string extraFileName = project.DirectoryPath + @"\" + project.Name + @".extra.chr";

            if (File.GetLastWriteTime(fileName) > _lastGraphicsUpdate)
            {
                LoadGraphics();
                if (GraphicsUpdated != null)
                {
                    GraphicsUpdated();
                }
            }

            if (File.GetLastWriteTime(extraFileName) > _lastExtraGraphicsUpdated)
            {
                LoadExtraGraphics();
                if (ExtraGraphicsUpdated != null)
                {
                    ExtraGraphicsUpdated();
                }
            }
        }

        public Color[][] GetRgbPalette(Palette palette)
        {
            Project project = _projectService.GetProject();
            Color[][] rgbPalette = new Color[4][];
            for (int i = 0; i < 4; i++)
            {
                rgbPalette[i] = new Color[4];
                for (int j = 0; j < 4; j++)
                {
                    rgbPalette[i][j] = project.RgbPalette[palette.IndexedColors[(i * 4) + j]];
                }
            }

            return rgbPalette;
        }

        public Color[] GetRgbPalette(string[] paletteIndex)
        {
            Project project = _projectService.GetProject();
            Color[] rgbPalette = new Color[4];
            for (int j = 0; j < 4; j++)
            {
                rgbPalette[j] = project.RgbPalette[Int32.Parse(paletteIndex[j], System.Globalization.NumberStyles.HexNumber)];
            }

            return rgbPalette;
        }

        public byte[] GetData()
        {
            return _graphicsData;
        }

        public void LoadGraphics()
        {
            Project project = _projectService.GetProject();
            string fileName = project.DirectoryPath + @"\" + project.Name + @".chr";

            _lastGraphicsUpdate = File.GetLastWriteTime(fileName);

            _graphicsData = File.ReadAllBytes(fileName);
            _tiles = new List<Tile>();

            int dataPointer = 0;

            for (int i = 0; i < 256; i++)
            {
                for (int j = 0; j < 64; j++)
                {
                    byte[] nextTileChunk = new byte[16];
                    for (int k = 0; k < 16; k++)
                    {
                        nextTileChunk[k] = _graphicsData[dataPointer++];
                    }

                    _tiles.Add(new Tile(nextTileChunk));
                }
            }
        }

        public void LoadExtraGraphics()
        {
            Project project = _projectService.GetProject();
            string extraFileName = project.DirectoryPath + @"\" + project.Name + @".extra.chr";

            _lastExtraGraphicsUpdated = File.GetLastWriteTime(extraFileName);

            byte[] extraGraphicsData = File.ReadAllBytes(extraFileName);
            _extraTiles = new List<Tile>();

            int dataPointer = 0;

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

        public Tile[] GetExtraTilesAtAddress(int address)
        {
            int tilesSkipped = address / 16;
            return _extraTiles.Skip(tilesSkipped).Take(256).ToArray();
        }

        public Tile GetExtraTileAtAddress(int address, int col, int row )
        {
            int tilesSkipped = address / 16 + (row * 16) + col;
            return _extraTiles.Skip(tilesSkipped).FirstOrDefault();
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
            return _projectService.GetProject().Palettes[index];
        }

        public List<Color> GetColors()
        {
            return _projectService.GetProject().RgbPalette.ToList();
        }

        public Tile[] GetTilesAtAddress(int address)
        {
            int tilesSkipped = address / 16;
            return _tiles.Skip(tilesSkipped).Take(256).ToArray();
        }

        public Tile GetTileAtAddress(int address, int col, int row)
        {
            int tilesSkipped = address / 16 + (row * 16) + col;
            return _tiles.Skip(tilesSkipped).FirstOrDefault();
        }
    }
}