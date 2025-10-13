using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FeChat;

public class Message
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("sender_id")]
    public string SenderId { get; set; } = "";

    [JsonPropertyName("receiver_id")]
    public string ReceiverId { get; set; } = "";

    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; } = "";

    [JsonPropertyName("file_name")]
    public string? FileName { get; set; }

    [JsonPropertyName("file_type")]
    public string? FileType { get; set; }

    [JsonPropertyName("file_contents")]
    public string? FileContents { get; set; }

    [JsonPropertyName("queue_deletion")]
    public bool QueueDeletion { get; set; }

    public bool IsTextMessage => FileType?.StartsWith("text/") == true;

    public string DisplayText => IsTextMessage && !string.IsNullOrEmpty(FileContents)
        ? System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(FileContents))
        : $"[File: {FileName}]";
}

public class ApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ConfigManager _configManager;
    private string BaseUrl => $"http://{_configManager.GetConfig().ServerIp}:{_configManager.GetConfig().ServerPort}";

    public ApiClient(ConfigManager configManager)
    {
        _configManager = configManager;
        _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
    }

    public async Task<List<Message>?> FetchMessagesAsync()
    {
        try
        {
            var config = _configManager.GetConfig();
            var request = new
            {
                signature = config.Signature,
                sender_id = config.UserId
            };

            var response = await _httpClient.GetAsync($"{BaseUrl}/fetch?sender_id={config.UserId}&signature={config.Signature}");
            response.EnsureSuccessStatusCode();

            var messages = await response.Content.ReadFromJsonAsync<List<Message>>();
            return messages;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fetch failed: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> SendMessageAsync(string receiverId, string messageText)
    {
        try
        {
            var config = _configManager.GetConfig();
            var payload = new
            {
                signature = config.Signature,
                sender_id = config.UserId,
                receiver_id = receiverId,
                message_text = messageText
            };

            var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/send_message", payload);
            response.EnsureSuccessStatusCode();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Send failed: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> HealthCheckAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/healthcheck");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}

