using Microsoft.EntityFrameworkCore;
using SistemaAN.Domain.Catalog;
using SistemaAN.Domain.Clientes;
using SistemaAN.Domain.Consumo;
using SistemaAN.Domain.Entregas;
using SistemaAN.Domain.Identity;
using SistemaAN.Domain.Pacotes;
using SistemaAN.Domain.Pets;
using SistemaAN.Domain.Planos;
using SistemaAN.Domain.Receitas;

namespace SistemaAN.Application.Common.Interfaces;

/// <summary>
/// Abstração do contexto de persistência exposta à camada de Aplicação.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<Usuario> Usuarios { get; }

    DbSet<Papel> Papeis { get; }

    DbSet<RefreshToken> RefreshTokens { get; }

    DbSet<CategoriaIngrediente> CategoriasIngredientes { get; }

    DbSet<Ingrediente> Ingredientes { get; }

    DbSet<FaixaConsumo> FaixasConsumo { get; }

    DbSet<Receita> Receitas { get; }

    DbSet<ItemReceita> ItensReceita { get; }

    DbSet<FrequenciaEntrega> FrequenciasEntrega { get; }

    DbSet<Cliente> Clientes { get; }

    DbSet<Pet> Pets { get; }

    DbSet<TamanhoPacote> TamanhosPacote { get; }

    DbSet<PlanoAlimentar> PlanosAlimentares { get; }

    DbSet<PlanoItemReceita> PlanoItensReceita { get; }

    DbSet<PlanoItemPacote> PlanoItemPacotes { get; }

    DbSet<Entrega> Entregas { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
