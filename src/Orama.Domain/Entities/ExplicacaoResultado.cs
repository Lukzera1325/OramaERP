using System.ComponentModel.DataAnnotations;

namespace Orama.Domain.Entities;

/// <summary>
/// Tipos de resultado que podem ser explicados
/// </summary>
public enum TipoResultado
{
    Lucro = 1,          // Resultado positivo
    Prejuizo = 2,       // Resultado negativo
    Atencao = 3         // Margem baixa, mas positiva
}

/// <summary>
/// Categoria da explicação para facilitar agrupamento
/// </summary>
public enum CategoriaExplicacao
{
    PrecoVsCusto = 1,           // Comparação direta preço × custo
    MargemBaixa = 2,            // Margem positiva mas baixa
    CustoElevado = 3,           // Custo acima do esperado
    PrecoAbaixoMercado = 4,     // Preço de venda muito baixo
    DescontoExcessivo = 5       // Desconto que impactou a margem
}

/// <summary>
/// Explicação de Resultado - Entidade Simples para Explicar Lucros e Prejuízos
/// 
/// Responsabilidades:
/// - Explicar em linguagem clara por que uma venda/produto deu lucro ou prejuízo
/// - Fornecer contexto compreensível para estagiários e gestores
/// - Ser determinística e auditável
/// 
/// Filosofia: Clareza > Sofisticação
/// </summary>
public class ExplicacaoResultado : BaseEntity
{
    // Multi-Tenant
    public int EmpresaId { get; set; }
    public virtual Empresa Empresa { get; set; } = null!;

    /// <summary>
    /// Referência da venda que está sendo explicada
    /// </summary>
    public int VendaId { get; set; }
    public virtual Venda Venda { get; set; } = null!;

    /// <summary>
    /// Produto específico sendo explicado (opcional)
    /// Para explicações por item da venda
    /// </summary>
    public int? ProdutoId { get; set; }
    public virtual Produto? Produto { get; set; }

    /// <summary>
    /// Tipo do resultado (Lucro, Prejuízo, Atenção)
    /// </summary>
    [Required]
    public TipoResultado Tipo { get; set; }

    /// <summary>
    /// Categoria da explicação para agrupamento
    /// </summary>
    [Required]
    public CategoriaExplicacao Categoria { get; set; }

    /// <summary>
    /// Explicação em linguagem clara e objetiva
    /// Deve ser compreensível por qualquer pessoa
    /// </summary>
    [Required]
    [StringLength(1000)]
    public string TextoExplicacao { get; set; } = string.Empty;

    /// <summary>
    /// Resumo curto da explicação (para listas)
    /// </summary>
    [Required]
    [StringLength(200)]
    public string ResumoExplicacao { get; set; } = string.Empty;

    /// <summary>
    /// Data em que a explicação foi gerada
    /// </summary>
    public DateTime DataExplicacao { get; set; } = DateTime.Now;

    // Dados de contexto para a explicação
    /// <summary>
    /// Valor de venda usado na explicação
    /// </summary>
    public decimal ValorVenda { get; set; }

    /// <summary>
    /// Custo usado na explicação
    /// </summary>
    public decimal CustoTotal { get; set; }

    /// <summary>
    /// Lucro/prejuízo calculado
    /// </summary>
    public decimal LucroCalculado { get; set; }

    /// <summary>
    /// Margem percentual
    /// </summary>
    public decimal MargemPercentual { get; set; }

    #region Propriedades Calculadas

    /// <summary>
    /// Descrição do tipo de resultado
    /// </summary>
    public string TipoDescricao => Tipo switch
    {
        TipoResultado.Lucro => "Lucro",
        TipoResultado.Prejuizo => "Prejuízo",
        TipoResultado.Atencao => "Atenção",
        _ => "Desconhecido"
    };

    /// <summary>
    /// Descrição da categoria
    /// </summary>
    public string CategoriaDescricao => Categoria switch
    {
        CategoriaExplicacao.PrecoVsCusto => "Preço vs Custo",
        CategoriaExplicacao.MargemBaixa => "Margem Baixa",
        CategoriaExplicacao.CustoElevado => "Custo Elevado",
        CategoriaExplicacao.PrecoAbaixoMercado => "Preço Baixo",
        CategoriaExplicacao.DescontoExcessivo => "Desconto Excessivo",
        _ => "Outros"
    };

    /// <summary>
    /// Ícone para exibição na interface
    /// </summary>
    public string Icone => Tipo switch
    {
        TipoResultado.Lucro => "fas fa-arrow-up text-success",
        TipoResultado.Prejuizo => "fas fa-arrow-down text-danger",
        TipoResultado.Atencao => "fas fa-exclamation-triangle text-warning",
        _ => "fas fa-question-circle text-muted"
    };

    /// <summary>
    /// Classe CSS para estilização
    /// </summary>
    public string CssClass => Tipo switch
    {
        TipoResultado.Lucro => "alert-success",
        TipoResultado.Prejuizo => "alert-danger",
        TipoResultado.Atencao => "alert-warning",
        _ => "alert-info"
    };

    #endregion

    #region Métodos Estáticos - Criação de Explicações

    /// <summary>
    /// Cria explicação para venda com prejuízo por preço abaixo do custo
    /// </summary>
    public static ExplicacaoResultado CriarExplicacaoPrecoAbaixoCusto(
        Venda venda, 
        int empresaId, 
        decimal precoMedio, 
        decimal custoMedio)
    {
        var diferenca = custoMedio - precoMedio;
        var percentualDiferenca = custoMedio > 0 ? (diferenca / custoMedio) * 100 : 0;

        var textoExplicacao = $"Esta venda teve prejuízo porque o preço médio de venda ({precoMedio:C}) " +
                             $"ficou {diferenca:C} abaixo do custo médio dos produtos ({custoMedio:C}). " +
                             $"Isso representa uma diferença de {percentualDiferenca:N1}% a menos do que o necessário para cobrir os custos.";

        var resumo = $"Preço de venda {percentualDiferenca:N1}% abaixo do custo";

        return new ExplicacaoResultado
        {
            EmpresaId = empresaId,
            VendaId = venda.Id,
            Tipo = TipoResultado.Prejuizo,
            Categoria = CategoriaExplicacao.PrecoVsCusto,
            TextoExplicacao = textoExplicacao,
            ResumoExplicacao = resumo,
            ValorVenda = venda.ValorTotal,
            CustoTotal = venda.CustoTotal,
            LucroCalculado = venda.LucroTotal,
            MargemPercentual = venda.MargemPercentual,
            DataExplicacao = DateTime.Now,
            DataCriacao = DateTime.Now,
            Ativo = true
        };
    }

    /// <summary>
    /// Cria explicação para venda com margem baixa
    /// </summary>
    public static ExplicacaoResultado CriarExplicacaoMargemBaixa(
        Venda venda, 
        int empresaId, 
        decimal margemMinima = 10m)
    {
        var textoExplicacao = $"Esta venda teve lucro de {venda.LucroTotal:C}, mas a margem de {venda.MargemPercentual:N1}% " +
                             $"está abaixo do recomendado ({margemMinima:N1}%). " +
                             $"Embora não seja prejuízo, a margem baixa pode não cobrir despesas operacionais e impostos.";

        var resumo = $"Margem de {venda.MargemPercentual:N1}% abaixo do recomendado";

        return new ExplicacaoResultado
        {
            EmpresaId = empresaId,
            VendaId = venda.Id,
            Tipo = TipoResultado.Atencao,
            Categoria = CategoriaExplicacao.MargemBaixa,
            TextoExplicacao = textoExplicacao,
            ResumoExplicacao = resumo,
            ValorVenda = venda.ValorTotal,
            CustoTotal = venda.CustoTotal,
            LucroCalculado = venda.LucroTotal,
            MargemPercentual = venda.MargemPercentual,
            DataExplicacao = DateTime.Now,
            DataCriacao = DateTime.Now,
            Ativo = true
        };
    }

    /// <summary>
    /// Cria explicação para venda com bom lucro
    /// </summary>
    public static ExplicacaoResultado CriarExplicacaoLucroPositivo(
        Venda venda, 
        int empresaId)
    {
        var textoExplicacao = $"Esta venda teve excelente resultado com lucro de {venda.LucroTotal:C} " +
                             $"e margem de {venda.MargemPercentual:N1}%. " +
                             $"O preço de venda ({venda.ValorTotal:C}) cobriu adequadamente o custo ({venda.CustoTotal:C}) " +
                             $"e ainda gerou uma margem saudável para o negócio.";

        var resumo = $"Margem saudável de {venda.MargemPercentual:N1}%";

        return new ExplicacaoResultado
        {
            EmpresaId = empresaId,
            VendaId = venda.Id,
            Tipo = TipoResultado.Lucro,
            Categoria = CategoriaExplicacao.PrecoVsCusto,
            TextoExplicacao = textoExplicacao,
            ResumoExplicacao = resumo,
            ValorVenda = venda.ValorTotal,
            CustoTotal = venda.CustoTotal,
            LucroCalculado = venda.LucroTotal,
            MargemPercentual = venda.MargemPercentual,
            DataExplicacao = DateTime.Now,
            DataCriacao = DateTime.Now,
            Ativo = true
        };
    }

    /// <summary>
    /// Cria explicação para produto específico vendido abaixo do custo
    /// </summary>
    public static ExplicacaoResultado CriarExplicacaoProdutoAbaixoCusto(
        Venda venda, 
        Produto produto, 
        VendaItem item, 
        int empresaId)
    {
        var prejuizoUnitario = item.PrecoUnitario - item.CustoUnitario;
        var prejuizoTotal = prejuizoUnitario * item.Quantidade;

        var textoExplicacao = $"O produto '{produto.Descricao}' foi vendido por {item.PrecoUnitario:C} " +
                             $"quando o custo unitário era {item.CustoUnitario:C}. " +
                             $"Isso gerou um prejuízo de {Math.Abs(prejuizoUnitario):C} por unidade. " +
                             $"Com {item.Quantidade} unidades vendidas, o prejuízo total deste produto foi {Math.Abs(prejuizoTotal):C}.";

        var resumo = $"Produto vendido {Math.Abs(prejuizoUnitario):C} abaixo do custo";

        return new ExplicacaoResultado
        {
            EmpresaId = empresaId,
            VendaId = venda.Id,
            ProdutoId = produto.Id,
            Tipo = TipoResultado.Prejuizo,
            Categoria = CategoriaExplicacao.PrecoVsCusto,
            TextoExplicacao = textoExplicacao,
            ResumoExplicacao = resumo,
            ValorVenda = item.ValorTotal,
            CustoTotal = item.CustoTotal,
            LucroCalculado = item.LucroItem,
            MargemPercentual = item.MargemItem,
            DataExplicacao = DateTime.Now,
            DataCriacao = DateTime.Now,
            Ativo = true
        };
    }

    /// <summary>
    /// Cria explicação para desconto excessivo
    /// </summary>
    public static ExplicacaoResultado CriarExplicacaoDescontoExcessivo(
        Venda venda, 
        int empresaId, 
        decimal descontoTotal)
    {
        var impactoDesconto = (descontoTotal / venda.SubTotal) * 100;

        var textoExplicacao = $"Esta venda teve desconto de {descontoTotal:C} ({impactoDesconto:N1}% do subtotal), " +
                             $"o que impactou significativamente a margem. " +
                             $"Sem o desconto, a margem seria maior. " +
                             $"É importante avaliar se descontos altos são necessários para fechar vendas.";

        var resumo = $"Desconto de {impactoDesconto:N1}% impactou a margem";

        return new ExplicacaoResultado
        {
            EmpresaId = empresaId,
            VendaId = venda.Id,
            Tipo = venda.LucroTotal < 0 ? TipoResultado.Prejuizo : TipoResultado.Atencao,
            Categoria = CategoriaExplicacao.DescontoExcessivo,
            TextoExplicacao = textoExplicacao,
            ResumoExplicacao = resumo,
            ValorVenda = venda.ValorTotal,
            CustoTotal = venda.CustoTotal,
            LucroCalculado = venda.LucroTotal,
            MargemPercentual = venda.MargemPercentual,
            DataExplicacao = DateTime.Now,
            DataCriacao = DateTime.Now,
            Ativo = true
        };
    }

    #endregion
}