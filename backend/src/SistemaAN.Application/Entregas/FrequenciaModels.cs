namespace SistemaAN.Application.Entregas;

public sealed record FrequenciaEntregaDto(
    long Id,
    string Nome,
    int? DiasCiclo,
    string Descricao,
    bool Personalizada,
    bool Ativo);

public sealed record SalvarFrequenciaEntregaRequest(
    string Nome,
    int? DiasCiclo,
    string Descricao,
    bool Personalizada,
    bool Ativo);

public sealed record AlternarStatusRequest(bool Ativo);
