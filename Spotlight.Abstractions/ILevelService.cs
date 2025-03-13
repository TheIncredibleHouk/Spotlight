using Spotlight.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotlight.Abstractions
{
    public interface ILevelService
    {
        void AddLevel(Level level, WorldInfo worldInfo);
        void RemoveLevel(LevelInfo info);
        List<IInfo> GetAllWorldsAndLevels(LevelInfo currentLevel = null);
        List<LevelInfo> AllLevels();
        Level LoadLevel(LevelInfo levelInfo);
        LevelInfo RenameLevel(LevelInfo levelInfo, string newName);
        void SaveLevel(Level level, string basePath = null, bool asTemp = false);
        void GenerateMetaData(LevelInfo levelInfo, MemoryStream thumbnailStream = null);
        List<LevelInfo> FlattenLevelInfos(List<LevelInfo> levelInfos);
    }
}
