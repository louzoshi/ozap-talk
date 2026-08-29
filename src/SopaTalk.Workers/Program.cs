using Hangfire;
using Hangfire.PostgreSql;
using Serilog;
using SopaTalk.Channels.Infrastructure;
using SopaTalk.Inbox.Infrastructure;
using SopaTalk.Crm.Infrastructure;
using SopaTalk.Chatbot.Infrastructure;
using SopaTalk.AiAgent.Infrastructure;

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

// This process runs background jobs only — it registers each module's infrastructure
// (DbContexts, repositories, job handlers) but no HTTP endpoints.
builder.Services.AddChannelsInfrastructure(builder.Configuration);
builder.Services.AddInboxInfrastructure(builder.Configuration);
builder.Services.AddCrmInfrastructure(builder.Configuration);
builder.Services.AddChatbotInfrastructure(builder.Configuration);
builder.Services.AddAiAgentInfrastructure(builder.Configuration);

var host = builder.Build();
host.Run();
