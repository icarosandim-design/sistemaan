using SistemaAN.Domain.Common;

namespace SistemaAN.Domain.Planos;

/// <summary>
/// Pacote de um item do plano (apenas Receita da Casa): quantos pacotes de um
/// determinado <c>TamanhoPacote</c> serão enviados/produzidos no ciclo.
/// </summary>
public class PlanoItemPacote : Entity
{
    private PlanoItemPacote() { } // EF Core

    private PlanoItemPacote(long tamanhoPacoteId, int quantidade)
    {
        TamanhoPacoteId = tamanhoPacoteId;
        Quantidade = quantidade;
    }

    public long PlanoItemReceitaId { get; private set; }
    public long TamanhoPacoteId { get; private set; }
    public int Quantidade { get; private set; }

    public static PlanoItemPacote Criar(long tamanhoPacoteId, int quantidade)
        => new(tamanhoPacoteId, quantidade);
}
