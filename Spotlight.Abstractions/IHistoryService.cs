using Spotlight.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotlight.Abstractions
{
    public interface IHistoryService
    {
        public Stack<TileChange> UndoTiles { get; }
        public Stack<TileChange> RedoTiles { get; }

        public Stack<LevelObjectChange> UndoLevelObjects { get; }
        public Stack<LevelObjectChange> RedoLevelObjects { get; }

        public Stack<WorldObjectChange> UndoWorldObjects { get; }
        public Stack<WorldObjectChange> RedoWorldObjects { get; }
    }
}
