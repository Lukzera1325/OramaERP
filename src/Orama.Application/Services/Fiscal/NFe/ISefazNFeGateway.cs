using Orama.Domain.Entities.Fiscal;
using Orama.Domain.Entities.Fiscal.NFe;

namespace Orama.Application.Services.Fiscal.NFe
{
    /// <summary>
    /// Interface para comunicação com SEFAZ
    /// </summary>
    public interface ISefazNFeGateway
    {
        /// <summary>
        /// Gera XML da NF-e
        /// </summary>
        Task<string> GerarXmlAsync(NFeDocumento nfe);
        
        /// <summary>
        /// Assina XML com certificado digital
        /// </summary>
        Task<string> AssinarXmlAsync(string xml, AmbienteFiscal ambiente);
        
        /// <summary>
        /// Envia NF-e para SEFAZ
        /// </summary>
        Task<ResultadoEnvioNFe> EnviarNFeAsync(string xmlAssinado, AmbienteFiscal ambiente);
        
        /// <summary>
        /// Consulta situação da NF-e
        /// </summary>
        Task<ResultadoConsultaNFe> ConsultarNFeAsync(string chaveAcesso, AmbienteFiscal ambiente);
        
        /// <summary>
        /// Cancela NF-e
        /// </summary>
        Task<ResultadoEventoNFe> CancelarNFeAsync(string chaveAcesso, string justificativa, AmbienteFiscal ambiente);
        
        /// <summary>
        /// Verifica se o serviço SEFAZ está disponível
        /// </summary>
        Task<bool> VerificarStatusServicoAsync(AmbienteFiscal ambiente, string uf);
    }
    
    /// <summary>
    /// Resultado do envio de NF-e
    /// </summary>
    public class ResultadoEnvioNFe
    {
        public bool Autorizada { get; set; }
        public string? Protocolo { get; set; }
        public int CodigoStatus { get; set; }
        public string Mensagem { get; set; } = string.Empty;
        public string? XmlRetorno { get; set; }
        public DateTime DataProcessamento { get; set; } = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Resultado da consulta de NF-e
    /// </summary>
    public class ResultadoConsultaNFe
    {
        public bool Encontrada { get; set; }
        public int CodigoStatus { get; set; }
        public string Mensagem { get; set; } = string.Empty;
        public string? Protocolo { get; set; }
        public DateTime? DataAutorizacao { get; set; }
        public string? XmlRetorno { get; set; }
    }
    
    /// <summary>
    /// Resultado de evento (cancelamento, etc.)
    /// </summary>
    public class ResultadoEventoNFe
    {
        public bool Sucesso { get; set; }
        public string? Protocolo { get; set; }
        public int CodigoStatus { get; set; }
        public string Mensagem { get; set; } = string.Empty;
        public string? XmlRetorno { get; set; }
        public DateTime DataProcessamento { get; set; } = DateTime.UtcNow;
    }
}