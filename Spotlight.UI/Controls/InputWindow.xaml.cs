using System.Windows;
using System.Windows.Input;

namespace Spotlight
{
    /// <summary>
    /// Interaction logic for InputWindow.xaml
    /// </summary>
    public partial class InputWindow : Window
    {
        public static string GetInput(string message, string defaultText = "")
        {
            InputWindow window = new InputWindow();
            window.DisplayText.Text = message;
            window.InputText = defaultText;
            //window.Owner = GlobalPanels.MainWindow;
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            window.ShowDialog();
            return window.InputText;
        }

        public InputWindow()
        {
            InitializeComponent();
        }

        public string InputText
        {
            get
            {
                return InputTextbox.Text;
            }
            set
            {
                InputTextbox.Text = value;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            InputText = null;
            this.Close();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                InputText = "";
                this.Close();
            }
        }

        private void InputTextbox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.Close();
            }
        }
    }
}