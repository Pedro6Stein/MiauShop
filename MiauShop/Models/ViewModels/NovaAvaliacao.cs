using System.ComponentModel.DataAnnotations;
using MiauShop.Models.Entities;

namespace MiauShop.Models.ViewModels;

// Formulário "Avaliar este produto" da página do produto.
public class NovaAvaliacao
{
    [Display(Name = "Seu nome")]
    [Required(ErrorMessage = "Informe seu nome.")]
    [StringLength(AvaliacaoProduto.TamanhoMaximoNome, MinimumLength = 2, ErrorMessage = "Use entre 2 e 60 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [Display(Name = "Nota")]
    [Range(AvaliacaoProduto.NotaMinima, AvaliacaoProduto.NotaMaxima, ErrorMessage = "Escolha de 1 a 5 estrelas.")]
    public int Nota { get; set; }

    [Display(Name = "Comentário")]
    [Required(ErrorMessage = "Conte o que achou do produto.")]
    [StringLength(AvaliacaoProduto.TamanhoMaximoComentario, MinimumLength = 10, ErrorMessage = "Use entre 10 e 600 caracteres.")]
    public string Comentario { get; set; } = string.Empty;

    // "Pote de mel" contra robôs: campo escondido que pessoas não veem nem preenchem.
    // Se vier preenchido, a avaliação é descartada em silêncio.
    public string? Site { get; set; }
}
