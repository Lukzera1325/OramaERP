using Microsoft.EntityFrameworkCore;
using Orama.Application.Services.Fiscal.NFe.Mapping;
using Orama.Domain.Entities.Fiscal;
using Orama.Domain.Entities.Fiscal.NFe;
using Orama.Infra.Data.Context;
using System.Text.Json;

namespace Orama.Application.Services.Fiscal.NFe
{
    /// <summary>
    /// Serviço para emissão de NF-e - Isolado do core
    /// </summary>
    public class NFeEmissaoService : INFeEmissaoService
    {
        private readonly OramaDbContext _context;
        private readonly IContextoFiscalService _contextoFiscalService;
        private readonly ISefazNFeGateway _sefazGateway;
        private readonly IVendaParaNFeMapper _vendaMapper;
        
        public NFeEmissaoService(
            OramaDbContext context,
            IContextoFiscalService contextoFiscalService,
            ISefazNFeGateway sefazGateway,
            IVendaParaNFeMapper vendaMapper)
        {
            _context = context;
            _contextoFiscalService = contextoFiscalService;
            _sefazGateway = sefazGateway;
            _vendaMapper = vendaMapper;
        }
        
        public async Task<NFeDocumento> GerarNFeAsync(int vendaId, int usuarioId)
        {
            // Validar venda
            var erros = await ValidarVendaParaNFeAsync(vendaId);
            if (erros.Any())
            {
                throw new InvalidOperationException($"Venda inválida para NF-e: {string.Join(", ", erros)}");
            }
            
            // Verificar se já existe NF-e para esta venda
            var nfeExistente = await _context.NFeDocumentos
                .FirstOrDefaultAsync(n => n.VendaId == vendaId);
            
            if (nfeExistente != null)
            {
                throw new InvalidOperationException("Já existe NF-e para esta venda");
            }
            
            // Buscar venda e empresa
            var venda = await _context.Vendas
                .Include(v => v.Itens)
                .ThenInclude(i => i.Produto)
                .FirstOrDefaultAsync(v => v.Id == vendaId);
            
            if (venda == null)
            {
                throw new ArgumentException("Venda não encontrada");
            }
            
            // Mapear venda para NF-e
            var nfeDocumento = await _vendaMapper.MapearVendaParaNFeAsync(venda, usuarioId);
            
            // Gerar número e chave de acesso
            nfeDocumento.Numero = await ObterProximoNumeroAsync(venda.EmpresaId);
            
            // Buscar configuração da empresa para UF e CNPJ
            var empresaConfig = await _context.EmpresasFiscaisConfig
                .Where(c => c.EmpresaId == venda.EmpresaId)
                .Where(c => c.VigenteDe <= DateTime.Today)
                .Where(c => c.VigenteAte == null || c.VigenteAte >= DateTime.Today)
                .OrderByDescending(c => c.VigenteDe)
                .FirstOrDefaultAsync();
            
            if (empresaConfig == null)
            {
                throw new InvalidOperationException("Configuração fiscal da empresa não encontrada");
            }
            
            // Buscar dados da empresa
            var empresa = await _context.Empresas.FindAsync(venda.EmpresaId);
            if (empresa == null)
            {
                throw new InvalidOperationException("Empresa não encontrada");
            }
            
            // Gerar chave de acesso
            var ufCodigo = ObterCodigoUF(empresaConfig.UF);
            nfeDocumento.GerarChaveAcesso(ufCodigo, empresa.Cnpj);
            
            // Criar snapshot fiscal
            nfeDocumento.SnapshotFiscal = await CriarSnapshotFiscalAsync(venda);
            
            // Salvar NF-e
            _context.NFeDocumentos.Add(nfeDocumento);
            await _context.SaveChangesAsync();
            
            return nfeDocumento;
        }
        
        public async Task<NFeDocumento> AssinarEEnviarAsync(int nfeId, int usuarioId)
        {
            var nfe = await _context.NFeDocumentos
                .Include(n => n.Itens)
                .FirstOrDefaultAsync(n => n.Id == nfeId);
            
            if (nfe == null)
            {
                throw new ArgumentException("NF-e não encontrada");
            }
            
            if (nfe.Status != NFeStatus.Gerada && nfe.Status != NFeStatus.Rascunho)
            {
                throw new InvalidOperationException($"NF-e não pode ser enviada. Status atual: {nfe.Status}");
            }
            
            try
            {
                // Verificar segurança para produção
                if (nfe.AmbienteFiscal == AmbienteFiscal.Producao)
                {
                    await ValidarProducaoHabilitadaAsync(nfe.VendaId);
                }
                
                // Gerar XML
                var xmlGerado = await _sefazGateway.GerarXmlAsync(nfe);
                nfe.XmlGerado = xmlGerado;
                nfe.Status = NFeStatus.Gerada;
                
                // Assinar XML
                var xmlAssinado = await _sefazGateway.AssinarXmlAsync(xmlGerado, nfe.AmbienteFiscal);
                nfe.XmlAssinado = xmlAssinado;
                nfe.Status = NFeStatus.Assinada;
                
                // Enviar para SEFAZ
                var resultado = await _sefazGateway.EnviarNFeAsync(xmlAssinado, nfe.AmbienteFiscal);
                
                nfe.Status = resultado.Autorizada ? NFeStatus.Autorizada : NFeStatus.Rejeitada;
                nfe.ProtocoloAutorizacao = resultado.Protocolo;
                nfe.CodigoStatusSefaz = resultado.CodigoStatus;
                nfe.MensagemSefaz = resultado.Mensagem;
                nfe.AlteradoEm = DateTime.UtcNow;
                nfe.AlteradoPor = usuarioId;
                
                await _context.SaveChangesAsync();
                
                return nfe;
            }
            catch (Exception ex)
            {
                nfe.Status = NFeStatus.Rejeitada;
                nfe.MensagemSefaz = ex.Message;
                nfe.AlteradoEm = DateTime.UtcNow;
                nfe.AlteradoPor = usuarioId;
                
                await _context.SaveChangesAsync();
                throw;
            }
        }
        
        public async Task<int> ObterProximoNumeroAsync(int empresaId, int serie = 1)
        {
            var ultimoNumero = await _context.NFeDocumentos
                .Where(n => n.Serie == serie)
                .Join(_context.Vendas, n => n.VendaId, v => v.Id, (n, v) => new { n.Numero, v.EmpresaId })
                .Where(x => x.EmpresaId == empresaId)
                .MaxAsync(x => (int?)x.Numero) ?? 0;
            
            return ultimoNumero + 1;
        }
        
        public async Task<List<string>> ValidarVendaParaNFeAsync(int vendaId)
        {
            var erros = new List<string>();
            
            var venda = await _context.Vendas
                .Include(v => v.Itens)
                .ThenInclude(i => i.Produto)
                .FirstOrDefaultAsync(v => v.Id == vendaId);
            
            if (venda == null)
            {
                erros.Add("Venda não encontrada");
                return erros;
            }
            
            if (venda.Status != Domain.Entities.StatusVenda.Faturada)
            {
                erros.Add("Venda deve estar finalizada");
            }
            
            if (!venda.Itens.Any())
            {
                erros.Add("Venda deve ter pelo menos um item");
            }
            
            // Validar configurações fiscais
            foreach (var item in venda.Itens)
            {
                try
                {
                    var contexto = await _contextoFiscalService.MontarContextoAsync(
                        venda.EmpresaId,
                        item.ProdutoId,
                        TipoOperacaoFiscal.Venda,
                        venda.DataVenda,
                        item.ValorTotal,
                        item.Quantidade);
                }
                catch (Exception ex)
                {
                    erros.Add($"Produto {item.Produto?.Descricao}: {ex.Message}");
                }
            }
            
            return erros;
        }
        
        public async Task<List<NFeDocumento>> ListarNFesAsync(int empresaId, DateTime? dataInicio = null, DateTime? dataFim = null)
        {
            var query = _context.NFeDocumentos
                .Join(_context.Vendas, n => n.VendaId, v => v.Id, (n, v) => new { NFe = n, Venda = v })
                .Where(x => x.Venda.EmpresaId == empresaId);
            
            if (dataInicio.HasValue)
                query = query.Where(x => x.NFe.DataEmissao >= dataInicio.Value);
            
            if (dataFim.HasValue)
                query = query.Where(x => x.NFe.DataEmissao <= dataFim.Value);
            
            return await query
                .Select(x => x.NFe)
                .OrderByDescending(n => n.DataEmissao)
                .ToListAsync();
        }
        
        public async Task<NFeDocumento?> ObterNFePorIdAsync(int nfeId, int empresaId)
        {
            return await _context.NFeDocumentos
                .Include(n => n.Itens)
                .Include(n => n.Eventos)
                .Join(_context.Vendas, n => n.VendaId, v => v.Id, (n, v) => new { NFe = n, Venda = v })
                .Where(x => x.Venda.EmpresaId == empresaId && x.NFe.Id == nfeId)
                .Select(x => x.NFe)
                .FirstOrDefaultAsync();
        }
        
        public async Task<NFeDocumento?> ObterNFePorVendaAsync(int vendaId, int empresaId)
        {
            return await _context.NFeDocumentos
                .Include(n => n.Itens)
                .Include(n => n.Eventos)
                .Join(_context.Vendas, n => n.VendaId, v => v.Id, (n, v) => new { NFe = n, Venda = v })
                .Where(x => x.Venda.EmpresaId == empresaId && x.NFe.VendaId == vendaId)
                .Select(x => x.NFe)
                .FirstOrDefaultAsync();
        }
        
        private async Task<string> CriarSnapshotFiscalAsync(Domain.Entities.Venda venda)
        {
            var snapshot = new
            {
                DataEmissao = DateTime.UtcNow,
                VendaId = venda.Id,
                EmpresaId = venda.EmpresaId,
                Itens = new List<object>()
            };
            
            foreach (var item in venda.Itens)
            {
                try
                {
                    var contexto = await _contextoFiscalService.MontarContextoAsync(
                        venda.EmpresaId,
                        item.ProdutoId,
                        TipoOperacaoFiscal.Venda,
                        venda.DataVenda,
                        item.ValorTotal,
                        item.Quantidade);
                    
                    snapshot.Itens.Add(new
                    {
                        ProdutoId = item.ProdutoId,
                        NCM = contexto.ProdutoFiscalConfig.NCM,
                        CFOP = contexto.OperacaoFiscalConfig.CFOPPadrao,
                        CstCsosn = contexto.ObterCSTOuCSOSN(),
                        Origem = contexto.ProdutoFiscalConfig.Origem,
                        AliquotaICMS = contexto.ProdutoFiscalConfig.AliquotaICMSPadrao,
                        RegimeTributario = contexto.RegimeTributario,
                        AmbienteFiscal = contexto.AmbienteFiscal,
                        VersaoFiscal = contexto.VersaoFiscal
                    });
                }
                catch (Exception ex)
                {
                    // Log do erro mas continua o processo
                    Console.WriteLine($"Erro ao criar snapshot para produto {item.ProdutoId}: {ex.Message}");
                }
            }
            
            return JsonSerializer.Serialize(snapshot, new JsonSerializerOptions { WriteIndented = true });
        }
        
        private async Task ValidarProducaoHabilitadaAsync(int vendaId)
        {
            var venda = await _context.Vendas.FindAsync(vendaId);
            if (venda == null) return;
            
            var empresaConfig = await _context.EmpresasFiscaisConfig
                .Where(c => c.EmpresaId == venda.EmpresaId)
                .Where(c => c.VigenteDe <= DateTime.Today)
                .Where(c => c.VigenteAte == null || c.VigenteAte >= DateTime.Today)
                .OrderByDescending(c => c.VigenteDe)
                .FirstOrDefaultAsync();
            
            if (empresaConfig?.AmbienteFiscal == AmbienteFiscal.Producao)
            {
                // TODO: Implementar validação adicional para produção
                // Por exemplo, verificar flag ProducaoHabilitada
                throw new InvalidOperationException("Emissão em produção requer configuração adicional de segurança");
            }
        }
        
        private static string ObterCodigoUF(string uf)
        {
            var codigosUF = new Dictionary<string, string>
            {
                {"AC", "12"}, {"AL", "17"}, {"AP", "16"}, {"AM", "23"}, {"BA", "29"},
                {"CE", "23"}, {"DF", "53"}, {"ES", "32"}, {"GO", "52"}, {"MA", "21"},
                {"MT", "51"}, {"MS", "50"}, {"MG", "31"}, {"PA", "15"}, {"PB", "25"},
                {"PR", "41"}, {"PE", "26"}, {"PI", "22"}, {"RJ", "33"}, {"RN", "24"},
                {"RS", "43"}, {"RO", "11"}, {"RR", "14"}, {"SC", "42"}, {"SP", "35"},
                {"SE", "28"}, {"TO", "17"}
            };
            
            return codigosUF.TryGetValue(uf.ToUpper(), out var codigo) ? codigo : "35"; // Default SP
        }
    }
}