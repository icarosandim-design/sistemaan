namespace SistemaAN.Application.Rotas;

public sealed record RotaResumoDto(
    long Id,
    DateOnly Data,
    string Nome,
    string Periodo,
    string? Entregador,
    string Status,
    int TotalEntregas);

public sealed record RotaParadaDto(
    long EntregaId,
    int Ordem,
    string ClienteNome,
    bool EhPj,
    long? PedidoId,
    string Endereco,
    string? Bairro,
    string? Cidade,
    string? Telefone,
    string PreferenciaHorario,
    string StatusEntrega,
    string ItensResumo,
    bool EnderecoIncompleto,
    string? ProntidaoTexto,
    string EstoqueAlerta,
    string PetNomes,
    int TotalGramas);

public sealed record RotaDto(
    long Id,
    DateOnly Data,
    string Nome,
    string Periodo,
    string? Entregador,
    string Status,
    string? Observacoes,
    IReadOnlyList<RotaParadaDto> Paradas);

public sealed record EntregaDisponivelDto(
    long Id,
    string ClienteNome,
    bool EhPj,
    long? PedidoId,
    string Endereco,
    string? Bairro,
    string? Cidade,
    string? Telefone,
    string PreferenciaHorario,
    string StatusEntrega,
    string ItensResumo,
    bool EnderecoIncompleto,
    string? ProntidaoTexto,
    long? EmRotaAtivaId,
    string PetNomes,
    int TotalGramas);

public sealed record CriarRotaRequest(DateOnly Data, string Nome, string Periodo, string? Entregador, string? Observacoes);

public sealed record AtualizarRotaRequest(string Nome, string Periodo, string? Entregador, string? Observacoes);

public sealed record AdicionarEntregaRotaRequest(long EntregaId);

public sealed record ReordenarRotaRequest(IReadOnlyList<long> EntregaIds);

public sealed record MudarStatusRotaRequest(string Status);
