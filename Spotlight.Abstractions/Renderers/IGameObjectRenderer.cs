using Spotlight.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotlight.Abstractions
{
    public interface IGameObjectRenderer : IRenderer
    {
        public void Update(Palette palette);
        public void Update();
        public byte[] GetRectangle(Rectangle rect);
        public void Clear();
        public void Update(List<LevelObject> _levelObjects, bool withOverlays);
    }
}
