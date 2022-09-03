using Spotlight.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Spotlight.Services
{
    public class RomService
    {

        private Dictionary<Guid, byte> _levelIndexTable;
        private Dictionary<Guid, byte> _worldIndexTable;
        private Dictionary<Guid, byte> _paletteIndexTable;
        private Dictionary<byte, int> _levelAddressTable;
        private Dictionary<byte, int> _levelTypeTable;

        public string ErrorMessage { get; private set; }

        private Rom _rom;
        private int _dataPointer;

        private GraphicsService _graphicsService;
        private PalettesService _palettesService;
        private TileService _tileService;
        private LevelService _levelService;
        private WorldService _worldService;
        private TextService _textService;
        private CompressionService _compressionService;
        private ErrorService _errorService;

        public RomService(ErrorService errorService, GraphicsService graphicsService, PalettesService palettesService, TileService tileService, LevelService levelService, WorldService worldService, TextService textService)
        {
            _errorService = errorService;
            _levelIndexTable = new Dictionary<Guid, byte>();
            _worldIndexTable = new Dictionary<Guid, byte>();
            _paletteIndexTable = new Dictionary<Guid, byte>();
            _levelAddressTable = new Dictionary<byte, int>();
            _levelTypeTable = new Dictionary<byte, int>();

            _graphicsService = graphicsService;
            _palettesService = palettesService;
            _tileService = tileService;
            _worldService = worldService;
            _textService = textService;
            _levelService = levelService;
            _compressionService = new CompressionService();
        }

        private void WriteGraphics()
        {
            int address = 0x80010;
            byte[] graphicsData = _graphicsService.GetData();

            for (int i = 0; i < graphicsData.Length; i++)
            {
                _rom[address + i] = graphicsData[i];
            }
        }

        public RomInfo CompileRom(string fileName)
        {
            RomInfo romInfo = new RomInfo();
            _rom = new Rom();
            if (!_rom.Load(fileName))
            {
                throw new Exception("Unable to load rom.");
            }

            WritePalettes(_palettesService.GetPalettes());
            WriteTileBlockData();

            _dataPointer = 0x24010;
            CompileWorlds();

            _dataPointer = 0x40010;
            _dataPointer = CompileLevels();

            WriteGraphics();
            _rom.Save();
            romInfo.LevelAddressEnd = _dataPointer;
            romInfo.SpaceRemaining = 0x7C00F - _dataPointer;
            romInfo.LevelsUsed = _levelService.AllLevels().Count;

            return romInfo;
        }

        private int CompileLevels()
        {
            string region = "";

            try
            {
                _levelIndexTable.Clear();
                _levelTypeTable.Clear();
                _levelAddressTable.Clear();

                byte levelIndex = 0;
                foreach (LevelInfo levelInfo in _levelService.AllLevels())
                {
                    _levelIndexTable.Add(levelInfo.Id, levelIndex++);
                }

                levelIndex = 0;

                foreach (LevelInfo levelInfo in _levelService.AllLevels())
                {
                    region = "Loading level " + levelInfo.Name;

                    Level level = _levelService.LoadLevel(levelInfo);

                    _levelTypeTable[levelIndex++] = level.TileSetIndex;

                    if (level != null)
                    {
                        _levelAddressTable.Add(_levelIndexTable[level.Id], _dataPointer);

                        _dataPointer = WriteLevel(level, _dataPointer);
                        levelInfo.Size = level.CompressedData.Length;
                        if (_dataPointer >= 0xFC000)
                        {
                            return -1;
                        }
                    }
                    else
                    {
                        _errorService.LogError("Unable to load level " + levelInfo.Name);
                    }
                }

                int bank, address, lastLevelPointer = _dataPointer;

                foreach (var index in _levelAddressTable.Keys)
                {
                    region = "Updating level pointer table.";

                    _dataPointer = _levelAddressTable[index];
                    bank = (byte)((_dataPointer - 0x10) / 0x2000);
                    address = (_dataPointer - 0x10 - (bank * 0x2000) + 0xA000);
                    _rom[0xDC10 + (index * 4)] = (byte)bank;
                    _rom[0xDC11 + (index * 4)] = (byte)(address & 0x00FF);
                    _rom[0xDC12 + (index * 4)] = (byte)((address & 0xFF00) >> 8);
                    _rom[0xDC13 + (index * 4)] = (byte)_levelTypeTable[index];
                }

                return lastLevelPointer;
            }
            catch (Exception e)
            {
                _errorService.LogError(e, string.Format("Error occurred in writing level data data. Region: {0}\n Address: {1}", region, _dataPointer));
            }

            return -1;

        }

        private bool CompileWorlds()
        {
            string region = "";
            World world = null;
            int bank, address;

            _worldIndexTable.Clear();

            try
            {

                foreach (WorldInfo worldInfo in _worldService.AllWorlds().OrderBy(w => w.Number))
                {
                    if (worldInfo.Name != "No World")
                    {
                        region = "Loading world";
                        world = _worldService.LoadWorld(worldInfo);

                        if (world != null)
                        {
                            _worldIndexTable.Add(worldInfo.Id, (byte)worldInfo.Number);

                            bank = (byte)(_dataPointer / 0x2000);
                            address = (_dataPointer - 0x10 - (bank * 0x2000) + 0xA000);

                            _rom[0x0D810 + ((worldInfo.Number) * 4)] = (byte)bank;
                            _rom[0x0D811 + ((worldInfo.Number) * 4)] = (byte)(address & 0x00FF);

                            _rom[0x0D812 + ((worldInfo.Number) * 4)] = (byte)((address & 0xFF00) >> 8);

                            region = "Writing world";

                            _dataPointer = WriteWorld(world, _dataPointer);

                            if (_dataPointer > 0x26010)
                            {
                                throw new Exception("World data overflow!");
                            }

                            if (_dataPointer >= 0xFC000)
                            {
                                return false;

                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _errorService.LogError(e, string.Format("Error occurred in writing world data. Region: {0}\n Address: {1}\nWorld:{2}", region, _dataPointer, world.Name));
                return false;
            }


            return true;
        }

        public int WriteLevel(Level level, int levelAddress)
        {
            string region = "";

            try
            {
                string name = level.Name.ToUpper();
                int nameTrimIndex = name.IndexOf("-");
                if(nameTrimIndex > -1)
                {
                    name = name.Substring(0, nameTrimIndex);
                }

                if (name.Length > 0x22)
                {
                    name = name.Substring(0, 0x22);
                }
                else
                {
                    name = name.PadRight(0x22, ' ');
                }

                int yStart = 0;
                yStart = level.StartY - 1;

                region = "Writing level header";
                _rom[levelAddress++] = (byte)level.MostCommonTile;
                _rom[levelAddress++] = (byte)level.StaticTileTableIndex;
                _rom[levelAddress++] = (byte)_paletteIndexTable[level.PaletteId];
                _rom[levelAddress++] = (byte)((level.AnimationType << 6) | (level.ScreenLength - 1));
                _rom[levelAddress++] = (byte)(byte)(((level.StartX & 0x0F) << 4) | ((level.StartX & 0xF0) >> 4)); ;
                _rom[levelAddress++] = (byte)(byte)(((yStart & 0x0F) << 4) | ((yStart & 0xF0) >> 4)); ;
                _rom[levelAddress++] = (byte)(level.MusicValue);
                _rom[levelAddress++] = (byte)0;
                _rom[levelAddress++] = (byte)((level.LevelPointers.Count << 4) | level.ScrollType);
                _rom[levelAddress++] = (byte)(level.Effects | level.PaletteEffect);
                _rom[levelAddress++] = (byte)level.EventType;
                _rom[levelAddress++] = (byte)0;
                _rom[levelAddress++] = (byte)0;

                region = "Writing level name";
                for (int i = 0; i < 0x22; i++)
                {
                    _rom[levelAddress++] = (byte)name[i];
                }

                region = "Writing level pointers";

                foreach (var pointer in level.LevelPointers.OrderBy(pt => pt.X).ThenBy(pt => pt.Y))
                {
                    if (pointer.ExitsLevel)
                    {
                        //_rom[levelAddress++] = (byte)(pointer.ExitsLevel ? 1 : 0);
                        if (_worldIndexTable.ContainsKey(pointer.LevelId))
                        {
                            _rom[levelAddress++] = _worldIndexTable[pointer.LevelId];
                        }
                        else
                        {
                            _rom[levelAddress++] = 0;
                        }
                    }
                    else
                    {
                        if (_levelIndexTable.ContainsKey(pointer.LevelId))
                        {
                            _rom[levelAddress++] = _levelIndexTable[pointer.LevelId];
                        }
                        else
                        {
                            _rom[levelAddress++] = 0;
                        }
                    }

                    int yExit = pointer.ExitY;
                    if (!pointer.ExitsLevel)
                    {
                        switch (pointer.ExitActionType)
                        {
                            default:
                                yExit = pointer.ExitY;
                                break;
                            case 0:
                            case 2:
                            case 3:
                            case 4:
                                yExit = pointer.ExitY - 1;
                                break;


                        }
                    }

                    _rom[levelAddress++] = (byte)pointer.X;
                    _rom[levelAddress++] = (byte)pointer.Y;
                    _rom[levelAddress++] = (byte)(((pointer.ExitX & 0x0F) << 4) | ((pointer.ExitX & 0xF0) >> 4));
                    if (pointer.ExitsLevel)
                    {
                        yExit += 2;
                    }

                    _rom[levelAddress++] = (byte)(((yExit & 0x0F) << 4) | ((yExit & 0xF0) >> 4));
                    _rom[levelAddress++] = (byte)((pointer.ExitsLevel ? 0x80 : 0x00) | (pointer.RedrawsLevel ? 0x40 : 0x00) | (pointer.KeepObjects ? 0x20 : 0x00) | (pointer.DisableWeather ? 0x10 : 0x00) | (pointer.ExitActionType));
                }

                region = "Compressing level data";

                byte[] levelData = level.CompressedData ?? _compressionService.CompressLevel(level);
                
                if (level.CompressedData == null)
                {
                    level.CompressedData = levelData;
                    _levelService.SaveLevel(level);
                }

                region = "Writing compressed level data";
                for (int i = 0; i < levelData.Length; i++)
                {
                    _rom[levelAddress++] = levelData[i];
                }

                _rom[levelAddress++] = (byte)0xFF;

                region = "Writing object data";
                foreach (LevelObject levelObject in level.FirstObjectData.OrderBy(s => s.X).ThenBy(s => s.Y))
                {
                    _rom[levelAddress++] = (byte)levelObject.GameObjectId;
                    _rom[levelAddress++] = (byte)levelObject.X;
                    _rom[levelAddress++] = (byte)((levelObject.Property << 5) | levelObject.Y);
                }

                _rom[levelAddress++] = (byte) GameObject.SecondQuestDivider.GameId;

                foreach (LevelObject levelObject in level.SecondObjectData.OrderBy(s => s.X).ThenBy(s => s.Y))
                {
                    _rom[levelAddress++] = (byte)levelObject.GameObjectId;
                    _rom[levelAddress++] = (byte)levelObject.X;
                    _rom[levelAddress++] = (byte)((levelObject.Property << 5) | levelObject.Y);
                }

                _rom[levelAddress++] = 0xFF;
                return levelAddress;
            }
            catch (Exception e)
            {
                _errorService.LogError(e, string.Format("Error occurred in writing a level. Region: {0}\n Address: {1}\nLevel: {2}", region, levelAddress, level.Name));
            }

            return 0;
        }


        public int WriteWorld(World world, int levelAddress)
        {
            string region = "";

            try
            {
                region = "Writing header data";
                _rom[levelAddress++] = (byte)world.TileTableIndex;
                _rom[levelAddress++] = (byte)_paletteIndexTable[world.PaletteId];
                _rom[levelAddress++] = (byte)(world.MusicValue);
                _rom[levelAddress++] = (byte)world.ScreenLength;
                _rom[levelAddress++] = (byte)world.Pointers.Count;

                region = "Writing world pointers";
                foreach (WorldPointer worldPointer in world.Pointers)
                {
                    if (!_levelIndexTable.ContainsKey(worldPointer.LevelId))
                    {
                        _rom[levelAddress++] = 0x00;
                    }
                    else
                    {
                        _rom[levelAddress++] = _levelIndexTable[worldPointer.LevelId];
                    }
                    _rom[levelAddress++] = (byte)(((worldPointer.X & 0xF0) >> 4) | ((worldPointer.X & 0x0F) << 4));
                    _rom[levelAddress++] = (byte)(worldPointer.Y + 2);
                }

                region = "Writing world object data";

                foreach(WorldObject worldObject in world.ObjectData)
                {
                    _rom[levelAddress++] = (byte)(worldObject.GameObjectId - 0xC7);
                    _rom[levelAddress++] = (byte)(worldObject.Y + 2);
                    _rom[levelAddress++] = (byte)(worldObject.X);
                }

                _rom[levelAddress++] = (byte)0xFF;

                region = "Compressing world data";
                byte[] worldData = world.CompressedData ?? _compressionService.CompressWorld(world);

                if (world.CompressedData == null)
                {
                    world.CompressedData = worldData;
                    _worldService.SaveWorld(world);
                }

                region = "Writing compressed world data";

                for (int i = 0; i < worldData.Length; i++)
                {
                    _rom[levelAddress++] = worldData[i];
                }

                _rom[levelAddress++] = (byte)0xFF;
                _rom[levelAddress++] = (byte)0xFF;
                return levelAddress;
            }
            catch (Exception e)
            {
                _errorService.LogError(e, string.Format("Error occurred in writing world data. Region: {0}\n Address: {1}\nWorld: {2}", region, levelAddress, world.Name));
            }

            return 0;
        }

        public bool WritePalettes(List<Palette> paletteList)
        {
            int address = 0x28010;
            byte paletteIndex = 0;
            Palette currentPalette;
            string region = "";

            try
            {

                foreach (Palette palette in paletteList)
                {
                    currentPalette = palette;
                    for (int i = 0; i < 32; i++)
                    {
                        _rom[address++] = (byte)palette.IndexedColors[i];
                    }

                    _paletteIndexTable[palette.Id] = paletteIndex++;
                }

                return true;
            }
            catch (Exception e)
            {
                _errorService.LogError(e, string.Format("Error occurred in writing palette data. Region: {0}\n Address: {1}\nPalette Name: {2}", region, address, paletteIndex));
            }

            return false;
        }


        private void WriteTileBlockData()
        {
            int dataPointer = 0x2C010;
            int tileSetIndex = -1;
            string region = "";

            try
            {
                List<TileSet> tileSets = _tileService.GetTileSets().ToList();

                for (int i = 0; i < 16; i++)
                {
                    region = "Writing block data for tile set " + i;

                    byte[] blockData = tileSets[i].GetTileData();
                    for (int j = 0; j < blockData.Length; j++)
                    {
                        _rom[dataPointer++] = (byte)blockData[j];
                    }
                }

                dataPointer = 0xC010;

                byte[] tilePropertyData = _tileService.GetTilePropertyData();

                for (int i = 0; i < 0x1000; i++)
                {
                    region = "Writing tile property data";

                    _rom[dataPointer++] = tilePropertyData[i];
                }

                for (int i = 1; i < 16; i++)
                {
                    dataPointer = i * 0x40 + 0xD010;
                    tileSetIndex = i;

                    region = "Writing tile interactons for tile set " + i;

                    TileSet currentTileSet = tileSets[i];
                    for (int j = 0; j < 8; j++)
                    {
                        if (j < currentTileSet.FireBallInteractions.Count)
                        {
                            _rom[dataPointer++] = (byte)currentTileSet.FireBallInteractions[j];
                        }
                        else
                        {
                            _rom[dataPointer++] = 0x00;
                        }
                    }

                    for (int j = 0; j < 8; j++)
                    {
                        if (j < currentTileSet.IceBallInteractions.Count)
                        {
                            _rom[dataPointer++] = (byte)currentTileSet.IceBallInteractions[j];
                        }
                        else
                        {
                            _rom[dataPointer++] = 0x00;
                        }
                    }

                    for (int j = 0; j < 8; j++)
                    {
                        if (j < currentTileSet.PSwitchAlterations.Count)
                        {
                            _rom[dataPointer++] = (byte)currentTileSet.PSwitchAlterations[j].From;
                            _rom[dataPointer++] = (byte)currentTileSet.PSwitchAlterations[j].To;
                        }
                        else
                        {
                            _rom[dataPointer++] = 0x00;
                            _rom[dataPointer++] = 0x00;
                        }
                    }

                    _rom[dataPointer++] = 0x00;
                    _rom[dataPointer++] = 0x00;
                    _rom[dataPointer++] = 0x00;
                    _rom[dataPointer++] = 0x00;
                    _rom[dataPointer++] = 0x00;
                    _rom[dataPointer++] = 0x00;
                    _rom[dataPointer++] = 0x00;
                    _rom[dataPointer++] = 0x00;
                    _rom[dataPointer++] = 0x00;
                    _rom[dataPointer++] = 0x00;
                }
            }
            catch (Exception e)
            {
                _errorService.LogError(e, string.Format("Error occurred in writing tile block data. Region: {0}\n Address: {1}\nTile Set: {2}", region, dataPointer, tileSetIndex));
            }
        }
    }
}
