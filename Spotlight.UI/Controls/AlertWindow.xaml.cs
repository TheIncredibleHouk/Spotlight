using System.Windows;
using System.Windows.Input;

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
            OkButton.Focus();
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