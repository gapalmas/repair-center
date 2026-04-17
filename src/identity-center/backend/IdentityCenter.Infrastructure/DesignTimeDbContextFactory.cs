using IdentityCenter.Infrastructure.Context;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace IdentityCenter.Infrastructure;

public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<IdentityDbContext>
{
    public IdentityDbContext CreateDbContext(string[] args)
    {
        var connectionString = ResolveConnectionString();

        var builder = new DbContextOptionsBuilder<IdentityDbContext>();
        builder.UseNpgsql(connectionString);

        return new IdentityDbContext(builder.Options);
    }

    private static string ResolveConnectionString()
    {
        var fromEnvironment = Environment.GetEnvironmentVariable("ConnectionStrings__IdentityPostgreSQL");
        if (!string.IsNullOrWhiteSpace(fromEnvironment))
            return fromEnvironment;

        var apiProjectPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "IdentityCenter.API"));

        var fromUserSecrets = ReadConnectionStringFromUserSecrets("c6fb5ea3-85bb-4e1a-a01a-6f56da23445f");
        if (!string.IsNullOrWhiteSpace(fromUserSecrets))
            return fromUserSecrets;

        var fromDevelopmentSettings = ReadConnectionStringFromAppSettings(Path.Combine(apiProjectPath, "appsettings.Development.json"));
        if (!string.IsNullOrWhiteSpace(fromDevelopmentSettings))
            return fromDevelopmentSettings;

        var fromBaseSettings = ReadConnectionStringFromAppSettings(Path.Combine(apiProjectPath, "appsettings.json"));
        if (!string.IsNullOrWhiteSpace(fromBaseSettings))
            return fromBaseSettings;

        return "Host=localhost;Port=5432;Database=identitycenter_dev;Username=postgres;Password=dag0";
    }

    private static string? ReadConnectionStringFromAppSettings(string filePath)
    {
        if (!File.Exists(filePath))
            return null;

        using var document = JsonDocument.Parse(File.ReadAllText(filePath));

        if (!document.RootElement.TryGetProperty("ConnectionStrings", out var connectionStrings))
            return null;

        if (!connectionStrings.TryGetProperty("IdentityPostgreSQL", out var connectionString))
            return null;

        return connectionString.GetString();
    }

    private static string? ReadConnectionStringFromUserSecrets(string userSecretsId)
    {
        var secretsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Microsoft",
            "UserSecrets",
            userSecretsId,
            "secrets.json");

        if (!File.Exists(secretsPath))
            return null;

        using var document = JsonDocument.Parse(File.ReadAllText(secretsPath));

        if (!document.RootElement.TryGetProperty("ConnectionStrings:IdentityPostgreSQL", out var connectionString))
            return null;

        return connectionString.GetString();
    }
}
