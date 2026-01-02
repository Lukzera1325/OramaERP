using System.ComponentModel.DataAnnotations;

namespace OramaGo.Models;

public abstract class BaseLocalModel
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// ID da empresa para isolamento multi-tenant
    /// </summary>
    public int EmpresaId { get; set; }

    /// <summary>
    /// Data de criação do registro
    /// </summary>
    public DateTime DataCriacao { get; set; } = DateTime.Now;

    /// <summary>
    /// Data da última modificação
    /// </summary>
    public DateTime DataModificacao { get; set; } = DateTime.Now;

    /// <summary>
    /// Indica se o registro está ativo
    /// </summary>
    public bool Ativo { get; set; } = true;

    /// <summary>
    /// Status de sincronização com o servidor
    /// </summary>
    public SyncStatus StatusSync { get; set; } = SyncStatus.Pending;

    /// <summary>
    /// Data da última sincronização
    /// </summary>
    public DateTime? DataUltimaSync { get; set; }

    /// <summary>
    /// ID do registro no servidor (pode ser diferente do ID local)
    /// </summary>
    public int? ServidorId { get; set; }

    /// <summary>
    /// Hash dos dados para detecção de conflitos
    /// </summary>
    public string? HashDados { get; set; }
}

public enum SyncStatus
{
    /// <summary>
    /// Registro criado localmente, aguardando sincronização
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Registro sincronizado com sucesso
    /// </summary>
    Synced = 1,

    /// <summary>
    /// Registro modificado localmente após sincronização
    /// </summary>
    Modified = 2,

    /// <summary>
    /// Conflito detectado durante sincronização
    /// </summary>
    Conflict = 3,

    /// <summary>
    /// Erro durante sincronização
    /// </summary>
    Error = 4
}