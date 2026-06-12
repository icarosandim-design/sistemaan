using Microsoft.EntityFrameworkCore;
using SistemaAN.Application.Common.Exceptions;
using SistemaAN.Application.Common.Interfaces;
using SistemaAN.Domain.Consumo;
using SistemaAN.Domain.Pets;

namespace SistemaAN.Application.Pets;

public sealed class PetService : IPetService
{
    private readonly IApplicationDbContext _db;

    public PetService(IApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<PetDto>> ListarPorClienteAsync(long clienteId, CancellationToken cancellationToken = default)
    {
        var existeCliente = await _db.Clientes.AnyAsync(c => c.Id == clienteId, cancellationToken);
        if (!existeCliente)
        {
            throw new NotFoundException("Cliente", clienteId);
        }

        var pets = await _db.Pets
            .Where(p => p.ClienteId == clienteId)
            .OrderByDescending(p => p.Ativo)
            .ThenBy(p => p.Nome)
            .ToListAsync(cancellationToken);

        var faixas = await FaixasAtivasAsync(cancellationToken);
        return pets.Select(p => Map(p, Sugerir(faixas, p.PesoKg))).ToList();
    }

    public async Task<PetDto> ObterAsync(long id, CancellationToken cancellationToken = default)
    {
        var pet = await _db.Pets.FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
            ?? throw new NotFoundException("Pet", id);
        var faixas = await FaixasAtivasAsync(cancellationToken);
        return Map(pet, Sugerir(faixas, pet.PesoKg));
    }

    public async Task<PetDto> CriarAsync(long clienteId, SalvarPetRequest request, CancellationToken cancellationToken = default)
    {
        var existeCliente = await _db.Clientes.AnyAsync(c => c.Id == clienteId, cancellationToken);
        if (!existeCliente)
        {
            throw new NotFoundException("Cliente", clienteId);
        }

        var dados = Validar(request);
        var pet = Pet.Criar(clienteId, dados);
        _db.Pets.Add(pet);
        await _db.SaveChangesAsync(cancellationToken);

        var faixas = await FaixasAtivasAsync(cancellationToken);
        return Map(pet, Sugerir(faixas, pet.PesoKg));
    }

    public async Task<PetDto> AtualizarAsync(long id, SalvarPetRequest request, CancellationToken cancellationToken = default)
    {
        var pet = await _db.Pets.FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
            ?? throw new NotFoundException("Pet", id);

        var dados = Validar(request);
        pet.Atualizar(dados);
        await _db.SaveChangesAsync(cancellationToken);

        var faixas = await FaixasAtivasAsync(cancellationToken);
        return Map(pet, Sugerir(faixas, pet.PesoKg));
    }

    public async Task InativarAsync(long id, CancellationToken cancellationToken = default)
    {
        var pet = await _db.Pets.FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
            ?? throw new NotFoundException("Pet", id);
        pet.Inativar();
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task ReativarAsync(long id, CancellationToken cancellationToken = default)
    {
        var pet = await _db.Pets.FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
            ?? throw new NotFoundException("Pet", id);
        pet.Reativar();
        await _db.SaveChangesAsync(cancellationToken);
    }

    private static DadosPet Validar(SalvarPetRequest r)
    {
        var erros = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(r.Nome))
        {
            erros["nome"] = ["Informe o nome do pet."];
        }

        if (r.PesoKg <= 0)
        {
            erros["pesoKg"] = ["O peso deve ser maior que zero."];
        }

        if (r.GramasDiaAjustadas is < 0)
        {
            erros["gramasDiaAjustadas"] = ["O valor não pode ser negativo."];
        }

        Sexo? sexo = null;
        if (!string.IsNullOrWhiteSpace(r.Sexo))
        {
            if (Enum.TryParse<Sexo>(r.Sexo, true, out var s))
            {
                sexo = s;
            }
            else
            {
                erros["sexo"] = ["Sexo inválido."];
            }
        }

        if (erros.Count > 0)
        {
            throw new ValidationException(erros);
        }

        return new DadosPet(
            r.Nome, r.Raca, r.PesoKg, r.DataNascimento, r.IdadeAprox, sexo,
            r.ObservacoesGerais, r.ObservacoesAlimentares, r.GramasDiaAjustadas);
    }

    private async Task<List<FaixaConsumo>> FaixasAtivasAsync(CancellationToken cancellationToken)
        => await _db.FaixasConsumo.Where(f => f.Ativo).ToListAsync(cancellationToken);

    private static int? Sugerir(List<FaixaConsumo> faixas, decimal peso)
        => faixas.FirstOrDefault(f => peso >= f.PesoInicial && peso <= f.PesoFinal)?.GramasPorDia;

    private static PetDto Map(Pet p, int? sugestao) => new(
        p.Id, p.ClienteId, p.Nome, p.Raca, p.PesoKg, p.DataNascimento, p.IdadeAprox,
        p.Sexo?.ToString(), p.Ativo, p.ObservacoesGerais, p.ObservacoesAlimentares,
        p.GramasDiaAjustadas, sugestao);
}
