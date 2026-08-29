using Hangfire;
using Hangfire.PostgreSql;
using Scalar.AspNetCore;
using Serilog;
using SopaTalk.Api;

var builder = WebApplication.CreateBuilder(args);

// --- Logging -------------------------------------------------------------------
builder.Host.UseSerilog((context, loggerConfig) => loggerConfig
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console());

// --- Platform services --------------------------------------------------------
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();
builder.Services.AddAuthentication().AddJwtBearer();
builder.Services.AddAuthorization();

var connectionString = builder.Configuration.GetConnectionString("Database")
    ?? throw new InvalidOperationException("ConnectionStrings:Database is not configured.");

builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UsePostgreSqlStorage(options => options.UseNpgsqlConnection(connectionString)));
builder.Services.AddHangfireServer();

// --- Modules -----------------------------------------------------------------
foreach (var module in ModuleRegistry.All)
{
    module.AddModule(builder.Services, builder.Configuration);
}

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseDefaultFiles();
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapHangfireDashboard("/jobs"); // TODO: lock this down to platform admins before any real deploy.

foreach (var module in ModuleRegistry.All)
{
    module.MapEndpoints(app);
}

// SPA fallback: the React/Vite build is served as static files from wwwroot in production.
app.MapFallbackToFile("index.html");

app.Run();

public partial class Program;
