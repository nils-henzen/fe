using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace FeChat;

public static class NotificationHelper
{
    // Windows Toast Notification support
    public static void ShowWindowsToast(string title, string message)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return;
        }

        try
        {
            // Use PowerShell to show Windows 10/11 toast notification
            var escapedTitle = title.Replace("'", "''").Replace("\"", "`\"");
            var escapedMessage = message.Replace("'", "''").Replace("\"", "`\"");
            
            var script = $@"
[Windows.UI.Notifications.ToastNotificationManager, Windows.UI.Notifications, ContentType = WindowsRuntime] > $null
$template = [Windows.UI.Notifications.ToastNotificationManager]::GetTemplateContent([Windows.UI.Notifications.ToastTemplateType]::ToastText02)
$toastXml = [xml] $template.GetXml()
$toastXml.GetElementsByTagName('text')[0].AppendChild($toastXml.CreateTextNode('{escapedTitle}')) > $null
$toastXml.GetElementsByTagName('text')[1].AppendChild($toastXml.CreateTextNode('{escapedMessage}')) > $null
$xml = New-Object Windows.Data.Xml.Dom.XmlDocument
$xml.LoadXml($toastXml.OuterXml)
$toast = [Windows.UI.Notifications.ToastNotification]::new($xml)
[Windows.UI.Notifications.ToastNotificationManager]::CreateToastNotifier('FeChat').Show($toast)
";

            var processStartInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{script}\"",
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using var process = Process.Start(processStartInfo);
            process?.WaitForExit(3000); // Wait max 3 seconds
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error showing Windows toast: {ex.Message}");
        }
    }
}

