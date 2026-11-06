using MiauShop.Models.Entities;

namespace MiauShop.Models;

// Números das avaliações APROVADAS de um produto, calculados no banco.
// PorNota[0] = quantidade de notas 1 ... PorNota[4] = quantidade de notas 5.
public record ResumoAvaliacoes(int[] PorNota, List<AvaliacaoProduto> Recentes)
{
    public static ResumoAvaliacoes Vazio => new(new int[AvaliacaoProduto.NotaMaxima], []);

    public int Total => PorNota.Sum();

    public double Media => Total == 0
        ? 0
        : PorNota.Select((quantidade, indice) => quantidade * (indice + 1)).Sum() / (double)Total;

    // Para desenhar as estrelas: arredonda para a meia estrela mais próxima (4,3 -> 4,5).
    public double MediaEmMeiaEstrela => Math.Round(Media * 2, MidpointRounding.AwayFromZero) / 2;

    // Largura da barra de cada nota (0 a 100), proporcional ao total.
    public int Percentual(int nota) =>
        Total == 0 ? 0 : (int)Math.Round(PorNota[nota - 1] * 100.0 / Total);
}
