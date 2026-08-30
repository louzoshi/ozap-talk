using System.Text.Json.Serialization;

namespace SopaTalk.Channels.Application.Webhooks;

/// <summary>
/// The subset of the WhatsApp Cloud API webhook body we consume. Extra fields (statuses,
/// media, reactions, …) are ignored for now.
/// </summary>
public sealed class MetaWebhookPayload
{
    [JsonPropertyName("entry")] public List<WebhookEntry> Entry { get; init; } = [];

    public sealed class WebhookEntry
    {
        [JsonPropertyName("changes")] public List<Change> Changes { get; init; } = [];
    }

    public sealed class Change
    {
        [JsonPropertyName("value")] public ChangeValue? Value { get; init; }
    }

    public sealed class ChangeValue
    {
        [JsonPropertyName("metadata")] public Metadata? Metadata { get; init; }
        [JsonPropertyName("contacts")] public List<Contact> Contacts { get; init; } = [];
        [JsonPropertyName("messages")] public List<Message> Messages { get; init; } = [];
    }

    public sealed class Metadata
    {
        [JsonPropertyName("phone_number_id")] public string PhoneNumberId { get; init; } = string.Empty;
        [JsonPropertyName("display_phone_number")] public string DisplayPhoneNumber { get; init; } = string.Empty;
    }

    public sealed class Contact
    {
        [JsonPropertyName("wa_id")] public string WaId { get; init; } = string.Empty;
        [JsonPropertyName("profile")] public Profile? Profile { get; init; }
    }

    public sealed class Profile
    {
        [JsonPropertyName("name")] public string? Name { get; init; }
    }

    public sealed class Message
    {
        [JsonPropertyName("id")] public string Id { get; init; } = string.Empty;
        [JsonPropertyName("from")] public string From { get; init; } = string.Empty;
        [JsonPropertyName("timestamp")] public string Timestamp { get; init; } = string.Empty;
        [JsonPropertyName("type")] public string Type { get; init; } = string.Empty;
        [JsonPropertyName("text")] public TextBody? Text { get; init; }
    }

    public sealed class TextBody
    {
        [JsonPropertyName("body")] public string Body { get; init; } = string.Empty;
    }
}
