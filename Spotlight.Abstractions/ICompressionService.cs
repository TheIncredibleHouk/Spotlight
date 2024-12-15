using Spotlight.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotlight.Abstractions
{
    public interface ICompressionService
    {
        byte[] CompressLevel(Level level);
        byte[] CompressWorld(World world);
    }
}
