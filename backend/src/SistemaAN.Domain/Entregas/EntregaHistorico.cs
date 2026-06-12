using SistemaAN.Domain.Common;

namespace SistemaAN.Domain.Entregas;

/// <summary>Registro append-only de alterações de uma entrega.</summary>
public class EntregaHistorico : Entity
{
    private EntregaHistorico() { } // EF Core

    private EntregaHistorico(string usuario, string evento, EntregaStatus? de, EntregaStatus? para)
    {
        Quando = DateTimeOffset.UtcNow;
        Usuario = usuario;
        Evento = evento;
        StatusDe = de;
        StatusPara = para;
    }

    public long EntregaId { get; private set; }
    public DateTimeOffset Quando { get; private set; }
    public string Usuario { get; private set; } = string.Empty;
    public string Evento { get; private set; } = string.Empty;
    public EntregaStatus? StatusDe { get; private set; }
    public EntregaStatus? StatusPara { get; private set; }

    public static EntregaHistorico Criar(string usuario, string evento, EntregaStatus? de = null, EntregaStatus? para = null)
        => new(usuario, evento, de, para);
}
