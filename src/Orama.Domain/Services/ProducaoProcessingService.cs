using Orama.Domain.Entities;

namespace Orama.Domain.Services;

/// <summary>
/// Domain Service para lógica complexa de produção
/// Contém regras de negócio que envolvem múltiplas entidades
/// </summary>
public class ProducaoProcessingService
{
    /// <summary>
    /// Valida se uma ordem de produção pode ser criada
    /// </summary>
    public (bool Valida, List<string> Erros) ValidarOrdemProducao(
        Produto produto, 
        decimal quantidade, 
        IEnumerable<EstruturaProduto> estrutura)
    {
        var erros = new List<string>();

        // Validar se produto pode ser produzido
        if (!produto.EhProduzivel())
        {
            erros.Add($"Produto '{produto.Descricao}' não é produzível");
        }

        // Validar se tem estrutura definida
        if (!estrutura.Any())
        {
            erros.Add($"Produto '{produto.Descricao}' não possui estrutura de produção definida");
        }

        // Validar estoque dos componentes
        foreach (var item in estrutura)
        {
            if (!item.ComponenteTemEstoqueSuficiente(quantidade))
            {
                var necessario = item.CalcularQuantidadeTotal(quantidade);
                var disponivel = item.ProdutoComponente.EstoqueAtual;
                
                erros.Add($"Estoque insuficiente do componente '{item.ProdutoComponente.Descricao}'. " +
                         $"Necessário: {necessario:N2}, Disponível: {disponivel:N2}");
            }
        }

        return (erros.Count == 0, erros);
    }

    /// <summary>
    /// Calcula o custo total de produção
    /// </summary>
    public decimal CalcularCustoProducao(
        decimal quantidadeProduzir, 
        IEnumerable<EstruturaProduto> estrutura)
    {
        decimal custoTotal = 0;

        foreach (var item in estrutura)
        {
            var quantidadeNecessaria = item.CalcularQuantidadeTotal(quantidadeProduzir);
            var custoComponente = quantidadeNecessaria * item.ProdutoComponente.PrecoCusto;
            custoTotal += custoComponente;
        }

        return custoTotal;
    }

    /// <summary>
    /// Cria os itens da ordem de produção baseado na estrutura
    /// </summary>
    public List<OrdemProducaoItem> CriarItensOrdemProducao(
        int ordemProducaoId,
        decimal quantidadeProduzir,
        IEnumerable<EstruturaProduto> estrutura)
    {
        var itens = new List<OrdemProducaoItem>();

        foreach (var estruturaItem in estrutura)
        {
            var item = new OrdemProducaoItem
            {
                OrdemProducaoId = ordemProducaoId,
                ProdutoId = estruturaItem.ProdutoComponenteId,
                QuantidadePlanejada = estruturaItem.CalcularQuantidadeTotal(quantidadeProduzir),
                CustoUnitario = estruturaItem.ProdutoComponente.PrecoCusto
            };

            itens.Add(item);
        }

        return itens;
    }

    /// <summary>
    /// Gera número sequencial para ordem de produção
    /// </summary>
    public string GerarNumeroOP(int empresaId, int proximoNumero)
    {
        var ano = DateTime.Now.Year;
        return $"OP{empresaId:D3}{ano}{proximoNumero:D6}";
    }
}