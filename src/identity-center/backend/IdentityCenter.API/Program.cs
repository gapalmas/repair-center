using IdentityCenter.API.Middleware;
using IdentityCenter.API.Security;
using IdentityCenter.API.Swagger;
using IdentityCenter.Infrastructure.Bootstrap;
using IdentityCenter.Infrastructure;
using IdentityCenter.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

var allowedOrigins = builder.Configuration
    .GetSection("AuthSecurity:AllowedOrigins")
    .Get<string[]>() ?? [];
var normalizedAllowedOrigins = allowedOrigins
    .Where(origin => !string.IsNullOrWhiteSpace(origin))
    .Select(origin => origin.Trim().TrimEnd('/'))
    .ToHashSet(StringComparer.OrdinalIgnoreCase);

// ── Servicios ────────────────────────────────────────────────
builder.Services.AddIdentityInfrastructure(builder.Configuration);
builder.Services.Configure<AuthSecurityOptions>(
    builder.Configuration.GetSection(AuthSecurityOptions.SectionName));
builder.Services.AddCors(options =>
{
    options.AddPolicy("IdentityCenterFrontend", policy =>
    {
        policy
            .SetIsOriginAllowed(origin =>
            {
                if (string.IsNullOrWhiteSpace(origin))
                {
                    return false;
                }

                var normalizedOrigin = origin.Trim().TrimEnd('/');
                if (normalizedAllowedOrigins.Contains(normalizedOrigin))
                {
                    return true;
                }

                if (!Uri.TryCreate(normalizedOrigin, UriKind.Absolute, out var uri))
                {
                    return false;
                }

                return uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
                    || uri.Host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase);
            })
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});
builder.Services.AddSingleton<AuthCookieService>();
builder.Services.AddSingleton<ClientSessionService>();
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "IdentityCenter API", Version = "v1" });
    c.OperationFilter<AuthCookieOperationFilter>();
});

var app = builder.Build();

// ── Inicializacion de base y bootstrap ───────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
    var bootstrapper = scope.ServiceProvider.GetRequiredService<IdentityBootstrapper>();
    var bootstrapOptions = scope.ServiceProvider
        .GetRequiredService<Microsoft.Extensions.Options.IOptions<BootstrapOptions>>()
        .Value;

    if (bootstrapOptions.ApplyMigrationsOnStartup)
    {
        await db.Database.MigrateAsync();

        // Safety net: some local databases may have inconsistent migration history.
        // Ensure this required column exists before auth queries run.
        await db.Database.ExecuteSqlRawAsync(@"
            ALTER TABLE IF EXISTS identity.usuarios
            ADD COLUMN IF NOT EXISTS ""DebeCambiarPassword"" boolean NOT NULL DEFAULT FALSE;");
    }

    await bootstrapper.RunAsync(CancellationToken.None);
}

// ── Middleware Pipeline ──────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseStaticFiles();
    app.UseSwaggerUI(c =>
    {
        c.InjectJavascript("/swagger/swagger-auth-session.js");
    });
}

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseCors("IdentityCenterFrontend");

app.MapControllers();

app.Run();
