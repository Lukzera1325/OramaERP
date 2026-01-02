using Microsoft.EntityFrameworkCore;
using Orama.Domain.Entities.Fiscal.NFe;
using Orama.Infra.Data.Context;

namespace Orama.Application.Services.Fiscal.NFe
{
    /// <summary>
    /// Serviço para consulta de NF-e
    /// </summary>
    public class NFeConsultaService : INFeConsultaService
    {
        private readonly OramaDbContext _context;
        private readonly ISefazNFeGateway _sefazGateway;
        
        public NFeConsultaService(OramaDbContext context, ISefazNFeGateway sefazGateway)
        {
            _context = context;
            _sefazGateway = sefazGateway;
        }
        
        public async Task<NFeDocumento> ConsultarSituacaoAsync(int nfeId, int usuarioId)
        {
            var nfe = await _context.NFeDocumentos
                .Include(n => n.Eventos)
                .FirstOrDefaultAsync(n => n.Id == nfeId);
            
            if (nfe == null)
                throw new ArgumentException("NF-e não encontrada");
            
            if (!nfe.PodeConsultar())
                throw new InvalidOperationException("NF-e não pode ser consultada");
            
            try
            {
                // Consultar na SEFAZ
                var resultado = await _sefazGateway.ConsultarNFeAsync(nfe.ChaveAcesso!, nfe.AmbienteFiscal);
                
                // Criar evento de consulta
                var evento = NFeEvento.CriarConsulta(nfe.Id, usuarioId);
                evento.Status = resultado.Encontrada ? StatusEventoNFe.Processado : StatusEventoNFe.Rejeitado;
                evento.CodigoStatusSefaz = resultado.CodigoStatus;
                evento.MensagemSefaz = resultado.Mensagem;
                evento.XmlRetorno = resultado.XmlRetorno;
                evento.Protocolo = resultado.Protocolo;
                
                nfe.Eventos.Add(evento);
                
                // Atualizar status da NF-e se necessário
                if (resultado.Encontrada && resultado.CodigoStatus == 100)
                {
                    nfe.Status = NFeStatus.Autorizada;
                    nfe.ProtocoloAutorizacao = resultado.Protocolo;
                }
                else if (resultado.CodigoStatus >= 200 && resultado.CodigoStatus < 300)
                {
                    nfe.Status = NFeStatus.Rejeitada;
                }
                
                nfe.UltimaConsulta = DateTime.UtcNow;
                nfe.CodigoStatusSefaz = resultado.CodigoStatus;
                nfe.MensagemSefaz = resultado.Mensagem;
                nfe.AlteradoEm = DateTime.UtcNow;
                nfe.AlteradoPor = usuarioId;
                
                await _context.SaveChangesAsync();
                
                return nfe;
            }
            catch (Exception ex)
            {
                // Criar evento de erro
                var eventoErro = NFeEvento.CriarConsulta(nfe.Id, usuarioId);
                eventoErro.Status = StatusEventoNFe.Erro;
                eventoErro.MensagemSefaz = ex.Message;
                
                nfe.Eventos.Add(eventoErro);
                nfe.UltimaConsulta = DateTime.UtcNow;
                
                await _context.SaveChangesAsync();
                throw;
            }
        }
        
        public async Task<ResultadoConsultaNFe> ConsultarPorChaveAsync(string chaveAcesso, int empresaId)
        {
            if (string.IsNullOrWhiteSpace(chaveAcesso) || chaveAcesso.Length != 44)
                throw new ArgumentException("Chave de acesso inválida");
            
            // Buscar configuração da empresa para determinar ambiente
            var empresaConfig = await _context.EmpresasFiscaisConfig
                .Where(c => c.EmpresaId == empresaId)
                .Where(c => c.VigenteDe <= DateTime.Today)
                .Where(c => c.VigenteAte == null || c.VigenteAte >= DateTime.Today)
                .OrderByDescending(c => c.VigenteDe)
                .FirstOrDefaultAsync();
            
            if (empresaConfig == null)
                throw new InvalidOperationException("Configuração fiscal da empresa não encontrada");
            
            return await _sefazGateway.ConsultarNFeAsync(chaveAcesso, empresaConfig.AmbienteFiscal);
        }
        
        public async Task<bool> VerificarStatusServicoAsync(int empresaId)
        {
            // Buscar configuração da empresa
            var empresaConfig = await _context.EmpresasFiscaisConfig
                .Where(c => c.EmpresaId == empresaId)
                .Where(c => c.VigenteDe <= DateTime.Today)
                .Where(c => c.VigenteAte == null || c.VigenteAte >= DateTime.Today)
                .OrderByDescending(c => c.VigenteDe)
                .FirstOrDefaultAsync();
            
            if (empresaConfig == null)
                return false;
            
            return await _sefazGateway.VerificarStatusServicoAsync(empresaConfig.AmbienteFiscal, empresaConfig.UF);
        }
        
        public async Task<int> AtualizarStatusPendentesAsync(int empresaId)
        {
            // Buscar NF-es enviadas mas não autorizadas
            var nfesPendentes = await _context.NFeDocumentos
                .Join(_context.Vendas, n => n.VendaId, v => v.Id, (n, v) => new { NFe = n, Venda = v })
                .Where(x => x.Venda.EmpresaId == empresaId)
                .Where(x => x.NFe.Status == NFeStatus.Enviada)
                .Where(x => !string.IsNullOrEmpty(x.NFe.ChaveAcesso))
                .Where(x => x.NFe.UltimaConsulta == null || x.NFe.UltimaConsulta < DateTime.UtcNow.AddMinutes(-5))
                .Select(x => x.NFe)
                .Take(10) // Limitar para não sobrecarregar
                .ToListAsync();
            
            var atualizadas = 0;
            
            foreach (var nfe in nfesPendentes)
            {
                try
                {
                    var resultado = await _sefazGateway.ConsultarNFeAsync(nfe.ChaveAcesso!, nfe.AmbienteFiscal);
                    
                    if (resultado.Encontrada && resultado.CodigoStatus == 100)
                    {
                        nfe.Status = NFeStatus.Autorizada;
                        nfe.ProtocoloAutorizacao = resultado.Protocolo;
                        atualizadas++;
                    }
                    else if (resultado.CodigoStatus >= 200 && resultado.CodigoStatus < 300)
                    {
                        nfe.Status = NFeStatus.Rejeitada;
                        atualizadas++;
                    }
                    
                    nfe.UltimaConsulta = DateTime.UtcNow;
                    nfe.CodigoStatusSefaz = resultado.CodigoStatus;
                    nfe.MensagemSefaz = resultado.Mensagem;
                }
                catch (Exception ex)
                {
                    // Log do erro mas continua o processo
                    Console.WriteLine($"Erro ao consultar NF-e {nfe.Id}: {ex.Message}");
                    nfe.UltimaConsulta = DateTime.UtcNow;
                }
            }
            
            if (atualizadas > 0)
            {
                await _context.SaveChangesAsync();
            }
            
            return atualizadas;
        }
    }
}