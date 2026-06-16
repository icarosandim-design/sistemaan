namespace SistemaAN.Application.Central;

/// <summary>
/// Agrega o resumo exibido na Central Operacional a partir dos dados reais já
/// existentes nos módulos. Leitura apenas.
/// </summary>
public interface ICentralService
{
    /// <summary>Resumo do dia informado (ou de hoje, quando <paramref name="data"/> é nulo).</summary>
    Task<CentralResumoDto> ObterResumoAsync(DateOnly? data, CancellationToken cancellationToken = default);
}
