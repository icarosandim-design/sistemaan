namespace SistemaAN.Application.Entregas;

public interface IFrequenciaEntregaService
{
    Task<IReadOnlyList<FrequenciaEntregaDto>> ListarAsync(CancellationToken cancellationToken = default);

    Task<FrequenciaEntregaDto> ObterAsync(long id, CancellationToken cancellationToken = default);

    Task<FrequenciaEntregaDto> CriarAsync(SalvarFrequenciaEntregaRequest request, CancellationToken cancellationToken = default);

    Task<FrequenciaEntregaDto> AtualizarAsync(long id, SalvarFrequenciaEntregaRequest request, CancellationToken cancellationToken = default);

    Task AlternarStatusAsync(long id, bool ativo, CancellationToken cancellationToken = default);
}
