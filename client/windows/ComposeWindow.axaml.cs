using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;

namespace FeChat;

public partial class ComposeWindow : Window
{
    private readonly ApiClient _apiClient;
    private readonly ConfigManager _configManager;
    private TextBox? _receiverTextBox;
    private TextBox? _messageTextBox;
    private Button? _sendButton;
    private Button? _cancelButton;

    public ComposeWindow(ApiClient apiClient, ConfigManager configManager)
    {
        _apiClient = apiClient;
        _configManager = configManager;

        InitializeComponent();
        AttachEventHandlers();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);

        _receiverTextBox = this.FindControl<TextBox>("ReceiverTextBox");
        _messageTextBox = this.FindControl<TextBox>("MessageTextBox");
        _sendButton = this.FindControl<Button>("SendButton");
        _cancelButton = this.FindControl<Button>("CancelButton");
    }

    private void AttachEventHandlers()
    {
        if (_sendButton != null)
            _sendButton.Click += SendButton_Click;

        if (_cancelButton != null)
            _cancelButton.Click += CancelButton_Click;
    }

    private async void SendButton_Click(object? sender, RoutedEventArgs e)
    {
        var receiver = _receiverTextBox?.Text?.Trim();
        var message = _messageTextBox?.Text?.Trim();

        if (string.IsNullOrEmpty(receiver))
        {
            ShowError("Please enter a recipient ID");
            return;
        }

        if (string.IsNullOrEmpty(message))
        {
            ShowError("Please enter a message");
            return;
        }

        // Disable button while sending
        if (_sendButton != null)
            _sendButton.IsEnabled = false;

        try
        {
            var success = await _apiClient.SendMessageAsync(receiver, message);

            if (success)
            {
                ShowSuccess("Message sent successfully!");

                // Clear fields
                if (_receiverTextBox != null)
                    _receiverTextBox.Text = "";
                if (_messageTextBox != null)
                    _messageTextBox.Text = "";

                // Close window after short delay
                await System.Threading.Tasks.Task.Delay(1000);
                Close();
            }
            else
            {
                ShowError("Failed to send message. Please try again.");
            }
        }
        catch (Exception ex)
        {
            ShowError($"Error: {ex.Message}");
        }
        finally
        {
            if (_sendButton != null)
                _sendButton.IsEnabled = true;
        }
    }

    private void CancelButton_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void ShowError(string message)
    {
        // Simple message box alternative
        var notification = new Window
        {
            Title = "Error",
            Width = 300,
            Height = 150,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Content = new StackPanel
            {
                Margin = new Thickness(20),
                Spacing = 15,
                Children =
                {
                    new TextBlock { Text = message, TextWrapping = Avalonia.Media.TextWrapping.Wrap },
                    new Button
                    {
                        Content = "OK",
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                        Width = 80
                    }
                }
            }
        };

        var button = (notification.Content as StackPanel)?.Children[1] as Button;
        if (button != null)
            button.Click += (s, e) => notification.Close();

        notification.ShowDialog(this);
    }

    private void ShowSuccess(string message)
    {
        var notification = new Window
        {
            Title = "Success",
            Width = 300,
            Height = 150,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Content = new StackPanel
            {
                Margin = new Thickness(20),
                Spacing = 15,
                Children =
                {
                    new TextBlock { Text = message, TextWrapping = Avalonia.Media.TextWrapping.Wrap },
                    new Button
                    {
                        Content = "OK",
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                        Width = 80
                    }
                }
            }
        };

        var button = (notification.Content as StackPanel)?.Children[1] as Button;
        if (button != null)
            button.Click += (s, e) => notification.Close();

        notification.ShowDialog(this);
    }
}

