using System.ComponentModel.DataAnnotations;

namespace Orama.Domain.Entities
{
    public class ApontamentoHoras
    {
        public int Id { get; set; }
        
        public int OrdemProducaoId { get; set; }
        public virtual OrdemProducao OrdemProducao { get; set; } = null!;
        
        public int? EtapaId { get; set; }
        public virtual OrdemProducaoEtapa? Etapa { get; set; }
        
        public int FuncionarioId { get; set; }
        public virtual Usuario Funcionario { get; set; } = null!;
        
        [Required]
        public DateTime DataInicio { get; set; }
        
        public DateTime? DataFim { get; set; }
        
        public decimal HorasTrabalhadas { get; set; }
        
        public decimal ValorHora { get; set; }
        
        public decimal CustoTotal => HorasTrabalhadas * ValorHora;
        
        public TipoApontamento Tipo { get; set; }
        
        [StringLength(500)]
        public string? Observacoes { get; set; }
        
        public DateTime DataCriacao { get; set; }
        
        public int UsuarioCriacaoId { get; set; }
        public virtual Usuario UsuarioCriacao { get; set; } = null!;
    }
    
    public enum TipoApontamento
    {
        Producao = 1,
        Setup = 2,
        Manutencao = 3,
        Parada = 4,
        Qualidade = 5
    }
}