using Microsoft.EntityFrameworkCore;
using Orama.Domain.Entities;
using Orama.Domain.Entities.Fiscal;

namespace Orama.Infra.Data.Context;

/// <summary>
/// Contexto principal do Entity Framework para o sistema Orama
/// </summary>
public class OramaDbContext : DbContext
{
    public OramaDbContext(DbContextOptions<OramaDbContext> options) : base(options)
    {
    }

    // Segurança
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Perfil> Perfis { get; set; }
    public DbSet<Permissao> Permissoes { get; set; }
    public DbSet<PerfilPermissao> PerfilPermissoes { get; set; }
    
    // Multi-Tenant
    public DbSet<Empresa> Empresas { get; set; }
    public DbSet<UsuarioEmpresa> UsuarioEmpresas { get; set; }
    
    // Cadastros
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<Fornecedor> Fornecedores { get; set; }
    public DbSet<Produto> Produtos { get; set; }
    public DbSet<Categoria> Categorias { get; set; }
    
    // Financeiro
    public DbSet<ContaBancaria> ContasBancarias { get; set; }
    public DbSet<ContaReceber> ContasReceber { get; set; }
    public DbSet<ContaPagar> ContasPagar { get; set; }
    public DbSet<MovimentacaoFinanceira> MovimentacoesFinanceiras { get; set; }
    
    // Vendas
    public DbSet<Venda> Vendas { get; set; }
    public DbSet<VendaItem> VendaItens { get; set; }
    
    // Compras
    public DbSet<Compra> Compras { get; set; }
    public DbSet<CompraItem> CompraItens { get; set; }
    
    // Estoque
    public DbSet<MovimentacaoEstoque> MovimentacoesEstoque { get; set; }
    
    // Produção SIMPLIFICADA
    public DbSet<OrdemProducao> OrdensProducao { get; set; }
    public DbSet<OrdemProducaoItem> OrdemProducaoItens { get; set; }

    // Produção Industrial - Estrutura de Produto (BOM)
    public DbSet<EstruturaProduto> EstruturasProdutos { get; set; }

    // Fiscal
    public DbSet<NotaFiscal> NotasFiscais { get; set; }
    public DbSet<NotaFiscalItem> NotasFiscaisItens { get; set; }

    // Alertas de Margem
    public DbSet<AlertaMargem> AlertasMargem { get; set; }

    // Explicações de Resultado
    public DbSet<ExplicacaoResultado> ExplicacoesResultados { get; set; }

    // Apoio à Decisão
    public DbSet<SugestaoAcao> SugestoesAcao { get; set; }
    public DbSet<DecisaoGerencial> DecisoesGerenciais { get; set; }
    public DbSet<ChecklistFechamento> ChecklistsFechamento { get; set; }
    public DbSet<ChecklistItem> ChecklistItens { get; set; }

    // Configurações Fiscais (ISOLADAS DO CORE)
    public DbSet<EmpresaFiscalConfig> EmpresasFiscaisConfig { get; set; }
    public DbSet<ProdutoFiscalConfig> ProdutosFiscaisConfig { get; set; }
    public DbSet<OperacaoFiscalConfig> OperacoesFiscaisConfig { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        ConfigurarRelacionamentos(modelBuilder);
        SeedData(modelBuilder);
    }

    private static void ConfigurarRelacionamentos(ModelBuilder modelBuilder)
    {
        // Usuario -> Perfil
        modelBuilder.Entity<Usuario>()
            .HasOne(u => u.Perfil)
            .WithMany(p => p.Usuarios)
            .HasForeignKey(u => u.PerfilId)
            .OnDelete(DeleteBehavior.Restrict);

        // UsuarioEmpresa (N:N)
        modelBuilder.Entity<UsuarioEmpresa>()
            .HasKey(ue => new { ue.UsuarioId, ue.EmpresaId });

        modelBuilder.Entity<UsuarioEmpresa>()
            .HasOne(ue => ue.Usuario)
            .WithMany(u => u.Empresas)
            .HasForeignKey(ue => ue.UsuarioId);

        modelBuilder.Entity<UsuarioEmpresa>()
            .HasOne(ue => ue.Empresa)
            .WithMany()
            .HasForeignKey(ue => ue.EmpresaId);

        // PerfilPermissao
        modelBuilder.Entity<PerfilPermissao>()
            .HasOne(pp => pp.Perfil)
            .WithMany(p => p.PerfilPermissoes)
            .HasForeignKey(pp => pp.PerfilId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PerfilPermissao>()
            .HasOne(pp => pp.Permissao)
            .WithMany(p => p.PerfilPermissoes)
            .HasForeignKey(pp => pp.PermissaoId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PerfilPermissao>()
            .HasIndex(pp => new { pp.PerfilId, pp.PermissaoId })
            .IsUnique();

        // Cliente -> Empresa
        modelBuilder.Entity<Cliente>()
            .HasOne(c => c.Empresa)
            .WithMany(e => e.Clientes)
            .HasForeignKey(c => c.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        // Fornecedor -> Empresa
        modelBuilder.Entity<Fornecedor>()
            .HasOne(f => f.Empresa)
            .WithMany(e => e.Fornecedores)
            .HasForeignKey(f => f.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        // Produto -> Empresa
        modelBuilder.Entity<Produto>()
            .HasOne(p => p.Empresa)
            .WithMany(e => e.Produtos)
            .HasForeignKey(p => p.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        // Produto -> Categoria
        modelBuilder.Entity<Produto>()
            .HasOne(p => p.CategoriaNavigation)
            .WithMany(c => c.Produtos)
            .HasForeignKey(p => p.CategoriaId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Produto>()
            .HasIndex(p => new { p.EmpresaId, p.Codigo })
            .IsUnique();

        // Categoria -> Empresa
        modelBuilder.Entity<Categoria>()
            .HasOne(c => c.Empresa)
            .WithMany()
            .HasForeignKey(c => c.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        // Venda -> Cliente
        modelBuilder.Entity<Venda>()
            .HasOne(v => v.Cliente)
            .WithMany()
            .HasForeignKey(v => v.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        // VendaItem -> Venda
        modelBuilder.Entity<VendaItem>()
            .HasOne(vi => vi.Venda)
            .WithMany(v => v.Itens)
            .HasForeignKey(vi => vi.VendaId)
            .OnDelete(DeleteBehavior.Cascade);

        // VendaItem -> Produto
        modelBuilder.Entity<VendaItem>()
            .HasOne(vi => vi.Produto)
            .WithMany()
            .HasForeignKey(vi => vi.ProdutoId)
            .OnDelete(DeleteBehavior.Restrict);

        // Compra -> Fornecedor
        modelBuilder.Entity<Compra>()
            .HasOne(c => c.Fornecedor)
            .WithMany()
            .HasForeignKey(c => c.FornecedorId)
            .OnDelete(DeleteBehavior.Restrict);

        // CompraItem -> Compra
        modelBuilder.Entity<CompraItem>()
            .HasOne(ci => ci.Compra)
            .WithMany(c => c.Itens)
            .HasForeignKey(ci => ci.CompraId)
            .OnDelete(DeleteBehavior.Cascade);

        // CompraItem -> Produto
        modelBuilder.Entity<CompraItem>()
            .HasOne(ci => ci.Produto)
            .WithMany()
            .HasForeignKey(ci => ci.ProdutoId)
            .OnDelete(DeleteBehavior.Restrict);

        // ContaReceber -> Cliente
        modelBuilder.Entity<ContaReceber>()
            .HasOne(cr => cr.Cliente)
            .WithMany()
            .HasForeignKey(cr => cr.ClienteId)
            .OnDelete(DeleteBehavior.SetNull);

        // ContaPagar -> Fornecedor
        modelBuilder.Entity<ContaPagar>()
            .HasOne(cp => cp.Fornecedor)
            .WithMany()
            .HasForeignKey(cp => cp.FornecedorId)
            .OnDelete(DeleteBehavior.SetNull);

        // MovimentacaoFinanceira -> ContaBancaria
        modelBuilder.Entity<MovimentacaoFinanceira>()
            .HasOne(mf => mf.ContaBancaria)
            .WithMany(cb => cb.Movimentacoes)
            .HasForeignKey(mf => mf.ContaBancariaId)
            .OnDelete(DeleteBehavior.Restrict);

        // MovimentacaoEstoque -> Produto
        modelBuilder.Entity<MovimentacaoEstoque>()
            .HasOne(me => me.Produto)
            .WithMany()
            .HasForeignKey(me => me.ProdutoId)
            .OnDelete(DeleteBehavior.Restrict);

        // MovimentacaoEstoque -> Usuario (opcional)
        modelBuilder.Entity<MovimentacaoEstoque>()
            .HasOne(me => me.Usuario)
            .WithMany()
            .HasForeignKey(me => me.UsuarioId)
            .OnDelete(DeleteBehavior.SetNull);

        // Venda -> Usuario (Vendedor - opcional)
        modelBuilder.Entity<Venda>()
            .HasOne(v => v.Vendedor)
            .WithMany()
            .HasForeignKey(v => v.VendedorId)
            .OnDelete(DeleteBehavior.SetNull);

        // === RELACIONAMENTOS DE PRODUÇÃO ===
        
        // OrdemProducao -> Empresa
        modelBuilder.Entity<OrdemProducao>()
            .HasOne(op => op.Empresa)
            .WithMany()
            .HasForeignKey(op => op.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        // OrdemProducao -> Produto
        modelBuilder.Entity<OrdemProducao>()
            .HasOne(op => op.Produto)
            .WithMany()
            .HasForeignKey(op => op.ProdutoId)
            .OnDelete(DeleteBehavior.Restrict);

        // OrdemProducao -> Usuario (Criação)
        modelBuilder.Entity<OrdemProducao>()
            .HasOne(op => op.UsuarioCriacao)
            .WithMany()
            .HasForeignKey(op => op.UsuarioCriacaoId)
            .OnDelete(DeleteBehavior.Restrict);

        // OrdemProducaoItem -> OrdemProducao
        modelBuilder.Entity<OrdemProducaoItem>()
            .HasOne(opi => opi.OrdemProducao)
            .WithMany(op => op.Itens)
            .HasForeignKey(opi => opi.OrdemProducaoId)
            .OnDelete(DeleteBehavior.Cascade);

        // OrdemProducaoItem -> Produto
        modelBuilder.Entity<OrdemProducaoItem>()
            .HasOne(opi => opi.Produto)
            .WithMany()
            .HasForeignKey(opi => opi.ProdutoId)
            .OnDelete(DeleteBehavior.Restrict);

        // Índices únicos
        modelBuilder.Entity<OrdemProducao>()
            .HasIndex(op => new { op.EmpresaId, op.Numero })
            .IsUnique();

        // === RELACIONAMENTOS FISCAIS ===
        
        // NotaFiscal -> Empresa
        modelBuilder.Entity<NotaFiscal>()
            .HasOne(nf => nf.Empresa)
            .WithMany()
            .HasForeignKey(nf => nf.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        // NotaFiscal -> Cliente (opcional)
        modelBuilder.Entity<NotaFiscal>()
            .HasOne(nf => nf.Cliente)
            .WithMany()
            .HasForeignKey(nf => nf.ClienteId)
            .OnDelete(DeleteBehavior.SetNull);

        // NotaFiscal -> Fornecedor (opcional)
        modelBuilder.Entity<NotaFiscal>()
            .HasOne(nf => nf.Fornecedor)
            .WithMany()
            .HasForeignKey(nf => nf.FornecedorId)
            .OnDelete(DeleteBehavior.SetNull);

        // NotaFiscal -> Venda (opcional)
        modelBuilder.Entity<NotaFiscal>()
            .HasOne(nf => nf.Venda)
            .WithMany()
            .HasForeignKey(nf => nf.VendaId)
            .OnDelete(DeleteBehavior.SetNull);

        // NotaFiscal -> Compra (opcional)
        modelBuilder.Entity<NotaFiscal>()
            .HasOne(nf => nf.Compra)
            .WithMany()
            .HasForeignKey(nf => nf.CompraId)
            .OnDelete(DeleteBehavior.SetNull);

        // NotaFiscalItem -> NotaFiscal
        modelBuilder.Entity<NotaFiscalItem>()
            .HasOne(nfi => nfi.NotaFiscal)
            .WithMany(nf => nf.Itens)
            .HasForeignKey(nfi => nfi.NotaFiscalId)
            .OnDelete(DeleteBehavior.Cascade);

        // NotaFiscalItem -> Produto
        modelBuilder.Entity<NotaFiscalItem>()
            .HasOne(nfi => nfi.Produto)
            .WithMany()
            .HasForeignKey(nfi => nfi.ProdutoId)
            .OnDelete(DeleteBehavior.Restrict);

        // Índices únicos para NotaFiscal
        modelBuilder.Entity<NotaFiscal>()
            .HasIndex(nf => new { nf.EmpresaId, nf.Numero, nf.Serie, nf.Tipo })
            .IsUnique();

        modelBuilder.Entity<NotaFiscal>()
            .HasIndex(nf => nf.ChaveAcesso)
            .IsUnique()
            .HasFilter("[ChaveAcesso] IS NOT NULL AND [ChaveAcesso] != ''");

        // === RELACIONAMENTOS DE ALERTAS DE MARGEM ===
        
        // AlertaMargem -> Empresa
        modelBuilder.Entity<AlertaMargem>()
            .HasOne(a => a.Empresa)
            .WithMany()
            .HasForeignKey(a => a.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        // AlertaMargem -> Venda
        modelBuilder.Entity<AlertaMargem>()
            .HasOne(a => a.Venda)
            .WithMany()
            .HasForeignKey(a => a.VendaId)
            .OnDelete(DeleteBehavior.Restrict);

        // AlertaMargem -> Produto (opcional)
        modelBuilder.Entity<AlertaMargem>()
            .HasOne(a => a.Produto)
            .WithMany()
            .HasForeignKey(a => a.ProdutoId)
            .OnDelete(DeleteBehavior.SetNull);

        // AlertaMargem -> Usuario (resolução - opcional)
        modelBuilder.Entity<AlertaMargem>()
            .HasOne(a => a.UsuarioResolucao)
            .WithMany()
            .HasForeignKey(a => a.UsuarioResolucaoId)
            .OnDelete(DeleteBehavior.SetNull);

        // === RELACIONAMENTOS DE EXPLICAÇÕES DE RESULTADO ===
        
        // ExplicacaoResultado -> Empresa
        modelBuilder.Entity<ExplicacaoResultado>()
            .HasOne(e => e.Empresa)
            .WithMany()
            .HasForeignKey(e => e.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        // ExplicacaoResultado -> Venda
        modelBuilder.Entity<ExplicacaoResultado>()
            .HasOne(e => e.Venda)
            .WithMany()
            .HasForeignKey(e => e.VendaId)
            .OnDelete(DeleteBehavior.Restrict);

        // ExplicacaoResultado -> Produto (opcional)
        modelBuilder.Entity<ExplicacaoResultado>()
            .HasOne(e => e.Produto)
            .WithMany()
            .HasForeignKey(e => e.ProdutoId)
            .OnDelete(DeleteBehavior.SetNull);

        // === RELACIONAMENTOS DE APOIO À DECISÃO ===
        
        // SugestaoAcao -> Empresa
        modelBuilder.Entity<SugestaoAcao>()
            .HasOne(s => s.Empresa)
            .WithMany()
            .HasForeignKey(s => s.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        // SugestaoAcao -> Usuario (resolução - opcional)
        modelBuilder.Entity<SugestaoAcao>()
            .HasOne(s => s.UsuarioResolucao)
            .WithMany()
            .HasForeignKey(s => s.UsuarioResolucaoId)
            .OnDelete(DeleteBehavior.SetNull);

        // DecisaoGerencial -> Empresa
        modelBuilder.Entity<DecisaoGerencial>()
            .HasOne(d => d.Empresa)
            .WithMany()
            .HasForeignKey(d => d.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        // DecisaoGerencial -> Usuario
        modelBuilder.Entity<DecisaoGerencial>()
            .HasOne(d => d.Usuario)
            .WithMany()
            .HasForeignKey(d => d.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        // ChecklistFechamento -> Empresa
        modelBuilder.Entity<ChecklistFechamento>()
            .HasOne(c => c.Empresa)
            .WithMany()
            .HasForeignKey(c => c.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        // ChecklistFechamento -> Usuario (conclusão - opcional)
        modelBuilder.Entity<ChecklistFechamento>()
            .HasOne(c => c.UsuarioConclusao)
            .WithMany()
            .HasForeignKey(c => c.UsuarioConclusaoId)
            .OnDelete(DeleteBehavior.SetNull);

        // ChecklistItem -> ChecklistFechamento
        modelBuilder.Entity<ChecklistItem>()
            .HasOne(i => i.ChecklistFechamento)
            .WithMany(c => c.Itens)
            .HasForeignKey(i => i.ChecklistFechamentoId)
            .OnDelete(DeleteBehavior.Cascade);

        // ChecklistItem -> Usuario (conclusão - opcional)
        modelBuilder.Entity<ChecklistItem>()
            .HasOne(i => i.UsuarioConclusao)
            .WithMany()
            .HasForeignKey(i => i.UsuarioConclusaoId)
            .OnDelete(DeleteBehavior.SetNull);

        // Índices únicos para ChecklistFechamento (um por período por empresa)
        modelBuilder.Entity<ChecklistFechamento>()
            .HasIndex(c => new { c.EmpresaId, c.Ano, c.Mes })
            .IsUnique();
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        // Empresas de exemplo
        modelBuilder.Entity<Empresa>().HasData(
            new Empresa
            {
                Id = 1,
                RazaoSocial = "Empresa Demonstração Ltda",
                NomeFantasia = "Orama Demo",
                Cnpj = "00.000.000/0001-00",
                Email = "contato@oramademo.com.br",
                DataCriacao = DateTime.UtcNow,
                Ativo = true
            },
            new Empresa
            {
                Id = 2,
                RazaoSocial = "Tech Solutions Brasil Ltda",
                NomeFantasia = "Tech Solutions",
                Cnpj = "11.111.111/0001-11",
                Email = "contato@techsolutions.com.br",
                DataCriacao = DateTime.UtcNow,
                Ativo = true
            },
            new Empresa
            {
                Id = 3,
                RazaoSocial = "Comércio Geral ABC Ltda",
                NomeFantasia = "ABC Comércio",
                Cnpj = "22.222.222/0001-22",
                Email = "contato@abccomercio.com.br",
                DataCriacao = DateTime.UtcNow,
                Ativo = true
            }
        );

        // Perfil Administrador
        modelBuilder.Entity<Perfil>().HasData(
            new Perfil
            {
                Id = 1,
                Nome = "Administrador",
                Descricao = "Acesso total ao sistema",
                DataCriacao = DateTime.UtcNow,
                Ativo = true
            }
        );

        // Usuário Administrador (Super Admin)
        modelBuilder.Entity<Usuario>().HasData(
            new Usuario
            {
                Id = 1,
                Nome = "Administrador",
                Email = "admin@orama.com.br",
                Senha = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                PerfilId = 1,
                EmpresaId = 1, // Empresa principal
                IsSuperAdmin = true,
                DataCriacao = DateTime.UtcNow,
                Ativo = true
            }
        );

        // Vincular usuário às empresas (Super Admin tem acesso a todas)
        modelBuilder.Entity<UsuarioEmpresa>().HasData(
            new UsuarioEmpresa
            {
                UsuarioId = 1,
                EmpresaId = 1,
                IsAdmin = true,
                DataVinculo = DateTime.UtcNow
            },
            new UsuarioEmpresa
            {
                UsuarioId = 1,
                EmpresaId = 2,
                IsAdmin = true,
                DataVinculo = DateTime.UtcNow
            },
            new UsuarioEmpresa
            {
                UsuarioId = 1,
                EmpresaId = 3,
                IsAdmin = true,
                DataVinculo = DateTime.UtcNow
            }
        );

        // Permissões
        var permissoes = new[]
        {
            new Permissao { Id = 1, Nome = "Usuarios.Visualizar", Modulo = "Segurança", Acao = "Visualizar", Descricao = "Visualizar usuários" },
            new Permissao { Id = 2, Nome = "Usuarios.Incluir", Modulo = "Segurança", Acao = "Incluir", Descricao = "Incluir usuários" },
            new Permissao { Id = 3, Nome = "Usuarios.Alterar", Modulo = "Segurança", Acao = "Alterar", Descricao = "Alterar usuários" },
            new Permissao { Id = 4, Nome = "Usuarios.Excluir", Modulo = "Segurança", Acao = "Excluir", Descricao = "Excluir usuários" },
            new Permissao { Id = 5, Nome = "Clientes.Visualizar", Modulo = "Cadastros", Acao = "Visualizar", Descricao = "Visualizar clientes" },
            new Permissao { Id = 6, Nome = "Clientes.Incluir", Modulo = "Cadastros", Acao = "Incluir", Descricao = "Incluir clientes" },
            new Permissao { Id = 7, Nome = "Clientes.Alterar", Modulo = "Cadastros", Acao = "Alterar", Descricao = "Alterar clientes" },
            new Permissao { Id = 8, Nome = "Clientes.Excluir", Modulo = "Cadastros", Acao = "Excluir", Descricao = "Excluir clientes" },
            new Permissao { Id = 9, Nome = "Produtos.Visualizar", Modulo = "Cadastros", Acao = "Visualizar", Descricao = "Visualizar produtos" },
            new Permissao { Id = 10, Nome = "Produtos.Incluir", Modulo = "Cadastros", Acao = "Incluir", Descricao = "Incluir produtos" },
            new Permissao { Id = 11, Nome = "Produtos.Alterar", Modulo = "Cadastros", Acao = "Alterar", Descricao = "Alterar produtos" },
            new Permissao { Id = 12, Nome = "Produtos.Excluir", Modulo = "Cadastros", Acao = "Excluir", Descricao = "Excluir produtos" },
            new Permissao { Id = 13, Nome = "Fornecedores.Visualizar", Modulo = "Cadastros", Acao = "Visualizar", Descricao = "Visualizar fornecedores" },
            new Permissao { Id = 14, Nome = "Fornecedores.Incluir", Modulo = "Cadastros", Acao = "Incluir", Descricao = "Incluir fornecedores" },
            new Permissao { Id = 15, Nome = "Fornecedores.Alterar", Modulo = "Cadastros", Acao = "Alterar", Descricao = "Alterar fornecedores" },
            new Permissao { Id = 16, Nome = "Fornecedores.Excluir", Modulo = "Cadastros", Acao = "Excluir", Descricao = "Excluir fornecedores" },
            new Permissao { Id = 17, Nome = "Financeiro.Visualizar", Modulo = "Financeiro", Acao = "Visualizar", Descricao = "Visualizar financeiro" },
            new Permissao { Id = 18, Nome = "Financeiro.Incluir", Modulo = "Financeiro", Acao = "Incluir", Descricao = "Incluir lançamentos" },
            new Permissao { Id = 19, Nome = "Financeiro.Alterar", Modulo = "Financeiro", Acao = "Alterar", Descricao = "Alterar lançamentos" },
            new Permissao { Id = 20, Nome = "Financeiro.Excluir", Modulo = "Financeiro", Acao = "Excluir", Descricao = "Excluir lançamentos" },
            new Permissao { Id = 21, Nome = "Vendas.Visualizar", Modulo = "Vendas", Acao = "Visualizar", Descricao = "Visualizar vendas" },
            new Permissao { Id = 22, Nome = "Vendas.Incluir", Modulo = "Vendas", Acao = "Incluir", Descricao = "Incluir vendas" },
            new Permissao { Id = 23, Nome = "Vendas.Alterar", Modulo = "Vendas", Acao = "Alterar", Descricao = "Alterar vendas" },
            new Permissao { Id = 24, Nome = "Vendas.Excluir", Modulo = "Vendas", Acao = "Excluir", Descricao = "Excluir vendas" },
            new Permissao { Id = 25, Nome = "Compras.Visualizar", Modulo = "Compras", Acao = "Visualizar", Descricao = "Visualizar compras" },
            new Permissao { Id = 26, Nome = "Compras.Incluir", Modulo = "Compras", Acao = "Incluir", Descricao = "Incluir compras" },
            new Permissao { Id = 27, Nome = "Compras.Alterar", Modulo = "Compras", Acao = "Alterar", Descricao = "Alterar compras" },
            new Permissao { Id = 28, Nome = "Compras.Excluir", Modulo = "Compras", Acao = "Excluir", Descricao = "Excluir compras" },
            new Permissao { Id = 29, Nome = "Estoque.Visualizar", Modulo = "Estoque", Acao = "Visualizar", Descricao = "Visualizar estoque" },
            new Permissao { Id = 30, Nome = "Estoque.Movimentar", Modulo = "Estoque", Acao = "Movimentar", Descricao = "Movimentar estoque" },
        };

        foreach (var p in permissoes)
        {
            p.DataCriacao = DateTime.UtcNow;
            p.Ativo = true;
        }
        modelBuilder.Entity<Permissao>().HasData(permissoes);

        // Conceder todas as permissões ao Administrador
        var perfilPermissoes = permissoes.Select((p, i) => new PerfilPermissao
        {
            Id = i + 1,
            PerfilId = 1,
            PermissaoId = p.Id,
            Concedida = true,
            DataCriacao = DateTime.UtcNow,
            Ativo = true
        }).ToArray();
        modelBuilder.Entity<PerfilPermissao>().HasData(perfilPermissoes);

        // Categorias para cada empresa
        modelBuilder.Entity<Categoria>().HasData(
            // Empresa 1 - Orama Demo
            new Categoria { Id = 1, EmpresaId = 1, Nome = "Eletrônicos", Descricao = "Produtos eletrônicos", DataCriacao = DateTime.UtcNow, Ativo = true },
            new Categoria { Id = 2, EmpresaId = 1, Nome = "Informática", Descricao = "Produtos de informática", DataCriacao = DateTime.UtcNow, Ativo = true },
            new Categoria { Id = 3, EmpresaId = 1, Nome = "Móveis", Descricao = "Móveis e decoração", DataCriacao = DateTime.UtcNow, Ativo = true },
            
            // Empresa 2 - Tech Solutions
            new Categoria { Id = 4, EmpresaId = 2, Nome = "Software", Descricao = "Licenças de software", DataCriacao = DateTime.UtcNow, Ativo = true },
            new Categoria { Id = 5, EmpresaId = 2, Nome = "Hardware", Descricao = "Equipamentos de TI", DataCriacao = DateTime.UtcNow, Ativo = true },
            new Categoria { Id = 6, EmpresaId = 2, Nome = "Serviços", Descricao = "Serviços de consultoria", DataCriacao = DateTime.UtcNow, Ativo = true },
            
            // Empresa 3 - ABC Comércio
            new Categoria { Id = 7, EmpresaId = 3, Nome = "Alimentação", Descricao = "Produtos alimentícios", DataCriacao = DateTime.UtcNow, Ativo = true },
            new Categoria { Id = 8, EmpresaId = 3, Nome = "Limpeza", Descricao = "Produtos de limpeza", DataCriacao = DateTime.UtcNow, Ativo = true },
            new Categoria { Id = 9, EmpresaId = 3, Nome = "Higiene", Descricao = "Produtos de higiene pessoal", DataCriacao = DateTime.UtcNow, Ativo = true }
        );

        // Produtos de exemplo para cada empresa
        modelBuilder.Entity<Produto>().HasData(
            // Empresa 1 - Orama Demo
            new Produto
            {
                Id = 1,
                EmpresaId = 1,
                Codigo = "NOTEBOOK001",
                Descricao = "Notebook Dell Inspiron 15",
                Unidade = "UN",
                Ncm = "84713012",
                PrecoCusto = 2500.00m,
                PrecoVenda = 3200.00m,
                MargemLucro = 28.00m,
                ControlaEstoque = true,
                EstoqueAtual = 10,
                EstoqueMinimo = 2,
                EstoqueMaximo = 50,
                CategoriaId = 2,
                DataCriacao = DateTime.UtcNow,
                Ativo = true
            },
            new Produto
            {
                Id = 2,
                EmpresaId = 1,
                Codigo = "MOUSE001",
                Descricao = "Mouse Óptico USB",
                Unidade = "UN",
                Ncm = "84716070",
                PrecoCusto = 15.00m,
                PrecoVenda = 25.00m,
                MargemLucro = 66.67m,
                ControlaEstoque = true,
                EstoqueAtual = 50,
                EstoqueMinimo = 10,
                EstoqueMaximo = 200,
                CategoriaId = 2,
                DataCriacao = DateTime.UtcNow,
                Ativo = true
            },
            
            // Empresa 2 - Tech Solutions
            new Produto
            {
                Id = 3,
                EmpresaId = 2,
                Codigo = "OFFICE365",
                Descricao = "Microsoft Office 365 Business",
                Unidade = "LIC",
                Ncm = "85234910",
                PrecoCusto = 180.00m,
                PrecoVenda = 250.00m,
                MargemLucro = 38.89m,
                ControlaEstoque = false,
                EstoqueAtual = 0,
                EstoqueMinimo = 0,
                EstoqueMaximo = 0,
                CategoriaId = 4,
                DataCriacao = DateTime.UtcNow,
                Ativo = true
            },
            new Produto
            {
                Id = 4,
                EmpresaId = 2,
                Codigo = "SERVIDOR001",
                Descricao = "Servidor Dell PowerEdge T340",
                Unidade = "UN",
                Ncm = "84713012",
                PrecoCusto = 8500.00m,
                PrecoVenda = 12000.00m,
                MargemLucro = 41.18m,
                ControlaEstoque = true,
                EstoqueAtual = 3,
                EstoqueMinimo = 1,
                EstoqueMaximo = 10,
                CategoriaId = 5,
                DataCriacao = DateTime.UtcNow,
                Ativo = true
            },
            
            // Empresa 3 - ABC Comércio
            new Produto
            {
                Id = 5,
                EmpresaId = 3,
                Codigo = "ARROZ001",
                Descricao = "Arroz Branco Tipo 1 - 5kg",
                Unidade = "PCT",
                Ncm = "10063021",
                PrecoCusto = 12.50m,
                PrecoVenda = 18.90m,
                MargemLucro = 51.20m,
                ControlaEstoque = true,
                EstoqueAtual = 200,
                EstoqueMinimo = 50,
                EstoqueMaximo = 500,
                CategoriaId = 7,
                DataCriacao = DateTime.UtcNow,
                Ativo = true
            },
            new Produto
            {
                Id = 6,
                EmpresaId = 3,
                Codigo = "DETERGENTE001",
                Descricao = "Detergente Líquido 500ml",
                Unidade = "UN",
                Ncm = "34022000",
                PrecoCusto = 1.80m,
                PrecoVenda = 3.50m,
                MargemLucro = 94.44m,
                ControlaEstoque = true,
                EstoqueAtual = 150,
                EstoqueMinimo = 30,
                EstoqueMaximo = 300,
                CategoriaId = 8,
                DataCriacao = DateTime.UtcNow,
                Ativo = true
            }
        );

        // Contas Bancárias para cada empresa
        modelBuilder.Entity<ContaBancaria>().HasData(
            // Empresa 1 - Orama Demo
            new ContaBancaria
            {
                Id = 1,
                EmpresaId = 1,
                Descricao = "Caixa Geral",
                TipoConta = TipoConta.Caixa,
                SaldoInicial = 1000.00m,
                SaldoAtual = 1000.00m,
                ContaPadrao = true,
                DataCriacao = DateTime.UtcNow,
                Ativo = true
            },
            new ContaBancaria
            {
                Id = 2,
                EmpresaId = 1,
                Descricao = "Banco do Brasil - CC 12345-6",
                TipoConta = TipoConta.ContaCorrente,
                Banco = "001",
                Agencia = "1234",
                NumeroConta = "12345-6",
                SaldoInicial = 50000.00m,
                SaldoAtual = 50000.00m,
                ContaPadrao = false,
                DataCriacao = DateTime.UtcNow,
                Ativo = true
            },
            
            // Empresa 2 - Tech Solutions
            new ContaBancaria
            {
                Id = 3,
                EmpresaId = 2,
                Descricao = "Caixa Geral",
                TipoConta = TipoConta.Caixa,
                SaldoInicial = 2000.00m,
                SaldoAtual = 2000.00m,
                ContaPadrao = true,
                DataCriacao = DateTime.UtcNow,
                Ativo = true
            },
            new ContaBancaria
            {
                Id = 4,
                EmpresaId = 2,
                Descricao = "Itaú - CC 98765-4",
                TipoConta = TipoConta.ContaCorrente,
                Banco = "341",
                Agencia = "9876",
                NumeroConta = "98765-4",
                SaldoInicial = 120000.00m,
                SaldoAtual = 120000.00m,
                ContaPadrao = false,
                DataCriacao = DateTime.UtcNow,
                Ativo = true
            },
            
            // Empresa 3 - ABC Comércio
            new ContaBancaria
            {
                Id = 5,
                EmpresaId = 3,
                Descricao = "Caixa Geral",
                TipoConta = TipoConta.Caixa,
                SaldoInicial = 500.00m,
                SaldoAtual = 500.00m,
                ContaPadrao = true,
                DataCriacao = DateTime.UtcNow,
                Ativo = true
            },
            new ContaBancaria
            {
                Id = 6,
                EmpresaId = 3,
                Descricao = "Caixa Econômica - CC 55555-5",
                TipoConta = TipoConta.ContaCorrente,
                Banco = "104",
                Agencia = "5555",
                NumeroConta = "55555-5",
                SaldoInicial = 25000.00m,
                SaldoAtual = 25000.00m,
                ContaPadrao = false,
                DataCriacao = DateTime.UtcNow,
                Ativo = true
            }
        );
    }

    public override int SaveChanges()
    {
        AtualizarTimestamps();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        AtualizarTimestamps();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void AtualizarTimestamps()
    {
        var entries = ChangeTracker.Entries<BaseEntity>();
        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
                entry.Entity.DataCriacao = DateTime.UtcNow;
            else if (entry.State == EntityState.Modified)
                entry.Entity.MarcarComoAtualizada();
        }
    }
}
