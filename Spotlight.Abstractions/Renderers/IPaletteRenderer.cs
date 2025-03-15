using Spotlight.Models;
using System.Drawing;


namespace Spotlight.Abstractions
{
    public interface IPaletteRenderer
    {
        public byte[] GetRectangle(Rectangle rect);
        public void Initialize(PaletteType paletteType);
        public void Update(Palette palette);
        public void Update();
    }
}
