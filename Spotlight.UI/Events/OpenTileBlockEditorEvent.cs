using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotlight.UI.Events
{
    public class OpenTileBlockEditorEvent
    {
        public Guid LevelId { get; set; }
        public int BlockId { get; set; }
    }
}
