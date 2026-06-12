namespace SistemaAN.Application.Clientes;

public sealed record ClienteDto(
    long Id,
    string Nome,
    string? Cpf,
    string? Telefone,
    string? Email,
    string? OrigemVenda,
    string? Observacoes,
    string? Rua,
    string? Numero,
    string? Complemento,
    string? Cep,
    string? Bairro,
    string? Cidade,
    string? Estado,
    bool Ativo,
    string? MotivoCancelamento,
    DateTimeOffset? DataCancelamento,
    string TipoCliente,
    string? FormaPagamento,
    int? DiaCobranca,
    decimal ValorRecorrenteMensal,
    string StatusFinanceiro,
    string? ObservacoesFinanceiras);

public sealed record SalvarClienteRequest(
    string Nome,
    string? Cpf,
    string? Telefone,
    string? Email,
    string? OrigemVenda,
    string? Observacoes,
    string? Rua,
    string? Numero,
    string? Complemento,
    string? Cep,
    string? Bairro,
    string? Cidade,
    string? Estado,
    string TipoCliente,
    string? FormaPagamento,
    int? DiaCobranca,
    decimal ValorRecorrenteMensal,
    string StatusFinanceiro,
    string? ObservacoesFinanceiras);

public sealed record CancelarClienteRequest(string Motivo);
