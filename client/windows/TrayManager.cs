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
    private ComposeWindow? _composeWindow;
    private MessagesWindow? _messagesWindow;
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

        // Setup context menu
        var menu = new NativeMenu();

        var checkMessagesItem = new NativeMenuItem("Check Messages");
        checkMessagesItem.Click += async (s, e) => await CheckMessagesAsync();
        menu.Add(checkMessagesItem);

        var viewMessagesItem = new NativeMenuItem("View Messages");
        viewMessagesItem.Click += (s, e) => ShowMessagesWindow();
        menu.Add(viewMessagesItem);

        var composeItem = new NativeMenuItem("Compose Message");
        composeItem.Click += (s, e) => ShowComposeWindow();
        menu.Add(composeItem);

        menu.Add(new NativeMenuItemSeparator());

        var configItem = new NativeMenuItem("Settings");
        configItem.Click += (s, e) => ShowSettings();
        menu.Add(configItem);

        var exitItem = new NativeMenuItem("Exit");
        exitItem.Click += (s, e) => Exit();
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
        _pollingTimer = new Timer(async _ => await CheckMessagesAsync(), null, TimeSpan.Zero, pollingInterval);
    }

    private async Task CheckMessagesAsync()
    {
        try
        {
            var messages = await _apiClient.FetchMessagesAsync();

            if (messages != null && messages.Any())
            {
                var newMessages = messages.Where(m => !_seenMessageIds.Contains(m.Id)).ToList();

                if (newMessages.Any())
                {
                    foreach (var msg in newMessages)
                    {
                        _seenMessageIds.Add(msg.Id);
                    }

                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        ShowNotification($"New Messages", $"You have {newMessages.Count} new message(s)");
                    });
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking messages: {ex.Message}");
        }
    }

    private void ShowNotification(string title, string message)
    {
        if (_notificationManager != null)
        {
            _notificationManager.Show(new Notification(title, message, NotificationType.Information));
        }
    }

    private void ShowComposeWindow()
    {
        if (_composeWindow == null || !_composeWindow.IsVisible)
        {
            _composeWindow = new ComposeWindow(_apiClient, _configManager);
            _composeWindow.Closed += (s, e) => _composeWindow = null;
            _composeWindow.Show();
        }
        else
        {
            _composeWindow.Activate();
        }
    }

    private void ShowMessagesWindow()
    {
        if (_messagesWindow == null || !_messagesWindow.IsVisible)
        {
            _messagesWindow = new MessagesWindow(_apiClient, _configManager);
            _messagesWindow.Closed += (s, e) => _messagesWindow = null;
            _messagesWindow.Show();
        }
        else
        {
            _messagesWindow.Activate();
        }
    }

    private void ShowSettings()
    {
        var config = _configManager.GetConfig();
        var message = $"Current Settings:\n\n" +
                     $"Server: {config.ServerIp}:{config.ServerPort}\n" +
                     $"User ID: {config.UserId}\n" +
                     $"Polling Interval: {_configManager.PollingIntervalSeconds}s\n\n" +
                     $"To change settings, edit the config file.";

        ShowNotification("Settings", message);
    }

    private void Exit()
    {
        _pollingTimer?.Dispose();
        _trayIcon.Dispose();
        _lifetime.Shutdown();
    }
}
