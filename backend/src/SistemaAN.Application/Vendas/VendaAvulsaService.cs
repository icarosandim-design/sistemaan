using System.Globalization;
using Microsoft.EntityFrameworkCore;
using SistemaAN.Application.Common.Exceptions;
using SistemaAN.Application.Common.Interfaces;
using SistemaAN.Domain.Clientes;
using SistemaAN.Domain.Entregas;
using SistemaAN.Domain.Receitas;

namespace SistemaAN.Application.Vendas;

/// <summary>
/// Venda avulsa PF: cliente PF + pet + itens de Receita da Casa → uma Entrega
/// Programada única (sem assinatura/recorrência). Reaproveita a Entrega para
/// integrar com Entregas/Estoque/Produção. Valor e forma de pagamento são
/// apenas informativos (registrados na observação interna; sem financeiro).
/// </summary>
public sealed class VendaAvulsaService : IVendaAvulsaService
{
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

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

        var (itens, totalGramas, totalPacotes) = await MontarItensAsync(request.Itens, cancellationToken);

        var snapshot = new DadosSnapshotEntrega(
            cliente.Nome,
            cliente.Telefone,
            cliente.Rua,
            cliente.Numero,
            cliente.Complemento,
            cliente.Cep,
            cliente.Bairro,
            cliente.Cidade,
            cliente.Estado,
            "Venda avulsa",
            0,
            cliente.PreferenciaHorario);

        var entrega = Entrega.Criar(cliente.Id, request.DataEntrega, snapshot);
        entrega.AdicionarPet(EntregaPet.Criar(pet.Id, pet.Nome, TipoReceita.Casa, null, totalGramas, itens));
        entrega.DefinirObservacoesInternas(ComporObservacao(request));
        entrega.RegistrarHistorico(usuario, "Entrega gerada a partir de Venda avulsa PF", null, EntregaStatus.Programada);

        _db.Entregas.Add(entrega);
        await _db.SaveChangesAsync(cancellationToken);

        return new VendaAvulsaResultadoDto(entrega.Id, entrega.DataPrevista, entrega.Status.ToString(), totalPacotes);
    }

    private async Task<(List<EntregaItem> Itens, int TotalGramas, int TotalPacotes)> MontarItensAsync(
        IReadOnlyList<SalvarVendaAvulsaItemRequest> reqs, CancellationToken ct)
    {
        if (reqs is null || reqs.Count == 0)
        {
            throw Erro("itens", "Inclua ao menos um item (receita + tamanho + quantidade).");
        }

        var receitaIds = reqs.Select(r => r.ReceitaId).Distinct().ToList();
        var tamanhoIds = reqs.Select(r => r.TamanhoPacoteId).Distinct().ToList();
        var receitas = await _db.Receitas.Where(r => receitaIds.Contains(r.Id)).ToDictionaryAsync(r => r.Id, ct);
        var tamanhos = await _db.TamanhosPacote.Where(t => tamanhoIds.Contains(t.Id)).ToDictionaryAsync(t => t.Id, ct);

        var itens = new List<EntregaItem>();
        var erros = new Dictionary<string, string[]>();
        for (var idx = 0; idx < reqs.Count; idx++)
        {
            var r = reqs[idx];
            if (!receitas.TryGetValue(r.ReceitaId, out var rec) || rec.Tipo != TipoReceita.Casa)
            {
                erros[$"itens[{idx}].receitaId"] = ["Selecione uma Receita da Casa válida."];
                continue;
            }
            if (!tamanhos.TryGetValue(r.TamanhoPacoteId, out var tam))
            {
                erros[$"itens[{idx}].tamanhoPacoteId"] = ["Selecione um tamanho de pacote válido."];
                continue;
            }
            if (r.Quantidade <= 0)
            {
                erros[$"itens[{idx}].quantidade"] = ["A quantidade deve ser maior que zero."];
                continue;
            }

            itens.Add(EntregaItem.CriarCasa(
                rec.Id, rec.Codigo, rec.Nome,
                r.Quantidade * tam.PesoGramas,
                new[] { EntregaItemPacote.Criar(tam.Nome, tam.PesoGramas, r.Quantidade) }));
        }
        if (erros.Count > 0)
        {
            throw new ValidationException(erros);
        }

        var totalGramas = reqs.Sum(r => r.Quantidade * tamanhos[r.TamanhoPacoteId].PesoGramas);
        var totalPacotes = reqs.Sum(r => r.Quantidade);
        return (itens, totalGramas, totalPacotes);
    }

    private static string ComporObservacao(SalvarVendaAvulsaRequest req)
    {
        var partes = new List<string> { "Venda avulsa PF" };
        if (req.Valor is > 0m)
        {
            partes.Add($"Valor: {req.Valor.Value.ToString("C2", PtBr)}");
        }
        if (!string.IsNullOrWhiteSpace(req.FormaPagamento))
        {
            partes.Add($"Pagamento: {req.FormaPagamento.Trim()}");
        }
        if (!string.IsNullOrWhiteSpace(req.Observacoes))
        {
            partes.Add(req.Observacoes.Trim());
        }
        return string.Join(" · ", partes);
    }

    private static ValidationException Erro(string campo, string msg)
        => new(new Dictionary<string, string[]> { [campo] = [msg] });
}
