using IdentityCenter.API.Middleware;
using IdentityCenter.API.Security;
using IdentityCenter.API.Swagger;
using IdentityCenter.Infrastructure.Bootstrap;
using IdentityCenter.Infrastructure;
using IdentityCenter.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var allowedOrigins = builder.Configuration
    .GetSection("AuthSecurity:AllowedOrigins")
    .Get<string[]>() ?? [];

// ── Servicios ────────────────────────────────────────────────
builder.Services.AddIdentityInfrastructure(builder.Configuration);
builder.Services.Configure<AuthSecurityOptions>(
    builder.Configuration.GetSection(AuthSecurityOptions.SectionName));
builder.Services.AddCors(options =>
{
    options.AddPolicy("IdentityCenterFrontend", policy =>
    {
        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }
    });
});
builder.Services.AddSingleton<AuthCookieService>();
builder.Services.AddSingleton<ClientSessionService>();
builder.Services.AddControllers();
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
