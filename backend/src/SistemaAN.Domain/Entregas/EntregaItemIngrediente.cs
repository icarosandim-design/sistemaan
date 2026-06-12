using SistemaAN.Domain.Common;

namespace SistemaAN.Domain.Entregas;

/// <summary>Snapshot de um ingrediente de Receita Personalizada (para Produção/custo).</summary>
public class EntregaItemIngrediente : Entity
{
    private EntregaItemIngrediente() { } // EF Core

    private EntregaItemIngrediente(
        long ingredienteId, string ingredienteNome, string categoria,
        int gramasCozidas, decimal custoKgCru, decimal coeficiente)
    {
        IngredienteId = ingredienteId;
        IngredienteNome = ingredienteNome;
        Categoria = categoria;
        GramasCozidas = gramasCozidas;
        CustoKgCru = custoKgCru;
        Coeficiente = coeficiente;
    }

    public long EntregaItemId { get; private set; }
    public long IngredienteId { get; private set; }
    public string IngredienteNome { get; private set; } = string.Empty;
    public string Categoria { get; private set; } = string.Empty;
    public int GramasCozidas { get; private set; }
    public decimal CustoKgCru { get; private set; }
    public decimal Coeficiente { get; private set; }

    public static EntregaItemIngrediente Criar(
        long ingredienteId, string ingredienteNome, string categoria,
        int gramasCozidas, decimal custoKgCru, decimal coeficiente)
        => new(ingredienteId, ingredienteNome, categoria, gramasCozidas, custoKgCru, coeficiente);
}
