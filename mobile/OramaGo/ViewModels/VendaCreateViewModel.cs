using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentValidation;
using OramaGo.Models;
using OramaGo.Services;
using System.Collections.ObjectModel;

namespace OramaGo.ViewModels;

public partial class VendaCreateViewModel : BaseViewModel
{
    private readonly IVendaService _vendaService;
    private readonly IVendaItemService _vendaItemService;
    private readonly IClienteService _clienteService;
    private readonly IProdutoService _produtoService;
    private readonly INavigationService _navigationService;
    private readonly IPermissionService _permissionService;
    private readonly IValidator<VendaLocal> _validator;

    [ObservableProperty]
    private VendaLocal venda = new();

    [ObservableProperty]
    private ObservableCollection<ClienteLocal> clientes = new();

    [ObservableProperty]
    private ObservableCollection<ProdutoLocal> produtos = new();

    [ObservableProperty]
    private ObservableCollection<VendaItemLocal> itens = new();

    [ObservableProperty]
    private ClienteLocal? clienteSelecionado;

    [ObservableProperty]
    private ProdutoLocal? produtoSelecionado;

    [ObservableProperty]
    private decimal quantidadeProduto = 1;

    [ObservableProperty]
    private decimal precoProduto;

    [ObservableProperty]
    private string searchClienteText = string.Empty;

    [ObservableProperty]
    private string searchProdutoText = string.Empty;

    [ObservableProperty]
    private bool isEditMode;

    [ObservableProperty]
    private bool canSave = true;

    [ObservableProperty]
    private bool canAddItem = true;

    [ObservableProperty]
    private bool hasItens;

    [ObservableProperty]
    private string validationErrors = string.Empty;

    public List<string> FormaPagamentoOptions { get; } = new()
    {
        "À Vista",
        "Boleto",
        "Cartão de Crédito", 
        "Cartão de Débito",
        "PIX",
        "Cheque",
        "Parcelado"
    };

    public int VendaId { get; set; }

    public VendaCreateViewModel(
        IVendaService vendaService,
        IVendaItemService vendaItemService,
        IClienteService clienteService,
        IProdutoService produtoService,
        INavigationService navigationService,
        IPermissionService permissionService,
        IValidator<VendaLocal> validator)
    {
        _vendaService = vendaService;
        _vendaItemService = vendaItemService;
        _clienteService = clienteService;
        _produtoService = produtoService;
        _navigationService = navigationService;
        _permissionService = permissionService;
        _validator = validator;
        
        Title = "Nova Venda";
        
        // Inicializar venda
        Venda = new VendaLocal
        {
            DataVenda = DateTime.Today,
            Status = StatusVendaLocal.Orcamento,
            FormaPagamento = FormaPagamentoLocal.AVista,
            Parcelas = 1
        };
    }

    public override async Task InitializeAsync()
    {
        await CheckPermissionsAsync();
        await LoadDataAsync();
        
        if (VendaId > 0)
        {
            await LoadVendaAsync();
        }
    }

    protected override async Task LoadDataAsync()
    {
        try
        {
            SetBusy(true);
            
            // Carregar clientes
            var clientesList = await _clienteService.GetAllAsync();
            Clientes.Clear();
            foreach (var cliente in clientesList.OrderBy(c => c.Nome))
            {
                Clientes.Add(cliente);
            }
            
            // Carregar produtos ativos para venda
            var produtosList = await _produtoService.GetAtivosParaVendaAsync();
            Produtos.Clear();
            foreach (var produto in produtosList.OrderBy(p => p.Nome))
            {
                Produtos.Add(produto);
            }
        }
        catch (Exception ex)
        {
            await _navigationService.DisplayAlertAsync("Erro", $"Erro ao carregar dados: {ex.Message}", "OK");
        }
        finally
        {
            SetBusy(false);
        }
    }

    [RelayCommand]
    private async Task SearchClienteAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchClienteText))
        {
            await LoadDataAsync();
            return;
        }

        try
        {
            var clientesFiltrados = await _clienteService.SearchAsync(SearchClienteText);
            
            Clientes.Clear();
            foreach (var cliente in clientesFiltrados.OrderBy(c => c.Nome))
            {
                Clientes.Add(cliente);
            }
        }
        catch (Exception ex)
        {
            await _navigationService.DisplayAlertAsync("Erro", $"Erro ao buscar clientes: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task SearchProdutoAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchProdutoText))
        {
            await LoadDataAsync();
            return;
        }

        try
        {
            var produtosFiltrados = await _produtoService.SearchAsync(SearchProdutoText);
            
            Produtos.Clear();
            foreach (var produto in produtosFiltrados.OrderBy(p => p.Nome))
            {
                Produtos.Add(produto);
            }
        }
        catch (Exception ex)
        {
            await _navigationService.DisplayAlertAsync("Erro", $"Erro ao buscar produtos: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task AdicionarItemAsync()
    {
        if (ClienteSelecionado == null)
        {
            await _navigationService.DisplayAlertAsync("Atenção", "Selecione um cliente primeiro", "OK");
            return;
        }

        if (ProdutoSelecionado == null)
        {
            await _navigationService.DisplayAlertAsync("Atenção", "Selecione um produto", "OK");
            return;
        }

        if (QuantidadeProduto <= 0)
        {
            await _navigationService.DisplayAlertAsync("Atenção", "Quantidade deve ser maior que zero", "OK");
            return;
        }

        try
        {
            SetBusy(true);

            // Salvar venda se ainda não foi salva
            if (Venda.Id == 0)
            {
                Venda.ClienteId = ClienteSelecionado.Id;
                Venda = await _vendaService.CreateAsync(Venda);
                IsEditMode = true;
                Title = $"Venda {Venda.Numero}";
            }

            // Usar preço do produto se não foi informado
            var preco = PrecoProduto > 0 ? PrecoProduto : ProdutoSelecionado.PrecoVenda;

            // Validar estoque
            if (ProdutoSelecionado.ControlaEstoque && ProdutoSelecionado.EstoqueAtual < QuantidadeProduto)
            {
                await _navigationService.DisplayAlertAsync("Estoque Insuficiente", 
                    $"Estoque disponível: {ProdutoSelecionado.EstoqueAtual:N2} {ProdutoSelecionado.Unidade}", "OK");
                return;
            }

            // Adicionar item
            var novoItem = await _vendaItemService.AdicionarItemAsync(
                Venda.Id, 
                ProdutoSelecionado.Id, 
                QuantidadeProduto, 
                preco);

            // Atualizar lista de itens
            await LoadItensAsync();

            // Limpar seleção
            ProdutoSelecionado = null;
            QuantidadeProduto = 1;
            PrecoProduto = 0;
            SearchProdutoText = string.Empty;

            await _navigationService.DisplayAlertAsync("Sucesso", "Item adicionado com sucesso", "OK");
        }
        catch (Exception ex)
        {
            await _navigationService.DisplayAlertAsync("Erro", $"Erro ao adicionar item: {ex.Message}", "OK");
        }
        finally
        {
            SetBusy(false);
        }
    }

    [RelayCommand]
    private async Task RemoverItemAsync(VendaItemLocal item)
    {
        if (item == null) return;

        var confirm = await _navigationService.DisplayConfirmAsync(
            "Confirmar Remoção",
            $"Deseja remover o item '{item.Produto?.Nome}'?",
            "Sim",
            "Não");

        if (confirm)
        {
            try
            {
                SetBusy(true);
                await _vendaItemService.RemoverItemAsync(item.Id);
                await LoadItensAsync();
                
                await _navigationService.DisplayAlertAsync("Sucesso", "Item removido com sucesso", "OK");
            }
            catch (Exception ex)
            {
                await _navigationService.DisplayAlertAsync("Erro", $"Erro ao remover item: {ex.Message}", "OK");
            }
            finally
            {
                SetBusy(false);
            }
        }
    }

    [RelayCommand]
    private async Task EditarItemAsync(VendaItemLocal item)
    {
        if (item == null) return;

        // Navegar para tela de edição de item
        await _navigationService.NavigateToAsync("vendas/item/editar", new Dictionary<string, object>
        {
            ["ItemId"] = item.Id
        });
    }

    [RelayCommand]
    private async Task SalvarVendaAsync()
    {
        try
        {
            SetBusy(true);
            ValidationErrors = string.Empty;

            if (ClienteSelecionado == null)
            {
                ValidationErrors = "Selecione um cliente";
                return;
            }

            // Validar venda
            var validationResult = await _validator.ValidateAsync(Venda);
            if (!validationResult.IsValid)
            {
                ValidationErrors = string.Join("\n", validationResult.Errors.Select(e => e.ErrorMessage));
                return;
            }

            if (Venda.Id == 0)
            {
                // Nova venda
                Venda.ClienteId = ClienteSelecionado.Id;
                Venda = await _vendaService.CreateAsync(Venda);
                IsEditMode = true;
                Title = $"Venda {Venda.Numero}";
            }
            else
            {
                // Atualizar venda existente
                Venda = await _vendaService.UpdateAsync(Venda);
            }

            await _navigationService.DisplayAlertAsync("Sucesso", "Venda salva com sucesso", "OK");
        }
        catch (Exception ex)
        {
            await _navigationService.DisplayAlertAsync("Erro", $"Erro ao salvar venda: {ex.Message}", "OK");
        }
        finally
        {
            SetBusy(false);
        }
    }

    [RelayCommand]
    private async Task AprovarVendaAsync()
    {
        if (Venda.Id == 0) return;

        var confirm = await _navigationService.DisplayConfirmAsync(
            "Confirmar Aprovação",
            "Deseja aprovar esta venda?",
            "Sim",
            "Não");

        if (confirm)
        {
            try
            {
                SetBusy(true);
                Venda = await _vendaService.AprovarVendaAsync(Venda.Id);
                
                await _navigationService.DisplayAlertAsync("Sucesso", "Venda aprovada com sucesso", "OK");
                await _navigationService.NavigateBackAsync();
            }
            catch (Exception ex)
            {
                await _navigationService.DisplayAlertAsync("Erro", $"Erro ao aprovar venda: {ex.Message}", "OK");
            }
            finally
            {
                SetBusy(false);
            }
        }
    }

    [RelayCommand]
    private async Task CancelarVendaAsync()
    {
        if (Venda.Id == 0) return;

        var motivo = await _navigationService.DisplayPromptAsync(
            "Cancelar Venda",
            "Informe o motivo do cancelamento:",
            "Motivo...");

        if (!string.IsNullOrWhiteSpace(motivo))
        {
            try
            {
                SetBusy(true);
                Venda = await _vendaService.CancelarVendaAsync(Venda.Id, motivo);
                
                await _navigationService.DisplayAlertAsync("Sucesso", "Venda cancelada com sucesso", "OK");
                await _navigationService.NavigateBackAsync();
            }
            catch (Exception ex)
            {
                await _navigationService.DisplayAlertAsync("Erro", $"Erro ao cancelar venda: {ex.Message}", "OK");
            }
            finally
            {
                SetBusy(false);
            }
        }
    }

    private async Task LoadVendaAsync()
    {
        try
        {
            SetBusy(true);
            
            var venda = await _vendaService.GetByIdAsync(VendaId);
            if (venda == null)
            {
                await _navigationService.DisplayAlertAsync("Erro", "Venda não encontrada", "OK");
                await _navigationService.NavigateBackAsync();
                return;
            }

            Venda = venda;
            IsEditMode = true;
            Title = $"Venda {Venda.Numero}";
            
            // Selecionar cliente
            ClienteSelecionado = Clientes.FirstOrDefault(c => c.Id == Venda.ClienteId);
            
            // Carregar itens
            await LoadItensAsync();
        }
        catch (Exception ex)
        {
            await _navigationService.DisplayAlertAsync("Erro", $"Erro ao carregar venda: {ex.Message}", "OK");
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async Task LoadItensAsync()
    {
        if (Venda.Id == 0) return;

        try
        {
            var itensList = await _vendaItemService.GetByVendaIdAsync(Venda.Id);
            
            Itens.Clear();
            foreach (var item in itensList)
            {
                Itens.Add(item);
            }
            
            HasItens = Itens.Count > 0;
            
            // Recarregar venda para obter totais atualizados
            var vendaAtualizada = await _vendaService.GetByIdAsync(Venda.Id);
            if (vendaAtualizada != null)
            {
                Venda.SubTotal = vendaAtualizada.SubTotal;
                Venda.ValorTotal = vendaAtualizada.ValorTotal;
            }
        }
        catch (Exception ex)
        {
            await _navigationService.DisplayAlertAsync("Erro", $"Erro ao carregar itens: {ex.Message}", "OK");
        }
    }

    private async Task CheckPermissionsAsync()
    {
        CanSave = await _permissionService.HasPermissionAsync(OramaGo.Services.Permissions.VendasIncluir);
        CanAddItem = CanSave;
    }

    partial void OnClienteSelecionadoChanged(ClienteLocal? value)
    {
        if (value != null && Venda.Id == 0)
        {
            Venda.ClienteId = value.Id;
        }
    }

    partial void OnProdutoSelecionadoChanged(ProdutoLocal? value)
    {
        if (value != null)
        {
            PrecoProduto = value.PrecoVenda;
        }
    }
}