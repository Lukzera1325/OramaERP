using Microsoft.EntityFrameworkCore;
using Orama.Domain.Entities.Fiscal.NFe;
using Orama.Infra.Data.Context;

namespace Orama.Application.Services.Fiscal.NFe
{
    /// <summary>
    /// Serviço para cancelamento de NF-e
    /// </summary>
    public class NFeCancelamentoService : INFeCancelamentoService
    {
        private readonly OramaDbContext _context;
        private readonly ISefazNFeGateway _sefazGateway;
        
        public NFeCancelamentoService(OramaDbContext context, ISefazNFeGateway sefazGateway)
        {
            _context = context;
            _sefazGateway = sefazGateway;
        }
        
        public async Task<NFeDocumento> CancelarNFeAsync(int nfeId, string justificativa, int usuarioId)
        {
            // Validar justificativa
            if (string.IsNullOrWhiteSpace(justificativa) || justificativa.Length < 15)
                throw new ArgumentException("Justificativa deve ter pelo menos 15 caracteres");
            
            var nfe = await _context.NFeDocumentos
                .Include(n => n.Eventos)
                .FirstOrDefaultAsync(n => n.Id == nfeId);
            
            if (nfe == null)
                throw new ArgumentException("NF-e não encontrada");
            
            // Verificar se pode cancelar
            var (pode, motivo) = await PodeCancelarAsync(nfeId);
            if (!pode)
                throw new InvalidOperationException(motivo);
            
            try
            {
                // Criar evento de cancelamento
                var evento = NFeEvento.CriarCancelamento(nfe.Id, justificativa, usuarioId);
                
                // Enviar cancelamento para SEFAZ
                var resultado = await _sefazGateway.CancelarNFeAsync(nfe.ChaveAcesso!, justificativa, nfe.AmbienteFiscal);
                
                // Atualizar evento com resultado
                evento.Status = resultado.Sucesso ? StatusEventoNFe.Processado : StatusEventoNFe.Rejeitado;
                evento.CodigoStatusSefaz = resultado.CodigoStatus;
                evento.MensagemSefaz = resultado.Mensagem;
                evento.XmlRetorno = resultado.XmlRetorno;
                evento.Protocolo = resultado.Protocolo;
                
                nfe.Eventos.Add(evento);
                
                // Atualizar status da NF-e
                if (resultado.Sucesso)
                {
                    nfe.Status = NFeStatus.Cancelada;
                    nfe.MensagemSefaz = "NF-e cancelada com sucesso";
                }
                else
                {
                    nfe.MensagemSefaz = resultado.Mensagem;
                }
                
                nfe.CodigoStatusSefaz = resultado.CodigoStatus;
                nfe.AlteradoEm = DateTime.UtcNow;
                nfe.AlteradoPor = usuarioId;
                
                await _context.SaveChangesAsync();
                
                if (!resultado.Sucesso)
                {
                    throw new InvalidOperationException($"Erro ao cancelar NF-e: {resultado.Mensagem}");
                }
                
                return nfe;
            }
            catch (Exception ex)
            {
                // Criar evento de erro
                var eventoErro = NFeEvento.CriarCancelamento(nfe.Id, justificativa, usuarioId);
                eventoErro.Status = StatusEventoNFe.Erro;
                eventoErro.MensagemSefaz = ex.Message;
                
                nfe.Eventos.Add(eventoErro);
                
                await _context.SaveChangesAsync();
                throw;
            }
        }
        
        public async Task<(bool Pode, string Motivo)> PodeCancelarAsync(int nfeId)
        {
            var nfe = await _context.NFeDocumentos.FindAsync(nfeId);
            
            if (nfe == null)
                return (false, "NF-e não encontrada");
            
            if (nfe.Status != NFeStatus.Autorizada)
                return (false, $"NF-e não pode ser cancelada. Status atual: {nfe.Status}");
            
            if (string.IsNullOrEmpty(nfe.ChaveAcesso))
                return (false, "NF-e não possui chave de acesso");
            
            // Verificar prazo de 24 horas
            if (nfe.DataEmissao < DateTime.Today.AddDays(-1))
                return (false, "Prazo de 24 horas para cancelamento expirado");
            
            // Verificar se já foi cancelada
            var jaCancelada = await _context.NFeEventos
                .AnyAsync(e => e.NFeDocumentoId == nfeId && 
                              e.TipoEvento == TipoEventoNFe.Cancelamento && 
                              e.Status == StatusEventoNFe.Processado);
            
            if (jaCancelada)
                return (false, "NF-e já foi cancelada");
            
            return (true, "NF-e pode ser cancelada");
        }
        
        public async Task<List<NFeDocumento>> ListarCancelaveisAsync(int empresaId)
        {
            var dataLimite = DateTime.Today.AddDays(-1);
            
            return await _context.NFeDocumentos
                .Join(_context.Vendas, n => n.VendaId, v => v.Id, (n, v) => new { NFe = n, Venda = v })
                .Where(x => x.Venda.EmpresaId == empresaId)
                .Where(x => x.NFe.Status == NFeStatus.Autorizada)
                .Where(x => x.NFe.DataEmissao >= dataLimite)
                .Where(x => !string.IsNullOrEmpty(x.NFe.ChaveAcesso))
                .Select(x => x.NFe)
                .OrderByDescending(n => n.DataEmissao)
                .ToListAsync();
        }
    }
}