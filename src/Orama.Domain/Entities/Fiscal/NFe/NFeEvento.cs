using System.ComponentModel.DataAnnotations;

namespace Orama.Domain.Entities.Fiscal.NFe
{
    /// <summary>
    /// Evento da NF-e (cancelamento, consulta, etc.)
    /// </summary>
    public class NFeEvento
    {
        public int Id { get; set; }
        
        /// <summary>
        /// Referência ao documento NF-e
        /// </summary>
        public int NFeDocumentoId { get; set; }
        public virtual NFeDocumento NFeDocumento { get; set; } = null!;
        
        /// <summary>
        /// Tipo do evento
        /// </summary>
        [Required]
        public TipoEventoNFe TipoEvento { get; set; }
        
        /// <summary>
        /// Número sequencial do evento
        /// </summary>
        [Required]
        public int NumeroSequencial { get; set; }
        
        /// <summary>
        /// Descrição do evento
        /// </summary>
        [Required]
        [StringLength(200)]
        public string Descricao { get; set; } = string.Empty;
        
        /// <summary>
        /// Justificativa (obrigatória para cancelamento)
        /// </summary>
        [StringLength(255)]
        public string? Justificativa { get; set; }
        
        /// <summary>
        /// XML do evento enviado
        /// </summary>
        public string? XmlEvento { get; set; }
        
        /// <summary>
        /// XML de retorno da SEFAZ
        /// </summary>
        public string? XmlRetorno { get; set; }
        
        /// <summary>
        /// Protocolo do evento
        /// </summary>
        [StringLength(50)]
        public string? Protocolo { get; set; }
        
        /// <summary>
        /// Status do evento
        /// </summary>
        [Required]
        public StatusEventoNFe Status { get; set; }
        
        /// <summary>
        /// Código de status da SEFAZ
        /// </summary>
        public int? CodigoStatusSefaz { get; set; }
        
        /// <summary>
        /// Mensagem de retorno da SEFAZ
        /// </summary>
        public string? MensagemSefaz { get; set; }
        
        /// <summary>
        /// Data/hora do evento
        /// </summary>
        [Required]
        public DateTime DataEvento { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Usuário que executou o evento
        /// </summary>
        public int ExecutadoPor { get; set; }
        
        /// <summary>
        /// Cria um evento de cancelamento
        /// </summary>
        public static NFeEvento CriarCancelamento(int nfeDocumentoId, string justificativa, int usuarioId)
        {
            if (string.IsNullOrWhiteSpace(justificativa) || justificativa.Length < 15)
                throw new ArgumentException("Justificativa deve ter pelo menos 15 caracteres");
            
            return new NFeEvento
            {
                NFeDocumentoId = nfeDocumentoId,
                TipoEvento = TipoEventoNFe.Cancelamento,
                NumeroSequencial = 1,
                Descricao = "Cancelamento de NF-e",
                Justificativa = justificativa,
                Status = StatusEventoNFe.Pendente,
                ExecutadoPor = usuarioId
            };
        }
        
        /// <summary>
        /// Cria um evento de consulta
        /// </summary>
        public static NFeEvento CriarConsulta(int nfeDocumentoId, int usuarioId)
        {
            return new NFeEvento
            {
                NFeDocumentoId = nfeDocumentoId,
                TipoEvento = TipoEventoNFe.Consulta,
                NumeroSequencial = 1,
                Descricao = "Consulta de situação da NF-e",
                Status = StatusEventoNFe.Pendente,
                ExecutadoPor = usuarioId
            };
        }
    }
    
    /// <summary>
    /// Tipos de evento da NF-e
    /// </summary>
    public enum TipoEventoNFe
    {
        Consulta = 1,
        Cancelamento = 2,
        CartaCorrecao = 3,
        Manifestacao = 4
    }
    
    /// <summary>
    /// Status do evento
    /// </summary>
    public enum StatusEventoNFe
    {
        Pendente = 1,
        Enviado = 2,
        Processado = 3,
        Rejeitado = 4,
        Erro = 5
    }
}