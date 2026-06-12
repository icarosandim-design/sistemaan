namespace SistemaAN.Application.Pacotes;

public interface ITamanhoPacoteService
{
    Task<IReadOnlyList<TamanhoPacoteDto>> ListarAsync(CancellationToken cancellationToken = default);

    Task<TamanhoPacoteDto> ObterAsync(long id, CancellationToken cancellationToken = default);

    Task<TamanhoPacoteDto> CriarAsync(SalvarTamanhoPacoteRequest request, CancellationToken cancellationToken = default);

    Task<TamanhoPacoteDto> AtualizarAsync(long id, SalvarTamanhoPacoteRequest request, CancellationToken cancellationToken = default);

    Task AlternarStatusAsync(long id, bool ativo, CancellationToken cancellationToken = default);
}
