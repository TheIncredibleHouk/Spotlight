using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Spotlight
{
    /// <summary>
    /// Interaction logic for PaletteRGBEntry.xaml
    /// </summary>
    public partial class PaletteRgbEditor : UserControl
    {
        public PaletteRgbEditor()
        {
            InitializeComponent();
        }

        public int PaletteIndex
        {
            set
            {
                ColorHex.Text = value.ToString("X2");
            }
        }

        private System.Drawing.Color _rgbColor;
        public System.Drawing.Color RgbColor
        {
            get
            {
                return _rgbColor;
            }
            set
            {
                _rgbColor = value;
                R.Text = _rgbColor.R.ToString();
                G.Text = _rgbColor.G.ToString();
                B.Text = _rgbColor.B.ToString();

                UpdateColorPreview();
            }
        }

        private void UpdateColorPreview()
        {
            ColorPreview.Background = new SolidColorBrush(Color.FromRgb(RgbColor.R, RgbColor.G, RgbColor.B));
        }

        private void TextBlock_KeyUp(object sender, KeyEventArgs e)
        {
            byte r, g, b;

            if (byte.TryParse(R.Text, out r) &&
               byte.TryParse(G.Text, out g) &&
               byte.TryParse(B.Text, out b))
            {
                _rgbColor = System.Drawing.Color.FromArgb(r, g, b);
            }
            else
            {
                _rgbColor = System.Drawing.Color.Black;
            }

            UpdateColorPreview();
        }
    }
}
