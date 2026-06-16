using Microsoft.EntityFrameworkCore;
using SistemaAN.Application.Common.Exceptions;
using SistemaAN.Application.Common.Interfaces;
using SistemaAN.Application.Estoque;
using SistemaAN.Domain.Entregas;
using SistemaAN.Domain.Estoque;
using SistemaAN.Domain.Receitas;
using SistemaAN.Domain.Rotas;

namespace SistemaAN.Application.Rotas;

public sealed class RotaService : IRotaService
{
    private static readonly EntregaStatus[] EntregaAtivos =
        [EntregaStatus.Programada, EntregaStatus.ConfirmadaCliente, EntregaStatus.SaiuParaEntrega, EntregaStatus.NaoEntregue];

    // Entregas que ainda PODEM entrar numa rota (não despachadas/entregues/canceladas).
    private static readonly EntregaStatus[] DisponivelParaRota =
        [EntregaStatus.Programada, EntregaStatus.ConfirmadaCliente];

    private static readonly StatusRota[] RotaAtivos =
        [StatusRota.Rascunho, StatusRota.Planejada, StatusRota.Despachada];

    private readonly IApplicationDbContext _db;
    private readonly IProdutoAcabadoService _produtoAcabado;

    public RotaService(IApplicationDbContext db, IProdutoAcabadoService produtoAcabado)
    {
        _db = db;
        _produtoAcabado = produtoAcabado;
    }

    public async Task<IReadOnlyList<RotaResumoDto>> ListarPorDataAsync(DateOnly data, CancellationToken cancellationToken = default)
        => await _db.Rotas
            .Where(r => r.Data == data)
            .OrderBy(r => r.Periodo).ThenBy(r => r.Id)
            .Select(r => new RotaResumoDto(r.Id, r.Data, r.Nome, r.Periodo.ToString(), r.Entregador, r.Status.ToString(), r.Paradas.Count))
            .ToListAsync(cancellationToken);

    public async Task<RotaDto> ObterAsync(long id, CancellationToken cancellationToken = default)
    {
        var rota = await _db.Rotas.Include(r => r.Paradas).AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken)
            ?? throw new NotFoundException("Rota", id);

        var entregaIds = rota.Paradas.Select(p => p.EntregaId).ToList();
        var entregas = await _db.Entregas
            .Where(e => entregaIds.Contains(e.Id))
            .Include(e => e.Pets).ThenInclude(p => p.Itens).ThenInclude(i => i.Pacotes)
            .AsNoTracking()
            .ToDictionaryAsync(e => e.Id, cancellationToken);

        var paradas = new List<RotaParadaDto>();
        foreach (var p in rota.Paradas.OrderBy(x => x.Ordem))
        {
            if (!entregas.TryGetValue(p.EntregaId, out var e))
            {
                continue;
            }
            var alerta = await EstoqueAlertaAsync(e, cancellationToken);
            paradas.Add(new RotaParadaDto(
                e.Id, p.Ordem, e.ClienteNome, e.PedidoId != null, e.PedidoId,
                EnderecoTexto(e), e.Bairro, e.Cidade, e.Telefone, e.PreferenciaHorario.ToString(),
                e.Status.ToString(), ItensResumo(e), EnderecoIncompleto(e), ProntidaoTexto(e), alerta,
                PetNomes(e), TotalGramas(e)));
        }

        return new RotaDto(rota.Id, rota.Data, rota.Nome, rota.Periodo.ToString(), rota.Entregador,
            rota.Status.ToString(), rota.Observacoes, paradas);
    }

    public async Task<IReadOnlyList<EntregaDisponivelDto>> DisponiveisAsync(DateOnly data, CancellationToken cancellationToken = default)
    {
        var ocupadas = await (from rp in _db.RotaParadas
                              join r in _db.Rotas on rp.RotaId equals r.Id
                              where RotaAtivos.Contains(r.Status)
                              select rp.EntregaId).Distinct().ToListAsync(cancellationToken);
        var ocupadasSet = ocupadas.ToHashSet();

        var entregas = await _db.Entregas
            .Where(e => e.DataPrevista == data && DisponivelParaRota.Contains(e.Status))
            .Include(e => e.Pets).ThenInclude(p => p.Itens).ThenInclude(i => i.Pacotes)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return entregas
            .Where(e => !ocupadasSet.Contains(e.Id))
            .OrderBy(e => e.PreferenciaHorario).ThenBy(e => e.ClienteNome)
            .Select(e => new EntregaDisponivelDto(
                e.Id, e.ClienteNome, e.PedidoId != null, e.PedidoId, EnderecoTexto(e), e.Bairro, e.Cidade, e.Telefone,
                e.PreferenciaHorario.ToString(), e.Status.ToString(), ItensResumo(e), EnderecoIncompleto(e), ProntidaoTexto(e), null,
                PetNomes(e), TotalGramas(e)))
            .ToList();
    }

    public async Task<RotaDto> CriarAsync(CriarRotaRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Nome))
        {
            throw Erro("nome", "Informe o nome da saída.");
        }
        var periodo = ParsePeriodo(request.Periodo);
        var rota = Rota.Criar(request.Data, request.Nome, periodo, request.Entregador, request.Observacoes);
        _db.Rotas.Add(rota);
        await _db.SaveChangesAsync(cancellationToken);
        return await ObterAsync(rota.Id, cancellationToken);
    }

    public async Task<RotaDto> AtualizarAsync(long id, AtualizarRotaRequest request, CancellationToken cancellationToken = default)
    {
        var rota = await CarregarAsync(id, cancellationToken);
        if (string.IsNullOrWhiteSpace(request.Nome))
        {
            throw Erro("nome", "Informe o nome da saída.");
        }
        rota.Atualizar(request.Nome, ParsePeriodo(request.Periodo), request.Entregador, request.Observacoes);
        await _db.SaveChangesAsync(cancellationToken);
        return await ObterAsync(id, cancellationToken);
    }

    public async Task<RotaDto> AdicionarEntregaAsync(long id, long entregaId, CancellationToken cancellationToken = default)
    {
        var rota = await CarregarAsync(id, cancellationToken);
        if (!rota.EhAtiva)
        {
            throw Erro("rota", "A rota não está ativa para receber entregas.");
        }

        var entrega = await _db.Entregas
            .Include(e => e.Pets).ThenInclude(p => p.Itens).ThenInclude(i => i.Pacotes)
            .FirstOrDefaultAsync(e => e.Id == entregaId, cancellationToken)
            ?? throw new NotFoundException("Entrega", entregaId);

        if (!EntregaAtivos.Contains(entrega.Status))
        {
            throw Erro("entrega", "Esta entrega não está disponível para rota (já entregue ou cancelada).");
        }
        if (EnderecoIncompleto(entrega))
        {
            throw Erro("entrega", "Endereço incompleto (logradouro, bairro e cidade). Complete o cadastro antes de adicionar à rota.");
        }

        var emOutraAtiva = await _db.Rotas.AnyAsync(
            r => r.Id != id && RotaAtivos.Contains(r.Status) && r.Paradas.Any(p => p.EntregaId == entregaId), cancellationToken);
        if (emOutraAtiva)
        {
            throw Erro("entrega", "Esta entrega já está vinculada a uma rota ativa.");
        }

        rota.AdicionarParada(entregaId);
        await _db.SaveChangesAsync(cancellationToken);
        return await ObterAsync(id, cancellationToken);
    }

    public async Task<RotaDto> RemoverEntregaAsync(long id, long entregaId, CancellationToken cancellationToken = default)
    {
        var rota = await CarregarAsync(id, cancellationToken);
        rota.RemoverParada(entregaId);
        await _db.SaveChangesAsync(cancellationToken);
        return await ObterAsync(id, cancellationToken);
    }

    public async Task<RotaDto> ReordenarAsync(long id, IReadOnlyList<long> entregaIds, CancellationToken cancellationToken = default)
    {
        var rota = await CarregarAsync(id, cancellationToken);
        rota.Reordenar(entregaIds ?? []);
        await _db.SaveChangesAsync(cancellationToken);
        return await ObterAsync(id, cancellationToken);
    }

    public async Task<RotaDto> MudarStatusAsync(long id, string status, string usuario, CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<StatusRota>(status, true, out var novo))
        {
            throw Erro("status", "Status de rota inválido.");
        }
        var rota = await CarregarAsync(id, cancellationToken);

        // Despachar: só as entregas DESTA rota passam para "Saiu para entrega".
        if (novo == StatusRota.Despachada)
        {
            var ids = rota.Paradas.Select(p => p.EntregaId).ToList();
            var entregas = await _db.Entregas.Where(e => ids.Contains(e.Id)).ToListAsync(cancellationToken);
            foreach (var e in entregas.Where(e => e.Status is EntregaStatus.Programada or EntregaStatus.ConfirmadaCliente))
            {
                e.MudarStatus(EntregaStatus.SaiuParaEntrega, usuario, $"Saiu para entrega — rota {rota.Nome}");
            }
        }

        rota.MudarStatus(novo);
        await _db.SaveChangesAsync(cancellationToken);
        return await ObterAsync(id, cancellationToken);
    }

    // ===================== Helpers =====================
    private async Task<Rota> CarregarAsync(long id, CancellationToken ct)
        => await _db.Rotas.Include(r => r.Paradas).FirstOrDefaultAsync(r => r.Id == id, ct)
            ?? throw new NotFoundException("Rota", id);

    private async Task<string> EstoqueAlertaAsync(Entrega e, CancellationToken ct)
    {
        var temCasa = e.Pets.SelectMany(p => p.Itens).Any(i => i.Tipo == TipoReceita.Casa);
        if (!temCasa)
        {
            return string.Empty;
        }
        var sit = await _produtoAcabado.SituacaoEntregaAsync(e.Id, ct);
        if (sit.Any(s => s.SemItemEstoque)) return "naocadastrado";
        if (sit.Any(s => s.TemFalta)) return "falta";
        return "ok";
    }

    private static string ItensResumo(Entrega e)
    {
        var partes = new List<string>();
        foreach (var pet in e.Pets)
        {
            foreach (var item in pet.Itens)
            {
                if (item.Tipo == TipoReceita.Casa)
                {
                    foreach (var pac in item.Pacotes)
                    {
                        partes.Add($"{item.ReceitaNome} {FormatarPeso(pac.PesoGramas)} — {pac.Quantidade}");
                    }
                }
                else
                {
                    partes.Add($"{item.ReceitaCodigo} {pet.PetNome} — {item.QuantidadePacotes ?? 0}");
                }
            }
        }
        return string.Join("; ", partes);
    }

    private static string? ProntidaoTexto(Entrega e)
    {
        var pers = e.Pets.SelectMany(p => p.Itens).Where(i => i.Tipo == TipoReceita.Personalizada).ToList();
        if (pers.Count == 0)
        {
            return null;
        }
        var total = pers.Sum(i => i.QuantidadePacotes ?? 0);
        var prontos = pers.Sum(i => i.StatusPreparo != StatusPreparoPersonalizada.NaoPronta ? i.PacotesProntos ?? 0 : 0);
        var rotulo = prontos >= total && total > 0 ? "Pronta" : prontos > 0 ? "Parcial" : "Não pronta";
        return $"{rotulo} {prontos}/{total}";
    }

    /// <summary>Nomes dos pets/cães da entrega (vazio para Pedido PJ, que usa container).</summary>
    private static string PetNomes(Entrega e)
    {
        var nomes = e.Pets
            .Where(p => p.PetId != null)
            .Select(p => p.PetNome)
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Distinct()
            .ToList();
        return string.Join(", ", nomes);
    }

    /// <summary>Peso total da entrega em gramas (Casa pelos pacotes; Personalizada por tamanho × pacotes).</summary>
    private static int TotalGramas(Entrega e)
    {
        var total = 0;
        foreach (var item in e.Pets.SelectMany(p => p.Itens))
        {
            if (item.Tipo == TipoReceita.Casa)
            {
                total += item.Pacotes.Sum(pac => pac.PesoGramas * pac.Quantidade);
            }
            else
            {
                total += (item.TamanhoPacoteGramas ?? 0) * (item.QuantidadePacotes ?? 0);
            }
        }
        return total;
    }

    private static string EnderecoTexto(Entrega e)
    {
        var partes = new List<string>();
        if (!string.IsNullOrWhiteSpace(e.Rua))
        {
            partes.Add(string.IsNullOrWhiteSpace(e.Numero) ? e.Rua! : $"{e.Rua}, {e.Numero}");
        }
        if (!string.IsNullOrWhiteSpace(e.Complemento))
        {
            partes.Add(e.Complemento!);
        }
        return partes.Count > 0 ? string.Join(" — ", partes) : "—";
    }

    private static bool EnderecoIncompleto(Entrega e)
        => string.IsNullOrWhiteSpace(e.Rua) || string.IsNullOrWhiteSpace(e.Bairro) || string.IsNullOrWhiteSpace(e.Cidade);

    private static string FormatarPeso(int gramas)
        => gramas >= 1000 ? $"{gramas / 1000m:0.##} kg" : $"{gramas} g";

    private static PeriodoRota ParsePeriodo(string? periodo)
        => Enum.TryParse<PeriodoRota>(periodo, true, out var p) ? p : PeriodoRota.HorarioComercial;

    private static ValidationException Erro(string campo, string msg)
        => new(new Dictionary<string, string[]> { [campo] = [msg] });
}
