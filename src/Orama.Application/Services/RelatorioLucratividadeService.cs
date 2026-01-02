using Microsoft.EntityFrameworkCore;
using Orama.Infra.Data.Context;

namespace Orama.Application.Services;

/// <summary>
/// Service para Relatórios de Lucratividade
/// 
/// Responsabilidades:
/// - Agregar dados já persistidos nas vendas
/// - Gerar relatórios simples sem recriar regras de negócio
/// - Consultas claras e legíveis usando LINQ
/// 
/// Padrão: Buscar dados → Agregar → Retornar
/// </summary>
public class RelatorioLucratividadeService : IRelatorioLucratividadeService
{
    private readonly OramaDbContext _context;

    public RelatorioLucratividadeService(OramaDbContext context)
    {
        _context = context;
    }

    #region Relatório 1: Produtos Mais Lucrativos

    /// <summary>
    /// Obtém relatório de produtos mais lucrativos
    /// Baseado exclusivamente em dados já persistidos nas vendas
    /// </summary>
    public async Task<IEnumerable<ProdutoLucratividade>> ObterProdutosMaisLucrativosAsync(
        DateTime dataInicio, 
        DateTime dataFim, 
        int empresaId, 
        int limite = 20)
    {
        var resultado = await _context.VendaItens
            .Include(vi => vi.Produto)
            .Include(vi => vi.Venda)
            .Where(vi => vi.Venda.EmpresaId == empresaId &&
                        vi.Venda.Status == Domain.Entities.StatusVenda.Faturada &&
                        vi.Venda.MargemCalculada &&
                        vi.Venda.DataVenda >= dataInicio &&
                        vi.Venda.DataVenda <= dataFim &&
                        vi.Venda.Ativo)
            .GroupBy(vi => new { 
                vi.ProdutoId, 
                vi.Produto.Codigo,
                vi.Produto.Descricao 
            })
            .Select(g => new ProdutoLucratividade
            {
                ProdutoId = g.Key.ProdutoId,
                CodigoProduto = g.Key.Codigo,
                NomeProduto = g.Key.Descricao,
                QuantidadeVendida = g.Sum(vi => vi.Quantidade),
                ReceitaTotal = g.Sum(vi => vi.ValorTotal),
                CustoTotal = g.Sum(vi => vi.CustoTotal),
                LucroTotal = g.Sum(vi => vi.ValorTotal - vi.CustoTotal),
                MargemMedia = g.Average(vi => vi.MargemItem)
            })
            .OrderByDescending(p => p.LucroTotal)
            .Take(limite)
            .ToListAsync();

        return resultado;
    }

    #endregion

    #region Relatório 2: Vendas com Margem Negativa

    /// <summary>
    /// Obtém vendas com margem negativa (prejuízo)
    /// Lista vendas onde LucroTotal < 0
    /// </summary>
    public async Task<IEnumerable<VendaMargemNegativa>> ObterVendasMargemNegativaAsync(
        DateTime dataInicio, 
        DateTime dataFim, 
        int empresaId)
    {
        var resultado = await _context.Vendas
            .Include(v => v.Cliente)
            .Include(v => v.Itens)
                .ThenInclude(i => i.Produto)
            .Where(v => v.EmpresaId == empresaId &&
                       v.Status == Domain.Entities.StatusVenda.Faturada &&
                       v.MargemCalculada &&
                       v.LucroTotal < 0 &&
                       v.DataVenda >= dataInicio &&
                       v.DataVenda <= dataFim &&
                       v.Ativo)
            .Select(v => new VendaMargemNegativa
            {
                VendaId = v.Id,
                NumeroVenda = v.Numero,
                DataVenda = v.DataVenda,
                NomeCliente = v.Cliente.Nome,
                ValorVenda = v.ValorTotal,
                CustoTotal = v.CustoTotal,
                LucroNegativo = v.LucroTotal,
                MargemPercentual = v.MargemPercentual,
                ProdutosPrincipais = v.Itens
                    .Where(i => i.MargemItem < 0)
                    .Select(i => i.Produto.Descricao)
                    .Take(3)
                    .ToList()
            })
            .OrderBy(v => v.LucroNegativo) // Maiores prejuízos primeiro
            .ToListAsync();

        return resultado;
    }

    #endregion

    #region Relatório 3: Evolução de Margem no Tempo

    /// <summary>
    /// Obtém evolução de margem por período (mensal)
    /// Mostra tendência de melhoria ou piora da margem
    /// </summary>
    public async Task<IEnumerable<EvolucaoMargem>> ObterEvolucaoMargemMensalAsync(
        DateTime dataInicio, 
        DateTime dataFim, 
        int empresaId)
    {
        var resultado = await _context.Vendas
            .Where(v => v.EmpresaId == empresaId &&
                       v.Status == Domain.Entities.StatusVenda.Faturada &&
                       v.MargemCalculada &&
                       v.DataVenda >= dataInicio &&
                       v.DataVenda <= dataFim &&
                       v.Ativo)
            .GroupBy(v => new { 
                Ano = v.DataVenda.Year, 
                Mes = v.DataVenda.Month 
            })
            .Select(g => new EvolucaoMargem
            {
                Periodo = $"{g.Key.Mes:D2}/{g.Key.Ano}",
                DataReferencia = new DateTime(g.Key.Ano, g.Key.Mes, 1),
                QuantidadeVendas = g.Count(),
                ReceitaTotal = g.Sum(v => v.ValorTotal),
                CustoTotal = g.Sum(v => v.CustoTotal),
                LucroTotal = g.Sum(v => v.LucroTotal),
                MargemMedia = g.Average(v => v.MargemPercentual)
            })
            .OrderBy(e => e.DataReferencia)
            .ToListAsync();

        return resultado;
    }

    /// <summary>
    /// Obtém evolução de margem por período (diário)
    /// Para análises de curto prazo
    /// </summary>
    public async Task<IEnumerable<EvolucaoMargem>> ObterEvolucaoMargemDiariaAsync(
        DateTime dataInicio, 
        DateTime dataFim, 
        int empresaId)
    {
        var resultado = await _context.Vendas
            .Where(v => v.EmpresaId == empresaId &&
                       v.Status == Domain.Entities.StatusVenda.Faturada &&
                       v.MargemCalculada &&
                       v.DataVenda >= dataInicio &&
                       v.DataVenda <= dataFim &&
                       v.Ativo)
            .GroupBy(v => v.DataVenda.Date)
            .Select(g => new EvolucaoMargem
            {
                Periodo = g.Key.ToString("dd/MM/yyyy"),
                DataReferencia = g.Key,
                QuantidadeVendas = g.Count(),
                ReceitaTotal = g.Sum(v => v.ValorTotal),
                CustoTotal = g.Sum(v => v.CustoTotal),
                LucroTotal = g.Sum(v => v.LucroTotal),
                MargemMedia = g.Average(v => v.MargemPercentual)
            })
            .OrderBy(e => e.DataReferencia)
            .ToListAsync();

        return resultado;
    }

    #endregion

    #region Resumos Executivos

    /// <summary>
    /// Obtém resumo executivo de lucratividade por período
    /// KPIs principais para tomada de decisão
    /// </summary>
    public async Task<ResumoLucratividade> ObterResumoLucratividadeAsync(
        DateTime dataInicio, 
        DateTime dataFim, 
        int empresaId)
    {
        var vendas = await _context.Vendas
            .Where(v => v.EmpresaId == empresaId &&
                       v.Status == Domain.Entities.StatusVenda.Faturada &&
                       v.MargemCalculada &&
                       v.DataVenda >= dataInicio &&
                       v.DataVenda <= dataFim &&
                       v.Ativo)
            .ToListAsync();

        if (!vendas.Any())
        {
            return new ResumoLucratividade
            {
                PeriodoInicio = dataInicio,
                PeriodoFim = dataFim,
                TotalVendas = 0,
                ReceitaTotal = 0,
                CustoTotal = 0,
                LucroTotal = 0,
                MargemMedia = 0,
                VendasComLucro = 0,
                VendasComPrejuizo = 0,
                MelhorMargem = 0,
                PiorMargem = 0
            };
        }

        return new ResumoLucratividade
        {
            PeriodoInicio = dataInicio,
            PeriodoFim = dataFim,
            TotalVendas = vendas.Count,
            ReceitaTotal = vendas.Sum(v => v.ValorTotal),
            CustoTotal = vendas.Sum(v => v.CustoTotal),
            LucroTotal = vendas.Sum(v => v.LucroTotal),
            MargemMedia = vendas.Average(v => v.MargemPercentual),
            VendasComLucro = vendas.Count(v => v.LucroTotal > 0),
            VendasComPrejuizo = vendas.Count(v => v.LucroTotal < 0),
            MelhorMargem = vendas.Max(v => v.MargemPercentual),
            PiorMargem = vendas.Min(v => v.MargemPercentual)
        };
    }

    #endregion
}

#region DTOs para Relatórios

/// <summary>
/// DTO para relatório de produtos mais lucrativos
/// </summary>
public class ProdutoLucratividade
{
    public int ProdutoId { get; set; }
    public string CodigoProduto { get; set; } = string.Empty;
    public string NomeProduto { get; set; } = string.Empty;
    public decimal QuantidadeVendida { get; set; }
    public decimal ReceitaTotal { get; set; }
    public decimal CustoTotal { get; set; }
    public decimal LucroTotal { get; set; }
    public decimal MargemMedia { get; set; }

    // Propriedades calculadas
    public decimal MargemPercentualCalculada => ReceitaTotal > 0 ? (LucroTotal / ReceitaTotal) * 100 : 0;
    public bool EhLucrativo => LucroTotal > 0;
}

/// <summary>
/// DTO para relatório de vendas com margem negativa
/// </summary>
public class VendaMargemNegativa
{
    public int VendaId { get; set; }
    public string NumeroVenda { get; set; } = string.Empty;
    public DateTime DataVenda { get; set; }
    public string NomeCliente { get; set; } = string.Empty;
    public decimal ValorVenda { get; set; }
    public decimal CustoTotal { get; set; }
    public decimal LucroNegativo { get; set; }
    public decimal MargemPercentual { get; set; }
    public List<string> ProdutosPrincipais { get; set; } = new();

    // Propriedades calculadas
    public decimal PercentualPrejuizo => ValorVenda > 0 ? Math.Abs(LucroNegativo / ValorVenda) * 100 : 0;
    public string ProdutosPrincipaisTexto => string.Join(", ", ProdutosPrincipais);
}

/// <summary>
/// DTO para relatório de evolução de margem
/// </summary>
public class EvolucaoMargem
{
    public string Periodo { get; set; } = string.Empty;
    public DateTime DataReferencia { get; set; }
    public int QuantidadeVendas { get; set; }
    public decimal ReceitaTotal { get; set; }
    public decimal CustoTotal { get; set; }
    public decimal LucroTotal { get; set; }
    public decimal MargemMedia { get; set; }

    // Propriedades calculadas
    public decimal MargemCalculada => ReceitaTotal > 0 ? (LucroTotal / ReceitaTotal) * 100 : 0;
    public bool PeriodoLucrativo => LucroTotal > 0;
    public decimal TicketMedio => QuantidadeVendas > 0 ? ReceitaTotal / QuantidadeVendas : 0;
}

/// <summary>
/// DTO para resumo executivo de lucratividade
/// </summary>
public class ResumoLucratividade
{
    public DateTime PeriodoInicio { get; set; }
    public DateTime PeriodoFim { get; set; }
    public int TotalVendas { get; set; }
    public decimal ReceitaTotal { get; set; }
    public decimal CustoTotal { get; set; }
    public decimal LucroTotal { get; set; }
    public decimal MargemMedia { get; set; }
    public int VendasComLucro { get; set; }
    public int VendasComPrejuizo { get; set; }
    public decimal MelhorMargem { get; set; }
    public decimal PiorMargem { get; set; }

    // Propriedades calculadas
    public decimal PercentualVendasLucrativas => TotalVendas > 0 ? (decimal)VendasComLucro / TotalVendas * 100 : 0;
    public decimal TicketMedio => TotalVendas > 0 ? ReceitaTotal / TotalVendas : 0;
    public bool PeriodoLucrativo => LucroTotal > 0;
}

#endregion