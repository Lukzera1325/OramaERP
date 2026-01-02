using System.ComponentModel.DataAnnotations;

namespace Orama.Domain.Entities.Fiscal
{
    /// <summary>
    /// Configuração Fiscal da Empresa - Autoridade máxima do ambiente fiscal
    /// Isolada do core, versionada por vigência
    /// </summary>
    public class EmpresaFiscalConfig
    {
        public int Id { get; set; }
        
        /// <summary>
        /// Referência à empresa do core (sem contaminar)
        /// </summary>
        public int EmpresaId { get; set; }
        
        /// <summary>
        /// Regime Tributário da empresa
        /// </summary>
        [Required]
        public RegimeTributario RegimeTributario { get; set; }
        
        /// <summary>
        /// Código de Regime Tributário para NF-e
        /// </summary>
        [Required]
        public CRT CRT { get; set; }
        
        /// <summary>
        /// UF da empresa para regras fiscais
        /// </summary>
        [Required]
        [StringLength(2)]
        public string UF { get; set; } = string.Empty;
        
        /// <summary>
        /// Ambiente fiscal explícito (nunca inferir do ambiente da aplicação)
        /// </summary>
        [Required]
        public AmbienteFiscal AmbienteFiscal { get; set; }
        
        /// <summary>
        /// Versão fiscal para controle de mudanças (ex: "2026.1")
        /// </summary>
        [Required]
        [StringLength(20)]
        public string VersaoFiscal { get; set; } = string.Empty;
        
        /// <summary>
        /// Referência ao certificado digital (sem implementar agora)
        /// </summary>
        public int? CertificadoId { get; set; }
        
        /// <summary>
        /// Controle de vigência - início
        /// </summary>
        [Required]
        public DateTime VigenteDe { get; set; }
        
        /// <summary>
        /// Controle de vigência - fim (null = vigente indefinidamente)
        /// </summary>
        public DateTime? VigenteAte { get; set; }
        
        /// <summary>
        /// Auditoria
        /// </summary>
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
        public int CriadoPor { get; set; }
        
        /// <summary>
        /// Verifica se a configuração está vigente em uma data específica
        /// </summary>
        public bool EstaVigente(DateTime data)
        {
            return data >= VigenteDe && (VigenteAte == null || data <= VigenteAte);
        }
    }
    
    /// <summary>
    /// Regime Tributário da empresa
    /// </summary>
    public enum RegimeTributario
    {
        SimplesNacional = 1,
        RegimeNormal = 2
    }
    
    /// <summary>
    /// Código de Regime Tributário para NF-e
    /// </summary>
    public enum CRT
    {
        /// <summary>
        /// Simples Nacional
        /// </summary>
        SimplesNacional = 1,
        
        /// <summary>
        /// Simples Nacional - Excesso de sublimite de receita bruta
        /// </summary>
        SimplesNacionalExcesso = 2,
        
        /// <summary>
        /// Regime Normal - Lucro Presumido
        /// </summary>
        LucroPresumido = 3
    }
    
    /// <summary>
    /// Ambiente fiscal explícito
    /// </summary>
    public enum AmbienteFiscal
    {
        Homologacao = 2,
        Producao = 1
    }
}