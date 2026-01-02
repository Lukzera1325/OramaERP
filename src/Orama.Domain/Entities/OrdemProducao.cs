using System.ComponentModel.DataAnnotations;

namespace Orama.Domain.Entities;

/// <summary>
/// Status simples da Ordem de Produção (adaptado para simplicidade)
/// </summary>
public enum StatusOrdemProducao
{
    Planejada = 1,    // Criada (compatível com original)
    Liberada = 2,     // Liberada para produção
    EmAndamento = 3,  // Em produção
    Pausada = 4,      // Pausada
    Finalizada = 5,   // Finalizada
    Cancelada = 6     // Cancelada
}

public enum PrioridadeOrdemProducao
{
    Baixa = 1,
    Normal = 2,
    Alta = 3,
    Urgente = 4
}

/// <summary>
/// Ordem de Produção - Versão simplificada e compatível
/// Mantém compatibilidade com estrutura existente mas com métodos de negócio
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
    public PrioridadeOrdemProducao Prioridade { get; set; } = PrioridadeOrdemProducao.Normal;

    [StringLength(500)]
    public string? Observacoes { get; set; }

    public decimal CustoMaterial { get; set; }
    public decimal CustoMaoObra { get; set; }
    public decimal CustoTotal => CustoMaterial + CustoMaoObra;

    public DateTime DataCriacao { get; set; } = DateTime.Now;
    public DateTime? DataAtualizacao { get; set; }

    public int UsuarioCriacaoId { get; set; }
    public virtual Usuario UsuarioCriacao { get; set; } = null!;

    // Relacionamentos (mantendo compatibilidade)
    public virtual ICollection<OrdemProducaoItem> Itens { get; set; } = new List<OrdemProducaoItem>();
    public virtual ICollection<OrdemProducaoEtapa> Etapas { get; set; } = new List<OrdemProducaoEtapa>();
    public virtual ICollection<ApontamentoHoras> ApontamentosHoras { get; set; } = new List<ApontamentoHoras>();
    public virtual ICollection<InspecaoQualidade> InspecoesQualidade { get; set; } = new List<InspecaoQualidade>();

    // Propriedades calculadas
    public string StatusDescricao => Status switch
    {
        StatusOrdemProducao.Planejada => "Planejada",
        StatusOrdemProducao.Liberada => "Liberada",
        StatusOrdemProducao.EmAndamento => "Em Andamento",
        StatusOrdemProducao.Pausada => "Pausada",
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

    // Métodos de negócio simplificados
    public void Liberar()
    {
        if (Status != StatusOrdemProducao.Planejada)
            throw new InvalidOperationException("Apenas ordens planejadas podem ser liberadas");

        Status = StatusOrdemProducao.Liberada;
        DataAtualizacao = DateTime.Now;
    }

    public void Iniciar()
    {
        if (Status != StatusOrdemProducao.Liberada)
            throw new InvalidOperationException("Apenas ordens liberadas podem ser iniciadas");

        Status = StatusOrdemProducao.EmAndamento;
        DataInicio = DateTime.Now;
        DataAtualizacao = DateTime.Now;
    }

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
    }

    public void Cancelar(string motivo)
    {
        if (Status == StatusOrdemProducao.Finalizada)
            throw new InvalidOperationException("Ordens finalizadas não podem ser canceladas");

        Status = StatusOrdemProducao.Cancelada;
        Observacoes = $"CANCELADA: {motivo}";
        DataAtualizacao = DateTime.Now;
    }

    public void Pausar(string motivo)
    {
        if (Status != StatusOrdemProducao.EmAndamento)
            throw new InvalidOperationException("Apenas ordens em andamento podem ser pausadas");

        Status = StatusOrdemProducao.Pausada;
        Observacoes = $"PAUSADA: {motivo}";
        DataAtualizacao = DateTime.Now;
    }

    public void Retomar()
    {
        if (Status != StatusOrdemProducao.Pausada)
            throw new InvalidOperationException("Apenas ordens pausadas podem ser retomadas");

        Status = StatusOrdemProducao.EmAndamento;
        DataAtualizacao = DateTime.Now;
    }

    // Validações de estado
    public bool PodeLiberar() => Status == StatusOrdemProducao.Planejada;
    public bool PodeIniciar() => Status == StatusOrdemProducao.Liberada;
    public bool PodeFinalizar() => Status == StatusOrdemProducao.EmAndamento;
    public bool PodeCancelar() => Status != StatusOrdemProducao.Finalizada;
    public bool PodePausar() => Status == StatusOrdemProducao.EmAndamento;
    public bool PodeRetomar() => Status == StatusOrdemProducao.Pausada;
}

/// <summary>
/// Item da Ordem de Produção (compatível com estrutura existente)
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
    public DateTime? DataAtualizacao { get; set; }
}