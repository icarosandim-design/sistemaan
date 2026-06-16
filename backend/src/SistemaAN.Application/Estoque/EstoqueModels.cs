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

/// <summary>
/// "Estoque" dinâmico de Receita Personalizada: pacotes já prontos (reservados)
/// em entregas ativas que ainda não foram entregues. Não é cadastro — é leitura.
/// </summary>
public sealed record PersonalizadaProntaDto(
    long EntregaId,
    string ReceitaCodigo,
    string ReceitaNome,
    int PesoGramas,
    string PetNome,
    string ClienteNome,
    DateOnly DataPrevista,
    int PacotesProntos,
    string StatusEntrega);


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
    string? Observacoes,
    decimal? Frete = null,
    bool FreteCompoeCusto = false);

// ===================== Movimentações (livro-razão geral) =====================
public sealed record MovimentacaoGeralDto(
    long Id,
    DateTimeOffset DataHora,
    long ItemEstoqueId,
    string ItemNome,
    string Categoria,
    string Tipo,
    string Sentido,
    decimal Quantidade,
    string Unidade,
    string? LoteCodigo,
    decimal CustoUnitario,
    decimal ValorTotal,
    decimal SaldoAnteriorItem,
    decimal SaldoPosteriorItem,
    string Usuario,
    string? Motivo,
    string? Observacao,
    string? FornecedorNome,
    string Origem);

public sealed record MovimentacaoPaginaDto(int Total, IReadOnlyList<MovimentacaoGeralDto> Itens);

public sealed record FiltroMovimentacoesRequest(
    DateOnly? DataInicio = null,
    DateOnly? DataFim = null,
    long? ItemEstoqueId = null,
    string? Categoria = null,
    string? Tipo = null,
    long? FornecedorId = null,
    long? LoteEstoqueId = null,
    string? Usuario = null,
    string? Motivo = null,
    string? Origem = null,
    int Pagina = 1,
    int TamanhoPagina = 50);

// ===================== Compras / Entradas =====================
public sealed record EntradaCompraDto(
    long Id,
    DateOnly DataCompra,
    DateOnly DataEntrada,
    long? FornecedorId,
    string? FornecedorNome,
    long ItemEstoqueId,
    string ItemNome,
    string Categoria,
    decimal Quantidade,
    string Unidade,
    decimal ValorUnitario,
    decimal CustoUnitarioEstoque,
    decimal ValorProdutos,
    decimal Frete,
    decimal ValorTotalPago,
    string LoteCodigo,
    DateOnly? Validade,
    string Usuario,
    string? Observacoes);

public sealed record FiltroEntradasRequest(
    DateOnly? DataInicio = null,
    DateOnly? DataFim = null,
    long? FornecedorId = null,
    long? ItemEstoqueId = null,
    string? Categoria = null,
    long? LoteEstoqueId = null,
    string? Usuario = null,
    decimal? ValorMin = null,
    decimal? ValorMax = null,
    bool? ComFrete = null);

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
