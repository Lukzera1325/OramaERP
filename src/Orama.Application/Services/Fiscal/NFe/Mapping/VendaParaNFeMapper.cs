using Orama.Application.Services.Fiscal;
using Orama.Domain.Entities;
using Orama.Domain.Entities.Fiscal;
using Orama.Domain.Entities.Fiscal.NFe;

namespace Orama.Application.Services.Fiscal.NFe.Mapping
{
    /// <summary>
    /// Mapper para converter Venda em NF-e - Isolado do core
    /// </summary>
    public class VendaParaNFeMapper : IVendaParaNFeMapper
    {
        private readonly IContextoFiscalService _contextoFiscalService;
        
        public VendaParaNFeMapper(IContextoFiscalService contextoFiscalService)
        {
            _contextoFiscalService = contextoFiscalService;
        }
        
        public async Task<NFeDocumento> MapearVendaParaNFeAsync(Venda venda, int usuarioId)
        {
            if (venda == null)
                throw new ArgumentNullException(nameof(venda));
            
            // Obter configuração fiscal da empresa para determinar ambiente
            var (empresaConfig, _, _) = await _contextoFiscalService.ObterConfiguracoesVigentesAsync(
                venda.EmpresaId, 
                venda.Itens.First().ProdutoId, 
                TipoOperacaoFiscal.Venda, 
                venda.DataVenda);
            
            if (empresaConfig == null)
                throw new InvalidOperationException("Configuração fiscal da empresa não encontrada");
            
            var nfeDocumento = new NFeDocumento
            {
                VendaId = venda.Id,
                Serie = 1, // Série padrão
                AmbienteFiscal = empresaConfig.AmbienteFiscal,
                Status = NFeStatus.Rascunho,
                DataEmissao = venda.DataVenda,
                CriadoPor = usuarioId
            };
            
            // Mapear itens
            var numeroItem = 1;
            foreach (var vendaItem in venda.Itens)
            {
                var nfeItem = await MapearItemVendaParaNFeAsync(vendaItem, numeroItem);
                nfeDocumento.Itens.Add(nfeItem);
                numeroItem++;
            }
            
            return nfeDocumento;
        }
        
        public async Task<NFeItem> MapearItemVendaParaNFeAsync(VendaItem vendaItem, int numeroItem)
        {
            if (vendaItem == null)
                throw new ArgumentNullException(nameof(vendaItem));
            
            // Obter contexto fiscal do item
            var contexto = await _contextoFiscalService.MontarContextoAsync(
                vendaItem.Venda?.EmpresaId ?? 0,
                vendaItem.ProdutoId,
                TipoOperacaoFiscal.Venda,
                vendaItem.Venda?.DataVenda ?? DateTime.Today,
                vendaItem.ValorTotal,
                vendaItem.Quantidade);
            
            var nfeItem = new NFeItem
            {
                ProdutoId = vendaItem.ProdutoId,
                NumeroItem = numeroItem,
                CodigoProduto = vendaItem.Produto?.Codigo ?? vendaItem.ProdutoId.ToString(),
                Descricao = vendaItem.Produto?.Descricao ?? "Produto",
                NCM = contexto.ProdutoFiscalConfig.NCM,
                CFOP = contexto.OperacaoFiscalConfig.CFOPPadrao,
                Unidade = vendaItem.Produto?.Unidade ?? "UN",
                Quantidade = vendaItem.Quantidade,
                ValorUnitario = vendaItem.ValorUnitario,
                ValorTotal = vendaItem.ValorTotal,
                Origem = (int)contexto.ProdutoFiscalConfig.Origem,
                CstCsosn = contexto.ObterCSTOuCSOSN() ?? "000",
                AliquotaICMS = contexto.ProdutoFiscalConfig.AliquotaICMSPadrao,
                BaseCalculoICMS = vendaItem.ValorTotal,
                ValorICMS = null // Será calculado pelo motor fiscal
            };
            
            // Calcular ICMS se houver alíquota
            if (nfeItem.AliquotaICMS.HasValue && nfeItem.AliquotaICMS > 0)
            {
                nfeItem.ValorICMS = (nfeItem.BaseCalculoICMS.Value * nfeItem.AliquotaICMS.Value) / 100;
            }
            
            // Validar item
            var erros = nfeItem.Validar();
            if (erros.Any())
            {
                throw new InvalidOperationException($"Item inválido: {string.Join(", ", erros)}");
            }
            
            return nfeItem;
        }
    }
}