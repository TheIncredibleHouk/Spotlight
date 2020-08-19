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
    public partial class ConfirmationWindow : Window
    {
        public static System.Windows.Forms.DialogResult Confirm(string text)
        {
            ConfirmationWindow window = new ConfirmationWindow();
            window.DisplayText.Text  = text;
            window.Owner = GlobalPanels.MainWindow;
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            window.ShowDialog();
            return window.Result;
        }

        public ConfirmationWindow()
        {
            InitializeComponent();
        }

        public System.Windows.Forms.DialogResult Result { get; private set; }
        
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Result = System.Windows.Forms.DialogResult.No;
            this.Close();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Result = System.Windows.Forms.DialogResult.Yes;
            this.Close();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Result = System.Windows.Forms.DialogResult.Yes;
                this.Close();
            }
        }
    }
}
