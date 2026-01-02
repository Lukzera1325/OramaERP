using System.ComponentModel.DataAnnotations;

namespace Orama.Domain.Entities;

/// <summary>
/// Entidade que representa um fornecedor
/// </summary>
public class Fornecedor : BaseEntity
{
    // Multi-Tenant
    public int EmpresaId { get; set; }
    public virtual Empresa? Empresa { get; set; }

    [Required(ErrorMessage = "Nome/Razão Social é obrigatório")]
    [StringLength(200, ErrorMessage = "Nome deve ter no máximo 200 caracteres")]
    public string Nome { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "Nome Fantasia deve ter no máximo 200 caracteres")]
    public string? NomeFantasia { get; set; }

    [Required(ErrorMessage = "Tipo de pessoa é obrigatório")]
    public TipoPessoa TipoPessoa { get; set; }

    [Required(ErrorMessage = "CPF/CNPJ é obrigatório")]
    [StringLength(18, ErrorMessage = "CPF/CNPJ deve ter no máximo 18 caracteres")]
    public string CpfCnpj { get; set; } = string.Empty;

    [StringLength(20, ErrorMessage = "RG/IE deve ter no máximo 20 caracteres")]
    public string? RgIe { get; set; }

    // Contato
    [StringLength(150)]
    [EmailAddress(ErrorMessage = "Email inválido")]
    public string? Email { get; set; }

    [StringLength(20)]
    public string? Telefone { get; set; }

    [StringLength(20)]
    public string? Celular { get; set; }

    [StringLength(100)]
    public string? Contato { get; set; } // Nome do contato na empresa

    // Endereço
    [StringLength(10)]
    public string? Cep { get; set; }

    [StringLength(200)]
    public string? Logradouro { get; set; }

    [StringLength(10)]
    public string? Numero { get; set; }

    [StringLength(100)]
    public string? Complemento { get; set; }

    [StringLength(100)]
    public string? Bairro { get; set; }

    [StringLength(100)]
    public string? Cidade { get; set; }

    [StringLength(2)]
    public string? Uf { get; set; }

    // Dados Bancários
    [StringLength(100)]
    public string? Banco { get; set; }

    [StringLength(10)]
    public string? Agencia { get; set; }

    [StringLength(20)]
    public string? ContaBancaria { get; set; }

    [StringLength(20)]
    public string? TipoConta { get; set; } // Corrente, Poupança

    [StringLength(20)]
    public string? ChavePix { get; set; }

    // Informações Comerciais
    public int PrazoPagamento { get; set; } = 30; // Dias

    [StringLength(500)]
    public string? Observacoes { get; set; }

    // Propriedades calculadas
    public string CpfCnpjFormatado
    {
        get
        {
            var numeros = new string(CpfCnpj?.Where(char.IsDigit).ToArray() ?? Array.Empty<char>());
            if (numeros.Length == 11)
                return Convert.ToUInt64(numeros).ToString(@"000\.000\.000\-00");
            if (numeros.Length == 14)
                return Convert.ToUInt64(numeros).ToString(@"00\.000\.000\/0000\-00");
            return CpfCnpj ?? string.Empty;
        }
    }

    public string NomeExibicao => !string.IsNullOrEmpty(NomeFantasia) ? NomeFantasia : Nome;
}
