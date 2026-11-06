using System.ComponentModel.DataAnnotations;
using MiauShop.Models;
using MiauShop.Models.Entities;
using MiauShop.Models.Enums;
using MiauShop.Models.ViewModels;
using MiauShop.Repositories.Interfaces;
using MiauShop.Services.Interfaces;

namespace MiauShop.Services;

// Assistente da loja: um roteiro com menu numerado + respostas cadastradas no painel.
// Não usa IA (para o projeto continuar gratuito). Preço, frete e status de pedido vêm sempre
// dos mesmos serviços da loja, então o assistente nunca "inventa" um valor ou prazo.
public class AssistenteService(
    IRepositorio<PerguntaFrequente> perguntas,
    IPerguntaSemRespostaRepositorio semResposta,
    ICatalogoService catalogo,
    IFreteService frete,
    ICarrinhoService carrinho,
    IPedidoService pedidos) : IAssistenteService
{
    public const int TamanhoMaximoMensagem = 200;
    private const int ProdutosPorResposta = 3;

    private static readonly OpcaoAssistente[] Menu =
    [
        new("1", "Dúvidas frequentes"),
        new("2", "Procurar um produto"),
        new("3", "Calcular frete e prazo"),
        new("4", "Acompanhar meu pedido"),
        new("5", "Falar com uma pessoa")
    ];

    private static readonly OpcaoAssistente VoltarAoMenu = new("0", "Voltar ao menu");

    public async Task<RespostaAssistente> ResponderAsync(string? etapa, string? texto, string? contexto)
    {
        texto = (texto ?? string.Empty).Trim();
        if (texto.Length > TamanhoMaximoMensagem)
            texto = texto[..TamanhoMaximoMensagem];

        if (etapa is null or EtapasAssistente.Inicio || texto.Length == 0)
            return BoasVindas();

        if (texto == "0" || BuscadorDeRespostas.Normalizar(texto) is "menu" or "voltar" or "inicio")
            return ComMenu(new RespostaAssistente(), "Certo! Como posso ajudar?");

        return etapa switch
        {
            EtapasAssistente.Duvidas => await ResponderDuvidaAsync(texto),
            EtapasAssistente.Produto => await BuscarProdutosAsync(texto, textoLivre: false),
            EtapasAssistente.Frete => await CalcularFreteAsync(texto),
            EtapasAssistente.PedidoNumero => PedirEmail(texto),
            EtapasAssistente.PedidoEmail => await ConsultarPedidoAsync(texto, contexto),
            _ => await ResponderMenuAsync(texto)
        };
    }

    // ---------- Etapas ----------

    private static RespostaAssistente BoasVindas() => ComMenu(new RespostaAssistente(),
        "Oi! Eu sou o Miau, o assistente da MiauShop.",
        "Escolha uma opção pelo número ou escreva sua dúvida.");

    private async Task<RespostaAssistente> ResponderMenuAsync(string texto)
    {
        switch (texto)
        {
            case "1": return await ListarDuvidasAsync();
            case "2": return Perguntar(EtapasAssistente.Produto, "Digite o nome ou o tipo de produto (ex.: caminha, petisco, moletom)", "O que você está procurando?");
            case "3": return Perguntar(EtapasAssistente.Frete, "Digite o CEP (00000-000)", "Qual é o seu CEP? Eu calculo o frete pela distância até a loja.");
            case "4": return Perguntar(EtapasAssistente.PedidoNumero, "Digite o número do pedido", "Qual é o número do pedido? Ele está no e-mail e na tela de confirmação da compra.");
            case "5": return Atendente();
        }

        return await ResponderTextoLivreAsync(texto);
    }

    private async Task<RespostaAssistente> ListarDuvidasAsync()
    {
        var lista = await PerguntasAtivasAsync();
        if (lista.Count == 0)
            return ComMenu(new RespostaAssistente(), "Ainda não há dúvidas cadastradas. Escreva a sua que eu tento ajudar.");

        var resposta = new RespostaAssistente { Etapa = EtapasAssistente.Duvidas, Dica = "Digite o número da dúvida" };
        resposta.Mensagens.Add("Sobre o que você quer saber?");
        for (var i = 0; i < lista.Count; i++)
            resposta.Opcoes.Add(new OpcaoAssistente((i + 1).ToString(), lista[i].Pergunta));
        resposta.Opcoes.Add(VoltarAoMenu);
        return resposta;
    }

    private async Task<RespostaAssistente> ResponderDuvidaAsync(string texto)
    {
        var lista = await PerguntasAtivasAsync();
        if (int.TryParse(texto, out var numero) && numero >= 1 && numero <= lista.Count)
            return ComMenu(new RespostaAssistente(), lista[numero - 1].Resposta, "Posso ajudar em algo mais?");

        return await ResponderTextoLivreAsync(texto);
    }

    // Texto escrito pelo cliente: CEP na frase -> frete; resposta cadastrada -> resposta;
    // pedido de ferramenta (rastrear, frete, atendente) -> ferramenta; nome de produto -> produtos.
    private async Task<RespostaAssistente> ResponderTextoLivreAsync(string texto)
    {
        switch (BuscadorDeRespostas.IdentificarCortesia(texto))
        {
            case Cortesia.Cumprimento: return BoasVindas();
            case Cortesia.Agradecimento: return ComMenu(new RespostaAssistente(), "Por nada! Posso ajudar em algo mais?");
        }

        if (BuscadorDeRespostas.EncontrarCep(texto) is string cep)
            return await CalcularFreteAsync(cep);

        if (BuscadorDeRespostas.MelhorResposta(texto, await PerguntasAtivasAsync()) is { } encontrada)
            return ComMenu(new RespostaAssistente(), encontrada.Resposta, "Posso ajudar em algo mais?");

        switch (BuscadorDeRespostas.IdentificarAcao(texto))
        {
            case EtapasAssistente.PedidoNumero: return await ResponderMenuAsync("4");
            case EtapasAssistente.Frete: return await ResponderMenuAsync("3");
            case EtapasAssistente.Atendente: return Atendente();
        }

        var produtos = await BuscarProdutosAsync(texto, textoLivre: true);
        if (produtos.Produtos.Count > 0)
            return produtos;

        await RegistrarSemRespostaAsync(texto);
        return ComMenu(new RespostaAssistente(),
            "Ainda não sei responder isso. Anotei sua pergunta para a equipe da loja criar uma resposta.",
            "Enquanto isso, veja se uma destas opções ajuda:");
    }

    private async Task<RespostaAssistente> BuscarProdutosAsync(string texto, bool textoLivre)
    {
        // Busca o texto inteiro; se não achar, palavra por palavra (até 3 palavras).
        // Na conversa livre, só as palavras importantes (um "oi" não deve achar produto nenhum).
        var achados = textoLivre ? new List<Produto>() : await BuscarAsync(texto);
        foreach (var palavra in BuscadorDeRespostas.PalavrasImportantes(texto).Take(3))
        {
            if (achados.Count >= ProdutosPorResposta) break;
            achados.AddRange((await BuscarAsync(palavra)).Where(p => achados.All(a => a.Id != p.Id)));
        }

        var resposta = new RespostaAssistente();
        if (achados.Count == 0)
        {
            if (textoLivre)
                return resposta; // quem chamou decide o que dizer

            resposta.Etapa = EtapasAssistente.Produto;
            resposta.Dica = "Tente outra palavra";
            resposta.Mensagens.Add("Não encontrei produtos com esse nome. Tente outra palavra, como \"brinquedo\", \"ração\" ou \"roupinha\".");
            resposta.Opcoes.Add(VoltarAoMenu);
            return resposta;
        }

        resposta.Mensagens.Add(textoLivre ? "Encontrei estes produtos que podem ajudar:" : "Olha o que encontrei:");
        foreach (var produto in achados.Take(ProdutosPorResposta))
            resposta.Produtos.Add(new ProdutoNoAssistente(produto.Nome, $"/Produtos/{produto.Slug}", produto.PrecoAtual(), produto.ImagemPrincipalUrl()));

        resposta.Links.Add(new LinkNoAssistente("Ver todos os resultados", $"/Produtos?busca={Uri.EscapeDataString(texto)}"));
        return ComMenu(resposta, "Quer procurar mais alguma coisa?");
    }

    private async Task<RespostaAssistente> CalcularFreteAsync(string texto)
    {
        var cep = BuscadorDeRespostas.EncontrarCep(texto) ?? texto;
        var compra = await carrinho.ObterAsync();
        var cotacao = await frete.CotarAsync(cep, compra.Subtotal);

        if (cotacao is null)
            return Perguntar(EtapasAssistente.Frete, "Digite o CEP (00000-000)",
                "Não encontrei esse CEP. Confira os 8 números e tente de novo (ou digite 0 para voltar).");

        // O CEP fica salvo no carrinho: lá o frete e o prazo já aparecem calculados.
        carrinho.DefinirCep(cep);

        var distancia = cotacao.DistanciaKm > 0
            ? $" ({(cotacao.Aproximado ? "cerca de " : "")}{cotacao.DistanciaKm} km de {cotacao.CidadeOrigem})"
            : "";
        var resposta = new RespostaAssistente();

        if (compra.Vazio)
        {
            resposta.Mensagens.Add($"Para {cotacao.Cidade}/{cotacao.Uf}{distancia}, o frete fica em {cotacao.ValorPelaDistancia:C}. " +
                                   $"Em compras a partir de {frete.MinimoParaFreteGratis:C0} ele é grátis.");
            resposta.Mensagens.Add($"O transporte leva cerca de {Dias(cotacao.DiasTransporte)}, mais o prazo de envio do fornecedor que aparece em cada produto.");
        }
        else
        {
            var valor = cotacao.Gratis ? "grátis" : cotacao.Valor.ToString("C");
            resposta.Mensagens.Add($"Para {cotacao.Cidade}/{cotacao.Uf}{distancia}, o frete do seu carrinho ({compra.Subtotal:C}) é {valor}.");
            if (compra.PrazoEntregaDias is int prazo)
                resposta.Mensagens.Add($"Entrega em até {Dias(prazo)} (envio do fornecedor + transporte).");
            resposta.Links.Add(new LinkNoAssistente("Ir para o carrinho", "/Carrinho"));
        }

        return ComMenu(resposta, "Posso ajudar em algo mais?");
    }

    private static RespostaAssistente PedirEmail(string texto)
    {
        if (!int.TryParse(texto.TrimStart('#').Trim(), out var numero) || numero <= 0)
            return Perguntar(EtapasAssistente.PedidoNumero, "Digite só os números do pedido",
                "Esse número não parece válido. Digite só os números do pedido (ex.: 12).");

        var resposta = Perguntar(EtapasAssistente.PedidoEmail, "Digite o e-mail da compra",
            $"Certo, pedido #{numero}. Agora me diga o e-mail usado na compra.");
        resposta.Contexto = numero.ToString();
        return resposta;
    }

    private async Task<RespostaAssistente> ConsultarPedidoAsync(string email, string? contexto)
    {
        if (!int.TryParse(contexto, out var numero))
            return await ResponderMenuAsync("4");

        if (!new EmailAddressAttribute().IsValid(email))
        {
            var deNovo = Perguntar(EtapasAssistente.PedidoEmail, "Digite o e-mail da compra", "Esse e-mail não parece válido. Pode conferir?");
            deNovo.Contexto = contexto;
            return deNovo;
        }

        // Mesma regra da tela "Acompanhar pedido": número + e-mail; errar qualquer um dá a mesma resposta.
        var pedido = await pedidos.ObterParaAcompanhamentoAsync(numero, email);
        var resposta = new RespostaAssistente();

        if (pedido is null)
        {
            resposta.Mensagens.Add("Não encontrei um pedido com esse número e e-mail. Confira os dados e tente de novo.");
            resposta.Opcoes.Add(new OpcaoAssistente("4", "Tentar de novo"));
            return ComMenu(resposta);
        }

        var ultimaAtualizacao = pedido.Historico.OrderByDescending(h => h.CriadoEm).FirstOrDefault()?.CriadoEm ?? pedido.CriadoEm;
        resposta.Mensagens.Add($"Pedido #{pedido.Id}: {pedido.Status.NomeExibicao()}.");
        resposta.Mensagens.Add($"Última atualização em {LinhaDoTempo.ParaBrasilia(ultimaAtualizacao):dd/MM/yyyy 'às' HH:mm}. Total: {pedido.Total:C}.");
        resposta.Links.Add(new LinkNoAssistente("Ver a linha do tempo do pedido", $"/Pedido/Acompanhar?numero={pedido.Id}"));
        return ComMenu(resposta, "Posso ajudar em algo mais?");
    }

    private static RespostaAssistente Atendente() => ComMenu(new RespostaAssistente(),
        "Numa loja de verdade, aqui você seria levado ao WhatsApp ou ao e-mail da equipe.",
        "Como a MiauShop é um projeto demonstrativo, ainda não há atendimento humano. Mas sua dúvida pode virar uma resposta: escreva aqui e a loja recebe.");

    // ---------- Apoio ----------

    private static RespostaAssistente Perguntar(string etapa, string dica, string mensagem)
    {
        var resposta = new RespostaAssistente { Etapa = etapa, Dica = dica };
        resposta.Mensagens.Add(mensagem);
        resposta.Opcoes.Add(VoltarAoMenu);
        return resposta;
    }

    private static RespostaAssistente ComMenu(RespostaAssistente resposta, params string[] mensagens)
    {
        resposta.Mensagens.AddRange(mensagens);
        resposta.Etapa = EtapasAssistente.Menu;
        resposta.Dica = "Digite o número da opção ou sua dúvida";
        foreach (var opcao in Menu.Where(m => resposta.Opcoes.All(o => o.Valor != m.Valor)))
            resposta.Opcoes.Add(opcao);
        return resposta;
    }

    private static string Dias(int dias) => dias == 1 ? "1 dia útil" : $"{dias} dias úteis";

    private async Task<List<Produto>> BuscarAsync(string termo) =>
        (await catalogo.BuscarAsync(new FiltroProdutos { Busca = termo })).Itens;

    private async Task<List<PerguntaFrequente>> PerguntasAtivasAsync() =>
        (await perguntas.ListarAsync()).Where(p => p.Ativa).OrderBy(p => p.Ordem).ThenBy(p => p.Id).ToList();

    // Guarda a dúvida para o painel. Não guarda nada que pareça dado pessoal (e-mail, telefone, documento).
    private async Task RegistrarSemRespostaAsync(string texto)
    {
        var normalizado = BuscadorDeRespostas.Normalizar(texto);
        if (normalizado.Length < 3 || BuscadorDeRespostas.PareceDadoPessoal(texto))
            return;

        try
        {
            var existente = await semResposta.ObterPorTextoAsync(normalizado);
            if (existente is null)
                semResposta.Adicionar(new PerguntaSemResposta { Texto = texto, TextoNormalizado = normalizado });
            else
                existente.Vezes++;

            await semResposta.SalvarAsync();
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException)
        {
            // Duas pessoas mandando a mesma frase ao mesmo tempo: uma delas já foi anotada.
        }
    }

    // ---------- Painel ----------

    public async Task<List<PerguntaFrequente>> ListarPerguntasAsync() =>
        (await perguntas.ListarAsync()).OrderBy(p => p.Ordem).ThenBy(p => p.Id).ToList();

    public Task<PerguntaFrequente?> ObterPerguntaAsync(int id) => perguntas.ObterPorIdAsync(id);

    public async Task SalvarPerguntaAsync(int id, string pergunta, string resposta, string? palavrasChave, int ordem, bool ativa)
    {
        var item = id == 0 ? new PerguntaFrequente() : await perguntas.ObterPorIdAsync(id)
                   ?? throw new InvalidOperationException("Pergunta não encontrada.");

        item.Pergunta = pergunta.Trim();
        item.Resposta = resposta.Trim();
        item.PalavrasChave = string.Join(", ", (palavrasChave ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
        item.Ordem = ordem;
        item.Ativa = ativa;

        if (id == 0)
            perguntas.Adicionar(item);

        await perguntas.SalvarAsync();
    }

    public async Task<bool> ExcluirPerguntaAsync(int id)
    {
        var item = await perguntas.ObterPorIdAsync(id);
        if (item is null)
            return false;

        perguntas.Remover(item);
        await perguntas.SalvarAsync();
        return true;
    }

    public Task<List<PerguntaSemResposta>> ListarSemRespostaAsync() => semResposta.ListarMaisFrequentesAsync(50);

    public Task<PerguntaSemResposta?> ObterSemRespostaAsync(int id) => semResposta.ObterPorIdAsync(id);

    public async Task<bool> ExcluirSemRespostaAsync(int id)
    {
        var item = await semResposta.ObterPorIdAsync(id);
        if (item is null)
            return false;

        semResposta.Remover(item);
        await semResposta.SalvarAsync();
        return true;
    }
}
