namespace Orama.Application.Services
{
    public interface ISimulacaoService
    {
        // Simulação de preço
        Task<SimulacaoPrecoDto> SimularNovoPrecoAsync(int produtoId, decimal novoPreco);
        Task<SimulacaoPrecoDto> SimularNovoCustoAsync(int produtoId, decimal novoCusto);
        Task<SimulacaoVendaDto> SimularVendaAsync(int produtoId, decimal quantidade, decimal precoUnitario);
        
        // Simulação de cenários
        Task<List<SimulacaoPrecoDto>> SimularCenariosProdutoAsync(int produtoId);
        Task<SimulacaoImpactoDto> SimularImpactoMargemAsync(int produtoId, decimal novoPreco, decimal novoCusto);
    }
    
    public class SimulacaoPrecoDto
    {
        public int ProdutoId { get; set; }
        public string ProdutoDescricao { get; set; } = string.Empty;
        
        // Valores atuais
        public decimal PrecoAtual { get; set; }
        public decimal CustoAtual { get; set; }
        public decimal MargemAtual { get; set; }
        public decimal LucroUnitarioAtual { get; set; }
        
        // Valores simulados
        public decimal PrecoSimulado { get; set; }
        public decimal CustoSimulado { get; set; }
        public decimal MargemSimulada { get; set; }
        public decimal LucroUnitarioSimulado { get; set; }
        
        // Comparação
        public decimal DiferencaMargem { get; set; }
        public decimal DiferencaLucro { get; set; }
        public string StatusSimulacao { get; set; } = string.Empty;
        public string Recomendacao { get; set; } = string.Empty;
        
        // Cenário
        public string TipoSimulacao { get; set; } = string.Empty; // "Preço" ou "Custo"
        public DateTime DataSimulacao { get; set; }
    }
    
    public class SimulacaoVendaDto
    {
        public int ProdutoId { get; set; }
        public string ProdutoDescricao { get; set; } = string.Empty;
        public decimal Quantidade { get; set; }
        public decimal PrecoUnitario { get; set; }
        public decimal CustoUnitario { get; set; }
        
        // Resultados
        public decimal ValorTotalVenda { get; set; }
        public decimal CustoTotalVenda { get; set; }
        public decimal LucroTotalVenda { get; set; }
        public decimal MargemPercentual { get; set; }
        
        // Análise
        public string StatusVenda { get; set; } = string.Empty;
        public string Recomendacao { get; set; } = string.Empty;
        public DateTime DataSimulacao { get; set; }
    }
    
    public class SimulacaoImpactoDto
    {
        public int ProdutoId { get; set; }
        public string ProdutoDescricao { get; set; } = string.Empty;
        
        // Cenário atual
        public decimal PrecoAtual { get; set; }
        public decimal CustoAtual { get; set; }
        public decimal MargemAtual { get; set; }
        
        // Cenário simulado
        public decimal PrecoSimulado { get; set; }
        public decimal CustoSimulado { get; set; }
        public decimal MargemSimulada { get; set; }
        
        // Impacto
        public decimal ImpactoMargem { get; set; }
        public decimal ImpactoLucroUnitario { get; set; }
        public string TipoImpacto { get; set; } = string.Empty; // "Positivo", "Negativo", "Neutro"
        public string Classificacao { get; set; } = string.Empty; // "Excelente", "Bom", "Regular", "Ruim"
        
        // Recomendações
        public List<string> Recomendacoes { get; set; } = new();
        public DateTime DataSimulacao { get; set; }
    }
}