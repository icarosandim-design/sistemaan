using SistemaAN.Domain.Common;

namespace SistemaAN.Domain.Catalog;

/// <summary>
/// Ingrediente: base para receitas, custo e produção. O custo é informado
/// manualmente nesta fase; o coeficiente é o rendimento (cozido ÷ cru).
/// </summary>
public class Ingrediente : AuditableEntity
{
    private Ingrediente() { } // EF Core

    private Ingrediente(string nome, long categoriaId, TipoConversao tipoConversao, decimal coeficiente, decimal custoAtualKg)
    {
        Nome = nome;
        CategoriaId = categoriaId;
        TipoConversao = tipoConversao;
        CoeficienteConversao = coeficiente;
        CustoAtualKg = custoAtualKg;
        Ativo = true;
    }

    public string Nome { get; private set; } = string.Empty;

    public long CategoriaId { get; private set; }

    public CategoriaIngrediente? Categoria { get; private set; }

    public TipoConversao TipoConversao { get; private set; }

    /// <summary>Rendimento = peso cozido ÷ peso cru. "Sem conversão" sempre = 1.</summary>
    public decimal CoeficienteConversao { get; private set; }

    public decimal CustoAtualKg { get; private set; }

    public bool Ativo { get; private set; }

    public static Ingrediente Criar(
        string nome,
        long categoriaId,
        TipoConversao tipoConversao,
        decimal coeficiente,
        decimal custoAtualKg)
        => new(nome.Trim(), categoriaId, tipoConversao, Normalizar(tipoConversao, coeficiente), custoAtualKg);

    public void Atualizar(
        string nome,
        long categoriaId,
        TipoConversao tipoConversao,
        decimal coeficiente,
        decimal custoAtualKg,
        bool ativo)
    {
        Nome = nome.Trim();
        CategoriaId = categoriaId;
        TipoConversao = tipoConversao;
        CoeficienteConversao = Normalizar(tipoConversao, coeficiente);
        CustoAtualKg = custoAtualKg;
        Ativo = ativo;
    }

    public void DefinirAtivo(bool ativo) => Ativo = ativo;

    private static decimal Normalizar(TipoConversao tipo, decimal coeficiente)
        => tipo == TipoConversao.SemConversao ? 1m : coeficiente;
}
