using Hangfire;
using Hangfire.PostgreSql;
using OzapTalk.Accounts.Infrastructure;
using OzapTalk.AiAgent.Infrastructure;
using OzapTalk.Channels.Infrastructure;
using OzapTalk.Chatbot.Infrastructure;
using OzapTalk.Crm.Infrastructure;
using OzapTalk.Inbox.Infrastructure;
using OzapTalk.SharedKernel.Messaging;
using OzapTalk.SharedKernel.MultiTenancy;
using OzapTalk.SharedKernel.Notifications;
using Serilog;

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

builder.Services.AddEmail(builder.Configuration);
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
