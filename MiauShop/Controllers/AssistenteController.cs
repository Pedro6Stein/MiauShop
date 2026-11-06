using MiauShop.Services;
using MiauShop.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace MiauShop.Controllers;

// Chat "Ajuda" da loja. Como o resto do site, devolve HTML pronto (partial view):
// o JavaScript só coloca na tela e guarda a etapa que o servidor mandou.
[Route("Assistente")]
public class AssistenteController(IAssistenteService assistente) : Controller
{
    public const string Politica = "assistente";

    // GET /Assistente/Inicio — saudação e menu, ao abrir o chat.
    [HttpGet("Inicio")]
    public async Task<IActionResult> Inicio() =>
        PartialView("_RespostaAssistente", await assistente.ResponderAsync(EtapasAssistente.Inicio, null, null));

    // POST /Assistente/Mensagem — etapa e contexto vêm da resposta anterior do próprio servidor.
    [HttpPost("Mensagem")]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting(Politica)]
    public async Task<IActionResult> Mensagem(string? etapa, string? texto, string? contexto) =>
        PartialView("_RespostaAssistente", await assistente.ResponderAsync(etapa, texto, contexto));
}
