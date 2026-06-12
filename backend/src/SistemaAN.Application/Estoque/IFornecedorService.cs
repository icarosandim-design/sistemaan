namespace SistemaAN.Application.Estoque;

public interface IFornecedorService
{
    Task<IReadOnlyList<FornecedorDto>> ListarAsync(bool apenasAtivos = false, CancellationToken cancellationToken = default);

    Task<FornecedorDto> ObterAsync(long id, CancellationToken cancellationToken = default);

    Task<FornecedorDto> CriarAsync(SalvarFornecedorRequest request, CancellationToken cancellationToken = default);

    Task<FornecedorDto> AtualizarAsync(long id, SalvarFornecedorRequest request, CancellationToken cancellationToken = default);

    Task AlternarStatusAsync(long id, bool ativo, CancellationToken cancellationToken = default);
}
