using System;
using Avalonia.Layout;

namespace FeChat;

public class ChatMessage
{
    public int MessageId { get; set; }
    public string Text { get; set; } = "";
    public string Time { get; set; } = "";
    public bool IsSentByMe { get; set; }
    public string? FileName { get; set; }
    public string? FileType { get; set; }
    public string? FileContents { get; set; }
    public bool IsFile => !string.IsNullOrEmpty(FileName);
    public bool IsTextMessage => FileType?.StartsWith("text/") == true;
    // Download button should appear only for real files (images, PDFs, etc.), not for FETXT or text messages
    public bool IsDownloadableFile => IsFile && !IsTextMessage && 
                                      FileType?.Equals("FETXT", StringComparison.OrdinalIgnoreCase) == false &&
                                      FileType?.Equals("TXT", StringComparison.OrdinalIgnoreCase) == false;
    public HorizontalAlignment Alignment => IsSentByMe ? HorizontalAlignment.Right : HorizontalAlignment.Left;
    public string BubbleColor => IsSentByMe ? "#95adca" : "#FFFFFF"; // rock-blue-500 for sent, white for received
}