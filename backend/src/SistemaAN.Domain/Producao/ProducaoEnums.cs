namespace SistemaAN.Domain.Producao;

/// <summary>Situação da ordem de produção (uma por dia).</summary>
public enum StatusOrdemProducao
{
    Planejada,
    EmAndamento,
    Finalizada,
}

/// <summary>Situação de uma ficha de produção (fila da cozinha).</summary>
public enum StatusFichaProducao
{
    Pendente,
    EmPreparo,
    Produzida,
    Envasada,
    Conferida,
    NaoFeita,
}
