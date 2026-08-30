namespace SopaTalk.Channels.Infrastructure.WhatsApp;

/// <summary>App-level WhatsApp settings, bound from the <c>Channels</c> configuration section.</summary>
public sealed class ChannelsOptions
{
    public const string SectionName = "Channels";

    /// <summary>The string you set as the webhook verify token in the Meta app dashboard.</summary>
    public string WebhookVerifyToken { get; init; } = string.Empty;

    /// <summary>Meta app secret — used to validate the <c>X-Hub-Signature-256</c> header.</summary>
    public string AppSecret { get; init; } = string.Empty;

    /// <summary>Graph API version, e.g. <c>v21.0</c>.</summary>
    public string GraphApiVersion { get; init; } = "v21.0";
}
