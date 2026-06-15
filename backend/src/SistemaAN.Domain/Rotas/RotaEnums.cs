namespace SistemaAN.Domain.Rotas;

/// <summary>Status da rota/saída de entrega.</summary>
public enum StatusRota
{
    Rascunho,
    Planejada,
    Despachada,
    Concluida,
    Cancelada,
}

/// <summary>Período sugerido da saída (orienta o encaixe pela preferência do cliente).</summary>
public enum PeriodoRota
{
    Manha,
    Tarde,
    HorarioComercial,
    Extra,
}
