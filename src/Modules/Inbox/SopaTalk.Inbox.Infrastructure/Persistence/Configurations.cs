using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SopaTalk.Inbox.Domain.Contacts;
using SopaTalk.Inbox.Domain.Conversations;
using SopaTalk.Inbox.Domain.Messages;

namespace SopaTalk.Inbox.Infrastructure.Persistence;

internal sealed class ContactConfiguration : IEntityTypeConfiguration<Contact>
{
    public void Configure(EntityTypeBuilder<Contact> builder)
    {
        builder.ToTable("contacts");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Phone).HasMaxLength(32).IsRequired();
        builder.Property(c => c.Name).HasMaxLength(120);
        builder.HasIndex(c => new { c.TenantId, c.Phone }).IsUnique();
        builder.Ignore(c => c.DomainEvents);
        builder.Ignore(c => c.DisplayName);
    }
}

internal sealed class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.ToTable("conversations");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(c => c.LastMessagePreview).HasMaxLength(160);
        builder.HasIndex(c => new { c.TenantId, c.Status, c.LastActivityAtUtc });
        builder.HasIndex(c => new { c.ChannelId, c.ContactId });

        builder.OwnsMany(c => c.Notes, note =>
        {
            note.ToTable("conversation_notes");
            note.WithOwner().HasForeignKey(n => n.ConversationId);
            note.HasKey(n => n.Id);
            note.Property(n => n.Body).HasMaxLength(4000).IsRequired();
        });

        builder.Ignore(c => c.DomainEvents);
    }
}

internal sealed class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("messages");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Direction).HasConversion<string>().HasMaxLength(20);
        builder.Property(m => m.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(m => m.Body).HasMaxLength(4096).IsRequired();
        builder.Property(m => m.ProviderMessageId).HasMaxLength(128);
        builder.Property(m => m.FailureReason).HasMaxLength(500);
        builder.HasIndex(m => new { m.ConversationId, m.CreatedAtUtc });
        builder.Ignore(m => m.DomainEvents);
    }
}
