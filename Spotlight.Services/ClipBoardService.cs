using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotlight.Services
{
    public class ClipBoardService
    {
        public ClipBoardItem ClipBoardItem { get; set; }

        public void Clear()
        {
            ClipBoardItem = null;
        }
    }

    public class ClipBoardItem
    {
        public object Data { get; private set; }
        public ClipBoardItemType Type { get; private set; }

        public ClipBoardItem(object data, ClipBoardItemType type)
        {
            Data = data;
            Type = type;
        }
    }

    public enum ClipBoardItemType
    {
        TileBuffer,
        SpriteBuffer
    }
}
