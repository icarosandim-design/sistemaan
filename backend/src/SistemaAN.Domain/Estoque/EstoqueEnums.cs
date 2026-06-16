namespace SistemaAN.Domain.Estoque;

/// <summary>Natureza do item de estoque.</summary>
public enum TipoItemEstoque
{
    /// <summary>Insumo usado na produção (alimentar ou não).</summary>
    Insumo,

    /// <summary>Pacote pronto de Receita da Casa (estoque geral por receita+tamanho).</summary>
    ProdutoAcabadoCasa,

    /// <summary>Produto comprado pronto (petisco, revenda) — entra por compra, não por produção.</summary>
    ProdutoComprado,
}

/// <summary>Unidade de medida do item/movimentação.</summary>
public enum UnidadeMedida
{
    Kg,
    G,
    Unidade,
    Pacote,
    Caixa,
    Litro,
    Ml,
    Outro,
}

/// <summary>Tipo da movimentação de estoque (livro-razão).</summary>
public enum TipoMovimentacao
{
    // Entradas
    EntradaCompra,
    EntradaProducao,
    AjustePositivo,
    TransferenciaEntrada,

    // Saídas
    SaidaProducao,
    Descarte,
    Perda,
    Vencimento,
    AjusteNegativo,
    TransferenciaSaida,
    ConsumoInterno,
    BaixaEntrega,
}

/// <summary>Sentido contábil da movimentação.</summary>
public enum SentidoMovimentacao
{
    Entrada,
    Saida,
}

/// <summary>Motivo detalhado de uma saída de estoque (opcional).</summary>
public enum MotivoSaida
{
    Producao,
    Venda,
    Descarte,
    Perda,
    Vencimento,
    AjusteManual,
    Transferencia,
    ConsumoInterno,
    Outro,
}

/// <summary>Situação de um lote de estoque.</summary>
public enum StatusLote
{
    Ativo,
    Esgotado,
    Vencido,
    Bloqueado,
}

/// <summary>Origem de um lote (de onde ele entrou no estoque).</summary>
public enum OrigemLote
{
    Compra,
    Producao,
}

/// <summary>Categoria do fornecedor (cadastro-base global).</summary>
public enum CategoriaFornecedor
{
    Ingredientes,
    Embalagens,
    Etiquetas,
    MaterialLimpeza,
    Servicos,
    Outros,
}

/// <summary>
/// Estrutura preparada para a prontidão da Receita Personalizada (será usada
/// pela Produção/Entregas no futuro). Não há entidade vinculada nesta fase.
/// </summary>
public enum StatusPreparoPersonalizada
{
    NaoPronta,
    ParcialmentePronta,
    Pronta,
}
