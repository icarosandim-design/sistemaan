using Microsoft.EntityFrameworkCore;
using SistemaAN.Application.Common.Exceptions;
using SistemaAN.Application.Common.Interfaces;
using SistemaAN.Domain.Catalog;
using SistemaAN.Domain.Clientes;
using SistemaAN.Domain.Entregas;
using SistemaAN.Domain.Pets;
using SistemaAN.Domain.Planos;
using SistemaAN.Domain.Receitas;

namespace SistemaAN.Application.Entregas;

public sealed class EntregaService : IEntregaService
{
    private const int HorizontePadrao = 45;

    private readonly IApplicationDbContext _db;

    public EntregaService(IApplicationDbContext db) => _db = db;

    // ===================== Geração =====================
    public async Task<GerarEntregasResultado> GerarAsync(int horizonteDias, string usuario, CancellationToken ct = default)
    {
        var horizonte = horizonteDias > 0 ? horizonteDias : HorizontePadrao;
        var clientes = await ClientesElegiveisAsync(null, ct);
        var dados = await CarregarDadosAsync(ct);
        var dedup = await CarregarDedupAsync(ct);

        var (geradas, qtdClientes) = GerarPara(clientes, dados, dedup, horizonte, usuario);
        await _db.SaveChangesAsync(ct);
        return new GerarEntregasResultado(geradas, qtdClientes);
    }

    public async Task<GerarEntregasResultado> RegerarFuturasDoClienteAsync(long clienteId, string usuario, CancellationToken ct = default)
    {
        var cliente = await _db.Clientes.FirstOrDefaultAsync(c => c.Id == clienteId, ct)
            ?? throw new NotFoundException("Cliente", clienteId);

        var hoje = Hoje();

        // Remove apenas as futuras ainda não operacionais (Programada).
        var futuras = await _db.Entregas
            .Where(e => e.ClienteId == clienteId && e.Status == EntregaStatus.Programada && e.DataPrevista >= hoje)
            .ToListAsync(ct);
        _db.Entregas.RemoveRange(futuras);
        await _db.SaveChangesAsync(ct);

        if (!cliente.Ativo || cliente.FrequenciaEntregaId is null || cliente.PrimeiraEntrega is null)
        {
            return new GerarEntregasResultado(0, 0);
        }

        var dados = await CarregarDadosAsync(ct);
        var dedup = await CarregarDedupAsync(ct);
        var (geradas, qtd) = GerarPara([cliente], dados, dedup, HorizontePadrao, usuario);
        await _db.SaveChangesAsync(ct);
        return new GerarEntregasResultado(geradas, qtd);
    }

    public async Task<GerarEntregasResultado> RegerarFuturasDoPetAsync(long petId, string usuario, CancellationToken ct = default)
    {
        var clienteId = await _db.Pets.Where(p => p.Id == petId).Select(p => (long?)p.ClienteId).FirstOrDefaultAsync(ct);
        return clienteId is null
            ? new GerarEntregasResultado(0, 0)
            : await RegerarFuturasDoClienteAsync(clienteId.Value, usuario, ct);
    }

    public async Task<GerarEntregasResultado> RegerarPorReceitaAsync(long receitaId, string usuario, CancellationToken ct = default)
    {
        var planosAtivos = _db.PlanosAlimentares.Where(p => p.Ativo);
        var petIds = _db.PlanoItensReceita
            .Where(pi => pi.ReceitaId == receitaId)
            .Join(planosAtivos, pi => pi.PlanoAlimentarId, p => p.Id, (pi, p) => p.PetId);
        var clienteIds = await _db.Pets
            .Where(p => petIds.Contains(p.Id))
            .Select(p => p.ClienteId)
            .Distinct()
            .ToListAsync(ct);

        var total = 0;
        foreach (var cid in clienteIds)
        {
            var r = await RegerarFuturasDoClienteAsync(cid, usuario, ct);
            total += r.Geradas;
        }
        return new GerarEntregasResultado(total, clienteIds.Count);
    }

    public async Task<GerarEntregasResultado> AlterarAgendaFuturaAsync(
        long entregaId, DateOnly novaData, long? frequenciaEntregaId, string motivo, string usuario, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(motivo))
        {
            throw new ValidationException(Erro("motivo", "Informe o motivo da alteração da agenda."));
        }

        var entrega = await _db.Entregas.FirstOrDefaultAsync(e => e.Id == entregaId, ct)
            ?? throw new NotFoundException("Entrega", entregaId);
        var cliente = await _db.Clientes.FirstOrDefaultAsync(c => c.Id == entrega.ClienteId, ct)
            ?? throw new NotFoundException("Cliente", entrega.ClienteId);

        var freqId = frequenciaEntregaId ?? cliente.FrequenciaEntregaId;
        if (freqId is null)
        {
            throw new ValidationException(Erro("frequenciaEntregaId", "Defina a frequência de entrega."));
        }
        if (!await _db.FrequenciasEntrega.AnyAsync(f => f.Id == freqId && f.Ativo, ct))
        {
            throw new ValidationException(Erro("frequenciaEntregaId", "Frequência inválida ou inativa."));
        }

        // Ajusta o padrão do cliente e regera as futuras elegíveis.
        cliente.DefinirEntrega(freqId, novaData);
        await _db.SaveChangesAsync(ct);
        return await RegerarFuturasDoClienteAsync(cliente.Id, usuario, ct);
    }

    private (int geradas, int clientes) GerarPara(
        IReadOnlyList<Cliente> clientes, DadosGeracao d, HashSet<(long, DateOnly)> dedup, int horizonte, string usuario)
    {
        var hoje = Hoje();
        var limite = hoje.AddDays(horizonte);
        var geradas = 0;
        var atendidos = new HashSet<long>();

        foreach (var c in clientes)
        {
            if (c.FrequenciaEntregaId is null || c.PrimeiraEntrega is null)
            {
                continue;
            }
            if (!d.Frequencias.TryGetValue(c.FrequenciaEntregaId.Value, out var freq) || freq.DiasCiclo is null or <= 0)
            {
                continue;
            }
            if (ConstruirPets(c.Id, d).Count == 0)
            {
                continue; // sem pets ativos com plano
            }

            foreach (var data in Datas(c.PrimeiraEntrega.Value, freq.DiasCiclo.Value, hoje, limite))
            {
                if (!dedup.Add((c.Id, data)))
                {
                    continue;
                }
                var entrega = Entrega.Criar(c.Id, data, Snapshot(c, freq));
                foreach (var pet in ConstruirPets(c.Id, d))
                {
                    entrega.AdicionarPet(pet);
                }
                entrega.RegistrarHistorico(usuario, "Entrega criada automaticamente", null, EntregaStatus.Programada);
                _db.Entregas.Add(entrega);
                geradas++;
                atendidos.Add(c.Id);
            }
        }

        return (geradas, atendidos.Count);
    }

    // ===================== Listagem / detalhe =====================
    public async Task<IReadOnlyList<EntregaResumoDto>> ListarAsync(
        DateOnly? data, string? status, long? clienteId, string? bairro, string? cidade, CancellationToken ct = default)
    {
        var query = _db.Entregas
            .Include(e => e.Pets).ThenInclude(p => p.Itens).ThenInclude(i => i.Pacotes)
            .AsQueryable();

        if (data is { } dt)
        {
            query = query.Where(e => e.DataPrevista == dt);
        }
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<EntregaStatus>(status, true, out var st))
        {
            query = query.Where(e => e.Status == st);
        }
        if (clienteId is { } cid)
        {
            query = query.Where(e => e.ClienteId == cid);
        }
        if (!string.IsNullOrWhiteSpace(bairro))
        {
            query = query.Where(e => e.Bairro == bairro);
        }
        if (!string.IsNullOrWhiteSpace(cidade))
        {
            query = query.Where(e => e.Cidade == cidade);
        }

        var lista = await query.OrderBy(e => e.DataPrevista).ThenBy(e => e.ClienteNome).ToListAsync(ct);
        return lista.Select(MapResumo).ToList();
    }

    public async Task<EntregaDto> ObterAsync(long id, CancellationToken ct = default)
    {
        var entrega = await CarregarCompletaAsync(id, ct);
        return Map(entrega);
    }

    // ===================== Transições =====================
    public async Task<EntregaDto> MudarStatusAsync(long id, string status, string usuario, CancellationToken ct = default)
    {
        var entrega = await CarregarCompletaAsync(id, ct);

        if (!Enum.TryParse<EntregaStatus>(status, true, out var novo))
        {
            throw new ValidationException(Erro("status", "Status inválido."));
        }

        var (ok, evento) = novo switch
        {
            EntregaStatus.ConfirmadaCliente => (entrega.Status == EntregaStatus.Programada, "Confirmada com cliente"),
            EntregaStatus.SaiuParaEntrega => (entrega.Status is EntregaStatus.ConfirmadaCliente or EntregaStatus.Programada, "Saiu para entrega"),
            EntregaStatus.Entregue => (entrega.Status == EntregaStatus.SaiuParaEntrega, "Entregue"),
            _ => (false, string.Empty),
        };

        if (!ok)
        {
            throw new ValidationException(Erro("status", $"Transição inválida de {entrega.Status} para {novo}."));
        }

        entrega.MudarStatus(novo, usuario, evento);
        await _db.SaveChangesAsync(ct);
        return Map(entrega);
    }

    public async Task<EntregaDto> MarcarNaoEntregueAsync(long id, string motivo, string usuario, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(motivo))
        {
            throw new ValidationException(Erro("motivo", "Informe o motivo da não entrega."));
        }
        var entrega = await CarregarCompletaAsync(id, ct);
        if (entrega.Status is EntregaStatus.Cancelada or EntregaStatus.Reagendada or EntregaStatus.Entregue)
        {
            throw new ValidationException(Erro("status", "Entrega não pode ser marcada como não entregue neste status."));
        }
        entrega.MarcarNaoEntregue(motivo, usuario);
        await _db.SaveChangesAsync(ct);
        return Map(entrega);
    }

    public async Task<EntregaDto> ReagendarAsync(long id, DateOnly novaData, string motivo, string usuario, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(motivo))
        {
            throw new ValidationException(Erro("motivo", "Informe o motivo do reagendamento."));
        }
        var original = await CarregarCompletaAsync(id, ct);
        if (original.Status is EntregaStatus.Cancelada or EntregaStatus.Reagendada)
        {
            throw new ValidationException(Erro("status", "Esta entrega não pode ser reagendada."));
        }

        // Cria a nova entrega clonando o snapshot na nova data.
        var nova = Entrega.Criar(original.ClienteId, novaData, SnapshotDe(original));
        foreach (var pet in ClonarPets(original))
        {
            nova.AdicionarPet(pet);
        }
        nova.DefinirOrigemReagendamento(original.Id);
        nova.RegistrarHistorico(usuario, $"Criada por reagendamento de {original.DataPrevista:dd/MM/yyyy}", null, EntregaStatus.Programada);
        _db.Entregas.Add(nova);
        await _db.SaveChangesAsync(ct);

        original.Reagendar(motivo, usuario, nova.Id);
        await _db.SaveChangesAsync(ct);

        return await ObterAsync(nova.Id, ct);
    }

    public async Task<EntregaDto> CancelarAsync(long id, string motivo, string usuario, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(motivo))
        {
            throw new ValidationException(Erro("motivo", "Informe o motivo do cancelamento."));
        }
        var entrega = await CarregarCompletaAsync(id, ct);
        if (entrega.Status == EntregaStatus.Cancelada)
        {
            throw new ValidationException(Erro("status", "Entrega já está cancelada."));
        }
        entrega.Cancelar(motivo, usuario);
        await _db.SaveChangesAsync(ct);
        return Map(entrega);
    }

    // ===================== Carga de dados =====================
    private async Task<List<Cliente>> ClientesElegiveisAsync(long? clienteId, CancellationToken ct)
        => await _db.Clientes
            .Where(c => c.Ativo && c.FrequenciaEntregaId != null && c.PrimeiraEntrega != null
                && (clienteId == null || c.Id == clienteId))
            .ToListAsync(ct);

    private async Task<HashSet<(long, DateOnly)>> CarregarDedupAsync(CancellationToken ct)
    {
        var chaves = await _db.Entregas
            .Where(e => e.Status != EntregaStatus.Cancelada && e.Status != EntregaStatus.Reagendada)
            .Select(e => new { e.ClienteId, e.DataPrevista })
            .ToListAsync(ct);
        return chaves.Select(k => (k.ClienteId, k.DataPrevista)).ToHashSet();
    }

    private async Task<DadosGeracao> CarregarDadosAsync(CancellationToken ct)
    {
        var frequencias = await _db.FrequenciasEntrega.ToDictionaryAsync(f => f.Id, ct);
        var tamanhos = await _db.TamanhosPacote.ToDictionaryAsync(t => t.Id, ct);
        var receitas = await _db.Receitas.Include(r => r.Itens).ToDictionaryAsync(r => r.Id, ct);
        var ingredientes = await _db.Ingredientes.Include(i => i.Categoria).ToDictionaryAsync(i => i.Id, ct);
        var pets = await _db.Pets.Where(p => p.Ativo).ToListAsync(ct);
        var planos = await _db.PlanosAlimentares
            .Where(p => p.Ativo)
            .Include(p => p.Itens).ThenInclude(i => i.Pacotes)
            .ToListAsync(ct);

        return new DadosGeracao(
            frequencias,
            tamanhos,
            receitas,
            ingredientes,
            pets.ToLookup(p => p.ClienteId),
            planos.ToDictionary(p => p.PetId));
    }

    private async Task<Entrega> CarregarCompletaAsync(long id, CancellationToken ct)
        => await _db.Entregas
            .Include(e => e.Pets).ThenInclude(p => p.Itens).ThenInclude(i => i.Pacotes)
            .Include(e => e.Pets).ThenInclude(p => p.Itens).ThenInclude(i => i.Ingredientes)
            .Include(e => e.Historico)
            .FirstOrDefaultAsync(e => e.Id == id, ct)
            ?? throw new NotFoundException("Entrega", id);

    // ===================== Construção do snapshot =====================
    private List<EntregaPet> ConstruirPets(long clienteId, DadosGeracao d)
    {
        var lista = new List<EntregaPet>();
        foreach (var pet in d.PetsPorCliente[clienteId])
        {
            if (!d.PlanoPorPet.TryGetValue(pet.Id, out var plano))
            {
                continue;
            }

            var itens = new List<EntregaItem>();
            var totalPet = 0;

            foreach (var pi in plano.Itens)
            {
                if (!d.Receitas.TryGetValue(pi.ReceitaId, out var receita))
                {
                    continue;
                }

                if (plano.Tipo == TipoReceita.Casa)
                {
                    var pacotes = pi.Pacotes.Select(pp =>
                    {
                        d.Tamanhos.TryGetValue(pp.TamanhoPacoteId, out var tam);
                        var peso = tam?.PesoGramas ?? 0;
                        var label = tam?.Nome ?? $"{peso} g";
                        return EntregaItemPacote.Criar(label, peso, pp.Quantidade);
                    }).ToList();
                    totalPet += pacotes.Sum(p => p.PesoGramas * p.Quantidade);
                    itens.Add(EntregaItem.CriarCasa(receita.Id, receita.Codigo, receita.Nome, pi.QuantidadeCicloGramas ?? 0, pacotes));
                }
                else
                {
                    var tamanho = receita.Itens.Sum(it => it.Gramas);
                    var qtd = pi.QuantidadePacotes ?? 0;
                    totalPet += tamanho * qtd;
                    var ingredientes = receita.Itens.Select(it =>
                    {
                        d.Ingredientes.TryGetValue(it.IngredienteId, out var ing);
                        return EntregaItemIngrediente.Criar(
                            it.IngredienteId,
                            ing?.Nome ?? "(removido)",
                            ing?.Categoria?.Nome ?? string.Empty,
                            it.Gramas,
                            ing?.CustoAtualKg ?? 0m,
                            ing?.CoeficienteConversao ?? 0m);
                    }).ToList();
                    itens.Add(EntregaItem.CriarPersonalizada(receita.Id, receita.Codigo, receita.Nome, tamanho, qtd, ingredientes));
                }
            }

            if (itens.Count == 0)
            {
                continue;
            }

            var gramasDia = plano.GramasDiaAjustadas ?? plano.GramasDiaSugeridas ?? pet.GramasDiaAjustadas;
            lista.Add(EntregaPet.Criar(pet.Id, pet.Nome, plano.Tipo, gramasDia, totalPet, itens));
        }

        return lista;
    }

    private static List<EntregaPet> ClonarPets(Entrega original)
    {
        var lista = new List<EntregaPet>();
        foreach (var op in original.Pets)
        {
            var itens = new List<EntregaItem>();
            foreach (var oi in op.Itens)
            {
                if (oi.Tipo == TipoReceita.Casa)
                {
                    var pacotes = oi.Pacotes.Select(p => EntregaItemPacote.Criar(p.TamanhoLabel, p.PesoGramas, p.Quantidade));
                    itens.Add(EntregaItem.CriarCasa(oi.ReceitaId, oi.ReceitaCodigo, oi.ReceitaNome, oi.QuantidadeCicloGramas ?? 0, pacotes));
                }
                else
                {
                    var ingr = oi.Ingredientes.Select(g =>
                        EntregaItemIngrediente.Criar(g.IngredienteId, g.IngredienteNome, g.Categoria, g.GramasCozidas, g.CustoKgCru, g.Coeficiente));
                    itens.Add(EntregaItem.CriarPersonalizada(oi.ReceitaId, oi.ReceitaCodigo, oi.ReceitaNome, oi.TamanhoPacoteGramas ?? 0, oi.QuantidadePacotes ?? 0, ingr));
                }
            }
            lista.Add(EntregaPet.Criar(op.PetId, op.PetNome, op.TipoAlimentacao, op.GramasDia, op.QuantidadeTotalGramas, itens));
        }
        return lista;
    }

    private static DadosSnapshotEntrega Snapshot(Cliente c, Domain.Entregas.FrequenciaEntrega freq)
        => new(c.Nome, c.Telefone, c.Rua, c.Numero, c.Complemento, c.Cep, c.Bairro, c.Cidade, c.Estado,
            freq.Nome, freq.DiasCiclo ?? 0);

    private static DadosSnapshotEntrega SnapshotDe(Entrega e)
        => new(e.ClienteNome, e.Telefone, e.Rua, e.Numero, e.Complemento, e.Cep, e.Bairro, e.Cidade, e.Estado,
            e.FrequenciaNome, e.DiasCiclo);

    // ===================== Datas =====================
    private static DateOnly Hoje() => DateOnly.FromDateTime(DateTime.UtcNow);

    private static IEnumerable<DateOnly> Datas(DateOnly primeira, int dias, DateOnly hoje, DateOnly limite)
    {
        if (dias <= 0)
        {
            yield break;
        }
        var data = primeira;
        if (data < hoje)
        {
            var faltam = hoje.DayNumber - data.DayNumber;
            var passos = (faltam + dias - 1) / dias;
            data = data.AddDays(passos * dias);
        }
        while (data <= limite)
        {
            yield return data;
            data = data.AddDays(dias);
        }
    }

    // ===================== Mapeamentos =====================
    private static Dictionary<string, string[]> Erro(string campo, string msg) => new() { [campo] = [msg] };

    private static EntregaResumoDto MapResumo(Entrega e)
    {
        var tipos = e.Pets.Select(p => p.TipoAlimentacao.ToString()).Distinct().ToList();
        var totalPacotes = e.Pets
            .SelectMany(p => p.Itens)
            .Sum(i => i.Tipo == TipoReceita.Casa ? i.Pacotes.Sum(x => x.Quantidade) : i.QuantidadePacotes ?? 0);
        return new EntregaResumoDto(
            e.Id, e.ClienteId, e.ClienteNome, e.DataPrevista, e.Status.ToString(), e.Bairro, e.Cidade,
            string.Join(", ", tipos), e.Pets.Sum(p => p.QuantidadeTotalGramas), totalPacotes, e.EntregadorId,
            e.Pets.Select(p => p.PetNome).ToList());
    }

    private static EntregaDto Map(Entrega e) => new(
        e.Id, e.ClienteId, e.DataPrevista, e.Status.ToString(), e.ClienteNome, e.Telefone,
        e.Rua, e.Numero, e.Complemento, e.Cep, e.Bairro, e.Cidade, e.Estado, e.FrequenciaNome, e.DiasCiclo,
        e.ObservacoesInternas, e.ObservacoesEntregador, e.EntregadorId,
        e.MotivoNaoEntrega, e.MotivoReagendamento, e.ReagendadaDeId, e.ReagendadaParaId, e.MotivoCancelamento,
        e.Pets.OrderBy(p => p.Id).Select(MapPet).ToList(),
        e.Historico.OrderBy(h => h.Quando).Select(h => new EntregaHistoricoDto(
            h.Quando, h.Usuario, h.Evento, h.StatusDe?.ToString(), h.StatusPara?.ToString())).ToList());

    private static EntregaPetDto MapPet(EntregaPet p) => new(
        p.Id, p.PetId, p.PetNome, p.TipoAlimentacao.ToString(), p.GramasDia, p.QuantidadeTotalGramas,
        p.Itens.OrderBy(i => i.Id).Select(MapItem).ToList());

    private static EntregaItemDto MapItem(EntregaItem i) => new(
        i.Id, i.ReceitaId, i.ReceitaCodigo, i.ReceitaNome, i.Tipo.ToString(),
        i.QuantidadeCicloGramas, i.TamanhoPacoteGramas, i.QuantidadePacotes,
        i.Pacotes.OrderBy(x => x.Id).Select(x => new EntregaItemPacoteDto(x.TamanhoLabel, x.PesoGramas, x.Quantidade)).ToList(),
        i.Ingredientes.OrderBy(x => x.Id).Select(x => new EntregaItemIngredienteDto(x.IngredienteId, x.IngredienteNome, x.Categoria, x.GramasCozidas)).ToList());

    private sealed record DadosGeracao(
        Dictionary<long, Domain.Entregas.FrequenciaEntrega> Frequencias,
        Dictionary<long, Domain.Pacotes.TamanhoPacote> Tamanhos,
        Dictionary<long, Receita> Receitas,
        Dictionary<long, Ingrediente> Ingredientes,
        ILookup<long, Pet> PetsPorCliente,
        Dictionary<long, PlanoAlimentar> PlanoPorPet);
}
