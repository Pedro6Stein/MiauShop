using System.ComponentModel.DataAnnotations;
using MiauShop.Models.ValueObjects;

namespace MiauShop.Models.ViewModels;

// Formulário "Seus dados". As regras de validação moram aqui (servidor);
// o aviso no navegador é gerado automaticamente a partir destes mesmos atributos.
public class DadosCheckout
{
    [Display(Name = "Nome completo")]
    [Required(ErrorMessage = "Informe seu nome.")]
    [StringLength(120, MinimumLength = 3, ErrorMessage = "O nome precisa ter entre 3 e 120 letras.")]
    public string Nome { get; set; } = string.Empty;

    [Display(Name = "E-mail")]
    [Required(ErrorMessage = "Informe seu e-mail.")]
    [EmailAddress(ErrorMessage = "Esse e-mail não parece válido.")]
    [StringLength(160)]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Celular")]
    [Required(ErrorMessage = "Informe um telefone para contato.")]
    [RegularExpression(@"^\(?\d{2}\)?\s?9?\d{4}-?\d{4}$", ErrorMessage = "Use o formato (11) 91234-5678.")]
    public string Telefone { get; set; } = string.Empty;

    [Display(Name = "CEP")]
    [Required(ErrorMessage = "Informe o CEP.")]
    [RegularExpression(@"^\d{5}-?\d{3}$", ErrorMessage = "O CEP tem 8 números.")]
    public string Cep { get; set; } = string.Empty;

    [Display(Name = "Rua")]
    [Required(ErrorMessage = "Informe a rua.")]
    [StringLength(150)]
    public string Logradouro { get; set; } = string.Empty;

    [Display(Name = "Número")]
    [Required(ErrorMessage = "Informe o número.")]
    [StringLength(20)]
    public string Numero { get; set; } = string.Empty;

    [Display(Name = "Complemento")]
    [StringLength(80)]
    public string? Complemento { get; set; }

    [Display(Name = "Bairro")]
    [Required(ErrorMessage = "Informe o bairro.")]
    [StringLength(80)]
    public string Bairro { get; set; } = string.Empty;

    // Cidade e UF não são digitadas: o servidor preenche a partir do CEP (CheckoutController).
    [Display(Name = "Cidade")]
    [StringLength(80)]
    public string Cidade { get; set; } = string.Empty;

    [Display(Name = "UF")]
    [StringLength(2)]
    public string Uf { get; set; } = string.Empty;

    public Endereco ParaEndereco()
    {
        var cep = new string(Cep.Where(char.IsDigit).ToArray());

        return new Endereco
        {
            Cep = $"{cep[..5]}-{cep[5..]}",
            Logradouro = Logradouro.Trim(),
            Numero = Numero.Trim(),
            Complemento = string.IsNullOrWhiteSpace(Complemento) ? null : Complemento.Trim(),
            Bairro = Bairro.Trim(),
            Cidade = Cidade.Trim(),
            Uf = Uf.Trim().ToUpperInvariant()
        };
    }
}
