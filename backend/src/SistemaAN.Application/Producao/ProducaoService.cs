using Microsoft.EntityFrameworkCore;
using SistemaAN.Application.Common.Exceptions;
using SistemaAN.Application.Common.Interfaces;
using SistemaAN.Domain.Entregas;
using SistemaAN.Domain.Estoque;
using SistemaAN.Domain.Producao;
using SistemaAN.Domain.Receitas;

namespace SistemaAN.Application.Producao;

public sealed class ProducaoService : IProducaoService
{
    private static readonly EntregaStatus[] StatusInativos = [EntregaStatus.Cancelada, EntregaStatus.Reagendada];

    private readonly IApplicationDbContext _db;
    private readonly Estoque.IEstoqueMovimentacaoService _estoque;

    public ProducaoService(IApplicationDbContext db, Estoque.IEstoqueMovimentacaoService estoque)
    {
        _db = db;
        _estoque = estoque;
    }

    // ===================== Demanda =====================
    public async Task<DemandaDto> ObterDemandaAsync(DateOnly inicio, DateOnly fim, CancellationToken cancellationToken = default)
    {
        var entregas = await _db.Entregas
            .Where(e => e.DataPrevista >= inicio && e.DataPrevista <= fim && !StatusInativos.Contains(e.Status))
            .Include(e => e.Pets).ThenInclude(p => p.Itens).ThenInclude(i => i.Pacotes)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        // Personalizadas não prontas, priorizadas pela entrega mais próxima.
        var personalizadas = new List<DemandaPersonalizadaDto>();
        foreach (var e in entregas)
        {
            foreach (var p in e.Pets)
            {
                foreach (var i in p.Itens)
                {
                    if (i.Tipo != TipoReceita.Personalizada || i.StatusPreparo == StatusPreparoPersonalizada.Pronta)
                    {
                        continue;
                    }

                    personalizadas.Add(new DemandaPersonalizadaDto(
                        i.Id, e.Id, p.Id, p.PetId, e.ClienteId, p.PetNome, e.ClienteNome,
                        i.ReceitaCodigo, i.ReceitaNome, e.DataPrevista,
                        i.QuantidadePacotes ?? 0, i.TamanhoPacoteGramas ?? 0, i.StatusPreparo.ToString()));
                }
            }
        }
        personalizadas = personalizadas
            .OrderBy(x => x.DataEntrega)
            .ThenBy(x => x.StatusPreparo == "NaoPronta" ? 0 : 1)
            .ToList();

        // Casa: necessário por (receita, peso) × estoque de produto acabado.
        var necessarioCasa = new Dictionary<(long ReceitaId, int Peso), (string Nome, int Qtd)>();
        foreach (var e in entregas)
        {
            foreach (var p in e.Pets)
            {
                foreach (var i in p.Itens)
                {
                    if (i.Tipo != TipoReceita.Casa)
                    {
                        continue;
                    }
                    foreach (var pac in i.Pacotes)
                    {
                        var chave = (i.ReceitaId, pac.PesoGramas);
                        var atual = necessarioCasa.TryGetValue(chave, out var v) ? v : (i.ReceitaNome, 0);
                        necessarioCasa[chave] = (atual.Nome, atual.Qtd + pac.Quantidade);
                    }
                }
            }
        }

        var tamanhos = await _db.TamanhosPacote.AsNoTracking()
            .Select(t => new { t.Id, t.Nome, t.PesoGramas })
            .ToListAsync(cancellationToken);
        var produtoAcabado = await _db.ItensEstoque.AsNoTracking()
            .Where(x => x.Tipo == TipoItemEstoque.ProdutoAcabadoCasa)
            .Select(x => new { x.Id, x.ReceitaId, x.TamanhoPacoteId, x.QuantidadeAtual })
            .ToListAsync(cancellationToken);

        var casa = new List<DemandaCasaDto>();
        foreach (var (chave, val) in necessarioCasa)
        {
            var tam = tamanhos.FirstOrDefault(t => t.PesoGramas == chave.Peso);
            var item = produtoAcabado.FirstOrDefault(x => x.ReceitaId == chave.ReceitaId && tam != null && x.TamanhoPacoteId == tam.Id);
            var estoque = item != null ? (int)Math.Floor(item.QuantidadeAtual) : 0;
            casa.Add(new DemandaCasaDto(
                chave.ReceitaId, val.Nome, tam?.Id, tam?.Nome ?? $"{chave.Peso} g", chave.Peso,
                val.Qtd, estoque, Math.Max(0, val.Qtd - estoque), item?.Id));
        }
        casa = casa.OrderByDescending(c => c.Falta).ThenBy(c => c.ReceitaNome).ToList();

        return new DemandaDto(personalizadas, casa);
    }

    // ===================== Ordens =====================
    public async Task<IReadOnlyList<OrdemProducaoResumoDto>> ListarOrdensAsync(CancellationToken cancellationToken = default)
        => await _db.OrdensProducao
            .OrderByDescending(o => o.Data)
            .Select(o => new OrdemProducaoResumoDto(o.Id, o.Data, o.Status.ToString(), o.Fichas.Count))
            .ToListAsync(cancellationToken);

    public async Task<OrdemProducaoDto?> ObterPorDataAsync(DateOnly data, CancellationToken cancellationToken = default)
    {
        var id = await _db.OrdensProducao.Where(o => o.Data == data).Select(o => (long?)o.Id).FirstOrDefaultAsync(cancellationToken);
        return id is null ? null : await ObterAsync(id.Value, cancellationToken);
    }

    public async Task<OrdemProducaoDto> ObterAsync(long id, CancellationToken cancellationToken = default)
    {
        var ordem = await _db.OrdensProducao
            .Include(o => o.Fichas).ThenInclude(f => f.Ingredientes)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken)
            ?? throw new NotFoundException("Ordem de produção", id);

        return await MapOrdemAsync(ordem, cancellationToken);
    }

    public async Task<OrdemProducaoDto> CriarOuObterOrdemAsync(DateOnly data, CancellationToken cancellationToken = default)
    {
        var existente = await _db.OrdensProducao.FirstOrDefaultAsync(o => o.Data == data, cancellationToken);
        if (existente is not null)
        {
            return await ObterAsync(existente.Id, cancellationToken);
        }

        var ordem = OrdemProducao.Criar(data);
        _db.OrdensProducao.Add(ordem);
        await _db.SaveChangesAsync(cancellationToken);
        return await ObterAsync(ordem.Id, cancellationToken);
    }

    public async Task<OrdemProducaoDto> AdicionarFichasAsync(long ordemId, AdicionarFichasRequest request, CancellationToken cancellationToken = default)
    {
        var ordem = await _db.OrdensProducao.Include(o => o.Fichas).FirstOrDefaultAsync(o => o.Id == ordemId, cancellationToken)
            ?? throw new NotFoundException("Ordem de produção", ordemId);

        if (!ordem.Editavel)
        {
            throw new ValidationException(new Dictionary<string, string[]> { ["ordem"] = ["A ordem já está finalizada e não pode ser editada."] });
        }

        // ----- Personalizadas -----
        var idsNovos = (request.EntregaItemIds ?? []).Where(id => ordem.Fichas.All(f => f.EntregaItemId != id)).ToList();
        if (idsNovos.Count > 0)
        {
            var entregas = await _db.Entregas
                .Where(e => e.Pets.Any(p => p.Itens.Any(i => idsNovos.Contains(i.Id))))
                .Include(e => e.Pets).ThenInclude(p => p.Itens).ThenInclude(i => i.Ingredientes)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            foreach (var e in entregas)
            {
                foreach (var p in e.Pets)
                {
                    foreach (var i in p.Itens.Where(x => idsNovos.Contains(x.Id) && x.Tipo == TipoReceita.Personalizada))
                    {
                        var ingredientes = i.Ingredientes.Select(g =>
                            FichaProducaoIngrediente.Criar(g.IngredienteId, g.IngredienteNome, g.Categoria, g.GramasCozidas, g.Coeficiente));
                        ordem.AdicionarFicha(FichaProducao.CriarPersonalizada(
                            e.Id, p.Id, i.Id, p.PetId, e.ClienteId, e.ClienteNome, p.PetNome,
                            i.ReceitaCodigo, i.ReceitaNome, e.DataPrevista,
                            i.QuantidadePacotes ?? 0, i.TamanhoPacoteGramas ?? 0, ingredientes));
                    }
                }
            }
        }

        // ----- Casa -----
        var casa = request.Casa ?? [];
        if (casa.Count > 0)
        {
            var receitaIds = casa.Select(c => c.ReceitaId).Distinct().ToList();
            var receitas = await _db.Receitas.Where(r => receitaIds.Contains(r.Id)).Include(r => r.Itens).AsNoTracking().ToListAsync(cancellationToken);
            var ingredientes = await _db.Ingredientes.AsNoTracking().Select(x => new { x.Id, x.Nome, x.CategoriaId, x.CoeficienteConversao }).ToListAsync(cancellationToken);
            var categorias = await _db.CategoriasIngredientes.AsNoTracking().ToDictionaryAsync(c => c.Id, c => c.Nome, cancellationToken);
            var tamanhos = await _db.TamanhosPacote.AsNoTracking().ToDictionaryAsync(t => t.Id, t => t.PesoGramas, cancellationToken);
            var produtoAcabado = await _db.ItensEstoque.AsNoTracking()
                .Where(x => x.Tipo == TipoItemEstoque.ProdutoAcabadoCasa)
                .Select(x => new { x.Id, x.ReceitaId, x.TamanhoPacoteId })
                .ToListAsync(cancellationToken);

            foreach (var c in casa)
            {
                var receita = receitas.FirstOrDefault(r => r.Id == c.ReceitaId);
                if (receita is null || !tamanhos.TryGetValue(c.TamanhoPacoteId, out var pesoPacote) || c.QuantidadePacotes <= 0)
                {
                    continue;
                }

                var somaReceita = receita.Itens.Sum(it => it.Gramas);
                var totalCozido = c.QuantidadePacotes * pesoPacote;
                var fator = somaReceita > 0 ? (decimal)totalCozido / somaReceita : 0m;

                var fichaIngs = new List<FichaProducaoIngrediente>();
                foreach (var it in receita.Itens)
                {
                    var ing = ingredientes.FirstOrDefault(x => x.Id == it.IngredienteId);
                    var nome = ing?.Nome ?? "—";
                    var catNome = ing != null && categorias.TryGetValue(ing.CategoriaId, out var cn) ? cn : "—";
                    var coef = ing?.CoeficienteConversao ?? 1m;
                    var gramasCozidas = (int)Math.Round(it.Gramas * fator, MidpointRounding.AwayFromZero);
                    fichaIngs.Add(FichaProducaoIngrediente.Criar(it.IngredienteId, nome, catNome, gramasCozidas, coef));
                }

                var itemEstoqueId = produtoAcabado.FirstOrDefault(x => x.ReceitaId == c.ReceitaId && x.TamanhoPacoteId == c.TamanhoPacoteId)?.Id;
                ordem.AdicionarFicha(FichaProducao.CriarCasa(
                    c.ReceitaId, c.TamanhoPacoteId, itemEstoqueId, receita.Codigo, receita.Nome,
                    c.QuantidadePacotes, pesoPacote, fichaIngs));
            }
        }

        ordem.Iniciar();
        await _db.SaveChangesAsync(cancellationToken);
        return await ObterAsync(ordem.Id, cancellationToken);
    }

    public async Task<OrdemProducaoDto> RemoverFichaAsync(long fichaId, CancellationToken cancellationToken = default)
    {
        var ordem = await _db.OrdensProducao.Include(o => o.Fichas)
            .FirstOrDefaultAsync(o => o.Fichas.Any(f => f.Id == fichaId), cancellationToken)
            ?? throw new NotFoundException("Ficha de produção", fichaId);

        if (!ordem.Editavel)
        {
            throw new ValidationException(new Dictionary<string, string[]> { ["ordem"] = ["A ordem já está finalizada e não pode ser editada."] });
        }

        var ficha = ordem.Fichas.First(f => f.Id == fichaId);
        ordem.RemoverFicha(ficha);
        await _db.SaveChangesAsync(cancellationToken);
        return await ObterAsync(ordem.Id, cancellationToken);
    }

    // ===================== Execução / finalização (Fase 3) =====================
    public async Task<OrdemProducaoDto> MudarStatusFichaAsync(long fichaId, string status, CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<StatusFichaProducao>(status, out var st))
        {
            throw new ValidationException(new Dictionary<string, string[]> { ["status"] = ["Status de ficha inválido."] });
        }
        var ficha = await CarregarFichaAsync(fichaId, cancellationToken);
        ficha.MudarStatus(st);
        await _db.SaveChangesAsync(cancellationToken);
        return await ObterAsync(ficha.OrdemProducaoId, cancellationToken);
    }

    public async Task<OrdemProducaoDto> ConcluirFichaAsync(long fichaId, ConcluirFichaRequest request, string usuario, CancellationToken cancellationToken = default)
    {
        var ficha = await CarregarFichaAsync(fichaId, cancellationToken);
        if (request.PacotesReais < 0)
        {
            throw new ValidationException(new Dictionary<string, string[]> { ["pacotesReais"] = ["Quantidade inválida."] });
        }
        ficha.Concluir(request.PacotesReais, request.PesoEnvasadoGramas, usuario, DateTimeOffset.UtcNow, request.Observacoes);
        await _db.SaveChangesAsync(cancellationToken);
        return await ObterAsync(ficha.OrdemProducaoId, cancellationToken);
    }

    public async Task<OrdemProducaoDto> MarcarNaoFeitaAsync(long fichaId, string motivo, string usuario, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(motivo))
        {
            throw new ValidationException(new Dictionary<string, string[]> { ["motivo"] = ["Informe o motivo."] });
        }
        var ficha = await CarregarFichaAsync(fichaId, cancellationToken);
        ficha.MarcarNaoFeita(motivo.Trim(), usuario, DateTimeOffset.UtcNow);
        await _db.SaveChangesAsync(cancellationToken);
        return await ObterAsync(ficha.OrdemProducaoId, cancellationToken);
    }

    public async Task<OrdemProducaoDto> RegistrarConsumoAsync(long ordemId, RegistrarConsumoRequest request, CancellationToken cancellationToken = default)
    {
        var ordem = await CarregarOrdemCompletaAsync(ordemId, cancellationToken);
        await GarantirConsumosAsync(ordem, cancellationToken);

        foreach (var item in request.Itens ?? [])
        {
            var consumo = ordem.Consumos.FirstOrDefault(c => c.IngredienteId == item.IngredienteId);
            consumo?.RegistrarReal(item.RealCruGramas, item.RealCozidoGramas, item.Motivo);
        }
        await _db.SaveChangesAsync(cancellationToken);
        return await ObterAsync(ordem.Id, cancellationToken);
    }

    public async Task<FinalizacaoResultadoDto> FinalizarAsync(long ordemId, FinalizarProducaoRequest request, string usuario, CancellationToken cancellationToken = default)
    {
        var ordem = await CarregarOrdemCompletaAsync(ordemId, cancellationToken);
        if (!ordem.Editavel)
        {
            throw new ValidationException(new Dictionary<string, string[]> { ["ordem"] = ["A ordem já está finalizada."] });
        }
        await GarantirConsumosAsync(ordem, cancellationToken);

        var pendencias = new List<string>();

        // 1. Baixa de insumos (cru real, ou planejado se vazio).
        foreach (var c in ordem.Consumos.Where(x => x.ItemEstoqueId is not null && !x.BaixaRealizada))
        {
            var cruGramas = c.RealCruGramas ?? c.PlanejadoCruGramas;
            var qtdUnidade = ConverterGramasParaUnidade(cruGramas, c.UnidadeEstoque);
            var ok = await _estoque.BaixarPorProducaoAsync(c.ItemEstoqueId!.Value, qtdUnidade, ordem.Id, usuario, $"Produção {ordem.Data:dd/MM/yyyy}", cancellationToken);
            if (ok)
            {
                c.MarcarBaixaRealizada();
            }
            else
            {
                pendencias.Add(c.IngredienteNome);
            }
        }

        // 2. Produto acabado da Casa (quantidade real confirmada, custo 0).
        var produtoAcabado = 0;
        foreach (var f in ordem.Fichas.Where(x => x.Tipo == TipoReceita.Casa && x.Status == StatusFichaProducao.Conferida && x.ItemEstoqueId is not null && (x.QuantidadePacotesReal ?? 0) > 0))
        {
            await _estoque.EntrarPorProducaoAsync(f.ItemEstoqueId!.Value, f.QuantidadePacotesReal!.Value, ordem.Id, usuario, cancellationToken);
            produtoAcabado += f.QuantidadePacotesReal!.Value;
        }

        // 3. Prontidão das personalizadas conferidas (escreve no EntregaItem).
        var personalizadasConferidas = ordem.Fichas
            .Where(x => x.Tipo == TipoReceita.Personalizada && x.Status == StatusFichaProducao.Conferida && x.EntregaItemId is not null)
            .ToList();
        var prontas = 0;
        if (personalizadasConferidas.Count > 0)
        {
            var ids = personalizadasConferidas.Select(f => f.EntregaItemId!.Value).ToList();
            var entregas = await _db.Entregas
                .Where(e => e.Pets.Any(p => p.Itens.Any(i => ids.Contains(i.Id))))
                .Include(e => e.Pets).ThenInclude(p => p.Itens)
                .ToListAsync(cancellationToken);
            var itensPorId = entregas.SelectMany(e => e.Pets).SelectMany(p => p.Itens).Where(i => ids.Contains(i.Id)).ToDictionary(i => i.Id);

            var agora = DateTimeOffset.UtcNow;
            foreach (var f in personalizadasConferidas)
            {
                if (!itensPorId.TryGetValue(f.EntregaItemId!.Value, out var ei))
                {
                    continue;
                }
                var planejado = f.QuantidadePacotes;
                var feito = f.QuantidadePacotesReal ?? planejado;
                var status = feito < planejado ? StatusPreparoPersonalizada.ParcialmentePronta : StatusPreparoPersonalizada.Pronta;
                ei.DefinirPreparo(status, feito, agora, usuario);
                prontas++;
            }
        }

        ordem.Finalizar(request.TudoProduzido, request.Observacoes, usuario, DateTimeOffset.UtcNow);
        await _db.SaveChangesAsync(cancellationToken);

        var consumosDto = ordem.Consumos
            .Select(c => new ResumoConsumoDto(c.IngredienteNome, c.PlanejadoCruGramas, c.RealCruGramas, c.PlanejadoCozidoGramas, c.RealCozidoGramas, c.BaixaRealizada))
            .OrderBy(c => c.IngredienteNome).ToList();

        return new FinalizacaoResultadoDto(
            ordem.Id, ordem.Status.ToString(),
            ordem.Fichas.Count(f => f.Status == StatusFichaProducao.Conferida),
            ordem.Fichas.Count(f => f.Status == StatusFichaProducao.NaoFeita),
            produtoAcabado, prontas, pendencias, consumosDto);
    }

    private async Task<FichaProducao> CarregarFichaAsync(long fichaId, CancellationToken cancellationToken)
    {
        var ordem = await _db.OrdensProducao.Include(o => o.Fichas)
            .FirstOrDefaultAsync(o => o.Fichas.Any(f => f.Id == fichaId), cancellationToken)
            ?? throw new NotFoundException("Ficha de produção", fichaId);
        return ordem.Fichas.First(f => f.Id == fichaId);
    }

    private async Task<OrdemProducao> CarregarOrdemCompletaAsync(long ordemId, CancellationToken cancellationToken)
        => await _db.OrdensProducao
            .Include(o => o.Fichas).ThenInclude(f => f.Ingredientes)
            .Include(o => o.Consumos)
            .FirstOrDefaultAsync(o => o.Id == ordemId, cancellationToken)
            ?? throw new NotFoundException("Ordem de produção", ordemId);

    /// <summary>Materializa o consumo consolidado (uma vez), se ainda não existir.</summary>
    private async Task GarantirConsumosAsync(OrdemProducao ordem, CancellationToken cancellationToken)
    {
        if (ordem.Consumos.Count > 0)
        {
            return;
        }

        var agrupado = ordem.Fichas
            .SelectMany(f => f.Ingredientes)
            .GroupBy(g => g.IngredienteId)
            .Select(grp => new { IngredienteId = grp.Key, Nome = grp.First().IngredienteNome, Coef = grp.First().Coeficiente, Cozido = grp.Sum(x => x.GramasCozidas) })
            .ToList();
        if (agrupado.Count == 0)
        {
            return;
        }

        var ids = agrupado.Select(a => a.IngredienteId).ToList();
        var vinculos = await _db.ItensEstoque.AsNoTracking()
            .Where(x => x.Tipo == TipoItemEstoque.Insumo && x.IngredienteId != null && ids.Contains(x.IngredienteId!.Value))
            .Select(x => new { x.Id, x.Nome, x.UnidadeMedida, IngredienteId = x.IngredienteId!.Value })
            .ToListAsync(cancellationToken);
        var porIngrediente = vinculos.ToDictionary(v => v.IngredienteId);

        foreach (var a in agrupado)
        {
            var cru = a.Coef > 0 ? (int)Math.Round(a.Cozido / a.Coef, MidpointRounding.AwayFromZero) : a.Cozido;
            porIngrediente.TryGetValue(a.IngredienteId, out var v);
            ordem.AdicionarConsumo(ConsumoIngredienteProducao.Criar(
                a.IngredienteId, a.Nome, v?.Id, v?.Nome, a.Coef, v?.UnidadeMedida.ToString(), a.Cozido, cru));
        }
    }

    private static decimal ConverterGramasParaUnidade(decimal gramas, string? unidade)
        => unidade is "Kg" or "Litro" ? gramas / 1000m : gramas;

    // ===================== Mapeamento + consolidação =====================
    private async Task<OrdemProducaoDto> MapOrdemAsync(OrdemProducao ordem, CancellationToken cancellationToken)
    {
        var fichas = ordem.Fichas.Select(f => new FichaProducaoDto(
            f.Id, f.Tipo.ToString(), f.ClienteNome, f.PetNome, f.ReceitaCodigo, f.ReceitaNome, f.DataEntrega,
            f.QuantidadePacotes, f.PesoPacoteGramas, f.QuantidadeTotalGramas, f.Status.ToString(),
            f.QuantidadePacotesReal, f.MotivoNaoFeita,
            f.Ingredientes.Select(g => new FichaIngredienteDto(g.IngredienteId, g.IngredienteNome, g.Categoria, g.GramasCozidas, g.Coeficiente)).ToList()))
            .ToList();

        // Consolidação por ingrediente.
        var agrupado = ordem.Fichas
            .SelectMany(f => f.Ingredientes)
            .GroupBy(g => g.IngredienteId)
            .Select(grp => new
            {
                IngredienteId = grp.Key,
                Nome = grp.First().IngredienteNome,
                Categoria = grp.First().Categoria,
                Coeficiente = grp.First().Coeficiente,
                Cozido = grp.Sum(x => x.GramasCozidas),
            })
            .ToList();

        var ingredienteIds = agrupado.Select(a => a.IngredienteId).ToList();
        var vinculos = await _db.ItensEstoque.AsNoTracking()
            .Where(x => x.Tipo == TipoItemEstoque.Insumo && x.IngredienteId != null && ingredienteIds.Contains(x.IngredienteId!.Value))
            .Select(x => new { x.Id, IngredienteId = x.IngredienteId!.Value })
            .ToListAsync(cancellationToken);
        var vinculoPorIngrediente = vinculos.ToDictionary(v => v.IngredienteId, v => v.Id);

        var consolidado = agrupado
            .Select(a =>
            {
                var temVinculo = vinculoPorIngrediente.TryGetValue(a.IngredienteId, out var itemId);
                var cru = a.Coeficiente > 0 ? (int)Math.Round(a.Cozido / a.Coeficiente, MidpointRounding.AwayFromZero) : a.Cozido;
                return new ConsumoConsolidadoDto(a.IngredienteId, a.Nome, a.Categoria, a.Cozido, cru, temVinculo ? itemId : null, !temVinculo);
            })
            .OrderBy(c => c.Categoria).ThenBy(c => c.IngredienteNome)
            .ToList();

        return new OrdemProducaoDto(ordem.Id, ordem.Data, ordem.Status.ToString(), ordem.Observacoes, fichas, consolidado);
    }
}
