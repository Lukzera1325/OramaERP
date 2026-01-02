using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OramaGo.Data;
using OramaGo.Models;

namespace OramaGo.Services;

public class DatabaseService : IDatabaseService
{
    private readonly OramaGoDbContext _context;
    private readonly ILogger<DatabaseService> _logger;

    public DatabaseService(OramaGoDbContext context, ILogger<DatabaseService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task InitializeDatabaseAsync()
    {
        try
        {
            _logger.LogInformation("Inicializando banco de dados SQLite...");

            // Garantir que o banco de dados seja criado
            await _context.Database.EnsureCreatedAsync();

            // Aplicar migrations pendentes
            var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();
            if (pendingMigrations.Any())
            {
                _logger.LogInformation($"Aplicando {pendingMigrations.Count()} migrations pendentes...");
                await _context.Database.MigrateAsync();
            }

            _logger.LogInformation("Banco de dados inicializado com sucesso");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao inicializar banco de dados");
            throw;
        }
    }

    public async Task<bool> DatabaseExistsAsync()
    {
        try
        {
            return await _context.Database.CanConnectAsync();
        }
        catch
        {
            return false;
        }
    }

    public async Task DeleteDatabaseAsync()
    {
        try
        {
            _logger.LogWarning("Deletando banco de dados...");
            await _context.Database.EnsureDeletedAsync();
            _logger.LogInformation("Banco de dados deletado com sucesso");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao deletar banco de dados");
            throw;
        }
    }

    public async Task SeedDataAsync()
    {
        try
        {
            _logger.LogInformation("Verificando se é necessário popular dados iniciais...");

            // Verificar se já existem dados
            var hasClientes = await _context.Clientes.AnyAsync();
            var hasProdutos = await _context.Produtos.AnyAsync();

            if (!hasClientes && !hasProdutos)
            {
                _logger.LogInformation("Populando dados de exemplo...");
                await SeedExampleDataAsync();
            }
            else
            {
                _logger.LogInformation("Dados já existem, pulando seed");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao popular dados iniciais");
            throw;
        }
    }

    public Task<string> GetDatabasePathAsync()
    {
        var connection = _context.Database.GetConnectionString();
        if (connection?.Contains("Data Source=") == true)
        {
            return Task.FromResult(connection.Split("Data Source=")[1].Split(';')[0]);
        }
        return Task.FromResult("Caminho não encontrado");
    }

    public async Task<long> GetDatabaseSizeAsync()
    {
        try
        {
            var path = await GetDatabasePathAsync();
            if (File.Exists(path))
            {
                var fileInfo = new FileInfo(path);
                return fileInfo.Length;
            }
            return 0;
        }
        catch
        {
            return 0;
        }
    }

    private async Task SeedExampleDataAsync()
    {
        // Dados de exemplo para desenvolvimento/teste
        var clientesExemplo = new List<ClienteLocal>
        {
            new()
            {
                EmpresaId = 1,
                Nome = "João Silva",
                CpfCnpj = "12345678901",
                Email = "joao@email.com",
                Telefone = "(11) 99999-9999",
                Endereco = "Rua das Flores, 123",
                Bairro = "Centro",
                Cidade = "São Paulo",
                Estado = "SP",
                Cep = "01234-567",
                LimiteCredito = 5000,
                SaldoAtual = 0,
                VendedorId = 1,
                StatusSync = Models.SyncStatus.Synced
            },
            new()
            {
                EmpresaId = 1,
                Nome = "Maria Santos",
                CpfCnpj = "98765432100",
                Email = "maria@email.com",
                Celular = "(11) 88888-8888",
                Endereco = "Av. Principal, 456",
                Bairro = "Jardim",
                Cidade = "São Paulo",
                Estado = "SP",
                Cep = "04567-890",
                LimiteCredito = 10000,
                SaldoAtual = 1500,
                VendedorId = 1,
                StatusSync = Models.SyncStatus.Synced
            }
        };

        var produtosExemplo = new List<ProdutoLocal>
        {
            new()
            {
                EmpresaId = 1,
                Codigo = "PROD001",
                Descricao = "Notebook Dell Inspiron",
                DescricaoDetalhada = "Notebook Dell Inspiron 15 3000, Intel Core i5, 8GB RAM, 256GB SSD",
                Categoria = "Informática",
                Marca = "Dell",
                Unidade = "UN",
                PrecoCusto = 2000,
                PrecoVenda = 2800,
                PrecoMinimo = 2400,
                MargemLucro = 40,
                EstoqueAtual = 10,
                EstoqueMinimo = 2,
                Peso = 2.1m,
                Dimensoes = "35.8 x 24.2 x 1.99 cm",
                ControlaEstoque = true,
                AtivoVenda = true,
                StatusSync = Models.SyncStatus.Synced
            },
            new()
            {
                EmpresaId = 1,
                Codigo = "PROD002",
                Descricao = "Mouse Wireless Logitech",
                DescricaoDetalhada = "Mouse sem fio Logitech M280, sensor óptico, 3 botões",
                Categoria = "Periféricos",
                Marca = "Logitech",
                Unidade = "UN",
                PrecoCusto = 45,
                PrecoVenda = 89,
                PrecoMinimo = 65,
                MargemLucro = 97,
                EstoqueAtual = 50,
                EstoqueMinimo = 10,
                Peso = 0.1m,
                Dimensoes = "10.5 x 6.7 x 3.8 cm",
                ControlaEstoque = true,
                AtivoVenda = true,
                StatusSync = Models.SyncStatus.Synced
            },
            new()
            {
                EmpresaId = 1,
                Codigo = "PROD003",
                Descricao = "Teclado Mecânico Gamer",
                DescricaoDetalhada = "Teclado mecânico RGB, switches blue, layout ABNT2",
                Categoria = "Periféricos",
                Marca = "Redragon",
                Unidade = "UN",
                PrecoCusto = 180,
                PrecoVenda = 299,
                PrecoMinimo = 220,
                MargemLucro = 66,
                EstoqueAtual = 25,
                EstoqueMinimo = 5,
                Peso = 1.2m,
                Dimensoes = "44 x 13.5 x 3.5 cm",
                ControlaEstoque = true,
                AtivoVenda = true,
                StatusSync = Models.SyncStatus.Synced
            }
        };

        // Adicionar dados ao contexto
        await _context.Clientes.AddRangeAsync(clientesExemplo);
        await _context.Produtos.AddRangeAsync(produtosExemplo);

        // Salvar alterações
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Dados de exemplo criados: {clientesExemplo.Count} clientes, {produtosExemplo.Count} produtos");
    }
}