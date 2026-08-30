using System.Text;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;
using SopaTalk.Api;
using SopaTalk.Api.Realtime;
using SopaTalk.Api.Security;
using SopaTalk.SharedContracts.Inbox;
using SopaTalk.SharedKernel.Messaging;
using SopaTalk.SharedKernel.MultiTenancy;

var builder = WebApplication.CreateBuilder(args);

// --- Logging -----------------------------------------------------------------
builder.Host.UseSerilog((context, loggerConfig) => loggerConfig
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console());

// --- Platform services ------------------------------------------------------
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
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
var signingKey = jwt["SigningKey"]
    ?? throw new InvalidOperationException("Jwt:SigningKey is not configured (use user-secrets in dev).");

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
    .AddPolicy("admin", policy => policy.RequireRole("Owner", "Admin"))
    .AddPolicy("owner", policy => policy.RequireRole("Owner"));

// --- Background jobs -------------------------------------------------------
var connectionString = builder.Configuration.GetConnectionString("Database")
    ?? throw new InvalidOperationException("ConnectionStrings:Database is not configured.");

builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UsePostgreSqlStorage(options => options.UseNpgsqlConnection(connectionString)));
builder.Services.AddHangfireServer();

// --- Modules -------------------------------------------------------------
foreach (var module in ModuleRegistry.All)
{
    module.AddModule(builder.Services, builder.Configuration);
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    foreach (var module in ModuleRegistry.All)
    {
        await module.MigrateAsync(app.Services, CancellationToken.None);
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
