namespace SistemaAN.Application.Producao;

// ===================== Demanda (leitura de Entregas + Estoque) =====================
public sealed record DemandaPersonalizadaDto(
    long EntregaItemId,
    long EntregaId,
    long EntregaPetId,
    long PetId,
    long ClienteId,
    string PetNome,
    string ClienteNome,
    string ReceitaCodigo,
    string ReceitaNome,
    DateOnly DataEntrega,
    int Pacotes,
    int PesoPacoteGramas,
    string StatusPreparo);

public sealed record DemandaCasaDto(
    long ReceitaId,
    string ReceitaNome,
    long? TamanhoPacoteId,
    string TamanhoNome,
    int PesoGramas,
    int Necessario,
    int Estoque,
    int Falta,
    long? ItemEstoqueId);

public sealed record DemandaDto(
    IReadOnlyList<DemandaPersonalizadaDto> Personalizadas,
    IReadOnlyList<DemandaCasaDto> Casa);

// ===================== Ordem de produção =====================
public sealed record FichaIngredienteDto(long IngredienteId, string IngredienteNome, string Categoria, int GramasCozidas, decimal Coeficiente);

public sealed record FichaProducaoDto(
    long Id,
    string Tipo,
    string? ClienteNome,
    string? PetNome,
    string ReceitaCodigo,
    string ReceitaNome,
    DateOnly? DataEntrega,
    int QuantidadePacotes,
    int PesoPacoteGramas,
    int QuantidadeTotalGramas,
    string Status,
    int? QuantidadePacotesReal,
    string? MotivoNaoFeita,
    IReadOnlyList<FichaIngredienteDto> Ingredientes);

public sealed record ConsumoConsolidadoDto(
    long IngredienteId,
    string IngredienteNome,
    string Categoria,
    int CozidoGramas,
    int CruGramas,
    long? ItemEstoqueId,
    bool SemItemVinculado);

public sealed record OrdemProducaoDto(
    long Id,
    DateOnly Data,
    string Status,
    string? Observacoes,
    IReadOnlyList<FichaProducaoDto> Fichas,
    IReadOnlyList<ConsumoConsolidadoDto> Consolidado);

public sealed record OrdemProducaoResumoDto(long Id, DateOnly Data, string Status, int TotalFichas);

// ===================== Requests =====================
public sealed record CriarOrdemRequest(DateOnly Data);

public sealed record FichaCasaRequest(long ReceitaId, long TamanhoPacoteId, int QuantidadePacotes);

public sealed record AdicionarFichasRequest(
    IReadOnlyList<long> EntregaItemIds,
    IReadOnlyList<FichaCasaRequest> Casa);
