using SistemaAN.Domain.Common;

namespace SistemaAN.Domain.Consumo;

/// <summary>
/// Faixa de peso → gramas/dia recomendadas. Base para sugerir o consumo de um
/// pet a partir do peso (usada futuramente em Pets e Plano Alimentar).
/// Faixa é FECHADA: aplica quando peso ∈ [PesoInicial, PesoFinal]. Faixas ativas
/// não podem se sobrepor nem se encostar (a próxima começa após o final da anterior).
/// </summary>
public class FaixaConsumo : AuditableEntity
{
    private FaixaConsumo() { } // EF Core

    private FaixaConsumo(decimal pesoInicial, decimal pesoFinal, int gramasPorDia)
    {
        PesoInicial = pesoInicial;
        PesoFinal = pesoFinal;
        GramasPorDia = gramasPorDia;
        Ativo = true;
    }

    public decimal PesoInicial { get; private set; }

    public decimal PesoFinal { get; private set; }

    public int GramasPorDia { get; private set; }

    public bool Ativo { get; private set; }

    public static FaixaConsumo Criar(decimal pesoInicial, decimal pesoFinal, int gramasPorDia)
        => new(pesoInicial, pesoFinal, gramasPorDia);

    public void Atualizar(decimal pesoInicial, decimal pesoFinal, int gramasPorDia, bool ativo)
    {
        PesoInicial = pesoInicial;
        PesoFinal = pesoFinal;
        GramasPorDia = gramasPorDia;
        Ativo = ativo;
    }

    public void DefinirAtivo(bool ativo) => Ativo = ativo;

    /// <summary>Indica se a faixa se aplica ao peso informado (intervalo fechado).</summary>
    public bool Aplica(decimal peso) => peso >= PesoInicial && peso <= PesoFinal;
}
