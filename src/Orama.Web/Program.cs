using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Orama.Application.Services;
using Orama.Domain.Entities;
using Orama.Infra.Data.Context;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException("ConnectionStrings:DefaultConnection deve ser configurada.");

var databaseProvider = builder.Configuration["Database:Provider"] ?? "Sqlite";
builder.Services.AddDbContext<OramaDbContext>(options =>
{
    if (string.Equals(databaseProvider, "PostgreSql", StringComparison.OrdinalIgnoreCase))
        options.UseNpgsql(connectionString);
    else if (string.Equals(databaseProvider, "Sqlite", StringComparison.OrdinalIgnoreCase))
        options.UseSqlite(connectionString);
    else
        throw new InvalidOperationException($"Database:Provider não suportado: {databaseProvider}");
});

// Configuração do MVC
builder.Services.AddControllersWithViews();
builder.Services.AddHealthChecks();

// Configuração da API com Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Órama ERP API", 
        Version = "v1",
        Description = "API para integração com o app mobile Órama Go"
    });
    
    // Configuração de autenticação JWT no Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando o esquema Bearer. Exemplo: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// Configuração de autenticação JWT
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];
if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Length < 32)
    throw new InvalidOperationException("Jwt:Key deve ser configurada externamente e possuir pelo menos 32 caracteres.");
if (string.IsNullOrWhiteSpace(jwtIssuer) || string.IsNullOrWhiteSpace(jwtAudience))
    throw new InvalidOperationException("Jwt:Issuer e Jwt:Audience devem ser configurados.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// Configuração de sessão para autenticação web
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("OramaGoMobile", policy =>
    {
        if (allowedOrigins.Length > 0)
            policy.WithOrigins(allowedOrigins).AllowAnyMethod().AllowAnyHeader();
    });
});

// Registro dos serviços de aplicação
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IPerfilService, PerfilService>();
builder.Services.AddScoped<IPermissaoService, PermissaoService>();
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<IProdutoService, ProdutoService>();
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<IFornecedorService, FornecedorService>();
builder.Services.AddScoped<IContaBancariaService, ContaBancariaService>();
builder.Services.AddScoped<IContaReceberService, ContaReceberService>();
builder.Services.AddScoped<IContaPagarService, ContaPagarService>();
builder.Services.AddScoped<IVendaService, VendaService>();
builder.Services.AddScoped<ICompraService, CompraService>();
builder.Services.AddScoped<IEstoqueService, EstoqueService>();

// Domain Services - Lógica de negócio complexa
builder.Services.AddScoped<Orama.Domain.Services.VendaProcessingService>();

// Serviços de Produção Industrial SIMPLIFICADOS
builder.Services.AddScoped<IEstruturaProdutoService, EstruturaProdutoService>();
builder.Services.AddScoped<IOrdemProducaoService, OrdemProducaoService>();

// Serviços de Relatórios de Lucratividade
builder.Services.AddScoped<IRelatorioLucratividadeService, RelatorioLucratividadeService>();

// Serviços de Alertas de Margem
builder.Services.AddScoped<IAlertaMargemService, AlertaMargemService>();

// Serviços de Explicação de Resultados
builder.Services.AddScoped<IExplicacaoResultadoService, ExplicacaoResultadoService>();

// Serviços de Apoio à Decisão
builder.Services.AddScoped<ISugestaoAcaoService, SugestaoAcaoService>();
builder.Services.AddScoped<ISimulacaoService, SimulacaoService>();
builder.Services.AddScoped<IDecisaoGerencialService, DecisaoGerencialService>();
builder.Services.AddScoped<IChecklistFechamentoService, ChecklistFechamentoService>();

// Serviços Fiscais (ISOLADOS DO CORE)
builder.Services.AddScoped<Orama.Application.Services.Fiscal.IEmpresaFiscalService, Orama.Application.Services.Fiscal.EmpresaFiscalService>();
builder.Services.AddScoped<Orama.Application.Services.Fiscal.IContextoFiscalService, Orama.Application.Services.Fiscal.ContextoFiscalService>();
builder.Services.AddScoped<Orama.Domain.Interfaces.ITaxCalculator, Orama.Application.Services.Fiscal.BasicTaxCalculator>();

// Serviços NF-e (ISOLADOS DO CORE)
builder.Services.AddScoped<Orama.Application.Services.Fiscal.NFe.INFeEmissaoService, Orama.Application.Services.Fiscal.NFe.NFeEmissaoService>();
builder.Services.AddScoped<Orama.Application.Services.Fiscal.NFe.INFeConsultaService, Orama.Application.Services.Fiscal.NFe.NFeConsultaService>();
builder.Services.AddScoped<Orama.Application.Services.Fiscal.NFe.INFeCancelamentoService, Orama.Application.Services.Fiscal.NFe.NFeCancelamentoService>();
builder.Services.AddScoped<Orama.Application.Services.Fiscal.NFe.ISefazNFeGateway, Orama.Application.Services.Fiscal.NFe.SefazNFeGatewayMock>();
builder.Services.AddScoped<Orama.Application.Services.Fiscal.NFe.Mapping.IVendaParaNFeMapper, Orama.Application.Services.Fiscal.NFe.Mapping.VendaParaNFeMapper>();

// Serviços Fiscais
// builder.Services.AddScoped<INotaFiscalService, NotaFiscalService>(); // Removido - não utilizado

// Registro do HttpClient para integrações externas
builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
    // Habilitar Swagger apenas em desenvolvimento
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Órama ERP API v1");
        c.RoutePrefix = "api/docs"; // Swagger UI em /api/docs
    });
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Configurar CORS antes da autenticação
app.UseCors("OramaGoMobile");

// Configurar autenticação e autorização
app.UseAuthentication();
app.UseAuthorization();

// Configurar sessão para web
app.UseSession();

// Rota padrão do MVC
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Mapear controllers da API
app.MapControllers();
app.MapHealthChecks("/health");

// Aplicar migrations automaticamente em desenvolvimento
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<OramaDbContext>();
    
    try
    {
        await context.Database.EnsureCreatedAsync();
        var seedEmail = builder.Configuration["Seed:AdminEmail"];
        var seedPassword = builder.Configuration["Seed:AdminPassword"];
        if (!string.IsNullOrWhiteSpace(seedEmail) && !string.IsNullOrWhiteSpace(seedPassword))
        {
            if (seedPassword.Length < 12)
                throw new InvalidOperationException("Seed:AdminPassword deve possuir pelo menos 12 caracteres.");

            if (!await context.Usuarios.AnyAsync())
            {
                var profile = await context.Perfis.OrderBy(p => p.Id).FirstOrDefaultAsync()
                    ?? throw new InvalidOperationException("Crie um perfil antes de provisionar o usuário inicial.");
                var company = await context.Empresas.OrderBy(e => e.Id).FirstOrDefaultAsync()
                    ?? throw new InvalidOperationException("Crie uma empresa antes de provisionar o usuário inicial.");
                var admin = new Usuario
                {
                    Nome = builder.Configuration["Seed:AdminName"] ?? "Administrador local",
                    Email = seedEmail,
                    Senha = BCrypt.Net.BCrypt.HashPassword(seedPassword),
                    PerfilId = profile.Id,
                    EmpresaId = company.Id,
                    IsSuperAdmin = false,
                    DataCriacao = DateTime.UtcNow,
                    Ativo = true
                };
                context.Usuarios.Add(admin);
                await context.SaveChangesAsync();
                context.UsuarioEmpresas.Add(new UsuarioEmpresa
                {
                    UsuarioId = admin.Id,
                    EmpresaId = company.Id,
                    IsAdmin = true
                });
                await context.SaveChangesAsync();
                app.Logger.LogInformation("Conta administrativa local provisionada por configuração externa.");
            }
        }
        app.Logger.LogInformation("Banco de desenvolvimento inicializado sem operação destrutiva.");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Falha ao inicializar banco de desenvolvimento.");
    }
}

app.Run();

public partial class Program { }
