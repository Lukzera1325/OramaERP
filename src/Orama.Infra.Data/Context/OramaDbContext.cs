using Microsoft.EntityFrameworkCore;
using Orama.Domain.Entities;

namespace Orama.Infra.Data.Context;

/// <summary>
/// Contexto principal do Entity Framework para o sistema Orama
/// Responsável por mapear as entidades para o banco PostgreSQL
/// </summary>
public class OramaDbContext : DbContext
{
    public OramaDbContext(DbContextOptions<OramaDbContext> options) : base(options)
    {
    }

    // DbSets - Representam as tabelas no banco de dados
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Perfil> Perfis { get; set; }
    public DbSet<Permissao> Permissoes { get; set; }
    public DbSet<PerfilPermissao> PerfilPermissoes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplicar configurações das entidades
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OramaDbContext).Assembly);

        // Configuração de relacionamentos
        ConfigurarRelacionamentos(modelBuilder);

        // Dados iniciais (Seed)
        SeedData(modelBuilder);
    }

    /// <summary>
    /// Configura os relacionamentos entre as entidades
    /// </summary>
    private static void ConfigurarRelacionamentos(ModelBuilder modelBuilder)
    {
        // Usuario -> Perfil (N:1)
        modelBuilder.Entity<Usuario>()
            .HasOne(u => u.Perfil)
            .WithMany(p => p.Usuarios)
            .HasForeignKey(u => u.PerfilId)
            .OnDelete(DeleteBehavior.Restrict);

        // PerfilPermissao -> Perfil (N:1)
        modelBuilder.Entity<PerfilPermissao>()
            .HasOne(pp => pp.Perfil)
            .WithMany(p => p.PerfilPermissoes)
            .HasForeignKey(pp => pp.PerfilId)
            .OnDelete(DeleteBehavior.Cascade);

        // PerfilPermissao -> Permissao (N:1)
        modelBuilder.Entity<PerfilPermissao>()
            .HasOne(pp => pp.Permissao)
            .WithMany(p => p.PerfilPermissoes)
            .HasForeignKey(pp => pp.PermissaoId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índice único para evitar duplicação de permissões por perfil
        modelBuilder.Entity<PerfilPermissao>()
            .HasIndex(pp => new { pp.PerfilId, pp.PermissaoId })
            .IsUnique();
    }

    /// <summary>
    /// Insere dados iniciais no banco de dados
    /// </summary>
    private static void SeedData(ModelBuilder modelBuilder)
    {
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

        // Usuário Administrador padrão
        modelBuilder.Entity<Usuario>().HasData(
            new Usuario
            {
                Id = 1,
                Nome = "Administrador",
                Email = "admin@orama.com.br",
                Senha = BCrypt.Net.BCrypt.HashPassword("Admin@123"), // Senha criptografada
                PerfilId = 1,
                DataCriacao = DateTime.UtcNow,
                Ativo = true
            }
        );

        // Permissões básicas do sistema
        var permissoes = new[]
        {
            // Módulo Segurança
            new Permissao { Id = 1, Nome = "Usuarios.Visualizar", Modulo = "Segurança", Acao = "Visualizar", Descricao = "Visualizar usuários" },
            new Permissao { Id = 2, Nome = "Usuarios.Incluir", Modulo = "Segurança", Acao = "Incluir", Descricao = "Incluir usuários" },
            new Permissao { Id = 3, Nome = "Usuarios.Alterar", Modulo = "Segurança", Acao = "Alterar", Descricao = "Alterar usuários" },
            new Permissao { Id = 4, Nome = "Usuarios.Excluir", Modulo = "Segurança", Acao = "Excluir", Descricao = "Excluir usuários" },
            
            // Módulo Cadastros
            new Permissao { Id = 5, Nome = "Clientes.Visualizar", Modulo = "Cadastros", Acao = "Visualizar", Descricao = "Visualizar clientes" },
            new Permissao { Id = 6, Nome = "Clientes.Incluir", Modulo = "Cadastros", Acao = "Incluir", Descricao = "Incluir clientes" },
            new Permissao { Id = 7, Nome = "Clientes.Alterar", Modulo = "Cadastros", Acao = "Alterar", Descricao = "Alterar clientes" },
            new Permissao { Id = 8, Nome = "Clientes.Excluir", Modulo = "Cadastros", Acao = "Excluir", Descricao = "Excluir clientes" },
            
            // Módulo Financeiro
            new Permissao { Id = 9, Nome = "Financeiro.Visualizar", Modulo = "Financeiro", Acao = "Visualizar", Descricao = "Visualizar financeiro" },
            new Permissao { Id = 10, Nome = "Financeiro.Incluir", Modulo = "Financeiro", Acao = "Incluir", Descricao = "Incluir lançamentos financeiros" },
            new Permissao { Id = 11, Nome = "Financeiro.Alterar", Modulo = "Financeiro", Acao = "Alterar", Descricao = "Alterar lançamentos financeiros" },
            new Permissao { Id = 12, Nome = "Financeiro.Excluir", Modulo = "Financeiro", Acao = "Excluir", Descricao = "Excluir lançamentos financeiros" }
        };

        foreach (var permissao in permissoes)
        {
            permissao.DataCriacao = DateTime.UtcNow;
            permissao.Ativo = true;
        }

        modelBuilder.Entity<Permissao>().HasData(permissoes);

        // Conceder todas as permissões ao perfil Administrador
        var perfilPermissoes = permissoes.Select((p, index) => new PerfilPermissao
        {
            Id = index + 1,
            PerfilId = 1,
            PermissaoId = p.Id,
            Concedida = true,
            DataCriacao = DateTime.UtcNow,
            Ativo = true
        }).ToArray();

        modelBuilder.Entity<PerfilPermissao>().HasData(perfilPermissoes);
    }

    /// <summary>
    /// Override do SaveChanges para atualizar automaticamente as datas
    /// </summary>
    public override int SaveChanges()
    {
        AtualizarTimestamps();
        return base.SaveChanges();
    }

    /// <summary>
    /// Override do SaveChangesAsync para atualizar automaticamente as datas
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        AtualizarTimestamps();
        return await base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Atualiza automaticamente as datas de criação e atualização
    /// </summary>
    private void AtualizarTimestamps()
    {
        var entries = ChangeTracker.Entries<BaseEntity>();

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.DataCriacao = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.MarcarComoAtualizada();
                    break;
            }
        }
    }
}