namespace SistemaAN.Application.Pets;

public sealed record PetDto(
    long Id,
    long ClienteId,
    string Nome,
    string? Raca,
    decimal PesoKg,
    DateOnly? DataNascimento,
    string? IdadeAprox,
    string? Sexo,
    bool Ativo,
    string? ObservacoesGerais,
    string? ObservacoesAlimentares,
    int? GramasDiaAjustadas,
    int? GramasDiaSugeridas);

public sealed record PetResumoDto(
    long Id,
    long ClienteId,
    string Nome,
    string TutorNome,
    string? Raca,
    decimal PesoKg,
    string? Sexo,
    bool Ativo,
    string? TipoAlimentacao,
    string? ReceitaAtual,
    DateOnly? ProximaEntrega);

public sealed record SalvarPetRequest(
    string Nome,
    string? Raca,
    decimal PesoKg,
    DateOnly? DataNascimento,
    string? IdadeAprox,
    string? Sexo,
    string? ObservacoesGerais,
    string? ObservacoesAlimentares,
    int? GramasDiaAjustadas);
