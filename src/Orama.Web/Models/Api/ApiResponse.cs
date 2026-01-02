namespace Orama.Web.Models.Api;

/// <summary>
/// Resposta padrão da API
/// </summary>
/// <typeparam name="T">Tipo dos dados retornados</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// Indica se a operação foi bem-sucedida
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Mensagem descritiva do resultado
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Dados retornados pela API
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Lista de erros, se houver
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// Timestamp da resposta
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Resposta paginada da API
/// </summary>
/// <typeparam name="T">Tipo dos dados retornados</typeparam>
public class PagedApiResponse<T> : ApiResponse<List<T>>
{
    /// <summary>
    /// Página atual (baseada em 1)
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Tamanho da página
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total de registros
    /// </summary>
    public int TotalRecords { get; set; }

    /// <summary>
    /// Total de páginas
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// Indica se há página anterior
    /// </summary>
    public bool HasPreviousPage => Page > 1;

    /// <summary>
    /// Indica se há próxima página
    /// </summary>
    public bool HasNextPage => Page < TotalPages;
}