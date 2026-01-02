using Microsoft.EntityFrameworkCore;
using Orama.Domain.Entities;
using Orama.Infra.Data.Context;

namespace Orama.Application.Services;

/// <summary>
/// Service para Ordens de Produção Industrial
/// 
/// Responsabilidades:
/// - Gerenciar o ciclo de vida das ordens de produção
/// - Coordenar integração com estoque
/// - Aplicar regras de negócio através da entidade OrdemProducao
/// 
/// Padrão: Buscar → Chamar Entidade → Persistir
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

    #region Consultas

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

    #endregion

    #region Operações de Produção

    /// <summary>
    /// Cria uma nova ordem de produção
    /// 
    /// Processo:
    /// 1. Valida produto e estrutura
    /// 2. Verifica estoque dos componentes
    /// 3. Gera número da ordem
    /// 4. Calcula custos
    /// 5. Cria itens baseado na estrutura de produtos
    /// </summary>
    public async Task<OrdemProducao> CriarAsync(int produtoId, decimal quantidade, string? observacoes, int empresaId, int usuarioId)
    {
        // Buscar produto e estrutura de produção
        var produto = await BuscarProdutoAsync(produtoId, empresaId);
        var estruturaProduto = await BuscarEstruturaProdutoAsync(produtoId, empresaId);

        // Validar usando regras da entidade
        var (ehValida, mensagensErro) = OrdemProducao.ValidarCriacaoOrdemProducao(produto, quantidade, estruturaProduto);
        if (!ehValida)
            throw new InvalidOperationException(string.Join("; ", mensagensErro));

        // Gerar número sequencial da ordem
        var proximoNumero = await ObterProximoNumeroSequencialAsync(empresaId);
        var numeroOrdem = OrdemProducao.GerarNumeroOrdemProducao(empresaId, proximoNumero);

        // Criar ordem com dados calculados
        var ordem = new OrdemProducao
        {
            EmpresaId = empresaId,
            Numero = numeroOrdem,
            ProdutoId = produtoId,
            QuantidadePlanejada = quantidade,
            Observacoes = observacoes,
            UsuarioCriacaoId = usuarioId,
            DataPlanejada = DateTime.Now.AddDays(1), // Padrão: produzir no próximo dia útil
            CustoMaterial = OrdemProducao.CalcularCustoProducao(quantidade, estruturaProduto)
        };

        _context.OrdensProducao.Add(ordem);
        await _context.SaveChangesAsync();

        // Criar itens (componentes) da ordem
        var itensOrdem = ordem.CriarItensProducao(quantidade, estruturaProduto);
        _context.OrdemProducaoItens.AddRange(itensOrdem);
        await _context.SaveChangesAsync();

        return ordem;
    }

    /// <summary>
    /// Libera ordem para produção
    /// Transição: Planejada → Liberada
    /// </summary>
    public async Task<bool> LiberarAsync(int id, int empresaId, int usuarioId)
    {
        var ordem = await BuscarOrdemAsync(id, empresaId);
        if (ordem == null) return false;

        // Aplicar regra de negócio através da entidade
        ordem.LiberarParaProducao();

        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Inicia a produção
    /// Transição: Liberada → Em Andamento
    /// </summary>
    public async Task<bool> IniciarAsync(int id, int empresaId, int usuarioId)
    {
        var ordem = await BuscarOrdemAsync(id, empresaId);
        if (ordem == null) return false;

        // Aplicar regra de negócio através da entidade
        ordem.IniciarProducao();

        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Finaliza a produção e integra com estoque
    /// Transição: Em Andamento → Finalizada
    /// 
    /// Efeitos:
    /// - Finaliza a ordem (via entidade)
    /// - Dá baixa nos componentes no estoque
    /// - Dá entrada do produto acabado no estoque
    /// </summary>
    public async Task<bool> FinalizarAsync(int id, decimal quantidadeProduzida, int empresaId, int usuarioId)
    {
        var ordem = await BuscarOrdemComItensAsync(id, empresaId);
        if (ordem == null) return false;

        // Finalizar produção (regra de negócio na entidade)
        ordem.FinalizarProducao(quantidadeProduzida);

        // Integrar com estoque: baixa componentes + entrada produto acabado
        await ProcessarMovimentacaoEstoqueAsync(ordem, empresaId, usuarioId);

        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Cancela a ordem de produção
    /// Transição: Qualquer Status (exceto Finalizada) → Cancelada
    /// </summary>
    public async Task<bool> CancelarAsync(int id, string motivo, int empresaId, int usuarioId)
    {
        var ordem = await BuscarOrdemAsync(id, empresaId);
        if (ordem == null) return false;

        // Aplicar regra de negócio através da entidade
        ordem.CancelarOrdemProducao(motivo);

        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Calcula custo total de uma ordem finalizada
    /// Baseado nas quantidades realmente consumidas
    /// </summary>
    public async Task<decimal> CalcularCustoProducaoAsync(int ordemProducaoId, int empresaId)
    {
        var ordem = await _context.OrdensProducao
            .Include(o => o.Itens)
                .ThenInclude(i => i.Produto)
            .FirstOrDefaultAsync(o => o.Id == ordemProducaoId && o.EmpresaId == empresaId);

        if (ordem == null) return 0;

        return ordem.Itens.Sum(item => item.CustoTotal);
    }

    #endregion

    #region Métodos Privados - Operações de Apoio

    /// <summary>
    /// Busca produto validando empresa
    /// </summary>
    private async Task<Produto> BuscarProdutoAsync(int produtoId, int empresaId)
    {
        var produto = await _context.Produtos
            .FirstOrDefaultAsync(p => p.Id == produtoId && p.EmpresaId == empresaId);

        return produto ?? throw new ArgumentException("Produto não encontrado");
    }

    /// <summary>
    /// Busca estrutura de produtos (BOM) com componentes
    /// </summary>
    private async Task<List<EstruturaProduto>> BuscarEstruturaProdutoAsync(int produtoId, int empresaId)
    {
        return await _context.EstruturasProdutos
            .Include(e => e.ProdutoComponente)
            .Where(e => e.ProdutoPaiId == produtoId && e.EmpresaId == empresaId)
            .ToListAsync();
    }

    /// <summary>
    /// Busca ordem de produção validando empresa
    /// </summary>
    private async Task<OrdemProducao?> BuscarOrdemAsync(int id, int empresaId)
    {
        return await _context.OrdensProducao
            .FirstOrDefaultAsync(o => o.Id == id && o.EmpresaId == empresaId);
    }

    /// <summary>
    /// Busca ordem com itens carregados (para operações de finalização)
    /// </summary>
    private async Task<OrdemProducao?> BuscarOrdemComItensAsync(int id, int empresaId)
    {
        return await _context.OrdensProducao
            .Include(o => o.Produto)
            .Include(o => o.Itens)
                .ThenInclude(i => i.Produto)
            .FirstOrDefaultAsync(o => o.Id == id && o.EmpresaId == empresaId);
    }

    /// <summary>
    /// Processa movimentação de estoque ao finalizar produção
    /// 
    /// Operações:
    /// 1. Saída: Componentes consumidos (baixa do estoque)
    /// 2. Entrada: Produto acabado produzido (entrada no estoque)
    /// </summary>
    private async Task ProcessarMovimentacaoEstoqueAsync(OrdemProducao ordem, int empresaId, int usuarioId)
    {
        var historicoMovimentacao = $"Produção OP {ordem.Numero}";

        // 1. Dar baixa nos componentes consumidos
        foreach (var item in ordem.Itens)
        {
            await _estoqueService.SaidaEstoqueAsync(
                item.ProdutoId, 
                item.QuantidadeConsumida, 
                historicoMovimentacao, 
                empresaId, 
                usuarioId);
        }

        // 2. Dar entrada no produto acabado
        await _estoqueService.EntradaEstoqueAsync(
            ordem.ProdutoId, 
            ordem.QuantidadeProduzida, 
            historicoMovimentacao, 
            empresaId, 
            usuarioId);
    }

    /// <summary>
    /// Obtém próximo número sequencial para geração da ordem
    /// </summary>
    private async Task<int> ObterProximoNumeroSequencialAsync(int empresaId)
    {
        var ultimaOrdem = await _context.OrdensProducao
            .Where(o => o.EmpresaId == empresaId)
            .OrderByDescending(o => o.Id)
            .FirstOrDefaultAsync();

        return ultimaOrdem?.Id + 1 ?? 1;
    }

    #endregion
}