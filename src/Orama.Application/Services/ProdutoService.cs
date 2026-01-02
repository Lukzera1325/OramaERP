using Microsoft.EntityFrameworkCore;
using Orama.Domain.Entities;
using Orama.Infra.Data.Context;

namespace Orama.Application.Services;

/// <summary>
/// Serviço para gerenciamento de produtos
/// </summary>
public class ProdutoService : IProdutoService
{
    private readonly OramaDbContext _context;

    public ProdutoService(OramaDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtém todos os produtos ativos
    /// </summary>
    public async Task<IEnumerable<Produto>> ObterTodosAsync()
    {
        return await _context.Produtos
            .Include(p => p.CategoriaNavigation)
            .Where(p => p.Ativo)
            .OrderBy(p => p.Descricao)
            .ToListAsync();
    }

    /// <summary>
    /// Obtém todos os produtos ativos por empresa
    /// </summary>
    public async Task<IEnumerable<Produto>> ObterTodosAsync(int empresaId)
    {
        return await _context.Produtos
            .Include(p => p.CategoriaNavigation)
            .Where(p => p.Ativo && p.EmpresaId == empresaId)
            .OrderBy(p => p.Descricao)
            .ToListAsync();
    }

    /// <summary>
    /// Obtém um produto por ID
    /// </summary>
    public async Task<Produto?> ObterPorIdAsync(int id)
    {
        return await _context.Produtos
            .Include(p => p.CategoriaNavigation)
            .FirstOrDefaultAsync(p => p.Id == id && p.Ativo);
    }

    /// <summary>
    /// Obtém um produto por ID e empresa
    /// </summary>
    public async Task<Produto?> ObterPorIdAsync(int id, int empresaId)
    {
        return await _context.Produtos
            .Include(p => p.CategoriaNavigation)
            .FirstOrDefaultAsync(p => p.Id == id && p.EmpresaId == empresaId && p.Ativo);
    }

    /// <summary>
    /// Obtém um produto por código
    /// </summary>
    public async Task<Produto?> ObterPorCodigoAsync(string codigo)
    {
        return await _context.Produtos
            .Include(p => p.CategoriaNavigation)
            .FirstOrDefaultAsync(p => p.Codigo.ToUpper() == codigo.ToUpper() && p.Ativo);
    }

    /// <summary>
    /// Cria um novo produto
    /// </summary>
    public async Task<Produto> CriarAsync(Produto produto)
    {
        // Verificar se código já existe
        if (await CodigoExisteAsync(produto.Codigo))
        {
            throw new InvalidOperationException("Código já está em uso por outro produto.");
        }

        // Converter código para maiúsculo
        produto.Codigo = produto.Codigo.ToUpper();
        
        // Calcular margem de lucro se não foi informada
        if (produto.MargemLucro == 0 && produto.PrecoCusto > 0 && produto.PrecoVenda > 0)
        {
            produto.MargemLucro = ((produto.PrecoVenda - produto.PrecoCusto) / produto.PrecoCusto) * 100;
        }
        
        _context.Produtos.Add(produto);
        await _context.SaveChangesAsync();
        
        return produto;
    }

    /// <summary>
    /// Atualiza um produto existente
    /// </summary>
    public async Task<Produto> AtualizarAsync(Produto produto)
    {
        var produtoExistente = await _context.Produtos.FindAsync(produto.Id);
        if (produtoExistente == null)
        {
            throw new InvalidOperationException("Produto não encontrado.");
        }

        // Verificar se código já existe (exceto para o próprio produto)
        if (await CodigoExisteAsync(produto.Codigo, produto.Id))
        {
            throw new InvalidOperationException("Código já está em uso por outro produto.");
        }

        // Atualizar propriedades
        produtoExistente.Codigo = produto.Codigo.ToUpper();
        produtoExistente.Descricao = produto.Descricao;
        produtoExistente.DescricaoDetalhada = produto.DescricaoDetalhada;
        produtoExistente.Unidade = produto.Unidade;
        produtoExistente.Ncm = produto.Ncm;
        produtoExistente.Cest = produto.Cest;
        produtoExistente.PrecoCusto = produto.PrecoCusto;
        produtoExistente.PrecoVenda = produto.PrecoVenda;
        produtoExistente.MargemLucro = produto.MargemLucro;
        produtoExistente.ControlaEstoque = produto.ControlaEstoque;
        produtoExistente.EstoqueMinimo = produto.EstoqueMinimo;
        produtoExistente.EstoqueMaximo = produto.EstoqueMaximo;
        produtoExistente.Peso = produto.Peso;
        produtoExistente.Altura = produto.Altura;
        produtoExistente.Largura = produto.Largura;
        produtoExistente.Profundidade = produto.Profundidade;
        produtoExistente.CategoriaId = produto.CategoriaId;
        produtoExistente.Observacoes = produto.Observacoes;

        // Recalcular margem se necessário
        if (produtoExistente.MargemLucro == 0 && produtoExistente.PrecoCusto > 0 && produtoExistente.PrecoVenda > 0)
        {
            produtoExistente.MargemLucro = ((produtoExistente.PrecoVenda - produtoExistente.PrecoCusto) / produtoExistente.PrecoCusto) * 100;
        }

        await _context.SaveChangesAsync();
        return produtoExistente;
    }

    /// <summary>
    /// Exclui um produto (soft delete)
    /// </summary>
    public async Task ExcluirAsync(int id)
    {
        var produto = await _context.Produtos.FindAsync(id);
        if (produto != null)
        {
            produto.Ativo = false;
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Verifica se um código já está em uso
    /// </summary>
    public async Task<bool> CodigoExisteAsync(string codigo, int? produtoId = null)
    {
        var query = _context.Produtos.Where(p => p.Codigo.ToUpper() == codigo.ToUpper() && p.Ativo);
        
        if (produtoId.HasValue)
        {
            query = query.Where(p => p.Id != produtoId.Value);
        }

        return await query.AnyAsync();
    }

    /// <summary>
    /// Busca produtos por código ou descrição
    /// </summary>
    public async Task<IEnumerable<Produto>> BuscarAsync(string termo)
    {
        if (string.IsNullOrWhiteSpace(termo))
            return await ObterTodosAsync();

        var termoUpper = termo.ToUpper();

        return await _context.Produtos
            .Include(p => p.CategoriaNavigation)
            .Where(p => p.Ativo && (
                p.Codigo.ToUpper().Contains(termoUpper) ||
                p.Descricao.ToUpper().Contains(termoUpper) ||
                (p.DescricaoDetalhada != null && p.DescricaoDetalhada.ToUpper().Contains(termoUpper))
            ))
            .OrderBy(p => p.Descricao)
            .ToListAsync();
    }

    /// <summary>
    /// Obtém produtos com estoque baixo
    /// </summary>
    public async Task<IEnumerable<Produto>> ObterProdutosEstoqueBaixoAsync()
    {
        return await _context.Produtos
            .Include(p => p.CategoriaNavigation)
            .Where(p => p.Ativo && p.ControlaEstoque && p.EstoqueAtual <= p.EstoqueMinimo)
            .OrderBy(p => p.Descricao)
            .ToListAsync();
    }

    /// <summary>
    /// Atualiza estoque de um produto
    /// </summary>
    public async Task AtualizarEstoqueAsync(int produtoId, decimal quantidade, string tipoMovimento)
    {
        var produto = await _context.Produtos.FindAsync(produtoId);
        if (produto == null || !produto.ControlaEstoque)
            return;

        switch (tipoMovimento.ToUpper())
        {
            case "ENTRADA":
                produto.EstoqueAtual += quantidade;
                break;
            case "SAIDA":
                produto.EstoqueAtual -= quantidade;
                if (produto.EstoqueAtual < 0)
                    produto.EstoqueAtual = 0;
                break;
            case "AJUSTE":
                produto.EstoqueAtual = quantidade;
                break;
        }

        await _context.SaveChangesAsync();
    }
}

/// <summary>
/// Serviço para gerenciamento de categorias
/// </summary>
public class CategoriaService : ICategoriaService
{
    private readonly OramaDbContext _context;

    public CategoriaService(OramaDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtém todas as categorias ativas
    /// </summary>
    public async Task<IEnumerable<Categoria>> ObterTodosAsync()
    {
        return await _context.Categorias
            .Where(c => c.Ativo)
            .OrderBy(c => c.Nome)
            .ToListAsync();
    }

    /// <summary>
    /// Obtém uma categoria por ID
    /// </summary>
    public async Task<Categoria?> ObterPorIdAsync(int id)
    {
        return await _context.Categorias
            .FirstOrDefaultAsync(c => c.Id == id && c.Ativo);
    }

    /// <summary>
    /// Cria uma nova categoria
    /// </summary>
    public async Task<Categoria> CriarAsync(Categoria categoria)
    {
        _context.Categorias.Add(categoria);
        await _context.SaveChangesAsync();
        return categoria;
    }

    /// <summary>
    /// Atualiza uma categoria existente
    /// </summary>
    public async Task<Categoria> AtualizarAsync(Categoria categoria)
    {
        var categoriaExistente = await _context.Categorias.FindAsync(categoria.Id);
        if (categoriaExistente == null)
        {
            throw new InvalidOperationException("Categoria não encontrada.");
        }

        categoriaExistente.Nome = categoria.Nome;
        categoriaExistente.Descricao = categoria.Descricao;

        await _context.SaveChangesAsync();
        return categoriaExistente;
    }

    /// <summary>
    /// Exclui uma categoria (soft delete)
    /// </summary>
    public async Task ExcluirAsync(int id)
    {
        var categoria = await _context.Categorias.FindAsync(id);
        if (categoria != null)
        {
            categoria.Ativo = false;
            await _context.SaveChangesAsync();
        }
    }
}