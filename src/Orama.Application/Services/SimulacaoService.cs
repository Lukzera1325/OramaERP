using Microsoft.EntityFrameworkCore;
using Orama.Domain.Entities;
using Orama.Infra.Data.Context;

namespace Orama.Application.Services
{
    public class SimulacaoService : ISimulacaoService
    {
        private readonly OramaDbContext _context;
        
        public SimulacaoService(OramaDbContext context)
        {
            _context = context;
        }
        
        public async Task<SimulacaoPrecoDto> SimularNovoPrecoAsync(int produtoId, decimal novoPreco)
        {
            var produto = await _context.Produtos
                .FirstOrDefaultAsync(p => p.Id == produtoId);
                
            if (produto == null)
                throw new ArgumentException("Produto não encontrado");
                
            // Cálculos atuais
            var margemAtual = produto.PrecoVenda > 0 ? ((produto.PrecoVenda - produto.PrecoCusto) / produto.PrecoVenda) * 100 : 0;
            var lucroAtual = produto.PrecoVenda - produto.PrecoCusto;
            
            // Cálculos simulados
            var margemSimulada = novoPreco > 0 ? ((novoPreco - produto.PrecoCusto) / novoPreco) * 100 : 0;
            var lucroSimulado = novoPreco - produto.PrecoCusto;
            
            // Análise
            var diferencaMargem = margemSimulada - margemAtual;
            var diferencaLucro = lucroSimulado - lucroAtual;
            
            var status = margemSimulada switch
            {
                < 0 => "Prejuízo",
                < 10 => "Margem Baixa",
                < 20 => "Margem Regular",
                < 30 => "Margem Boa",
                _ => "Margem Excelente"
            };
            
            var recomendacao = margemSimulada switch
            {
                < 0 => "⚠️ Preço resultará em prejuízo. Não recomendado.",
                < 10 => "⚠️ Margem muito baixa. Considere aumentar mais o preço.",
                < 20 => "✅ Margem aceitável, mas pode ser melhorada.",
                < 30 => "✅ Boa margem. Preço competitivo e lucrativo.",
                _ => "✅ Excelente margem. Preço muito lucrativo."
            };
            
            return new SimulacaoPrecoDto
            {
                ProdutoId = produto.Id,
                ProdutoDescricao = produto.Descricao,
                PrecoAtual = produto.PrecoVenda,
                CustoAtual = produto.PrecoCusto,
                MargemAtual = margemAtual,
                LucroUnitarioAtual = lucroAtual,
                PrecoSimulado = novoPreco,
                CustoSimulado = produto.PrecoCusto,
                MargemSimulada = margemSimulada,
                LucroUnitarioSimulado = lucroSimulado,
                DiferencaMargem = diferencaMargem,
                DiferencaLucro = diferencaLucro,
                StatusSimulacao = status,
                Recomendacao = recomendacao,
                TipoSimulacao = "Preço",
                DataSimulacao = DateTime.Now
            };
        }
        
        public async Task<SimulacaoPrecoDto> SimularNovoCustoAsync(int produtoId, decimal novoCusto)
        {
            var produto = await _context.Produtos
                .FirstOrDefaultAsync(p => p.Id == produtoId);
                
            if (produto == null)
                throw new ArgumentException("Produto não encontrado");
                
            // Cálculos atuais
            var margemAtual = produto.PrecoVenda > 0 ? ((produto.PrecoVenda - produto.PrecoCusto) / produto.PrecoVenda) * 100 : 0;
            var lucroAtual = produto.PrecoVenda - produto.PrecoCusto;
            
            // Cálculos simulados
            var margemSimulada = produto.PrecoVenda > 0 ? ((produto.PrecoVenda - novoCusto) / produto.PrecoVenda) * 100 : 0;
            var lucroSimulado = produto.PrecoVenda - novoCusto;
            
            // Análise
            var diferencaMargem = margemSimulada - margemAtual;
            var diferencaLucro = lucroSimulado - lucroAtual;
            
            var status = margemSimulada switch
            {
                < 0 => "Prejuízo",
                < 10 => "Margem Baixa",
                < 20 => "Margem Regular",
                < 30 => "Margem Boa",
                _ => "Margem Excelente"
            };
            
            var recomendacao = diferencaMargem switch
            {
                < -10 => "⚠️ Aumento de custo impactará muito a margem. Considere reajustar preço.",
                < -5 => "⚠️ Aumento de custo reduzirá margem significativamente.",
                < 0 => "⚠️ Aumento de custo reduzirá margem.",
                > 5 => "✅ Redução de custo melhorará muito a margem.",
                > 0 => "✅ Redução de custo melhorará a margem.",
                _ => "➡️ Custo mantém margem atual."
            };
            
            return new SimulacaoPrecoDto
            {
                ProdutoId = produto.Id,
                ProdutoDescricao = produto.Descricao,
                PrecoAtual = produto.PrecoVenda,
                CustoAtual = produto.PrecoCusto,
                MargemAtual = margemAtual,
                LucroUnitarioAtual = lucroAtual,
                PrecoSimulado = produto.PrecoVenda,
                CustoSimulado = novoCusto,
                MargemSimulada = margemSimulada,
                LucroUnitarioSimulado = lucroSimulado,
                DiferencaMargem = diferencaMargem,
                DiferencaLucro = diferencaLucro,
                StatusSimulacao = status,
                Recomendacao = recomendacao,
                TipoSimulacao = "Custo",
                DataSimulacao = DateTime.Now
            };
        }
        
        public async Task<SimulacaoVendaDto> SimularVendaAsync(int produtoId, decimal quantidade, decimal precoUnitario)
        {
            var produto = await _context.Produtos
                .FirstOrDefaultAsync(p => p.Id == produtoId);
                
            if (produto == null)
                throw new ArgumentException("Produto não encontrado");
                
            // Cálculos da venda
            var valorTotal = quantidade * precoUnitario;
            var custoTotal = quantidade * produto.PrecoCusto;
            var lucroTotal = valorTotal - custoTotal;
            var margemPercentual = valorTotal > 0 ? (lucroTotal / valorTotal) * 100 : 0;
            
            // Análise
            var status = margemPercentual switch
            {
                < 0 => "Venda com Prejuízo",
                < 10 => "Venda com Margem Baixa",
                < 20 => "Venda com Margem Regular",
                < 30 => "Venda com Margem Boa",
                _ => "Venda com Margem Excelente"
            };
            
            var recomendacao = margemPercentual switch
            {
                < 0 => "❌ Venda resultará em prejuízo. Não recomendada.",
                < 10 => "⚠️ Margem muito baixa. Considere aumentar preço.",
                < 20 => "✅ Margem aceitável para a venda.",
                < 30 => "✅ Boa margem. Venda recomendada.",
                _ => "✅ Excelente margem. Venda muito lucrativa."
            };
            
            return new SimulacaoVendaDto
            {
                ProdutoId = produto.Id,
                ProdutoDescricao = produto.Descricao,
                Quantidade = quantidade,
                PrecoUnitario = precoUnitario,
                CustoUnitario = produto.PrecoCusto,
                ValorTotalVenda = valorTotal,
                CustoTotalVenda = custoTotal,
                LucroTotalVenda = lucroTotal,
                MargemPercentual = margemPercentual,
                StatusVenda = status,
                Recomendacao = recomendacao,
                DataSimulacao = DateTime.Now
            };
        }
        
        public async Task<List<SimulacaoPrecoDto>> SimularCenariosProdutoAsync(int produtoId)
        {
            var produto = await _context.Produtos
                .FirstOrDefaultAsync(p => p.Id == produtoId);
                
            if (produto == null)
                throw new ArgumentException("Produto não encontrado");
                
            var cenarios = new List<SimulacaoPrecoDto>();
            
            // Cenários de preço (variações de -20% a +50%)
            var variacoes = new[] { -0.2m, -0.1m, -0.05m, 0.05m, 0.1m, 0.2m, 0.3m, 0.5m };
            
            foreach (var variacao in variacoes)
            {
                var novoPreco = produto.PrecoVenda * (1 + variacao);
                if (novoPreco > 0)
                {
                    var simulacao = await SimularNovoPrecoAsync(produtoId, novoPreco);
                    simulacao.TipoSimulacao = $"Preço {(variacao >= 0 ? "+" : "")}{variacao:P0}";
                    cenarios.Add(simulacao);
                }
            }
            
            return cenarios.OrderByDescending(c => c.MargemSimulada).ToList();
        }
        
        public async Task<SimulacaoImpactoDto> SimularImpactoMargemAsync(int produtoId, decimal novoPreco, decimal novoCusto)
        {
            var produto = await _context.Produtos
                .FirstOrDefaultAsync(p => p.Id == produtoId);
                
            if (produto == null)
                throw new ArgumentException("Produto não encontrado");
                
            // Cálculos atuais
            var margemAtual = produto.PrecoVenda > 0 ? ((produto.PrecoVenda - produto.PrecoCusto) / produto.PrecoVenda) * 100 : 0;
            var lucroAtual = produto.PrecoVenda - produto.PrecoCusto;
            
            // Cálculos simulados
            var margemSimulada = novoPreco > 0 ? ((novoPreco - novoCusto) / novoPreco) * 100 : 0;
            var lucroSimulado = novoPreco - novoCusto;
            
            // Impactos
            var impactoMargem = margemSimulada - margemAtual;
            var impactoLucro = lucroSimulado - lucroAtual;
            
            var tipoImpacto = impactoMargem switch
            {
                > 5 => "Positivo",
                < -5 => "Negativo",
                _ => "Neutro"
            };
            
            var classificacao = margemSimulada switch
            {
                < 0 => "Ruim",
                < 10 => "Regular",
                < 25 => "Bom",
                _ => "Excelente"
            };
            
            // Recomendações
            var recomendacoes = new List<string>();
            
            if (margemSimulada < 0)
            {
                recomendacoes.Add("Cenário resultará em prejuízo");
                recomendacoes.Add("Considere aumentar preço ou reduzir custo");
            }
            else if (margemSimulada < 10)
            {
                recomendacoes.Add("Margem muito baixa");
                recomendacoes.Add("Pode não cobrir despesas operacionais");
            }
            else if (impactoMargem > 10)
            {
                recomendacoes.Add("Excelente melhoria na margem");
                recomendacoes.Add("Cenário muito favorável");
            }
            else if (impactoMargem > 5)
            {
                recomendacoes.Add("Boa melhoria na margem");
                recomendacoes.Add("Cenário recomendado");
            }
            
            if (novoPreco > produto.PrecoVenda * 1.2m)
            {
                recomendacoes.Add("Aumento de preço significativo - verificar aceitação do mercado");
            }
            
            if (novoCusto < produto.PrecoCusto * 0.8m)
            {
                recomendacoes.Add("Redução de custo significativa - verificar viabilidade");
            }
            
            return new SimulacaoImpactoDto
            {
                ProdutoId = produto.Id,
                ProdutoDescricao = produto.Descricao,
                PrecoAtual = produto.PrecoVenda,
                CustoAtual = produto.PrecoCusto,
                MargemAtual = margemAtual,
                PrecoSimulado = novoPreco,
                CustoSimulado = novoCusto,
                MargemSimulada = margemSimulada,
                ImpactoMargem = impactoMargem,
                ImpactoLucroUnitario = impactoLucro,
                TipoImpacto = tipoImpacto,
                Classificacao = classificacao,
                Recomendacoes = recomendacoes,
                DataSimulacao = DateTime.Now
            };
        }
    }
}