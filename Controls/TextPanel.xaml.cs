using Spotlight.Services;
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
    /// Interaction logic for TextPanel.xaml
    /// </summary>
    public partial class TextPanel : UserControl
    {
        private TextService _textService;

        public TextPanel()
        {
            InitializeComponent();
        }

        public TextPanel(TextService textService)
        {
            InitializeComponent();

            _textService = textService;
            TextTables.ItemsSource = _textService.TableNames();
            NewKeyValueButton.IsEnabled = false;
        }

        private void TextTables_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            KeyValueList.Children.Clear();

            foreach (var textbox in _textService.GetTable((string)TextTables.SelectedItem).Select(kv => new KeyValueTextbox(kv.Key, kv.Value)))
            {
                KeyValueList.Children.Add(textbox);
                textbox.DeleteButtonClicked += KvTextbox_DeleteButtonClicked;
            }

            NewKeyValueButton.IsEnabled = true;
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            var textTable = _textService.GetTable((string)TextTables.SelectedItem);
            textTable.Clear();

            foreach(KeyValueTextbox kvTextbox in KeyValueList.Children)
            {
                textTable.Add(kvTextbox.KeyValue);
                
            }
        }

        private void KvTextbox_DeleteButtonClicked(KeyValueTextbox sender)
        {
            KeyValueList.Children.Remove(sender);
            NewKeyValueButton.IsEnabled = false;
        }

        private void NewButton_Click(object sender, RoutedEventArgs e)
        {
            InputWindow inputWindow = new InputWindow();
            inputWindow.DisplayText = "Text Table Name";
            inputWindow.ShowDialog();

            if (!string.IsNullOrWhiteSpace(inputWindow.InputText))
            {
                _textService.AddTable(inputWindow.InputText);
                var newTables = _textService.TableNames();
                TextTables.ItemsSource = newTables;
                TextTables.SelectedItem = newTables[newTables.Count - 1];
            }
        }

        private void NewKeyValueButton_Click(object sender, RoutedEventArgs e)
        {
            var kvTextBox = new KeyValueTextbox();
            KeyValueList.Children.Add(kvTextBox);
            kvTextBox.DeleteButtonClicked += KvTextbox_DeleteButtonClicked;
        }
    }
}
