namespace SistemaAN.Application.Clientes;

/// <summary>Cadastro de Clientes Pessoa Jurídica (Cliente Natureza=PJ + dados PJ 1:1).</summary>
public interface IClientePjService
{
    Task<IReadOnlyList<ClientePjResumoDto>> ListarAsync(string? busca, bool? ativo, CancellationToken cancellationToken = default);

    Task<ClientePjDto> ObterAsync(long id, CancellationToken cancellationToken = default);

    Task<ClientePjDto> CriarAsync(SalvarClientePjRequest request, CancellationToken cancellationToken = default);

    Task<ClientePjDto> AtualizarAsync(long id, SalvarClientePjRequest request, CancellationToken cancellationToken = default);

    Task<ClientePjDto> AlternarStatusAsync(long id, bool ativo, CancellationToken cancellationToken = default);

    IReadOnlyList<TipoPjOpcaoDto> ListarTipos();
}
