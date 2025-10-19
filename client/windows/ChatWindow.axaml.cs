using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace FeChat;

public class ChatMessage
{
    public string Text { get; set; } = "";
    public string Time { get; set; } = "";
    public bool IsSentByMe { get; set; }
    public HorizontalAlignment Alignment => IsSentByMe ? HorizontalAlignment.Right : HorizontalAlignment.Left;
    public string BubbleColor => IsSentByMe ? "#95adca" : "#FFFFFF"; // rock-blue-500 for sent, white for received
}

public partial class ChatWindow : Window
{
    private readonly ApiClient _apiClient;
    private readonly ConfigManager _configManager;
    private readonly string _contactId;
    private TextBlock? _contactNameText;
    private TextBlock? _contactInitialText;
    private Button? _refreshButton;
    private Button? _sendButton;
    private TextBox? _messageInputBox;
    private ItemsControl? _messagesItemsControl;
    private ScrollViewer? _messageScrollViewer;
    private ObservableCollection<ChatMessage> _messages = new();

    public ChatWindow(ApiClient apiClient, ConfigManager configManager, string contactId)
    {
        _apiClient = apiClient;
        _configManager = configManager;
        _contactId = contactId;

        InitializeComponent();
        SetupUI();
        AttachEventHandlers();

        // Load messages on startup
        _ = LoadMessagesAsync();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);

        _contactNameText = this.FindControl<TextBlock>("ContactNameText");
        _contactInitialText = this.FindControl<TextBlock>("ContactInitialText");
        _refreshButton = this.FindControl<Button>("RefreshButton");
        _sendButton = this.FindControl<Button>("SendButton");
        _messageInputBox = this.FindControl<TextBox>("MessageInputBox");
        _messagesItemsControl = this.FindControl<ItemsControl>("MessagesItemsControl");
        _messageScrollViewer = this.FindControl<ScrollViewer>("MessageScrollViewer");

        if (_messagesItemsControl != null)
            _messagesItemsControl.ItemsSource = _messages;
    }

    private void SetupUI()
    {
        Title = $"Chat with {_contactId}";
        
        if (_contactNameText != null)
            _contactNameText.Text = _contactId;

        if (_contactInitialText != null)
            _contactInitialText.Text = string.IsNullOrEmpty(_contactId) ? "?" : _contactId[0].ToString().ToUpper();
    }

    private void AttachEventHandlers()
    {
        if (_refreshButton != null)
            _refreshButton.Click += RefreshButton_Click;

        if (_sendButton != null)
            _sendButton.Click += SendButton_Click;

        if (_messageInputBox != null)
        {
            _messageInputBox.KeyDown += MessageInputBox_KeyDown;
        }
    }

    private void MessageInputBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && !e.KeyModifiers.HasFlag(KeyModifiers.Shift))
        {
            e.Handled = true;
            SendButton_Click(sender, new RoutedEventArgs());
        }
    }

    private async void RefreshButton_Click(object? sender, RoutedEventArgs e)
    {
        await LoadMessagesAsync();
    }

    private async void SendButton_Click(object? sender, RoutedEventArgs e)
    {
        var messageText = _messageInputBox?.Text?.Trim();

        if (string.IsNullOrEmpty(messageText))
            return;

        // Disable send button
        if (_sendButton != null)
            _sendButton.IsEnabled = false;

        try
        {
            var success = await _apiClient.SendMessageAsync(_contactId, messageText);

            if (success)
            {
                // Add message to UI immediately
                _messages.Add(new ChatMessage
                {
                    Text = messageText,
                    Time = DateTime.Now.ToString("HH:mm"),
                    IsSentByMe = true
                });

                // Clear input
                if (_messageInputBox != null)
                    _messageInputBox.Text = "";

                // Scroll to bottom
                ScrollToBottom();

                // Optionally refresh to get all messages
                await Task.Delay(500);
                await LoadMessagesAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending message: {ex.Message}");
        }
        finally
        {
            if (_sendButton != null)
                _sendButton.IsEnabled = true;

            _messageInputBox?.Focus();
        }
    }

    private async Task LoadMessagesAsync()
    {
        try
        {
            if (_refreshButton != null)
                _refreshButton.IsEnabled = false;

            var messages = await _apiClient.FetchMessagesAsync();

            if (messages != null && messages.Any())
            {
                var currentUserId = _configManager.GetConfig().UserId;

                // Filter messages for this conversation
                var conversationMessages = messages
                    .Where(m => (m.SenderId == currentUserId && m.ReceiverId == _contactId) ||
                               (m.SenderId == _contactId && m.ReceiverId == currentUserId))
                    .OrderBy(m => m.Timestamp)
                    .ToList();

                _messages.Clear();

                foreach (var msg in conversationMessages)
                {
                    _messages.Add(new ChatMessage
                    {
                        Text = msg.DisplayText,
                        Time = FormatMessageTime(msg.Timestamp),
                        IsSentByMe = msg.SenderId == currentUserId
                    });
                }

                // Scroll to bottom after loading
                await Task.Delay(100);
                ScrollToBottom();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading messages: {ex.Message}");
        }
        finally
        {
            if (_refreshButton != null)
                _refreshButton.IsEnabled = true;
        }
    }

    private string FormatMessageTime(string timestamp)
    {
        if (DateTime.TryParse(timestamp, out var dt))
        {
            var now = DateTime.Now;
            if (dt.Date == now.Date)
                return dt.ToString("HH:mm");
            else if (dt.Date == now.AddDays(-1).Date)
                return "Yesterday " + dt.ToString("HH:mm");
            else
                return dt.ToString("MMM d, HH:mm");
        }
        return timestamp;
    }

    private void ScrollToBottom()
    {
        if (_messageScrollViewer != null)
        {
            _messageScrollViewer.ScrollToEnd();
        }
    }
}
