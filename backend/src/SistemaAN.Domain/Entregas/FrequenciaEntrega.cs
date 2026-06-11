using SistemaAN.Domain.Common;

namespace SistemaAN.Domain.Entregas;

/// <summary>
/// Ciclo de entrega usado (futuramente) para gerar a agenda de entregas de um
/// pet a partir da primeira data do plano. "Personalizada" tem o ciclo definido
/// manualmente por pet no futuro (sem dias fixos aqui).
/// </summary>
public class FrequenciaEntrega : AuditableEntity
{
    private FrequenciaEntrega() { } // EF Core

    private FrequenciaEntrega(string nome, int? diasCiclo, string descricao, bool personalizada)
    {
        Nome = nome;
        Descricao = descricao;
        Personalizada = personalizada;
        DiasCiclo = personalizada ? null : diasCiclo;
        Ativo = true;
    }

    public string Nome { get; private set; } = string.Empty;

    /// <summary>Dias do ciclo (nulo quando personalizada).</summary>
    public int? DiasCiclo { get; private set; }

    public string Descricao { get; private set; } = string.Empty;

    public bool Personalizada { get; private set; }

    public bool Ativo { get; private set; }

    public static FrequenciaEntrega Criar(string nome, int? diasCiclo, string? descricao, bool personalizada)
        => new(nome.Trim(), diasCiclo, descricao?.Trim() ?? string.Empty, personalizada);

    public void Atualizar(string nome, int? diasCiclo, string? descricao, bool personalizada, bool ativo)
    {
        Nome = nome.Trim();
        Personalizada = personalizada;
        DiasCiclo = personalizada ? null : diasCiclo;
        Descricao = descricao?.Trim() ?? string.Empty;
        Ativo = ativo;
    }

    public void DefinirAtivo(bool ativo) => Ativo = ativo;
}
