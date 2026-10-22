using MiauShop.Models.Enums;
using MiauShop.Models.ValueObjects;

namespace MiauShop.Models.Entities;

// O Pedido protege as próprias regras: ninguém de fora altera Status, itens
// ou totais diretamente; tudo passa pelos métodos abaixo.
public class Pedido : EntidadeBase
{
    // Para cada status, para quais ele pode ir.
    private static readonly Dictionary<StatusPedido, StatusPedido[]> TransicoesValidas = new()
    {
        [StatusPedido.Recebido] = [StatusPedido.PagamentoAprovado, StatusPedido.Cancelado],
        [StatusPedido.PagamentoAprovado] = [StatusPedido.Enviado, StatusPedido.Cancelado],
        [StatusPedido.Enviado] = [StatusPedido.Entregue],
        [StatusPedido.Entregue] = [],
        [StatusPedido.Cancelado] = []
    };

    // Listas privadas: de fora só dá para LER (IReadOnlyCollection).
    private readonly List<ItemPedido> _itens = new();
    private readonly List<HistoricoStatusPedido> _historico = new();

    // Construtor vazio usado pelo Entity Framework ao ler do banco.
    private Pedido() { }

    // O número do pedido mostrado ao cliente é o próprio Id (ex.: "Pedido #123").

    // Vazio por enquanto (compra como visitante); preparado para contas de cliente.
    public string? UsuarioId { get; private set; }
    public Usuario? Usuario { get; private set; }

    public string ClienteNome { get; private set; } = string.Empty;
    public string ClienteEmail { get; private set; } = string.Empty;
    public string ClienteTelefone { get; private set; } = string.Empty;
    public Endereco EnderecoEntrega { get; private set; } = new();

    public FormaPagamento FormaPagamento { get; private set; }
    public StatusPedido Status { get; private set; }

    public decimal Subtotal { get; private set; }
    public decimal Frete { get; private set; }
    public decimal Total { get; private set; }

    public IReadOnlyCollection<ItemPedido> Itens => _itens;
    public IReadOnlyCollection<HistoricoStatusPedido> Historico => _historico;

    // Única forma de criar um pedido novo: já nasce "Recebido" e com histórico.
    public static Pedido Criar(
        string clienteNome,
        string clienteEmail,
        string clienteTelefone,
        Endereco enderecoEntrega,
        FormaPagamento formaPagamento,
        string? usuarioId = null)
    {
        var pedido = new Pedido
        {
            ClienteNome = clienteNome,
            ClienteEmail = clienteEmail.Trim().ToLowerInvariant(),
            ClienteTelefone = clienteTelefone,
            EnderecoEntrega = enderecoEntrega,
            FormaPagamento = formaPagamento,
            UsuarioId = usuarioId,
            Status = StatusPedido.Recebido
        };

        pedido._historico.Add(new HistoricoStatusPedido(
            StatusPedido.Recebido, OrigemStatus.Sistema, "Pedido recebido pela loja."));

        return pedido;
    }

    public void AdicionarItem(ItemPedido item)
    {
        if (Status != StatusPedido.Recebido)
            throw new InvalidOperationException("Só é possível adicionar itens a um pedido recém-criado.");

        _itens.Add(item);
        RecalcularTotais();
    }

    public void DefinirFrete(decimal valor)
    {
        if (valor < 0)
            throw new ArgumentOutOfRangeException(nameof(valor), "O frete não pode ser negativo.");

        Frete = valor;
        RecalcularTotais();
    }

    public bool PodeAvancarPara(StatusPedido novoStatus) =>
        TransicoesValidas[Status].Contains(novoStatus);

    // "quando" permite registrar o horário em que a mudança aconteceu
    // (usado no avanço automático, para a linha do tempo não ficar com tudo no mesmo minuto).
    public void AlterarStatus(StatusPedido novoStatus, OrigemStatus origem, string? observacao = null, DateTime? quando = null)
    {
        if (!PodeAvancarPara(novoStatus))
            throw new InvalidOperationException($"Não é possível mudar o pedido de {Status} para {novoStatus}.");

        Status = novoStatus;
        _historico.Add(new HistoricoStatusPedido(novoStatus, origem, observacao)
        {
            CriadoEm = quando ?? default
        });
    }

    private void RecalcularTotais()
    {
        Subtotal = _itens.Sum(i => i.Subtotal());
        Total = Subtotal + Frete;
    }
}
