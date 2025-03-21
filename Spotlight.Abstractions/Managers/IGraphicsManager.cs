﻿using Spotlight.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotlight.Abstractions
{
    public interface IGraphicsManager
    {
        public void Initialize(Tile[] graphicsTable);
        public void Initialize(Tile[] globalTable, Tile[] overlayTable);
        public void Initialize(Tile[] topTable, Tile[] bottomTable, Tile[] globalTable, Tile[] overlayTable);
        public Tile GetAbsoluteTile(int tileTableIndex, int tileIndex);
        public Tile GetRelativeTile(int tileIndex);
        public Tile GetOverlayTile(int bank, int tileIndex);
        public void SetFullTable(Tile[] fullTable);
        public void SetTopTable(Tile[] staticTable);
        public void SetBottomTable(Tile[] animatedTable);
        public void SetGlobalTiles(Tile[] globalTable, Tile[] extraTable);
    }
}
