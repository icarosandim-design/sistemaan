using SistemaAN.Domain.Clientes;
using SistemaAN.Domain.Common;

namespace SistemaAN.Domain.Entregas;

/// <summary>
/// Entrega de um cliente em uma data (demanda oficial). Guarda snapshot do
/// cliente e dos pets/itens no momento da geração. Transições de status são
/// validadas na camada de Aplicação; aqui aplicamos a mudança e o histórico.
/// </summary>
public class Entrega : AuditableEntity
{
    private readonly List<EntregaPet> _pets = [];
    private readonly List<EntregaHistorico> _historico = [];

    private Entrega() { } // EF Core

    private Entrega(long clienteId, DateOnly dataPrevista, DadosSnapshotEntrega s)
    {
        ClienteId = clienteId;
        DataPrevista = dataPrevista;
        Status = EntregaStatus.Programada;
        ClienteNome = s.ClienteNome;
        Telefone = s.Telefone;
        Rua = s.Rua;
        Numero = s.Numero;
        Complemento = s.Complemento;
        Cep = s.Cep;
        Bairro = s.Bairro;
        Cidade = s.Cidade;
        Estado = s.Estado;
        FrequenciaNome = s.FrequenciaNome;
        DiasCiclo = s.DiasCiclo;
        PreferenciaHorario = s.PreferenciaHorario;
    }

    public long ClienteId { get; private set; }
    public DateOnly DataPrevista { get; private set; }
    public EntregaStatus Status { get; private set; }

    // ----- Snapshot do cliente -----
    public string ClienteNome { get; private set; } = string.Empty;
    public string? Telefone { get; private set; }
    public string? Rua { get; private set; }
    public string? Numero { get; private set; }
    public string? Complemento { get; private set; }
    public string? Cep { get; private set; }
    public string? Bairro { get; private set; }
    public string? Cidade { get; private set; }
    public string? Estado { get; private set; }
    public string FrequenciaNome { get; private set; } = string.Empty;
    public int DiasCiclo { get; private set; }
    public PreferenciaHorarioEntrega PreferenciaHorario { get; private set; }

    public string? ObservacoesInternas { get; private set; }
    public string? ObservacoesEntregador { get; private set; }
    public long? EntregadorId { get; private set; }

    // ----- Tratativas -----
    public string? MotivoNaoEntrega { get; private set; }
    public string? MotivoReagendamento { get; private set; }
    public long? ReagendadaDeId { get; private set; }
    public long? ReagendadaParaId { get; private set; }
    public DateTimeOffset? ReagendadaEm { get; private set; }
    public string? MotivoCancelamento { get; private set; }
    public DateTimeOffset? CanceladaEm { get; private set; }

    /// <summary>Pedido PJ que originou esta entrega (nulo para entregas PF recorrentes).</summary>
    public long? PedidoId { get; private set; }

    /// <summary>Indica que o produto acabado já foi baixado do estoque (idempotência da entrega concluída).</summary>
    public bool EstoqueBaixado { get; private set; }

    public IReadOnlyCollection<EntregaPet> Pets => _pets.AsReadOnly();
    public IReadOnlyCollection<EntregaHistorico> Historico => _historico.AsReadOnly();

    public static Entrega Criar(long clienteId, DateOnly dataPrevista, DadosSnapshotEntrega snapshot)
        => new(clienteId, dataPrevista, snapshot);

    public void AdicionarPet(EntregaPet pet) => _pets.Add(pet);

    public void VincularPedido(long pedidoId) => PedidoId = pedidoId;

    public void LimparPets() => _pets.Clear();

    /// <summary>Ajusta a data prevista (usado ao editar um Pedido PJ confirmado ainda Programado).</summary>
    public void AlterarDataPrevista(DateOnly data) => DataPrevista = data;

    public void MarcarEstoqueBaixado() => EstoqueBaixado = true;

    /// <summary>Define a observação interna da entrega (ex.: dados da venda avulsa).</summary>
    public void DefinirObservacoesInternas(string? texto)
        => ObservacoesInternas = string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();

    public void RegistrarHistorico(string usuario, string evento, EntregaStatus? de = null, EntregaStatus? para = null)
        => _historico.Add(EntregaHistorico.Criar(usuario, evento, de, para));

    public void DefinirOrigemReagendamento(long entregaOriginalId) => ReagendadaDeId = entregaOriginalId;

    // ----- Transições (validação de origem fica no serviço) -----
    public void MudarStatus(EntregaStatus novo, string usuario, string evento)
    {
        var de = Status;
        Status = novo;
        RegistrarHistorico(usuario, evento, de, novo);
    }

    public void MarcarNaoEntregue(string motivo, string usuario)
    {
        MotivoNaoEntrega = motivo.Trim();
        MudarStatus(EntregaStatus.NaoEntregue, usuario, $"Marcada como Não entregue — {motivo.Trim()}");
    }

    public void Reagendar(string motivo, string usuario, long? reagendadaParaId)
    {
        MotivoReagendamento = motivo.Trim();
        ReagendadaEm = DateTimeOffset.UtcNow;
        ReagendadaParaId = reagendadaParaId;
        MudarStatus(EntregaStatus.Reagendada, usuario, $"Reagendada — {motivo.Trim()}");
    }

    public void Cancelar(string motivo, string usuario)
    {
        MotivoCancelamento = motivo.Trim();
        CanceladaEm = DateTimeOffset.UtcNow;
        MudarStatus(EntregaStatus.Cancelada, usuario, $"Cancelada — {motivo.Trim()}");
    }
}

public sealed record DadosSnapshotEntrega(
    string ClienteNome,
    string? Telefone,
    string? Rua,
    string? Numero,
    string? Complemento,
    string? Cep,
    string? Bairro,
    string? Cidade,
    string? Estado,
    string FrequenciaNome,
    int DiasCiclo,
    PreferenciaHorarioEntrega PreferenciaHorario = PreferenciaHorarioEntrega.HorarioComercial);
