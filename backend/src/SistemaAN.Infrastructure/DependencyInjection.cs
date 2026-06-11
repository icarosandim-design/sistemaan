using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SistemaAN.Application.Common.Interfaces;
using SistemaAN.Application.Identity;
using SistemaAN.Infrastructure.Common;
using SistemaAN.Infrastructure.Identity;
using SistemaAN.Infrastructure.Persistence;
using SistemaAN.Infrastructure.Persistence.Interceptors;
using SistemaAN.Infrastructure.Persistence.Seed;

namespace SistemaAN.Infrastructure;

/// <summary>
/// Registro da camada de Infraestrutura: persistência (EF Core + PostgreSQL),
/// auditoria, provedor de tempo, hashing de senha e autenticação JWT.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddPersistence(configuration);
        services.AddSecurity(configuration);
        return services;
    }

    private static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException(
                "Connection string 'Default' não configurada (ConnectionStrings__Default).");

        services.AddSingleton<AuditableEntityInterceptor>();

        services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
        {
            options
                .UseNpgsql(connectionString, npgsql =>
                    npgsql.MigrationsHistoryTable("__ef_migrations_history"))
                .UseSnakeCaseNamingConvention()
                .AddInterceptors(serviceProvider.GetRequiredService<AuditableEntityInterceptor>());
        });

        services.AddScoped<IApplicationDbContext>(sp =>
            sp.GetRequiredService<ApplicationDbContext>());

        services.AddScoped<IdentityDataSeeder>();
        services.AddScoped<CatalogDataSeeder>();
        services.AddScoped<ConsumoDataSeeder>();
        services.AddScoped<FrequenciasDataSeeder>();

        return services;
    }

    private static IServiceCollection AddSecurity(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtSettings = new JwtSettings();
        configuration.GetSection(JwtSettings.SectionName).Bind(jwtSettings);
        services.AddSingleton(jwtSettings);

        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        var key = string.IsNullOrWhiteSpace(jwtSettings.SecretKey)
            ? new string('0', 32)
            : jwtSettings.SecretKey;

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                    ClockSkew = TimeSpan.Zero,
                    NameClaimType = "sub",
                    RoleClaimType = JwtTokenGenerator.RoleClaimType,
                };
            });

        services.AddAuthorization();

        return services;
    }
}
