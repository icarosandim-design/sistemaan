using SistemaAN.Domain.Common;

namespace SistemaAN.Domain.Producao;

/// <summary>
/// Consumo consolidado de um ingrediente na ordem (somando todas as fichas). Guarda o
/// planejado (cozido/cru) e, na finalização, o real (cru/cozido) que baixa o estoque.
/// </summary>
public class ConsumoIngredienteProducao : Entity
{
    private ConsumoIngredienteProducao() { } // EF Core

    private ConsumoIngredienteProducao(
        long ingredienteId, string ingredienteNome, long? itemEstoqueId, string? itemEstoqueNome,
        decimal coeficiente, string? unidadeEstoque, int planejadoCozidoGramas, int planejadoCruGramas)
    {
        IngredienteId = ingredienteId;
        IngredienteNome = ingredienteNome;
        ItemEstoqueId = itemEstoqueId;
        ItemEstoqueNome = itemEstoqueNome;
        Coeficiente = coeficiente;
        UnidadeEstoque = unidadeEstoque;
        PlanejadoCozidoGramas = planejadoCozidoGramas;
        PlanejadoCruGramas = planejadoCruGramas;
        BaixaRealizada = false;
    }

    public long OrdemProducaoId { get; private set; }
    public long IngredienteId { get; private set; }
    public string IngredienteNome { get; private set; } = string.Empty;
    public long? ItemEstoqueId { get; private set; }
    public string? ItemEstoqueNome { get; private set; }
    public decimal Coeficiente { get; private set; }
    public string? UnidadeEstoque { get; private set; }

    public int PlanejadoCozidoGramas { get; private set; }
    public int PlanejadoCruGramas { get; private set; }

    public decimal? RealCruGramas { get; private set; }
    public decimal? RealCozidoGramas { get; private set; }
    public bool BaixaRealizada { get; private set; }
    public string? Observacao { get; private set; }

    public static ConsumoIngredienteProducao Criar(
        long ingredienteId, string ingredienteNome, long? itemEstoqueId, string? itemEstoqueNome,
        decimal coeficiente, string? unidadeEstoque, int planejadoCozidoGramas, int planejadoCruGramas)
        => new(ingredienteId, ingredienteNome, itemEstoqueId, itemEstoqueNome, coeficiente, unidadeEstoque, planejadoCozidoGramas, planejadoCruGramas);

    public void RegistrarReal(decimal? realCruGramas, decimal? realCozidoGramas, string? observacao)
    {
        RealCruGramas = realCruGramas;
        RealCozidoGramas = realCozidoGramas;
        Observacao = observacao;
    }

    public void MarcarBaixaRealizada() => BaixaRealizada = true;
}
