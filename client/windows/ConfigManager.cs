using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FeChat;

public class AppConfig
{
    [JsonPropertyName("server_ip")]
    public string ServerIp { get; set; } = "127.0.0.1";

    [JsonPropertyName("server_port")]
    public int ServerPort { get; set; } = 5000;

    [JsonPropertyName("user_id")]
    public string UserId { get; set; } = "user1";

    [JsonPropertyName("signature")]
    public string Signature { get; set; } = "default_signature";
}

public class ConfigManager
{
    private readonly string _configPath;
    private AppConfig? _config;
    public int PollingIntervalSeconds { get; set; } = 30;

    public ConfigManager()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appFolder = Path.Combine(appDataPath, "FeChat");
        Directory.CreateDirectory(appFolder);
        _configPath = Path.Combine(appFolder, "config.json");

        LoadConfig();
    }

    private void LoadConfig()
    {
        try
        {
            if (File.Exists(_configPath))
            {
                var json = File.ReadAllText(_configPath);
                _config = JsonSerializer.Deserialize<AppConfig>(json);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading config: {ex.Message}");
        }

        // Create default config if not exists
        if (_config == null)
        {
            _config = new AppConfig();
            SaveConfig();
        }
    }

    private void SaveConfig()
    {
        try
        {
            var json = JsonSerializer.Serialize(_config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_configPath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving config: {ex.Message}");
        }
    }

    public AppConfig GetConfig() => _config ?? new AppConfig();
}

