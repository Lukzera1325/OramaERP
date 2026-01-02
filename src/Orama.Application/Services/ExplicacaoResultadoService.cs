using Microsoft.EntityFrameworkCore;
using Orama.Domain.Entities;
using Orama.Infra.Data.Context;

namespace Orama.Application.Services;

/// <summary>
/// Service para Explicação de Resultados
/// 
/// Responsabilidades:
/// - Gerar explicações claras sobre lucros e prejuízos
/// - Aplicar regras simples e determinísticas
/// - Coordenar persistência das explicações
/// 
/// Filosofia: Clareza > Sofisticação
/// Padrão: Buscar → Aplicar Regras Simples → Persistir
/// </summary>
public class ExplicacaoResultadoService : IExplicacaoResultadoService
{
    private readonly OramaDbContext _context;

    // Constantes para regras simples
    private const decimal MARGEM_MINIMA_RECOMENDADA = 10m; // 10%
    private const decimal DESCONTO_ALTO_LIMITE = 15m; // 15%

    public ExplicacaoResultadoService(OramaDbContext context)
    {
        _context = context;
    }

    #region Geração de Explicações

    /// <summary>
    /// Gera explicações para uma venda faturada
    /// Chamado automaticamente após calcular margem da venda
    /// </summary>
    public async Task GerarExplicacoesVendaAsync(int vendaId, int empresaId)
    {
        // Buscar venda com dados necessários
        var venda = await _context.Vendas
            .Include(v => v.Cliente)
            .Include(v => v.Itens)
                .ThenInclude(i => i.Produto)
            .FirstOrDefaultAsync(v => v.Id == vendaId && v.EmpresaId == empresaId);

        if (venda == null || !venda.MargemCalculada)
            return;

        // Verificar se já existem explicações para esta venda (evitar duplicação)
        var explicacoesExistentes = await _context.ExplicacoesResultados
            .Where(e => e.VendaId == vendaId && e.EmpresaId == empresaId)
            .ToListAsync();

        if (explicacoesExistentes.Any())
            return; // Já foram geradas

        var explicacoes = new List<ExplicacaoResultado>();

        // Regra 1: Analisar resultado geral da venda
        var explicacaoGeral = AnalisarResultadoGeralVenda(venda, empresaId);
        if (explicacaoGeral != null)
            explicacoes.Add(explicacaoGeral);

        // Regra 2: Analisar produtos vendidos abaixo do custo
        var explicacoesProdutos = AnalisarProdutosAbaixoCusto(venda, empresaId);
        explicacoes.AddRange(explicacoesProdutos);

        // Regra 3: Analisar impacto de descontos
        var explicacaoDesconto = AnalisarImpactoDescontos(venda, empresaId);
        if (explicacaoDesconto != null)
            explicacoes.Add(explicacaoDesconto);

        // Persistir explicações
        if (explicacoes.Any())
        {
            _context.ExplicacoesResultados.AddRange(explicacoes);
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Analisa o resultado geral da venda e gera explicação apropriada
    /// </summary>
    private ExplicacaoResultado? AnalisarResultadoGeralVenda(Venda venda, int empresaId)
    {
        // Regra simples 1: Prejuízo - preço abaixo do custo
        if (venda.LucroTotal < 0)
        {
            var precoMedio = venda.Itens.Any() ? venda.Itens.Average(i => i.PrecoUnitario) : 0;
            var custoMedio = venda.Itens.Any() ? venda.Itens.Average(i => i.CustoUnitario) : 0;
            
            return ExplicacaoResultado.CriarExplicacaoPrecoAbaixoCusto(venda, empresaId, precoMedio, custoMedio);
        }

        // Regra simples 2: Margem baixa (positiva, mas abaixo do recomendado)
        if (venda.MargemPercentual > 0 && venda.MargemPercentual < MARGEM_MINIMA_RECOMENDADA)
        {
            return ExplicacaoResultado.CriarExplicacaoMargemBaixa(venda, empresaId, MARGEM_MINIMA_RECOMENDADA);
        }

        // Regra simples 3: Lucro positivo e margem saudável
        if (venda.LucroTotal > 0 && venda.MargemPercentual >= MARGEM_MINIMA_RECOMENDADA)
        {
            return ExplicacaoResultado.CriarExplicacaoLucroPositivo(venda, empresaId);
        }

        return null;
    }

    /// <summary>
    /// Analisa produtos vendidos abaixo do custo e gera explicações específicas
    /// </summary>
    private List<ExplicacaoResultado> AnalisarProdutosAbaixoCusto(Venda venda, int empresaId)
    {
        var explicacoes = new List<ExplicacaoResultado>();

        foreach (var item in venda.Itens)
        {
            // Regra simples: produto vendido abaixo do custo unitário
            if (item.PrecoUnitario < item.CustoUnitario)
            {
                var explicacao = ExplicacaoResultado.CriarExplicacaoProdutoAbaixoCusto(
                    venda, item.Produto, item, empresaId);
                explicacoes.Add(explicacao);
            }
        }

        return explicacoes;
    }

    /// <summary>
    /// Analisa o impacto de descontos na margem
    /// </summary>
    private ExplicacaoResultado? AnalisarImpactoDescontos(Venda venda, int empresaId)
    {
        // Regra simples: desconto alto que impacta a margem
        if (venda.ValorDesconto > 0)
        {
            var percentualDesconto = venda.SubTotal > 0 ? (venda.ValorDesconto / venda.SubTotal) * 100 : 0;
            
            if (percentualDesconto >= DESCONTO_ALTO_LIMITE)
            {
                return ExplicacaoResultado.CriarExplicacaoDescontoExcessivo(venda, empresaId, venda.ValorDesconto);
            }
        }

        return null;
    }

    #endregion

    #region Consultas de Explicações

    /// <summary>
    /// Obtém todas as explicações de uma venda
    /// </summary>
    public async Task<IEnumerable<ExplicacaoResultado>> ObterExplicacoesVendaAsync(int vendaId, int empresaId)
    {
        return await _context.ExplicacoesResultados
            .Include(e => e.Venda)
                .ThenInclude(v => v.Cliente)
            .Include(e => e.Produto)
            .Where(e => e.VendaId == vendaId && e.EmpresaId == empresaId)
            .OrderBy(e => e.Tipo) // Prejuízo primeiro, depois Atenção, depois Lucro
            .ThenBy(e => e.DataExplicacao)
            .ToListAsync();
    }

    /// <summary>
    /// Obtém explicações por tipo de resultado
    /// </summary>
    public async Task<IEnumerable<ExplicacaoResultado>> ObterExplicacoesPorTipoAsync(
        TipoResultado tipo, 
        int empresaId, 
        DateTime? dataInicio = null, 
        DateTime? dataFim = null)
    {
        var query = _context.ExplicacoesResultados
            .Include(e => e.Venda)
                .ThenInclude(v => v.Cliente)
            .Include(e => e.Produto)
            .Where(e => e.EmpresaId == empresaId && e.Tipo == tipo);

        if (dataInicio.HasValue)
            query = query.Where(e => e.DataExplicacao >= dataInicio.Value);

        if (dataFim.HasValue)
            query = query.Where(e => e.DataExplicacao <= dataFim.Value);

        return await query
            .OrderByDescending(e => e.DataExplicacao)
            .ToListAsync();
    }

    /// <summary>
    /// Obtém explicações por categoria
    /// </summary>
    public async Task<IEnumerable<ExplicacaoResultado>> ObterExplicacoesPorCategoriaAsync(
        CategoriaExplicacao categoria, 
        int empresaId, 
        DateTime? dataInicio = null, 
        DateTime? dataFim = null)
    {
        var query = _context.ExplicacoesResultados
            .Include(e => e.Venda)
                .ThenInclude(v => v.Cliente)
            .Include(e => e.Produto)
            .Where(e => e.EmpresaId == empresaId && e.Categoria == categoria);

        if (dataInicio.HasValue)
            query = query.Where(e => e.DataExplicacao >= dataInicio.Value);

        if (dataFim.HasValue)
            query = query.Where(e => e.DataExplicacao <= dataFim.Value);

        return await query
            .OrderByDescending(e => e.DataExplicacao)
            .ToListAsync();
    }

    /// <summary>
    /// Obtém explicação por ID
    /// </summary>
    public async Task<ExplicacaoResultado?> ObterExplicacaoPorIdAsync(int id, int empresaId)
    {
        return await _context.ExplicacoesResultados
            .Include(e => e.Venda)
                .ThenInclude(v => v.Cliente)
            .Include(e => e.Produto)
            .FirstOrDefaultAsync(e => e.Id == id && e.EmpresaId == empresaId);
    }

    /// <summary>
    /// Obtém resumo de explicações por período
    /// </summary>
    public async Task<dynamic> ObterResumoExplicacoesAsync(
        DateTime dataInicio, 
        DateTime dataFim, 
        int empresaId)
    {
        var explicacoes = await _context.ExplicacoesResultados
            .Where(e => e.EmpresaId == empresaId &&
                       e.DataExplicacao >= dataInicio &&
                       e.DataExplicacao <= dataFim)
            .ToListAsync();

        return new
        {
            TotalExplicacoes = explicacoes.Count,
            ExplicacoesLucro = explicacoes.Count(e => e.Tipo == TipoResultado.Lucro),
            ExplicacoesPrejuizo = explicacoes.Count(e => e.Tipo == TipoResultado.Prejuizo),
            ExplicacoesAtencao = explicacoes.Count(e => e.Tipo == TipoResultado.Atencao),
            
            // Por categoria
            PrecoVsCusto = explicacoes.Count(e => e.Categoria == CategoriaExplicacao.PrecoVsCusto),
            MargemBaixa = explicacoes.Count(e => e.Categoria == CategoriaExplicacao.MargemBaixa),
            DescontoExcessivo = explicacoes.Count(e => e.Categoria == CategoriaExplicacao.DescontoExcessivo),
            
            // Impacto financeiro
            LucroTotalExplicado = explicacoes.Sum(e => e.LucroCalculado),
            MargemMediaExplicada = explicacoes.Any() ? explicacoes.Average(e => e.MargemPercentual) : 0
        };
    }

    #endregion

    #region Análises Específicas

    /// <summary>
    /// Obtém principais motivos de prejuízo no período
    /// </summary>
    public async Task<IEnumerable<dynamic>> ObterPrincipaisMotivosPrejuizoAsync(
        DateTime dataInicio, 
        DateTime dataFim, 
        int empresaId)
    {
        var explicacoesPrejuizo = await _context.ExplicacoesResultados
            .Where(e => e.EmpresaId == empresaId &&
                       e.Tipo == TipoResultado.Prejuizo &&
                       e.DataExplicacao >= dataInicio &&
                       e.DataExplicacao <= dataFim)
            .GroupBy(e => e.Categoria)
            .Select(g => new
            {
                Categoria = g.Key,
                CategoriaDescricao = g.First().CategoriaDescricao,
                Quantidade = g.Count(),
                PrejuizoTotal = g.Sum(e => Math.Abs(e.LucroCalculado)),
                ExemplosResumo = g.Take(3).Select(e => e.ResumoExplicacao).ToList()
            })
            .OrderByDescending(x => x.PrejuizoTotal)
            .ToListAsync();

        return explicacoesPrejuizo;
    }

    /// <summary>
    /// Obtém produtos que mais geram prejuízo
    /// </summary>
    public async Task<IEnumerable<dynamic>> ObterProdutosMaisPrejuizoAsync(
        DateTime dataInicio, 
        DateTime dataFim, 
        int empresaId, 
        int limite = 10)
    {
        var produtosPrejuizo = await _context.ExplicacoesResultados
            .Include(e => e.Produto)
            .Where(e => e.EmpresaId == empresaId &&
                       e.Tipo == TipoResultado.Prejuizo &&
                       e.ProdutoId.HasValue &&
                       e.DataExplicacao >= dataInicio &&
                       e.DataExplicacao <= dataFim)
            .GroupBy(e => new { e.ProdutoId, e.Produto!.Codigo, e.Produto.Descricao })
            .Select(g => new
            {
                ProdutoId = g.Key.ProdutoId,
                Codigo = g.Key.Codigo,
                Descricao = g.Key.Descricao,
                QuantidadeVendas = g.Count(),
                PrejuizoTotal = g.Sum(e => Math.Abs(e.LucroCalculado)),
                UltimaExplicacao = g.OrderByDescending(e => e.DataExplicacao).First().ResumoExplicacao
            })
            .OrderByDescending(x => x.PrejuizoTotal)
            .Take(limite)
            .ToListAsync();

        return produtosPrejuizo;
    }

    /// <summary>
    /// Verifica se uma venda precisa de nova explicação
    /// (para casos de recálculo de margem)
    /// </summary>
    public async Task<bool> VendaPrecisaNovaExplicacaoAsync(int vendaId, int empresaId)
    {
        var venda = await _context.Vendas
            .FirstOrDefaultAsync(v => v.Id == vendaId && v.EmpresaId == empresaId);

        if (venda == null || !venda.MargemCalculada)
            return false;

        var explicacaoExistente = await _context.ExplicacoesResultados
            .Where(e => e.VendaId == vendaId && e.EmpresaId == empresaId)
            .OrderByDescending(e => e.DataExplicacao)
            .FirstOrDefaultAsync();

        // Se não tem explicação, precisa
        if (explicacaoExistente == null)
            return true;

        // Se a margem foi recalculada após a última explicação, precisa de nova
        return venda.DataMargemCalculada > explicacaoExistente.DataExplicacao;
    }

    #endregion
}