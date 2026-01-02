using Orama.Domain.Entities;

namespace Orama.Application.Services;

/// <summary>
/// Interface para serviços de produto
/// </summary>
public interface IProdutoService
{
    /// <summary>
    /// Obtém todos os produtos ativos
    /// </summary>
    Task<IEnumerable<Produto>> ObterTodosAsync();
    
    /// <summary>
    /// Obtém todos os produtos ativos por empresa
    /// </summary>
    Task<IEnumerable<Produto>> ObterTodosAsync(int empresaId);
    
    /// <summary>
    /// Obtém um produto por ID
    /// </summary>
    Task<Produto?> ObterPorIdAsync(int id);
    
    /// <summary>
    /// Obtém um produto por ID e empresa
    /// </summary>
    Task<Produto?> ObterPorIdAsync(int id, int empresaId);
    
    /// <summary>
    /// Obtém um produto por código
    /// </summary>
    Task<Produto?> ObterPorCodigoAsync(string codigo);
    
    /// <summary>
    /// Cria um novo produto
    /// </summary>
    Task<Produto> CriarAsync(Produto produto);
    
    /// <summary>
    /// Atualiza um produto existente
    /// </summary>
    Task<Produto> AtualizarAsync(Produto produto);
    
    /// <summary>
    /// Exclui um produto (soft delete)
    /// </summary>
    Task ExcluirAsync(int id);
    
    /// <summary>
    /// Verifica se um código já está em uso
    /// </summary>
    Task<bool> CodigoExisteAsync(string codigo, int? produtoId = null);
    
    /// <summary>
    /// Busca produtos por código ou descrição
    /// </summary>
    Task<IEnumerable<Produto>> BuscarAsync(string termo);
    
    /// <summary>
    /// Obtém produtos com estoque baixo
    /// </summary>
    Task<IEnumerable<Produto>> ObterProdutosEstoqueBaixoAsync();
    
    /// <summary>
    /// Atualiza estoque de um produto
    /// </summary>
    Task AtualizarEstoqueAsync(int produtoId, decimal quantidade, string tipoMovimento);
}

/// <summary>
/// Interface para serviços de categoria
/// </summary>
public interface ICategoriaService
{
    /// <summary>
    /// Obtém todas as categorias ativas
    /// </summary>
    Task<IEnumerable<Categoria>> ObterTodosAsync();
    
    /// <summary>
    /// Obtém uma categoria por ID
    /// </summary>
    Task<Categoria?> ObterPorIdAsync(int id);
    
    /// <summary>
    /// Cria uma nova categoria
    /// </summary>
    Task<Categoria> CriarAsync(Categoria categoria);
    
    /// <summary>
    /// Atualiza uma categoria existente
    /// </summary>
    Task<Categoria> AtualizarAsync(Categoria categoria);
    
    /// <summary>
    /// Exclui uma categoria (soft delete)
    /// </summary>
    Task ExcluirAsync(int id);
}