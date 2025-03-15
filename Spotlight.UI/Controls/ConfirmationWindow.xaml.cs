using System.Windows;
using System.Windows.Input;
using System.Windows.Forms;

namespace Spotlight
{
    /// <summary>
    /// Interaction logic for InputWindow.xaml
    /// </summary>
    public partial class ConfirmationWindow : Window
    {
        public static DialogResult Confirm(string text)
        {
            ConfirmationWindow window = new ConfirmationWindow();
            window.DisplayText.Text = text;
            //window.Owner = GlobalPanels.MainWindow;
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

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Result = System.Windows.Forms.DialogResult.Yes;
                this.Close();
            }
        }
    }
}