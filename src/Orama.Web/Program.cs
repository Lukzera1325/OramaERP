using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Orama.Application.Services;
using Orama.Domain.Entities;
using Orama.Infra.Data.Context;

var builder = WebApplication.CreateBuilder(args);

// Configuração do Entity Framework com SQLite (desenvolvimento local)
builder.Services.AddDbContext<OramaDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configuração do MVC
builder.Services.AddControllersWithViews();

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
var jwtKey = builder.Configuration["Jwt:Key"] ?? "ChaveSecretaParaOramaGoMobile2024!@#";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "OramaERP";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "OramaGoMobile";

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
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

// Configuração de CORS para API
builder.Services.AddCors(options =>
{
    options.AddPolicy("OramaGoMobile", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
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

// Aplicar migrations automaticamente em desenvolvimento
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<OramaDbContext>();
    
    try
    {
        // Deletar banco existente se estiver vazio ou corrompido
        if (context.Database.GetDbConnection().State == System.Data.ConnectionState.Closed)
        {
            await context.Database.OpenConnectionAsync();
        }
        
        // Verificar se o banco precisa ser recriado
        var canConnect = await context.Database.CanConnectAsync();
        if (!canConnect)
        {
            Console.WriteLine("🔄 Recriando banco de dados...");
            await context.Database.EnsureDeletedAsync();
        }
        
        // Criar o banco de dados com os dados iniciais
        var created = await context.Database.EnsureCreatedAsync();
        
        if (created)
        {
            Console.WriteLine("✅ Banco de dados criado com sucesso!");
        }
        else
        {
            Console.WriteLine("ℹ️ Banco de dados já existe");
        }
        
        // Verificar se o usuário admin existe
        var adminExists = await context.Usuarios.AnyAsync(u => u.Email == "admin@orama.com.br");
        if (adminExists)
        {
            Console.WriteLine("👤 Usuário administrador encontrado");
        }
        else
        {
            Console.WriteLine("⚠️ Usuário administrador não encontrado!");
        }
        
        Console.WriteLine("📧 Login: admin@orama.com.br");
        Console.WriteLine("🔑 Senha: Admin@123");
        Console.WriteLine($"🌐 URL: http://localhost:5050");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️ Erro ao inicializar o banco de dados: {ex.Message}");
        if (ex.InnerException != null)
        {
            Console.WriteLine($"Erro interno: {ex.InnerException.Message}");
        }
    }
}

app.Run();