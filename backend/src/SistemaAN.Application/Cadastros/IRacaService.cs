namespace SistemaAN.Application.Cadastros;

/// <summary>Cadastro de raças de cães (inclui SRD). Base usada no cadastro de pets.</summary>
public interface IRacaService
{
    Task<IReadOnlyList<RacaDto>> ListarAsync(bool incluirInativas = false, CancellationToken cancellationToken = default);

    Task<RacaDto> ObterAsync(long id, CancellationToken cancellationToken = default);

    Task<RacaDto> CriarAsync(SalvarRacaRequest request, CancellationToken cancellationToken = default);

    Task<RacaDto> AtualizarAsync(long id, SalvarRacaRequest request, CancellationToken cancellationToken = default);

    Task AlternarStatusAsync(long id, bool ativo, CancellationToken cancellationToken = default);
}
