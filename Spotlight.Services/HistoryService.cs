using Spotlight.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotlight.Services
{
    public class HistoryService
    {
        public HistoryService()
        {
            UndoTiles = new Stack<TileChange>();
            RedoTiles = new Stack<TileChange>();
            UndoLevelObjects = new Stack<LevelObjectChange>();
            RedoLevelObjects = new Stack<LevelObjectChange>();
        }

        
        public Stack<TileChange> UndoTiles { get; private set; }
        public Stack<TileChange> RedoTiles { get; private set; }

        public Stack<LevelObjectChange> UndoLevelObjects { get; private set; }
        public Stack<LevelObjectChange> RedoLevelObjects { get; private set; }

    }
}
