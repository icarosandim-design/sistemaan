namespace SistemaAN.Application.Clientes;

/// <summary>Cadastro de origens de venda (lista gerenciável usada no cliente).</summary>
public interface IOrigemVendaService
{
    Task<IReadOnlyList<OrigemVendaDto>> ListarAsync(bool incluirInativas = false, CancellationToken cancellationToken = default);

    Task<OrigemVendaDto> ObterAsync(long id, CancellationToken cancellationToken = default);

    Task<OrigemVendaDto> CriarAsync(SalvarOrigemVendaRequest request, CancellationToken cancellationToken = default);

    Task<OrigemVendaDto> AtualizarAsync(long id, SalvarOrigemVendaRequest request, CancellationToken cancellationToken = default);

    Task AlternarStatusAsync(long id, bool ativo, CancellationToken cancellationToken = default);
}
