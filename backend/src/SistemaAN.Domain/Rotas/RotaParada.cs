using SistemaAN.Domain.Common;

namespace SistemaAN.Domain.Rotas;

/// <summary>Uma parada da rota: vínculo (id-only) com a Entrega + ordem na saída.</summary>
public class RotaParada : Entity
{
    private RotaParada() { } // EF Core

    private RotaParada(long entregaId, int ordem)
    {
        EntregaId = entregaId;
        Ordem = ordem;
    }

    public long RotaId { get; private set; }
    public long EntregaId { get; private set; }
    public int Ordem { get; private set; }

    public static RotaParada Criar(long entregaId, int ordem) => new(entregaId, ordem);

    public void DefinirOrdem(int ordem) => Ordem = ordem;
}
