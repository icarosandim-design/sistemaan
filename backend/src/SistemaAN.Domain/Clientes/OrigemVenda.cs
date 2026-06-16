using SistemaAN.Domain.Common;

namespace SistemaAN.Domain.Clientes;

/// <summary>
/// Origem de venda (cadastro extensível). Vinculada ao cliente pelo nome,
/// permitindo gerenciar a lista e gerar relatórios por origem.
/// </summary>
public class OrigemVenda : Entity
{
    private OrigemVenda() { } // EF Core

    private OrigemVenda(string nome, int ordem)
    {
        Nome = nome;
        Ordem = ordem;
        Ativo = true;
    }

    public string Nome { get; private set; } = string.Empty;

    /// <summary>Ordem de exibição (menor primeiro).</summary>
    public int Ordem { get; private set; }

    public bool Ativo { get; private set; }

    public static OrigemVenda Criar(string nome, int ordem = 0) => new(nome.Trim(), ordem);

    public void Atualizar(string nome, int ordem, bool ativo)
    {
        Nome = nome.Trim();
        Ordem = ordem;
        Ativo = ativo;
    }

    public void DefinirAtivo(bool ativo) => Ativo = ativo;
}
