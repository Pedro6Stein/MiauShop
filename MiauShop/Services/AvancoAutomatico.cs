using MiauShop.Models.Entities;
using MiauShop.Models.Enums;
using Microsoft.Extensions.Options;

namespace MiauShop.Services;

// Seção "AvancoAutomatico" da configuração. Recurso de DEMONSTRAÇÃO do portfólio:
// os status avançam sozinhos com o tempo. Para uso real da loja, Habilitado = false
// (aí só o administrador muda o status, pelo painel).
public class OpcoesAvancoAutomatico
{
    public const string Secao = "AvancoAutomatico";

    public bool Habilitado { get; set; }
    public int MinutosAtePagamento { get; set; } = 1;
    public int MinutosAteEnvio { get; set; } = 5;
    public int MinutosAteEntrega { get; set; } = 15;
}

// Regra única do avanço automático, usada no acompanhamento do cliente e na lista do painel.
public class AvancoAutomatico(IOptions<OpcoesAvancoAutomatico> opcoes)
{
    private readonly OpcoesAvancoAutomatico _opcoes = opcoes.Value;

    // Devolve true se algum status mudou (quem chamou decide salvar).
    public bool Aplicar(Pedido pedido, DateTime agoraUtc)
    {
        if (!_opcoes.Habilitado)
            return false;

        var etapas = new (StatusPedido Status, int Minutos, string Observacao)[]
        {
            (StatusPedido.PagamentoAprovado, _opcoes.MinutosAtePagamento, "Pagamento aprovado (simulação)."),
            (StatusPedido.Enviado, _opcoes.MinutosAteEnvio, "Pedido enviado pelo fornecedor."),
            (StatusPedido.Entregue, _opcoes.MinutosAteEntrega, "Pedido entregue. Aproveite!")
        };

        var mudou = false;

        foreach (var etapa in etapas)
        {
            var quando = pedido.CriadoEm.AddMinutes(etapa.Minutos);

            // PodeAvancarPara impede, por exemplo, avançar um pedido cancelado pelo admin.
            if (agoraUtc >= quando && pedido.PodeAvancarPara(etapa.Status))
            {
                pedido.AlterarStatus(etapa.Status, OrigemStatus.Automatico, etapa.Observacao, quando);
                mudou = true;
            }
        }

        return mudou;
    }
}
