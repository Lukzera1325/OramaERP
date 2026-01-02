using Microsoft.EntityFrameworkCore;
using Orama.Domain.Entities;
using Orama.Infra.Data.Context;

namespace Orama.Application.Services;

/// <summary>
/// Service SIMPLIFICADO para ordens de produção
/// Busca -> Chama entidade -> Persiste
/// </summary>
public class OrdemProducaoService : IOrdemProducaoService
{
    private readonly OramaDbContext _context;
    private readonly IEstoqueService _estoqueService;

    public OrdemProducaoService(OramaDbContext context, IEstoqueService estoqueService)
    {
        _context = context;
        _estoqueService = estoqueService;
    }

    public async Task<IEnumerable<OrdemProducao>> ObterTodosAsync(int empresaId)
    {
        return await _context.OrdensProducao
            .Include(o => o.Produto)
            .Include(o => o.UsuarioCriacao)
            .Where(o => o.EmpresaId == empresaId)
            .OrderByDescending(o => o.DataCriacao)
            .ToListAsync();
    }

    public async Task<OrdemProducao?> ObterPorIdAsync(int id, int empresaId)
    {
        return await _context.OrdensProducao
            .Include(o => o.Produto)
            .Include(o => o.UsuarioCriacao)
            .Include(o => o.Itens)
                .ThenInclude(i => i.Produto)
            .FirstOrDefaultAsync(o => o.Id == id && o.EmpresaId == empresaId);
    }

    public async Task<IEnumerable<OrdemProducao>> ObterPorStatusAsync(StatusOrdemProducao status, int empresaId)
    {
        return await _context.OrdensProducao
            .Include(o => o.Produto)
            .Where(o => o.Status == status && o.EmpresaId == empresaId)
            .OrderBy(o => o.DataCriacao)
            .ToListAsync();
    }

    public async Task<OrdemProducao> CriarAsync(int produtoId, decimal quantidade, string? observacoes, int empresaId, int usuarioId)
    {
        // Buscar produto e estrutura
        var produto = await _context.Produtos
            .FirstOrDefaultAsync(p => p.Id == produtoId && p.EmpresaId == empresaId);

        if (produto == null)
            throw new ArgumentException("Produto não encontrado");

        var estrutura = await _context.EstruturasProdutos
            .Include(e => e.ProdutoComponente)
            .Where(e => e.ProdutoPaiId == produtoId && e.EmpresaId == empresaId)
            .ToListAsync();

        // Validar usando método da entidade
        var (valida, erros) = OrdemProducao.ValidarCriacao(produto, quantidade, estrutura);
        if (!valida)
            throw new InvalidOperationException(string.Join("; ", erros));

        // Gerar número usando método da entidade
        var proximoNumero = await ObterProximoNumeroAsync(empresaId);
        var numero = OrdemProducao.GerarNumero(empresaId, proximoNumero);

        // Criar ordem
        var ordem = new OrdemProducao
        {
            EmpresaId = empresaId,
            Numero = numero,
            ProdutoId = produtoId,
            QuantidadePlanejada = quantidade,
            Observacoes = observacoes,
            UsuarioCriacaoId = usuarioId,
            DataPlanejada = DateTime.Now.AddDays(1),
            CustoMaterial = OrdemProducao.CalcularCusto(quantidade, estrutura)
        };

        _context.OrdensProducao.Add(ordem);
        await _context.SaveChangesAsync();

        // Criar itens usando método da entidade
        var itens = ordem.CriarItens(quantidade, estrutura);
        _context.OrdemProducaoItens.AddRange(itens);
        await _context.SaveChangesAsync();

        return ordem;
    }

    public async Task<bool> LiberarAsync(int id, int empresaId, int usuarioId)
    {
        var ordem = await _context.OrdensProducao
            .FirstOrDefaultAsync(o => o.Id == id && o.EmpresaId == empresaId);

        if (ordem == null) return false;

        ordem.Liberar();
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IniciarAsync(int id, int empresaId, int usuarioId)
    {
        var ordem = await _context.OrdensProducao
            .FirstOrDefaultAsync(o => o.Id == id && o.EmpresaId == empresaId);

        if (ordem == null) return false;

        ordem.Iniciar();
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> FinalizarAsync(int id, decimal quantidadeProduzida, int empresaId, int usuarioId)
    {
        var ordem = await _context.OrdensProducao
            .Include(o => o.Produto)
            .Include(o => o.Itens)
                .ThenInclude(i => i.Produto)
            .FirstOrDefaultAsync(o => o.Id == id && o.EmpresaId == empresaId);

        if (ordem == null) return false;

        // Finalizar usando método da entidade
        ordem.Finalizar(quantidadeProduzida);

        // Integração com estoque
        await ProcessarEstoqueAsync(ordem, empresaId, usuarioId);

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CancelarAsync(int id, string motivo, int empresaId, int usuarioId)
    {
        var ordem = await _context.OrdensProducao
            .FirstOrDefaultAsync(o => o.Id == id && o.EmpresaId == empresaId);

        if (ordem == null) return false;

        ordem.Cancelar(motivo);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<OrdemProducao>> ObterOrdensAtrasadasAsync(int empresaId)
    {
        var dataLimite = DateTime.Now.AddDays(-7);

        return await _context.OrdensProducao
            .Include(o => o.Produto)
            .Where(o => o.EmpresaId == empresaId && 
                       o.Status == StatusOrdemProducao.EmAndamento &&
                       o.DataCriacao <= dataLimite)
            .OrderBy(o => o.DataCriacao)
            .ToListAsync();
    }

    public async Task<decimal> CalcularCustoProducaoAsync(int ordemProducaoId, int empresaId)
    {
        var ordem = await _context.OrdensProducao
            .Include(o => o.Itens)
                .ThenInclude(i => i.Produto)
            .FirstOrDefaultAsync(o => o.Id == ordemProducaoId && o.EmpresaId == empresaId);

        if (ordem == null) return 0;

        return ordem.Itens.Sum(i => i.CustoTotal);
    }

    // Método privado: Integração com estoque
    private async Task ProcessarEstoqueAsync(OrdemProducao ordem, int empresaId, int usuarioId)
    {
        // Dar baixa nos componentes
        foreach (var item in ordem.Itens)
        {
            await _estoqueService.SaidaEstoqueAsync(
                item.ProdutoId, 
                item.QuantidadeConsumida, 
                $"Produção OP {ordem.Numero}", 
                empresaId, 
                usuarioId);
        }

        // Dar entrada no produto acabado
        await _estoqueService.EntradaEstoqueAsync(
            ordem.ProdutoId, 
            ordem.QuantidadeProduzida, 
            $"Produção OP {ordem.Numero}", 
            empresaId, 
            usuarioId);
    }

    // Método privado: Próximo número
    private async Task<int> ObterProximoNumeroAsync(int empresaId)
    {
        var ultimaOrdem = await _context.OrdensProducao
            .Where(o => o.EmpresaId == empresaId)
            .OrderByDescending(o => o.Id)
            .FirstOrDefaultAsync();

        return ultimaOrdem?.Id + 1 ?? 1;
    }
}