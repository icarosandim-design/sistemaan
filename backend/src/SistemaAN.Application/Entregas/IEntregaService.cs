namespace SistemaAN.Application.Entregas;

public interface IEntregaService
{
    Task<GerarEntregasResultado> GerarAsync(int horizonteDias, string usuario, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EntregaResumoDto>> ListarAsync(
        DateOnly? data, string? status, long? clienteId, string? bairro, string? cidade,
        CancellationToken cancellationToken = default);

    Task<EntregaDto> ObterAsync(long id, CancellationToken cancellationToken = default);

    Task<EntregaDto> MudarStatusAsync(long id, string status, string usuario, CancellationToken cancellationToken = default);

    Task<EntregaDto> MarcarNaoEntregueAsync(long id, string motivo, string usuario, CancellationToken cancellationToken = default);

    Task<EntregaDto> ReagendarAsync(long id, DateOnly novaData, string motivo, string usuario, CancellationToken cancellationToken = default);

    Task<EntregaDto> CancelarAsync(long id, string motivo, string usuario, CancellationToken cancellationToken = default);

    /// <summary>Regerar entregas futuras ainda não operacionais (Programada) do cliente.</summary>
    Task<GerarEntregasResultado> RegerarFuturasDoClienteAsync(long clienteId, string usuario, CancellationToken cancellationToken = default);

    /// <summary>Regerar futuras do cliente dono do pet (atalho usado em hooks).</summary>
    Task<GerarEntregasResultado> RegerarFuturasDoPetAsync(long petId, string usuario, CancellationToken cancellationToken = default);

    /// <summary>Regerar futuras dos clientes cujo plano ativo usa a receita informada.</summary>
    Task<GerarEntregasResultado> RegerarPorReceitaAsync(long receitaId, string usuario, CancellationToken cancellationToken = default);

    /// <summary>"Esta e próximas": ajusta a agenda do cliente e regera as futuras elegíveis.</summary>
    Task<GerarEntregasResultado> AlterarAgendaFuturaAsync(
        long entregaId, DateOnly novaData, long? frequenciaEntregaId, string motivo, string usuario,
        CancellationToken cancellationToken = default);
}
