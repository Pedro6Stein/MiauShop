using MiauShop.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace MiauShop.Data;

// Perguntas e respostas iniciais do assistente. Só entram se a tabela estiver vazia;
// depois disso, quem cuida é o painel (Admin > Assistente).
public static class SeedAssistente
{
    private static readonly (string Pergunta, string Resposta, string PalavrasChave)[] Perguntas =
    [
        ("O frete é grátis?",
         "Sim, em compras a partir de R$ 150. Abaixo disso, o frete é calculado pela distância entre a loja e o seu CEP. Escolha a opção 3 do menu para simular.",
         "frete gratis, gratis, sem frete, entrega gratis"),
        ("Qual é o prazo de entrega?",
         "O prazo é o tempo de envio do fornecedor (aparece na página de cada produto) mais o transporte até você, cerca de 1 dia útil a cada 500 km. Escolha a opção 3 do menu e eu calculo para o seu CEP.",
         "prazo, demora, quando chega, quantos dias, chegar, entrega"),
        ("Quais são as formas de pagamento?",
         "Pix, cartão de crédito e boleto. Como a MiauShop é um projeto demonstrativo, nenhum pagamento é processado: na hora de pagar, o sistema avisa e registra um pedido de teste.",
         "pagamento, pagar, pix, cartao, boleto, parcela, parcelar"),
        ("Posso trocar ou devolver um produto?",
         "Pode. Você tem até 7 dias depois de receber para desistir da compra (Código de Defesa do Consumidor). Para defeitos, vale a garantia que aparece na ficha técnica de cada produto.",
         "troca, trocar, devolver, devolucao, arrependi, defeito, garantia"),
        ("Como escolho o tamanho da roupinha?",
         "Cada roupinha tem as medidas na ficha técnica da página do produto. Meça seu pet da nuca até a base do rabo e escolha o tamanho mais próximo. Na dúvida, prefira o maior.",
         "tamanho, medida, medidas, roupa, roupinha, moletom, serve"),
        ("Preciso criar uma conta para comprar?",
         "Não. Você compra sem cadastro: é só informar nome, e-mail e endereço no checkout. Para acompanhar o pedido, use o número dele e o seu e-mail.",
         "conta, cadastro, cadastrar, login, senha, registrar"),
        ("As compras são reais?",
         "Não. A MiauShop é um projeto acadêmico de demonstração (Fatec Bragança Paulista): nenhum produto é enviado e nenhum valor é cobrado.",
         "real, verdade, golpe, confiavel, seguro, demonstracao, projeto"),
        ("Vocês têm produtos para cães e para gatos?",
         "Temos para os dois! No topo da loja, use os botões \"Cães\" e \"Gatos\" para filtrar. Produtos marcados como \"Cães e gatos\" servem para ambos.",
         "cachorro, caes, cao, gato, gatos, filhote, pet"),
    ];

    public static async Task InicializarAsync(AppDbContext contexto)
    {
        if (await contexto.PerguntasFrequentes.AnyAsync())
            return;

        for (var i = 0; i < Perguntas.Length; i++)
        {
            var (pergunta, resposta, chaves) = Perguntas[i];
            contexto.PerguntasFrequentes.Add(new PerguntaFrequente
            {
                Pergunta = pergunta,
                Resposta = resposta,
                PalavrasChave = chaves,
                Ordem = i + 1
            });
        }

        await contexto.SaveChangesAsync();
    }
}
