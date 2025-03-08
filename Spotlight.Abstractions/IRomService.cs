using Spotlight.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotlight.Abstractions
{
    public interface IRomService
    {
        public RomInfo CompileRom(string fileName);
        //public int WriteLevel(Level level, int levelAddress, bool enterableFromWorld);
        //public int WriteWorld(World world, int levelAddress);
        //public bool WritePalettes(List<Palette> paletteList);
    }
}
