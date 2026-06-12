namespace SistemaAN.Application.Planos;

public interface IPlanoAlimentarService
{
    /// <summary>Plano vigente do pet (ou null se não houver).</summary>
    Task<PlanoAlimentarDto?> ObterPorPetAsync(long petId, CancellationToken cancellationToken = default);

    /// <summary>Cria ou atualiza (upsert) o plano vigente do pet.</summary>
    Task<PlanoAlimentarDto> SalvarAsync(long petId, SalvarPlanoRequest request, CancellationToken cancellationToken = default);

    /// <summary>Inativa/reativa o plano vigente do pet.</summary>
    Task AlternarStatusAsync(long petId, bool ativo, CancellationToken cancellationToken = default);
}
