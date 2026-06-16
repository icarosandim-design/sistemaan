using Microsoft.EntityFrameworkCore;
using SistemaAN.Application.Common.Exceptions;
using SistemaAN.Application.Common.Interfaces;
using SistemaAN.Domain.Clientes;
using SistemaAN.Domain.Entregas;
using SistemaAN.Domain.Produtos;
using SistemaAN.Domain.Receitas;
using SistemaAN.Domain.Vendas;

namespace SistemaAN.Application.Vendas;

/// <summary>
/// Venda avulsa PF: cliente PF + pet + Produtos (Receita da Casa). Cria a VendaAvulsa
/// (cabeçalho comercial com valor estruturado e forma de pagamento) e gera a Entrega
/// Programada correspondente (operação/logística). O preço vem do produto/venda — a
/// Entrega não define preço.
/// </summary>
public sealed class VendaAvulsaService : IVendaAvulsaService
{
    private readonly IApplicationDbContext _db;

    public VendaAvulsaService(IApplicationDbContext db) => _db = db;

    public async Task<VendaAvulsaResultadoDto> CriarAsync(SalvarVendaAvulsaRequest request, string usuario, CancellationToken cancellationToken = default)
    {
        var cliente = await _db.Clientes.FirstOrDefaultAsync(c => c.Id == request.ClienteId, cancellationToken)
            ?? throw new NotFoundException("Cliente", request.ClienteId);
        if (cliente.Natureza != NaturezaCliente.PessoaFisica)
        {
            throw Erro("clienteId", "A venda avulsa deve ser de um Cliente Pessoa Física.");
        }
        if (!cliente.Ativo)
        {
            throw Erro("clienteId", "Cliente inativo não pode receber venda avulsa.");
        }

        var pet = await _db.Pets.FirstOrDefaultAsync(p => p.Id == request.PetId, cancellationToken)
            ?? throw Erro("petId", "Selecione um pet para a venda.");
        if (pet.ClienteId != cliente.Id)
        {
            throw Erro("petId", "O pet selecionado não pertence a este cliente.");
        }

        if (request.DataEntrega == default)
        {
            throw Erro("dataEntrega", "Informe a data de entrega.");
        }

        var resolvidos = await ResolverItensAsync(request.Itens, cancellationToken);

        // ----- Entrega (operação) -----
        var snapshot = new DadosSnapshotEntrega(
            cliente.Nome, cliente.Telefone, cliente.Rua, cliente.Numero, cliente.Complemento,
            cliente.Cep, cliente.Bairro, cliente.Cidade, cliente.Estado, "Venda avulsa", 0, cliente.PreferenciaHorario);

        var entregaItens = resolvidos.Select(x => EntregaItem.CriarCasa(
            x.Receita.Id, x.Receita.Codigo, x.Receita.Nome,
            x.Quantidade * x.PesoGramas,
            new[] { EntregaItemPacote.Criar(x.TamanhoNome, x.PesoGramas, x.Quantidade) },
            x.ProdutoId)).ToList();
        var totalGramas = resolvidos.Sum(x => x.Quantidade * x.PesoGramas);
        var totalPacotes = resolvidos.Sum(x => x.Quantidade);

        var entrega = Entrega.Criar(cliente.Id, request.DataEntrega, snapshot);
        entrega.AdicionarPet(EntregaPet.Criar(pet.Id, pet.Nome, TipoReceita.Casa, null, totalGramas, entregaItens));
        if (!string.IsNullOrWhiteSpace(request.Observacoes))
        {
            entrega.DefinirObservacoesInternas($"Venda avulsa PF · {request.Observacoes!.Trim()}");
        }
        else
        {
            entrega.DefinirObservacoesInternas("Venda avulsa PF");
        }
        entrega.RegistrarHistorico(usuario, "Entrega gerada a partir de Venda avulsa PF", null, EntregaStatus.Programada);
        _db.Entregas.Add(entrega);
        await _db.SaveChangesAsync(cancellationToken);

        // ----- VendaAvulsa (comercial) -----
        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);
        var venda = VendaAvulsa.Criar(cliente.Id, pet.Id, hoje, request.FormaPagamento, request.Observacoes);
        foreach (var x in resolvidos)
        {
            venda.AdicionarItem(VendaAvulsaItem.Criar(x.ProdutoId, x.Quantidade, x.Preco, null));
        }
        venda.RecalcularTotal();
        venda.VincularEntrega(entrega.Id);
        _db.VendasAvulsas.Add(venda);
        await _db.SaveChangesAsync(cancellationToken);

        return new VendaAvulsaResultadoDto(venda.Id, entrega.Id, entrega.DataPrevista, entrega.Status.ToString(), venda.ValorTotal, totalPacotes);
    }

    private sealed record ItemResolvido(long ProdutoId, Receita Receita, string TamanhoNome, int PesoGramas, int Quantidade, decimal Preco);

    private async Task<List<ItemResolvido>> ResolverItensAsync(IReadOnlyList<SalvarVendaAvulsaItemRequest> reqs, CancellationToken ct)
    {
        if (reqs is null || reqs.Count == 0)
        {
            throw Erro("itens", "Inclua ao menos um item (produto + quantidade).");
        }

        var produtoIds = reqs.Select(r => r.ProdutoId).Distinct().ToList();
        var produtos = await _db.Produtos.Where(p => produtoIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id, ct);
        var recIds = produtos.Values.Where(p => p.ReceitaCasaId != null).Select(p => p.ReceitaCasaId!.Value).Distinct().ToList();
        var tamIds = produtos.Values.Where(p => p.TamanhoPacoteId != null).Select(p => p.TamanhoPacoteId!.Value).Distinct().ToList();
        var receitas = await _db.Receitas.Where(r => recIds.Contains(r.Id)).ToDictionaryAsync(r => r.Id, ct);
        var tamanhos = await _db.TamanhosPacote.Where(t => tamIds.Contains(t.Id)).ToDictionaryAsync(t => t.Id, ct);

        var lista = new List<ItemResolvido>();
        var erros = new Dictionary<string, string[]>();
        for (var idx = 0; idx < reqs.Count; idx++)
        {
            var r = reqs[idx];
            if (!produtos.TryGetValue(r.ProdutoId, out var prod) || prod.Tipo != TipoProduto.ReceitaDaCasa
                || prod.ReceitaCasaId is not { } prc || prod.TamanhoPacoteId is not { } ptam)
            {
                erros[$"itens[{idx}].produtoId"] = ["Selecione um produto de Receita da Casa válido."];
                continue;
            }
            if (!receitas.TryGetValue(prc, out var rec) || !tamanhos.TryGetValue(ptam, out var tam))
            {
                erros[$"itens[{idx}].produtoId"] = ["Produto sem receita/tamanho válidos."];
                continue;
            }
            if (r.Quantidade <= 0)
            {
                erros[$"itens[{idx}].quantidade"] = ["A quantidade deve ser maior que zero."];
                continue;
            }
            var preco = r.PrecoUnitario ?? prod.PrecoVendaAvulsaPF;
            if (preco < 0m)
            {
                erros[$"itens[{idx}].precoUnitario"] = ["O preço unitário não pode ser negativo."];
                continue;
            }
            lista.Add(new ItemResolvido(prod.Id, rec, tam.Nome, tam.PesoGramas, r.Quantidade, preco));
        }
        if (erros.Count > 0)
        {
            throw new ValidationException(erros);
        }
        return lista;
    }

    private static ValidationException Erro(string campo, string msg)
        => new(new Dictionary<string, string[]> { [campo] = [msg] });
}
