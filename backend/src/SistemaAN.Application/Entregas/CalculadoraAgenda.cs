namespace SistemaAN.Application.Entregas;

/// <summary>
/// Serviço de cálculo da agenda de entregas futuras.
///
/// Regra: as entregas futuras são geradas a partir de (data inicial + ciclo),
/// independentemente da confirmação de entregas anteriores. A confirmação de
/// uma entrega apenas muda o status daquela entrega — NÃO cria a próxima.
///
/// Nesta fase é apenas o cálculo (sem persistir entregas). Será reutilizado por
/// Pets, Plano Alimentar, Entregas, Calendário e Planejamento de Produção.
/// </summary>
public static class CalculadoraAgenda
{
    /// <summary>
    /// Gera as datas de entrega de <paramref name="inicial"/> em diante, somando
    /// <paramref name="diasCiclo"/>, até o limite do horizonte de planejamento.
    /// </summary>
    public static IReadOnlyList<DateOnly> Gerar(DateOnly inicial, int diasCiclo, int horizonteDias)
    {
        var datas = new List<DateOnly>();
        if (diasCiclo <= 0 || horizonteDias < 0)
        {
            return datas;
        }

        var limite = inicial.AddDays(horizonteDias);
        for (var data = inicial; data <= limite; data = data.AddDays(diasCiclo))
        {
            datas.Add(data);
        }

        return datas;
    }
}
