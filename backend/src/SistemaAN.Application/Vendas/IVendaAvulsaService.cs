namespace SistemaAN.Application.Vendas;

/// <summary>Venda avulsa PF: registra uma venda única e gera a entrega correspondente.</summary>
public interface IVendaAvulsaService
{
    Task<VendaAvulsaResultadoDto> CriarAsync(SalvarVendaAvulsaRequest request, string usuario, CancellationToken cancellationToken = default);
}
