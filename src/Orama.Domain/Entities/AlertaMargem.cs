using System.ComponentModel.DataAnnotations;

namespace Orama.Domain.Entities;

/// <summary>
/// Tipos de alerta de margem
/// </summary>
public enum TipoAlertaMargem
{
    VendaMargemNegativa = 1,      // Venda com margem negativa total
    ProdutoAbaixoCusto = 2        // Produto vendido abaixo do custo unitário
}

/// <summary>
/// Status do alerta
/// </summary>
public enum StatusAlerta
{
    Ativo = 1,        // Alerta ativo, precisa de atenção
    Resolvido = 2     // Alerta resolvido/tratado
}

/// <summary>
/// Alerta de Margem - Entidade Simples para Detectar Prejuízos
/// 
/// Responsabilidades:
/// - Registrar problemas de margem detectados automaticamente
/// - Permitir rastreamento e resolução de alertas
/// - Fornecer informações claras sobre o problema
/// </summary>
public class AlertaMargem : BaseEntity
{
    // Multi-Tenant
    public int EmpresaId { get; set; }
    public virtual Empresa Empresa { get; set; } = null!;

    /// <summary>
    /// Tipo do alerta (Venda com margem negativa, Produto abaixo do custo, etc.)
    /// </summary>
    [Required]
    public TipoAlertaMargem Tipo { get; set; }

    /// <summary>
    /// Referência da venda que gerou o alerta
    /// </summary>
    public int VendaId { get; set; }
    public virtual Venda Venda { get; set; } = null!;

    /// <summary>
    /// Produto específico que causou o alerta (opcional)
    /// Para alertas de produto abaixo do custo
    /// </summary>
    public int? ProdutoId { get; set; }
    public virtual Produto? Produto { get; set; }

    /// <summary>
    /// Descrição simples e clara do problema
    /// </summary>
    [Required]
    [StringLength(500)]
    public string Descricao { get; set; } = string.Empty;

    /// <summary>
    /// Status atual do alerta
    /// </summary>
    [Required]
    public StatusAlerta Status { get; set; } = StatusAlerta.Ativo;

    /// <summary>
    /// Data em que o alerta foi gerado
    /// </summary>
    public DateTime DataAlerta { get; set; } = DateTime.Now;

    /// <summary>
    /// Data em que o alerta foi resolvido (se aplicável)
    /// </summary>
    public DateTime? DataResolucao { get; set; }

    /// <summary>
    /// Usuário que resolveu o alerta (se aplicável)
    /// </summary>
    public int? UsuarioResolucaoId { get; set; }
    public virtual Usuario? UsuarioResolucao { get; set; }

    /// <summary>
    /// Observações sobre a resolução do alerta
    /// </summary>
    [StringLength(500)]
    public string? ObservacoesResolucao { get; set; }

    // Dados do problema para referência rápida
    /// <summary>
    /// Valor da venda que gerou o alerta
    /// </summary>
    public decimal ValorVenda { get; set; }

    /// <summary>
    /// Custo total da venda
    /// </summary>
    public decimal CustoTotal { get; set; }

    /// <summary>
    /// Lucro/prejuízo da venda
    /// </summary>
    public decimal LucroVenda { get; set; }

    /// <summary>
    /// Margem percentual da venda
    /// </summary>
    public decimal MargemPercentual { get; set; }

    #region Propriedades Calculadas

    /// <summary>
    /// Descrição do tipo de alerta
    /// </summary>
    public string TipoDescricao => Tipo switch
    {
        TipoAlertaMargem.VendaMargemNegativa => "Venda com Margem Negativa",
        TipoAlertaMargem.ProdutoAbaixoCusto => "Produto Vendido Abaixo do Custo",
        _ => "Desconhecido"
    };

    /// <summary>
    /// Descrição do status do alerta
    /// </summary>
    public string StatusDescricao => Status switch
    {
        StatusAlerta.Ativo => "Ativo",
        StatusAlerta.Resolvido => "Resolvido",
        _ => "Desconhecido"
    };

    /// <summary>
    /// Indica se o alerta está ativo
    /// </summary>
    public bool EhAtivo => Status == StatusAlerta.Ativo;

    /// <summary>
    /// Indica se o alerta foi resolvido
    /// </summary>
    public bool EhResolvido => Status == StatusAlerta.Resolvido;

    /// <summary>
    /// Calcula há quantos dias o alerta foi gerado
    /// </summary>
    public int DiasDesdeAlerta => (DateTime.Now - DataAlerta).Days;

    #endregion

    #region Métodos de Negócio

    /// <summary>
    /// Resolve o alerta manualmente
    /// </summary>
    public void Resolver(int usuarioId, string? observacoes = null)
    {
        if (Status == StatusAlerta.Resolvido)
            throw new InvalidOperationException("Alerta já foi resolvido");

        Status = StatusAlerta.Resolvido;
        DataResolucao = DateTime.Now;
        UsuarioResolucaoId = usuarioId;
        ObservacoesResolucao = observacoes;
        DataAtualizacao = DateTime.Now;
    }

    /// <summary>
    /// Reativa o alerta (caso tenha sido resolvido por engano)
    /// </summary>
    public void Reativar()
    {
        if (Status == StatusAlerta.Ativo)
            throw new InvalidOperationException("Alerta já está ativo");

        Status = StatusAlerta.Ativo;
        DataResolucao = null;
        UsuarioResolucaoId = null;
        ObservacoesResolucao = null;
        DataAtualizacao = DateTime.Now;
    }

    #endregion

    #region Métodos Estáticos - Criação de Alertas

    /// <summary>
    /// Cria alerta para venda com margem negativa
    /// </summary>
    public static AlertaMargem CriarAlertaVendaMargemNegativa(Venda venda, int empresaId)
    {
        if (venda.MargemPercentual >= 0)
            throw new ArgumentException("Venda não possui margem negativa");

        var descricao = $"Venda {venda.Numero} realizada com prejuízo de {venda.LucroTotal:C} " +
                       $"(margem: {venda.MargemPercentual:N1}%). " +
                       $"Cliente: {venda.Cliente?.Nome ?? "N/A"}";

        return new AlertaMargem
        {
            EmpresaId = empresaId,
            Tipo = TipoAlertaMargem.VendaMargemNegativa,
            VendaId = venda.Id,
            Descricao = descricao,
            ValorVenda = venda.ValorTotal,
            CustoTotal = venda.CustoTotal,
            LucroVenda = venda.LucroTotal,
            MargemPercentual = venda.MargemPercentual,
            DataAlerta = DateTime.Now,
            DataCriacao = DateTime.Now,
            Ativo = true
        };
    }

    /// <summary>
    /// Cria alerta para produto vendido abaixo do custo
    /// </summary>
    public static AlertaMargem CriarAlertaProdutoAbaixoCusto(
        Venda venda, 
        Produto produto, 
        decimal precoVendido, 
        decimal custoUnitario, 
        int empresaId)
    {
        if (precoVendido >= custoUnitario)
            throw new ArgumentException("Produto não foi vendido abaixo do custo");

        var prejuizoUnitario = precoVendido - custoUnitario;
        var margemUnitaria = custoUnitario > 0 ? (prejuizoUnitario / precoVendido) * 100 : 0;

        var descricao = $"Produto '{produto.Descricao}' vendido por {precoVendido:C} " +
                       $"abaixo do custo de {custoUnitario:C} " +
                       $"(prejuízo unitário: {prejuizoUnitario:C}, margem: {margemUnitaria:N1}%). " +
                       $"Venda: {venda.Numero}";

        return new AlertaMargem
        {
            EmpresaId = empresaId,
            Tipo = TipoAlertaMargem.ProdutoAbaixoCusto,
            VendaId = venda.Id,
            ProdutoId = produto.Id,
            Descricao = descricao,
            ValorVenda = venda.ValorTotal,
            CustoTotal = venda.CustoTotal,
            LucroVenda = venda.LucroTotal,
            MargemPercentual = venda.MargemPercentual,
            DataAlerta = DateTime.Now,
            DataCriacao = DateTime.Now,
            Ativo = true
        };
    }

    #endregion
}