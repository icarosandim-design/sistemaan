namespace SistemaAN.Application.Receitas;

public interface IReceitaPersonalizadaService
{
    Task<IReadOnlyList<ReceitaPersonalizadaDto>> ListarPorPetAsync(long petId, CancellationToken cancellationToken = default);

    Task<ReceitaPersonalizadaDto> ObterAsync(long id, CancellationToken cancellationToken = default);

    Task<ReceitaPersonalizadaDto> CriarAsync(long petId, SalvarReceitaPersonalizadaRequest request, CancellationToken cancellationToken = default);

    Task<ReceitaPersonalizadaDto> AtualizarAsync(long id, SalvarReceitaPersonalizadaRequest request, CancellationToken cancellationToken = default);

    Task AlternarStatusAsync(long id, bool ativo, CancellationToken cancellationToken = default);
}
