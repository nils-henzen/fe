using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;

namespace FeChat
{
    public partial class ComposePanel : UserControl
    {
        private TextBox? _recipientTextBox;
        private TextBox? _messageTextBox;
        private Button? _sendButton;
        private Button? _cancelButton;
        private Button? _selectFileButton;
        private Button? _pasteFileButton;
        private TextBlock? _selectedFileTextBlock;
        private string? _selectedFilePath;

        public event EventHandler<(string recipient, string message)>? SendClicked;
        public event EventHandler<(string recipient, string filePath)>? SendFileClicked;
        public event EventHandler? CancelClicked;

        public ComposePanel()
        {
            AvaloniaXamlLoader.Load(this);
            _recipientTextBox = this.FindControl<TextBox>("RecipientTextBox");
            _messageTextBox = this.FindControl<TextBox>("MessageTextBox");
            _sendButton = this.FindControl<Button>("SendButton");
            _cancelButton = this.FindControl<Button>("CancelButton");
            _selectFileButton = this.FindControl<Button>("SelectFileButton");
            _pasteFileButton = this.FindControl<Button>("PasteFileButton");
            _selectedFileTextBlock = this.FindControl<TextBlock>("SelectedFileTextBlock");

            if (_sendButton != null)
                _sendButton.Click += OnSendClicked;
            if (_cancelButton != null)
                _cancelButton.Click += OnCancelClicked;
            if (_selectFileButton != null)
                _selectFileButton.Click += OnSelectFileClicked;
            if (_pasteFileButton != null)
                _pasteFileButton.Click += OnPasteFileClicked;
        }

        private void OnSendClicked(object? sender, RoutedEventArgs e)
        {
            var recipient = _recipientTextBox?.Text ?? string.Empty;
            if (!string.IsNullOrEmpty(_selectedFilePath))
            {
                SendFileClicked?.Invoke(this, (recipient, _selectedFilePath));
                return;
            }
            var message = _messageTextBox?.Text ?? string.Empty;
            SendClicked?.Invoke(this, (recipient, message));
        }

        private async void OnSelectFileClicked(object? sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.AllowMultiple = false;
            var result = await dialog.ShowAsync(GetWindow());
            if (result != null && result.Length > 0)
            {
                _selectedFilePath = result[0];
                _selectedFileTextBlock!.Text = $"Selected: {System.IO.Path.GetFileName(_selectedFilePath)}";
            }
        }

        private async void OnPasteFileClicked(object? sender, RoutedEventArgs e)
        {
            if (_selectedFileTextBlock != null)
                _selectedFileTextBlock.Text = "File paste not supported on Windows.";
        }

        private Window? GetWindow()
        {
            return this.VisualRoot as Window;
        }

        private void OnCancelClicked(object? sender, RoutedEventArgs e)
        {
            CancelClicked?.Invoke(this, EventArgs.Empty);
        }

        public void Clear()
        {
            if (_recipientTextBox != null) _recipientTextBox.Text = string.Empty;
            if (_messageTextBox != null) _messageTextBox.Text = string.Empty;
            _selectedFilePath = null;
            if (_selectedFileTextBlock != null) _selectedFileTextBlock.Text = string.Empty;
        }
    }
}
