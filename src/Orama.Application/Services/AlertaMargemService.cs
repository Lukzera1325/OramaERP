using Microsoft.EntityFrameworkCore;
using Orama.Domain.Entities;
using Orama.Infra.Data.Context;

namespace Orama.Application.Services;

/// <summary>
/// Service para Alertas de Margem
/// 
/// Responsabilidades:
/// - Gerenciar alertas de margem negativa
/// - Coordenar criação automática de alertas
/// - Permitir resolução manual de alertas
/// 
/// Padrão: Buscar → Aplicar Regra do Domain → Persistir
/// </summary>
public class AlertaMargemService : IAlertaMargemService
{
    private readonly OramaDbContext _context;

    public AlertaMargemService(OramaDbContext context)
    {
        _context = context;
    }

    #region Criação Automática de Alertas

    /// <summary>
    /// Processa alertas para uma venda faturada
    /// Chamado automaticamente após calcular margem da venda
    /// </summary>
    public async Task ProcessarAlertasVendaAsync(int vendaId, int empresaId)
    {
        // Buscar venda com dados necessários
        var venda = await _context.Vendas
            .Include(v => v.Cliente)
            .Include(v => v.Itens)
                .ThenInclude(i => i.Produto)
            .FirstOrDefaultAsync(v => v.Id == vendaId && v.EmpresaId == empresaId);

        if (venda == null || !venda.MargemCalculada)
            return;

        // Buscar produtos para análise
        var produtoIds = venda.Itens.Select(i => i.ProdutoId).ToList();
        var produtos = await _context.Produtos
            .Where(p => produtoIds.Contains(p.Id) && p.EmpresaId == empresaId)
            .ToListAsync();

        // Detectar problemas usando regra do Domain
        var alertasParaCriar = venda.DetectarProblemasDeMargemParaAlertas(produtos, empresaId);

        if (alertasParaCriar.Any())
        {
            // Verificar se já existem alertas para esta venda (evitar duplicação)
            var alertasExistentes = await _context.AlertasMargem
                .Where(a => a.VendaId == vendaId && a.EmpresaId == empresaId)
                .ToListAsync();

            // Filtrar apenas alertas novos
            var alertasNovos = alertasParaCriar
                .Where(novo => !alertasExistentes.Any(existente => 
                    existente.Tipo == novo.Tipo && 
                    existente.ProdutoId == novo.ProdutoId))
                .ToList();

            if (alertasNovos.Any())
            {
                _context.AlertasMargem.AddRange(alertasNovos);
                await _context.SaveChangesAsync();
            }
        }
    }

    #endregion

    #region Consultas de Alertas

    /// <summary>
    /// Obtém todos os alertas ativos de uma empresa
    /// </summary>
    public async Task<IEnumerable<AlertaMargem>> ObterAlertasAtivosAsync(int empresaId)
    {
        return await _context.AlertasMargem
            .Include(a => a.Venda)
                .ThenInclude(v => v.Cliente)
            .Include(a => a.Produto)
            .Where(a => a.EmpresaId == empresaId && a.Status == StatusAlerta.Ativo)
            .OrderByDescending(a => a.DataAlerta)
            .ToListAsync();
    }

    /// <summary>
    /// Obtém alertas por período
    /// </summary>
    public async Task<IEnumerable<AlertaMargem>> ObterAlertasPorPeriodoAsync(
        DateTime dataInicio, 
        DateTime dataFim, 
        int empresaId, 
        StatusAlerta? status = null)
    {
        var query = _context.AlertasMargem
            .Include(a => a.Venda)
                .ThenInclude(v => v.Cliente)
            .Include(a => a.Produto)
            .Where(a => a.EmpresaId == empresaId &&
                       a.DataAlerta >= dataInicio &&
                       a.DataAlerta <= dataFim);

        if (status.HasValue)
            query = query.Where(a => a.Status == status.Value);

        return await query
            .OrderByDescending(a => a.DataAlerta)
            .ToListAsync();
    }

    /// <summary>
    /// Obtém alertas por tipo
    /// </summary>
    public async Task<IEnumerable<AlertaMargem>> ObterAlertasPorTipoAsync(
        TipoAlertaMargem tipo, 
        int empresaId, 
        StatusAlerta? status = null)
    {
        var query = _context.AlertasMargem
            .Include(a => a.Venda)
                .ThenInclude(v => v.Cliente)
            .Include(a => a.Produto)
            .Where(a => a.EmpresaId == empresaId && a.Tipo == tipo);

        if (status.HasValue)
            query = query.Where(a => a.Status == status.Value);

        return await query
            .OrderByDescending(a => a.DataAlerta)
            .ToListAsync();
    }

    /// <summary>
    /// Obtém alerta por ID
    /// </summary>
    public async Task<AlertaMargem?> ObterAlertaPorIdAsync(int id, int empresaId)
    {
        return await _context.AlertasMargem
            .Include(a => a.Venda)
                .ThenInclude(v => v.Cliente)
            .Include(a => a.Produto)
            .Include(a => a.UsuarioResolucao)
            .FirstOrDefaultAsync(a => a.Id == id && a.EmpresaId == empresaId);
    }

    /// <summary>
    /// Conta alertas ativos por empresa
    /// </summary>
    public async Task<int> ContarAlertasAtivosAsync(int empresaId)
    {
        return await _context.AlertasMargem
            .CountAsync(a => a.EmpresaId == empresaId && a.Status == StatusAlerta.Ativo);
    }

    #endregion

    #region Resolução de Alertas

    /// <summary>
    /// Resolve um alerta manualmente
    /// </summary>
    public async Task<bool> ResolverAlertaAsync(int alertaId, int usuarioId, int empresaId, string? observacoes = null)
    {
        var alerta = await _context.AlertasMargem
            .FirstOrDefaultAsync(a => a.Id == alertaId && a.EmpresaId == empresaId);

        if (alerta == null)
            return false;

        // Usar método do Domain para resolver
        alerta.Resolver(usuarioId, observacoes);

        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Reativa um alerta
    /// </summary>
    public async Task<bool> ReativarAlertaAsync(int alertaId, int empresaId)
    {
        var alerta = await _context.AlertasMargem
            .FirstOrDefaultAsync(a => a.Id == alertaId && a.EmpresaId == empresaId);

        if (alerta == null)
            return false;

        // Usar método do Domain para reativar
        alerta.Reativar();

        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Resolve múltiplos alertas em lote
    /// </summary>
    public async Task<int> ResolverAlertasEmLoteAsync(
        IEnumerable<int> alertaIds, 
        int usuarioId, 
        int empresaId, 
        string? observacoes = null)
    {
        var alertas = await _context.AlertasMargem
            .Where(a => alertaIds.Contains(a.Id) && 
                       a.EmpresaId == empresaId && 
                       a.Status == StatusAlerta.Ativo)
            .ToListAsync();

        foreach (var alerta in alertas)
        {
            alerta.Resolver(usuarioId, observacoes);
        }

        await _context.SaveChangesAsync();
        return alertas.Count;
    }

    #endregion

    #region Estatísticas

    /// <summary>
    /// Obtém estatísticas de alertas por período
    /// </summary>
    public async Task<dynamic> ObterEstatisticasAlertasAsync(
        DateTime dataInicio, 
        DateTime dataFim, 
        int empresaId)
    {
        var alertas = await _context.AlertasMargem
            .Where(a => a.EmpresaId == empresaId &&
                       a.DataAlerta >= dataInicio &&
                       a.DataAlerta <= dataFim)
            .ToListAsync();

        return new
        {
            TotalAlertas = alertas.Count,
            AlertasAtivos = alertas.Count(a => a.Status == StatusAlerta.Ativo),
            AlertasResolvidos = alertas.Count(a => a.Status == StatusAlerta.Resolvido),
            AlertasVendaMargemNegativa = alertas.Count(a => a.Tipo == TipoAlertaMargem.VendaMargemNegativa),
            AlertasProdutoAbaixoCusto = alertas.Count(a => a.Tipo == TipoAlertaMargem.ProdutoAbaixoCusto),
            PrejuizoTotalDetectado = alertas.Sum(a => Math.Abs(a.LucroVenda)),
            TempoMedioResolucao = alertas
                .Where(a => a.DataResolucao.HasValue)
                .Select(a => (a.DataResolucao!.Value - a.DataAlerta).TotalDays)
                .DefaultIfEmpty(0)
                .Average()
        };
    }

    #endregion
}