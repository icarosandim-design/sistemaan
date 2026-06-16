namespace SistemaAN.Application.Cadastros;

/// <summary>Cadastro de doenças de cães. Vinculadas aos pets (N:N).</summary>
public interface IDoencaService
{
    Task<IReadOnlyList<DoencaDto>> ListarAsync(bool incluirInativas = false, CancellationToken cancellationToken = default);

    Task<DoencaDto> ObterAsync(long id, CancellationToken cancellationToken = default);

    Task<DoencaDto> CriarAsync(SalvarDoencaRequest request, CancellationToken cancellationToken = default);

    Task<DoencaDto> AtualizarAsync(long id, SalvarDoencaRequest request, CancellationToken cancellationToken = default);

    Task AlternarStatusAsync(long id, bool ativo, CancellationToken cancellationToken = default);
}
