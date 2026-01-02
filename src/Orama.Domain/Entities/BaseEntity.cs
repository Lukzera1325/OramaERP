namespace Orama.Domain.Entities;

/// <summary>
/// Classe base para todas as entidades do sistema
/// Contém propriedades comuns como Id, datas de criação e atualização
/// </summary>
public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    public DateTime? DataAtualizacao { get; set; }
    public DateTime DataModificacao => DataAtualizacao ?? DataCriacao;
    public bool Ativo { get; set; } = true;
    
    /// <summary>
    /// Método para marcar a entidade como atualizada
    /// </summary>
    public void MarcarComoAtualizada()
    {
        DataAtualizacao = DateTime.UtcNow;
    }
}