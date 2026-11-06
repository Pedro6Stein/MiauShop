using MiauShop.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace MiauShop.Data;

// Ficha técnica e avaliações de exemplo dos 16 produtos do seed.
// Roda em toda inicialização, mas só preenche o que estiver vazio: funciona no banco novo
// e também no banco que já está no ar (que foi criado antes desses campos existirem).
// Nomes e comentários são fictícios, como o resto da loja.
public static class SeedDetalhesProdutos
{
    private record Ficha(string Material, string Medidas, string Garantia, string Caracteristicas);

    private static readonly Dictionary<string, Ficha> Fichas = new()
    {
        ["varinha-pena-arco-iris"] = new("Haste de plástico ABS, penas naturais e sininho de metal",
            "Haste de 45 cm + 40 cm de cordão", "90 dias contra defeitos de fabricação",
            "Haste flexível que não quebra fácil\nSininho que chama a atenção do gato\nPenas fáceis de trocar"),
        ["bolinha-mordedor-dental"] = new("Borracha natural atóxica, sem BPA",
            "P: 6 cm de diâmetro · G: 8 cm de diâmetro", "90 dias contra defeitos de fabricação",
            "Relevos que massageiam a gengiva\nFlutua na água\nPode lavar com água e sabão neutro"),
        ["arranhador-torre-nuvem"] = new("MDF revestido de pelúcia, postes de sisal natural",
            "Altura 92 cm · base 40 × 40 cm · suporta até 8 kg", "6 meses contra defeitos de fabricação",
            "3 plataformas em formato de nuvem\nPostes de sisal substituíveis\nMontagem sem ferramentas especiais"),
        ["pelucia-osso-com-apito"] = new("Pelúcia de poliéster com costura dupla e apito interno",
            "26 × 10 cm", "30 dias contra defeitos de fabricação",
            "Apito que faz barulho a cada mordida\nCostura reforçada nas pontas\nLavável à mão"),
        ["bifinho-de-frango-natural"] = new("Peito de frango desidratado (100% frango)",
            "Pacotes de 65 g ou 250 g", "Validade de 12 meses (fechado)",
            "Sem corantes nem conservantes\nRico em proteína\nIdeal para adestramento"),
        ["sache-gourmet-salmao"] = new("Salmão, frango, caldo de peixe, vitaminas e minerais",
            "12 sachês de 85 g", "Validade de 18 meses (fechado)",
            "Alimento completo para gatos adultos\nAjuda na hidratação\nPedaços macios ao molho"),
        ["racao-natural-filhotes"] = new("Frango, arroz integral, ovo, óleo de peixe (DHA) e cálcio",
            "Sacos de 2,5 kg ou 7 kg", "Validade de 12 meses (fechado)",
            "Para filhotes de todos os portes\nSem corantes artificiais\nEmbalagem com fecho zip"),
        ["petisco-cremoso-de-atum"] = new("Atum, caldo de atum, fibras e taurina",
            "4 tubinhos de 15 g", "Validade de 12 meses (fechado)",
            "Para dar direto na mão\nPode misturar com a ração\nSem grãos"),
        ["moletom-pet-inverno"] = new("Tricô 100% acrílico, punhos com elástico",
            "Costas — P: 25 cm · M: 32 cm · G: 40 cm (meça da nuca à base do rabo)", "30 dias contra defeitos de fabricação",
            "Abertura para a coleira ou peitoral\nLavável na máquina (ciclo delicado)\nNão solta pelos"),
        ["capa-de-chuva-amarelinha"] = new("Nylon impermeável com forro de poliéster",
            "Costas — P: 28 cm · M: 36 cm · G: 45 cm", "90 dias contra defeitos de fabricação",
            "Faixa refletiva para passeios à noite\nCapuz removível\nFecho em velcro na barriga"),
        ["bandana-xadrez"] = new("Algodão 100%",
            "Pescoço — P: 22 a 32 cm · M: 32 a 44 cm", "30 dias contra defeitos de fabricação",
            "Fecho com botão de pressão\nTecido macio que não irrita a pele\nLavável na máquina"),
        ["pijama-listrado-soninho"] = new("Malha de algodão (95%) com elastano (5%)",
            "Costas — P: 24 cm · M: 30 cm", "30 dias contra defeitos de fabricação",
            "Tecido respirável\nAcompanha os movimentos\nLavável na máquina"),
        ["caminha-nuvem-antiestresse"] = new("Pelúcia de poliéster, enchimento de fibra siliconada, base antiderrapante",
            "P: 50 cm de diâmetro · M: 70 cm de diâmetro", "6 meses contra defeitos de fabricação",
            "Bordas altas que dão sensação de aconchego\nCapa removível e lavável\nBase antiderrapante"),
        ["fonte-bebedouro-2l"] = new("Plástico livre de BPA, bomba submersa silenciosa",
            "Reservatório de 2 L · 20 × 20 × 15 cm · cabo USB de 1,5 m", "1 ano contra defeitos de fabricação",
            "Filtro de carvão ativado (troca a cada 30 dias)\nFunciona com carregador de celular (5 V)\nLuz que avisa quando a água está baixa"),
        ["peitoral-ajustavel-com-guia"] = new("Poliéster acolchoado, fivelas de engate rápido, argola de metal",
            "Peito — P: 35 a 45 cm · M: 45 a 60 cm · G: 60 a 80 cm · guia de 1,2 m", "90 dias contra defeitos de fabricação",
            "Ajuste em 4 pontos\nNão aperta o pescoço\nAlça de mão acolchoada na guia"),
        ["comedouro-elevado-duplo"] = new("Madeira de reflorestamento e tigelas de aço inox",
            "Altura 12 cm · tigelas de 400 ml cada", "6 meses contra defeitos de fabricação",
            "Tigelas removíveis, vão à lava-louças\nPés com borracha antiderrapante\nMelhora a postura nas refeições"),
    };

    private static readonly (string Slug, string Nome, int Nota, string Comentario, int DiasAtras)[] Avaliacoes =
    [
        ("varinha-pena-arco-iris", "Juliana Prado", 5, "Minha gata fica doida com o sininho! Brinca todos os dias antes de dormir.", 12),
        ("varinha-pena-arco-iris", "Marcos Lima", 4, "Muito boa, as penas soltaram um pouco depois de duas semanas, mas ela continua amando.", 30),
        ("bolinha-mordedor-dental", "Renata Alves", 5, "Resistente de verdade. Meu labrador destrói tudo e essa bolinha está inteira.", 8),
        ("bolinha-mordedor-dental", "Pedro Henrique", 5, "Quica de um jeito engraçado, ele corre atrás o tempo todo.", 21),
        ("bolinha-mordedor-dental", "Carla Mendes", 4, "Boa, só achei o tamanho P pequeno para cachorro médio. Peguei o G depois.", 40),
        ("arranhador-torre-nuvem", "Beatriz Souza", 5, "Lindo e firme. Os gatos pararam de arranhar o sofá na primeira semana.", 5),
        ("arranhador-torre-nuvem", "Thiago Rocha", 4, "Montagem fácil. A plataforma de cima é a preferida do meu gato.", 26),
        ("sache-gourmet-salmao", "Fernanda Dias", 5, "Meu gato é chatíssimo para comer e aceitou todos os sachês.", 14),
        ("sache-gourmet-salmao", "Lucas Martins", 4, "Bom custo-benefício na caixa com 12.", 33),
        ("moletom-pet-inverno", "Aline Costa", 5, "Ficou perfeito no meu shih-tzu. Segui a tabela de medidas e acertei o tamanho.", 9),
        ("moletom-pet-inverno", "Rafael Nunes", 4, "Quentinho e bonito. Lavei na máquina e não deformou.", 19),
        ("capa-de-chuva-amarelinha", "Patrícia Gomes", 5, "Voltamos do passeio sequinhos. A faixa refletiva ajuda muito à noite.", 16),
        ("caminha-nuvem-antiestresse", "Camila Ribeiro", 5, "Minha cachorra dorme a noite toda nela. Muito macia!", 7),
        ("caminha-nuvem-antiestresse", "Gustavo Ferreira", 5, "Os dois gatos disputam a caminha. Vou ter que comprar outra.", 22),
        ("caminha-nuvem-antiestresse", "Larissa Teixeira", 4, "Muito boa. A capa sai fácil para lavar.", 45),
        ("fonte-bebedouro-2l", "Vinícius Araújo", 5, "Silenciosa e meu gato passou a beber muito mais água.", 11),
        ("fonte-bebedouro-2l", "Mariana Lopes", 4, "Ótima, só lembrar de trocar o filtro todo mês.", 28),
        ("peitoral-ajustavel-com-guia", "Bruno Carvalho", 5, "Meu beagle parou de engasgar no passeio. Recomendo.", 13),
        ("bifinho-de-frango-natural", "Isabela Moreira", 5, "Uso no adestramento e funciona demais. Cheiro de frango de verdade.", 6),
        ("comedouro-elevado-duplo", "Diego Barbosa", 4, "Bonito e firme, as tigelas de inox são fáceis de lavar.", 35),
    ];

    public static async Task InicializarAsync(AppDbContext contexto)
    {
        var slugs = Fichas.Keys.ToList();
        var produtos = await contexto.Produtos
            .Where(p => slugs.Contains(p.Slug))
            .ToDictionaryAsync(p => p.Slug);

        // Ficha técnica: só nos produtos que ainda não têm (não sobrescreve o que foi editado no painel).
        foreach (var (slug, ficha) in Fichas)
        {
            if (!produtos.TryGetValue(slug, out var produto) || produto.TemFichaTecnica())
                continue;

            produto.Material = ficha.Material;
            produto.Medidas = ficha.Medidas;
            produto.Garantia = ficha.Garantia;
            produto.Caracteristicas ??= ficha.Caracteristicas;
        }

        // Avaliações de exemplo: só se a loja ainda não tiver nenhuma.
        if (!await contexto.AvaliacoesProduto.AnyAsync())
        {
            var agora = DateTime.UtcNow;
            foreach (var (slug, nome, nota, comentario, diasAtras) in Avaliacoes)
            {
                if (!produtos.TryGetValue(slug, out var produto))
                    continue;

                var avaliacao = AvaliacaoProduto.Nova(produto.Id, nome, nota, comentario);
                avaliacao.Aprovar();
                avaliacao.CriadoEm = agora.AddDays(-diasAtras); // datas espalhadas, como numa loja real
                contexto.AvaliacoesProduto.Add(avaliacao);
            }
        }

        await contexto.SaveChangesAsync();
    }
}
