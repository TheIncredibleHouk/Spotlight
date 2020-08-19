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
using System.Windows.Shapes;

namespace Spotlight
{
    /// <summary>
    /// Interaction logic for InputWindow.xaml
    /// </summary>
    public partial class AlertWindow : Window
    {
        public static void Alert(string text)
        {
            AlertWindow window = new AlertWindow();
            window.DisplayText.Text = text;
            window.Owner = GlobalPanels.MainWindow;
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            window.ShowDialog();
        }

        public AlertWindow()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close();
            }
            
            if (e.Key == Key.Enter)
            {
                this.Close();
            }
        }
    }
}
