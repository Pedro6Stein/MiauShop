using System.ComponentModel.DataAnnotations;
using MiauShop.Models.Entities;

namespace MiauShop.Areas.Admin.ViewModels;

public class AssistentePainelViewModel
{
    public List<PerguntaFrequente> Perguntas { get; init; } = [];
    public List<PerguntaSemResposta> SemResposta { get; init; } = [];
}

public class PerguntaFormulario
{
    public int Id { get; set; }

    // Quando a resposta nasce de uma dúvida sem resposta, ela sai da lista ao salvar.
    public int? SemRespostaId { get; set; }

    [Display(Name = "Pergunta (aparece na lista do assistente)")]
    [Required(ErrorMessage = "Escreva a pergunta.")]
    [StringLength(PerguntaFrequente.TamanhoMaximoPergunta, MinimumLength = 5, ErrorMessage = "Use entre 5 e 150 caracteres.")]
    public string Pergunta { get; set; } = string.Empty;

    [Display(Name = "Resposta")]
    [Required(ErrorMessage = "Escreva a resposta.")]
    [StringLength(PerguntaFrequente.TamanhoMaximoResposta, MinimumLength = 5, ErrorMessage = "Use entre 5 e 1000 caracteres.")]
    public string Resposta { get; set; } = string.Empty;

    [Display(Name = "Palavras-chave (separadas por vírgula)")]
    [StringLength(PerguntaFrequente.TamanhoMaximoPalavrasChave)]
    public string? PalavrasChave { get; set; }

    [Display(Name = "Ordem na lista")]
    [Range(0, 99, ErrorMessage = "Use um número de 0 a 99.")]
    public int Ordem { get; set; }

    [Display(Name = "Ativa (o assistente usa esta resposta)")]
    public bool Ativa { get; set; } = true;
}
