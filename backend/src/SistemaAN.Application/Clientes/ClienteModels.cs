namespace SistemaAN.Application.Clientes;

public sealed record ClienteDto(
    long Id,
    string Nome,
    string? Telefone,
    string? Email,
    string? Endereco,
    string? Bairro,
    string? Cidade,
    string? Observacoes,
    bool Ativo,
    string TipoCliente,
    string? FormaPagamento,
    int? DiaCobranca,
    decimal ValorRecorrenteMensal,
    string StatusFinanceiro,
    string? ObservacoesFinanceiras);

public sealed record SalvarClienteRequest(
    string Nome,
    string? Telefone,
    string? Email,
    string? Endereco,
    string? Bairro,
    string? Cidade,
    string? Observacoes,
    bool Ativo,
    string TipoCliente,
    string? FormaPagamento,
    int? DiaCobranca,
    decimal ValorRecorrenteMensal,
    string StatusFinanceiro,
    string? ObservacoesFinanceiras);

public sealed record AlternarStatusClienteRequest(bool Ativo);
