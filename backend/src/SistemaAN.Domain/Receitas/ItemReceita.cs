using SistemaAN.Domain.Common;

namespace SistemaAN.Domain.Receitas;

/// <summary>Item da ficha técnica: ingrediente + gramas COZIDAS (o que vai na bacia).</summary>
public class ItemReceita : Entity
{
    private ItemReceita() { } // EF Core

    private ItemReceita(long ingredienteId, int gramas)
    {
        IngredienteId = ingredienteId;
        Gramas = gramas;
    }

    public long ReceitaId { get; private set; }

    public long IngredienteId { get; private set; }

    /// <summary>Quantidade em gramas cozidas.</summary>
    public int Gramas { get; private set; }

    public static ItemReceita Criar(long ingredienteId, int gramas) => new(ingredienteId, gramas);
}
