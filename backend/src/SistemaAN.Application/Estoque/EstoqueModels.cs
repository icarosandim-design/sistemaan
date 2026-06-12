namespace SistemaAN.Application.Estoque;

// ===================== Fornecedor =====================
public sealed record FornecedorDto(
    long Id,
    string Nome,
    string? NomeFantasia,
    string? Documento,
    string? Telefone,
    string? WhatsApp,
    string? Email,
    string? PessoaContato,
    string? Endereco,
    string? Cidade,
    string? Estado,
    string? Categoria,
    string? Observacoes,
    int? PrazoPagamentoDias,
    string? FormaPagamentoPreferida,
    string? ChavePix,
    string? DadosBancarios,
    bool Ativo);

public sealed record SalvarFornecedorRequest(
    string Nome,
    string? NomeFantasia,
    string? Documento,
    string? Telefone,
    string? WhatsApp,
    string? Email,
    string? PessoaContato,
    string? Endereco,
    string? Cidade,
    string? Estado,
    string? Categoria,
    string? Observacoes,
    int? PrazoPagamentoDias,
    string? FormaPagamentoPreferida,
    string? ChavePix,
    string? DadosBancarios,
    bool Ativo);

// ===================== Item de Estoque =====================
public sealed record ItemEstoqueDto(
    long Id,
    string Tipo,
    string Nome,
    string Categoria,
    string UnidadeMedida,
    long? IngredienteId,
    string? IngredienteNome,
    long? ReceitaId,
    string? ReceitaNome,
    long? TamanhoPacoteId,
    string? TamanhoPacoteNome,
    decimal QuantidadeAtual,
    decimal QuantidadeMinima,
    decimal CustoMedio,
    long? FornecedorPrincipalId,
    string? FornecedorPrincipalNome,
    string? LocalArmazenamento,
    bool ControlaValidade,
    bool Ativo,
    string? Observacoes,
    bool AbaixoDoMinimo);

public sealed record CriarItemInsumoRequest(
    string Nome,
    string Categoria,
    string UnidadeMedida,
    long? IngredienteId,
    decimal QuantidadeMinima,
    long? FornecedorPrincipalId,
    string? LocalArmazenamento,
    bool ControlaValidade,
    string? Observacoes,
    bool Ativo);

public sealed record CriarItemProdutoAcabadoRequest(
    string Nome,
    long ReceitaId,
    long TamanhoPacoteId,
    decimal QuantidadeMinima,
    string? LocalArmazenamento,
    bool ControlaValidade,
    string? Observacoes,
    bool Ativo);

public sealed record AtualizarItemEstoqueRequest(
    string Nome,
    string Categoria,
    string UnidadeMedida,
    decimal QuantidadeMinima,
    long? FornecedorPrincipalId,
    string? LocalArmazenamento,
    bool ControlaValidade,
    string? Observacoes,
    bool Ativo);

// ===================== Lote / Movimentação =====================
public sealed record LoteEstoqueDto(
    long Id,
    long ItemEstoqueId,
    string Codigo,
    DateOnly DataEntrada,
    DateOnly? Validade,
    decimal QuantidadeInicial,
    decimal QuantidadeAtual,
    decimal CustoUnitario,
    long? FornecedorId,
    string? FornecedorNome,
    string Origem,
    string Status);

public sealed record MovimentacaoEstoqueDto(
    long Id,
    long ItemEstoqueId,
    string ItemNome,
    long? LoteEstoqueId,
    string? LoteCodigo,
    string Tipo,
    string Sentido,
    decimal Quantidade,
    decimal SaldoAnteriorItem,
    decimal SaldoPosteriorItem,
    decimal CustoUnitario,
    decimal ValorTotal,
    string Usuario,
    DateTimeOffset DataHora,
    string? MotivoCodigo,
    string? Motivo,
    string? Observacao);

// ===================== Operações =====================
public sealed record RegistrarEntradaRequest(
    long ItemEstoqueId,
    decimal Quantidade,
    decimal? ValorUnitario,
    decimal? ValorTotal,
    long? FornecedorId,
    DateOnly DataCompra,
    DateOnly DataEntrada,
    DateOnly? Validade,
    string? LoteCodigo,
    string? LocalArmazenamento,
    string? Observacoes);

public sealed record RegistrarSaidaRequest(
    long ItemEstoqueId,
    decimal Quantidade,
    string Tipo,
    string? MotivoCodigo,
    string? Motivo,
    string? Observacao);

public sealed record RegistrarAjusteRequest(
    long ItemEstoqueId,
    long? LoteEstoqueId,
    decimal NovaQuantidade,
    string Motivo,
    string? Observacao);

public sealed record AlternarStatusRequest(bool Ativo);
