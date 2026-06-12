using SistemaAN.Domain.Common;

namespace SistemaAN.Domain.Estoque;

/// <summary>
/// Item de estoque com **saldo corrente**. Cobre Insumos e Produto Acabado da Casa.
/// O saldo (<see cref="QuantidadeAtual"/>) e o <see cref="CustoMedio"/> são caches
/// projetados das movimentações — **nunca** editados sem movimentação correspondente.
/// </summary>
public class ItemEstoque : AuditableEntity
{
    private ItemEstoque() { } // EF Core

    private ItemEstoque(
        TipoItemEstoque tipo,
        string nome,
        CategoriaEstoque categoria,
        UnidadeMedida unidadeMedida,
        long? ingredienteId,
        long? receitaId,
        long? tamanhoPacoteId)
    {
        Tipo = tipo;
        Nome = nome;
        Categoria = categoria;
        UnidadeMedida = unidadeMedida;
        IngredienteId = ingredienteId;
        ReceitaId = receitaId;
        TamanhoPacoteId = tamanhoPacoteId;
        QuantidadeAtual = 0m;
        CustoMedio = 0m;
        Ativo = true;
    }

    public TipoItemEstoque Tipo { get; private set; }

    public string Nome { get; private set; } = string.Empty;

    public CategoriaEstoque Categoria { get; private set; }

    public UnidadeMedida UnidadeMedida { get; private set; }

    /// <summary>Vínculo opcional com o Ingrediente (item alimentar). 1:1.</summary>
    public long? IngredienteId { get; private set; }

    /// <summary>Produto acabado da casa: receita de origem.</summary>
    public long? ReceitaId { get; private set; }

    /// <summary>Produto acabado da casa: tamanho do pacote.</summary>
    public long? TamanhoPacoteId { get; private set; }

    /// <summary>Saldo físico atual (cache das movimentações).</summary>
    public decimal QuantidadeAtual { get; private set; }

    public decimal QuantidadeMinima { get; private set; }

    /// <summary>Custo médio ponderado móvel (cache, atualizado nas entradas).</summary>
    public decimal CustoMedio { get; private set; }

    public long? FornecedorPrincipalId { get; private set; }

    public string? LocalArmazenamento { get; private set; }

    public bool ControlaValidade { get; private set; }

    public bool Ativo { get; private set; }

    public string? Observacoes { get; private set; }

    public static ItemEstoque CriarInsumo(
        string nome,
        CategoriaEstoque categoria,
        UnidadeMedida unidadeMedida,
        long? ingredienteId,
        decimal quantidadeMinima,
        long? fornecedorPrincipalId,
        string? localArmazenamento,
        bool controlaValidade,
        string? observacoes)
    {
        var item = new ItemEstoque(TipoItemEstoque.Insumo, nome.Trim(), categoria, unidadeMedida, ingredienteId, null, null);
        item.QuantidadeMinima = quantidadeMinima;
        item.FornecedorPrincipalId = fornecedorPrincipalId;
        item.LocalArmazenamento = Texto(localArmazenamento);
        item.ControlaValidade = controlaValidade;
        item.Observacoes = Texto(observacoes);
        return item;
    }

    public static ItemEstoque CriarProdutoAcabadoCasa(
        string nome,
        long receitaId,
        long tamanhoPacoteId,
        decimal quantidadeMinima,
        string? localArmazenamento,
        bool controlaValidade,
        string? observacoes)
    {
        var item = new ItemEstoque(TipoItemEstoque.ProdutoAcabadoCasa, nome.Trim(),
            CategoriaEstoque.ProdutoAcabado, UnidadeMedida.Pacote, null, receitaId, tamanhoPacoteId);
        item.QuantidadeMinima = quantidadeMinima;
        item.LocalArmazenamento = Texto(localArmazenamento);
        item.ControlaValidade = controlaValidade;
        item.Observacoes = Texto(observacoes);
        return item;
    }

    /// <summary>Atualiza os campos editáveis (tipo e vínculos são imutáveis).</summary>
    public void Atualizar(
        string nome,
        CategoriaEstoque categoria,
        UnidadeMedida unidadeMedida,
        decimal quantidadeMinima,
        long? fornecedorPrincipalId,
        string? localArmazenamento,
        bool controlaValidade,
        string? observacoes,
        bool ativo)
    {
        Nome = nome.Trim();
        // Produto acabado mantém categoria/unidade canônicas.
        if (Tipo == TipoItemEstoque.Insumo)
        {
            Categoria = categoria;
            UnidadeMedida = unidadeMedida;
            FornecedorPrincipalId = fornecedorPrincipalId;
        }
        QuantidadeMinima = quantidadeMinima;
        LocalArmazenamento = Texto(localArmazenamento);
        ControlaValidade = controlaValidade;
        Observacoes = Texto(observacoes);
        Ativo = ativo;
    }

    public void DefinirAtivo(bool ativo) => Ativo = ativo;

    /// <summary>
    /// Aplica uma entrada: aumenta o saldo e recalcula o custo médio ponderado móvel.
    /// </summary>
    public void RegistrarEntrada(decimal quantidade, decimal custoUnitario)
    {
        if (quantidade <= 0m)
        {
            throw new InvalidOperationException("Quantidade de entrada deve ser maior que zero.");
        }

        var novoSaldo = QuantidadeAtual + quantidade;
        var custoPonderado = (QuantidadeAtual * CustoMedio) + (quantidade * custoUnitario);
        CustoMedio = novoSaldo > 0m ? Math.Round(custoPonderado / novoSaldo, 4, MidpointRounding.AwayFromZero) : custoUnitario;
        QuantidadeAtual = novoSaldo;
    }

    /// <summary>Aplica uma saída: reduz o saldo (não altera o custo médio).</summary>
    public void RegistrarSaida(decimal quantidade)
    {
        if (quantidade <= 0m)
        {
            throw new InvalidOperationException("Quantidade de saída deve ser maior que zero.");
        }

        if (quantidade > QuantidadeAtual)
        {
            throw new InvalidOperationException("Saldo insuficiente: saída não pode deixar o estoque negativo.");
        }

        QuantidadeAtual -= quantidade;
    }

    /// <summary>Define o saldo por ajuste manual (a diferença vira movimentação).</summary>
    public void AjustarSaldo(decimal novaQuantidade)
    {
        if (novaQuantidade < 0m)
        {
            throw new InvalidOperationException("Saldo ajustado não pode ser negativo.");
        }

        QuantidadeAtual = novaQuantidade;
    }

    public bool AbaixoDoMinimo => QuantidadeAtual < QuantidadeMinima;

    private static string? Texto(string? v)
    {
        var t = v?.Trim();
        return string.IsNullOrEmpty(t) ? null : t;
    }
}
