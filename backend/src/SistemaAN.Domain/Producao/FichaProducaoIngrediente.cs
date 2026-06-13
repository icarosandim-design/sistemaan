using SistemaAN.Domain.Common;

namespace SistemaAN.Domain.Producao;

/// <summary>Snapshot de um ingrediente da ficha (para consolidação e ficha técnica).</summary>
public class FichaProducaoIngrediente : Entity
{
    private FichaProducaoIngrediente() { } // EF Core

    private FichaProducaoIngrediente(long ingredienteId, string ingredienteNome, string categoria, int gramasCozidas, decimal coeficiente)
    {
        IngredienteId = ingredienteId;
        IngredienteNome = ingredienteNome;
        Categoria = categoria;
        GramasCozidas = gramasCozidas;
        Coeficiente = coeficiente;
    }

    public long FichaProducaoId { get; private set; }
    public long IngredienteId { get; private set; }
    public string IngredienteNome { get; private set; } = string.Empty;
    public string Categoria { get; private set; } = string.Empty;
    public int GramasCozidas { get; private set; }
    public decimal Coeficiente { get; private set; }

    public static FichaProducaoIngrediente Criar(long ingredienteId, string ingredienteNome, string categoria, int gramasCozidas, decimal coeficiente)
        => new(ingredienteId, ingredienteNome, categoria, gramasCozidas, coeficiente);
}
