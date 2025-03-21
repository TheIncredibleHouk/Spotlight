﻿using Spotlight.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotlight.Abstractions
{
    public interface ILevelDataManager
    {
        public void Initialize(Level level, TileSet tileSet = null);
        public bool PSwitchActive { get; set; }
        public int GetData(int x, int y);
        public void SetData(int x, int y, int tileValue);
        public void ReplaceValue(int existingValue, int newValue);
        public List<LevelObject> GetLevelObjects(Rectangle area);
        public List<LevelPointer> GetPointers(Rectangle area);
    }
}
