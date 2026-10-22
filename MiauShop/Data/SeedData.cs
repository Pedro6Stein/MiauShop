using MiauShop.Models.Entities;
using MiauShop.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace MiauShop.Data;

// Dados de exemplo da loja. Roda na inicialização e só insere se o banco estiver vazio.
public static class SeedData
{
    public static async Task InicializarAsync(AppDbContext contexto)
    {
        // Aplica migrations pendentes (no Render/Neon o banco é criado sozinho no primeiro deploy).
        await contexto.Database.MigrateAsync();

        if (await contexto.Categorias.AnyAsync())
            return;

        var brinquedos = Categoria("Brinquedos", "brinquedos", "bi-balloon-heart", 1);
        var comidinhas = Categoria("Comidinhas & Petiscos", "comidinhas-e-petiscos", "bi-cup-hot", 2);
        var roupinhas = Categoria("Roupinhas", "roupinhas", "bi-stars", 3);
        var acessorios = Categoria("Acessórios", "acessorios", "bi-house-heart", 4);

        var petMax = new Fornecedor { Nome = "PetMax Distribuidora", PrazoEnvioDias = 5 };
        var naturalPet = new Fornecedor { Nome = "Natural Pet Foods", PrazoEnvioDias = 3 };
        var mundoPet = new Fornecedor { Nome = "Mundo Pet Import", PrazoEnvioDias = 12 };

        var produtos = new List<Produto>
        {
            // Brinquedos
            Produto("Varinha Pena Arco-Íris", "varinha-pena-arco-iris", Animal.Gato, brinquedos, mundoPet,
                24.90m, null, destaque: true,
                "Penas coloridas e sininho para despertar o caçador que existe no seu gato.",
                "Varinha leve com penas em tons de arco-íris e um sininho na ponta. Estimula o instinto de caça, ajuda a gastar energia e fortalece o vínculo entre você e seu gato nas brincadeiras do dia a dia.",
                "Varinha com penas coloridas e sininho para gatos",
                ("Único", 0m)),
            Produto("Bolinha Mordedor Dental", "bolinha-mordedor-dental", Animal.Cao, brinquedos, petMax,
                29.90m, 22.90m, destaque: false,
                "Borracha resistente com relevos que ajudam a limpar os dentes enquanto ele brinca.",
                "Bolinha de borracha atóxica com relevos que massageiam a gengiva e ajudam a remover resíduos dos dentes. Quica de forma imprevisível, deixando a brincadeira de buscar ainda mais divertida.",
                "Bolinha mordedora de borracha turquesa com relevos",
                ("P", 0m), ("G", 4m)),
            Produto("Arranhador Torre Nuvem", "arranhador-torre-nuvem", Animal.Gato, brinquedos, mundoPet,
                189.90m, 159.90m, destaque: true,
                "Torre com plataformas em formato de nuvem para arranhar, escalar e cochilar.",
                "Arranhador vertical com postes revestidos de sisal e plataformas acolchoadas em formato de nuvem. Protege seus móveis, oferece um mirante para o gato observar a casa e um cantinho macio para descansar.",
                "Arranhador em torre com plataformas de nuvem e um gato no topo",
                ("Único", 0m)),
            Produto("Pelúcia Osso com Apito", "pelucia-osso-com-apito", Animal.Cao, brinquedos, petMax,
                34.90m, null, destaque: false,
                "Pelúcia macia com apito interno, perfeita para cães que amam carregar brinquedos.",
                "Osso de pelúcia com costura reforçada e apito interno que faz barulho a cada mordida. Ideal para cães de porte pequeno e médio que adoram carregar seus brinquedos pela casa.",
                "Brinquedo de pelúcia coral em formato de osso",
                ("Único", 0m)),

            // Comidinhas & Petiscos
            Produto("Bifinho de Frango Natural", "bifinho-de-frango-natural", Animal.Cao, comidinhas, naturalPet,
                19.90m, null, destaque: false,
                "Petisco desidratado feito só com frango, sem corantes e sem conservantes.",
                "Bifinhos de peito de frango desidratados lentamente, sem corantes, conservantes ou aromatizantes artificiais. Ótimos para adestramento e para recompensar o bom comportamento.",
                "Pacote de petisco com bifinhos de frango desidratado num pratinho",
                ("65 g", 0m), ("250 g", 24m)),
            Produto("Sachê Gourmet Salmão (caixa c/ 12)", "sache-gourmet-salmao", Animal.Gato, comidinhas, naturalPet,
                42.90m, 36.90m, destaque: true,
                "Caixa com 12 sachês de salmão em pedaços ao molho, para gatos adultos.",
                "Alimento úmido completo com pedaços de salmão ao molho. Ajuda na hidratação diária e agrada até os gatos mais exigentes. Caixa com 12 sachês de 85 g.",
                "Caixa de papelão com sachês de comida para gato",
                ("Único", 0m)),
            Produto("Ração Natural Filhotes", "racao-natural-filhotes", Animal.Cao, comidinhas, naturalPet,
                119.90m, null, destaque: false,
                "Ração natural com ingredientes selecionados para o crescimento saudável do filhote.",
                "Ração natural para cães filhotes de todos os portes, com proteína de frango, DHA para o desenvolvimento cerebral e cálcio na medida certa para ossos e dentes fortes.",
                "Saco de ração para filhotes com grãos e uma tigela",
                ("2,5 kg", 0m), ("7 kg", 160m)),
            Produto("Petisco Cremoso de Atum (4 un.)", "petisco-cremoso-de-atum", Animal.Gato, comidinhas, naturalPet,
                21.90m, null, destaque: false,
                "Petisco cremoso para dar na mão: o momento favorito do dia do seu gato.",
                "Petisco cremoso sabor atum em tubinhos individuais, para oferecer direto na mão ou sobre a ração. Pacote com 4 unidades de 15 g.",
                "Sachês em tubo de petisco cremoso para gatos",
                ("Único", 0m)),

            // Roupinhas
            Produto("Moletom Pet Inverno", "moletom-pet-inverno", Animal.Ambos, roupinhas, petMax,
                69.90m, 54.90m, destaque: true,
                "Moletom quentinho de tricô para os dias frios, fácil de vestir.",
                "Moletom de tricô macio com elástico nas mangas e abertura confortável para a coleira. Mantém seu pet aquecido sem atrapalhar os movimentos. Consulte a tabela de medidas antes de escolher o tamanho.",
                "Cachorro shih-tzu usando moletom de tricô mostarda",
                ("P", 0m), ("M", 0m), ("G", 5m)),
            Produto("Capa de Chuva Amarelinha", "capa-de-chuva-amarelinha", Animal.Cao, roupinhas, petMax,
                79.90m, null, destaque: false,
                "Capa impermeável com capuz para passear mesmo nos dias de chuva.",
                "Capa de chuva impermeável com capuz, faixa refletiva para passeios à noite e fechamento em velcro. Seu cão volta do passeio sequinho.",
                "Cachorro beagle usando capa de chuva amarela com capuz",
                ("P", 0m), ("M", 0m), ("G", 6m)),
            Produto("Bandana Xadrez", "bandana-xadrez", Animal.Ambos, roupinhas, petMax,
                24.90m, null, destaque: false,
                "Bandana de algodão xadrez: charme instantâneo para cães e gatos.",
                "Bandana de algodão macio com estampa xadrez vermelha e branca e fechamento com botão de pressão. Leve, confortável e perfeita para fotos.",
                "Gato laranja usando bandana xadrez vermelha e branca",
                ("P", 0m), ("M", 0m)),
            Produto("Pijama Listrado Soninho", "pijama-listrado-soninho", Animal.Gato, roupinhas, mundoPet,
                49.90m, null, destaque: false,
                "Pijama de malha listrada, macio e elástico, para as sonecas mais gostosas.",
                "Pijama de malha de algodão com elastano, listrado em azul-claro e branco. Tecido respirável que acompanha os movimentos e mantém o gato aquecido nas noites frias.",
                "Gato cinza dormindo vestindo pijama listrado azul e branco",
                ("P", 0m), ("M", 0m)),

            // Acessórios
            Produto("Caminha Nuvem Antiestresse", "caminha-nuvem-antiestresse", Animal.Ambos, acessorios, mundoPet,
                149.90m, 119.90m, destaque: true,
                "Caminha redonda de pelúcia que abraça o pet e ajuda a reduzir a ansiedade.",
                "Cama redonda de pelúcia com bordas altas e enchimento fofo, que dão ao pet a sensação de aconchego e segurança. Base antiderrapante e capa lavável.",
                "Cama redonda de pelúcia creme com gato branco dormindo",
                ("P", 0m), ("M", 40m)),
            Produto("Fonte Bebedouro 2 L", "fonte-bebedouro-2l", Animal.Gato, acessorios, mundoPet,
                139.90m, null, destaque: true,
                "Água sempre fresca e corrente, que incentiva o gato a beber mais.",
                "Fonte elétrica silenciosa com filtro de carvão ativado e reservatório de 2 litros. A água em movimento atrai os gatos e ajuda a prevenir problemas urinários.",
                "Fonte bebedouro branca para gatos com água em cascata",
                ("Único", 0m)),
            Produto("Peitoral Ajustável com Guia", "peitoral-ajustavel-com-guia", Animal.Cao, acessorios, petMax,
                59.90m, null, destaque: false,
                "Peitoral acolchoado que não machuca o pescoço, com guia combinando.",
                "Peitoral acolchoado com ajuste em quatro pontos e fivela de engate rápido. Distribui a força pelo peito em vez do pescoço. Acompanha guia de 1,2 m na mesma cor.",
                "Peitoral acolchoado verde-menta com guia",
                ("P", 0m), ("M", 0m), ("G", 0m)),
            Produto("Comedouro Elevado Duplo", "comedouro-elevado-duplo", Animal.Ambos, acessorios, petMax,
                74.90m, null, destaque: false,
                "Suporte de madeira com duas tigelas de inox, na altura certa para comer melhor.",
                "Comedouro elevado em madeira de reflorestamento com duas tigelas de inox removíveis. A altura elevada melhora a postura durante as refeições e facilita a digestão.",
                "Comedouro elevado de madeira com duas tigelas de inox",
                ("Único", 0m)),
        };

        contexto.Produtos.AddRange(produtos);
        await contexto.SaveChangesAsync();
    }

    private static Categoria Categoria(string nome, string slug, string icone, int ordem) =>
        new() { Nome = nome, Slug = slug, Icone = icone, Ordem = ordem };

    private static Produto Produto(
        string nome, string slug, Animal animal, Categoria categoria, Fornecedor fornecedor,
        decimal preco, decimal? precoPromocional, bool destaque,
        string descricaoCurta, string descricao, string textoAlternativo,
        params (string Nome, decimal Ajuste)[] variacoes)
    {
        var produto = new Produto
        {
            Nome = nome,
            Slug = slug,
            Animal = animal,
            Categoria = categoria,
            Fornecedor = fornecedor,
            Preco = preco,
            PrecoPromocional = precoPromocional,
            Destaque = destaque,
            DescricaoCurta = descricaoCurta,
            Descricao = descricao
        };

        produto.Imagens.Add(new ProdutoImagem
        {
            Url = $"/img/produtos/{slug}.webp",
            TextoAlternativo = textoAlternativo,
            Principal = true
        });

        // SKU = slug + posição da variação (ex.: "moletom-pet-inverno-3"), sempre único.
        for (var i = 0; i < variacoes.Length; i++)
        {
            produto.Variacoes.Add(new ProdutoVariacao
            {
                Nome = variacoes[i].Nome,
                AjustePreco = variacoes[i].Ajuste,
                Sku = $"{slug}-{i + 1}",
                Ordem = i
            });
        }

        return produto;
    }
}
