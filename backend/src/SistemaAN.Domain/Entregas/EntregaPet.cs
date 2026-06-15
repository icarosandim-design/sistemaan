using SistemaAN.Domain.Common;
using SistemaAN.Domain.Receitas;

namespace SistemaAN.Domain.Entregas;

/// <summary>Snapshot de um pet na entrega.</summary>
public class EntregaPet : Entity
{
    private readonly List<EntregaItem> _itens = [];

    private EntregaPet() { } // EF Core

    private EntregaPet(long? petId, string petNome, TipoReceita tipoAlimentacao, int? gramasDia, int quantidadeTotalGramas)
    {
        PetId = petId;
        PetNome = petNome;
        TipoAlimentacao = tipoAlimentacao;
        GramasDia = gramasDia;
        QuantidadeTotalGramas = quantidadeTotalGramas;
    }

    public long EntregaId { get; private set; }

    /// <summary>Pet de origem (PF). Nulo em entregas de Pedido PJ (grupo "container").</summary>
    public long? PetId { get; private set; }
    public string PetNome { get; private set; } = string.Empty;
    public TipoReceita TipoAlimentacao { get; private set; }
    public int? GramasDia { get; private set; }
    public int QuantidadeTotalGramas { get; private set; }

    public IReadOnlyCollection<EntregaItem> Itens => _itens.AsReadOnly();

    public static EntregaPet Criar(
        long? petId, string petNome, TipoReceita tipoAlimentacao, int? gramasDia,
        int quantidadeTotalGramas, IEnumerable<EntregaItem> itens)
    {
        var pet = new EntregaPet(petId, petNome, tipoAlimentacao, gramasDia, quantidadeTotalGramas);
        pet._itens.AddRange(itens);
        return pet;
    }
}
