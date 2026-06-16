namespace SistemaAN.Application.Central;

/// <summary>
/// Contrato de leitura/agregação da Central Operacional. A Central é uma camada
/// FINA: estes DTOs apenas resumem o que os módulos donos (Clientes, Pets,
/// Produção, Estoque, Entregas, Rotas) já calculam. Nenhuma regra de negócio nova.
/// </summary>
public sealed record CentralResumoDto(
    DateOnly DiaSelecionado,
    IReadOnlyList<CentralKpiDto> Kpis,
    IReadOnlyList<CentralEntregaDiaDto> Entregas7,
    CentralProducaoDto? Producao,
    IReadOnlyList<CentralProducaoCasaDto> ProducaoCasa,
    IReadOnlyList<CentralProducaoPersonalizadaDto> ProducaoPersonalizada,
    IReadOnlyList<CentralIngredienteDto> Ingredientes,
    IReadOnlyList<CentralEstoqueItemDto> Estoque,
    CentralRotasDto? Rotas,
    IReadOnlyList<CentralAlertaDto> Alertas);

/// <summary>KPI de topo. <see cref="SomenteAdmin"/> marca cartões financeiros.</summary>
public sealed record CentralKpiDto(string Label, string Valor, string Icone, bool SomenteAdmin = false);

public sealed record CentralEntregaDiaDto(DateOnly Data, int Entregas, int Pf, int Pj, int Alertas, bool Hoje);

/// <summary>Resumo da ordem de produção do dia (nulo quando não há ordem).</summary>
public sealed record CentralProducaoDto(
    long OrdemId,
    string Status,
    bool Finalizada,
    int TotalFichas,
    int Casa,
    int Personalizadas,
    int Pendencias);

public sealed record CentralProducaoCasaDto(string Produto, int Pacotes);

public sealed record CentralProducaoPersonalizadaDto(string Pet, string Codigo, string Tutor, int Pacotes);

public sealed record CentralIngredienteDto(string Nome, decimal Cozidos, decimal Crus, decimal Estoque);

/// <summary>Item de produto acabado da casa. <see cref="Status"/> = "ok" | "baixo".</summary>
public sealed record CentralEstoqueItemDto(string Produto, decimal Saldo, decimal Minimo, string Status);

/// <summary>Resumo das saídas/rotas do dia (nulo quando o dia não tem rotas nem entregas pendentes).</summary>
public sealed record CentralRotasDto(
    int Total,
    int Planejadas,
    int Despachadas,
    int Concluidas,
    int SemEntregador,
    int EntregasSemRota);

/// <summary>Alerta operacional resumido. <see cref="Rota"/> aponta para o módulo dono.</summary>
public sealed record CentralAlertaDto(string Tipo, string Icone, string Texto, string? Rota);
