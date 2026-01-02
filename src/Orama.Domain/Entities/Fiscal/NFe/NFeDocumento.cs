using System.ComponentModel.DataAnnotations;
using Orama.Domain.Entities.Fiscal;

namespace Orama.Domain.Entities.Fiscal.NFe
{
    /// <summary>
    /// Documento NF-e - Isolado do core, apenas referencia Venda por ID
    /// </summary>
    public class NFeDocumento
    {
        public int Id { get; set; }
        
        /// <summary>
        /// Referência à venda (SEM navegação - isolamento)
        /// </summary>
        public int VendaId { get; set; }
        
        /// <summary>
        /// Número da NF-e
        /// </summary>
        [Required]
        public int Numero { get; set; }
        
        /// <summary>
        /// Série da NF-e
        /// </summary>
        [Required]
        public int Serie { get; set; }
        
        /// <summary>
        /// Chave de acesso da NF-e (44 dígitos)
        /// </summary>
        [StringLength(44)]
        public string? ChaveAcesso { get; set; }
        
        /// <summary>
        /// Ambiente fiscal usado na emissão
        /// </summary>
        [Required]
        public AmbienteFiscal AmbienteFiscal { get; set; }
        
        /// <summary>
        /// Status atual da NF-e
        /// </summary>
        [Required]
        public NFeStatus Status { get; set; }
        
        /// <summary>
        /// XML gerado (antes da assinatura)
        /// </summary>
        public string? XmlGerado { get; set; }
        
        /// <summary>
        /// XML assinado (após assinatura digital)
        /// </summary>
        public string? XmlAssinado { get; set; }
        
        /// <summary>
        /// Protocolo de autorização da SEFAZ
        /// </summary>
        [StringLength(50)]
        public string? ProtocoloAutorizacao { get; set; }
        
        /// <summary>
        /// Data de emissão da NF-e
        /// </summary>
        [Required]
        public DateTime DataEmissao { get; set; }
        
        /// <summary>
        /// Snapshot fiscal usado na emissão (para auditoria)
        /// JSON com todas as configurações fiscais aplicadas
        /// </summary>
        public string? SnapshotFiscal { get; set; }
        
        /// <summary>
        /// Mensagem de retorno da SEFAZ (sucesso ou erro)
        /// </summary>
        public string? MensagemSefaz { get; set; }
        
        /// <summary>
        /// Código de status da SEFAZ
        /// </summary>
        public int? CodigoStatusSefaz { get; set; }
        
        /// <summary>
        /// Data da última consulta de status
        /// </summary>
        public DateTime? UltimaConsulta { get; set; }
        
        /// <summary>
        /// Auditoria
        /// </summary>
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
        public int CriadoPor { get; set; }
        public DateTime? AlteradoEm { get; set; }
        public int? AlteradoPor { get; set; }
        
        /// <summary>
        /// Itens da NF-e
        /// </summary>
        public virtual ICollection<NFeItem> Itens { get; set; } = new List<NFeItem>();
        
        /// <summary>
        /// Eventos da NF-e (cancelamento, consultas, etc.)
        /// </summary>
        public virtual ICollection<NFeEvento> Eventos { get; set; } = new List<NFeEvento>();
        
        /// <summary>
        /// Verifica se a NF-e pode ser cancelada
        /// </summary>
        public bool PodeCancelar()
        {
            return Status == NFeStatus.Autorizada && 
                   DataEmissao >= DateTime.Today.AddDays(-1); // 24h para cancelar
        }
        
        /// <summary>
        /// Verifica se a NF-e pode ser consultada
        /// </summary>
        public bool PodeConsultar()
        {
            return !string.IsNullOrEmpty(ChaveAcesso) && 
                   Status != NFeStatus.Rascunho;
        }
        
        /// <summary>
        /// Gera a chave de acesso da NF-e
        /// </summary>
        public void GerarChaveAcesso(string ufCodigo, string cnpj, string modelo = "55")
        {
            if (Numero <= 0 || Serie <= 0)
                throw new InvalidOperationException("Número e série devem estar definidos");
            
            var dataEmissao = DataEmissao.ToString("yyMM");
            var cnpjLimpo = cnpj.Replace(".", "").Replace("/", "").Replace("-", "");
            var numeroFormatado = Numero.ToString("D9");
            var serieFormatada = Serie.ToString("D3");
            var codigoNumerico = new Random().Next(10000000, 99999999).ToString();
            
            var chaveBase = $"{ufCodigo}{dataEmissao}{cnpjLimpo}{modelo}{serieFormatada}{numeroFormatado}1{codigoNumerico}";
            var digitoVerificador = CalcularDigitoVerificador(chaveBase);
            
            ChaveAcesso = chaveBase + digitoVerificador;
        }
        
        /// <summary>
        /// Calcula o dígito verificador da chave de acesso
        /// </summary>
        private static int CalcularDigitoVerificador(string chave)
        {
            var sequencia = "4329876543298765432987654329876543298765432";
            var soma = 0;
            
            for (int i = 0; i < chave.Length; i++)
            {
                soma += int.Parse(chave[i].ToString()) * int.Parse(sequencia[i].ToString());
            }
            
            var resto = soma % 11;
            return resto < 2 ? 0 : 11 - resto;
        }
    }
    
    /// <summary>
    /// Status da NF-e
    /// </summary>
    public enum NFeStatus
    {
        Rascunho = 1,
        Gerada = 2,
        Assinada = 3,
        Enviada = 4,
        Autorizada = 5,
        Rejeitada = 6,
        Cancelada = 7,
        Denegada = 8
    }
}