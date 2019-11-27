using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotlight.Models
{
    public class TileInteraction
    {
        public string Name { get; private set; }
        public int Value { get; private set; }

        public TileInteraction(string name, int value)
        {
            Name = name;
            Value = value;
        }

    }
}
