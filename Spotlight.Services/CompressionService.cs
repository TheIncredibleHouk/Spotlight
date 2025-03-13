using Spotlight.Abstractions;
using Spotlight.Models;
using Spotlight.Services;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Spotlight
{
    public class CompressionService : ICompressionService
    {
        private Level _level;
        private ILevelDataManager _levelDataManager;
        private IWorldDataManager _worldDataManager;
        private static CompressionPoint _currentPoint, _savedPoint;

        public CompressionService(ILevelDataManager levelDataManager, IWorldDataManager worldDataManager)
        {
            _levelDataManager = levelDataManager;
            _worldDataManager = worldDataManager;
        }
        public byte[] CompressLevel(Level level)
        {
            List<byte> returnBytes = new List<byte>();
            CompressionPoint restoreToPoint = new CompressionPoint();
            CompressionCommand currentCommand = null;
            CompressionCommand attemptCommand = null;
            CompressionCommand useCommand = null;

            ResetPoint();

            _level = level;
            _levelDataManager.Initialize(level);

            while (!_currentPoint.EOD)
            {
                // we're assuming writeraw, if we find a better command, we'll stop writeraw and use the better command
                SavePoint();

                useCommand = null;
                attemptCommand = TryPattern();

                if (attemptCommand != null)
                {
                    useCommand = attemptCommand;
                    restoreToPoint = _currentPoint;
                }

                RestorePoint();

                attemptCommand = TryRepeat();

                if (attemptCommand != null)
                {
                    if (useCommand != null)
                    {
                        if (useCommand.GetData().Length > attemptCommand.GetData().Length)
                        {
                            useCommand = attemptCommand;
                            restoreToPoint = _currentPoint;
                        }
                    }
                    else
                    {
                        useCommand = attemptCommand;
                        restoreToPoint = _currentPoint;
                    }
                }

                RestorePoint();
                attemptCommand = TrySkip();
                if (attemptCommand != null)
                {
                    if (useCommand != null)
                    {
                        if (useCommand.GetData().Length > attemptCommand.GetData().Length)
                        {
                            useCommand = attemptCommand;
                            restoreToPoint = _currentPoint;
                        }
                    }
                    else
                    {
                        useCommand = attemptCommand;
                        restoreToPoint = _currentPoint;
                    }
                }

                if (useCommand != null)
                {
                    if (currentCommand != null)
                    {
                        returnBytes.AddRange(currentCommand.GetData());
                        currentCommand = null;
                    }

                    returnBytes.AddRange(useCommand.GetData());
                    _currentPoint = restoreToPoint;
                    continue;
                }

                // made it here, we need to write raw
                RestorePoint();
                if (currentCommand == null)
                {
                    currentCommand = new CompressionCommand();
                    currentCommand.CommandType = CompressionCommandType.WriteRaw;
                }

                currentCommand.Data.Add(NextByte());
                SavePoint();

                if (currentCommand.Data.Count == 0x40)
                {
                    returnBytes.AddRange(currentCommand.GetData());
                    currentCommand = null;
                    SavePoint();
                }
            }

            if (currentCommand != null)
            {
                returnBytes.AddRange(currentCommand.GetData());
            }
            return returnBytes.ToArray();
        }

        public byte[] CompressWorld(World world)
        {
            _worldDataManager.Initialize(world);
            byte[] data = new byte[9 * 16 * world.ScreenLength];
            int counter = 0;

            for (int p = 0; p < world.ScreenLength; p++)
            {
                for (int y = 0; y < World.BLOCK_HEIGHT; y++)
                {
                    for (int x = 0; x < 16; x++)
                    {
                        data[counter++] = (byte)_worldDataManager.GetData((p * 16) + x, y);
                    }
                }
            }

            return data;
        }
        private CompressionCommand TrySkip()
        {
            byte repeatTile;
            int repeatCount = 0;
            repeatTile = (byte)_level.MostCommonTile;
            while (repeatTile == NextByte() && repeatCount < 0x40)
            {
                repeatCount++;
            }

            // no well repeatable tiles, return null
            if (repeatCount < 2)
            {
                return null;
            }

            CompressionCommand c = new CompressionCommand();
            c.CommandType = CompressionCommandType.SkipTile;
            c.RepeatTimes = repeatCount;

            if (!_currentPoint.EOD)
            {
                PreviousByte();
            }
            return c;
        }

        private CompressionCommand TryRepeat()
        {
            byte repeatTile;
            int repeatCount = 1;
            repeatTile = NextByte();
            while (!_currentPoint.EOD && repeatTile == NextByte() && repeatCount < 0x40)
            {
                repeatCount++;
            }

            // no well repeatable tiles, return null
            if (repeatCount == 1)
            {
                return null;
            }

            CompressionCommand c = new CompressionCommand();
            c.Data.Add(repeatTile);
            c.CommandType = CompressionCommandType.RepeatTile;
            c.RepeatTimes = repeatCount;

            if (!_currentPoint.EOD)
            {
                PreviousByte();
            }
            return c;
        }

        // the most complicated of the commands, we test to see if any patterns exist. if no patterns exist within 16 tiles, we return null
        private CompressionCommand TryPattern()
        {
            byte[] patternChunk = null;
            byte[] smallestChunk = null;
            bool hasMatch = false;
            CompressionCommand command = new CompressionCommand();
            CompressionPoint localPoint = _currentPoint;
            command.CommandType = CompressionCommandType.RepeatPattern;

            // basically, try patterns up to 16 in size. We want the smallest pattern that can be repeated
            // we break at 1 as a pattern of 1 is a RepeatTile command
            for (int i = 16; !_currentPoint.EOD && i > 1; i--)
            {
                patternChunk = new byte[i];

                // get pattern
                for (int j = 0; !_currentPoint.EOD && j < i; j++)
                {
                    patternChunk[j] = NextByte();
                }

                // assume there is a match, if no match, set false and break
                if (!_currentPoint.EOD)
                {
                    hasMatch = true;
                }

                for (int k = 0; !_currentPoint.EOD && k < i; k++)
                {
                    if (patternChunk[k] != NextByte())
                    {
                        hasMatch = false;
                        break;
                    }
                }

                if (hasMatch)
                {
                    // we have a match, set as smallest matchableChunk
                    smallestChunk = patternChunk;
                }

                // reset pointer before trying the next pattern
                RestorePoint();
            }

            // no smallestChunk then there was no discernable pattern
            if (smallestChunk == null)
            {
                return null;
            }

            // ok so we DO have a smallest chunk, let's get the number of times this repeats
            int repeatCount = 0;
            RestorePoint();

            // for a pattern repeat to exist we have to repeat at least twice, so 0x00 = 2 repeats, 0x03 = 6 repeats
            bool noRepeat = false;
            while (repeatCount < 0x3F && !noRepeat)
            {
                localPoint = _currentPoint;
                for (int i = 0; i < smallestChunk.Length; i++)
                {
                    if (smallestChunk[i] != NextByte())
                    {
                        noRepeat = true;

                        break;
                    }
                }

                if (!noRepeat)
                {
                    // we made it here, so one pattern was found, yay!
                    repeatCount++;
                    localPoint = _currentPoint;
                }
            }

            // return pointer back to the point that we tried last match
            _currentPoint = localPoint;

            command.Data.AddRange(smallestChunk);
            command.RepeatTimes = repeatCount;

            return command;
        }

        private void ResetPoint()
        {
            _currentPoint = new CompressionPoint();
        }

        private void SavePoint()
        {
            _savedPoint = _currentPoint;
        }

        private void RestorePoint()
        {
            _currentPoint = _savedPoint;
        }

        private byte NextByte()
        {
            if (_currentPoint.EOD)
            {
                return 0xFF;
            }

            int data = _levelDataManager.GetData(_currentPoint.XPointer, _currentPoint.YPointer);

            _currentPoint.XPointer++;
            if (_currentPoint.XPointer >= 0x10 * _level.ScreenLength)
            {
                _currentPoint.XPointer = 0;
                _currentPoint.YPointer++;
                if (_currentPoint.YPointer >= 27)
                {
                    _currentPoint.EOD = true;
                }
            }

            return (byte)data;
        }

        private void PreviousByte()
        {
            _currentPoint.XPointer--;
            if (_currentPoint.XPointer < 0)
            {
                _currentPoint.XPointer = (_level.ScreenLength * 0x10) - 1;
                _currentPoint.YPointer--;
                _currentPoint.EOD = false;
            }
        }
    }
}