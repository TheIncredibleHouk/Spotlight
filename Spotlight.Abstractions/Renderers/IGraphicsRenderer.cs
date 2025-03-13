using Spotlight.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotlight.Abstractions
{
    public interface IGraphicsRenderer
    {
        public void Update(Palette palette, int paletteIndex);
    }
}
