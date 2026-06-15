using SistemaAN.Domain.Common;

namespace SistemaAN.Domain.Rotas;

/// <summary>
/// Rota/Saída de entrega de um dia. Pode haver várias por data (manhã, tarde, extra…).
/// Contém as paradas (entregas) ordenadas. Logística simples: sem GPS/mapa nesta fase.
/// </summary>
public class Rota : AuditableEntity
{
    private readonly List<RotaParada> _paradas = [];

    private Rota() { } // EF Core

    private Rota(DateOnly data, string nome, PeriodoRota periodo, string? entregador, string? observacoes)
    {
        Data = data;
        Nome = nome;
        Periodo = periodo;
        Entregador = entregador;
        Observacoes = observacoes;
        Status = StatusRota.Rascunho;
    }

    public DateOnly Data { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public PeriodoRota Periodo { get; private set; }
    public string? Entregador { get; private set; }
    public StatusRota Status { get; private set; }
    public string? Observacoes { get; private set; }

    public IReadOnlyCollection<RotaParada> Paradas => _paradas.AsReadOnly();

    /// <summary>Rotas ativas "seguram" a entrega (não pode estar em duas ao mesmo tempo).</summary>
    public bool EhAtiva => Status is StatusRota.Rascunho or StatusRota.Planejada or StatusRota.Despachada;

    public static Rota Criar(DateOnly data, string nome, PeriodoRota periodo, string? entregador, string? observacoes)
        => new(data, nome.Trim(), periodo, Texto(entregador), Texto(observacoes));

    public void Atualizar(string nome, PeriodoRota periodo, string? entregador, string? observacoes)
    {
        Nome = nome.Trim();
        Periodo = periodo;
        Entregador = Texto(entregador);
        Observacoes = Texto(observacoes);
    }

    public bool ContemEntrega(long entregaId) => _paradas.Exists(p => p.EntregaId == entregaId);

    public void AdicionarParada(long entregaId)
    {
        if (ContemEntrega(entregaId))
        {
            return;
        }
        var ordem = _paradas.Count == 0 ? 1 : _paradas.Max(p => p.Ordem) + 1;
        _paradas.Add(RotaParada.Criar(entregaId, ordem));
    }

    public void RemoverParada(long entregaId)
    {
        var parada = _paradas.Find(p => p.EntregaId == entregaId);
        if (parada is not null)
        {
            _paradas.Remove(parada);
            Renumerar();
        }
    }

    /// <summary>Reordena as paradas conforme a sequência de entregaIds informada.</summary>
    public void Reordenar(IReadOnlyList<long> entregaIds)
    {
        var ordem = 1;
        foreach (var id in entregaIds)
        {
            var parada = _paradas.Find(p => p.EntregaId == id);
            parada?.DefinirOrdem(ordem++);
        }
        // Itens não citados vão para o fim, preservando ordem relativa.
        foreach (var parada in _paradas.Where(p => !entregaIds.Contains(p.EntregaId)).OrderBy(p => p.Ordem))
        {
            parada.DefinirOrdem(ordem++);
        }
    }

    public void MudarStatus(StatusRota status) => Status = status;

    private void Renumerar()
    {
        var ordem = 1;
        foreach (var parada in _paradas.OrderBy(p => p.Ordem))
        {
            parada.DefinirOrdem(ordem++);
        }
    }

    private static string? Texto(string? valor)
    {
        var t = valor?.Trim();
        return string.IsNullOrEmpty(t) ? null : t;
    }
}
