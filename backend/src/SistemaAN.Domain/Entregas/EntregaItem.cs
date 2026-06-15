using SistemaAN.Domain.Common;
using SistemaAN.Domain.Estoque;
using SistemaAN.Domain.Receitas;

namespace SistemaAN.Domain.Entregas;

/// <summary>Snapshot de uma receita do pet na entrega.</summary>
public class EntregaItem : Entity
{
    private readonly List<EntregaItemPacote> _pacotes = [];
    private readonly List<EntregaItemIngrediente> _ingredientes = [];

    private EntregaItem() { } // EF Core

    private EntregaItem(
        long receitaId, string receitaCodigo, string receitaNome, TipoReceita tipo,
        int? quantidadeCicloGramas, int? tamanhoPacoteGramas, int? quantidadePacotes)
    {
        ReceitaId = receitaId;
        ReceitaCodigo = receitaCodigo;
        ReceitaNome = receitaNome;
        Tipo = tipo;
        QuantidadeCicloGramas = quantidadeCicloGramas;
        TamanhoPacoteGramas = tamanhoPacoteGramas;
        QuantidadePacotes = quantidadePacotes;
        StatusPreparo = StatusPreparoPersonalizada.NaoPronta;
    }

    public long EntregaPetId { get; private set; }
    public long ReceitaId { get; private set; }
    public string ReceitaCodigo { get; private set; } = string.Empty;
    public string ReceitaNome { get; private set; } = string.Empty;
    public TipoReceita Tipo { get; private set; }

    /// <summary>Casa: gramas necessárias no ciclo.</summary>
    public int? QuantidadeCicloGramas { get; private set; }

    /// <summary>Personalizada: tamanho do pacote (= soma dos ingredientes).</summary>
    public int? TamanhoPacoteGramas { get; private set; }

    /// <summary>Personalizada: nº de pacotes no ciclo.</summary>
    public int? QuantidadePacotes { get; private set; }

    // ----- Prontidão (Personalizada) — preenchida pela Produção -----
    public StatusPreparoPersonalizada StatusPreparo { get; private set; }
    public int? PacotesProntos { get; private set; }
    public DateTimeOffset? PreparadoEm { get; private set; }
    public string? PreparadoPor { get; private set; }

    public IReadOnlyCollection<EntregaItemPacote> Pacotes => _pacotes.AsReadOnly();
    public IReadOnlyCollection<EntregaItemIngrediente> Ingredientes => _ingredientes.AsReadOnly();

    /// <summary>Registra a prontidão desta receita (chamado pela Produção ao concluir/não fazer).</summary>
    public void DefinirPreparo(StatusPreparoPersonalizada status, int? pacotesProntos, DateTimeOffset? quando, string? usuario)
    {
        StatusPreparo = status;
        PacotesProntos = pacotesProntos;
        PreparadoEm = quando;
        PreparadoPor = usuario;
    }

    /// <summary>
    /// Baixa o produto finalizado/reservado da Personalizada quando a entrega é concluída.
    /// A reserva (pacotes prontos) é zerada — não fica disponível após a entrega.
    /// </summary>
    public void BaixarReservaEntregue()
    {
        if (Tipo == TipoReceita.Personalizada)
        {
            PacotesProntos = 0;
        }
    }

    public static EntregaItem CriarCasa(
        long receitaId, string codigo, string nome, int quantidadeCicloGramas,
        IEnumerable<EntregaItemPacote> pacotes)
    {
        var item = new EntregaItem(receitaId, codigo, nome, TipoReceita.Casa, quantidadeCicloGramas, null, null);
        item._pacotes.AddRange(pacotes);
        return item;
    }

    public static EntregaItem CriarPersonalizada(
        long receitaId, string codigo, string nome, int tamanhoPacoteGramas, int quantidadePacotes,
        IEnumerable<EntregaItemIngrediente> ingredientes)
    {
        var item = new EntregaItem(receitaId, codigo, nome, TipoReceita.Personalizada, null, tamanhoPacoteGramas, quantidadePacotes);
        item._ingredientes.AddRange(ingredientes);
        return item;
    }
}
