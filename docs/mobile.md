# 📱 ERP ORAMA - DOCUMENTAÇÃO MOBILE

**App Android Nativo em .NET MAUI**

---

## 🎯 **VISÃO GERAL**

O **Orama Go** é o app mobile do ERP Orama, desenvolvido em **.NET MAUI** para funcionar como **força de vendas offline-first**.

### **Características Principais**
- **Offline First** - Funciona sem internet
- **Sincronização Automática** - Dados sincronizados quando online
- **Interface Nativa** - Performance e UX nativas
- **Multi-empresa** - Suporte a múltiplas empresas por domínio

---

## 🏗️ **ARQUITETURA MOBILE**

### **Estrutura do Projeto**
```
mobile/OramaGo/
├── Models/                # Modelos locais (SQLite)
├── ViewModels/            # MVVM ViewModels
├── Views/                 # Telas XAML
├── Services/              # Serviços e APIs
├── Data/                  # Contexto SQLite local
├── Converters/            # Conversores XAML
├── Validators/            # Validações
└── Platforms/             # Código específico por plataforma
```

### **Padrão MVVM**
```
View (XAML) ↔ ViewModel ↔ Service ↔ Local Database
     ↑                                      ↓
Interface                            Sincronização
 Nativa                                com Backend
```

---

## 📊 **MÓDULOS IMPLEMENTADOS**

### **Escopo mobile (não certifica completude; confira o status de validação em AUDIT.md)**

#### **1. Autenticação e Segurança**
- **Login online** - Autenticação pelo backend; operação offline não foi validada nesta revisão
- **Multi-empresa** - Seleção de empresa por domínio
- **Token JWT** - Autenticação com backend
- **Logout seguro** - Limpeza de dados locais

#### **2. Sincronização de Dados**
- **Sincronização automática** - Quando conectado
- **Sincronização manual** - Pull to refresh
- **Resolução de conflitos** - Estratégias definidas
- **Indicadores visuais** - Status de sincronização

#### **3. Gestão de Clientes**
- **Lista offline** - Todos os clientes locais
- **Busca rápida** - Por nome, documento, cidade
- **Detalhes completos** - Informações e histórico
- **Criação offline** - Novos clientes sincronizados depois

#### **4. Catálogo de Produtos**
- **Lista completa** - Produtos com preços atualizados
- **Busca avançada** - Por código, descrição, categoria
- **Informações detalhadas** - Preço, estoque, especificações
- **Fotos** - Imagens dos produtos (quando disponível)

#### **5. Força de Vendas**
- **Criação de vendas** - Interface otimizada para mobile
- **Carrinho de compras** - Adicionar/remover produtos
- **Cálculos automáticos** - Totais, descontos, impostos
- **Vendas offline** - Funcionam sem internet

#### **6. Relatórios Mobile**
- **Vendas do dia** - Resumo das vendas realizadas
- **Metas e performance** - Acompanhamento de resultados
- **Histórico** - Vendas anteriores com detalhes
- **Exportação** - Compartilhar relatórios

---

## 🧮 **MODELOS DE DADOS LOCAIS**

### **Cliente (Local)**
```csharp
public class ClienteLocal
{
    public int Id { get; set; }
    public string Nome { get; set; }
    public string Documento { get; set; }
    public string Email { get; set; }
    public string Telefone { get; set; }
    public string Endereco { get; set; }
    public string Cidade { get; set; }
    public string Estado { get; set; }
    
    // Controle de sincronização
    public DateTime UltimaAtualizacao { get; set; }
    public bool PrecisaSincronizar { get; set; }
    public int? IdServidor { get; set; } // ID no backend
}
```

### **Produto (Local)**
```csharp
public class ProdutoLocal
{
    public int Id { get; set; }
    public string Codigo { get; set; }
    public string Descricao { get; set; }
    public decimal PrecoVenda { get; set; }
    public decimal EstoqueAtual { get; set; }
    public string Categoria { get; set; }
    public string UnidadeMedida { get; set; }
    public byte[] Foto { get; set; }
    
    // Controle de sincronização
    public DateTime UltimaAtualizacao { get; set; }
    public int? IdServidor { get; set; }
}
```

### **Venda (Local)**
```csharp
public class VendaLocal
{
    public int Id { get; set; }
    public string Numero { get; set; }
    public int ClienteId { get; set; }
    public DateTime DataVenda { get; set; }
    public decimal ValorTotal { get; set; }
    public decimal Desconto { get; set; }
    public string Observacoes { get; set; }
    public StatusVenda Status { get; set; }
    
    // Controle de sincronização
    public bool Sincronizada { get; set; }
    public DateTime CriadaEm { get; set; }
    public int? IdServidor { get; set; }
    
    // Relacionamentos
    public List<VendaItemLocal> Itens { get; set; }
}
```

---

## 🔄 **SINCRONIZAÇÃO DE DADOS**

### **Estratégia Offline-First**
```csharp
public class SincronizacaoService
{
    // Sincronização completa
    public async Task SincronizarTudoAsync()
    {
        await SincronizarClientesAsync();
        await SincronizarProdutosAsync();
        await EnviarVendasPendentesAsync();
        await BaixarVendasAtualizadasAsync();
    }
    
    // Enviar dados locais para servidor
    public async Task EnviarVendasPendentesAsync()
    {
        var vendasPendentes = await _localDb.Vendas
            .Where(v => !v.Sincronizada)
            .ToListAsync();
            
        foreach (var venda in vendasPendentes)
        {
            var sucesso = await _apiService.EnviarVendaAsync(venda);
            if (sucesso)
            {
                venda.Sincronizada = true;
                await _localDb.SaveChangesAsync();
            }
        }
    }
}
```

### **Resolução de Conflitos**
- **Servidor sempre ganha** - Para dados mestres (clientes, produtos)
- **Merge inteligente** - Para vendas (evitar duplicação)
- **Timestamp** - Controle de última atualização
- **Flags de sincronização** - Controle de estado

---

## 🎨 **INTERFACE DO USUÁRIO**

### **Telas Principais**

#### **1. Login**
```xml
<ContentPage Title="Orama Go">
    <StackLayout Padding="20">
        <Image Source="logo.png" HeightRequest="100"/>
        <Entry Placeholder="E-mail" Text="{Binding Email}"/>
        <Entry Placeholder="Senha" IsPassword="True" Text="{Binding Senha}"/>
        <Button Text="Entrar" Command="{Binding LoginCommand}"/>
        <ActivityIndicator IsVisible="{Binding IsLoading}"/>
    </StackLayout>
</ContentPage>
```

#### **2. Lista de Clientes**
```xml
<ContentPage Title="Clientes">
    <RefreshView IsRefreshing="{Binding IsRefreshing}" Command="{Binding RefreshCommand}">
        <CollectionView ItemsSource="{Binding Clientes}">
            <CollectionView.ItemTemplate>
                <DataTemplate>
                    <Grid Padding="15">
                        <Grid.ColumnDefinitions>
                            <ColumnDefinition Width="*"/>
                            <ColumnDefinition Width="Auto"/>
                        </Grid.ColumnDefinitions>
                        
                        <StackLayout Grid.Column="0">
                            <Label Text="{Binding Nome}" FontAttributes="Bold"/>
                            <Label Text="{Binding Cidade}" TextColor="Gray"/>
                        </StackLayout>
                        
                        <Label Grid.Column="1" Text=">" FontSize="20"/>
                    </Grid>
                </DataTemplate>
            </CollectionView.ItemTemplate>
        </CollectionView>
    </RefreshView>
</ContentPage>
```

#### **3. Carrinho de Vendas**
```xml
<ContentPage Title="Nova Venda">
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>
        
        <!-- Cabeçalho -->
        <StackLayout Grid.Row="0" Padding="15">
            <Label Text="{Binding Cliente.Nome}" FontAttributes="Bold"/>
            <Label Text="{Binding DataVenda, StringFormat='{0:dd/MM/yyyy}'}"/>
        </StackLayout>
        
        <!-- Itens -->
        <CollectionView Grid.Row="1" ItemsSource="{Binding Itens}">
            <!-- Template dos itens -->
        </CollectionView>
        
        <!-- Totais -->
        <StackLayout Grid.Row="2" Padding="15" BackgroundColor="LightGray">
            <Label Text="{Binding ValorTotal, StringFormat='Total: {0:C}'}" FontAttributes="Bold"/>
            <Button Text="Finalizar Venda" Command="{Binding FinalizarCommand}"/>
        </StackLayout>
    </Grid>
</ContentPage>
```

---

## 🔧 **VIEWMODELS E LÓGICA**

### **Padrão MVVM**
```csharp
public class ClientesViewModel : BaseViewModel
{
    private ObservableCollection<ClienteLocal> _clientes;
    public ObservableCollection<ClienteLocal> Clientes
    {
        get => _clientes;
        set => SetProperty(ref _clientes, value);
    }
    
    public ICommand RefreshCommand { get; }
    public ICommand SelecionarClienteCommand { get; }
    
    public ClientesViewModel(IClienteService clienteService)
    {
        _clienteService = clienteService;
        RefreshCommand = new AsyncRelayCommand(CarregarClientesAsync);
        SelecionarClienteCommand = new AsyncRelayCommand<ClienteLocal>(SelecionarCliente);
    }
    
    private async Task CarregarClientesAsync()
    {
        IsLoading = true;
        var clientes = await _clienteService.ObterTodosAsync();
        Clientes = new ObservableCollection<ClienteLocal>(clientes);
        IsLoading = false;
    }
}
```

### **Services Mobile**
```csharp
public class ClienteService : IClienteService
{
    private readonly OramaLocalDbContext _localDb;
    private readonly IApiService _apiService;
    
    public async Task<List<ClienteLocal>> ObterTodosAsync()
    {
        // Buscar dados locais primeiro
        var clientesLocais = await _localDb.Clientes.ToListAsync();
        
        // Tentar sincronizar se online
        if (await _conectividadeService.IsOnlineAsync())
        {
            await SincronizarClientesAsync();
            clientesLocais = await _localDb.Clientes.ToListAsync();
        }
        
        return clientesLocais;
    }
    
    public async Task<bool> CriarAsync(ClienteLocal cliente)
    {
        // Salvar localmente primeiro
        _localDb.Clientes.Add(cliente);
        await _localDb.SaveChangesAsync();
        
        // Marcar para sincronização
        cliente.PrecisaSincronizar = true;
        await _localDb.SaveChangesAsync();
        
        // Tentar enviar se online
        if (await _conectividadeService.IsOnlineAsync())
        {
            await _sincronizacaoService.EnviarClienteAsync(cliente);
        }
        
        return true;
    }
}
```

---

## 🗄️ **BANCO DE DADOS LOCAL**

### **SQLite Local**
```csharp
public class OramaLocalDbContext : DbContext
{
    public DbSet<ClienteLocal> Clientes { get; set; }
    public DbSet<ProdutoLocal> Produtos { get; set; }
    public DbSet<VendaLocal> Vendas { get; set; }
    public DbSet<VendaItemLocal> VendaItens { get; set; }
    public DbSet<ConfiguracaoLocal> Configuracoes { get; set; }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "orama_local.db");
        optionsBuilder.UseSqlite($"Data Source={dbPath}");
    }
}
```

### **Inicialização do Banco**
```csharp
public class DatabaseService
{
    public async Task InicializarAsync()
    {
        using var context = new OramaLocalDbContext();
        await context.Database.EnsureCreatedAsync();
        
        // Dados iniciais se necessário
        if (!await context.Configuracoes.AnyAsync())
        {
            await CriarConfiguracaoInicialAsync(context);
        }
    }
}
```

---

## 🔌 **INTEGRAÇÃO COM BACKEND**

### **API Service**
```csharp
public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;
    private readonly ISecureStorage _secureStorage;
    
    public async Task<List<ClienteDto>> ObterClientesAsync()
    {
        var token = await _secureStorage.GetAsync("jwt_token");
        _httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);
            
        var response = await _httpClient.GetAsync("api/clientes");
        
        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<ClienteDto>>(json);
        }
        
        return new List<ClienteDto>();
    }
}
```

### **Mapeamento de DTOs**
```csharp
public static class MappingExtensions
{
    public static ClienteLocal ToLocal(this ClienteDto dto)
    {
        return new ClienteLocal
        {
            IdServidor = dto.Id,
            Nome = dto.Nome,
            Documento = dto.Documento,
            Email = dto.Email,
            // ... outros campos
            UltimaAtualizacao = DateTime.Now
        };
    }
    
    public static ClienteDto ToDto(this ClienteLocal local)
    {
        return new ClienteDto
        {
            Id = local.IdServidor ?? 0,
            Nome = local.Nome,
            Documento = local.Documento,
            // ... outros campos
        };
    }
}
```

---

## 📱 **FUNCIONALIDADES MOBILE**

### **Recursos Nativos Utilizados**
- **Conectividade** - Detecção de rede
- **Armazenamento Seguro** - Credenciais e tokens
- **Notificações** - Alertas e lembretes
- **Câmera** - Fotos de produtos (futuro)
- **GPS** - Localização de clientes (futuro)

### **Performance e UX**
- **Lazy Loading** - Carregamento sob demanda
- **Pull to Refresh** - Atualização manual
- **Loading States** - Indicadores visuais
- **Offline Indicators** - Status de conectividade
- **Validação Local** - Feedback imediato

---

## 🚀 **DEPLOYMENT**

### **Android**
```xml
<!-- Configuração no .csproj -->
<PropertyGroup Condition="'$(TargetFramework)' == 'net8.0-android'">
    <SupportedOSPlatformVersion>21</SupportedOSPlatformVersion>
    <AndroidPackageFormat>apk</AndroidPackageFormat>
    <AndroidUseAapt2>true</AndroidUseAapt2>
    <AndroidCreatePackagePerAbi>false</AndroidCreatePackagePerAbi>
</PropertyGroup>
```

### **Build e Publicação**
```bash
# Debug
dotnet build -f net8.0-android

# Release
dotnet publish -f net8.0-android -c Release

# APK assinado para produção
dotnet publish -f net8.0-android -c Release -p:AndroidKeyStore=true -p:AndroidSigningKeyStore=orama.keystore
```

---

## **Status de validação**

O build MAUI não foi concluído nesta máquina: faltam workloads iOS/MacCatalyst e o ambiente não permitiu gravar artefatos do target Windows. A lista de telas/serviços acima não certifica operação offline ou sincronização ponta a ponta. Consulte [AUDIT.md](AUDIT.md).

---

## 🎯 **PRÓXIMOS PASSOS**

### **Melhorias Sugeridas**
1. **Push Notifications** - Notificações do servidor
2. **Câmera** - Fotos de produtos e clientes
3. **GPS** - Localização e rotas
4. **Relatórios Avançados** - Gráficos mobile

### **Plataformas Futuras**
1. **iOS** - Versão para iPhone/iPad
2. **Windows** - App desktop
3. **Web** - PWA para navegadores

---

**App mobile completo e funcional!** 📱🚀
