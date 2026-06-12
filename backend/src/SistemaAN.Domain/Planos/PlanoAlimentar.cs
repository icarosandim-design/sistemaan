using SistemaAN.Domain.Common;
using SistemaAN.Domain.Receitas;

namespace SistemaAN.Domain.Planos;

/// <summary>
/// Plano Alimentar vigente de um pet (1:1). Define o que o pet come (tipo,
/// receitas, gramas/dia, pacotes). A entrega (data + frequência) pertence ao
/// Cliente (compartilhada por todos os pets). Totais/custos são derivados.
/// </summary>
public class PlanoAlimentar : AuditableEntity
{
    private readonly List<PlanoItemReceita> _itens = [];

    private PlanoAlimentar() { } // EF Core

    private PlanoAlimentar(long petId, DadosPlano dados)
    {
        PetId = petId;
        Ativo = true;
        Aplicar(dados);
    }

    public long PetId { get; private set; }
    public int? GramasDiaSugeridas { get; private set; }
    public int? GramasDiaAjustadas { get; private set; }
    public TipoReceita Tipo { get; private set; }
    public bool Ativo { get; private set; }

    public IReadOnlyCollection<PlanoItemReceita> Itens => _itens.AsReadOnly();

    public static PlanoAlimentar Criar(long petId, DadosPlano dados) => new(petId, dados);

    public void Atualizar(DadosPlano dados) => Aplicar(dados);

    public void DefinirAtivo(bool ativo) => Ativo = ativo;

    public void SubstituirItens(IEnumerable<PlanoItemReceita> itens)
    {
        _itens.Clear();
        _itens.AddRange(itens);
    }

    private void Aplicar(DadosPlano d)
    {
        GramasDiaSugeridas = d.GramasDiaSugeridas;
        GramasDiaAjustadas = d.GramasDiaAjustadas;
        Tipo = d.Tipo;
    }
}

public sealed record DadosPlano(
    int? GramasDiaSugeridas,
    int? GramasDiaAjustadas,
    TipoReceita Tipo);
