using Spotlight.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Spotlight
{
    /// <summary>
    /// Interaction logic for PaletteItem.xaml
    /// </summary>
    public partial class PaletteItem : UserControl
    {
        public PaletteItem()
        {
            InitializeComponent();
        }

        private Color[] _colors;
        public Color[] Colors
        {
            get
            {
                return _colors;
            }
            set
            {
                _colors = value;
                Color1.Fill = new SolidColorBrush(value[0]);
                Color2.Fill = new SolidColorBrush(value[1]);
                Color3.Fill = new SolidColorBrush(value[2]);
                Color4.Fill = new SolidColorBrush(value[3]);
            }
        }
    }
}
