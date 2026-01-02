using System.ComponentModel.DataAnnotations;

namespace Orama.Domain.Entities;

/// <summary>
/// Entidade que representa um cliente
/// </summary>
public class Cliente : BaseEntity
{
    // Multi-Tenant
    public int EmpresaId { get; set; }
    public virtual Empresa? Empresa { get; set; }

    [Required(ErrorMessage = "Nome/Razão Social é obrigatório")]
    [StringLength(200, ErrorMessage = "Nome/Razão Social deve ter no máximo 200 caracteres")]
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

    [EmailAddress(ErrorMessage = "Email inválido")]
    [StringLength(150, ErrorMessage = "Email deve ter no máximo 150 caracteres")]
    public string? Email { get; set; }

    [StringLength(20, ErrorMessage = "Telefone deve ter no máximo 20 caracteres")]
    public string? Telefone { get; set; }

    [StringLength(20, ErrorMessage = "Celular deve ter no máximo 20 caracteres")]
    public string? Celular { get; set; }

    // Endereço Principal
    [StringLength(10, ErrorMessage = "CEP deve ter no máximo 10 caracteres")]
    public string? Cep { get; set; }

    [StringLength(200, ErrorMessage = "Logradouro deve ter no máximo 200 caracteres")]
    public string? Logradouro { get; set; }

    // Propriedade para compatibilidade com API
    public string? Endereco 
    { 
        get => Logradouro; 
        set => Logradouro = value; 
    }

    // Propriedade para compatibilidade com API  
    public string? Estado 
    { 
        get => Uf; 
        set => Uf = value; 
    }

    [StringLength(10, ErrorMessage = "Número deve ter no máximo 10 caracteres")]
    public string? Numero { get; set; }

    [StringLength(100, ErrorMessage = "Complemento deve ter no máximo 100 caracteres")]
    public string? Complemento { get; set; }

    [StringLength(100, ErrorMessage = "Bairro deve ter no máximo 100 caracteres")]
    public string? Bairro { get; set; }

    [StringLength(100, ErrorMessage = "Cidade deve ter no máximo 100 caracteres")]
    public string? Cidade { get; set; }

    [StringLength(2, ErrorMessage = "UF deve ter 2 caracteres")]
    public string? Uf { get; set; }

    // Informações Comerciais
    public decimal LimiteCredito { get; set; }

    [StringLength(500, ErrorMessage = "Observações deve ter no máximo 500 caracteres")]
    public string? Observacoes { get; set; }

    // Propriedades calculadas
    public bool IsPessoaFisica => TipoPessoa == TipoPessoa.Fisica;
    public bool IsPessoaJuridica => TipoPessoa == TipoPessoa.Juridica;

    public string CpfCnpjFormatado
    {
        get
        {
            if (string.IsNullOrWhiteSpace(CpfCnpj))
                return string.Empty;

            var numeros = new string(CpfCnpj.Where(char.IsDigit).ToArray());

            if (numeros.Length == 11) // CPF
            {
                return $"{numeros.Substring(0, 3)}.{numeros.Substring(3, 3)}.{numeros.Substring(6, 3)}-{numeros.Substring(9, 2)}";
            }
            else if (numeros.Length == 14) // CNPJ
            {
                return $"{numeros.Substring(0, 2)}.{numeros.Substring(2, 3)}.{numeros.Substring(5, 3)}/{numeros.Substring(8, 4)}-{numeros.Substring(12, 2)}";
            }

            return CpfCnpj;
        }
    }

    public string EnderecoCompleto
    {
        get
        {
            var endereco = new List<string>();

            if (!string.IsNullOrWhiteSpace(Logradouro))
                endereco.Add(Logradouro);

            if (!string.IsNullOrWhiteSpace(Numero))
                endereco.Add($"nº {Numero}");

            if (!string.IsNullOrWhiteSpace(Complemento))
                endereco.Add(Complemento);

            if (!string.IsNullOrWhiteSpace(Bairro))
                endereco.Add(Bairro);

            if (!string.IsNullOrWhiteSpace(Cidade) && !string.IsNullOrWhiteSpace(Uf))
                endereco.Add($"{Cidade}/{Uf}");

            if (!string.IsNullOrWhiteSpace(Cep))
                endereco.Add($"CEP: {Cep}");

            return string.Join(", ", endereco);
        }
    }
}

/// <summary>
/// Tipo de pessoa (Física ou Jurídica)
/// </summary>
public enum TipoPessoa
{
    Fisica = 1,
    Juridica = 2
}