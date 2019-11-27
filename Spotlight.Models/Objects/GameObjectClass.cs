using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotlight.Models
{
    public class GameObjectClass
    {
        public GameObjectClass()
        {
            Groups = new List<GameObjectGroup>();
        }
        public int Number { get; set; }
        public List<GameObjectGroup> Groups { get; set; }
    }
}
