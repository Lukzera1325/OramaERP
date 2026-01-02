using Orama.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace Orama.Web.Models;

/// <summary>
/// ViewModel para cadastro e edição de clientes
/// </summary>
public class ClienteViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Nome/Razão Social é obrigatório")]
    [StringLength(200, ErrorMessage = "Nome/Razão Social deve ter no máximo 200 caracteres")]
    [Display(Name = "Nome/Razão Social")]
    public string Nome { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "Nome Fantasia deve ter no máximo 200 caracteres")]
    [Display(Name = "Nome Fantasia")]
    public string? NomeFantasia { get; set; }

    [Required(ErrorMessage = "Tipo de pessoa é obrigatório")]
    [Display(Name = "Tipo de Pessoa")]
    public TipoPessoa TipoPessoa { get; set; }

    [Required(ErrorMessage = "CPF/CNPJ é obrigatório")]
    [StringLength(18, ErrorMessage = "CPF/CNPJ deve ter no máximo 18 caracteres")]
    [Display(Name = "CPF/CNPJ")]
    public string CpfCnpj { get; set; } = string.Empty;

    [StringLength(20, ErrorMessage = "RG/IE deve ter no máximo 20 caracteres")]
    [Display(Name = "RG/Inscrição Estadual")]
    public string? RgIe { get; set; }

    [EmailAddress(ErrorMessage = "Email inválido")]
    [StringLength(150, ErrorMessage = "Email deve ter no máximo 150 caracteres")]
    [Display(Name = "Email")]
    public string? Email { get; set; }

    [StringLength(20, ErrorMessage = "Telefone deve ter no máximo 20 caracteres")]
    [Display(Name = "Telefone")]
    public string? Telefone { get; set; }

    [StringLength(20, ErrorMessage = "Celular deve ter no máximo 20 caracteres")]
    [Display(Name = "Celular")]
    public string? Celular { get; set; }

    // Endereço
    [StringLength(10, ErrorMessage = "CEP deve ter no máximo 10 caracteres")]
    [Display(Name = "CEP")]
    public string? Cep { get; set; }

    [StringLength(200, ErrorMessage = "Logradouro deve ter no máximo 200 caracteres")]
    [Display(Name = "Logradouro")]
    public string? Logradouro { get; set; }

    [StringLength(10, ErrorMessage = "Número deve ter no máximo 10 caracteres")]
    [Display(Name = "Número")]
    public string? Numero { get; set; }

    [StringLength(100, ErrorMessage = "Complemento deve ter no máximo 100 caracteres")]
    [Display(Name = "Complemento")]
    public string? Complemento { get; set; }

    [StringLength(100, ErrorMessage = "Bairro deve ter no máximo 100 caracteres")]
    [Display(Name = "Bairro")]
    public string? Bairro { get; set; }

    [StringLength(100, ErrorMessage = "Cidade deve ter no máximo 100 caracteres")]
    [Display(Name = "Cidade")]
    public string? Cidade { get; set; }

    [StringLength(2, ErrorMessage = "UF deve ter 2 caracteres")]
    [Display(Name = "UF")]
    public string? Uf { get; set; }

    // Informações Comerciais
    [Display(Name = "Limite de Crédito")]
    [Range(0, double.MaxValue, ErrorMessage = "Limite de crédito deve ser maior ou igual a zero")]
    public decimal LimiteCredito { get; set; }

    [StringLength(500, ErrorMessage = "Observações deve ter no máximo 500 caracteres")]
    [Display(Name = "Observações")]
    public string? Observacoes { get; set; }

    // Propriedades para exibição
    public DateTime DataCriacao { get; set; }
    public bool Ativo { get; set; } = true;

    /// <summary>
    /// Indica se é um novo cliente
    /// </summary>
    public bool IsNovoCliente => Id == 0;

    /// <summary>
    /// Indica se é pessoa física
    /// </summary>
    public bool IsPessoaFisica => TipoPessoa == TipoPessoa.Fisica;

    /// <summary>
    /// Indica se é pessoa jurídica
    /// </summary>
    public bool IsPessoaJuridica => TipoPessoa == TipoPessoa.Juridica;

    /// <summary>
    /// Converte ViewModel para Entity
    /// </summary>
    public Cliente ToEntity()
    {
        return new Cliente
        {
            Id = Id,
            Nome = Nome,
            NomeFantasia = NomeFantasia,
            TipoPessoa = TipoPessoa,
            CpfCnpj = CpfCnpj,
            RgIe = RgIe,
            Email = Email,
            Telefone = Telefone,
            Celular = Celular,
            Cep = Cep,
            Logradouro = Logradouro,
            Numero = Numero,
            Complemento = Complemento,
            Bairro = Bairro,
            Cidade = Cidade,
            Uf = Uf,
            LimiteCredito = LimiteCredito,
            Observacoes = Observacoes,
            Ativo = Ativo
        };
    }

    /// <summary>
    /// Cria ViewModel a partir de Entity
    /// </summary>
    public static ClienteViewModel FromEntity(Cliente cliente)
    {
        return new ClienteViewModel
        {
            Id = cliente.Id,
            Nome = cliente.Nome,
            NomeFantasia = cliente.NomeFantasia,
            TipoPessoa = cliente.TipoPessoa,
            CpfCnpj = cliente.CpfCnpj,
            RgIe = cliente.RgIe,
            Email = cliente.Email,
            Telefone = cliente.Telefone,
            Celular = cliente.Celular,
            Cep = cliente.Cep,
            Logradouro = cliente.Logradouro,
            Numero = cliente.Numero,
            Complemento = cliente.Complemento,
            Bairro = cliente.Bairro,
            Cidade = cliente.Cidade,
            Uf = cliente.Uf,
            LimiteCredito = cliente.LimiteCredito,
            Observacoes = cliente.Observacoes,
            DataCriacao = cliente.DataCriacao,
            Ativo = cliente.Ativo
        };
    }
}