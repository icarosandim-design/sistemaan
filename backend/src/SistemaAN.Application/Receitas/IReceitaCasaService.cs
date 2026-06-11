namespace SistemaAN.Application.Receitas;

public interface IReceitaCasaService
{
    Task<IReadOnlyList<ReceitaCasaDto>> ListarAsync(CancellationToken cancellationToken = default);

    Task<ReceitaCasaDto> ObterAsync(long id, CancellationToken cancellationToken = default);

    Task<ReceitaCasaDto> CriarAsync(SalvarReceitaCasaRequest request, CancellationToken cancellationToken = default);

    Task<ReceitaCasaDto> AtualizarAsync(long id, SalvarReceitaCasaRequest request, CancellationToken cancellationToken = default);

    Task AlternarStatusAsync(long id, bool ativo, CancellationToken cancellationToken = default);
}
