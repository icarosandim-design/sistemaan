using SistemaAN.Domain.Common;

namespace SistemaAN.Domain.Pets;

/// <summary>
/// Doença de cão (cadastro gerenciável). Um pet pode ter mais de uma doença
/// (relacionamento N:N via <see cref="PetDoenca"/>).
/// </summary>
public class Doenca : Entity
{
    private Doenca() { } // EF Core

    private Doenca(string nome, int ordem)
    {
        Nome = nome;
        Ordem = ordem;
        Ativo = true;
    }

    public string Nome { get; private set; } = string.Empty;

    /// <summary>Ordem de exibição (menor primeiro).</summary>
    public int Ordem { get; private set; }

    public bool Ativo { get; private set; }

    public static Doenca Criar(string nome, int ordem = 0) => new(nome.Trim(), ordem);

    public void Atualizar(string nome, int ordem, bool ativo)
    {
        Nome = nome.Trim();
        Ordem = ordem;
        Ativo = ativo;
    }

    public void DefinirAtivo(bool ativo) => Ativo = ativo;
}
