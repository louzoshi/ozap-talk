using FluentAssertions;
using SopaTalk.Inbox.Domain.Conversations;
using SopaTalk.Inbox.Domain.Conversations.Events;

namespace SopaTalk.Inbox.UnitTests.Conversations;

public class ConversationTests
{
    private static Conversation NewConversation() =>
        Conversation.Start(Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7());

    [Fact]
    public void Start_opens_the_conversation_and_raises_event()
    {
        var conversation = NewConversation();

        conversation.Status.Should().Be(ConversationStatus.Open);
        conversation.DomainEvents.Should().ContainSingle(e => e is ConversationStarted);
    }

    [Fact]
    public void AssignTo_sets_the_agent_and_raises_event()
    {
        var conversation = NewConversation();
        var agentId = Guid.CreateVersion7();

        var result = conversation.AssignTo(agentId);

        result.IsSuccess.Should().BeTrue();
        conversation.AssignedAgentId.Should().Be(agentId);
        conversation.DomainEvents.Should().Contain(e => e is ConversationAssigned);
    }

    [Fact]
    public void AssignTo_fails_when_conversation_is_closed()
    {
        var conversation = NewConversation();
        conversation.Close();

        var result = conversation.AssignTo(Guid.CreateVersion7());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ConversationErrors.ClosedCannotBeAssigned);
    }

    [Fact]
    public void Close_twice_fails()
    {
        var conversation = NewConversation();
        conversation.Close();

        var result = conversation.Close();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ConversationErrors.AlreadyClosed);
    }

    [Fact]
    public void AddNote_rejects_empty_body()
    {
        var conversation = NewConversation();

        var act = () => conversation.AddNote(Guid.CreateVersion7(), "   ");

        act.Should().Throw<ArgumentException>();
    }
}
