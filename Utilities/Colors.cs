namespace Spotlight
{
    public static class ColorConverter
    {
        public static System.Windows.Media.Color ToMediaColor(this System.Drawing.Color color)
        {
            return new System.Windows.Media.Color() { R = color.R, G = color.G, B = color.B, A = color.A };
        }
    }
}