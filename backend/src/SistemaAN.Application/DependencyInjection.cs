using Microsoft.Extensions.DependencyInjection;
using SistemaAN.Application.Catalog;
using SistemaAN.Application.Clientes;
using SistemaAN.Application.Consumo;
using SistemaAN.Application.Entregas;
using SistemaAN.Application.Estoque;
using SistemaAN.Application.Identity;
using SistemaAN.Application.Pacotes;
using SistemaAN.Application.Pets;
using SistemaAN.Application.Planos;
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
        services.AddScoped<IClienteService, ClienteService>();
        services.AddScoped<IPetService, PetService>();
        services.AddScoped<ITamanhoPacoteService, TamanhoPacoteService>();
        services.AddScoped<IReceitaPersonalizadaService, ReceitaPersonalizadaService>();
        services.AddScoped<IPlanoAlimentarService, PlanoAlimentarService>();
        services.AddScoped<IEntregaService, EntregaService>();
        services.AddScoped<IFornecedorService, FornecedorService>();
        services.AddScoped<IItemEstoqueService, ItemEstoqueService>();
        services.AddScoped<IEstoqueMovimentacaoService, EstoqueMovimentacaoService>();
        return services;
    }
}
