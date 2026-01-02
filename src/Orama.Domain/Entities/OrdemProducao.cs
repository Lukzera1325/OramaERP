using System.ComponentModel.DataAnnotations;

namespace Orama.Domain.Entities;

/// <summary>
/// Status da Ordem de Produção - Fluxo Industrial Simplificado
/// Planejada → Liberada → Em Andamento → Finalizada
/// </summary>
public enum StatusOrdemProducao
{
    Planejada = 1,    // Ordem criada, aguardando liberação
    Liberada = 2,     // Liberada para iniciar produção
    EmAndamento = 3,  // Produção em execução
    Finalizada = 5,   // Produção concluída, estoque atualizado
    Cancelada = 6     // Ordem cancelada
}

/// <summary>
/// Ordem de Produção Industrial - Entidade Rica com Regras de Negócio
/// 
/// Responsabilidades:
/// - Controlar o fluxo de produção (Planejar → Liberar → Iniciar → Finalizar)
/// - Validar pré-condições para cada transição de estado
/// - Calcular custos e quantidades de produção
/// - Integrar com estrutura de produtos (BOM)
/// </summary>
public class OrdemProducao
{
    #region Propriedades Básicas
    
    public int Id { get; set; }

    [Required]
    [StringLength(20)]
    public string Numero { get; set; } = string.Empty;

    // Multi-Tenant
    public int EmpresaId { get; set; }
    public virtual Empresa Empresa { get; set; } = null!;

    // Produto a ser produzido
    public int ProdutoId { get; set; }
    public virtual Produto Produto { get; set; } = null!;

    [Required]
    public decimal QuantidadePlanejada { get; set; }
    public decimal QuantidadeProduzida { get; set; }

    public DateTime DataPlanejada { get; set; } = DateTime.Now;
    public DateTime? DataInicio { get; set; }
    public DateTime? DataFim { get; set; }

    [Required]
    public StatusOrdemProducao Status { get; set; } = StatusOrdemProducao.Planejada;

    [StringLength(500)]
    public string? Observacoes { get; set; }

    public decimal CustoMaterial { get; set; }

    public DateTime DataCriacao { get; set; } = DateTime.Now;
    public DateTime? DataAtualizacao { get; set; }

    public int UsuarioCriacaoId { get; set; }
    public virtual Usuario UsuarioCriacao { get; set; } = null!;

    // Relacionamentos essenciais
    public virtual ICollection<OrdemProducaoItem> Itens { get; set; } = new List<OrdemProducaoItem>();

    #endregion

    #region Propriedades Calculadas

    public string StatusDescricao => Status switch
    {
        StatusOrdemProducao.Planejada => "Planejada",
        StatusOrdemProducao.Liberada => "Liberada",
        StatusOrdemProducao.EmAndamento => "Em Andamento",
        StatusOrdemProducao.Finalizada => "Finalizada",
        StatusOrdemProducao.Cancelada => "Cancelada",
        _ => "Desconhecido"
    };

    public decimal PercentualConcluido
    {
        get
        {
            if (QuantidadePlanejada == 0) return 0;
            return (QuantidadeProduzida / QuantidadePlanejada) * 100;
        }
    }

    #endregion

    #region Métodos Estáticos - Operações de Criação e Validação

    /// <summary>
    /// Valida se uma ordem de produção pode ser criada
    /// Verifica: produto produzível, estrutura definida, estoque suficiente
    /// </summary>
    public static (bool EhValida, List<string> MensagensErro) ValidarCriacaoOrdemProducao(
        Produto produto, 
        decimal quantidadeDesejada, 
        IEnumerable<EstruturaProduto> estruturaProduto)
    {
        var erros = new List<string>();

        // Validar se produto pode ser produzido
        if (!produto.EhProduzivel())
            erros.Add($"Produto '{produto.Descricao}' não é produzível");

        // Validar se tem estrutura de produção (BOM)
        if (!estruturaProduto.Any())
            erros.Add($"Produto '{produto.Descricao}' não possui estrutura de produção definida");

        // Validar estoque dos componentes
        foreach (var componenteEstrutura in estruturaProduto)
        {
            if (!componenteEstrutura.ComponenteTemEstoqueSuficiente(quantidadeDesejada))
            {
                var quantidadeNecessaria = componenteEstrutura.CalcularQuantidadeTotal(quantidadeDesejada);
                var estoqueDisponivel = componenteEstrutura.ProdutoComponente.EstoqueAtual;
                
                erros.Add($"Estoque insuficiente do componente '{componenteEstrutura.ProdutoComponente.Descricao}'. " +
                         $"Necessário: {quantidadeNecessaria:N2}, Disponível: {estoqueDisponivel:N2}");
            }
        }

        return (erros.Count == 0, erros);
    }

    /// <summary>
    /// Gera número sequencial da Ordem de Produção
    /// Formato: OP{EmpresaId}{Ano}{Sequencial}
    /// Exemplo: OP0012024000001
    /// </summary>
    public static string GerarNumeroOrdemProducao(int empresaId, int proximoNumeroSequencial)
    {
        var anoAtual = DateTime.Now.Year;
        return $"OP{empresaId:D3}{anoAtual}{proximoNumeroSequencial:D6}";
    }

    /// <summary>
    /// Calcula custo total de produção baseado na estrutura de produtos
    /// Soma: (quantidade necessária de cada componente) × (preço de custo)
    /// </summary>
    public static decimal CalcularCustoProducao(decimal quantidadeAProduzir, IEnumerable<EstruturaProduto> estruturaProduto)
    {
        return estruturaProduto.Sum(componente => 
            componente.CalcularQuantidadeTotal(quantidadeAProduzir) * componente.ProdutoComponente.PrecoCusto);
    }

    #endregion

    #region Métodos de Instância - Operações da Ordem

    /// <summary>
    /// Cria os itens (componentes) da ordem baseado na estrutura de produtos
    /// Cada item representa um componente necessário para a produção
    /// </summary>
    public List<OrdemProducaoItem> CriarItensProducao(decimal quantidadeAProduzir, IEnumerable<EstruturaProduto> estruturaProduto)
    {
        var itensOrdem = new List<OrdemProducaoItem>();

        foreach (var componenteEstrutura in estruturaProduto)
        {
            var itemOrdem = new OrdemProducaoItem
            {
                OrdemProducaoId = Id,
                ProdutoId = componenteEstrutura.ProdutoComponenteId,
                QuantidadePlanejada = componenteEstrutura.CalcularQuantidadeTotal(quantidadeAProduzir),
                CustoUnitario = componenteEstrutura.ProdutoComponente.PrecoCusto
            };
            itensOrdem.Add(itemOrdem);
        }

        return itensOrdem;
    }

    #endregion

    #region Fluxo de Produção - Transições de Estado

    /// <summary>
    /// Libera a ordem para produção
    /// Transição: Planejada → Liberada
    /// </summary>
    public void LiberarParaProducao()
    {
        if (Status != StatusOrdemProducao.Planejada)
            throw new InvalidOperationException("Apenas ordens planejadas podem ser liberadas para produção");

        Status = StatusOrdemProducao.Liberada;
        DataAtualizacao = DateTime.Now;
    }

    /// <summary>
    /// Inicia a produção
    /// Transição: Liberada → Em Andamento
    /// Registra data/hora de início
    /// </summary>
    public void IniciarProducao()
    {
        if (Status != StatusOrdemProducao.Liberada)
            throw new InvalidOperationException("Apenas ordens liberadas podem ter a produção iniciada");

        Status = StatusOrdemProducao.EmAndamento;
        DataInicio = DateTime.Now;
        DataAtualizacao = DateTime.Now;
    }

    /// <summary>
    /// Finaliza a produção
    /// Transição: Em Andamento → Finalizada
    /// 
    /// Efeitos:
    /// - Registra quantidade produzida
    /// - Atualiza quantidades consumidas dos componentes
    /// - Registra data/hora de conclusão
    /// 
    /// Nota: A integração com estoque é feita pelo Service
    /// </summary>
    public void FinalizarProducao(decimal quantidadeProduzida)
    {
        if (Status != StatusOrdemProducao.EmAndamento)
            throw new InvalidOperationException("Apenas ordens em andamento podem ser finalizadas");

        if (quantidadeProduzida <= 0)
            throw new ArgumentException("Quantidade produzida deve ser maior que zero");

        Status = StatusOrdemProducao.Finalizada;
        QuantidadeProduzida = quantidadeProduzida;
        DataFim = DateTime.Now;
        DataAtualizacao = DateTime.Now;

        // Calcular quantidades realmente consumidas dos componentes
        AtualizarQuantidadesConsumidasDosComponentes(quantidadeProduzida);
    }

    /// <summary>
    /// Cancela a ordem de produção
    /// Transição: Qualquer Status (exceto Finalizada) → Cancelada
    /// </summary>
    public void CancelarOrdemProducao(string motivoCancelamento)
    {
        if (Status == StatusOrdemProducao.Finalizada)
            throw new InvalidOperationException("Ordens finalizadas não podem ser canceladas");

        Status = StatusOrdemProducao.Cancelada;
        Observacoes = $"CANCELADA: {motivoCancelamento}";
        DataAtualizacao = DateTime.Now;
    }

    #endregion

    #region Validações de Estado

    /// <summary>
    /// Verifica se a ordem pode ser liberada para produção
    /// </summary>
    public bool PodeLiberarParaProducao() => Status == StatusOrdemProducao.Planejada;

    /// <summary>
    /// Verifica se a produção pode ser iniciada
    /// </summary>
    public bool PodeIniciarProducao() => Status == StatusOrdemProducao.Liberada;

    /// <summary>
    /// Verifica se a produção pode ser finalizada
    /// </summary>
    public bool PodeFinalizarProducao() => Status == StatusOrdemProducao.EmAndamento;

    /// <summary>
    /// Verifica se a ordem pode ser cancelada
    /// </summary>
    public bool PodeCancelarOrdem() => Status != StatusOrdemProducao.Finalizada;

    #endregion

    #region Métodos Privados

    /// <summary>
    /// Atualiza as quantidades consumidas dos componentes baseado na produção real
    /// Proporção: (quantidade planejada / quantidade total planejada) × quantidade produzida
    /// </summary>
    private void AtualizarQuantidadesConsumidasDosComponentes(decimal quantidadeProduzida)
    {
        foreach (var item in Itens)
        {
            // Calcula proporcionalmente quanto foi consumido de cada componente
            item.QuantidadeConsumida = (item.QuantidadePlanejada / QuantidadePlanejada) * quantidadeProduzida;
        }
    }

    #endregion
}
/// <summary>
/// Item (Componente) da Ordem de Produção
/// 
/// Representa um componente necessário para produzir o produto final.
/// Cada item corresponde a uma linha da estrutura de produtos (BOM).
/// 
/// Exemplo: Para produzir 1 Martelo
/// - Item 1: Cabo de Madeira (1 unidade)
/// - Item 2: Cabeça de Ferro (1 unidade)
/// </summary>
public class OrdemProducaoItem
{
    public int Id { get; set; }

    public int OrdemProducaoId { get; set; }
    public virtual OrdemProducao OrdemProducao { get; set; } = null!;

    public int ProdutoId { get; set; }
    public virtual Produto Produto { get; set; } = null!;

    /// <summary>
    /// Quantidade necessária do componente (calculada pela estrutura de produtos)
    /// </summary>
    [Display(Name = "Quantidade Planejada")]
    public decimal QuantidadePlanejada { get; set; }

    /// <summary>
    /// Quantidade realmente consumida na produção
    /// Atualizada quando a ordem é finalizada
    /// </summary>
    [Display(Name = "Quantidade Consumida")]
    public decimal QuantidadeConsumida { get; set; } = 0;

    /// <summary>
    /// Preço de custo unitário do componente
    /// </summary>
    [Display(Name = "Custo Unitário")]
    public decimal CustoUnitario { get; set; }

    /// <summary>
    /// Custo total do componente na produção
    /// Calculado: Quantidade Consumida × Custo Unitário
    /// </summary>
    public decimal CustoTotal => QuantidadeConsumida * CustoUnitario;

    public DateTime DataCriacao { get; set; } = DateTime.Now;
}