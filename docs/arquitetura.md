# 🏗️ ARQUITETURA ERP ORAMA

**Padrões e Estrutura do Sistema**

---

## 🎯 **VISÃO ARQUITETURAL**

### **Clean Architecture + DDD Pragmático**
```
┌─────────────────────────────────────────────────────────┐
│                    PRESENTATION                         │
│  Controllers, Views, APIs, ViewModels                  │
├─────────────────────────────────────────────────────────┤
│                    APPLICATION                          │
│     Services, DTOs, Interfaces, Use Cases              │
├─────────────────────────────────────────────────────────┤
│                      DOMAIN                             │
│   Entities, Value Objects, Domain Services             │
├─────────────────────────────────────────────────────────┤
│                  INFRASTRUCTURE                         │
│  Data Access, External Services, Cross-Cutting         │
└─────────────────────────────────────────────────────────┘
```

### **Princípios Fundamentais**
- **Simplicidade** - Código compreensível por estagiários
- **Clareza** - Nomes autoexplicativos e responsabilidades bem definidas
- **Pragmatismo** - DDD sem complexidade desnecessária
- **Manutenibilidade** - Fácil de evoluir e manter

---

## 📁 **ESTRUTURA DE PASTAS**

### **Backend (.NET 8)**
```
src/
├── Orama.Domain/                    # Camada de Domínio
│   ├── Entities/                    # Entidades ricas
│   ├── Enums/                       # Enumerações
│   ├── Services/                    # Domain Services
│   └── ValueObjects/                # Objetos de valor
│
├── Orama.Application/               # Camada de Aplicação
│   ├── Services/                    # Application Services
│   ├── Interfaces/                  # Contratos de serviços
│   └── DTOs/                        # Data Transfer Objects
│
├── Orama.Infra.Data/               # Infraestrutura de Dados
│   ├── Context/                     # DbContext
│   ├── Configurations/              # Configurações EF
│   ├── Migrations/                  # Migrações
│   └── Repositories/                # Repositórios (se necessário)
│
├── Orama.Infra.CrossCutting/       # Infraestrutura Transversal
│   ├── Security/                    # Segurança e autenticação
│   ├── Extensions/                  # Métodos de extensão
│   └── Helpers/                     # Utilitários
│
└── Orama.Web/                      # Camada de Apresentação
    ├── Controllers/                 # Controllers MVC e API
    ├── Views/                       # Views Razor
    ├── Models/                      # ViewModels
    ├── wwwroot/                     # Arquivos estáticos
    └── Areas/                       # Áreas (se necessário)
```

### **Mobile (.NET MAUI)**
```
mobile/OramaGo/
├── Models/                          # Modelos locais
├── ViewModels/                      # MVVM ViewModels
├── Views/                           # Telas XAML
├── Services/                        # Serviços mobile
├── Data/                            # SQLite local
├── Converters/                      # Conversores XAML
├── Validators/                      # Validações
└── Platforms/                       # Código específico por plataforma
```

---

## 🧮 **PADRÕES DE CÓDIGO**

### **1. Entidades Ricas (Domain)**
```csharp
public class Venda
{
    // Propriedades com validação
    public string Numero { get; private set; }
    public StatusVenda Status { get; private set; }
    
    // Construtor com validação
    public Venda(int clienteId, List<VendaItem> itens)
    {
        if (clienteId <= 0) throw new ArgumentException("Cliente inválido");
        if (!itens.Any()) throw new ArgumentException("Venda deve ter itens");
        
        ClienteId = clienteId;
        Itens = itens;
        Status = StatusVenda.Orcamento;
        Numero = GerarNumero();
        DataCriacao = DateTime.Now;
    }
    
    // Métodos de negócio
    public void Faturar()
    {
        if (Status != StatusVenda.Orcamento)
            throw new InvalidOperationException("Apenas orçamentos podem ser faturados");
            
        Status = StatusVenda.Faturada;
        DataFaturamento = DateTime.Now;
        CalcularMargemELucro();
    }
    
    // Regras de negócio encapsuladas
    private void CalcularMargemELucro()
    {
        CustoTotal = Itens.Sum(i => i.CustoTotal);
        LucroTotal = ValorTotal - CustoTotal;
        MargemPercentual = ValorTotal > 0 ? (LucroTotal / ValorTotal) * 100 : 0;
        DataMargemCalculada = DateTime.Now;
    }
}
```

### **2. Application Services (Casos de Uso)**
```csharp
public class VendaService : IVendaService
{
    private readonly OramaDbContext _context;
    private readonly IEstoqueService _estoqueService;
    
    // Um método = Um caso de uso
    public async Task<bool> FaturarAsync(int vendaId)
    {
        // 1. Buscar e validar
        var venda = await _context.Vendas
            .Include(v => v.Itens)
            .FirstOrDefaultAsync(v => v.Id == vendaId);
            
        if (venda == null) return false;
        
        // 2. Executar regra de negócio (Domain)
        venda.Faturar();
        
        // 3. Coordenar efeitos colaterais
        await ProcessarMovimentacoesEstoque(venda);
        await GerarContasReceber(venda);
        
        // 4. Persistir
        return await _context.SaveChangesAsync() > 0;
    }
    
    // Métodos auxiliares privados
    private async Task ProcessarMovimentacoesEstoque(Venda venda)
    {
        foreach (var item in venda.Itens)
        {
            await _estoqueService.SaidaAsync(item.ProdutoId, item.Quantidade, 
                $"Venda {venda.Numero}");
        }
    }
}
```

### **3. Controllers Minimalistas**
```csharp
public class VendasController : BaseController
{
    private readonly IVendaService _vendaService;
    
    // Padrão: Receber → Validar → Chamar Service → Retornar
    public async Task<IActionResult> Faturar(int id)
    {
        var sucesso = await _vendaService.FaturarAsync(id);
        
        if (sucesso)
        {
            TempData["Sucesso"] = "Venda faturada com sucesso!";
        }
        else
        {
            TempData["Erro"] = "Erro ao faturar venda.";
        }
        
        return RedirectToAction(nameof(Detalhes), new { id });
    }
}
```

---

## 🔄 **FLUXO DE DADOS**

### **Fluxo de Request (Web)**
```
1. User Action (Click/Submit)
   ↓
2. Controller Action
   ├── Validar ModelState
   ├── Chamar Application Service
   └── Retornar View/Redirect
   ↓
3. Application Service
   ├── Buscar dados (DbContext)
   ├── Executar regras (Domain)
   ├── Coordenar efeitos colaterais
   └── Persistir mudanças
   ↓
4. Domain Entity
   ├── Validar invariantes
   ├── Executar regras de negócio
   └── Manter consistência
```

### **Fluxo de Sincronização (Mobile)**
```
1. Mobile App (Offline)
   ├── Salvar dados localmente (SQLite)
   ├── Marcar para sincronização
   └── Continuar funcionando
   ↓
2. Quando Online
   ├── Detectar conectividade
   ├── Enviar dados pendentes
   ├── Baixar atualizações
   └── Resolver conflitos
   ↓
3. Backend APIs
   ├── Receber dados mobile
   ├── Validar e processar
   ├── Retornar confirmação
   └── Enviar atualizações
```

---

## 🗄️ **ESTRATÉGIA DE DADOS**

### **Entity Framework Core**
```csharp
public class OramaDbContext : DbContext
{
    // DbSets organizados por módulo
    
    // Core
    public DbSet<Empresa> Empresas { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }
    
    // Cadastros
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<Fornecedor> Fornecedores { get; set; }
    public DbSet<Produto> Produtos { get; set; }
    
    // Operacional
    public DbSet<Venda> Vendas { get; set; }
    public DbSet<Compra> Compras { get; set; }
    public DbSet<OrdemProducao> OrdensProducao { get; set; }
    
    // Configurações via Fluent API
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OramaDbContext).Assembly);
    }
}
```

### **Configurações de Entidades**
```csharp
public class VendaConfiguration : IEntityTypeConfiguration<Venda>
{
    public void Configure(EntityTypeBuilder<Venda> builder)
    {
        builder.ToTable("Vendas");
        
        builder.HasKey(v => v.Id);
        
        builder.Property(v => v.Numero)
            .IsRequired()
            .HasMaxLength(20);
            
        builder.Property(v => v.ValorTotal)
            .HasColumnType("decimal(18,2)");
            
        // Relacionamentos
        builder.HasMany(v => v.Itens)
            .WithOne(i => i.Venda)
            .HasForeignKey(i => i.VendaId)
            .OnDelete(DeleteBehavior.Cascade);
            
        // Índices para performance
        builder.HasIndex(v => v.Numero).IsUnique();
        builder.HasIndex(v => v.ClienteId);
        builder.HasIndex(v => v.DataCriacao);
    }
}
```

---

## 🔐 **SEGURANÇA E AUTENTICAÇÃO**

### **ASP.NET Identity + JWT**
```csharp
// Configuração no Program.cs
builder.Services.AddIdentity<Usuario, IdentityRole<int>>()
    .AddEntityFrameworkStores<OramaDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});
```

### **Multi-tenant por Empresa**
```csharp
public class BaseController : Controller
{
    protected int EmpresaId => User.GetEmpresaId();
    protected int UsuarioId => User.GetUsuarioId();
}

public static class ClaimsPrincipalExtensions
{
    public static int GetEmpresaId(this ClaimsPrincipal user)
    {
        var claim = user.FindFirst("EmpresaId");
        return claim != null ? int.Parse(claim.Value) : 0;
    }
}
```

---

## 📊 **PERFORMANCE E OTIMIZAÇÃO**

### **Consultas Otimizadas**
```csharp
// Include explícito para evitar N+1
public async Task<VendaDto> ObterComDetalhesAsync(int id)
{
    return await _context.Vendas
        .Include(v => v.Cliente)
        .Include(v => v.Itens)
            .ThenInclude(i => i.Produto)
        .Where(v => v.Id == id && v.EmpresaId == EmpresaId)
        .Select(v => new VendaDto
        {
            Id = v.Id,
            Numero = v.Numero,
            ClienteNome = v.Cliente.Nome,
            ValorTotal = v.ValorTotal,
            Itens = v.Itens.Select(i => new VendaItemDto
            {
                ProdutoDescricao = i.Produto.Descricao,
                Quantidade = i.Quantidade,
                PrecoUnitario = i.PrecoUnitario
            }).ToList()
        })
        .FirstOrDefaultAsync();
}
```

### **Paginação Eficiente**
```csharp
public async Task<PagedResult<VendaDto>> ObterPaginadoAsync(int page, int size)
{
    var query = _context.Vendas
        .Where(v => v.EmpresaId == EmpresaId)
        .OrderByDescending(v => v.DataCriacao);
        
    var total = await query.CountAsync();
    
    var itens = await query
        .Skip((page - 1) * size)
        .Take(size)
        .Select(v => new VendaDto { ... })
        .ToListAsync();
        
    return new PagedResult<VendaDto>
    {
        Items = itens,
        TotalCount = total,
        Page = page,
        PageSize = size
    };
}
```

---

## 🧪 **TESTES E QUALIDADE**

### **Estrutura de Testes (Sugerida)**
```
tests/
├── Orama.Domain.Tests/              # Testes de unidade (Domain)
├── Orama.Application.Tests/         # Testes de integração (Services)
├── Orama.Web.Tests/                 # Testes de controllers
└── Orama.IntegrationTests/          # Testes end-to-end
```

### **Exemplo de Teste de Domínio**
```csharp
[Test]
public void Venda_Faturar_DeveCalcularMargemCorretamente()
{
    // Arrange
    var itens = new List<VendaItem>
    {
        new VendaItem(1, 10, 100, 80), // Produto 1: 10 un × R$ 100 (custo R$ 80)
        new VendaItem(2, 5, 200, 150)  // Produto 2: 5 un × R$ 200 (custo R$ 150)
    };
    var venda = new Venda(clienteId: 1, itens);
    
    // Act
    venda.Faturar();
    
    // Assert
    Assert.AreEqual(2750, venda.ValorTotal);    // (10×100) + (5×200)
    Assert.AreEqual(1550, venda.CustoTotal);    // (10×80) + (5×150)
    Assert.AreEqual(1200, venda.LucroTotal);    // 2750 - 1550
    Assert.AreEqual(43.64m, venda.MargemPercentual, 0.01m); // (1200/2750)×100
}
```

---

## 🚀 **DEPLOYMENT E DEVOPS**

### **Docker Compose**
```yaml
version: '3.8'
services:
  orama-web:
    build: .
    ports:
      - "5000:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Server=db;Database=OramaERP;User=sa;Password=YourPassword123;
    depends_on:
      - db
      
  db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourPassword123
    volumes:
      - sqldata:/var/opt/mssql
      
volumes:
  sqldata:
```

### **CI/CD Pipeline (Sugerido)**
```yaml
# .github/workflows/ci-cd.yml
name: CI/CD Pipeline

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v3
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 8.0.x
    - name: Restore dependencies
      run: dotnet restore
    - name: Build
      run: dotnet build --no-restore
    - name: Test
      run: dotnet test --no-build --verbosity normal
      
  deploy:
    needs: test
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/main'
    steps:
    - name: Deploy to production
      run: echo "Deploy to production server"
```

---

## 📋 **CONVENÇÕES E PADRÕES**

### **Nomenclatura**
- **Classes**: PascalCase (ex: `VendaService`)
- **Métodos**: PascalCase (ex: `CriarAsync`)
- **Propriedades**: PascalCase (ex: `ValorTotal`)
- **Variáveis**: camelCase (ex: `vendaId`)
- **Constantes**: UPPER_CASE (ex: `MAX_ITEMS`)

### **Organização de Arquivos**
- **Um arquivo por classe**
- **Namespace igual ao caminho da pasta**
- **Interfaces começam com 'I'**
- **Services terminam com 'Service'**
- **Controllers terminam com 'Controller'**

### **Comentários e Documentação**
```csharp
/// <summary>
/// Fatura uma venda, calculando margem e processando estoque
/// </summary>
/// <param name="vendaId">ID da venda a ser faturada</param>
/// <returns>True se faturada com sucesso</returns>
public async Task<bool> FaturarAsync(int vendaId)
{
    // Comentários apenas quando necessário
    // Código deve ser autoexplicativo
}
```

---

## 🎯 **PRINCÍPIOS ARQUITETURAIS**

### **SOLID Aplicado**
- **S** - Single Responsibility: Uma classe, uma responsabilidade
- **O** - Open/Closed: Aberto para extensão, fechado para modificação
- **L** - Liskov Substitution: Subtipos devem ser substituíveis
- **I** - Interface Segregation: Interfaces específicas e coesas
- **D** - Dependency Inversion: Dependa de abstrações, não de implementações

### **DDD Pragmático**
- **Entidades ricas** com comportamento
- **Services focados** em casos de uso
- **Agregados simples** e coesos
- **Linguagem ubíqua** no código

### **Clean Code**
- **Nomes expressivos** e autoexplicativos
- **Funções pequenas** e focadas
- **Comentários apenas** quando necessário
- **Testes como** documentação viva

---

**Arquitetura sólida, simples e evolutiva!** 🏗️✨