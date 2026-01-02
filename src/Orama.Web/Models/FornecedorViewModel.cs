using System.ComponentModel.DataAnnotations;
using Orama.Domain.Entities;

namespace Orama.Web.Models;

/// <summary>
/// ViewModel para fornecedor
/// </summary>
public class FornecedorViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Nome é obrigatório")]
    [StringLength(200, ErrorMessage = "Nome deve ter no máximo 200 caracteres")]
    [Display(Name = "Nome/Razão Social")]
    public string Nome { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "Nome fantasia deve ter no máximo 200 caracteres")]
    [Display(Name = "Nome Fantasia")]
    public string? NomeFantasia { get; set; }

    [Required(ErrorMessage = "Tipo de pessoa é obrigatório")]
    [Display(Name = "Tipo de Pessoa")]
    public TipoPessoa TipoPessoa { get; set; } = TipoPessoa.Juridica;

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

    [StringLength(10, ErrorMessage = "CEP deve ter no máximo 10 caracteres")]
    [Display(Name = "CEP")]
    public string? Cep { get; set; }

    [StringLength(200, ErrorMessage = "Logradouro deve ter no máximo 200 caracteres")]
    [Display(Name = "Logradouro")]
    public string? Logradouro { get; set; }

    [StringLength(20, ErrorMessage = "Número deve ter no máximo 20 caracteres")]
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

    [StringLength(500, ErrorMessage = "Observações deve ter no máximo 500 caracteres")]
    [Display(Name = "Observações")]
    public string? Observacoes { get; set; }

    public DateTime DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }
    public bool Ativo { get; set; } = true;

    /// <summary>
    /// Converte ViewModel para Entity
    /// </summary>
    public Fornecedor ToEntity(int empresaId)
    {
        return new Fornecedor
        {
            Id = Id,
            EmpresaId = empresaId,
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
            Observacoes = Observacoes,
            DataCriacao = DataCriacao,
            DataAtualizacao = DataAtualizacao,
            Ativo = Ativo
        };
    }

    /// <summary>
    /// Cria ViewModel a partir de Entity
    /// </summary>
    public static FornecedorViewModel FromEntity(Fornecedor fornecedor)
    {
        return new FornecedorViewModel
        {
            Id = fornecedor.Id,
            Nome = fornecedor.Nome,
            NomeFantasia = fornecedor.NomeFantasia,
            TipoPessoa = fornecedor.TipoPessoa,
            CpfCnpj = fornecedor.CpfCnpj,
            RgIe = fornecedor.RgIe,
            Email = fornecedor.Email,
            Telefone = fornecedor.Telefone,
            Celular = fornecedor.Celular,
            Cep = fornecedor.Cep,
            Logradouro = fornecedor.Logradouro,
            Numero = fornecedor.Numero,
            Complemento = fornecedor.Complemento,
            Bairro = fornecedor.Bairro,
            Cidade = fornecedor.Cidade,
            Uf = fornecedor.Uf,
            Observacoes = fornecedor.Observacoes,
            DataCriacao = fornecedor.DataCriacao,
            DataAtualizacao = fornecedor.DataAtualizacao,
            Ativo = fornecedor.Ativo
        };
    }
}