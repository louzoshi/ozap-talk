using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using OzapTalk.Channels.Application.Webhooks;
using OzapTalk.Channels.Infrastructure.WhatsApp;

namespace OzapTalk.Channels.UnitTests;

public class WebhookTests
{
    private const string Secret = "app-secret";

    private static string Sign(byte[] body)
    {
        var hash = HMACSHA256.HashData(Encoding.UTF8.GetBytes(Secret), body);
        return "sha256=" + Convert.ToHexStringLower(hash);
    }

    [Fact]
    public void Signature_accepts_a_correctly_signed_body()
    {
        var body = "{\"hello\":\"world\"}"u8.ToArray();
        WebhookSignature.IsValid(Secret, Sign(body), body).Should().BeTrue();
    }

    [Fact]
    public void Signature_rejects_tampered_body()
    {
        var signature = Sign("{\"hello\":\"world\"}"u8.ToArray());
        WebhookSignature.IsValid(Secret, signature, "{\"hello\":\"tampered\"}"u8.ToArray()).Should().BeFalse();
    }

    [Fact]
    public void Signature_rejects_missing_header()
    {
        WebhookSignature.IsValid(Secret, null, "{}"u8.ToArray()).Should().BeFalse();
    }

    [Fact]
    public void Payload_parses_a_text_message()
    {
        const string json = """
        {
          "entry": [{
            "changes": [{
              "value": {
                "metadata": { "phone_number_id": "123456", "display_phone_number": "+55 11 90000-0000" },
                "contacts": [{ "wa_id": "5511999999999", "profile": { "name": "Ana" } }],
                "messages": [{
                  "id": "wamid.ABC", "from": "5511999999999", "timestamp": "1710000000",
                  "type": "text", "text": { "body": "Oi, tudo bem?" }
                }]
              }
            }]
          }]
        }
        """;

        var payload = JsonSerializer.Deserialize<MetaWebhookPayload>(json)!;
        var value = payload.Entry[0].Changes[0].Value!;

        value.Metadata!.PhoneNumberId.Should().Be("123456");
        value.Contacts[0].Profile!.Name.Should().Be("Ana");
        value.Messages[0].Text!.Body.Should().Be("Oi, tudo bem?");
        value.Messages[0].Id.Should().Be("wamid.ABC");
    }
}
