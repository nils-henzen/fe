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