using IdentityCenter.Application.Interfaces;
using IdentityCenter.Application.Services;
using IdentityCenter.Infrastructure.Bootstrap;
using IdentityCenter.Infrastructure.Context;
using IdentityCenter.Infrastructure.Repositories;
using IdentityCenter.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityCenter.Infrastructure;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddIdentityInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("IdentityPostgreSQL")
            ?? throw new InvalidOperationException(
                "La cadena de conexión IdentityPostgreSQL no está configurada. " +
                "Defina la variable de entorno ConnectionStrings__IdentityPostgreSQL.");

        services.AddDbContext<IdentityDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.Configure<BootstrapOptions>(
            configuration.GetSection(BootstrapOptions.SectionName));

        // Repositories
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IRolRepository, RolRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Security
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        // Application Services
        services.AddScoped<TenantService>();
        services.AddScoped<RolService>();
        services.AddScoped<UsuarioService>();
        services.AddScoped<AuthService>();
        services.AddScoped<IdentityBootstrapper>();

        return services;
    }
}
