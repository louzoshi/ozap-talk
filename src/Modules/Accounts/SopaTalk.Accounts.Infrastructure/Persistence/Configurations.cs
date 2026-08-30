using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SopaTalk.Accounts.Domain.Accounts;
using SopaTalk.Accounts.Domain.Users;

namespace SopaTalk.Accounts.Infrastructure.Persistence;

internal sealed class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("accounts");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Name).HasMaxLength(120).IsRequired();
        builder.Property(a => a.Slug).HasMaxLength(40).IsRequired();
        builder.HasIndex(a => a.Slug).IsUnique();
        builder.Property(a => a.Plan).HasConversion<string>().HasMaxLength(20);
        builder.Ignore(a => a.DomainEvents);
    }
}

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.TenantId).IsRequired();
        builder.Property(u => u.Email).HasMaxLength(256).IsRequired();
        builder.Property(u => u.DisplayName).HasMaxLength(120).IsRequired();
        builder.Property(u => u.PasswordHash).HasMaxLength(256).IsRequired();
        builder.Property(u => u.Role).HasConversion<string>().HasMaxLength(20);

        // Email is the global sign-in identifier — unique across all tenants.
        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasIndex(u => u.TenantId);
        builder.Ignore(u => u.DomainEvents);
    }
}
