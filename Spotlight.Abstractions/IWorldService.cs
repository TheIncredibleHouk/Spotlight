using Spotlight.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotlight.Abstractions
{
    public interface IWorldService
    {
        List<WorldInfo> AllWorlds();
        World LoadWorld(WorldInfo worldInfo);
        void SaveWorld(World world, string basePath = null);
        WorldInfo RenameWorld(WorldInfo worldInfo, string newName);
        void GenerateMetaData(WorldInfo worldInfo, MemoryStream thumbnailStream = null);
    }
}
