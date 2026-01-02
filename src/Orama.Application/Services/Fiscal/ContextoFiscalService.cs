using Microsoft.EntityFrameworkCore;
using Orama.Domain.Entities.Fiscal;
using Orama.Infra.Data.Context;

namespace Orama.Application.Services.Fiscal
{
    /// <summary>
    /// Serviço para montagem de contextos fiscais
    /// Responsável por reunir todas as configurações necessárias
    /// </summary>
    public class ContextoFiscalService : IContextoFiscalService
    {
        private readonly OramaDbContext _context;
        
        public ContextoFiscalService(OramaDbContext context)
        {
            _context = context;
        }
        
        public async Task<ContextoFiscal> MontarContextoAsync(
            int empresaId,
            int produtoId,
            TipoOperacaoFiscal tipoOperacao,
            DateTime dataOperacao,
            decimal valorOperacao,
            decimal quantidade = 1,
            string? ufDestino = null)
        {
            // Obter configurações vigentes
            var (empresaConfig, produtoConfig, operacaoConfig) = 
                await ObterConfiguracoesVigentesAsync(empresaId, produtoId, tipoOperacao, dataOperacao);
            
            // Validar se todas as configurações foram encontradas
            if (empresaConfig == null)
                throw new InvalidOperationException($"Configuração fiscal da empresa {empresaId} não encontrada para {dataOperacao:dd/MM/yyyy}");
            
            if (produtoConfig == null)
                throw new InvalidOperationException($"Configuração fiscal do produto {produtoId} não encontrada para {dataOperacao:dd/MM/yyyy}");
            
            if (operacaoConfig == null)
                throw new InvalidOperationException($"Configuração fiscal da operação {tipoOperacao} não encontrada para {dataOperacao:dd/MM/yyyy}");
            
            // Montar contexto
            var contexto = new ContextoFiscal
            {
                EmpresaFiscalConfig = empresaConfig,
                ProdutoFiscalConfig = produtoConfig,
                OperacaoFiscalConfig = operacaoConfig,
                DataOperacao = dataOperacao,
                ValorOperacao = valorOperacao,
                Quantidade = quantidade,
                UFDestino = ufDestino
            };
            
            // Validar contexto montado
            var erros = await ValidarContextoAsync(contexto);
            if (erros.Any())
            {
                throw new InvalidOperationException($"Contexto fiscal inválido: {string.Join(", ", erros)}");
            }
            
            return contexto;
        }
        
        public async Task<List<string>> ValidarContextoAsync(ContextoFiscal contexto)
        {
            var erros = new List<string>();
            
            if (contexto.EmpresaFiscalConfig == null)
                erros.Add("Configuração fiscal da empresa é obrigatória");
            
            if (contexto.ProdutoFiscalConfig == null)
                erros.Add("Configuração fiscal do produto é obrigatória");
            
            if (contexto.OperacaoFiscalConfig == null)
                erros.Add("Configuração fiscal da operação é obrigatória");
            
            if (contexto.DataOperacao == default)
                erros.Add("Data da operação é obrigatória");
            
            if (contexto.ValorOperacao <= 0)
                erros.Add("Valor da operação deve ser maior que zero");
            
            if (contexto.Quantidade <= 0)
                erros.Add("Quantidade deve ser maior que zero");
            
            // Validar vigências
            if (contexto.EmpresaFiscalConfig != null && !contexto.EmpresaFiscalConfig.EstaVigente(contexto.DataOperacao))
                erros.Add("Configuração fiscal da empresa não está vigente na data da operação");
            
            if (contexto.ProdutoFiscalConfig != null && !contexto.ProdutoFiscalConfig.EstaVigente(contexto.DataOperacao))
                erros.Add("Configuração fiscal do produto não está vigente na data da operação");
            
            if (contexto.OperacaoFiscalConfig != null && !contexto.OperacaoFiscalConfig.EstaVigente(contexto.DataOperacao))
                erros.Add("Configuração fiscal da operação não está vigente na data da operação");
            
            // Validar CST/CSOSN
            if (contexto.ProdutoFiscalConfig != null && contexto.EmpresaFiscalConfig != null)
            {
                var cstOuCsosn = contexto.ObterCSTOuCSOSN();
                if (string.IsNullOrWhiteSpace(cstOuCsosn))
                {
                    var regime = contexto.RegimeTributario;
                    erros.Add($"CST/CSOSN não configurado para o regime {regime}");
                }
            }
            
            return erros;
        }
        
        public async Task<(EmpresaFiscalConfig? empresa, ProdutoFiscalConfig? produto, OperacaoFiscalConfig? operacao)> 
            ObterConfiguracoesVigentesAsync(int empresaId, int produtoId, TipoOperacaoFiscal tipoOperacao, DateTime data)
        {
            // Buscar configuração da empresa
            var empresaConfig = await _context.EmpresasFiscaisConfig
                .Where(c => c.EmpresaId == empresaId)
                .Where(c => c.VigenteDe <= data)
                .Where(c => c.VigenteAte == null || c.VigenteAte >= data)
                .OrderByDescending(c => c.VigenteDe)
                .FirstOrDefaultAsync();
            
            // Buscar configuração do produto
            var produtoConfig = await _context.ProdutosFiscaisConfig
                .Where(c => c.ProdutoId == produtoId)
                .Where(c => c.VigenteDe <= data)
                .Where(c => c.VigenteAte == null || c.VigenteAte >= data)
                .OrderByDescending(c => c.VigenteDe)
                .FirstOrDefaultAsync();
            
            // Buscar configuração da operação
            var operacaoConfig = await _context.OperacoesFiscaisConfig
                .Where(c => c.TipoOperacao == tipoOperacao)
                .Where(c => c.VigenteDe <= data)
                .Where(c => c.VigenteAte == null || c.VigenteAte >= data)
                .OrderByDescending(c => c.VigenteDe)
                .FirstOrDefaultAsync();
            
            return (empresaConfig, produtoConfig, operacaoConfig);
        }
    }
}