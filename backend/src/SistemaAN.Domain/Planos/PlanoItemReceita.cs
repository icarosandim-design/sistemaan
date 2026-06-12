using SistemaAN.Domain.Common;

namespace SistemaAN.Domain.Planos;

/// <summary>
/// Receita escolhida no plano. Para Casa: <see cref="QuantidadeCicloGramas"/>
/// (necessário/distribuído) + pacotes por tamanho. Para Personalizada:
/// <see cref="QuantidadePacotes"/> (o pacote é o tamanho da própria receita).
/// </summary>
public class PlanoItemReceita : Entity
{
    private readonly List<PlanoItemPacote> _pacotes = [];

    private PlanoItemReceita() { } // EF Core

    private PlanoItemReceita(long receitaId, int? quantidadeCicloGramas, int? quantidadePacotes)
    {
        ReceitaId = receitaId;
        QuantidadeCicloGramas = quantidadeCicloGramas;
        QuantidadePacotes = quantidadePacotes;
    }

    public long PlanoAlimentarId { get; private set; }
    public long ReceitaId { get; private set; }

    /// <summary>Casa: gramas necessárias/distribuídas da receita no ciclo.</summary>
    public int? QuantidadeCicloGramas { get; private set; }

    /// <summary>Personalizada: nº de pacotes da receita no ciclo.</summary>
    public int? QuantidadePacotes { get; private set; }

    public IReadOnlyCollection<PlanoItemPacote> Pacotes => _pacotes.AsReadOnly();

    public static PlanoItemReceita Criar(
        long receitaId,
        int? quantidadeCicloGramas,
        int? quantidadePacotes,
        IEnumerable<PlanoItemPacote> pacotes)
    {
        var item = new PlanoItemReceita(receitaId, quantidadeCicloGramas, quantidadePacotes);
        item._pacotes.AddRange(pacotes);
        return item;
    }
}
