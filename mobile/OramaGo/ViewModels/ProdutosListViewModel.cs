using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OramaGo.Models;
using OramaGo.Services;
using System.Collections.ObjectModel;

namespace OramaGo.ViewModels;

public partial class ProdutosListViewModel : BaseViewModel
{
    private readonly IProdutoService _produtoService;
    private readonly INavigationService _navigationService;
    private readonly IPermissionService _permissionService;

    [ObservableProperty]
    private ObservableCollection<ProdutoLocal> produtos = new();

    [ObservableProperty]
    private ObservableCollection<ProdutoLocal> produtosFiltrados = new();

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private bool hasProdutos;

    [ObservableProperty]
    private int totalProdutos;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    [ObservableProperty]
    private bool canAddProduto;

    [ObservableProperty]
    private bool canEditProduto;

    [ObservableProperty]
    private string selectedCategoria = "Todas";

    [ObservableProperty]
    private ObservableCollection<string> categorias = new() { "Todas" };

    [ObservableProperty]
    private bool showOnlyAvailable = false;

    public ProdutosListViewModel(
        IProdutoService produtoService,
        INavigationService navigationService,
        IPermissionService permissionService)
    {
        _produtoService = produtoService;
        _navigationService = navigationService;
        _permissionService = permissionService;
        
        Title = "Produtos";
    }

    public override async Task InitializeAsync()
    {
        await CheckPermissionsAsync();
        await LoadDataAsync();
        await LoadCategoriasAsync();
    }

    protected override async Task LoadDataAsync()
    {
        try
        {
            SetBusy(true);
            StatusMessage = "Carregando produtos...";

            var produtosList = await _produtoService.GetAllAsync();
            
            Produtos.Clear();
            foreach (var produto in produtosList.OrderBy(p => p.Nome))
            {
                Produtos.Add(produto);
            }

            await ApplyFilterAsync();
            
            TotalProdutos = Produtos.Count;
            HasProdutos = TotalProdutos > 0;
            
            StatusMessage = HasProdutos 
                ? $"{TotalProdutos} produto(s) encontrado(s)"
                : "Nenhum produto cadastrado";
        }
        catch (Exception ex)
        {
            StatusMessage = "Erro ao carregar produtos";
            await _navigationService.DisplayAlertAsync("Erro", $"Erro ao carregar produtos: {ex.Message}", "OK");
        }
        finally
        {
            SetBusy(false);
        }
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        await ApplyFilterAsync();
    }

    [RelayCommand]
    private async Task ClearSearchAsync()
    {
        SearchText = string.Empty;
        SelectedCategoria = "Todas";
        ShowOnlyAvailable = false;
        await ApplyFilterAsync();
    }

    [RelayCommand]
    private async Task AddProdutoAsync()
    {
        if (!CanAddProduto)
        {
            await _navigationService.DisplayAlertAsync("Acesso Negado", "Você não tem permissão para adicionar produtos", "OK");
            return;
        }

        await _navigationService.NavigateToAsync("produtos/editar", new Dictionary<string, object>
        {
            ["IsNew"] = true
        });
    }

    [RelayCommand]
    private async Task ViewProdutoAsync(ProdutoLocal produto)
    {
        if (produto == null) return;

        await _navigationService.NavigateToAsync("produtos/detalhes", new Dictionary<string, object>
        {
            ["ProdutoId"] = produto.Id
        });
    }

    [RelayCommand]
    private async Task EditProdutoAsync(ProdutoLocal produto)
    {
        if (produto == null) return;

        if (!CanEditProduto)
        {
            await _navigationService.DisplayAlertAsync("Acesso Negado", "Você não tem permissão para editar produtos", "OK");
            return;
        }

        await _navigationService.NavigateToAsync("produtos/editar", new Dictionary<string, object>
        {
            ["ProdutoId"] = produto.Id,
            ["IsNew"] = false
        });
    }

    [RelayCommand]
    private async Task DeleteProdutoAsync(ProdutoLocal produto)
    {
        if (produto == null) return;

        if (!CanEditProduto)
        {
            await _navigationService.DisplayAlertAsync("Acesso Negado", "Você não tem permissão para excluir produtos", "OK");
            return;
        }

        var confirm = await _navigationService.DisplayConfirmAsync(
            "Confirmar Exclusão",
            $"Deseja realmente excluir o produto '{produto.Nome}'?",
            "Sim",
            "Não");

        if (confirm)
        {
            try
            {
                SetBusy(true);
                await _produtoService.DeleteAsync(produto.Id);
                
                Produtos.Remove(produto);
                await ApplyFilterAsync();
                
                TotalProdutos = Produtos.Count;
                HasProdutos = TotalProdutos > 0;
                
                await _navigationService.DisplayAlertAsync("Sucesso", "Produto excluído com sucesso", "OK");
            }
            catch (Exception ex)
            {
                await _navigationService.DisplayAlertAsync("Erro", $"Erro ao excluir produto: {ex.Message}", "OK");
            }
            finally
            {
                SetBusy(false);
            }
        }
    }

    [RelayCommand]
    private async Task FilterByCategoriaAsync()
    {
        await ApplyFilterAsync();
    }

    [RelayCommand]
    private async Task ToggleAvailableFilterAsync()
    {
        ShowOnlyAvailable = !ShowOnlyAvailable;
        await ApplyFilterAsync();
    }

    private async Task ApplyFilterAsync()
    {
        try
        {
            ProdutosFiltrados.Clear();

            var filteredProdutos = Produtos.AsEnumerable();

            // Filtro por texto de busca
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var searchTerm = SearchText.ToLower().Trim();
                filteredProdutos = filteredProdutos.Where(p => 
                    p.Nome.ToLower().Contains(searchTerm) ||
                    p.Codigo.ToLower().Contains(searchTerm) ||
                    (!string.IsNullOrEmpty(p.Categoria) && p.Categoria.ToLower().Contains(searchTerm)) ||
                    (!string.IsNullOrEmpty(p.Descricao) && p.Descricao.ToLower().Contains(searchTerm)));
            }

            // Filtro por categoria
            if (SelectedCategoria != "Todas" && !string.IsNullOrEmpty(SelectedCategoria))
            {
                filteredProdutos = filteredProdutos.Where(p => p.Categoria == SelectedCategoria);
            }

            // Filtro por disponibilidade
            if (ShowOnlyAvailable)
            {
                filteredProdutos = filteredProdutos.Where(p => p.EstoqueAtual > 0);
            }

            foreach (var produto in filteredProdutos.OrderBy(p => p.Nome))
            {
                ProdutosFiltrados.Add(produto);
            }

            StatusMessage = ProdutosFiltrados.Count > 0
                ? $"{ProdutosFiltrados.Count} produto(s) encontrado(s)"
                : string.IsNullOrWhiteSpace(SearchText) && SelectedCategoria == "Todas" && !ShowOnlyAvailable
                    ? "Nenhum produto cadastrado"
                    : "Nenhum produto encontrado com os critérios de busca";

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            await _navigationService.DisplayAlertAsync("Erro", $"Erro ao filtrar produtos: {ex.Message}", "OK");
        }
    }

    private async Task LoadCategoriasAsync()
    {
        try
        {
            var categoriasList = await _produtoService.GetCategoriasAsync();
            
            Categorias.Clear();
            Categorias.Add("Todas");
            
            foreach (var categoria in categoriasList.OrderBy(c => c))
            {
                Categorias.Add(categoria);
            }
        }
        catch (Exception ex)
        {
            await _navigationService.DisplayAlertAsync("Erro", $"Erro ao carregar categorias: {ex.Message}", "OK");
        }
    }

    private async Task CheckPermissionsAsync()
    {
        CanAddProduto = await _permissionService.HasPermissionAsync(OramaGo.Services.Permissions.ProdutosIncluir);
        CanEditProduto = await _permissionService.HasPermissionAsync(OramaGo.Services.Permissions.ProdutosAlterar);
    }

    partial void OnSearchTextChanged(string value)
    {
        // Aplicar filtro com delay para evitar muitas chamadas
        Task.Run(async () =>
        {
            await Task.Delay(300);
            if (SearchText == value) // Verificar se ainda é o mesmo texto
            {
                await ApplyFilterAsync();
            }
        });
    }

    partial void OnSelectedCategoriaChanged(string value)
    {
        Task.Run(async () => await ApplyFilterAsync());
    }
}