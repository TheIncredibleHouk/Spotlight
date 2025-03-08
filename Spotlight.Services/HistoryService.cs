using Spotlight.Abstractions;
using Spotlight.Models;
using System.Collections.Generic;

namespace Spotlight.Services
{
    public class HistoryService : IHistoryService
    {
        public HistoryService()
        {
            UndoTiles = new Stack<TileChange>();
            RedoTiles = new Stack<TileChange>();
            UndoLevelObjects = new Stack<LevelObjectChange>();
            RedoLevelObjects = new Stack<LevelObjectChange>();
            UndoWorldObjects = new Stack<WorldObjectChange>();
            RedoWorldObjects = new Stack<WorldObjectChange>();
        }

        public Stack<TileChange> UndoTiles { get; private set; }
        public Stack<TileChange> RedoTiles { get; private set; }

        public Stack<LevelObjectChange> UndoLevelObjects { get; private set; }
        public Stack<LevelObjectChange> RedoLevelObjects { get; private set; }

        public Stack<WorldObjectChange> UndoWorldObjects { get; private set; }
        public Stack<WorldObjectChange> RedoWorldObjects { get; private set; }
    }
}