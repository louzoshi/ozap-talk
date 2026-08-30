using Hangfire;
using Hangfire.PostgreSql;
using Serilog;
using SopaTalk.Accounts.Infrastructure;
using SopaTalk.AiAgent.Infrastructure;
using SopaTalk.Channels.Infrastructure;
using SopaTalk.Chatbot.Infrastructure;
using SopaTalk.Crm.Infrastructure;
using SopaTalk.Inbox.Infrastructure;
using SopaTalk.SharedKernel.Messaging;
using SopaTalk.SharedKernel.MultiTenancy;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSerilog(config => config
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console());

var connectionString = builder.Configuration.GetConnectionString("Database")
    ?? throw new InvalidOperationException("ConnectionStrings:Database is not configured.");

builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UsePostgreSqlStorage(options => options.UseNpgsqlConnection(connectionString)));
builder.Services.AddHangfireServer();

// Each job sets its own tenant on the scoped context before touching a module DbContext.
builder.Services.AddScoped<TenantContext>();
builder.Services.AddScoped<ITenantContext>(sp => sp.GetRequiredService<TenantContext>());
builder.Services.AddScoped<ISettableTenantContext>(sp => sp.GetRequiredService<TenantContext>());

builder.Services.AddInProcessEventBus();

// This process runs background jobs only — it registers each module's infrastructure
// (DbContexts, repositories, job handlers) but no HTTP endpoints.
builder.Services.AddAccountsInfrastructure(builder.Configuration);
builder.Services.AddChannelsInfrastructure(builder.Configuration);
builder.Services.AddInboxInfrastructure(builder.Configuration);
builder.Services.AddCrmInfrastructure(builder.Configuration);
builder.Services.AddChatbotInfrastructure(builder.Configuration);
builder.Services.AddAiAgentInfrastructure(builder.Configuration);

var host = builder.Build();
host.Run();
