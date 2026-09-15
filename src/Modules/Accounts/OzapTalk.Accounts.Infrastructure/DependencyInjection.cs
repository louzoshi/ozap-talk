using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OzapTalk.Accounts.Application.Abstractions;
using OzapTalk.Accounts.Infrastructure.Jobs;
using OzapTalk.Accounts.Infrastructure.Notifications;
using OzapTalk.Accounts.Infrastructure.Persistence;
using OzapTalk.Accounts.Infrastructure.Security;

namespace OzapTalk.Accounts.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAccountsInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database")
            ?? throw new InvalidOperationException("ConnectionStrings:Database is not configured.");

        services.AddDbContext<AccountsDbContext>(options => options
            .UseNpgsql(connectionString, npgsql => npgsql
                .MigrationsHistoryTable("__ef_migrations_history", AccountsDbContext.Schema)));

        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IInvitationRepository, InvitationRepository>();
        services.AddScoped<IAccountsUnitOfWork>(sp => sp.GetRequiredService<AccountsDbContext>());

        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddSingleton<IAccessTokenIssuer, JwtAccessTokenIssuer>();
        services.AddSingleton<IInvitationTokens, InvitationTokens>();
        services.AddSingleton<IInviteLinkBuilder, InviteLinkBuilder>();
        services.AddScoped<IInvitationEmailSender, InvitationEmailSender>();

        services.AddScoped<WeeklyDigestJob>();

        services.AddOptions<AccountsOptions>()
            .Bind(configuration.GetSection(AccountsOptions.SectionName));

        // Not ValidateOnStart: the workers host also calls this method but never issues
        // tokens. The API validates Jwt config explicitly at startup in Program.cs.
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateDataAnnotations();

        return services;
    }
}
