namespace MiauShop.Models.Entities;

// Classe abstrata: não existe "uma EntidadeBase" sozinha, só as filhas.
// As datas são preenchidas automaticamente pelo AppDbContext ao salvar.
public abstract class EntidadeBase
{
    public int Id { get; set; }
    public DateTime CriadoEm { get; set; }
    public DateTime AtualizadoEm { get; set; }
}
