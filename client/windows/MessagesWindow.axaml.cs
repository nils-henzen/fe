using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace FeChat;

public partial class MessagesWindow : Window
{
    private readonly ApiClient _apiClient;
    private readonly ConfigManager _configManager;
    private ListBox? _messagesListBox;
    private Button? _refreshButton;
    private TextBlock? _statusTextBlock;
    private ObservableCollection<Message> _messages = new();

    public MessagesWindow(ApiClient apiClient, ConfigManager configManager)
    {
        _apiClient = apiClient;
        _configManager = configManager;

        InitializeComponent();
        AttachEventHandlers();

        // Load messages on startup
        _ = LoadMessagesAsync();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);

        _messagesListBox = this.FindControl<ListBox>("MessagesListBox");
        _refreshButton = this.FindControl<Button>("RefreshButton");
        _statusTextBlock = this.FindControl<TextBlock>("StatusTextBlock");

        if (_messagesListBox != null)
            _messagesListBox.ItemsSource = _messages;
    }

    private void AttachEventHandlers()
    {
        if (_refreshButton != null)
            _refreshButton.Click += RefreshButton_Click;
    }

    private async void RefreshButton_Click(object? sender, RoutedEventArgs e)
    {
        await LoadMessagesAsync();
    }

    private async System.Threading.Tasks.Task LoadMessagesAsync()
    {
        try
        {
            UpdateStatus("Loading messages...");

            if (_refreshButton != null)
                _refreshButton.IsEnabled = false;

            var messages = await _apiClient.FetchMessagesAsync();

            _messages.Clear();

            if (messages != null && messages.Any())
            {
                // Sort by timestamp (newest first)
                var sorted = messages.OrderByDescending(m => m.Timestamp);

                foreach (var msg in sorted)
                {
                    _messages.Add(msg);
                }

                UpdateStatus($"Loaded {messages.Count} message(s)");
            }
            else
            {
                UpdateStatus("No messages found");
            }
        }
        catch (Exception ex)
        {
            UpdateStatus($"Error loading messages: {ex.Message}");
        }
        finally
        {
            if (_refreshButton != null)
                _refreshButton.IsEnabled = true;
        }
    }

    private void UpdateStatus(string status)
    {
        if (_statusTextBlock != null)
            _statusTextBlock.Text = status;
    }
}

