using System.ComponentModel.DataAnnotations;

namespace Orama.Domain.Entities
{
    public class SugestaoAcao
    {
        public int Id { get; set; }
        public int EmpresaId { get; set; }
        
        // Referência ao problema
        public TipoReferenciaSugestao TipoReferencia { get; set; }
        public int ReferenciaId { get; set; } // ID do Produto, Venda ou Alerta
        public string ReferenciaDescricao { get; set; } = string.Empty; // Ex: "Produto: Martelo Pro"
        
        // Problema identificado
        public TipoProblema TipoProblema { get; set; }
        public string DescricaoProblema { get; set; } = string.Empty;
        
        // Sugestão de ação
        public TipoAcaoSugerida AcaoSugerida { get; set; }
        public string TextoSugestao { get; set; } = string.Empty;
        public string Justificativa { get; set; } = string.Empty;
        
        // Prioridade e status
        public PrioridadeSugestao Prioridade { get; set; }
        public StatusSugestao Status { get; set; }
        
        // Controle
        public DateTime DataCriacao { get; set; }
        public DateTime? DataResolucao { get; set; }
        public int? UsuarioResolucaoId { get; set; }
        public string? ObservacaoResolucao { get; set; }
        
        // Relacionamentos
        public Empresa Empresa { get; set; } = null!;
        public Usuario? UsuarioResolucao { get; set; }
        
        // Métodos de negócio
        public static SugestaoAcao CriarSugestaoPrecoAbaixoCusto(int empresaId, int produtoId, string produtoDescricao, decimal precoAtual, decimal custoAtual)
        {
            var diferenca = custoAtual - precoAtual;
            var percentual = precoAtual > 0 ? (diferenca / precoAtual) * 100 : 0;
            
            return new SugestaoAcao
            {
                EmpresaId = empresaId,
                TipoReferencia = TipoReferenciaSugestao.Produto,
                ReferenciaId = produtoId,
                ReferenciaDescricao = $"Produto: {produtoDescricao}",
                TipoProblema = TipoProblema.PrecoAbaixoCusto,
                DescricaoProblema = $"Preço de venda (R$ {precoAtual:F2}) está R$ {diferenca:F2} abaixo do custo (R$ {custoAtual:F2})",
                AcaoSugerida = TipoAcaoSugerida.RevisarPrecoVenda,
                TextoSugestao = $"Revisar preço de venda para pelo menos R$ {custoAtual * 1.1m:F2} (custo + 10% margem mínima)",
                Justificativa = $"Produto está sendo vendido com prejuízo de {percentual:F1}%. Recomenda-se ajustar preço para garantir margem mínima.",
                Prioridade = diferenca > (custoAtual * 0.2m) ? PrioridadeSugestao.Alta : PrioridadeSugestao.Media,
                Status = StatusSugestao.Pendente,
                DataCriacao = DateTime.Now
            };
        }
        
        public static SugestaoAcao CriarSugestaoMargemBaixa(int empresaId, int produtoId, string produtoDescricao, decimal margem)
        {
            return new SugestaoAcao
            {
                EmpresaId = empresaId,
                TipoReferencia = TipoReferenciaSugestao.Produto,
                ReferenciaId = produtoId,
                ReferenciaDescricao = $"Produto: {produtoDescricao}",
                TipoProblema = TipoProblema.MargemBaixa,
                DescricaoProblema = $"Margem de {margem:F1}% está abaixo do recomendado (10%)",
                AcaoSugerida = TipoAcaoSugerida.AvaliarPrecoOuCusto,
                TextoSugestao = "Avaliar aumento de preço ou negociar redução de custo com fornecedor",
                Justificativa = "Margem baixa pode não cobrir despesas operacionais e impostos",
                Prioridade = margem < 5 ? PrioridadeSugestao.Media : PrioridadeSugestao.Baixa,
                Status = StatusSugestao.Pendente,
                DataCriacao = DateTime.Now
            };
        }
        
        public static SugestaoAcao CriarSugestaoVendaRecorrentePrejuizo(int empresaId, int produtoId, string produtoDescricao, int quantidadeVendas)
        {
            return new SugestaoAcao
            {
                EmpresaId = empresaId,
                TipoReferencia = TipoReferenciaSugestao.Produto,
                ReferenciaId = produtoId,
                ReferenciaDescricao = $"Produto: {produtoDescricao}",
                TipoProblema = TipoProblema.PrejuizoRecorrente,
                DescricaoProblema = $"Produto teve {quantidadeVendas} vendas com prejuízo nos últimos 30 dias",
                AcaoSugerida = TipoAcaoSugerida.SuspenderVendaTemporariamente,
                TextoSugestao = "Suspender vendas temporariamente até revisar preços e custos",
                Justificativa = "Prejuízo recorrente indica problema estrutural que precisa ser corrigido",
                Prioridade = PrioridadeSugestao.Alta,
                Status = StatusSugestao.Pendente,
                DataCriacao = DateTime.Now
            };
        }
        
        public void Resolver(int usuarioId, string observacao)
        {
            Status = StatusSugestao.Resolvida;
            DataResolucao = DateTime.Now;
            UsuarioResolucaoId = usuarioId;
            ObservacaoResolucao = observacao;
        }
        
        public void Descartar(int usuarioId, string motivo)
        {
            Status = StatusSugestao.Descartada;
            DataResolucao = DateTime.Now;
            UsuarioResolucaoId = usuarioId;
            ObservacaoResolucao = $"Descartada: {motivo}";
        }
        
        // Propriedades calculadas
        public string StatusDescricao => Status switch
        {
            StatusSugestao.Pendente => "Pendente",
            StatusSugestao.Resolvida => "Resolvida",
            StatusSugestao.Descartada => "Descartada",
            _ => "Desconhecido"
        };
        
        public string PrioridadeDescricao => Prioridade switch
        {
            PrioridadeSugestao.Alta => "Alta",
            PrioridadeSugestao.Media => "Média",
            PrioridadeSugestao.Baixa => "Baixa",
            _ => "Normal"
        };
        
        public string CssClassPrioridade => Prioridade switch
        {
            PrioridadeSugestao.Alta => "table-danger",
            PrioridadeSugestao.Media => "table-warning",
            PrioridadeSugestao.Baixa => "table-info",
            _ => ""
        };
        
        public string IconePrioridade => Prioridade switch
        {
            PrioridadeSugestao.Alta => "fas fa-exclamation-triangle text-danger",
            PrioridadeSugestao.Media => "fas fa-exclamation-circle text-warning",
            PrioridadeSugestao.Baixa => "fas fa-info-circle text-info",
            _ => "fas fa-circle"
        };
        
        public int DiasDesdeCreacao => (DateTime.Now - DataCriacao).Days;
    }
    
    public enum TipoReferenciaSugestao
    {
        Produto = 1,
        Venda = 2,
        Alerta = 3
    }
    
    public enum TipoProblema
    {
        PrecoAbaixoCusto = 1,
        MargemBaixa = 2,
        PrejuizoRecorrente = 3,
        CustoElevado = 4,
        DescontoExcessivo = 5
    }
    
    public enum TipoAcaoSugerida
    {
        RevisarPrecoVenda = 1,
        NegociarCustoFornecedor = 2,
        AvaliarPrecoOuCusto = 3,
        SuspenderVendaTemporariamente = 4,
        ReavaliarEstruturaProduto = 5,
        LimitarDesconto = 6
    }
    
    public enum PrioridadeSugestao
    {
        Baixa = 1,
        Media = 2,
        Alta = 3
    }
    
    public enum StatusSugestao
    {
        Pendente = 1,
        Resolvida = 2,
        Descartada = 3
    }
}