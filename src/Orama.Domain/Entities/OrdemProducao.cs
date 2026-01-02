using System.ComponentModel.DataAnnotations;

namespace Orama.Domain.Entities;

/// <summary>
/// Status simples da Ordem de Produção
/// </summary>
public enum StatusOrdemProducao
{
    Planejada = 1,    // Criada
    Liberada = 2,     // Liberada para produção
    EmAndamento = 3,  // Em produção
    Finalizada = 5,   // Finalizada
    Cancelada = 6     // Cancelada
}

/// <summary>
/// Ordem de Produção - SIMPLIFICADA
/// Centraliza todas as regras de produção industrial
/// </summary>
public class OrdemProducao
{
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

    // Propriedades calculadas
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

    // REGRAS DE NEGÓCIO CENTRALIZADAS NA ENTIDADE

    /// <summary>
    /// Valida se a ordem pode ser criada
    /// </summary>
    public static (bool Valida, List<string> Erros) ValidarCriacao(Produto produto, decimal quantidade, IEnumerable<EstruturaProduto> estrutura)
    {
        var erros = new List<string>();

        if (!produto.EhProduzivel())
            erros.Add($"Produto '{produto.Descricao}' não é produzível");

        if (!estrutura.Any())
            erros.Add($"Produto '{produto.Descricao}' não possui estrutura de produção");

        foreach (var item in estrutura)
        {
            if (!item.ComponenteTemEstoqueSuficiente(quantidade))
            {
                var necessario = item.CalcularQuantidadeTotal(quantidade);
                var disponivel = item.ProdutoComponente.EstoqueAtual;
                erros.Add($"Estoque insuficiente: {item.ProdutoComponente.Descricao}. Necessário: {necessario:N2}, Disponível: {disponivel:N2}");
            }
        }

        return (erros.Count == 0, erros);
    }

    /// <summary>
    /// Gera número da OP
    /// </summary>
    public static string GerarNumero(int empresaId, int proximoNumero)
    {
        var ano = DateTime.Now.Year;
        return $"OP{empresaId:D3}{ano}{proximoNumero:D6}";
    }

    /// <summary>
    /// Calcula custo de produção
    /// </summary>
    public static decimal CalcularCusto(decimal quantidade, IEnumerable<EstruturaProduto> estrutura)
    {
        return estrutura.Sum(item => item.CalcularQuantidadeTotal(quantidade) * item.ProdutoComponente.PrecoCusto);
    }

    /// <summary>
    /// Cria itens da ordem baseado na estrutura
    /// </summary>
    public List<OrdemProducaoItem> CriarItens(decimal quantidade, IEnumerable<EstruturaProduto> estrutura)
    {
        var itens = new List<OrdemProducaoItem>();

        foreach (var estruturaItem in estrutura)
        {
            var item = new OrdemProducaoItem
            {
                OrdemProducaoId = Id,
                ProdutoId = estruturaItem.ProdutoComponenteId,
                QuantidadePlanejada = estruturaItem.CalcularQuantidadeTotal(quantidade),
                CustoUnitario = estruturaItem.ProdutoComponente.PrecoCusto
            };
            itens.Add(item);
        }

        return itens;
    }

    /// <summary>
    /// Liberar ordem para produção
    /// </summary>
    public void Liberar()
    {
        if (Status != StatusOrdemProducao.Planejada)
            throw new InvalidOperationException("Apenas ordens planejadas podem ser liberadas");

        Status = StatusOrdemProducao.Liberada;
        DataAtualizacao = DateTime.Now;
    }

    /// <summary>
    /// Iniciar produção
    /// </summary>
    public void Iniciar()
    {
        if (Status != StatusOrdemProducao.Liberada)
            throw new InvalidOperationException("Apenas ordens liberadas podem ser iniciadas");

        Status = StatusOrdemProducao.EmAndamento;
        DataInicio = DateTime.Now;
        DataAtualizacao = DateTime.Now;
    }

    /// <summary>
    /// Finalizar produção
    /// </summary>
    public void Finalizar(decimal quantidadeProduzida)
    {
        if (Status != StatusOrdemProducao.EmAndamento)
            throw new InvalidOperationException("Apenas ordens em andamento podem ser finalizadas");

        if (quantidadeProduzida <= 0)
            throw new ArgumentException("Quantidade produzida deve ser maior que zero");

        Status = StatusOrdemProducao.Finalizada;
        QuantidadeProduzida = quantidadeProduzida;
        DataFim = DateTime.Now;
        DataAtualizacao = DateTime.Now;

        // Atualizar quantidades consumidas nos itens
        foreach (var item in Itens)
        {
            item.QuantidadeConsumida = (item.QuantidadePlanejada / QuantidadePlanejada) * quantidadeProduzida;
        }
    }

    /// <summary>
    /// Cancelar ordem
    /// </summary>
    public void Cancelar(string motivo)
    {
        if (Status == StatusOrdemProducao.Finalizada)
            throw new InvalidOperationException("Ordens finalizadas não podem ser canceladas");

        Status = StatusOrdemProducao.Cancelada;
        Observacoes = $"CANCELADA: {motivo}";
        DataAtualizacao = DateTime.Now;
    }

    // Validações de estado simples
    public bool PodeLiberar() => Status == StatusOrdemProducao.Planejada;
    public bool PodeIniciar() => Status == StatusOrdemProducao.Liberada;
    public bool PodeFinalizar() => Status == StatusOrdemProducao.EmAndamento;
}

/// <summary>
/// Item da Ordem de Produção - SIMPLIFICADO
/// </summary>
public class OrdemProducaoItem
{
    public int Id { get; set; }

    public int OrdemProducaoId { get; set; }
    public virtual OrdemProducao OrdemProducao { get; set; } = null!;

    public int ProdutoId { get; set; }
    public virtual Produto Produto { get; set; } = null!;

    [Display(Name = "Quantidade Planejada")]
    public decimal QuantidadePlanejada { get; set; }

    [Display(Name = "Quantidade Consumida")]
    public decimal QuantidadeConsumida { get; set; } = 0;

    [Display(Name = "Custo Unitário")]
    public decimal CustoUnitario { get; set; }

    public decimal CustoTotal => QuantidadeConsumida * CustoUnitario;

    public DateTime DataCriacao { get; set; } = DateTime.Now;
}