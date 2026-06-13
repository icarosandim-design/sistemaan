using SistemaAN.Domain.Common;

namespace SistemaAN.Domain.Producao;

/// <summary>
/// Ordem de produção — **uma por dia**. Contém as fichas (receitas/pets) e o consumo
/// consolidado de ingredientes. Editável até ser finalizada.
/// </summary>
public class OrdemProducao : AuditableEntity
{
    private readonly List<FichaProducao> _fichas = [];
    private readonly List<ConsumoIngredienteProducao> _consumos = [];

    private OrdemProducao() { } // EF Core

    private OrdemProducao(DateOnly data)
    {
        Data = data;
        Status = StatusOrdemProducao.Planejada;
    }

    public DateOnly Data { get; private set; }
    public StatusOrdemProducao Status { get; private set; }
    public string? Observacoes { get; private set; }

    public DateTimeOffset? FinalizadaEm { get; private set; }
    public string? FinalizadaPor { get; private set; }
    public bool? TudoProduzido { get; private set; }
    public string? ObservacoesFinalizacao { get; private set; }

    public IReadOnlyCollection<FichaProducao> Fichas => _fichas.AsReadOnly();
    public IReadOnlyCollection<ConsumoIngredienteProducao> Consumos => _consumos.AsReadOnly();

    public static OrdemProducao Criar(DateOnly data) => new(data);

    public bool Editavel => Status != StatusOrdemProducao.Finalizada;

    public void AdicionarFicha(FichaProducao ficha) => _fichas.Add(ficha);

    public void RemoverFicha(FichaProducao ficha) => _fichas.Remove(ficha);

    public void LimparConsumos() => _consumos.Clear();

    public void AdicionarConsumo(ConsumoIngredienteProducao consumo) => _consumos.Add(consumo);

    public void Iniciar()
    {
        if (Status == StatusOrdemProducao.Planejada)
        {
            Status = StatusOrdemProducao.EmAndamento;
        }
    }

    public void Finalizar(bool tudoProduzido, string? observacoes, string usuario, DateTimeOffset quando)
    {
        Status = StatusOrdemProducao.Finalizada;
        TudoProduzido = tudoProduzido;
        ObservacoesFinalizacao = observacoes;
        FinalizadaPor = usuario;
        FinalizadaEm = quando;
    }
}
