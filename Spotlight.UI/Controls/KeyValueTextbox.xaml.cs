using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Spotlight
{
    /// <summary>
    /// Interaction logic for KeyValueTextbox.xaml
    /// </summary>
    public partial class KeyValueTextbox : UserControl
    {
        public delegate void DeleteButtonEventHandler(KeyValueTextbox sender);

        public event DeleteButtonEventHandler DeleteButtonClicked;

        public KeyValuePair<string, string> KeyValue
        {
            get
            {
                return new KeyValuePair<string, string>(KeyTextbox.Text, ValueTextbox.Text);
            }
            set
            {
                KeyTextbox.Text = value.Key;
                ValueTextbox.Text = value.Value;
            }
        }

        public KeyValueTextbox()
        {
            InitializeComponent();
        }

        public KeyValueTextbox(string key, string value)
        {
            InitializeComponent();
            KeyValue = new KeyValuePair<string, string>(key, value);
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            DeleteButtonClicked(this);
        }
    }
}