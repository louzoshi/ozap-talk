using System.Text;
using System.Text.Json.Serialization;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using OzapTalk.Api;
using OzapTalk.Api.Jobs;
using OzapTalk.Api.Realtime;
using OzapTalk.Api.Security;
using OzapTalk.SharedContracts.Inbox;
using OzapTalk.SharedKernel.Messaging;
using OzapTalk.SharedKernel.MultiTenancy;
using OzapTalk.SharedKernel.Notifications;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// --- Logging -----------------------------------------------------------------
builder.Host.UseSerilog((context, loggerConfig) => loggerConfig
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console());

// --- Platform services ------------------------------------------------------
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();

// Enums trafegam como string ("Operator") no JSON da API, não como número.
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddHealthChecks();
builder.Services.AddHttpContextAccessor();

// Multi-tenancy: one tenant context per request/scope, set by TenantResolutionMiddleware.
builder.Services.AddScoped<TenantContext>();
builder.Services.AddScoped<ITenantContext>(sp => sp.GetRequiredService<TenantContext>());
builder.Services.AddScoped<ISettableTenantContext>(sp => sp.GetRequiredService<TenantContext>());

builder.Services.AddInProcessEventBus();
builder.Services.AddSignalR();
builder.Services.AddScoped<IIntegrationEventHandler<ConversationChanged>, ConversationChangedRelay>();

// --- Authentication / authorization ----------------------------------------
var jwt = builder.Configuration.GetSection("Jwt");
var signingKey = jwt["SigningKey"];
if (string.IsNullOrWhiteSpace(signingKey))
{
    throw new InvalidOperationException(
        "Jwt:SigningKey is not configured (user-secrets in dev, Jwt__SigningKey in production).");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwt["Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = "sub",
            RoleClaimType = "role",
        };

        // SignalR can't send an Authorization header on the WebSocket handshake —
        // accept the token from the query string for hub connections only.
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(accessToken) &&
                    context.HttpContext.Request.Path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            },
        };
    });

builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build())
    .AddPolicy("operator", policy => policy.RequireRole("Owner", "Admin", "Operator"))
    .AddPolicy("admin", policy => policy.RequireRole("Owner", "Admin"))
    .AddPolicy("owner", policy => policy.RequireRole("Owner"));

// --- Background jobs -------------------------------------------------------
var connectionString = builder.Configuration.GetConnectionString("Database")
    ?? throw new InvalidOperationException("ConnectionStrings:Database is not configured.");

builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UsePostgreSqlStorage(options => options.UseNpgsqlConnection(connectionString)));
builder.Services.AddHangfireServer();

builder.Services.AddEmail(builder.Configuration);

// --- Modules -------------------------------------------------------------
foreach (var module in ModuleRegistry.All)
{
    module.AddModule(builder.Services, builder.Configuration);
}

var app = builder.Build();

// As migrations sobem junto com o app quando Database:MigrateOnStartup está ligado — é o
// caso do deploy de instância única (ver deploy/macmini). Em Development, sempre roda.
if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("Database:MigrateOnStartup"))
{
    foreach (var module in ModuleRegistry.All)
    {
        await module.MigrateAsync(app.Services, CancellationToken.None);
    }
}

// Cada módulo declara seus jobs recorrentes; o host só empresta o agendador. O
// agendamento vive no Postgres, então registrar aqui basta — o OzapTalk.Workers executa
// o que estiver na fila sem precisar registrar de novo.
using (var scope = app.Services.CreateScope())
{
    var registry = new HangfireRecurringJobRegistry(
        scope.ServiceProvider.GetRequiredService<IRecurringJobManager>());

    foreach (var module in ModuleRegistry.All)
    {
        module.RegisterRecurringJobs(registry);
    }
}

app.UseSerilogRequestLogging();
app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseDefaultFiles();
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();
    app.MapScalarApiReference().AllowAnonymous();
}

app.UseAuthentication();
app.UseMiddleware<TenantResolutionMiddleware>();
app.UseAuthorization();

app.MapHealthChecks("/health").AllowAnonymous();
app.MapHub<InboxHub>("/hubs/inbox");

app.MapHangfireDashboard("/jobs", new DashboardOptions
{
    Authorization = [new HangfireDashboardAuthorizationFilter(app.Environment)],
}).AllowAnonymous();

foreach (var module in ModuleRegistry.All)
{
    module.MapEndpoints(app);
}

// SPA fallback: the React/Vite build is served as static files from wwwroot in production.
app.MapFallbackToFile("index.html").AllowAnonymous();

app.Run();

public partial class Program;
