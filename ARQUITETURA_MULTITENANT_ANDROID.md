# Arquitetura Multi-Tenant para Órama Go Android

## Visão Geral

Este documento descreve a arquitetura proposta para implementar multi-tenancy no aplicativo Android Órama Go, permitindo que cada empresa tenha seu próprio domínio/subdomínio e isolamento completo de dados.

## Conceito Multi-Tenant

### Modelo Proposto: Domain-Based Multi-Tenancy

Cada empresa terá seu próprio domínio ou subdomínio:
- `empresa1.orama.com.br`
- `empresa2.orama.com.br`
- `minhaempresa.com.br` (domínio próprio)

### Benefícios
- **Isolamento completo** de dados entre empresas
- **Branding personalizado** por empresa
- **Escalabilidade** horizontal
- **Segurança** aprimorada
- **Facilidade de backup/restore** por empresa

## Arquitetura Técnica

### 1. Backend API (Já Implementado Parcialmente)

#### Estrutura Atual
```csharp
// Entidade Empresa já existe
public class Empresa : BaseEntity
{
    public string RazaoSocial { get; set; }
    public string Cnpj { get; set; }
    public string? Dominio { get; set; } // NOVO CAMPO
    // ... outros campos
}

// BaseController já implementa isolamento
public abstract class BaseController : Controller
{
    protected int ObterEmpresaId() => HttpContext.Session.GetInt32("EmpresaId") ?? 0;
}
```

#### Melhorias Necessárias

**1. Adicionar campo Dominio na entidade Empresa:**
```csharp
[StringLength(100)]
public string? Dominio { get; set; } // empresa1.orama.com.br

[StringLength(100)]
public string? DominioPersonalizado { get; set; } // minhaempresa.com.br
```

**2. Middleware de Tenant Resolution:**
```csharp
public class TenantResolutionMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var host = context.Request.Host.Host;
        var empresa = await ResolverEmpresaPorDominio(host);
        
        if (empresa != null)
        {
            context.Items["TenantId"] = empresa.Id;
            context.Items["TenantInfo"] = empresa;
        }
        
        await next(context);
    }
}
```

**3. API Endpoints Específicos:**
```csharp
[ApiController]
[Route("api/tenant")]
public class TenantApiController : ControllerBase
{
    [HttpGet("info")]
    public async Task<IActionResult> GetTenantInfo()
    {
        var host = Request.Host.Host;
        var empresa = await _empresaService.ObterPorDominioAsync(host);
        
        return Ok(new TenantInfoResponse
        {
            EmpresaId = empresa.Id,
            Nome = empresa.NomeExibicao,
            Logo = empresa.LogoUrl,
            CorPrimaria = empresa.CorTema,
            Configuracoes = empresa.ConfiguracoesApp
        });
    }
}
```

### 2. Android App - Arquitetura Multi-Tenant

#### Estrutura de Configuração

**1. Tenant Configuration Service:**
```csharp
public interface ITenantConfigService
{
    Task<TenantConfig> GetTenantConfigAsync(string domain);
    Task<bool> ValidateTenantAsync(string domain);
    Task SaveTenantConfigAsync(TenantConfig config);
    Task<List<TenantConfig>> GetSavedTenantsAsync();
}

public class TenantConfig
{
    public int EmpresaId { get; set; }
    public string Nome { get; set; }
    public string Dominio { get; set; }
    public string ApiBaseUrl { get; set; }
    public string LogoUrl { get; set; }
    public string CorPrimaria { get; set; }
    public Dictionary<string, object> Configuracoes { get; set; }
}
```

**2. Tenant Selection Screen:**
```csharp
public partial class TenantSelectionPage : ContentPage
{
    public TenantSelectionPage()
    {
        InitializeComponent();
        BindingContext = new TenantSelectionViewModel();
    }
}

public class TenantSelectionViewModel : BaseViewModel
{
    public ObservableCollection<TenantConfig> SavedTenants { get; set; }
    public Command<TenantConfig> SelectTenantCommand { get; set; }
    public Command AddNewTenantCommand { get; set; }
    
    private async Task SelectTenant(TenantConfig tenant)
    {
        // Configurar app para o tenant selecionado
        await _tenantConfigService.SetCurrentTenantAsync(tenant);
        await Shell.Current.GoToAsync("//login");
    }
}
```

**3. Dynamic BaseService Configuration:**
```csharp
public abstract class BaseService<T> : IBaseService<T> where T : BaseLocalModel
{
    protected readonly HttpClient _httpClient;
    protected readonly ITenantConfigService _tenantConfig;
    
    protected async Task<string> GetApiBaseUrlAsync()
    {
        var currentTenant = await _tenantConfig.GetCurrentTenantAsync();
        return currentTenant?.ApiBaseUrl ?? "https://api.orama.com.br";
    }
    
    protected async Task<HttpClient> GetConfiguredHttpClientAsync()
    {
        var baseUrl = await GetApiBaseUrlAsync();
        _httpClient.BaseAddress = new Uri(baseUrl);
        return _httpClient;
    }
}
```

#### Fluxo de Autenticação Multi-Tenant

**1. Tenant Discovery:**
```csharp
public class TenantDiscoveryService
{
    public async Task<TenantConfig> DiscoverTenantAsync(string domain)
    {
        try
        {
            var client = new HttpClient();
            var response = await client.GetAsync($"https://{domain}/api/tenant/info");
            
            if (response.IsSuccessStatusCode)
            {
                var tenantInfo = await response.Content.ReadFromJsonAsync<TenantInfoResponse>();
                return new TenantConfig
                {
                    EmpresaId = tenantInfo.EmpresaId,
                    Nome = tenantInfo.Nome,
                    Dominio = domain,
                    ApiBaseUrl = $"https://{domain}/api",
                    LogoUrl = tenantInfo.Logo,
                    CorPrimaria = tenantInfo.CorPrimaria
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erro ao descobrir tenant: {domain}");
        }
        
        return null;
    }
}
```

**2. Enhanced AuthService:**
```csharp
public class AuthService : IAuthService
{
    private readonly ITenantConfigService _tenantConfig;
    
    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var currentTenant = await _tenantConfig.GetCurrentTenantAsync();
        if (currentTenant == null)
            throw new InvalidOperationException("Nenhum tenant selecionado");
        
        var client = await GetConfiguredHttpClientAsync();
        var response = await client.PostAsJsonAsync("/auth/login", request);
        
        // ... resto da implementação
    }
}
```

### 3. Database Strategy

#### Opção 1: Database per Tenant (Recomendada)
- Cada empresa tem seu próprio banco SQLite
- Isolamento completo
- Facilita backup/restore
- Melhor performance

```csharp
public class TenantDbContextFactory
{
    public OramaGoDbContext CreateContext(int tenantId)
    {
        var dbPath = Path.Combine(
            FileSystem.AppDataDirectory, 
            $"orama_tenant_{tenantId}.db"
        );
        
        var options = new DbContextOptionsBuilder<OramaGoDbContext>()
            .UseSqlite($"Data Source={dbPath}")
            .Options;
            
        return new OramaGoDbContext(options);
    }
}
```

#### Opção 2: Shared Database with Tenant ID
- Um banco com campo TenantId em todas as tabelas
- Mais complexo de gerenciar
- Menor uso de espaço

### 4. UI/UX Personalização

#### Theme Service:
```csharp
public class ThemeService
{
    public async Task ApplyTenantThemeAsync(TenantConfig tenant)
    {
        if (Application.Current?.Resources != null)
        {
            Application.Current.Resources["PrimaryColor"] = Color.FromArgb(tenant.CorPrimaria);
            Application.Current.Resources["TenantLogo"] = tenant.LogoUrl;
            Application.Current.Resources["TenantName"] = tenant.Nome;
        }
    }
}
```

## Implementação Passo a Passo

### Fase 1: Backend Preparation
1. ✅ Adicionar campo `Dominio` na entidade `Empresa`
2. ✅ Criar migration para novo campo
3. ✅ Implementar `TenantResolutionMiddleware`
4. ✅ Criar `TenantApiController`
5. ✅ Atualizar `AuthApiController` para suportar multi-tenancy

### Fase 2: Android Core Services
1. ✅ Implementar `ITenantConfigService`
2. ✅ Criar `TenantDiscoveryService`
3. ✅ Atualizar `BaseService` para multi-tenancy
4. ✅ Implementar `TenantDbContextFactory`

### Fase 3: UI Implementation
1. ✅ Criar `TenantSelectionPage`
2. ✅ Implementar `TenantSelectionViewModel`
3. ✅ Atualizar fluxo de login
4. ✅ Implementar `ThemeService`

### Fase 4: Testing & Polish
1. ✅ Testes de isolamento de dados
2. ✅ Testes de performance
3. ✅ Validação de segurança
4. ✅ Documentação final

## Considerações de Segurança

### 1. Isolamento de Dados
- Validação rigorosa de TenantId em todas as operações
- Middleware de validação de acesso
- Logs de auditoria por tenant

### 2. Autenticação
- Tokens JWT com claim de TenantId
- Validação de domínio no backend
- Rate limiting por tenant

### 3. Backup e Recovery
- Backup automático por tenant
- Estratégia de disaster recovery
- Migração de dados entre ambientes

## Exemplo de Uso

### Cenário: Empresa ABC quer usar o Órama Go

1. **Configuração no Backend:**
   - Admin cria empresa "ABC Ltda" no sistema
   - Define domínio: `abc.orama.com.br`
   - Configura logo e cores da empresa

2. **Primeiro Acesso no App:**
   - Usuário abre o app
   - Digita: `abc.orama.com.br`
   - App descobre configurações do tenant
   - Salva configuração localmente
   - Redireciona para login

3. **Login:**
   - Usuário faz login com credenciais da empresa ABC
   - App autentica contra `https://abc.orama.com.br/api`
   - Recebe token JWT com TenantId da empresa ABC
   - Aplica tema personalizado da empresa

4. **Uso Normal:**
   - Todas as operações são isoladas para empresa ABC
   - Dados sincronizam apenas com tenant ABC
   - UI mostra logo e cores da empresa ABC

## Próximos Passos

1. **Implementar campos de domínio na entidade Empresa**
2. **Criar middleware de tenant resolution**
3. **Desenvolver tela de seleção de tenant no Android**
4. **Implementar descoberta automática de tenant**
5. **Criar sistema de temas dinâmicos**
6. **Testes de isolamento e segurança**

Esta arquitetura permitirá que o Órama Go seja verdadeiramente multi-tenant, oferecendo isolamento completo de dados e personalização por empresa, mantendo a simplicidade de uso para o usuário final.