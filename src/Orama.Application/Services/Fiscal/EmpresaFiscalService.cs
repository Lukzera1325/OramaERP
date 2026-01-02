using Microsoft.EntityFrameworkCore;
using Orama.Domain.Entities.Fiscal;
using Orama.Infra.Data.Context;

namespace Orama.Application.Services.Fiscal
{
    /// <summary>
    /// Serviço para gerenciamento de configurações fiscais da empresa
    /// Isolado do core, focado apenas em configuração fiscal
    /// </summary>
    public class EmpresaFiscalService : IEmpresaFiscalService
    {
        private readonly OramaDbContext _context;
        
        public EmpresaFiscalService(OramaDbContext context)
        {
            _context = context;
        }
        
        public async Task<EmpresaFiscalConfig?> ObterConfiguracaoVigenteAsync(int empresaId, DateTime data)
        {
            return await _context.EmpresasFiscaisConfig
                .Where(c => c.EmpresaId == empresaId)
                .Where(c => c.VigenteDe <= data)
                .Where(c => c.VigenteAte == null || c.VigenteAte >= data)
                .OrderByDescending(c => c.VigenteDe)
                .FirstOrDefaultAsync();
        }
        
        public async Task<EmpresaFiscalConfig> CriarConfiguracaoAsync(EmpresaFiscalConfig configuracao)
        {
            // Validar antes de criar
            var erros = await ValidarConfiguracaoAsync(configuracao);
            if (erros.Any())
            {
                throw new InvalidOperationException($"Configuração inválida: {string.Join(", ", erros)}");
            }
            
            // Verificar se já existe configuração vigente para a mesma data
            var configuracaoExistente = await ObterConfiguracaoVigenteAsync(configuracao.EmpresaId, configuracao.VigenteDe);
            if (configuracaoExistente != null)
            {
                throw new InvalidOperationException("Já existe uma configuração fiscal vigente para esta data");
            }
            
            configuracao.CriadoEm = DateTime.UtcNow;
            
            _context.EmpresasFiscaisConfig.Add(configuracao);
            await _context.SaveChangesAsync();
            
            return configuracao;
        }
        
        public async Task<EmpresaFiscalConfig> AtualizarConfiguracaoAsync(EmpresaFiscalConfig configuracao)
        {
            var erros = await ValidarConfiguracaoAsync(configuracao);
            if (erros.Any())
            {
                throw new InvalidOperationException($"Configuração inválida: {string.Join(", ", erros)}");
            }
            
            _context.EmpresasFiscaisConfig.Update(configuracao);
            await _context.SaveChangesAsync();
            
            return configuracao;
        }
        
        public async Task EncerrarVigenciaAsync(int configuracaoId, DateTime dataEncerramento)
        {
            var configuracao = await _context.EmpresasFiscaisConfig.FindAsync(configuracaoId);
            if (configuracao == null)
            {
                throw new ArgumentException("Configuração fiscal não encontrada");
            }
            
            if (dataEncerramento < configuracao.VigenteDe)
            {
                throw new InvalidOperationException("Data de encerramento não pode ser anterior ao início da vigência");
            }
            
            configuracao.VigenteAte = dataEncerramento;
            await _context.SaveChangesAsync();
        }
        
        public async Task<List<EmpresaFiscalConfig>> ListarConfiguracoesAsync(int empresaId)
        {
            return await _context.EmpresasFiscaisConfig
                .Where(c => c.EmpresaId == empresaId)
                .OrderByDescending(c => c.VigenteDe)
                .ToListAsync();
        }
        
        public async Task<List<string>> ValidarConfiguracaoAsync(EmpresaFiscalConfig configuracao)
        {
            var erros = new List<string>();
            
            if (configuracao.EmpresaId <= 0)
                erros.Add("EmpresaId é obrigatório");
            
            if (string.IsNullOrWhiteSpace(configuracao.UF) || configuracao.UF.Length != 2)
                erros.Add("UF deve ter exatamente 2 caracteres");
            
            if (string.IsNullOrWhiteSpace(configuracao.VersaoFiscal))
                erros.Add("VersaoFiscal é obrigatória");
            
            if (configuracao.VigenteDe == default)
                erros.Add("VigenteDe é obrigatório");
            
            if (configuracao.VigenteAte.HasValue && configuracao.VigenteAte <= configuracao.VigenteDe)
                erros.Add("VigenteAte deve ser posterior a VigenteDe");
            
            // Validar combinação CRT x Regime
            if (configuracao.RegimeTributario == RegimeTributario.SimplesNacional && 
                configuracao.CRT != CRT.SimplesNacional && configuracao.CRT != CRT.SimplesNacionalExcesso)
            {
                erros.Add("CRT incompatível com Simples Nacional");
            }
            
            if (configuracao.RegimeTributario == RegimeTributario.RegimeNormal && 
                configuracao.CRT == CRT.SimplesNacional)
            {
                erros.Add("CRT incompatível com Regime Normal");
            }
            
            return erros;
        }
    }
}