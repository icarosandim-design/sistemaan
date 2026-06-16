using SistemaAN.Domain.Common;

namespace SistemaAN.Domain.Clientes;

/// <summary>
/// Motivo de cancelamento (cadastro gerenciável). Usado no cancelamento de
/// clientes/assinaturas e como dimensão do relatório de cancelamentos.
/// </summary>
public class MotivoCancelamento : AuditableEntity
{
    private MotivoCancelamento() { } // EF Core

    private MotivoCancelamento(string nome, int ordem, string? observacoes)
    {
        Nome = nome;
        Ordem = ordem;
        Observacoes = observacoes;
        Ativo = true;
    }

    public string Nome { get; private set; } = string.Empty;

    /// <summary>Ordem de exibição (menor primeiro).</summary>
    public int Ordem { get; private set; }

    public bool Ativo { get; private set; }

    public string? Observacoes { get; private set; }

    public static MotivoCancelamento Criar(string nome, int ordem = 0, string? observacoes = null)
        => new(nome.Trim(), ordem, observacoes?.Trim());

    public void Atualizar(string nome, int ordem, bool ativo, string? observacoes)
    {
        Nome = nome.Trim();
        Ordem = ordem;
        Ativo = ativo;
        Observacoes = observacoes?.Trim();
    }

    public void DefinirAtivo(bool ativo) => Ativo = ativo;
}
