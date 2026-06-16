namespace SistemaAN.Application.Relatorios;

// ---------------------------------------------------------------------------
// Comuns
// ---------------------------------------------------------------------------
public sealed record RelatorioPeriodo(DateOnly Inicio, DateOnly Fim);

/// <summary>Filtros aceitos pelos relatórios (todos opcionais; período obrigatório nos endpoints).</summary>
public sealed record RelatorioFiltro(
    DateOnly? Inicio = null,
    DateOnly? Fim = null,
    string? Tipo = null,
    long? ClienteId = null,
    long? ReceitaId = null,
    long? IngredienteId = null,
    long? MotivoId = null,
    long? FornecedorId = null,
    string? Status = null,
    string? Cidade = null,
    string? Bairro = null,
    int Pagina = 1,
    int TamanhoPagina = 50);

public sealed record ChaveValorDto(string Chave, decimal Kg, int Quantidade);

// ---------------------------------------------------------------------------
// Dashboard
// ---------------------------------------------------------------------------
public sealed record DashboardCardDto(string Chave, string Label, string Valor, string? Detalhe, bool Estimado, bool Pendente);

public sealed record VendasPorMesDto(string Mes, decimal Kg, int Quantidade, decimal ReceitaRecorrenteAtiva);

public sealed record VendasPorTipoDto(string Tipo, decimal Kg, int Quantidade);

public sealed record CancelamentoPorMotivoDto(string Motivo, int Quantidade, decimal ReceitaMensalPerdida, decimal KgMensalPerdido);

public sealed record ProducaoPlanRealDto(string Periodo, decimal KgPlanejado, decimal KgReal, decimal Diferenca);

public sealed record CustoPerdaIngredienteDto(string Ingrediente, decimal PerdaKg, decimal PerdaValor, decimal SobraKg, decimal SobraValor, bool Estimado);

public sealed record EvolucaoCustoPontoDto(string Mes, decimal CustoMedioCompra, decimal UltimoCusto, decimal VariacaoPercentual);

public sealed record DashboardDto(
    RelatorioPeriodo Periodo,
    IReadOnlyList<DashboardCardDto> Cards,
    IReadOnlyList<VendasPorMesDto> VendasPorMes,
    IReadOnlyList<VendasPorTipoDto> VendasPorTipo,
    IReadOnlyList<CancelamentoPorMotivoDto> CancelamentosPorMotivo,
    IReadOnlyList<ProducaoPlanRealDto> ProducaoPlanejadoReal,
    IReadOnlyList<CustoPerdaIngredienteDto> CustoPerdasTop,
    long? IngredienteEvolucaoId,
    string? IngredienteEvolucaoNome,
    IReadOnlyList<EvolucaoCustoPontoDto> EvolucaoCustoInsumo,
    bool ReceitaPorVendaIndisponivel);

// ---------------------------------------------------------------------------
// Relatório de Vendas
// ---------------------------------------------------------------------------
public sealed record VendaLinhaDto(
    DateOnly Data,
    string Tipo,
    string Cliente,
    string? Pet,
    string? Raca,
    decimal? Valor,
    decimal Custo,
    string? Origem);

public sealed record RelatorioVendasResumoDto(
    int QuantidadeVendas,
    decimal TotalKg,
    decimal ReceitaRecorrenteAtivaMensal,
    decimal CustoTotalEstimado,
    IReadOnlyList<VendasPorTipoDto> PorTipo,
    IReadOnlyList<ChaveValorDto> PorReceita,
    IReadOnlyList<ChaveValorDto> PorCidade,
    bool ReceitaPorVendaIndisponivel);

public sealed record RelatorioVendasDto(
    RelatorioPeriodo Periodo,
    RelatorioVendasResumoDto Resumo,
    IReadOnlyList<VendaLinhaDto> Linhas,
    int Total,
    int Pagina,
    int TamanhoPagina);

// ---------------------------------------------------------------------------
// Relatório de Cancelamentos
// ---------------------------------------------------------------------------
public sealed record CancelamentoLinhaDto(
    DateOnly? Data,
    string Cliente,
    string? Pets,
    string Motivo,
    string? Observacao,
    decimal ValorMensalPerdido,
    decimal KgMensalPerdido,
    int? DiasComoCliente,
    string? Usuario);

public sealed record RelatorioCancelamentosResumoDto(
    int TotalCancelamentos,
    decimal ReceitaMensalPerdida,
    decimal KgMensalPerdido,
    decimal TicketMedioMensal,
    double? TempoMedioDiasAteCancelamento,
    IReadOnlyList<CancelamentoPorMotivoDto> PorMotivo,
    IReadOnlyList<ChaveValorDto> PorCidade);

public sealed record RelatorioCancelamentosDto(
    RelatorioPeriodo Periodo,
    RelatorioCancelamentosResumoDto Resumo,
    IReadOnlyList<CancelamentoLinhaDto> Linhas,
    int Total,
    int Pagina,
    int TamanhoPagina);

// ---------------------------------------------------------------------------
// Relatório de Produção Planejado x Real
// ---------------------------------------------------------------------------
public sealed record ProducaoLinhaDto(
    DateOnly Data,
    string Ingrediente,
    decimal PlanejadoCruGramas,
    decimal RealCruGramas,
    decimal PlanejadoCozidoGramas,
    decimal RealCozidoGramas,
    decimal DiferencaCruGramas,
    decimal CustoPlanejado,
    decimal CustoRealEstimado,
    string Status);

public sealed record RelatorioProducaoResumoDto(
    int ProducoesPlanejadas,
    int ProducoesFinalizadas,
    decimal KgPlanejado,
    decimal KgReal,
    decimal CruPrevistoGramas,
    decimal CruRealGramas,
    decimal DiferencaCruGramas,
    decimal CozidoPrevistoGramas,
    decimal CozidoRealGramas,
    decimal DiferencaCozidoGramas,
    decimal CustoPlanejado,
    decimal CustoRealEstimado,
    decimal DiferencaCusto,
    bool CustoEstimado);

public sealed record RelatorioProducaoDto(
    RelatorioPeriodo Periodo,
    RelatorioProducaoResumoDto Resumo,
    IReadOnlyList<ProducaoLinhaDto> Linhas,
    int Total,
    int Pagina,
    int TamanhoPagina);

// ---------------------------------------------------------------------------
// Relatório de Perdas de Ingredientes
// ---------------------------------------------------------------------------
public sealed record PerdaLinhaDto(
    DateOnly Data,
    string Ingrediente,
    decimal PerdaKg,
    decimal PerdaValor,
    decimal SobraKg,
    decimal SobraValor,
    decimal CustoMedioUsado,
    decimal? FatorCadastrado,
    decimal? FatorReal,
    bool Estimado);

public sealed record RelatorioPerdasResumoDto(
    decimal PerdaTotalKg,
    decimal PerdaTotalValor,
    decimal SobraTotalKg,
    decimal SobraTotalValor,
    IReadOnlyList<CustoPerdaIngredienteDto> PorIngrediente,
    bool Estimado);

public sealed record RelatorioPerdasDto(
    RelatorioPeriodo Periodo,
    RelatorioPerdasResumoDto Resumo,
    IReadOnlyList<PerdaLinhaDto> Linhas,
    int Total,
    int Pagina,
    int TamanhoPagina);

// ---------------------------------------------------------------------------
// Relatório de Evolução de Custo dos Insumos
// ---------------------------------------------------------------------------
public sealed record CustoInsumoLinhaDto(
    DateOnly Data,
    string Ingrediente,
    string? Fornecedor,
    decimal Quantidade,
    string Unidade,
    decimal ValorUnitario,
    decimal ValorTotal,
    string Origem);

public sealed record CustoInsumoResumoItemDto(
    long IngredienteId,
    string Ingrediente,
    decimal CustoMedioAtual,
    decimal UltimoCusto,
    decimal MenorCustoPeriodo,
    decimal MaiorCustoPeriodo,
    decimal QuantidadeComprada,
    decimal ValorTotalComprado,
    decimal VariacaoValor,
    decimal VariacaoPercentual);

public sealed record RelatorioCustosInsumosDto(
    RelatorioPeriodo Periodo,
    IReadOnlyList<CustoInsumoResumoItemDto> Resumo,
    IReadOnlyList<CustoInsumoLinhaDto> Linhas,
    int Total,
    int Pagina,
    int TamanhoPagina,
    bool HistoricoLimitado);

// ---------------------------------------------------------------------------
// Relatório de Estoque e Produto Acabado
// ---------------------------------------------------------------------------
public sealed record EstoqueLinhaDto(
    string Item,
    string Tipo,
    decimal SaldoFisico,
    string Unidade,
    decimal Minimo,
    decimal CustoMedio,
    decimal ValorEstimado,
    string Status);

public sealed record RelatorioEstoqueResumoDto(
    int TotalItens,
    int ItensAbaixoMinimo,
    int ItensSemSaldo,
    decimal ValorEstimadoTotal,
    int ProdutoAcabadoComSaldo,
    int ProdutoAcabadoSemSaldo,
    bool ComprometidoIndisponivel);

public sealed record RelatorioEstoqueDto(
    RelatorioEstoqueResumoDto Resumo,
    IReadOnlyList<EstoqueLinhaDto> Linhas);
