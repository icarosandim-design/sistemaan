using Microsoft.Extensions.DependencyInjection;
using SistemaAN.Application.Catalog;
using SistemaAN.Application.Consumo;
using SistemaAN.Application.Entregas;
using SistemaAN.Application.Identity;
using SistemaAN.Application.Receitas;

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
        services.AddScoped<IIngredienteService, IngredienteService>();
        services.AddScoped<IFaixaConsumoService, FaixaConsumoService>();
        services.AddScoped<IReceitaCasaService, ReceitaCasaService>();
        services.AddScoped<IFrequenciaEntregaService, FrequenciaEntregaService>();
        return services;
    }
}
