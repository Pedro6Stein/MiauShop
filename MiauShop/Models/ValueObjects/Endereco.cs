namespace MiauShop.Models.ValueObjects;

// Value object: não tem Id nem tabela própria.
// As colunas ficam embutidas na tabela de quem o usa (ex.: Pedidos).
public class Endereco
{
    public string Cep { get; set; } = string.Empty;
    public string Logradouro { get; set; } = string.Empty;
    public string Numero { get; set; } = string.Empty;
    public string? Complemento { get; set; }
    public string Bairro { get; set; } = string.Empty;
    public string Cidade { get; set; } = string.Empty;
    public string Uf { get; set; } = string.Empty;
}
