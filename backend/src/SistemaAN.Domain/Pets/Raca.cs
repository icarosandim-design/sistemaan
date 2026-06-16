using SistemaAN.Domain.Common;

namespace SistemaAN.Domain.Pets;

/// <summary>
/// Raça de cão (cadastro gerenciável). Inclui SRD (sem raça definida).
/// O Pet referencia esta base ao ser cadastrado.
/// </summary>
public class Raca : Entity
{
    private Raca() { } // EF Core

    private Raca(string nome, int ordem)
    {
        Nome = nome;
        Ordem = ordem;
        Ativo = true;
    }

    public string Nome { get; private set; } = string.Empty;

    /// <summary>Ordem de exibição (menor primeiro).</summary>
    public int Ordem { get; private set; }

    public bool Ativo { get; private set; }

    public static Raca Criar(string nome, int ordem = 0) => new(nome.Trim(), ordem);

    public void Atualizar(string nome, int ordem, bool ativo)
    {
        Nome = nome.Trim();
        Ordem = ordem;
        Ativo = ativo;
    }

    public void DefinirAtivo(bool ativo) => Ativo = ativo;
}
