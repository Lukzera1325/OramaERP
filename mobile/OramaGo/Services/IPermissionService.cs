namespace OramaGo.Services;

public interface IPermissionService
{
    Task<bool> HasPermissionAsync(string permission);
    Task<bool> HasAnyPermissionAsync(params string[] permissions);
    Task<bool> HasAllPermissionsAsync(params string[] permissions);
    Task<bool> CanAccessOramaGoAsync();
    Task<List<string>> GetUserPermissionsAsync();
    Task RefreshPermissionsAsync();
}

public static class Permissions
{
    // Permissões de Vendas
    public const string VendasVisualizar = "Vendas.Visualizar";
    public const string VendasIncluir = "Vendas.Incluir";
    public const string VendasAlterar = "Vendas.Alterar";
    public const string VendasExcluir = "Vendas.Excluir";
    public const string VendasAprovar = "Vendas.Aprovar";
    public const string VendasFaturar = "Vendas.Faturar";
    public const string VendasConsultar = "Vendas.Consultar";

    // Permissões de Clientes
    public const string ClientesVisualizar = "Clientes.Visualizar";
    public const string ClientesIncluir = "Clientes.Incluir";
    public const string ClientesAlterar = "Clientes.Alterar";
    public const string ClientesExcluir = "Clientes.Excluir";

    // Permissões de Produtos
    public const string ProdutosVisualizar = "Produtos.Visualizar";
    public const string ProdutosIncluir = "Produtos.Incluir";
    public const string ProdutosAlterar = "Produtos.Alterar";
    public const string ProdutosExcluir = "Produtos.Excluir";

    // Permissões de Atividades
    public const string AtividadesVisualizar = "Atividades.Visualizar";
    public const string AtividadesIncluir = "Atividades.Incluir";
    public const string AtividadesAlterar = "Atividades.Alterar";
    public const string AtividadesExcluir = "Atividades.Excluir";

    // Permissão especial para Órama Go
    public const string OramaGoAcesso = "OramaGo.Acesso";
}