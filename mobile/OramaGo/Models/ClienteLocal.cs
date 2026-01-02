using System.ComponentModel.DataAnnotations;

namespace OramaGo.Models;

public class ClienteLocal : BaseLocalModel
{
    [Required]
    [StringLength(200)]
    public string Nome { get; set; } = string.Empty;

    [StringLength(20)]
    public string? CpfCnpj { get; set; }

    [StringLength(20)]
    public string? RgIe { get; set; }

    [EmailAddress]
    [StringLength(100)]
    public string? Email { get; set; }

    [StringLength(20)]
    public string? Telefone { get; set; }

    [StringLength(20)]
    public string? Celular { get; set; }

    [StringLength(10)]
    public string? Cep { get; set; }

    [StringLength(200)]
    public string? Endereco { get; set; }

    [StringLength(10)]
    public string? Numero { get; set; }

    [StringLength(100)]
    public string? Complemento { get; set; }

    [StringLength(100)]
    public string? Bairro { get; set; }

    [StringLength(100)]
    public string? Cidade { get; set; }

    [StringLength(2)]
    public string? Estado { get; set; }

    public DateTime? DataNascimento { get; set; }

    [StringLength(500)]
    public string? Observacoes { get; set; }

    /// <summary>
    /// Limite de crédito do cliente
    /// </summary>
    public decimal LimiteCredito { get; set; }

    /// <summary>
    /// Saldo atual do cliente (contas em aberto)
    /// </summary>
    public decimal SaldoAtual { get; set; }

    /// <summary>
    /// Indica se o cliente está bloqueado para vendas
    /// </summary>
    public bool Bloqueado { get; set; }

    /// <summary>
    /// Motivo do bloqueio
    /// </summary>
    [StringLength(200)]
    public string? MotivoBloqueio { get; set; }

    /// <summary>
    /// Vendedor responsável pelo cliente
    /// </summary>
    public int? VendedorId { get; set; }

    // Propriedades calculadas
    public string CpfCnpjFormatado => FormatarCpfCnpj(CpfCnpj);
    public string TelefoneCompleto => !string.IsNullOrEmpty(Celular) ? Celular : Telefone ?? "";
    public string EnderecoCompleto => $"{Endereco}, {Numero} - {Bairro}, {Cidade}/{Estado}";
    public decimal CreditoDisponivel => LimiteCredito - SaldoAtual;

    private static string FormatarCpfCnpj(string? documento)
    {
        if (string.IsNullOrEmpty(documento))
            return "";

        documento = documento.Replace(".", "").Replace("-", "").Replace("/", "");

        return documento.Length switch
        {
            11 => $"{documento[..3]}.{documento.Substring(3, 3)}.{documento.Substring(6, 3)}-{documento.Substring(9, 2)}",
            14 => $"{documento[..2]}.{documento.Substring(2, 3)}.{documento.Substring(5, 3)}/{documento.Substring(8, 4)}-{documento.Substring(12, 2)}",
            _ => documento
        };
    }
}