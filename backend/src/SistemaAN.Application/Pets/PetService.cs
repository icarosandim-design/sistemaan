using Microsoft.EntityFrameworkCore;
using SistemaAN.Application.Common.Exceptions;
using SistemaAN.Application.Common.Interfaces;
using SistemaAN.Application.Entregas;
using SistemaAN.Domain.Clientes;
using SistemaAN.Domain.Consumo;
using SistemaAN.Domain.Entregas;
using SistemaAN.Domain.Estoque;
using SistemaAN.Domain.Pets;
using SistemaAN.Domain.Receitas;

namespace SistemaAN.Application.Pets;

public sealed class PetService : IPetService
{
    private readonly IApplicationDbContext _db;
    private readonly IEntregaService _entregas;

    public PetService(IApplicationDbContext db, IEntregaService entregas)
    {
        _db = db;
        _entregas = entregas;
    }

    private async Task RegerarEntregasAsync(long clienteId, CancellationToken ct)
    {
        try
        {
            await _entregas.RegerarFuturasDoClienteAsync(clienteId, "sistema", ct);
        }
        catch
        {
            // best-effort
        }
    }

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

    public async Task<IReadOnlyList<PetResumoDto>> ListarTodosAsync(string? busca, bool? ativo, string? tipo, CancellationToken cancellationToken = default)
    {
        // Pets pertencem a Clientes Pessoa Física.
        var q = from p in _db.Pets
                join c in _db.Clientes on p.ClienteId equals c.Id
                where c.Natureza == NaturezaCliente.PessoaFisica
                select new { Pet = p, Tutor = c.Nome };

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var t = busca.Trim();
            q = q.Where(x => x.Pet.Nome.Contains(t) || x.Tutor.Contains(t));
        }
        if (ativo.HasValue)
        {
            q = q.Where(x => x.Pet.Ativo == ativo.Value);
        }

        var rows = await q.OrderByDescending(x => x.Pet.Ativo).ThenBy(x => x.Pet.Nome).ToListAsync(cancellationToken);
        var petIds = rows.Select(x => x.Pet.Id).ToList();
        var clienteIds = rows.Select(x => x.Pet.ClienteId).Distinct().ToList();

        var planos = await _db.PlanosAlimentares
            .Where(pl => pl.Ativo && petIds.Contains(pl.PetId))
            .Include(pl => pl.Itens)
            .ToListAsync(cancellationToken);
        var planoPorPet = planos.ToDictionary(pl => pl.PetId);

        var receitaIds = planos.SelectMany(pl => pl.Itens).Select(i => i.ReceitaId).Distinct().ToList();
        var receitas = await _db.Receitas.Where(r => receitaIds.Contains(r.Id))
            .Select(r => new { r.Id, r.Codigo })
            .ToDictionaryAsync(r => r.Id, r => r.Codigo, cancellationToken);

        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);
        var ativos = new[] { EntregaStatus.Programada, EntregaStatus.ConfirmadaCliente, EntregaStatus.SaiuParaEntrega };
        var proximas = await _db.Entregas
            .Where(e => ativos.Contains(e.Status) && e.DataPrevista >= hoje && clienteIds.Contains(e.ClienteId))
            .GroupBy(e => e.ClienteId)
            .Select(g => new { ClienteId = g.Key, Data = g.Min(x => x.DataPrevista) })
            .ToListAsync(cancellationToken);
        var proximaPorCliente = proximas.ToDictionary(x => x.ClienteId, x => x.Data);

        var lista = rows.Select(x =>
        {
            planoPorPet.TryGetValue(x.Pet.Id, out var plano);
            string? tipoAlim = plano?.Tipo.ToString();
            string? receitaAtual = null;
            if (plano is not null)
            {
                var cods = plano.Itens.Select(i => receitas.TryGetValue(i.ReceitaId, out var cod) ? cod : null)
                    .Where(c => c is not null).Distinct().ToList();
                receitaAtual = cods.Count > 0 ? string.Join(", ", cods) : null;
            }
            DateOnly? proxima = proximaPorCliente.TryGetValue(x.Pet.ClienteId, out var d) ? d : null;
            return new PetResumoDto(
                x.Pet.Id, x.Pet.ClienteId, x.Pet.Nome, x.Tutor, x.Pet.Raca, x.Pet.PesoKg,
                x.Pet.Sexo?.ToString(), x.Pet.Ativo, tipoAlim, receitaAtual, proxima);
        }).ToList();

        if (!string.IsNullOrWhiteSpace(tipo))
        {
            lista = lista.Where(x => string.Equals(x.TipoAlimentacao, tipo, StringComparison.OrdinalIgnoreCase)).ToList();
        }
        return lista;
    }

    public async Task<PetDto> ObterAsync(long id, CancellationToken cancellationToken = default)
    {
        var pet = await _db.Pets.FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
            ?? throw new NotFoundException("Pet", id);
        var faixas = await FaixasAtivasAsync(cancellationToken);
        return Map(pet, Sugerir(faixas, pet.PesoKg));
    }

    public async Task<IReadOnlyList<PetReceitaProntaDto>> ProntosPorReceitaAsync(long petId, CancellationToken cancellationToken = default)
    {
        var existe = await _db.Pets.AnyAsync(p => p.Id == petId, cancellationToken);
        if (!existe)
        {
            throw new NotFoundException("Pet", petId);
        }

        // Considera entregas ativas (não entregues/canceladas/reagendadas) que incluem o pet.
        var ativos = new[] { EntregaStatus.Programada, EntregaStatus.ConfirmadaCliente, EntregaStatus.SaiuParaEntrega, EntregaStatus.NaoEntregue };
        var entregas = await _db.Entregas
            .Where(e => ativos.Contains(e.Status) && e.Pets.Any(p => p.PetId == petId))
            .Include(e => e.Pets).ThenInclude(p => p.Itens).ThenInclude(i => i.Pacotes)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        // Personalizada: pacotes reservados/produzidos para o pet (PacotesProntos).
        var personalizada = new Dictionary<(string Nome, int Peso), int>();
        // Casa: receitas do pet (estoque é geral; computamos o físico).
        var casa = new Dictionary<(long ReceitaId, int Peso), string>();

        foreach (var pet in entregas.SelectMany(e => e.Pets).Where(p => p.PetId == petId))
        {
            foreach (var item in pet.Itens)
            {
                if (item.Tipo == TipoReceita.Personalizada)
                {
                    var peso = item.TamanhoPacoteGramas ?? 0;
                    var prontos = item.StatusPreparo != StatusPreparoPersonalizada.NaoPronta ? item.PacotesProntos ?? 0 : 0;
                    var chave = (item.ReceitaNome, peso);
                    personalizada[chave] = (personalizada.TryGetValue(chave, out var v) ? v : 0) + prontos;
                }
                else
                {
                    foreach (var pac in item.Pacotes)
                    {
                        casa[(item.ReceitaId, pac.PesoGramas)] = item.ReceitaNome;
                    }
                }
            }
        }

        var resultado = new List<PetReceitaProntaDto>();
        foreach (var (chave, prontos) in personalizada)
        {
            resultado.Add(new PetReceitaProntaDto("Personalizada", chave.Nome, FormatarPeso(chave.Peso), prontos));
        }

        if (casa.Count > 0)
        {
            var pesoPorTamanho = await _db.TamanhosPacote.AsNoTracking()
                .Select(t => new { t.Id, t.PesoGramas })
                .ToDictionaryAsync(t => t.Id, t => t.PesoGramas, cancellationToken);
            var produtoAcabado = await _db.ItensEstoque.AsNoTracking()
                .Where(x => x.Tipo == TipoItemEstoque.ProdutoAcabadoCasa && x.ReceitaId != null && x.TamanhoPacoteId != null)
                .Select(x => new { ReceitaId = x.ReceitaId!.Value, TamanhoPacoteId = x.TamanhoPacoteId!.Value, x.QuantidadeAtual })
                .ToListAsync(cancellationToken);
            var fisicoPorChave = new Dictionary<(long, int), int>();
            foreach (var pa in produtoAcabado)
            {
                if (pesoPorTamanho.TryGetValue(pa.TamanhoPacoteId, out var peso))
                {
                    fisicoPorChave[(pa.ReceitaId, peso)] = (int)Math.Floor(pa.QuantidadeAtual);
                }
            }
            foreach (var (chave, nome) in casa)
            {
                var fisico = fisicoPorChave.TryGetValue(chave, out var f) ? f : 0;
                resultado.Add(new PetReceitaProntaDto("Casa", nome, FormatarPeso(chave.Item2), fisico));
            }
        }

        return resultado.OrderBy(r => r.Tipo).ThenBy(r => r.ReceitaNome).ToList();
    }

    private static string FormatarPeso(int gramas)
        => gramas >= 1000 ? $"{gramas / 1000m:0.##} kg" : $"{gramas} g";

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
        await RegerarEntregasAsync(clienteId, cancellationToken);

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
        await RegerarEntregasAsync(pet.ClienteId, cancellationToken);

        var faixas = await FaixasAtivasAsync(cancellationToken);
        return Map(pet, Sugerir(faixas, pet.PesoKg));
    }

    public async Task InativarAsync(long id, CancellationToken cancellationToken = default)
    {
        var pet = await _db.Pets.FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
            ?? throw new NotFoundException("Pet", id);
        pet.Inativar();
        await _db.SaveChangesAsync(cancellationToken);
        await RegerarEntregasAsync(pet.ClienteId, cancellationToken);
    }

    public async Task ReativarAsync(long id, CancellationToken cancellationToken = default)
    {
        var pet = await _db.Pets.FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
            ?? throw new NotFoundException("Pet", id);
        pet.Reativar();
        await _db.SaveChangesAsync(cancellationToken);
        await RegerarEntregasAsync(pet.ClienteId, cancellationToken);
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
