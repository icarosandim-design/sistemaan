using System.Reflection;
using Microsoft.EntityFrameworkCore;
using SistemaAN.Application.Common.Interfaces;
using SistemaAN.Domain.Catalog;
using SistemaAN.Domain.Clientes;
using SistemaAN.Domain.Consumo;
using SistemaAN.Domain.Entregas;
using SistemaAN.Domain.Estoque;
using SistemaAN.Domain.Identity;
using SistemaAN.Domain.Pacotes;
using SistemaAN.Domain.Pedidos;
using SistemaAN.Domain.Pets;
using SistemaAN.Domain.Planos;
using SistemaAN.Domain.Producao;
using SistemaAN.Domain.Receitas;
using SistemaAN.Domain.Rotas;

namespace SistemaAN.Infrastructure.Persistence;

/// <summary>
/// Contexto principal do EF Core. As configurações de mapeamento são aplicadas
/// por reflexão a partir das classes <c>IEntityTypeConfiguration</c> deste assembly.
/// </summary>
public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();

    public DbSet<Papel> Papeis => Set<Papel>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<CategoriaIngrediente> CategoriasIngredientes => Set<CategoriaIngrediente>();

    public DbSet<Ingrediente> Ingredientes => Set<Ingrediente>();

    public DbSet<FaixaConsumo> FaixasConsumo => Set<FaixaConsumo>();

    public DbSet<Receita> Receitas => Set<Receita>();

    public DbSet<ItemReceita> ItensReceita => Set<ItemReceita>();

    public DbSet<FrequenciaEntrega> FrequenciasEntrega => Set<FrequenciaEntrega>();

    public DbSet<Cliente> Clientes => Set<Cliente>();

    public DbSet<ClientePj> ClientesPj => Set<ClientePj>();

    public DbSet<OrigemVenda> OrigensVenda => Set<OrigemVenda>();

    public DbSet<MotivoCancelamento> MotivosCancelamento => Set<MotivoCancelamento>();

    public DbSet<Pet> Pets => Set<Pet>();

    public DbSet<Raca> Racas => Set<Raca>();

    public DbSet<Doenca> Doencas => Set<Doenca>();

    public DbSet<PetDoenca> PetDoencas => Set<PetDoenca>();

    public DbSet<TamanhoPacote> TamanhosPacote => Set<TamanhoPacote>();

    public DbSet<PlanoAlimentar> PlanosAlimentares => Set<PlanoAlimentar>();

    public DbSet<PlanoItemReceita> PlanoItensReceita => Set<PlanoItemReceita>();

    public DbSet<PlanoItemPacote> PlanoItemPacotes => Set<PlanoItemPacote>();

    public DbSet<Entrega> Entregas => Set<Entrega>();

    public DbSet<Fornecedor> Fornecedores => Set<Fornecedor>();

    public DbSet<ItemEstoque> ItensEstoque => Set<ItemEstoque>();

    public DbSet<LoteEstoque> LotesEstoque => Set<LoteEstoque>();

    public DbSet<MovimentacaoEstoque> MovimentacoesEstoque => Set<MovimentacaoEstoque>();

    public DbSet<EntradaEstoque> EntradasEstoque => Set<EntradaEstoque>();

    public DbSet<AjusteEstoque> AjustesEstoque => Set<AjusteEstoque>();

    public DbSet<OrdemProducao> OrdensProducao => Set<OrdemProducao>();

    public DbSet<FichaProducao> FichasProducao => Set<FichaProducao>();

    public DbSet<ConsumoIngredienteProducao> ConsumosProducao => Set<ConsumoIngredienteProducao>();

    public DbSet<Pedido> Pedidos => Set<Pedido>();

    public DbSet<PedidoItem> PedidoItens => Set<PedidoItem>();

    public DbSet<Rota> Rotas => Set<Rota>();

    public DbSet<RotaParada> RotaParadas => Set<RotaParada>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("public");
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
