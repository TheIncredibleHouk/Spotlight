using Microsoft.Extensions.DependencyInjection;
using Spotlight.Abstractions;
using Spotlight.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Spotlight
{
    /// <summary>
    /// Interaction logic for TextPanel.xaml
    /// </summary>
    public partial class TextPanel : UserControl
    {
        private ITextService _textService;
        private IProjectService _projectService;

        public Guid Id { get; private set; }

        public TextPanel()
        {
            InitializeComponent();
            InitializeServices();
            InitializeUI();

            Id = Guid.NewGuid();
        }

        private void InitializeServices()
        {
            _projectService = App.Services.GetService<IProjectService>();
            _textService = App.Services.GetService<ITextService>();
        }

        private void InitializeUI()
        {
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
            List<KeyValuePair<string, string>> textTable = new List<KeyValuePair<string, string>>();

            foreach (KeyValueTextbox kvTextbox in KeyValueList.Children)
            {
                textTable.Add(kvTextbox.KeyValue);
            }

            _textService.SetTable((string)TextTables.SelectedItem, textTable);
            _projectService.SaveProject();
        }

        private void KvTextbox_DeleteButtonClicked(KeyValueTextbox sender)
        {
            KeyValueList.Children.Remove(sender);
        }

        private void NewButton_Click(object sender, RoutedEventArgs e)
        {
            string inputText = InputWindow.GetInput("Text Table Name");

            if (!string.IsNullOrWhiteSpace(inputText))
            {
                _textService.AddTable(inputText);
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