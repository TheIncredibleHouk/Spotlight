using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotlight.Models
{
    public class ClipboardItem
    {
        public object Data { get; private set; }
        public ClipBoardItemType Type { get; private set; }

        public ClipboardItem(object data, ClipBoardItemType type)
        {
            Data = data;
            Type = type;
        }
    }

}
