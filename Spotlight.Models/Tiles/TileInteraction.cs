namespace Spotlight.Models
{
    public class TileInteraction
    {
        public string Name { get; set; }
        public int Value { get; set; }
        public TileBlockOverlay Overlay { get; set; }

        public static int Mask = 0x0F;

        public bool HasInteraction(int property)
        {
            return Value == (int)(property & Mask);
        }

        public TileInteraction()
        {
        }
    }
}