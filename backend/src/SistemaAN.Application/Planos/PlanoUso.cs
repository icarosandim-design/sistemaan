using Microsoft.EntityFrameworkCore;
using SistemaAN.Application.Common.Interfaces;

namespace SistemaAN.Application.Planos;

/// <summary>
/// Consultas de "uso por plano alimentar vigente", usadas para bloquear a
/// inativação de itens que estão em algum plano ativo.
/// </summary>
public static class PlanoUso
{
    public static Task<bool> ReceitaEmPlanoAtivoAsync(IApplicationDbContext db, long receitaId, CancellationToken ct)
    {
        var planosAtivos = db.PlanosAlimentares.Where(p => p.Ativo).Select(p => p.Id);
        return db.PlanoItensReceita.AnyAsync(
            pi => pi.ReceitaId == receitaId && planosAtivos.Contains(pi.PlanoAlimentarId), ct);
    }

    public static Task<bool> TamanhoPacoteEmPlanoAtivoAsync(IApplicationDbContext db, long tamanhoId, CancellationToken ct)
    {
        var planosAtivos = db.PlanosAlimentares.Where(p => p.Ativo).Select(p => p.Id);
        var itensAtivos = db.PlanoItensReceita
            .Where(pi => planosAtivos.Contains(pi.PlanoAlimentarId))
            .Select(pi => pi.Id);
        return db.PlanoItemPacotes.AnyAsync(
            pp => pp.TamanhoPacoteId == tamanhoId && itensAtivos.Contains(pp.PlanoItemReceitaId), ct);
    }

    public static Task<bool> FrequenciaEmPlanoAtivoAsync(IApplicationDbContext db, long frequenciaId, CancellationToken ct)
        => db.Clientes.AnyAsync(c => c.Ativo && c.FrequenciaEntregaId == frequenciaId, ct);

    public static Task<bool> IngredienteEmPlanoAtivoAsync(IApplicationDbContext db, long ingredienteId, CancellationToken ct)
    {
        var planosAtivos = db.PlanosAlimentares.Where(p => p.Ativo).Select(p => p.Id);
        var receitasEmPlano = db.PlanoItensReceita
            .Where(pi => planosAtivos.Contains(pi.PlanoAlimentarId))
            .Select(pi => pi.ReceitaId);
        return db.ItensReceita.AnyAsync(
            it => it.IngredienteId == ingredienteId && receitasEmPlano.Contains(it.ReceitaId), ct);
    }
}
