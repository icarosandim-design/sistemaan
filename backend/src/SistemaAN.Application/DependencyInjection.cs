using Microsoft.Extensions.DependencyInjection;

namespace SistemaAN.Application;

/// <summary>
/// Registro da camada de Aplicação. Casos de uso (handlers), validadores e
/// serviços de cada módulo de negócio serão adicionados aqui nas próximas etapas.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Placeholder intencional: a fundação não registra regras de negócio.
        return services;
    }
}
