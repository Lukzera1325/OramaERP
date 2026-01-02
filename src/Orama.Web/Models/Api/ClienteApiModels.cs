using System.ComponentModel.DataAnnotations;

namespace Orama.Web.Models.Api;

/// <summary>
/// DTO do cliente para API
/// </summary>
public class ClienteApiDto
{
    /// <summary>
    /// ID do cliente
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Nome do cliente
    /// </summary>
    public string Nome { get; set; } = string.Empty;

    /// <summary>
    /// CPF ou CNPJ do cliente
    /// </summary>
    public string? CpfCnpj { get; set; }

    /// <summary>
    /// Email do cliente
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Telefone do cliente
    /// </summary>
    public string? Telefone { get; set; }

    /// <summary>
    /// Celular do cliente
    /// </summary>
    public string? Celular { get; set; }

    /// <summary>
    /// Endereço do cliente
    /// </summary>
    public string? Endereco { get; set; }

    /// <summary>
    /// Número do endereço
    /// </summary>
    public string? Numero { get; set; }

    /// <summary>
    /// Complemento do endereço
    /// </summary>
    public string? Complemento { get; set; }

    /// <summary>
    /// Bairro do cliente
    /// </summary>
    public string? Bairro { get; set; }

    /// <summary>
    /// Cidade do cliente
    /// </summary>
    public string? Cidade { get; set; }

    /// <summary>
    /// Estado do cliente
    /// </summary>
    public string? Estado { get; set; }

    /// <summary>
    /// CEP do cliente
    /// </summary>
    public string? Cep { get; set; }

    /// <summary>
    /// Observações sobre o cliente
    /// </summary>
    public string? Observacoes { get; set; }

    /// <summary>
    /// Limite de crédito do cliente
    /// </summary>
    public decimal LimiteCredito { get; set; }

    /// <summary>
    /// Indica se o cliente está ativo
    /// </summary>
    public bool Ativo { get; set; }

    /// <summary>
    /// Data de criação do cliente
    /// </summary>
    public DateTime DataCriacao { get; set; }

    /// <summary>
    /// Data da última modificação
    /// </summary>
    public DateTime DataModificacao { get; set; }
}

/// <summary>
/// DTO para criação de cliente via API
/// </summary>
public class ClienteCreateApiDto
{
    /// <summary>
    /// Nome do cliente
    /// </summary>
    [Required(ErrorMessage = "Nome é obrigatório")]
    [StringLength(200, ErrorMessage = "Nome deve ter no máximo 200 caracteres")]
    public string Nome { get; set; } = string.Empty;

    /// <summary>
    /// CPF ou CNPJ do cliente
    /// </summary>
    [StringLength(20, ErrorMessage = "CPF/CNPJ deve ter no máximo 20 caracteres")]
    public string? CpfCnpj { get; set; }

    /// <summary>
    /// Email do cliente
    /// </summary>
    [EmailAddress(ErrorMessage = "Email inválido")]
    [StringLength(100, ErrorMessage = "Email deve ter no máximo 100 caracteres")]
    public string? Email { get; set; }

    /// <summary>
    /// Telefone do cliente
    /// </summary>
    [StringLength(20, ErrorMessage = "Telefone deve ter no máximo 20 caracteres")]
    public string? Telefone { get; set; }

    /// <summary>
    /// Celular do cliente
    /// </summary>
    [StringLength(20, ErrorMessage = "Celular deve ter no máximo 20 caracteres")]
    public string? Celular { get; set; }

    /// <summary>
    /// Endereço do cliente
    /// </summary>
    [StringLength(200, ErrorMessage = "Endereço deve ter no máximo 200 caracteres")]
    public string? Endereco { get; set; }

    /// <summary>
    /// Número do endereço
    /// </summary>
    [StringLength(20, ErrorMessage = "Número deve ter no máximo 20 caracteres")]
    public string? Numero { get; set; }

    /// <summary>
    /// Complemento do endereço
    /// </summary>
    [StringLength(100, ErrorMessage = "Complemento deve ter no máximo 100 caracteres")]
    public string? Complemento { get; set; }

    /// <summary>
    /// Bairro do cliente
    /// </summary>
    [StringLength(100, ErrorMessage = "Bairro deve ter no máximo 100 caracteres")]
    public string? Bairro { get; set; }

    /// <summary>
    /// Cidade do cliente
    /// </summary>
    [StringLength(100, ErrorMessage = "Cidade deve ter no máximo 100 caracteres")]
    public string? Cidade { get; set; }

    /// <summary>
    /// Estado do cliente
    /// </summary>
    [StringLength(2, ErrorMessage = "Estado deve ter no máximo 2 caracteres")]
    public string? Estado { get; set; }

    /// <summary>
    /// CEP do cliente
    /// </summary>
    [StringLength(10, ErrorMessage = "CEP deve ter no máximo 10 caracteres")]
    public string? Cep { get; set; }

    /// <summary>
    /// Observações sobre o cliente
    /// </summary>
    [StringLength(500, ErrorMessage = "Observações devem ter no máximo 500 caracteres")]
    public string? Observacoes { get; set; }

    /// <summary>
    /// Limite de crédito do cliente
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "Limite de crédito deve ser maior ou igual a zero")]
    public decimal LimiteCredito { get; set; }
}

/// <summary>
/// DTO para atualização de cliente via API
/// </summary>
public class ClienteUpdateApiDto : ClienteCreateApiDto
{
    // Herda todos os campos de ClienteCreateApiDto
}

/// <summary>
/// DTO para estatísticas de sincronização
/// </summary>
public class SyncStatsApiDto
{
    /// <summary>
    /// Total de registros
    /// </summary>
    public int TotalRecords { get; set; }

    /// <summary>
    /// Data da última sincronização
    /// </summary>
    public DateTime? LastSyncDate { get; set; }

    /// <summary>
    /// Número de registros novos desde a última sincronização
    /// </summary>
    public int NewRecords { get; set; }

    /// <summary>
    /// Número de registros atualizados desde a última sincronização
    /// </summary>
    public int UpdatedRecords { get; set; }

    /// <summary>
    /// Timestamp do servidor
    /// </summary>
    public DateTime ServerTimestamp { get; set; }
}