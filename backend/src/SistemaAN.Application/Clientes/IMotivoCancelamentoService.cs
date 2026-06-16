namespace SistemaAN.Application.Clientes;

/// <summary>Cadastro de motivos de cancelamento (lista gerenciável usada no cancelamento e nos relatórios).</summary>
public interface IMotivoCancelamentoService
{
    Task<IReadOnlyList<MotivoCancelamentoDto>> ListarAsync(bool incluirInativos = false, CancellationToken cancellationToken = default);

    Task<MotivoCancelamentoDto> ObterAsync(long id, CancellationToken cancellationToken = default);

    Task<MotivoCancelamentoDto> CriarAsync(SalvarMotivoCancelamentoRequest request, CancellationToken cancellationToken = default);

    Task<MotivoCancelamentoDto> AtualizarAsync(long id, SalvarMotivoCancelamentoRequest request, CancellationToken cancellationToken = default);

    Task AlternarStatusAsync(long id, bool ativo, CancellationToken cancellationToken = default);
}
