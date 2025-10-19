using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace FeChat;

public partial class MainChatWindow : Window
{
    private readonly ApiClient _apiClient;
    private readonly ConfigManager _configManager;
    private ListBox? _contactsListBox;
    private Button? _newChatButton;
    private Button? _refreshButton;
    private Button? _sendButton;
    private TextBox? _messageInputBox;
    private ItemsControl? _messagesItemsControl;
    private ScrollViewer? _messageScrollViewer;
    private Border? _chatHeaderBorder;
    private Border? _inputAreaBorder;
    private Panel? _emptyStatePanel;
    private TextBlock? _chatContactNameText;
    private TextBlock? _chatContactInitialText;
    private Border? _titleBar;
    private Button? _closeButton;
    private ComposePanel? _composePanel;
    
    private ObservableCollection<Contact> _contacts = new();
    private ObservableCollection<ChatMessage> _messages = new();
    private string? _currentContactId;

    public MainChatWindow(ApiClient apiClient, ConfigManager configManager)
    {
        _apiClient = apiClient;
        _configManager = configManager;

        InitializeComponent();
        AttachEventHandlers();

        // Load contacts on startup
        _ = LoadContactsAsync();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);

        _contactsListBox = this.FindControl<ListBox>("ContactsListBox");
        _newChatButton = this.FindControl<Button>("NewChatButton");
        _refreshButton = this.FindControl<Button>("RefreshButton");
        _sendButton = this.FindControl<Button>("SendButton");
        _messageInputBox = this.FindControl<TextBox>("MessageInputBox");
        _messagesItemsControl = this.FindControl<ItemsControl>("MessagesItemsControl");
        _messageScrollViewer = this.FindControl<ScrollViewer>("MessageScrollViewer");
        _chatHeaderBorder = this.FindControl<Border>("ChatHeaderBorder");
        _inputAreaBorder = this.FindControl<Border>("InputAreaBorder");
        _emptyStatePanel = this.FindControl<Panel>("EmptyStatePanel");
        _chatContactNameText = this.FindControl<TextBlock>("ChatContactNameText");
        _chatContactInitialText = this.FindControl<TextBlock>("ChatContactInitialText");
        _titleBar = this.FindControl<Border>("TitleBar");
        _closeButton = this.FindControl<Button>("CloseButton");
        _composePanel = this.FindControl<ComposePanel>("ComposePanel");

        if (_contactsListBox != null)
            _contactsListBox.ItemsSource = _contacts;

        if (_messagesItemsControl != null)
            _messagesItemsControl.ItemsSource = _messages;
    }

    private void AttachEventHandlers()
    {
        if (_newChatButton != null)
            _newChatButton.Click += NewChatButton_Click;
        if (_composePanel != null)
        {
            _composePanel.SendClicked += ComposePanel_SendClicked;
            _composePanel.CancelClicked += ComposePanel_CancelClicked;
        }

        if (_refreshButton != null)
            _refreshButton.Click += RefreshButton_Click;

        if (_sendButton != null)
            _sendButton.Click += SendButton_Click;

        if (_messageInputBox != null)
            _messageInputBox.KeyDown += MessageInputBox_KeyDown;

        if (_contactsListBox != null)
            _contactsListBox.SelectionChanged += ContactsListBox_SelectionChanged;

        if (_closeButton != null)
            _closeButton.Click += (_, _) => Hide();

        // Make title bar draggable
        if (_titleBar != null)
        {
            _titleBar.PointerPressed += (sender, e) =>
            {
                if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
                {
                    BeginMoveDrag(e);
                }
            };
        }

        // Handle window deactivation to hide window
        this.Deactivated += (_, _) =>
        {
            if (!IsPointerOver)
            {
                Hide();
            }
        };
    }

    public void PositionNearTrayIcon()
    {
        // Get screen working area (excludes taskbar)
        var screen = Screens.Primary;
        if (screen == null) return;

        var workingArea = screen.WorkingArea;
        var scalingFactor = screen.Scaling;

        // Position at bottom-right corner, above taskbar
        var windowWidth = Width * scalingFactor;
        var windowHeight = Height * scalingFactor;

        var left = (workingArea.Right - windowWidth - 10) / scalingFactor;
        var top = (workingArea.Bottom - windowHeight - 10) / scalingFactor;

        Position = new PixelPoint((int)left, (int)top);
    }

    private void NewChatButton_Click(object? sender, RoutedEventArgs e)
    {
        // Hide chat/message UI and show compose panel
        if (_chatHeaderBorder != null) _chatHeaderBorder.IsVisible = false;
        if (_emptyStatePanel != null) _emptyStatePanel.IsVisible = false;
        if (_messageScrollViewer != null) _messageScrollViewer.IsVisible = false;
        if (_inputAreaBorder != null) _inputAreaBorder.IsVisible = false;
        if (_composePanel != null)
        {
            _composePanel.Clear();
            _composePanel.IsVisible = true;
        }
    }

    private void ComposePanel_SendClicked(object? sender, (string recipient, string message) args)
    {
        // Hide compose panel and restore chat/message UI
        if (_composePanel != null) _composePanel.IsVisible = false;
        ShowChatUI(args.recipient);
        // Optionally, send the message to the backend here
        // For now, just open the chat with the new contact
    }

    private void ComposePanel_CancelClicked(object? sender, EventArgs e)
    {
        // Hide compose panel and restore chat/message UI
        if (_composePanel != null) _composePanel.IsVisible = false;
        ShowDefaultUI();
    }

    private void ShowChatUI(string contactId)
    {
        // Show chat UI for the given contact
        if (_chatHeaderBorder != null) _chatHeaderBorder.IsVisible = true;
        if (_messageScrollViewer != null) _messageScrollViewer.IsVisible = true;
        if (_inputAreaBorder != null) _inputAreaBorder.IsVisible = true;
        if (_emptyStatePanel != null) _emptyStatePanel.IsVisible = false;
        // Set contact info, load messages, etc.
        _currentContactId = contactId;
        // ...existing logic to load messages for contactId...
    }

    private void ShowDefaultUI()
    {
        // Show empty state or last selected chat
        if (_emptyStatePanel != null) _emptyStatePanel.IsVisible = true;
        if (_chatHeaderBorder != null) _chatHeaderBorder.IsVisible = false;
        if (_messageScrollViewer != null) _messageScrollViewer.IsVisible = false;
        if (_inputAreaBorder != null) _inputAreaBorder.IsVisible = false;
    }

    private async void RefreshButton_Click(object? sender, RoutedEventArgs e)
    {
        if (_currentContactId != null)
        {
            await LoadMessagesForContact(_currentContactId);
        }
        await LoadContactsAsync();
    }

    private async void SendButton_Click(object? sender, RoutedEventArgs e)
    {
        if (_currentContactId == null) return;

        var messageText = _messageInputBox?.Text?.Trim();
        if (string.IsNullOrEmpty(messageText)) return;

        if (_sendButton != null)
            _sendButton.IsEnabled = false;

        try
        {
            var success = await _apiClient.SendMessageAsync(_currentContactId, messageText);

            if (success)
            {
                _messages.Add(new ChatMessage
                {
                    Text = messageText,
                    Time = DateTime.Now.ToString("HH:mm"),
                    IsSentByMe = true
                });

                if (_messageInputBox != null)
                    _messageInputBox.Text = "";

                ScrollToBottom();

                await Task.Delay(500);
                await LoadMessagesForContact(_currentContactId);
                await LoadContactsAsync();
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

    private void MessageInputBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && !e.KeyModifiers.HasFlag(KeyModifiers.Shift))
        {
            e.Handled = true;
            SendButton_Click(sender, new RoutedEventArgs());
        }
    }

    private async void ContactsListBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_contactsListBox?.SelectedItem is Contact contact)
        {
            await OpenChat(contact.ContactId);
        }
    }

    private async Task OpenChat(string contactId)
    {
        _currentContactId = contactId;

        // Update UI
        if (_chatContactNameText != null)
            _chatContactNameText.Text = contactId;

        if (_chatContactInitialText != null)
            _chatContactInitialText.Text = string.IsNullOrEmpty(contactId) ? "?" : contactId[0].ToString().ToUpper();

        // Show chat UI, hide empty state
        if (_emptyStatePanel != null)
            _emptyStatePanel.IsVisible = false;

        if (_chatHeaderBorder != null)
            _chatHeaderBorder.IsVisible = true;

        if (_messageScrollViewer != null)
            _messageScrollViewer.IsVisible = true;

        if (_inputAreaBorder != null)
            _inputAreaBorder.IsVisible = true;

        // Load messages
        await LoadMessagesForContact(contactId);
    }

    public async Task LoadContactsAsync()
    {
        try
        {
            var messages = await _apiClient.FetchMessagesAsync();

            if (messages != null && messages.Any())
            {
                var currentUserId = _configManager.GetConfig().UserId;

                var contactGroups = messages
                    .Select(m => m.SenderId == currentUserId ? m.ReceiverId : m.SenderId)
                    .Where(id => !string.IsNullOrEmpty(id))
                    .Distinct()
                    .Select(contactId =>
                    {
                        var contactMessages = messages
                            .Where(m => m.SenderId == contactId || m.ReceiverId == contactId)
                            .OrderByDescending(m => m.Timestamp)
                            .ToList();

                        var lastMsg = contactMessages.First();

                        return new Contact
                        {
                            ContactId = contactId,
                            LastMessage = lastMsg.DisplayText.Length > 35
                                ? lastMsg.DisplayText.Substring(0, 32) + "..."
                                : lastMsg.DisplayText,
                            LastMessageTime = FormatTime(lastMsg.Timestamp),
                            UnreadCount = 0
                        };
                    })
                    .OrderByDescending(c => c.LastMessageTime)
                    .ToList();

                _contacts.Clear();
                foreach (var contact in contactGroups)
                {
                    _contacts.Add(contact);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading contacts: {ex.Message}");
        }
    }

    private async Task LoadMessagesForContact(string contactId)
    {
        try
        {
            if (_refreshButton != null)
                _refreshButton.IsEnabled = false;

            var messages = await _apiClient.FetchMessagesAsync();

            if (messages != null && messages.Any())
            {
                var currentUserId = _configManager.GetConfig().UserId;

                var conversationMessages = messages
                    .Where(m => (m.SenderId == currentUserId && m.ReceiverId == contactId) ||
                               (m.SenderId == contactId && m.ReceiverId == currentUserId))
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

    private string FormatTime(string timestamp)
    {
        if (DateTime.TryParse(timestamp, out var dt))
        {
            var now = DateTime.Now;
            var diff = now - dt;

            if (diff.TotalMinutes < 1)
                return "Just now";
            else if (diff.TotalHours < 1)
                return $"{(int)diff.TotalMinutes}m ago";
            else if (diff.TotalDays < 1)
                return $"{(int)diff.TotalHours}h ago";
            else if (diff.TotalDays < 7)
                return $"{(int)diff.TotalDays}d ago";
            else
                return dt.ToString("MMM d");
        }
        return timestamp;
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
