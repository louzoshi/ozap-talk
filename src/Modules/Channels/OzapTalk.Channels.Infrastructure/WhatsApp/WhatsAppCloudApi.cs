using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using OzapTalk.Channels.Application.Abstractions;

namespace OzapTalk.Channels.Infrastructure.WhatsApp;

internal sealed class WhatsAppCloudApi(HttpClient http, IOptions<ChannelsOptions> options) : IWhatsAppApi
{
    private readonly string _apiVersion = options.Value.GraphApiVersion;

    public async Task<string> SendTextAsync(
        string phoneNumberId, string accessToken, string toPhone, string text, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"/{_apiVersion}/{phoneNumberId}/messages")
        {
            Content = JsonContent.Create(new
            {
                messaging_product = "whatsapp",
                to = toPhone,
                type = "text",
                text = new { body = text },
            }),
        };
        request.Headers.Authorization = new("Bearer", accessToken);

        using var response = await http.SendAsync(request, ct);
        var body = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
            throw new WhatsAppApiException($"WhatsApp API {(int)response.StatusCode}: {body}");

        var parsed = JsonSerializer.Deserialize<SendResponse>(body);
        return parsed?.Messages?.FirstOrDefault()?.Id
            ?? throw new WhatsAppApiException($"WhatsApp API returned no message id: {body}");
    }

    private sealed class SendResponse
    {
        [JsonPropertyName("messages")] public List<SentMessage>? Messages { get; init; }
    }

    private sealed class SentMessage
    {
        [JsonPropertyName("id")] public string Id { get; init; } = string.Empty;
    }
}

public sealed class WhatsAppApiException(string message) : Exception(message);
