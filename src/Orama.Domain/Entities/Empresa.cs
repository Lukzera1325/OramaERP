using System.ComponentModel.DataAnnotations;

namespace Orama.Domain.Entities;

/// <summary>
/// Entidade que representa uma empresa/cliente do sistema (Multi-Tenant)
/// </summary>
public class Empresa : BaseEntity
{
    [Required(ErrorMessage = "Razão Social é obrigatória")]
    [StringLength(200, ErrorMessage = "Razão Social deve ter no máximo 200 caracteres")]
    public string RazaoSocial { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "Nome Fantasia deve ter no máximo 200 caracteres")]
    public string? NomeFantasia { get; set; }

    [Required(ErrorMessage = "CNPJ é obrigatório")]
    [StringLength(18, ErrorMessage = "CNPJ deve ter no máximo 18 caracteres")]
    public string Cnpj { get; set; } = string.Empty;

    [StringLength(20, ErrorMessage = "IE deve ter no máximo 20 caracteres")]
    public string? InscricaoEstadual { get; set; }

    [StringLength(20, ErrorMessage = "IM deve ter no máximo 20 caracteres")]
    public string? InscricaoMunicipal { get; set; }

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

    // Contato
    [StringLength(150)]
    public string? Email { get; set; }

    [StringLength(20)]
    public string? Telefone { get; set; }

    // Configurações
    public int RegimeTributario { get; set; } = 1; // 1=Simples, 2=Lucro Presumido, 3=Lucro Real

    // Relacionamentos
    public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
    public virtual ICollection<Cliente> Clientes { get; set; } = new List<Cliente>();
    public virtual ICollection<Fornecedor> Fornecedores { get; set; } = new List<Fornecedor>();
    public virtual ICollection<Produto> Produtos { get; set; } = new List<Produto>();

    // Propriedades calculadas
    public string NomeExibicao => !string.IsNullOrEmpty(NomeFantasia) ? NomeFantasia : RazaoSocial;
}

/// <summary>
/// Relacionamento Usuário-Empresa (um usuário pode acessar várias empresas)
/// </summary>
public class UsuarioEmpresa
{
    public int UsuarioId { get; set; }
    public virtual Usuario Usuario { get; set; } = null!;

    public int EmpresaId { get; set; }
    public virtual Empresa Empresa { get; set; } = null!;

    public bool IsAdmin { get; set; } // Admin da empresa específica
    public DateTime DataVinculo { get; set; } = DateTime.UtcNow;
}
