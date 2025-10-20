using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Notifications;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FeChat;

public class TrayManager
{
    private readonly IClassicDesktopStyleApplicationLifetime _lifetime;
    private readonly TrayIcon _trayIcon;
    private readonly ApiClient _apiClient;
    private readonly ConfigManager _configManager;
    private Timer? _pollingTimer;
    private HashSet<int> _seenMessageIds = new();
    private MainChatWindow? _mainChatWindow;
    private WindowNotificationManager? _notificationManager;

    public TrayManager(IClassicDesktopStyleApplicationLifetime lifetime)
    {
        _lifetime = lifetime;
        _configManager = new ConfigManager();
        _apiClient = new ApiClient(_configManager);

        // Create tray icon
        _trayIcon = new TrayIcon
        {
            Icon = CreateTrayIconImage(),
            ToolTipText = "Fe Chat Client",
            IsVisible = true
        };

        // Add click handler to open main chat window
        _trayIcon.Clicked += (_, _) => ShowMainChatWindow();

        // Setup context menu
        var menu = new NativeMenu();

        var openChatsItem = new NativeMenuItem("Open Chats");
        openChatsItem.Click += (_, _) => ShowMainChatWindow();
        menu.Add(openChatsItem);

        var checkMessagesItem = new NativeMenuItem("Check Messages");
        checkMessagesItem.Click += async (_, _) => await CheckMessagesAsync();
        menu.Add(checkMessagesItem);

        menu.Add(new NativeMenuItemSeparator());

        var exitItem = new NativeMenuItem("Exit");
        exitItem.Click += (_, _) => Exit();
        menu.Add(exitItem);

        _trayIcon.Menu = menu;

        // Setup notification manager with main window if available
        if (_lifetime.MainWindow != null)
        {
            _notificationManager = new WindowNotificationManager(_lifetime.MainWindow)
            {
                Position = NotificationPosition.TopRight,
                MaxItems = 3
            };
        }

        // Start polling
        StartPolling();
    }

    private WindowIcon CreateTrayIconImage()
    {
        // Create a simple colored icon bitmap (you can replace with actual icon file)
        var width = 32;
        var height = 32;
        var bitmap = new WriteableBitmap(new PixelSize(width, height), new Vector(96, 96), Avalonia.Platform.PixelFormat.Bgra8888, AlphaFormat.Premul);

        using (var frameBuffer = bitmap.Lock())
        {
            unsafe
            {
                var ptr = (uint*)frameBuffer.Address;
                // Create a simple gradient icon (blue theme)
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        var distanceFromCenter = Math.Sqrt(Math.Pow(x - width / 2.0, 2) + Math.Pow(y - height / 2.0, 2));
                        if (distanceFromCenter < width / 2.0 - 2)
                        {
                            // Blue gradient
                            byte alpha = 255;
                            byte red = (byte)(100 + x * 3);
                            byte green = (byte)(150 + y * 2);
                            byte blue = 255;
                            ptr[y * width + x] = (uint)(alpha << 24 | red << 16 | green << 8 | blue);
                        }
                        else
                        {
                            ptr[y * width + x] = 0; // Transparent
                        }
                    }
                }
            }
        }

        return new WindowIcon(bitmap);
    }

    private void StartPolling()
    {
        var pollingInterval = TimeSpan.FromSeconds(_configManager.PollingIntervalSeconds);
        _pollingTimer = new Timer(_ => 
        {
            _ = CheckMessagesAsync();
        }, null, TimeSpan.Zero, pollingInterval);
    }

    private async Task CheckMessagesAsync()
    {
        try
        {
            var messages = await _apiClient.FetchMessagesAsync();

            if (messages != null && messages.Any())
            {
                var currentUserId = _configManager.GetConfig().UserId;
                var newMessages = messages.Where(m => !_seenMessageIds.Contains(m.Id)).ToList();

                if (newMessages.Any())
                {
                    foreach (var msg in newMessages)
                    {
                        _seenMessageIds.Add(msg.Id);
                    }

                    // Only show notification if app is not in foreground
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        if (!IsAnyWindowActive())
                        {
                            // Show more detailed notification with sender info
                            var incomingMessages = newMessages.Where(m => m.SenderId != currentUserId).ToList();
                            
                            if (incomingMessages.Any())
                            {
                                if (incomingMessages.Count == 1)
                                {
                                    var msg = incomingMessages.First();
                                    var preview = msg.DisplayText.Length > 50 
                                        ? msg.DisplayText.Substring(0, 47) + "..." 
                                        : msg.DisplayText;
                                    ShowNotification($"New message from {msg.SenderId}", preview);
                                }
                                else
                                {
                                    ShowNotification("New Messages", $"You have {incomingMessages.Count} new message(s)");
                                }
                            }
                        }
                    });
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking messages: {ex.Message}");
        }
    }

    private bool IsAnyWindowActive()
    {
        // Check if main chat window is active
        if (_mainChatWindow?.IsActive == true && _mainChatWindow.IsVisible)
        {
            return true;
        }

        // Check if any other window from this app is active
        if (_lifetime.Windows != null)
        {
            foreach (var window in _lifetime.Windows)
            {
                if (window.IsActive && window.IsVisible)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private void ShowNotification(string title, string message)
    {
        // Try Windows native toast notification first
        try
        {
            NotificationHelper.ShowWindowsToast(title, message);
        }
        catch
        {
            // Fallback to Avalonia notification if Windows toast fails
            if (_notificationManager != null)
            {
                _notificationManager.Show(new Notification(title, message, NotificationType.Information));
            }
        }
    }

    private void ShowMainChatWindow()
    {
        if (_mainChatWindow == null)
        {
            _mainChatWindow = new MainChatWindow(_apiClient, _configManager);
            _mainChatWindow.Closed += (_, _) => _mainChatWindow = null;
        }

        if (_mainChatWindow.IsVisible)
        {
            _mainChatWindow.Hide();
        }
        else
        {
            _mainChatWindow.PositionNearTrayIcon();
            _mainChatWindow.Show();
            _mainChatWindow.Activate();
            _ = _mainChatWindow.LoadContactsAsync();
        }
    }

    private void Exit()
    {
        _pollingTimer?.Dispose();
        _trayIcon.Dispose();
        _lifetime.Shutdown();
    }
}
