using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace FeChat;

public class Contact
{
    public string ContactId { get; set; } = "";
    public string LastMessage { get; set; } = "";
    public string LastMessageTime { get; set; } = "";
    public int UnreadCount { get; set; } = 0;
    public bool HasUnread => UnreadCount > 0;
    public string Initial => string.IsNullOrEmpty(ContactId) ? "?" : ContactId[0].ToString().ToUpper();
}

public partial class ContactsWindow : Window
{
    private readonly ApiClient _apiClient;
    private readonly ConfigManager _configManager;
    private ListBox? _contactsListBox;
    private Button? _newChatButton;
    private TextBlock? _statusTextBlock;
    private ObservableCollection<Contact> _contacts = new();
    private Dictionary<string, ChatWindow> _openChats = new();

    public ContactsWindow(ApiClient apiClient, ConfigManager configManager)
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
        _statusTextBlock = this.FindControl<TextBlock>("StatusTextBlock");

        if (_contactsListBox != null)
            _contactsListBox.ItemsSource = _contacts;
    }

    private void AttachEventHandlers()
    {
        if (_newChatButton != null)
            _newChatButton.Click += NewChatButton_Click;

        if (_contactsListBox != null)
            _contactsListBox.SelectionChanged += ContactsListBox_SelectionChanged;
    }

    private async void NewChatButton_Click(object? sender, RoutedEventArgs e)
    {
        // Logic for new chat button click
    }

    private void ContactsListBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_contactsListBox?.SelectedItem is Contact contact)
        {
            OpenChatWindow(contact.ContactId);
            _contactsListBox.SelectedItem = null; // Deselect
        }
    }

    private void OpenChatWindow(string contactId)
    {
        if (_openChats.TryGetValue(contactId, out var existingWindow) && existingWindow.IsVisible)
        {
            existingWindow.Activate();
        }
        else
        {
            var chatWindow = new ChatWindow(_apiClient, _configManager, contactId);
            chatWindow.Closed += (s, e) => _openChats.Remove(contactId);
            _openChats[contactId] = chatWindow;
            chatWindow.Show();
        }
    }

    public async Task LoadContactsAsync()
    {
        try
        {
            UpdateStatus("Loading contacts...");

            var messages = await _apiClient.FetchMessagesAsync();

            if (messages != null && messages.Any())
            {
                var currentUserId = _configManager.GetConfig().UserId;
                
                // Group messages by contact (either sender or receiver, whichever is not current user)
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
                            LastMessage = lastMsg.DisplayText.Length > 50 
                                ? lastMsg.DisplayText.Substring(0, 47) + "..." 
                                : lastMsg.DisplayText,
                            LastMessageTime = FormatTime(lastMsg.Timestamp),
                            UnreadCount = 0 // TODO: Implement unread tracking
                        };
                    })
                    .OrderByDescending(c => c.LastMessageTime)
                    .ToList();

                _contacts.Clear();
                foreach (var contact in contactGroups)
                {
                    _contacts.Add(contact);
                }

                UpdateStatus($"{_contacts.Count} contact(s)");
            }
            else
            {
                _contacts.Clear();
                UpdateStatus("No contacts yet. Start a new chat!");
            }
        }
        catch (Exception ex)
        {
            UpdateStatus($"Error: {ex.Message}");
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

    private void UpdateStatus(string status)
    {
        if (_statusTextBlock != null)
            _statusTextBlock.Text = status;
    }
}
