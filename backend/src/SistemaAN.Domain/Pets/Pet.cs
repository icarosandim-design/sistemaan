using SistemaAN.Domain.Common;

namespace SistemaAN.Domain.Pets;

/// <summary>
/// Pet de um cliente: apenas o cadastro básico do animal. Frequência, receitas
/// e demais regras de alimentação pertencem ao Plano Alimentar (etapa futura),
/// não ao Pet. A sugestão de gramas/dia é calculada a partir da Tabela de
/// Consumo (não é persistida); o usuário pode informar um ajuste manual.
/// </summary>
public class Pet : AuditableEntity
{
    private Pet() { } // EF Core

    public long ClienteId { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    /// <summary>Raça referenciada no cadastro de Raças (opcional).</summary>
    public long? RacaId { get; private set; }
    /// <summary>Nome da raça (snapshot para exibição/compatibilidade).</summary>
    public string? Raca { get; private set; }
    public decimal PesoKg { get; private set; }
    public DateOnly? DataNascimento { get; private set; }
    public string? IdadeAprox { get; private set; }
    public Sexo? Sexo { get; private set; }
    public bool Ativo { get; private set; }
    public string? ObservacoesGerais { get; private set; }
    public string? ObservacoesAlimentares { get; private set; }
    public int? GramasDiaAjustadas { get; private set; }

    public static Pet Criar(long clienteId, DadosPet dados)
    {
        var p = new Pet { ClienteId = clienteId, Ativo = true };
        p.Aplicar(dados);
        return p;
    }

    public void Atualizar(DadosPet dados) => Aplicar(dados);

    public void Inativar() => Ativo = false;

    public void Reativar() => Ativo = true;

    private void Aplicar(DadosPet d)
    {
        Nome = d.Nome.Trim();
        RacaId = d.RacaId;
        Raca = Texto(d.Raca);
        PesoKg = d.PesoKg;
        DataNascimento = d.DataNascimento;
        IdadeAprox = Texto(d.IdadeAprox);
        Sexo = d.Sexo;
        ObservacoesGerais = Texto(d.ObservacoesGerais);
        ObservacoesAlimentares = Texto(d.ObservacoesAlimentares);
        GramasDiaAjustadas = d.GramasDiaAjustadas;
    }

    private static string? Texto(string? valor)
    {
        var t = valor?.Trim();
        return string.IsNullOrEmpty(t) ? null : t;
    }
}

public sealed record DadosPet(
    string Nome,
    long? RacaId,
    string? Raca,
    decimal PesoKg,
    DateOnly? DataNascimento,
    string? IdadeAprox,
    Sexo? Sexo,
    string? ObservacoesGerais,
    string? ObservacoesAlimentares,
    int? GramasDiaAjustadas);
