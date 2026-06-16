using SistemaAN.Domain.Common;

namespace SistemaAN.Domain.Pets;

/// <summary>Vínculo N:N entre Pet e Doença.</summary>
public class PetDoenca : Entity
{
    private PetDoenca() { } // EF Core

    private PetDoenca(long petId, long doencaId)
    {
        PetId = petId;
        DoencaId = doencaId;
    }

    public long PetId { get; private set; }
    public long DoencaId { get; private set; }

    public static PetDoenca Criar(long petId, long doencaId) => new(petId, doencaId);
}
