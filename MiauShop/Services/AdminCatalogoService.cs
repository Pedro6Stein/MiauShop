using MiauShop.Areas.Admin.ViewModels;
using MiauShop.Models.Entities;
using MiauShop.Repositories.Interfaces;
using MiauShop.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MiauShop.Services;

public class AdminCatalogoService(
    IRepositorio<Categoria> categoriaRepositorio,
    IRepositorio<Fornecedor> fornecedorRepositorio,
    IProdutoRepositorio produtoRepositorio) : IAdminCatalogoService
{
    public const int ProdutosPorPagina = 20;
    private const string MensagemConcorrencia =
        "Este produto foi alterado por outra pessoa enquanto você editava. Recarregue a página e confira os dados.";

    // ---------- Categorias ----------

    public async Task<List<CategoriaLinha>> ListarCategoriasAsync()
    {
        var quantidades = await produtoRepositorio.ContarPorCategoriaAsync();
        return (await categoriaRepositorio.ListarAsync())
            .OrderBy(c => c.Ordem).ThenBy(c => c.Nome)
            .Select(c => new CategoriaLinha(c, quantidades.GetValueOrDefault(c.Id)))
            .ToList();
    }

    public async Task<CategoriaFormulario?> ObterCategoriaAsync(int id) =>
        await categoriaRepositorio.ObterPorIdAsync(id) is { } c
            ? new CategoriaFormulario { Id = c.Id, Nome = c.Nome, Descricao = c.Descricao, Icone = c.Icone, Ordem = c.Ordem, Ativa = c.Ativa }
            : null;

    public async Task<List<string>> SalvarCategoriaAsync(CategoriaFormulario formulario)
    {
        if (!CategoriaFormulario.IconesDisponiveis.Any(i => i.Classe == formulario.Icone))
            return ["Escolha um dos ícones da lista."];

        var todas = await categoriaRepositorio.ListarAsync();
        var nome = formulario.Nome.Trim();

        if (todas.Any(c => c.Id != formulario.Id && string.Equals(c.Nome, nome, StringComparison.OrdinalIgnoreCase)))
            return ["Já existe uma categoria com esse nome."];

        Categoria categoria;
        if (formulario.Id == 0)
        {
            // Slug gerado só na criação: renomear não quebra links já compartilhados.
            categoria = new Categoria { Slug = GeradorSlug.Unico(nome, todas.Select(c => c.Slug)) };
            categoriaRepositorio.Adicionar(categoria);
        }
        else
        {
            categoria = await categoriaRepositorio.ObterPorIdAsync(formulario.Id) ?? throw new InvalidOperationException("Categoria não encontrada.");
        }

        categoria.Nome = nome;
        categoria.Descricao = string.IsNullOrWhiteSpace(formulario.Descricao) ? null : formulario.Descricao.Trim();
        categoria.Icone = formulario.Icone;
        categoria.Ordem = formulario.Ordem;
        categoria.Ativa = formulario.Ativa;

        await categoriaRepositorio.SalvarAsync();
        return [];
    }

    public async Task<List<string>> ExcluirCategoriaAsync(int id)
    {
        var categoria = await categoriaRepositorio.ObterPorIdAsync(id);
        if (categoria is null)
            return ["Categoria não encontrada."];

        var produtos = (await produtoRepositorio.ContarPorCategoriaAsync()).GetValueOrDefault(id);
        if (produtos > 0)
            return [$"Esta categoria tem {produtos} produto(s). Para tirá-la da loja, desative em vez de excluir."];

        categoriaRepositorio.Remover(categoria);
        await categoriaRepositorio.SalvarAsync();
        return [];
    }

    // ---------- Fornecedores ----------

    public async Task<List<FornecedorLinha>> ListarFornecedoresAsync()
    {
        var quantidades = await produtoRepositorio.ContarPorFornecedorAsync();
        return (await fornecedorRepositorio.ListarAsync())
            .OrderBy(f => f.Nome)
            .Select(f => new FornecedorLinha(f, quantidades.GetValueOrDefault(f.Id)))
            .ToList();
    }

    public async Task<FornecedorFormulario?> ObterFornecedorAsync(int id) =>
        await fornecedorRepositorio.ObterPorIdAsync(id) is { } f
            ? new FornecedorFormulario { Id = f.Id, Nome = f.Nome, PrazoEnvioDias = f.PrazoEnvioDias, Contato = f.Contato, Ativo = f.Ativo }
            : null;

    public async Task<List<string>> SalvarFornecedorAsync(FornecedorFormulario formulario)
    {
        Fornecedor fornecedor;
        if (formulario.Id == 0)
        {
            fornecedor = new Fornecedor();
            fornecedorRepositorio.Adicionar(fornecedor);
        }
        else
        {
            fornecedor = await fornecedorRepositorio.ObterPorIdAsync(formulario.Id) ?? throw new InvalidOperationException("Fornecedor não encontrado.");

            // Não deixa produto à venda sem ninguém para enviar.
            if (fornecedor.Ativo && !formulario.Ativo)
            {
                var ativos = await produtoRepositorio.ContarAtivosDoFornecedorAsync(fornecedor.Id);
                if (ativos > 0)
                    return [$"Este fornecedor ainda tem {ativos} produto(s) ativo(s). Desative ou troque o fornecedor desses produtos antes."];
            }
        }

        fornecedor.Nome = formulario.Nome.Trim();
        fornecedor.PrazoEnvioDias = formulario.PrazoEnvioDias;
        fornecedor.Contato = string.IsNullOrWhiteSpace(formulario.Contato) ? null : formulario.Contato.Trim();
        fornecedor.Ativo = formulario.Ativo;

        await fornecedorRepositorio.SalvarAsync();
        return [];
    }

    public async Task<List<string>> ExcluirFornecedorAsync(int id)
    {
        var fornecedor = await fornecedorRepositorio.ObterPorIdAsync(id);
        if (fornecedor is null)
            return ["Fornecedor não encontrado."];

        var produtos = (await produtoRepositorio.ContarPorFornecedorAsync()).GetValueOrDefault(id);
        if (produtos > 0)
            return [$"Este fornecedor atende {produtos} produto(s). Desative em vez de excluir."];

        fornecedorRepositorio.Remover(fornecedor);
        await fornecedorRepositorio.SalvarAsync();
        return [];
    }

    // ---------- Produtos ----------

    public async Task<ProdutosListaAdminViewModel> ListarProdutosAsync(string? busca, int? categoriaId, bool? ativo, int pagina)
    {
        pagina = Math.Max(pagina, 1);
        var (produtos, total) = await produtoRepositorio.ListarParaPainelAsync(busca, categoriaId, ativo, pagina, ProdutosPorPagina);

        return new ProdutosListaAdminViewModel
        {
            Produtos = produtos,
            Categorias = (await categoriaRepositorio.ListarAsync()).OrderBy(c => c.Ordem).ToList(),
            Busca = busca,
            CategoriaId = categoriaId,
            Ativo = ativo,
            Pagina = pagina,
            Total = total,
            TotalPaginas = (int)Math.Ceiling(total / (double)ProdutosPorPagina)
        };
    }

    public async Task<ProdutoEdicaoViewModel?> ObterEdicaoProdutoAsync(int? id, ProdutoFormulario? formularioComErros = null)
    {
        Produto? produto = null;
        if (id is int produtoId)
        {
            produto = await produtoRepositorio.ObterParaEdicaoAsync(produtoId);
            if (produto is null)
                return null;
        }

        var imagem = produto?.Imagens.FirstOrDefault(i => i.Principal);
        var formulario = formularioComErros ?? (produto is null
            ? new ProdutoFormulario()
            : new ProdutoFormulario
            {
                Id = produto.Id,
                Versao = produto.Versao,
                Nome = produto.Nome,
                DescricaoCurta = produto.DescricaoCurta,
                Descricao = produto.Descricao,
                CategoriaId = produto.CategoriaId,
                FornecedorId = produto.FornecedorId,
                Animal = produto.Animal,
                Preco = produto.Preco,
                PrecoPromocional = produto.PrecoPromocional,
                Destaque = produto.Destaque,
                Ativo = produto.Ativo,
                ImagemUrl = imagem?.Url ?? string.Empty,
                ImagemTexto = imagem?.TextoAlternativo ?? string.Empty
            });

        return new ProdutoEdicaoViewModel
        {
            Formulario = formulario,
            Slug = produto?.Slug,
            Variacoes = produto?.Variacoes.OrderBy(v => v.Ordem).ToList() ?? [],
            // No seletor só entram categorias/fornecedores ativos (mais o atual, se tiver sido desativado).
            Categorias = (await categoriaRepositorio.ListarAsync())
                .Where(c => c.Ativa || c.Id == formulario.CategoriaId).OrderBy(c => c.Ordem).ToList(),
            Fornecedores = (await fornecedorRepositorio.ListarAsync())
                .Where(f => f.Ativo || f.Id == formulario.FornecedorId).OrderBy(f => f.Nome).ToList()
        };
    }

    public async Task<(int Id, List<string> Erros)> SalvarProdutoAsync(ProdutoFormulario formulario)
    {
        var categoria = await categoriaRepositorio.ObterPorIdAsync(formulario.CategoriaId!.Value);
        var fornecedor = await fornecedorRepositorio.ObterPorIdAsync(formulario.FornecedorId!.Value);
        if (categoria is null || fornecedor is null)
            return (formulario.Id, ["Escolha uma categoria e um fornecedor válidos."]);
        if (formulario.Ativo && !fornecedor.Ativo)
            return (formulario.Id, ["O fornecedor escolhido está desativado. Escolha um fornecedor ativo."]);

        Produto produto;
        if (formulario.Id == 0)
        {
            var nome = formulario.Nome.Trim();
            produto = new Produto { Slug = GeradorSlug.Unico(nome, await produtoRepositorio.ListarSlugsAsync()) };
            produto.AdicionarVariacao(ProdutoVariacao.NomeUnica, 0); // todo produto nasce com a variação "Único"
            produtoRepositorio.Adicionar(produto);
        }
        else
        {
            produto = await produtoRepositorio.ObterParaEdicaoAsync(formulario.Id)
                      ?? throw new InvalidOperationException("Produto não encontrado.");
            produtoRepositorio.DefinirVersaoOriginal(produto, formulario.Versao);
        }

        produto.Nome = formulario.Nome.Trim();
        produto.DescricaoCurta = formulario.DescricaoCurta.Trim();
        produto.Descricao = formulario.Descricao.Trim();
        produto.CategoriaId = categoria.Id;
        produto.FornecedorId = fornecedor.Id;
        produto.Animal = formulario.Animal;
        produto.Preco = formulario.Preco!.Value;
        produto.PrecoPromocional = formulario.PrecoPromocional;
        produto.Destaque = formulario.Destaque;
        produto.Ativo = formulario.Ativo;
        produto.DefinirImagemPrincipal(formulario.ImagemUrl.Trim(), formulario.ImagemTexto.Trim());

        var erros = produto.ValidarRegras().ToList();
        if (erros.Count > 0)
            return (formulario.Id, erros);

        try
        {
            await produtoRepositorio.SalvarAsync();
            return (produto.Id, []);
        }
        catch (DbUpdateConcurrencyException)
        {
            return (formulario.Id, [MensagemConcorrencia]);
        }
    }

    public async Task<List<string>> SalvarVariacaoAsync(int produtoId, VariacaoFormulario formulario)
    {
        var produto = await produtoRepositorio.ObterParaEdicaoAsync(produtoId);
        if (produto is null)
            return ["Produto não encontrado."];

        var nome = formulario.Nome.Trim();
        if (produto.Variacoes.Any(v => v.Id != formulario.Id && string.Equals(v.Nome, nome, StringComparison.OrdinalIgnoreCase)))
            return ["Este produto já tem uma variação com esse nome."];

        if (formulario.Id == 0)
        {
            produto.AdicionarVariacao(nome, formulario.AjustePreco).Ativa = formulario.Ativa;
        }
        else
        {
            var variacao = produto.Variacoes.FirstOrDefault(v => v.Id == formulario.Id);
            if (variacao is null)
                return ["Variação não encontrada."];

            variacao.Nome = nome;
            variacao.AjustePreco = formulario.AjustePreco;
            variacao.Ativa = formulario.Ativa;
        }

        // Ex.: desativar a última variação ativa de um produto ativo é recusado.
        var erros = produto.ValidarRegras().ToList();
        if (erros.Count > 0)
            return erros;

        await produtoRepositorio.SalvarAsync();
        return [];
    }
}
