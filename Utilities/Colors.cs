namespace Spotlight
{
    public static class ColorConverter
    {
        public static System.Windows.Media.Color ToMediaColor(this System.Drawing.Color color)
        {
            return new System.Windows.Media.Color() { R = color.R, G = color.G, B = color.B, A = color.A };
        }

        public static System.Windows.Media.Color[] ToMediaColor(this System.Drawing.Color[] colors)
        {
            System.Windows.Media.Color[] newColors = new System.Windows.Media.Color[colors.Length];
            for(int i =0; i < colors.Length; i++)
            {
                System.Drawing.Color color = colors[i];
                newColors[i] = new System.Windows.Media.Color() { R = color.R, G = color.G, B = color.B, A = color.A };
            }

            return newColors;
        }
    }
}