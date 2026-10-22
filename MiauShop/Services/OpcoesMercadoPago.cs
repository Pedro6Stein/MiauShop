namespace MiauShop.Services;

// Seção "MercadoPago" da configuração. A Public Key de TESTE fica em user secrets (local)
// ou variável de ambiente (Render) — nunca no código nem no GitHub.
// Só a Public Key é usada: o MiauShop nunca chama a API de cobrança.
public class OpcoesMercadoPago
{
    public const string Secao = "MercadoPago";

    public string? PublicKey { get; set; }

    public bool Configurado => !string.IsNullOrWhiteSpace(PublicKey);
}
