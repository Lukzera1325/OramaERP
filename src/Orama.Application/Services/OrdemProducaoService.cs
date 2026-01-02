using Microsoft.EntityFrameworkCore;
using Orama.Domain.Entities;
using Orama.Domain.Services;
using Orama.Infra.Data.Context;

namespace Orama.Application.Services;

/// <summary>
/// Service para gerenciar ordens de produção
/// Um método = um caso de uso
/// </summary>
public class OrdemProducaoService : IOrdemProducaoService
{
    private readonly OramaDbContext _context;
    private readonly ProducaoProcessingService _producaoService;
    private readonly IEstoqueService _estoqueService;

    public OrdemProducaoService(
        OramaDbContext context, 
        ProducaoProcessingService producaoService,
        IEstoqueService estoqueService)
    {
        _context = context;
        _producaoService = producaoService;
        _estoqueService = estoqueService;
    }

    // Caso de uso: Listar todas as ordens
    public async Task<IEnumerable<OrdemProducao>> ObterTodosAsync(int empresaId)
    {
        return await _context.OrdensProducao
            .Include(o => o.Produto)
            .Include(o => o.UsuarioCriacao)
            .Where(o => o.EmpresaId == empresaId)
            .OrderByDescending(o => o.DataCriacao)
            .ToListAsync();
    }

    // Caso de uso: Obter ordem por ID
    public async Task<OrdemProducao?> ObterPorIdAsync(int id, int empresaId)
    {
        return await _context.OrdensProducao
            .Include(o => o.Produto)
            .Include(o => o.UsuarioCriacao)
            .Include(o => o.Itens)
                .ThenInclude(i => i.Produto)
            .FirstOrDefaultAsync(o => o.Id == id && o.EmpresaId == empresaId);
    }

    // Caso de uso: Obter ordens por status
    public async Task<IEnumerable<OrdemProducao>> ObterPorStatusAsync(StatusOrdemProducao status, int empresaId)
    {
        return await _context.OrdensProducao
            .Include(o => o.Produto)
            .Where(o => o.Status == status && o.EmpresaId == empresaId)
            .OrderBy(o => o.DataCriacao)
            .ToListAsync();
    }

    // Caso de uso: Criar nova ordem de produção
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

        // Validar usando Domain Service
        var (valida, erros) = _producaoService.ValidarOrdemProducao(produto, quantidade, estrutura);
        if (!valida)
            throw new InvalidOperationException(string.Join("; ", erros));

        // Gerar número da OP
        var proximoNumero = await ObterProximoNumeroAsync(empresaId);
        var numero = _producaoService.GerarNumeroOP(empresaId, proximoNumero);

        // Criar ordem
        var ordem = new OrdemProducao
        {
            EmpresaId = empresaId,
            Numero = numero,
            ProdutoId = produtoId,
            QuantidadePlanejada = quantidade,
            Observacoes = observacoes,
            UsuarioCriacaoId = usuarioId,
            DataPlanejada = DateTime.Now.AddDays(1) // Padrão: produzir amanhã
        };

        _context.OrdensProducao.Add(ordem);
        await _context.SaveChangesAsync();

        // Criar itens da ordem
        var itens = _producaoService.CriarItensOrdemProducao(ordem.Id, quantidade, estrutura);
        _context.OrdemProducaoItens.AddRange(itens);
        await _context.SaveChangesAsync();

        return ordem;
    }

    // Caso de uso: Liberar ordem para produção
    public async Task<bool> LiberarAsync(int id, int empresaId, int usuarioId)
    {
        var ordem = await _context.OrdensProducao
            .FirstOrDefaultAsync(o => o.Id == id && o.EmpresaId == empresaId);

        if (ordem == null) return false;

        // Usar método de negócio da entidade
        ordem.Liberar();

        await _context.SaveChangesAsync();
        return true;
    }

    // Caso de uso: Iniciar produção
    public async Task<bool> IniciarAsync(int id, int empresaId, int usuarioId)
    {
        var ordem = await _context.OrdensProducao
            .FirstOrDefaultAsync(o => o.Id == id && o.EmpresaId == empresaId);

        if (ordem == null) return false;

        // Usar método de negócio da entidade
        ordem.Iniciar();

        await _context.SaveChangesAsync();
        return true;
    }

    // Caso de uso: Finalizar produção (com integração ao estoque)
    public async Task<bool> FinalizarAsync(int id, decimal quantidadeProduzida, int empresaId, int usuarioId)
    {
        var ordem = await _context.OrdensProducao
            .Include(o => o.Produto)
            .Include(o => o.Itens)
                .ThenInclude(i => i.Produto)
            .FirstOrDefaultAsync(o => o.Id == id && o.EmpresaId == empresaId);

        if (ordem == null) return false;

        // Usar método de negócio da entidade
        ordem.Finalizar(quantidadeProduzida);

        // INTEGRAÇÃO COM ESTOQUE - Regras no Domain
        await ProcessarMovimentacaoEstoqueAsync(ordem, quantidadeProduzida, empresaId, usuarioId);

        await _context.SaveChangesAsync();
        return true;
    }

    // Caso de uso: Cancelar ordem
    public async Task<bool> CancelarAsync(int id, string motivo, int empresaId, int usuarioId)
    {
        var ordem = await _context.OrdensProducao
            .FirstOrDefaultAsync(o => o.Id == id && o.EmpresaId == empresaId);

        if (ordem == null) return false;

        // Usar método de negócio da entidade
        ordem.Cancelar(motivo);

        await _context.SaveChangesAsync();
        return true;
    }

    // Caso de uso: Calcular custo de produção
    public async Task<decimal> CalcularCustoProducaoAsync(int ordemProducaoId, int empresaId)
    {
        var ordem = await _context.OrdensProducao
            .Include(o => o.Itens)
                .ThenInclude(i => i.Produto)
            .FirstOrDefaultAsync(o => o.Id == ordemProducaoId && o.EmpresaId == empresaId);

        if (ordem == null) return 0;

        return ordem.Itens.Sum(i => i.CustoTotal);
    }

    // Caso de uso: Obter ordens atrasadas
    public async Task<IEnumerable<OrdemProducao>> ObterOrdensAtrasadasAsync(int empresaId)
    {
        var dataLimite = DateTime.Now.AddDays(-7); // Ordens criadas há mais de 7 dias

        return await _context.OrdensProducao
            .Include(o => o.Produto)
            .Where(o => o.EmpresaId == empresaId && 
                       o.Status == StatusOrdemProducao.EmAndamento &&
                       o.DataCriacao <= dataLimite)
            .OrderBy(o => o.DataCriacao)
            .ToListAsync();
    }

    // Método privado: Processar movimentação de estoque
    private async Task ProcessarMovimentacaoEstoqueAsync(OrdemProducao ordem, decimal quantidadeProduzida, int empresaId, int usuarioId)
    {
        // 1. Dar baixa nos componentes
        foreach (var item in ordem.Itens)
        {
            var quantidadeConsumida = (item.QuantidadePlanejada / ordem.QuantidadePlanejada) * quantidadeProduzida;
            
            await _estoqueService.SaidaEstoqueAsync(
                item.ProdutoId, 
                quantidadeConsumida, 
                $"Produção OP {ordem.Numero}", 
                empresaId, 
                usuarioId);

            // Atualizar quantidade consumida no item
            item.QuantidadeConsumida = quantidadeConsumida;
        }

        // 2. Dar entrada no produto acabado
        await _estoqueService.EntradaEstoqueAsync(
            ordem.ProdutoId, 
            quantidadeProduzida, 
            $"Produção OP {ordem.Numero}", 
            empresaId, 
            usuarioId);
    }

    // Método privado: Obter próximo número sequencial
    private async Task<int> ObterProximoNumeroAsync(int empresaId)
    {
        var ultimaOrdem = await _context.OrdensProducao
            .Where(o => o.EmpresaId == empresaId)
            .OrderByDescending(o => o.Id)
            .FirstOrDefaultAsync();

        return ultimaOrdem?.Id + 1 ?? 1;
    }
}