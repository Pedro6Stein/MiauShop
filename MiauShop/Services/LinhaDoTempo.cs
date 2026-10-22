using MiauShop.Models.Entities;
using MiauShop.Models.Enums;

namespace MiauShop.Services;

public enum EstadoEtapa { Feita, Atual, Pendente }

public record EtapaPedido(StatusPedido Status, string Nome, EstadoEtapa Estado, DateTime? Quando);

// Monta, no servidor, a linha do tempo do acompanhamento: quais etapas já aconteceram,
// qual é a atual e quando cada uma aconteceu (horário de Brasília). A tela só desenha.
public static class LinhaDoTempo
{
    // Caminho normal de um pedido. "Cancelado" fica fora: a tela mostra um aviso próprio.
    private static readonly StatusPedido[] Caminho =
    [
        StatusPedido.Recebido,
        StatusPedido.PagamentoAprovado,
        StatusPedido.Enviado,
        StatusPedido.Entregue
    ];

    private static readonly TimeZoneInfo Brasilia = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");

    // Atual = o status em que o pedido está. Feita = já aparece no histórico. Pendente = ainda não aconteceu.
    // (Pedido cancelado não tem etapa atual: o que aconteceu fica "feito" e o resto, pendente.)
    public static List<EtapaPedido> Montar(Pedido pedido) =>
        Caminho.Select(status =>
            {
                var quando = DataDe(pedido, status);
                var estado = status == pedido.Status ? EstadoEtapa.Atual
                    : quando is not null ? EstadoEtapa.Feita
                    : EstadoEtapa.Pendente;

                return new EtapaPedido(status, status.NomeExibicao(), estado, quando);
            })
            .ToList();

    public static DateTime? DataDe(Pedido pedido, StatusPedido status)
    {
        var registro = pedido.Historico.FirstOrDefault(h => h.Status == status);
        return registro is null ? null : ParaBrasilia(registro.CriadoEm);
    }

    public static DateTime ParaBrasilia(DateTime utc) =>
        TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), Brasilia);
}
