using FluentAssertions;
using OzapTalk.Inbox.Domain.Conversations;
using OzapTalk.Inbox.Domain.Messages;

namespace OzapTalk.Inbox.UnitTests.Conversations;

public class ConversationFlowTests
{
    private static Conversation New() =>
        Conversation.Start(Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7());

    [Fact]
    public void RecordInbound_bumps_unread_and_sets_preview()
    {
        var c = New();

        c.RecordInbound("Olá, preciso de ajuda", DateTimeOffset.UtcNow);

        c.UnreadCount.Should().Be(1);
        c.LastMessagePreview.Should().Be("Olá, preciso de ajuda");
        c.Status.Should().Be(ConversationStatus.Open);
    }

    [Fact]
    public void RecordOutbound_moves_to_waiting_without_touching_unread()
    {
        var c = New();
        c.RecordInbound("oi", DateTimeOffset.UtcNow);

        c.RecordOutbound("Claro, como posso ajudar?", DateTimeOffset.UtcNow);

        c.Status.Should().Be(ConversationStatus.Waiting);
        c.UnreadCount.Should().Be(1);
    }

    [Fact]
    public void MarkRead_clears_unread()
    {
        var c = New();
        c.RecordInbound("a", DateTimeOffset.UtcNow);
        c.RecordInbound("b", DateTimeOffset.UtcNow);

        c.MarkRead();

        c.UnreadCount.Should().Be(0);
    }

    [Fact]
    public void Long_preview_is_truncated()
    {
        var c = New();
        c.RecordInbound(new string('x', 500), DateTimeOffset.UtcNow);

        c.LastMessagePreview!.Length.Should().Be(120);
        c.LastMessagePreview.Should().EndWith("...");
    }
}

public class MessageTests
{
    [Fact]
    public void Inbound_message_is_received_with_provider_id()
    {
        var m = Message.Inbound(Guid.NewGuid(), Guid.NewGuid(), "oi", "wamid.X");

        m.Direction.Should().Be(MessageDirection.Inbound);
        m.Status.Should().Be(MessageStatus.Received);
        m.ProviderMessageId.Should().Be("wamid.X");
    }

    [Fact]
    public void Queued_reply_becomes_sent_then_carries_provider_id()
    {
        var m = Message.QueuedReply(Guid.NewGuid(), Guid.NewGuid(), "resposta", Guid.NewGuid());
        m.Status.Should().Be(MessageStatus.Queued);

        m.MarkSent("wamid.Y");

        m.Status.Should().Be(MessageStatus.Sent);
        m.ProviderMessageId.Should().Be("wamid.Y");
    }

    [Fact]
    public void Failed_reply_records_reason()
    {
        var m = Message.QueuedReply(Guid.NewGuid(), Guid.NewGuid(), "x", Guid.NewGuid());

        m.MarkFailed("rate limited");

        m.Status.Should().Be(MessageStatus.Failed);
        m.FailureReason.Should().Be("rate limited");
    }
}
