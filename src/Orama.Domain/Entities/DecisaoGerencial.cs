using System.ComponentModel.DataAnnotations;

namespace Orama.Domain.Entities
{
    public class DecisaoGerencial
    {
        public int Id { get; set; }
        public int EmpresaId { get; set; }
        
        // Referência ao que gerou a decisão
        public TipoReferenciaDecisao TipoReferencia { get; set; }
        public int ReferenciaId { get; set; } // ID do Produto, Venda, Alerta ou Sugestão
        public string ReferenciaDescricao { get; set; } = string.Empty;
        
        // Contexto da decisão
        public string ProblemaIdentificado { get; set; } = string.Empty;
        public string AcaoTomada { get; set; } = string.Empty;
        public string Observacoes { get; set; } = string.Empty;
        
        // Resultado (opcional)
        public string? ResultadoObtido { get; set; }
        public bool? FoiEfetiva { get; set; }
        
        // Controle
        public DateTime DataDecisao { get; set; }
        public int UsuarioId { get; set; }
        
        // Relacionamentos
        public Empresa Empresa { get; set; } = null!;
        public Usuario Usuario { get; set; } = null!;
        
        // Métodos de criação
        public static DecisaoGerencial CriarDecisaoProduto(int empresaId, int usuarioId, int produtoId, string produtoDescricao, string problema, string acao, string observacoes = "")
        {
            return new DecisaoGerencial
            {
                EmpresaId = empresaId,
                UsuarioId = usuarioId,
                TipoReferencia = TipoReferenciaDecisao.Produto,
                ReferenciaId = produtoId,
                ReferenciaDescricao = $"Produto: {produtoDescricao}",
                ProblemaIdentificado = problema,
                AcaoTomada = acao,
                Observacoes = observacoes,
                DataDecisao = DateTime.Now
            };
        }
        
        public static DecisaoGerencial CriarDecisaoVenda(int empresaId, int usuarioId, int vendaId, string vendaNumero, string problema, string acao, string observacoes = "")
        {
            return new DecisaoGerencial
            {
                EmpresaId = empresaId,
                UsuarioId = usuarioId,
                TipoReferencia = TipoReferenciaDecisao.Venda,
                ReferenciaId = vendaId,
                ReferenciaDescricao = $"Venda: {vendaNumero}",
                ProblemaIdentificado = problema,
                AcaoTomada = acao,
                Observacoes = observacoes,
                DataDecisao = DateTime.Now
            };
        }
        
        public static DecisaoGerencial CriarDecisaoSugestao(int empresaId, int usuarioId, int sugestaoId, string sugestaoDescricao, string acao, string observacoes = "")
        {
            return new DecisaoGerencial
            {
                EmpresaId = empresaId,
                UsuarioId = usuarioId,
                TipoReferencia = TipoReferenciaDecisao.Sugestao,
                ReferenciaId = sugestaoId,
                ReferenciaDescricao = $"Sugestão: {sugestaoDescricao}",
                ProblemaIdentificado = "Sugestão de ação do sistema",
                AcaoTomada = acao,
                Observacoes = observacoes,
                DataDecisao = DateTime.Now
            };
        }
        
        public void RegistrarResultado(string resultado, bool foiEfetiva)
        {
            ResultadoObtido = resultado;
            FoiEfetiva = foiEfetiva;
        }
        
        // Propriedades calculadas
        public string TipoReferenciaDescricao => TipoReferencia switch
        {
            TipoReferenciaDecisao.Produto => "Produto",
            TipoReferenciaDecisao.Venda => "Venda",
            TipoReferenciaDecisao.Alerta => "Alerta",
            TipoReferenciaDecisao.Sugestao => "Sugestão",
            _ => "Desconhecido"
        };
        
        public string StatusEfetividade => FoiEfetiva switch
        {
            true => "Efetiva",
            false => "Não efetiva",
            null => "Não avaliada"
        };
        
        public string CssClassEfetividade => FoiEfetiva switch
        {
            true => "text-success",
            false => "text-danger",
            null => "text-muted"
        };
        
        public int DiasDesdeDecisao => (DateTime.Now - DataDecisao).Days;
    }
    
    public enum TipoReferenciaDecisao
    {
        Produto = 1,
        Venda = 2,
        Alerta = 3,
        Sugestao = 4
    }
}