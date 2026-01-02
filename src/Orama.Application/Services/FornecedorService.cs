using Microsoft.EntityFrameworkCore;
using Orama.Domain.Entities;
using Orama.Infra.Data.Context;

namespace Orama.Application.Services;

/// <summary>
/// Serviço para gerenciamento de fornecedores
/// </summary>
public class FornecedorService : IFornecedorService
{
    private readonly OramaDbContext _context;
    private readonly HttpClient _httpClient;

    public FornecedorService(OramaDbContext context, HttpClient httpClient)
    {
        _context = context;
        _httpClient = httpClient;
    }

    /// <summary>
    /// Obtém todos os fornecedores ativos de uma empresa
    /// </summary>
    public async Task<IEnumerable<Fornecedor>> ObterTodosAsync(int empresaId)
    {
        return await _context.Fornecedores
            .Where(f => f.EmpresaId == empresaId && f.Ativo)
            .OrderBy(f => f.Nome)
            .ToListAsync();
    }

    /// <summary>
    /// Obtém um fornecedor por ID
    /// </summary>
    public async Task<Fornecedor?> ObterPorIdAsync(int id, int empresaId)
    {
        return await _context.Fornecedores
            .FirstOrDefaultAsync(f => f.Id == id && f.EmpresaId == empresaId && f.Ativo);
    }

    /// <summary>
    /// Obtém um fornecedor por CNPJ
    /// </summary>
    public async Task<Fornecedor?> ObterPorCnpjAsync(string cnpj, int empresaId)
    {
        return await _context.Fornecedores
            .FirstOrDefaultAsync(f => f.CpfCnpj == cnpj && f.EmpresaId == empresaId && f.Ativo);
    }

    /// <summary>
    /// Inclui um novo fornecedor
    /// </summary>
    public async Task<Fornecedor> IncluirAsync(Fornecedor fornecedor)
    {
        // Verificar se CNPJ já está cadastrado
        if (await CnpjJaCadastradoAsync(fornecedor.CpfCnpj, fornecedor.EmpresaId))
            throw new InvalidOperationException($"CNPJ {fornecedor.CpfCnpj} já está cadastrado para outro fornecedor");

        _context.Fornecedores.Add(fornecedor);
        await _context.SaveChangesAsync();
        return fornecedor;
    }

    /// <summary>
    /// Altera um fornecedor existente
    /// </summary>
    public async Task<Fornecedor> AlterarAsync(Fornecedor fornecedor)
    {
        var fornecedorExistente = await _context.Fornecedores.FindAsync(fornecedor.Id);
        if (fornecedorExistente == null)
            throw new InvalidOperationException("Fornecedor não encontrado");

        // Verificar se CNPJ já está cadastrado para outro fornecedor
        if (await CnpjJaCadastradoAsync(fornecedor.CpfCnpj, fornecedor.EmpresaId, fornecedor.Id))
            throw new InvalidOperationException($"CNPJ {fornecedor.CpfCnpj} já está cadastrado para outro fornecedor");

        // Atualizar campos
        fornecedorExistente.Nome = fornecedor.Nome;
        fornecedorExistente.NomeFantasia = fornecedor.NomeFantasia;
        fornecedorExistente.TipoPessoa = fornecedor.TipoPessoa;
        fornecedorExistente.CpfCnpj = fornecedor.CpfCnpj;
        fornecedorExistente.RgIe = fornecedor.RgIe;
        fornecedorExistente.Email = fornecedor.Email;
        fornecedorExistente.Telefone = fornecedor.Telefone;
        fornecedorExistente.Celular = fornecedor.Celular;
        fornecedorExistente.Cep = fornecedor.Cep;
        fornecedorExistente.Logradouro = fornecedor.Logradouro;
        fornecedorExistente.Numero = fornecedor.Numero;
        fornecedorExistente.Complemento = fornecedor.Complemento;
        fornecedorExistente.Bairro = fornecedor.Bairro;
        fornecedorExistente.Cidade = fornecedor.Cidade;
        fornecedorExistente.Uf = fornecedor.Uf;
        fornecedorExistente.Observacoes = fornecedor.Observacoes;
        fornecedorExistente.MarcarComoAtualizada();

        await _context.SaveChangesAsync();
        return fornecedorExistente;
    }

    /// <summary>
    /// Exclui um fornecedor (soft delete)
    /// </summary>
    public async Task<bool> ExcluirAsync(int id, int empresaId)
    {
        var fornecedor = await _context.Fornecedores
            .FirstOrDefaultAsync(f => f.Id == id && f.EmpresaId == empresaId);
        
        if (fornecedor == null)
            return false;

        // Verificar se tem compras vinculadas
        var temCompras = await _context.Compras
            .AnyAsync(c => c.FornecedorId == id && c.EmpresaId == empresaId);
        
        if (temCompras)
            throw new InvalidOperationException("Não é possível excluir este fornecedor pois existem compras vinculadas a ele");

        fornecedor.Ativo = false;
        fornecedor.MarcarComoAtualizada();
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Verifica se um CNPJ já está cadastrado
    /// </summary>
    public async Task<bool> CnpjJaCadastradoAsync(string cnpj, int empresaId, int? fornecedorId = null)
    {
        var query = _context.Fornecedores
            .Where(f => f.CpfCnpj == cnpj && f.EmpresaId == empresaId && f.Ativo);

        if (fornecedorId.HasValue)
            query = query.Where(f => f.Id != fornecedorId.Value);

        return await query.AnyAsync();
    }

    /// <summary>
    /// Consulta dados do CNPJ na Receita Federal
    /// </summary>
    public async Task<DadosCnpj?> ConsultarCnpjAsync(string cnpj)
    {
        try
        {
            // Limpar formatação do CNPJ
            cnpj = cnpj.Replace(".", "").Replace("/", "").Replace("-", "");

            if (cnpj.Length != 14)
                return null;

            var url = $"https://www.receitaws.com.br/v1/cnpj/{cnpj}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            var dados = System.Text.Json.JsonSerializer.Deserialize<DadosCnpj>(json, new System.Text.Json.JsonSerializerOptions
            {
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.SnakeCaseLower
            });

            return dados;
        }
        catch
        {
            return null;
        }
    }
}