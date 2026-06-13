using SistemaAN.Domain.Common;
using SistemaAN.Domain.Receitas;

namespace SistemaAN.Domain.Producao;

/// <summary>
/// Ficha de produção (uma receita/pet dentro da ordem do dia). Snapshot dos dados no
/// momento em que entra na produção. Personalizada referencia a entrega; Casa referencia
/// a receita + tamanho + item de produto acabado de destino.
/// </summary>
public class FichaProducao : Entity
{
    private readonly List<FichaProducaoIngrediente> _ingredientes = [];

    private FichaProducao() { } // EF Core

    private FichaProducao(TipoReceita tipo, string receitaCodigo, string receitaNome, int quantidadePacotes, int pesoPacoteGramas, int quantidadeTotalGramas)
    {
        Tipo = tipo;
        ReceitaCodigo = receitaCodigo;
        ReceitaNome = receitaNome;
        QuantidadePacotes = quantidadePacotes;
        PesoPacoteGramas = pesoPacoteGramas;
        QuantidadeTotalGramas = quantidadeTotalGramas;
        Status = StatusFichaProducao.Pendente;
    }

    public long OrdemProducaoId { get; private set; }
    public TipoReceita Tipo { get; private set; }

    // Personalizada (referências à entrega) — id-only (FK Restrict)
    public long? EntregaId { get; private set; }
    public long? EntregaPetId { get; private set; }
    public long? EntregaItemId { get; private set; }
    public long? PetId { get; private set; }
    public long? ClienteId { get; private set; }

    // Casa
    public long? ReceitaId { get; private set; }
    public long? TamanhoPacoteId { get; private set; }
    public long? ItemEstoqueId { get; private set; }

    // Snapshot
    public string? ClienteNome { get; private set; }
    public string? PetNome { get; private set; }
    public string ReceitaCodigo { get; private set; } = string.Empty;
    public string ReceitaNome { get; private set; } = string.Empty;
    public DateOnly? DataEntrega { get; private set; }

    // Planejado
    public int QuantidadePacotes { get; private set; }
    public int PesoPacoteGramas { get; private set; }
    public int QuantidadeTotalGramas { get; private set; }

    public StatusFichaProducao Status { get; private set; }
    public string? MotivoNaoFeita { get; private set; }

    // Real (conclusão — Fase 3)
    public int? QuantidadePacotesReal { get; private set; }
    public int? PesoEnvasadoGramas { get; private set; }
    public DateTimeOffset? ConcluidaEm { get; private set; }
    public string? ConcluidaPor { get; private set; }
    public string? Observacoes { get; private set; }

    public IReadOnlyCollection<FichaProducaoIngrediente> Ingredientes => _ingredientes.AsReadOnly();

    public static FichaProducao CriarPersonalizada(
        long entregaId, long entregaPetId, long entregaItemId, long petId, long clienteId,
        string clienteNome, string petNome, string receitaCodigo, string receitaNome, DateOnly? dataEntrega,
        int quantidadePacotes, int pesoPacoteGramas, IEnumerable<FichaProducaoIngrediente> ingredientes)
    {
        var f = new FichaProducao(TipoReceita.Personalizada, receitaCodigo, receitaNome, quantidadePacotes, pesoPacoteGramas, quantidadePacotes * pesoPacoteGramas)
        {
            EntregaId = entregaId,
            EntregaPetId = entregaPetId,
            EntregaItemId = entregaItemId,
            PetId = petId,
            ClienteId = clienteId,
            ClienteNome = clienteNome,
            PetNome = petNome,
            DataEntrega = dataEntrega,
        };
        f._ingredientes.AddRange(ingredientes);
        return f;
    }

    public static FichaProducao CriarCasa(
        long receitaId, long tamanhoPacoteId, long? itemEstoqueId, string receitaCodigo, string receitaNome,
        int quantidadePacotes, int pesoPacoteGramas, IEnumerable<FichaProducaoIngrediente> ingredientes)
    {
        var f = new FichaProducao(TipoReceita.Casa, receitaCodigo, receitaNome, quantidadePacotes, pesoPacoteGramas, quantidadePacotes * pesoPacoteGramas)
        {
            ReceitaId = receitaId,
            TamanhoPacoteId = tamanhoPacoteId,
            ItemEstoqueId = itemEstoqueId,
        };
        f._ingredientes.AddRange(ingredientes);
        return f;
    }

    public void MudarStatus(StatusFichaProducao status) => Status = status;

    public void Concluir(int pacotesReais, int? pesoEnvasado, string usuario, DateTimeOffset quando, string? observacoes)
    {
        Status = StatusFichaProducao.Conferida;
        QuantidadePacotesReal = pacotesReais;
        PesoEnvasadoGramas = pesoEnvasado;
        ConcluidaPor = usuario;
        ConcluidaEm = quando;
        Observacoes = observacoes;
    }

    public void MarcarNaoFeita(string motivo, string usuario, DateTimeOffset quando)
    {
        Status = StatusFichaProducao.NaoFeita;
        MotivoNaoFeita = motivo;
        ConcluidaPor = usuario;
        ConcluidaEm = quando;
    }
}
