using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OramaGo.Models;
using OramaGo.Services;

namespace OramaGo.ViewModels;

[QueryProperty(nameof(ProdutoId), "ProdutoId")]
public partial class ProdutoDetailViewModel : BaseViewModel
{
    private readonly IProdutoService _produtoService;
    private readonly INavigationService _navigationService;
    private readonly IPermissionService _permissionService;

    [ObservableProperty]
    private ProdutoLocal? produto;

    [ObservableProperty]
    private bool canEditProduto;

    [ObservableProperty]
    private bool hasImage;

    [ObservableProperty]
    private string statusEstoque = string.Empty;

    [ObservableProperty]
    private string statusEstoqueColor = "#4CAF50";

    [ObservableProperty]
    private bool isEstoqueBaixo;

    [ObservableProperty]
    private bool isEstoqueDisponivel;

    public int ProdutoId { get; set; }

    public ProdutoDetailViewModel(
        IProdutoService produtoService,
        INavigationService navigationService,
        IPermissionService permissionService)
    {
        _produtoService = produtoService;
        _navigationService = navigationService;
        _permissionService = permissionService;
        
        Title = "Detalhes do Produto";
    }

    public override async Task InitializeAsync()
    {
        await CheckPermissionsAsync();
        await LoadProdutoAsync();
    }

    protected override async Task LoadDataAsync()
    {
        await LoadProdutoAsync();
    }

    [RelayCommand]
    private async Task EditProdutoAsync()
    {
        if (Produto == null) return;

        if (!CanEditProduto)
        {
            await _navigationService.DisplayAlertAsync("Acesso Negado", "Você não tem permissão para editar produtos", "OK");
            return;
        }

        await _navigationService.NavigateToAsync("produtos/editar", new Dictionary<string, object>
        {
            ["ProdutoId"] = Produto.Id,
            ["IsNew"] = false
        });
    }

    [RelayCommand]
    private async Task ShareProdutoAsync()
    {
        if (Produto == null) return;

        var shareText = $"*{Produto.Nome}*\n\n" +
                       $"Código: {Produto.Codigo}\n" +
                       $"Preço: {Produto.PrecoVenda:C2}\n" +
                       $"Estoque: {Produto.EstoqueAtual:N2} {Produto.Unidade}\n";

        if (!string.IsNullOrEmpty(Produto.Descricao))
            shareText += $"Descrição: {Produto.Descricao}\n";

        if (!string.IsNullOrEmpty(Produto.Categoria))
            shareText += $"Categoria: {Produto.Categoria}\n";

        try
        {
            await Microsoft.Maui.ApplicationModel.DataTransfer.Share.RequestAsync(new Microsoft.Maui.ApplicationModel.DataTransfer.ShareTextRequest
            {
                Text = shareText,
                Title = "Compartilhar Produto"
            });
        }
        catch (Exception ex)
        {
            await _navigationService.DisplayAlertAsync("Erro", $"Erro ao compartilhar produto: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task ViewImageAsync()
    {
        if (Produto == null || string.IsNullOrEmpty(Produto.ImagemUrl)) return;

        // Implementar visualização de imagem em tela cheia
        await _navigationService.DisplayAlertAsync("Imagem", "Funcionalidade de visualização de imagem será implementada em breve", "OK");
    }

    [RelayCommand]
    private async Task CopyCodigoAsync()
    {
        if (Produto == null) return;

        try
        {
            await Microsoft.Maui.ApplicationModel.DataTransfer.Clipboard.SetTextAsync(Produto.Codigo);
            await _navigationService.DisplayAlertAsync("Sucesso", "Código copiado para a área de transferência", "OK");
        }
        catch (Exception ex)
        {
            await _navigationService.DisplayAlertAsync("Erro", $"Erro ao copiar código: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task CopyCodigoBarrasAsync()
    {
        if (Produto == null || string.IsNullOrEmpty(Produto.CodigoBarras)) return;

        try
        {
            await Microsoft.Maui.ApplicationModel.DataTransfer.Clipboard.SetTextAsync(Produto.CodigoBarras);
            await _navigationService.DisplayAlertAsync("Sucesso", "Código de barras copiado para a área de transferência", "OK");
        }
        catch (Exception ex)
        {
            await _navigationService.DisplayAlertAsync("Erro", $"Erro ao copiar código de barras: {ex.Message}", "OK");
        }
    }

    private async Task LoadProdutoAsync()
    {
        if (ProdutoId <= 0) return;

        try
        {
            SetBusy(true);

            var produto = await _produtoService.GetByIdAsync(ProdutoId);
            if (produto == null)
            {
                await _navigationService.DisplayAlertAsync("Erro", "Produto não encontrado", "OK");
                await _navigationService.NavigateBackAsync();
                return;
            }

            Produto = produto;
            UpdateStatusEstoque();
            HasImage = !string.IsNullOrEmpty(produto.ImagemUrl);
        }
        catch (Exception ex)
        {
            await _navigationService.DisplayAlertAsync("Erro", $"Erro ao carregar produto: {ex.Message}", "OK");
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void UpdateStatusEstoque()
    {
        if (Produto == null) return;

        IsEstoqueBaixo = Produto.EstoqueBaixo;
        IsEstoqueDisponivel = Produto.EstoqueDisponivel;

        if (Produto.EstoqueAtual <= 0)
        {
            StatusEstoque = "SEM ESTOQUE";
            StatusEstoqueColor = "#F44336"; // Vermelho
        }
        else if (Produto.EstoqueBaixo)
        {
            StatusEstoque = "ESTOQUE BAIXO";
            StatusEstoqueColor = "#FF9800"; // Laranja
        }
        else
        {
            StatusEstoque = "DISPONÍVEL";
            StatusEstoqueColor = "#4CAF50"; // Verde
        }
    }

    private async Task CheckPermissionsAsync()
    {
        CanEditProduto = await _permissionService.HasPermissionAsync(OramaGo.Services.Permissions.ProdutosAlterar);
    }

    partial void OnProdutoChanged(ProdutoLocal? value)
    {
        if (value != null)
        {
            Title = value.Nome;
            UpdateStatusEstoque();
        }
    }
}