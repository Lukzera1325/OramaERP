using Orama.Domain.Entities;

namespace Orama.Application.Services;

/// <summary>
/// Interface para gerenciar estruturas de produto (BOM)
/// </summary>
public interface IEstruturaProdutoService
{
    // Consultas
    Task<IEnumerable<EstruturaProduto>> ObterEstruturaPorProdutoAsync(int produtoId, int empresaId);
    Task<EstruturaProduto?> ObterPorIdAsync(int id, int empresaId);
    Task<IEnumerable<EstruturaProduto>> ObterTodosAsync(int empresaId);

    // Operações
    Task<EstruturaProduto> CriarAsync(EstruturaProduto estrutura);
    Task<EstruturaProduto> AtualizarAsync(EstruturaProduto estrutura);
    Task<bool> ExcluirAsync(int id, int empresaId);

    // Validações
    Task<bool> ProdutoTemEstruturaAsync(int produtoId, int empresaId);
    Task<(bool Valida, List<string> Erros)> ValidarEstruturaAsync(int produtoId, int empresaId);
}