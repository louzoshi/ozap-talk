using Microsoft.EntityFrameworkCore;
using SopaTalk.Channels.Application.Abstractions;
using SopaTalk.Channels.Domain.WhatsApp;
using SopaTalk.SharedKernel.MultiTenancy;
using SopaTalk.SharedKernel.Persistence;

namespace SopaTalk.Channels.Infrastructure.Persistence;

public sealed class ChannelsDbContext(DbContextOptions<ChannelsDbContext> options, ITenantContext tenantContext)
    : TenantDbContext(options, tenantContext), IChannelsUnitOfWork
{
    public const string Schema = "channels";

    public DbSet<WhatsAppChannel> Channels => Set<WhatsAppChannel>();
    public DbSet<ProcessedInboundMessage> ProcessedMessages => Set<ProcessedInboundMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ChannelsDbContext).Assembly);

        modelBuilder.Entity<WhatsAppChannel>().HasQueryFilter(c => c.TenantId == CurrentTenantId);
        modelBuilder.Entity<ProcessedInboundMessage>().HasQueryFilter(p => p.TenantId == CurrentTenantId);

        base.OnModelCreating(modelBuilder);
    }
}
