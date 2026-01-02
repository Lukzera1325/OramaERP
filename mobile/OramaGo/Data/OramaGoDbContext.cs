using Microsoft.EntityFrameworkCore;
using OramaGo.Models;

namespace OramaGo.Data;

public class OramaGoDbContext : DbContext
{
    public DbSet<ClienteLocal> Clientes { get; set; }
    public DbSet<ProdutoLocal> Produtos { get; set; }
    public DbSet<VendaLocal> Vendas { get; set; }
    public DbSet<VendaItemLocal> VendaItens { get; set; }

    public OramaGoDbContext(DbContextOptions<OramaGoDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configurações para ClienteLocal
        modelBuilder.Entity<ClienteLocal>(entity =>
        {
            entity.HasIndex(e => e.CpfCnpj).IsUnique();
            entity.HasIndex(e => e.Email);
            entity.HasIndex(e => new { e.EmpresaId, e.Ativo });
            
            entity.Property(e => e.LimiteCredito).HasPrecision(18, 2);
            entity.Property(e => e.SaldoAtual).HasPrecision(18, 2);
        });

        // Configurações para ProdutoLocal
        modelBuilder.Entity<ProdutoLocal>(entity =>
        {
            entity.HasIndex(e => e.Codigo).IsUnique();
            entity.HasIndex(e => e.CodigoBarras);
            entity.HasIndex(e => new { e.EmpresaId, e.AtivoVenda });
            
            entity.Property(e => e.PrecoCusto).HasPrecision(18, 2);
            entity.Property(e => e.PrecoVenda).HasPrecision(18, 2);
            entity.Property(e => e.PrecoMinimo).HasPrecision(18, 2);
            entity.Property(e => e.MargemLucro).HasPrecision(5, 2);
            entity.Property(e => e.EstoqueAtual).HasPrecision(18, 3);
            entity.Property(e => e.EstoqueMinimo).HasPrecision(18, 3);
            entity.Property(e => e.Peso).HasPrecision(18, 3);
        });

        // Configurações para VendaLocal
        modelBuilder.Entity<VendaLocal>(entity =>
        {
            entity.HasIndex(e => e.Numero).IsUnique();
            entity.HasIndex(e => new { e.EmpresaId, e.DataVenda });
            entity.HasIndex(e => new { e.EmpresaId, e.Status });
            entity.HasIndex(e => new { e.EmpresaId, e.ClienteId });
            
            entity.Property(e => e.SubTotal).HasPrecision(18, 2);
            entity.Property(e => e.ValorDesconto).HasPrecision(18, 2);
            entity.Property(e => e.PercentualDesconto).HasPrecision(5, 2);
            entity.Property(e => e.ValorFrete).HasPrecision(18, 2);
            entity.Property(e => e.ValorTotal).HasPrecision(18, 2);

            // Relacionamento com Cliente
            entity.HasOne(e => e.Cliente)
                  .WithMany()
                  .HasForeignKey(e => e.ClienteId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Relacionamento com Itens
            entity.HasMany(e => e.Itens)
                  .WithOne(e => e.Venda)
                  .HasForeignKey(e => e.VendaId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configurações para VendaItemLocal
        modelBuilder.Entity<VendaItemLocal>(entity =>
        {
            entity.HasIndex(e => new { e.VendaId, e.ProdutoId });
            
            entity.Property(e => e.Quantidade).HasPrecision(18, 3);
            entity.Property(e => e.PrecoUnitario).HasPrecision(18, 2);
            entity.Property(e => e.PercentualDesconto).HasPrecision(5, 2);
            entity.Property(e => e.ValorDesconto).HasPrecision(18, 2);
            entity.Property(e => e.ValorTotal).HasPrecision(18, 2);

            // Relacionamento com Produto
            entity.HasOne(e => e.Produto)
                  .WithMany()
                  .HasForeignKey(e => e.ProdutoId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configurações globais para BaseLocalModel
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseLocalModel).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .HasIndex(nameof(BaseLocalModel.EmpresaId), nameof(BaseLocalModel.Ativo));
                
                modelBuilder.Entity(entityType.ClrType)
                    .HasIndex(nameof(BaseLocalModel.StatusSync));
            }
        }
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries<BaseLocalModel>();

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.DataCriacao = DateTime.Now;
                    entry.Entity.DataModificacao = DateTime.Now;
                    break;

                case EntityState.Modified:
                    entry.Entity.DataModificacao = DateTime.Now;
                    // Marcar como modificado se já estava sincronizado
                    if (entry.Entity.StatusSync == SyncStatus.Synced)
                    {
                        entry.Entity.StatusSync = SyncStatus.Modified;
                    }
                    break;
            }
        }
    }
}