using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;

namespace FeChat
{
    public partial class ComposePanel : UserControl
    {
        public event EventHandler<(string recipient, string message)>? SendClicked;
        public event EventHandler? CancelClicked;

        private TextBox? _recipientTextBox;
        private TextBox? _messageTextBox;
        private Button? _sendButton;
        private Button? _cancelButton;

        public ComposePanel()
        {
            AvaloniaXamlLoader.Load(this);
            _recipientTextBox = this.FindControl<TextBox>("RecipientTextBox");
            _messageTextBox = this.FindControl<TextBox>("MessageTextBox");
            _sendButton = this.FindControl<Button>("SendButton");
            _cancelButton = this.FindControl<Button>("CancelButton");

            if (_sendButton != null)
                _sendButton.Click += OnSendClicked;
            if (_cancelButton != null)
                _cancelButton.Click += OnCancelClicked;
        }

        private void OnSendClicked(object? sender, RoutedEventArgs e)
        {
            var recipient = _recipientTextBox?.Text ?? string.Empty;
            var message = _messageTextBox?.Text ?? string.Empty;
            SendClicked?.Invoke(this, (recipient, message));
        }

        private void OnCancelClicked(object? sender, RoutedEventArgs e)
        {
            CancelClicked?.Invoke(this, EventArgs.Empty);
        }

        public void Clear()
        {
            if (_recipientTextBox != null) _recipientTextBox.Text = string.Empty;
            if (_messageTextBox != null) _messageTextBox.Text = string.Empty;
        }
    }
}

