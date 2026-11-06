using MiauShop.Areas.Admin.ViewModels;
using MiauShop.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MiauShop.Areas.Admin.Controllers;

// Perguntas e respostas do assistente da loja, e as dúvidas que ele ainda não soube responder.
public class AtendimentoController(IAssistenteService assistente) : AdminControllerBase
{
    // GET /Admin/Atendimento
    public async Task<IActionResult> Index() => View(new AssistentePainelViewModel
    {
        Perguntas = await assistente.ListarPerguntasAsync(),
        SemResposta = await assistente.ListarSemRespostaAsync()
    });

    // GET /Admin/Atendimento/Nova?semResposta=3 — já vem com a dúvida do cliente no campo "Pergunta".
    public async Task<IActionResult> Nova(int? semResposta)
    {
        var formulario = new PerguntaFormulario { Ordem = (await assistente.ListarPerguntasAsync()).Count + 1 };

        if (semResposta is int id && await assistente.ObterSemRespostaAsync(id) is { } duvida)
        {
            formulario.SemRespostaId = duvida.Id;
            formulario.Pergunta = duvida.Texto;
        }

        return View("Formulario", formulario);
    }

    public async Task<IActionResult> Editar(int id) =>
        await assistente.ObterPerguntaAsync(id) is { } pergunta
            ? View("Formulario", new PerguntaFormulario
            {
                Id = pergunta.Id,
                Pergunta = pergunta.Pergunta,
                Resposta = pergunta.Resposta,
                PalavrasChave = pergunta.PalavrasChave,
                Ordem = pergunta.Ordem,
                Ativa = pergunta.Ativa
            })
            : NotFound();

    [HttpPost]
    public async Task<IActionResult> Salvar(PerguntaFormulario formulario)
    {
        if (!ModelState.IsValid)
            return View("Formulario", formulario);

        await assistente.SalvarPerguntaAsync(formulario.Id, formulario.Pergunta, formulario.Resposta,
            formulario.PalavrasChave, formulario.Ordem, formulario.Ativa);

        if (formulario.SemRespostaId is int duvida)
            await assistente.ExcluirSemRespostaAsync(duvida);

        TempData["Sucesso"] = "Resposta salva. O assistente já está usando.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Excluir(int id)
    {
        if (await assistente.ExcluirPerguntaAsync(id)) TempData["Sucesso"] = "Pergunta excluída.";
        else TempData["Erro"] = "Pergunta não encontrada.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> DescartarDuvida(int id)
    {
        await assistente.ExcluirSemRespostaAsync(id);
        TempData["Sucesso"] = "Dúvida descartada.";
        return RedirectToAction(nameof(Index));
    }
}
