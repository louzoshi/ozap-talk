using Microsoft.EntityFrameworkCore;
using SopaTalk.Inbox.Application.Abstractions;
using SopaTalk.Inbox.Domain.Contacts;
using SopaTalk.Inbox.Domain.Conversations;
using SopaTalk.Inbox.Domain.Messages;
using SopaTalk.SharedKernel.MultiTenancy;
using SopaTalk.SharedKernel.Persistence;

namespace SopaTalk.Inbox.Infrastructure.Persistence;

public sealed class InboxDbContext(DbContextOptions<InboxDbContext> options, ITenantContext tenantContext)
    : TenantDbContext(options, tenantContext), IInboxUnitOfWork
{
    public const string Schema = "inbox";

    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<Message> Messages => Set<Message>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InboxDbContext).Assembly);

        modelBuilder.Entity<Contact>().HasQueryFilter(c => c.TenantId == CurrentTenantId);
        modelBuilder.Entity<Conversation>().HasQueryFilter(c => c.TenantId == CurrentTenantId);
        modelBuilder.Entity<Message>().HasQueryFilter(m => m.TenantId == CurrentTenantId);

        base.OnModelCreating(modelBuilder);
    }
}
