using SistemaAN.Domain.Common;

namespace SistemaAN.Domain.Pacotes;

/// <summary>
/// Tamanho de pacote (ex.: 250g, 500g). Cadastro global e reutilizável, usado
/// futuramente por Plano Alimentar, Sugestão de pacotes, Entregas, Produção,
/// Ordem de Produção e Estoque de produto acabado.
/// </summary>
public class TamanhoPacote : AuditableEntity
{
    private TamanhoPacote() { } // EF Core

    private TamanhoPacote(string nome, int pesoGramas, string? observacao)
    {
        Nome = nome;
        PesoGramas = pesoGramas;
        Observacao = observacao;
        Ativo = true;
    }

    public string Nome { get; private set; } = string.Empty;

    /// <summary>Peso do pacote em gramas (deve ser maior que zero).</summary>
    public int PesoGramas { get; private set; }

    public bool Ativo { get; private set; }

    public string? Observacao { get; private set; }

    public static TamanhoPacote Criar(string nome, int pesoGramas, string? observacao)
        => new(nome.Trim(), pesoGramas, Texto(observacao));

    public void Atualizar(string nome, int pesoGramas, string? observacao, bool ativo)
    {
        Nome = nome.Trim();
        PesoGramas = pesoGramas;
        Observacao = Texto(observacao);
        Ativo = ativo;
    }

    public void DefinirAtivo(bool ativo) => Ativo = ativo;

    private static string? Texto(string? v)
    {
        var t = v?.Trim();
        return string.IsNullOrEmpty(t) ? null : t;
    }
}
