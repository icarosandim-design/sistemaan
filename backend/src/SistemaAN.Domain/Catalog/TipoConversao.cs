namespace SistemaAN.Domain.Catalog;

public enum TipoConversao
{
    Perda,
    Ganho,
    SemConversao,
}

/// <summary>Mapeia o enum para os valores de banco/API (snake_case).</summary>
public static class TipoConversaoMap
{
    public static string ToDbValue(this TipoConversao tipo) => tipo switch
    {
        TipoConversao.Perda => "perda",
        TipoConversao.Ganho => "ganho",
        TipoConversao.SemConversao => "sem_conversao",
        _ => throw new ArgumentOutOfRangeException(nameof(tipo)),
    };

    public static TipoConversao FromDbValue(string valor) => valor switch
    {
        "perda" => TipoConversao.Perda,
        "ganho" => TipoConversao.Ganho,
        "sem_conversao" => TipoConversao.SemConversao,
        _ => throw new ArgumentException($"Tipo de conversão inválido: {valor}"),
    };
}
