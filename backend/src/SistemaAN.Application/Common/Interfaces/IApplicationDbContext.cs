using Microsoft.EntityFrameworkCore;
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
using SistemaAN.Domain.Produtos;
using SistemaAN.Domain.Receitas;
using SistemaAN.Domain.Rotas;
using SistemaAN.Domain.Vendas;

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

    DbSet<ClientePj> ClientesPj { get; }

    DbSet<OrigemVenda> OrigensVenda { get; }

    DbSet<MotivoCancelamento> MotivosCancelamento { get; }

    DbSet<Pet> Pets { get; }

    DbSet<Raca> Racas { get; }

    DbSet<Doenca> Doencas { get; }

    DbSet<PetDoenca> PetDoencas { get; }

    DbSet<TamanhoPacote> TamanhosPacote { get; }

    DbSet<PlanoAlimentar> PlanosAlimentares { get; }

    DbSet<PlanoItemReceita> PlanoItensReceita { get; }

    DbSet<PlanoItemPacote> PlanoItemPacotes { get; }

    DbSet<Entrega> Entregas { get; }

    DbSet<Fornecedor> Fornecedores { get; }

    DbSet<ItemEstoque> ItensEstoque { get; }

    DbSet<LoteEstoque> LotesEstoque { get; }

    DbSet<MovimentacaoEstoque> MovimentacoesEstoque { get; }

    DbSet<EntradaEstoque> EntradasEstoque { get; }

    DbSet<AjusteEstoque> AjustesEstoque { get; }

    DbSet<OrdemProducao> OrdensProducao { get; }

    DbSet<FichaProducao> FichasProducao { get; }

    DbSet<ConsumoIngredienteProducao> ConsumosProducao { get; }

    DbSet<Pedido> Pedidos { get; }

    DbSet<PedidoItem> PedidoItens { get; }

    DbSet<Rota> Rotas { get; }

    DbSet<RotaParada> RotaParadas { get; }

    DbSet<Produto> Produtos { get; }

    DbSet<VendaAvulsa> VendasAvulsas { get; }

    DbSet<VendaAvulsaItem> VendasAvulsasItens { get; }

    DbSet<NotaCompra> NotasCompra { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
