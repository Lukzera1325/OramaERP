using Microsoft.EntityFrameworkCore;
using Orama.Domain.Entities;
using Orama.Infra.Data.Context;

namespace Orama.Application.Services;

/// <summary>
/// Service para gerenciar estruturas de produto (BOM)
/// Um método = um caso de uso
/// </summary>
public class EstruturaProdutoService : IEstruturaProdutoService
{
    private readonly OramaDbContext _context;

    public EstruturaProdutoService(OramaDbContext context)
    {
        _context = context;
    }

    // Caso de uso: Obter estrutura de um produto
    public async Task<IEnumerable<EstruturaProduto>> ObterEstruturaPorProdutoAsync(int produtoId, int empresaId)
    {
        return await _context.EstruturasProdutos
            .Include(e => e.ProdutoPai)
            .Include(e => e.ProdutoComponente)
            .Where(e => e.ProdutoPaiId == produtoId && e.EmpresaId == empresaId)
            .OrderBy(e => e.ProdutoComponente.Descricao)
            .ToListAsync();
    }

    // Caso de uso: Obter estrutura por ID
    public async Task<EstruturaProduto?> ObterPorIdAsync(int id, int empresaId)
    {
        return await _context.EstruturasProdutos
            .Include(e => e.ProdutoPai)
            .Include(e => e.ProdutoComponente)
            .FirstOrDefaultAsync(e => e.Id == id && e.EmpresaId == empresaId);
    }

    // Caso de uso: Listar todas as estruturas
    public async Task<IEnumerable<EstruturaProduto>> ObterTodosAsync(int empresaId)
    {
        return await _context.EstruturasProdutos
            .Include(e => e.ProdutoPai)
            .Include(e => e.ProdutoComponente)
            .Where(e => e.EmpresaId == empresaId)
            .OrderBy(e => e.ProdutoPai.Descricao)
            .ThenBy(e => e.ProdutoComponente.Descricao)
            .ToListAsync();
    }

    // Caso de uso: Criar nova estrutura
    public async Task<EstruturaProduto> CriarAsync(EstruturaProduto estrutura)
    {
        // Validar se não existe duplicata
        var existe = await _context.EstruturasProdutos
            .AnyAsync(e => e.ProdutoPaiId == estrutura.ProdutoPaiId && 
                          e.ProdutoComponenteId == estrutura.ProdutoComponenteId &&
                          e.EmpresaId == estrutura.EmpresaId);

        if (existe)
            throw new InvalidOperationException("Este componente já está na estrutura do produto");

        // Validar se não é circular (produto não pode ser componente de si mesmo)
        if (estrutura.ProdutoPaiId == estrutura.ProdutoComponenteId)
            throw new InvalidOperationException("Produto não pode ser componente de si mesmo");

        _context.EstruturasProdutos.Add(estrutura);
        await _context.SaveChangesAsync();

        return estrutura;
    }

    // Caso de uso: Atualizar estrutura
    public async Task<EstruturaProduto> AtualizarAsync(EstruturaProduto estrutura)
    {
        var estruturaExistente = await _context.EstruturasProdutos
            .FirstOrDefaultAsync(e => e.Id == estrutura.Id && e.EmpresaId == estrutura.EmpresaId);

        if (estruturaExistente == null)
            throw new ArgumentException("Estrutura não encontrada");

        estruturaExistente.QuantidadeNecessaria = estrutura.QuantidadeNecessaria;
        estruturaExistente.Unidade = estrutura.Unidade;
        estruturaExistente.Observacoes = estrutura.Observacoes;

        await _context.SaveChangesAsync();
        return estruturaExistente;
    }

    // Caso de uso: Excluir estrutura
    public async Task<bool> ExcluirAsync(int id, int empresaId)
    {
        var estrutura = await _context.EstruturasProdutos
            .FirstOrDefaultAsync(e => e.Id == id && e.EmpresaId == empresaId);

        if (estrutura == null) return false;

        _context.EstruturasProdutos.Remove(estrutura);
        await _context.SaveChangesAsync();

        return true;
    }

    // Caso de uso: Verificar se produto tem estrutura
    public async Task<bool> ProdutoTemEstruturaAsync(int produtoId, int empresaId)
    {
        return await _context.EstruturasProdutos
            .AnyAsync(e => e.ProdutoPaiId == produtoId && e.EmpresaId == empresaId);
    }

    // Caso de uso: Validar estrutura completa
    public async Task<(bool Valida, List<string> Erros)> ValidarEstruturaAsync(int produtoId, int empresaId)
    {
        var erros = new List<string>();

        var estrutura = await ObterEstruturaPorProdutoAsync(produtoId, empresaId);
        
        if (!estrutura.Any())
        {
            erros.Add("Produto não possui estrutura definida");
        }

        foreach (var item in estrutura)
        {
            if (item.QuantidadeNecessaria <= 0)
            {
                erros.Add($"Quantidade inválida para componente {item.ProdutoComponente.Descricao}");
            }

            if (!item.ProdutoComponente.PodeSerUsadoNaProducao())
            {
                erros.Add($"Componente {item.ProdutoComponente.Descricao} não pode ser usado na produção");
            }
        }

        return (erros.Count == 0, erros);
    }
}