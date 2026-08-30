using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SopaTalk.Channels.Domain.WhatsApp;

namespace SopaTalk.Channels.Infrastructure.Persistence;

internal sealed class WhatsAppChannelConfiguration : IEntityTypeConfiguration<WhatsAppChannel>
{
    public void Configure(EntityTypeBuilder<WhatsAppChannel> builder)
    {
        builder.ToTable("whatsapp_channels");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.PhoneNumberId).HasMaxLength(64).IsRequired();
        builder.Property(c => c.DisplayPhoneNumber).HasMaxLength(32);
        builder.Property(c => c.WabaId).HasMaxLength(64);
        builder.Property(c => c.AccessToken).IsRequired();
        builder.HasIndex(c => c.PhoneNumberId).IsUnique();
        builder.HasIndex(c => c.TenantId);
        builder.Ignore(c => c.DomainEvents);
    }
}

internal sealed class ProcessedInboundMessageConfiguration : IEntityTypeConfiguration<ProcessedInboundMessage>
{
    public void Configure(EntityTypeBuilder<ProcessedInboundMessage> builder)
    {
        builder.ToTable("processed_inbound_messages");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.ProviderMessageId).HasMaxLength(128).IsRequired();
        builder.HasIndex(p => new { p.TenantId, p.ProviderMessageId }).IsUnique();
    }
}
