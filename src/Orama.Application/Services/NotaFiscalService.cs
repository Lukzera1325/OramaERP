using Microsoft.EntityFrameworkCore;
using Orama.Domain.Entities;
using Orama.Infra.Data.Context;

namespace Orama.Application.Services
{
    public class NotaFiscalService : INotaFiscalService
    {
        private readonly OramaDbContext _context;

        public NotaFiscalService(OramaDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<NotaFiscal>> ObterTodosAsync(int empresaId)
        {
            return await _context.NotasFiscais
                .Include(nf => nf.Cliente)
                .Include(nf => nf.Fornecedor)
                .Include(nf => nf.Venda)
                .Include(nf => nf.Compra)
                .Where(nf => nf.EmpresaId == empresaId)
                .OrderByDescending(nf => nf.DataEmissao)
                .ToListAsync();
        }

        public async Task<NotaFiscal?> ObterPorIdAsync(int id, int empresaId)
        {
            return await _context.NotasFiscais
                .Include(nf => nf.Cliente)
                .Include(nf => nf.Fornecedor)
                .Include(nf => nf.Venda)
                .Include(nf => nf.Compra)
                .Include(nf => nf.Itens)
                    .ThenInclude(i => i.Produto)
                .FirstOrDefaultAsync(nf => nf.Id == id && nf.EmpresaId == empresaId);
        }

        public async Task<NotaFiscal> CriarAsync(NotaFiscal notaFiscal)
        {
            // Gerar número se não informado
            if (string.IsNullOrEmpty(notaFiscal.Numero))
            {
                notaFiscal.Numero = await GerarProximoNumeroAsync(notaFiscal.Serie, notaFiscal.Tipo, notaFiscal.EmpresaId);
            }

            // Calcular totais
            foreach (var item in notaFiscal.Itens)
            {
                item.CalcularValores();
            }
            notaFiscal.CalcularTotais();

            _context.NotasFiscais.Add(notaFiscal);
            await _context.SaveChangesAsync();

            return notaFiscal;
        }

        public async Task<NotaFiscal> AtualizarAsync(NotaFiscal notaFiscal)
        {
            var notaExistente = await _context.NotasFiscais
                .Include(nf => nf.Itens)
                .FirstOrDefaultAsync(nf => nf.Id == notaFiscal.Id);

            if (notaExistente == null)
                throw new ArgumentException("Nota fiscal não encontrada");

            if (!notaExistente.PodeEditar())
                throw new InvalidOperationException("Nota fiscal não pode ser editada");

            // Atualizar propriedades
            notaExistente.DataEmissao = notaFiscal.DataEmissao;
            notaExistente.DataSaida = notaFiscal.DataSaida;
            notaExistente.ClienteId = notaFiscal.ClienteId;
            notaExistente.FornecedorId = notaFiscal.FornecedorId;
            notaExistente.NaturezaOperacao = notaFiscal.NaturezaOperacao;
            notaExistente.CFOP = notaFiscal.CFOP;
            notaExistente.ValorFrete = notaFiscal.ValorFrete;
            notaExistente.ValorSeguro = notaFiscal.ValorSeguro;
            notaExistente.ValorDesconto = notaFiscal.ValorDesconto;
            notaExistente.ValorOutrasDespesas = notaFiscal.ValorOutrasDespesas;
            notaExistente.InformacaoComplementar = notaFiscal.InformacaoComplementar;
            notaExistente.ObservacaoFisco = notaFiscal.ObservacaoFisco;
            notaExistente.DataAlteracao = DateTime.Now;
            notaExistente.UsuarioAlteracaoId = notaFiscal.UsuarioAlteracaoId;

            // Atualizar itens
            _context.NotasFiscaisItens.RemoveRange(notaExistente.Itens);
            
            foreach (var item in notaFiscal.Itens)
            {
                item.NotaFiscalId = notaExistente.Id;
                item.CalcularValores();
                notaExistente.Itens.Add(item);
            }

            // Recalcular totais
            notaExistente.CalcularTotais();

            await _context.SaveChangesAsync();
            return notaExistente;
        }

        public async Task<bool> ExcluirAsync(int id, int empresaId)
        {
            var notaFiscal = await _context.NotasFiscais
                .FirstOrDefaultAsync(nf => nf.Id == id && nf.EmpresaId == empresaId);

            if (notaFiscal == null)
                return false;

            if (!notaFiscal.PodeEditar())
                throw new InvalidOperationException("Nota fiscal não pode ser excluída");

            _context.NotasFiscais.Remove(notaFiscal);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<NotaFiscal> CriarDeVendaAsync(int vendaId, int empresaId)
        {
            var venda = await _context.Vendas
                .Include(v => v.Cliente)
                .Include(v => v.Itens)
                    .ThenInclude(i => i.Produto)
                .FirstOrDefaultAsync(v => v.Id == vendaId && v.EmpresaId == empresaId);

            if (venda == null)
                throw new ArgumentException("Venda não encontrada");

            var notaFiscal = new NotaFiscal
            {
                EmpresaId = empresaId,
                Tipo = "Saida",
                Status = "Rascunho",
                DataEmissao = DateTime.Now,
                DataSaida = DateTime.Now,
                ClienteId = venda.ClienteId,
                VendaId = venda.Id,
                NaturezaOperacao = "Venda",
                CFOP = "5102", // Venda de mercadoria adquirida ou recebida de terceiros
                Serie = "1",
                UsuarioCriacaoId = 1 // TODO: Obter do contexto
            };

            // Criar itens da nota fiscal baseados na venda
            foreach (var itemVenda in venda.Itens)
            {
                var itemNF = new NotaFiscalItem
                {
                    ProdutoId = itemVenda.ProdutoId,
                    Descricao = itemVenda.Produto.Descricao,
                    Unidade = "UN",
                    Quantidade = itemVenda.Quantidade,
                    ValorUnitario = itemVenda.ValorUnitario,
                    CFOP = "5102",
                    NCM = "00000000", // TODO: Obter do produto
                    CST = "00", // Tributada integralmente
                    AliquotaICMS = 18, // TODO: Configurável
                    AliquotaPIS = 1.65m,
                    AliquotaCOFINS = 7.6m
                };

                itemNF.CalcularValores();
                notaFiscal.Itens.Add(itemNF);
            }

            return await CriarAsync(notaFiscal);
        }

        public async Task<NotaFiscal> CriarDeCompraAsync(int compraId, int empresaId)
        {
            var compra = await _context.Compras
                .Include(c => c.Fornecedor)
                .Include(c => c.Itens)
                    .ThenInclude(i => i.Produto)
                .FirstOrDefaultAsync(c => c.Id == compraId && c.EmpresaId == empresaId);

            if (compra == null)
                throw new ArgumentException("Compra não encontrada");

            var notaFiscal = new NotaFiscal
            {
                EmpresaId = empresaId,
                Tipo = "Entrada",
                Status = "Rascunho",
                DataEmissao = DateTime.Now,
                FornecedorId = compra.FornecedorId,
                CompraId = compra.Id,
                NaturezaOperacao = "Compra",
                CFOP = "1102", // Compra para comercialização
                Serie = "1",
                UsuarioCriacaoId = 1 // TODO: Obter do contexto
            };

            // Criar itens da nota fiscal baseados na compra
            foreach (var itemCompra in compra.Itens)
            {
                var itemNF = new NotaFiscalItem
                {
                    ProdutoId = itemCompra.ProdutoId,
                    Descricao = itemCompra.Produto.Descricao,
                    Unidade = "UN",
                    Quantidade = itemCompra.Quantidade,
                    ValorUnitario = itemCompra.ValorUnitario,
                    CFOP = "1102",
                    NCM = "00000000", // TODO: Obter do produto
                    CST = "00", // Tributada integralmente
                    AliquotaICMS = 18, // TODO: Configurável
                    AliquotaPIS = 1.65m,
                    AliquotaCOFINS = 7.6m
                };

                itemNF.CalcularValores();
                notaFiscal.Itens.Add(itemNF);
            }

            return await CriarAsync(notaFiscal);
        }

        public async Task<string> GerarProximoNumeroAsync(string serie, string tipo, int empresaId)
        {
            var ultimaNota = await _context.NotasFiscais
                .Where(nf => nf.EmpresaId == empresaId && nf.Serie == serie && nf.Tipo == tipo)
                .OrderByDescending(nf => nf.Numero)
                .FirstOrDefaultAsync();

            if (ultimaNota == null)
                return "1";

            if (int.TryParse(ultimaNota.Numero, out int ultimoNumero))
                return (ultimoNumero + 1).ToString();

            return "1";
        }

        public async Task<bool> AutorizarAsync(int id, int empresaId)
        {
            var notaFiscal = await _context.NotasFiscais
                .FirstOrDefaultAsync(nf => nf.Id == id && nf.EmpresaId == empresaId);

            if (notaFiscal == null || notaFiscal.Status != "Rascunho")
                return false;

            // Validar nota fiscal
            if (!await ValidarNotaFiscalAsync(notaFiscal))
                return false;

            // Simular autorização (em produção, integrar com SEFAZ)
            notaFiscal.Status = "Autorizada";
            notaFiscal.DataAutorizacao = DateTime.Now;
            notaFiscal.ChaveAcesso = GerarChaveAcesso(notaFiscal);
            notaFiscal.Protocolo = DateTime.Now.ToString("yyyyMMddHHmmss");

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CancelarAsync(int id, string justificativa, int empresaId)
        {
            var notaFiscal = await _context.NotasFiscais
                .FirstOrDefaultAsync(nf => nf.Id == id && nf.EmpresaId == empresaId);

            if (notaFiscal == null || !notaFiscal.PodeCancelar())
                return false;

            // Simular cancelamento (em produção, integrar com SEFAZ)
            notaFiscal.Status = "Cancelada";
            notaFiscal.ObservacaoFisco = $"CANCELADA: {justificativa}";

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<NotaFiscal>> ObterPorPeriodoAsync(DateTime dataInicio, DateTime dataFim, int empresaId)
        {
            return await _context.NotasFiscais
                .Include(nf => nf.Cliente)
                .Include(nf => nf.Fornecedor)
                .Where(nf => nf.EmpresaId == empresaId && 
                            nf.DataEmissao >= dataInicio && 
                            nf.DataEmissao <= dataFim)
                .OrderByDescending(nf => nf.DataEmissao)
                .ToListAsync();
        }

        public async Task<IEnumerable<NotaFiscal>> ObterPorStatusAsync(string status, int empresaId)
        {
            return await _context.NotasFiscais
                .Include(nf => nf.Cliente)
                .Include(nf => nf.Fornecedor)
                .Where(nf => nf.EmpresaId == empresaId && nf.Status == status)
                .OrderByDescending(nf => nf.DataEmissao)
                .ToListAsync();
        }

        public async Task<IEnumerable<NotaFiscal>> ObterPorClienteAsync(int clienteId, int empresaId)
        {
            return await _context.NotasFiscais
                .Where(nf => nf.EmpresaId == empresaId && nf.ClienteId == clienteId)
                .OrderByDescending(nf => nf.DataEmissao)
                .ToListAsync();
        }

        public async Task<IEnumerable<NotaFiscal>> ObterPorFornecedorAsync(int fornecedorId, int empresaId)
        {
            return await _context.NotasFiscais
                .Where(nf => nf.EmpresaId == empresaId && nf.FornecedorId == fornecedorId)
                .OrderByDescending(nf => nf.DataEmissao)
                .ToListAsync();
        }

        public async Task<decimal> ObterTotalVendasPeriodoAsync(DateTime dataInicio, DateTime dataFim, int empresaId)
        {
            return await _context.NotasFiscais
                .Where(nf => nf.EmpresaId == empresaId && 
                            nf.Tipo == "Saida" && 
                            nf.Status == "Autorizada" &&
                            nf.DataEmissao >= dataInicio && 
                            nf.DataEmissao <= dataFim)
                .SumAsync(nf => nf.ValorTotal);
        }

        public async Task<decimal> ObterTotalComprasPeriodoAsync(DateTime dataInicio, DateTime dataFim, int empresaId)
        {
            return await _context.NotasFiscais
                .Where(nf => nf.EmpresaId == empresaId && 
                            nf.Tipo == "Entrada" && 
                            nf.Status == "Autorizada" &&
                            nf.DataEmissao >= dataInicio && 
                            nf.DataEmissao <= dataFim)
                .SumAsync(nf => nf.ValorTotal);
        }

        public async Task<IEnumerable<dynamic>> ObterResumoImpostosAsync(DateTime dataInicio, DateTime dataFim, int empresaId)
        {
            return await _context.NotasFiscais
                .Where(nf => nf.EmpresaId == empresaId && 
                            nf.Status == "Autorizada" &&
                            nf.DataEmissao >= dataInicio && 
                            nf.DataEmissao <= dataFim)
                .GroupBy(nf => nf.Tipo)
                .Select(g => new
                {
                    Tipo = g.Key,
                    Quantidade = g.Count(),
                    ValorTotal = g.Sum(nf => nf.ValorTotal),
                    ValorICMS = g.Sum(nf => nf.ValorICMS),
                    ValorIPI = g.Sum(nf => nf.ValorIPI),
                    ValorPIS = g.Sum(nf => nf.ValorPIS),
                    ValorCOFINS = g.Sum(nf => nf.ValorCOFINS)
                })
                .ToListAsync();
        }

        public async Task<bool> ValidarNotaFiscalAsync(NotaFiscal notaFiscal)
        {
            var erros = await ObterErrosValidacaoAsync(notaFiscal);
            return !erros.Any();
        }

        public async Task<List<string>> ObterErrosValidacaoAsync(NotaFiscal notaFiscal)
        {
            var erros = new List<string>();

            // Validações básicas
            if (string.IsNullOrEmpty(notaFiscal.Numero))
                erros.Add("Número da nota fiscal é obrigatório");

            if (string.IsNullOrEmpty(notaFiscal.NaturezaOperacao))
                erros.Add("Natureza da operação é obrigatória");

            if (string.IsNullOrEmpty(notaFiscal.CFOP))
                erros.Add("CFOP é obrigatório");

            if (notaFiscal.Tipo == "Saida" && notaFiscal.ClienteId == null)
                erros.Add("Cliente é obrigatório para notas de saída");

            if (notaFiscal.Tipo == "Entrada" && notaFiscal.FornecedorId == null)
                erros.Add("Fornecedor é obrigatório para notas de entrada");

            if (!notaFiscal.Itens.Any())
                erros.Add("Nota fiscal deve ter pelo menos um item");

            // Validar se número já existe
            if (await ExisteNumeroAsync(notaFiscal.Numero, notaFiscal.Serie, notaFiscal.Tipo, notaFiscal.EmpresaId, notaFiscal.Id))
                erros.Add("Já existe uma nota fiscal com este número e série");

            return erros;
        }

        public async Task<bool> ExisteNumeroAsync(string numero, string serie, string tipo, int empresaId, int? excludeId = null)
        {
            var query = _context.NotasFiscais
                .Where(nf => nf.EmpresaId == empresaId && 
                            nf.Numero == numero && 
                            nf.Serie == serie && 
                            nf.Tipo == tipo);

            if (excludeId.HasValue)
                query = query.Where(nf => nf.Id != excludeId.Value);

            return await query.AnyAsync();
        }

        public async Task<NotaFiscal?> ObterPorChaveAcessoAsync(string chaveAcesso, int empresaId)
        {
            return await _context.NotasFiscais
                .Include(nf => nf.Cliente)
                .Include(nf => nf.Fornecedor)
                .Include(nf => nf.Itens)
                .FirstOrDefaultAsync(nf => nf.EmpresaId == empresaId && nf.ChaveAcesso == chaveAcesso);
        }

        private string GerarChaveAcesso(NotaFiscal notaFiscal)
        {
            // Simulação de chave de acesso (em produção, seguir padrão SEFAZ)
            var random = new Random();
            return $"{notaFiscal.EmpresaId:D2}{notaFiscal.DataEmissao:yyMM}{notaFiscal.Serie:D3}{notaFiscal.Numero:D9}{random.Next(10000000, 99999999)}";
        }
    }
}