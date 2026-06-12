using SistemaAN.Domain.Common;

namespace SistemaAN.Domain.Entregas;

/// <summary>Snapshot de pacotes de um item (Receita da Casa).</summary>
public class EntregaItemPacote : Entity
{
    private EntregaItemPacote() { } // EF Core

    private EntregaItemPacote(string tamanhoLabel, int pesoGramas, int quantidade)
    {
        TamanhoLabel = tamanhoLabel;
        PesoGramas = pesoGramas;
        Quantidade = quantidade;
    }

    public long EntregaItemId { get; private set; }
    public string TamanhoLabel { get; private set; } = string.Empty;
    public int PesoGramas { get; private set; }
    public int Quantidade { get; private set; }

    public static EntregaItemPacote Criar(string tamanhoLabel, int pesoGramas, int quantidade)
        => new(tamanhoLabel, pesoGramas, quantidade);
}
