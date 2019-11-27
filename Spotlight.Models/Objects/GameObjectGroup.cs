using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotlight.Models
{
    public class GameObjectGroup
    {
        public GameObjectGroup()
        {
            GameObjects = new List<GameObject>();
        }

        public string Name { get; set; }
        public List<GameObject> GameObjects { get; set; }
    }
}
