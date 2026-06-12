namespace SistemaAN.Application.Entregas;

public sealed record EntregaItemPacoteDto(string TamanhoLabel, int PesoGramas, int Quantidade);

public sealed record EntregaItemIngredienteDto(
    long IngredienteId, string Nome, string Categoria, int GramasCozidas);

public sealed record EntregaItemDto(
    long Id,
    long ReceitaId,
    string ReceitaCodigo,
    string ReceitaNome,
    string Tipo,
    int? QuantidadeCicloGramas,
    int? TamanhoPacoteGramas,
    int? QuantidadePacotes,
    IReadOnlyList<EntregaItemPacoteDto> Pacotes,
    IReadOnlyList<EntregaItemIngredienteDto> Ingredientes);

public sealed record EntregaPetDto(
    long Id,
    long PetId,
    string PetNome,
    string TipoAlimentacao,
    int? GramasDia,
    int QuantidadeTotalGramas,
    IReadOnlyList<EntregaItemDto> Itens);

public sealed record EntregaHistoricoDto(
    DateTimeOffset Quando, string Usuario, string Evento, string? StatusDe, string? StatusPara);

public sealed record EntregaDto(
    long Id,
    long ClienteId,
    DateOnly DataPrevista,
    string Status,
    string ClienteNome,
    string? Telefone,
    string? Rua,
    string? Numero,
    string? Complemento,
    string? Cep,
    string? Bairro,
    string? Cidade,
    string? Estado,
    string FrequenciaNome,
    int DiasCiclo,
    string? ObservacoesInternas,
    string? ObservacoesEntregador,
    long? EntregadorId,
    string? MotivoNaoEntrega,
    string? MotivoReagendamento,
    long? ReagendadaDeId,
    long? ReagendadaParaId,
    string? MotivoCancelamento,
    IReadOnlyList<EntregaPetDto> Pets,
    IReadOnlyList<EntregaHistoricoDto> Historico);

public sealed record EntregaResumoDto(
    long Id,
    long ClienteId,
    string ClienteNome,
    string? Telefone,
    DateOnly DataPrevista,
    string Status,
    string? Bairro,
    string? Cidade,
    string Tipos,
    int TotalGramas,
    int TotalPacotes,
    long? EntregadorId,
    IReadOnlyList<string> Pets);

public sealed record GerarEntregasRequest(int HorizonteDias = 90);

public sealed record GerarEntregasResultado(int Geradas, int Clientes);

public sealed record MudarStatusEntregaRequest(string Status);

public sealed record MotivoRequest(string Motivo);

public sealed record ReagendarEntregaRequest(DateOnly NovaData, string Motivo);

public sealed record AlterarAgendaRequest(DateOnly NovaData, long? FrequenciaEntregaId, string Motivo);
