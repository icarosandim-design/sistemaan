namespace SistemaAN.Application.Consumo;

public interface IFaixaConsumoService
{
    Task<IReadOnlyList<FaixaConsumoDto>> ListarAsync(CancellationToken cancellationToken = default);

    Task<FaixaConsumoDto> ObterAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>Retorna a faixa ativa que se aplica ao peso, ou null.</summary>
    Task<FaixaConsumoDto?> ConsultarPorPesoAsync(decimal peso, CancellationToken cancellationToken = default);

    Task<FaixaConsumoDto> CriarAsync(SalvarFaixaConsumoRequest request, CancellationToken cancellationToken = default);

    Task<FaixaConsumoDto> AtualizarAsync(long id, SalvarFaixaConsumoRequest request, CancellationToken cancellationToken = default);
}
