using Microsoft.Extensions.DependencyInjection;
using SistemaAN.Application.Identity;

namespace SistemaAN.Application;

/// <summary>
/// Registro da camada de Aplicação. Casos de uso/serviços de cada módulo são
/// adicionados aqui.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        return services;
    }
}
