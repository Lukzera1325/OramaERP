using Microsoft.EntityFrameworkCore;
using Orama.Domain.Entities;
using Orama.Infra.Data.Context;
using System.Text.Json;

namespace Orama.Application.Services;

/// <summary>
/// Serviço para gerenciamento de clientes
/// </summary>
public class ClienteService : IClienteService
{
    private readonly OramaDbContext _context;
    private readonly HttpClient _httpClient;

    public ClienteService(OramaDbContext context, HttpClient httpClient)
    {
        _context = context;
        _httpClient = httpClient;
    }

    /// <summary>
    /// Obtém todos os clientes ativos
    /// </summary>
    public async Task<IEnumerable<Cliente>> ObterTodosAsync()
    {
        return await _context.Clientes
            .Where(c => c.Ativo)
            .OrderBy(c => c.Nome)
            .ToListAsync();
    }

    /// <summary>
    /// Obtém todos os clientes ativos por empresa
    /// </summary>
    public async Task<IEnumerable<Cliente>> ObterTodosAsync(int empresaId)
    {
        return await _context.Clientes
            .Where(c => c.Ativo && c.EmpresaId == empresaId)
            .OrderBy(c => c.Nome)
            .ToListAsync();
    }

    /// <summary>
    /// Obtém um cliente por ID
    /// </summary>
    public async Task<Cliente?> ObterPorIdAsync(int id)
    {
        return await _context.Clientes
            .FirstOrDefaultAsync(c => c.Id == id && c.Ativo);
    }

    /// <summary>
    /// Obtém um cliente por ID e empresa
    /// </summary>
    public async Task<Cliente?> ObterPorIdAsync(int id, int empresaId)
    {
        return await _context.Clientes
            .FirstOrDefaultAsync(c => c.Id == id && c.EmpresaId == empresaId && c.Ativo);
    }

    /// <summary>
    /// Obtém um cliente por CPF/CNPJ
    /// </summary>
    public async Task<Cliente?> ObterPorCpfCnpjAsync(string cpfCnpj)
    {
        var numerosSomenteNumeros = new string(cpfCnpj.Where(char.IsDigit).ToArray());
        
        return await _context.Clientes
            .FirstOrDefaultAsync(c => c.CpfCnpj.Replace(".", "").Replace("-", "").Replace("/", "") == numerosSomenteNumeros && c.Ativo);
    }

    /// <summary>
    /// Obtém um cliente por CPF/CNPJ e empresa
    /// </summary>
    public async Task<Cliente?> ObterPorCpfCnpjAsync(string cpfCnpj, int empresaId)
    {
        var numerosSomenteNumeros = new string(cpfCnpj.Where(char.IsDigit).ToArray());
        
        return await _context.Clientes
            .FirstOrDefaultAsync(c => c.CpfCnpj.Replace(".", "").Replace("-", "").Replace("/", "") == numerosSomenteNumeros && c.EmpresaId == empresaId && c.Ativo);
    }

    /// <summary>
    /// Cria um novo cliente
    /// </summary>
    public async Task<Cliente> CriarAsync(Cliente cliente)
    {
        // Verificar se CPF/CNPJ já existe
        if (await CpfCnpjExisteAsync(cliente.CpfCnpj))
        {
            throw new InvalidOperationException("CPF/CNPJ já está em uso por outro cliente.");
        }

        // Limpar CPF/CNPJ (manter apenas números)
        cliente.CpfCnpj = new string(cliente.CpfCnpj.Where(char.IsDigit).ToArray());
        
        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();
        
        return cliente;
    }

    /// <summary>
    /// Atualiza um cliente existente
    /// </summary>
    public async Task<Cliente> AtualizarAsync(Cliente cliente)
    {
        var clienteExistente = await _context.Clientes.FindAsync(cliente.Id);
        if (clienteExistente == null)
        {
            throw new InvalidOperationException("Cliente não encontrado.");
        }

        // Verificar se CPF/CNPJ já existe (exceto para o próprio cliente)
        if (await CpfCnpjExisteAsync(cliente.CpfCnpj, cliente.Id))
        {
            throw new InvalidOperationException("CPF/CNPJ já está em uso por outro cliente.");
        }

        // Limpar CPF/CNPJ (manter apenas números)
        cliente.CpfCnpj = new string(cliente.CpfCnpj.Where(char.IsDigit).ToArray());

        // Atualizar propriedades
        clienteExistente.Nome = cliente.Nome;
        clienteExistente.NomeFantasia = cliente.NomeFantasia;
        clienteExistente.TipoPessoa = cliente.TipoPessoa;
        clienteExistente.CpfCnpj = cliente.CpfCnpj;
        clienteExistente.RgIe = cliente.RgIe;
        clienteExistente.Email = cliente.Email;
        clienteExistente.Telefone = cliente.Telefone;
        clienteExistente.Celular = cliente.Celular;
        clienteExistente.Cep = cliente.Cep;
        clienteExistente.Logradouro = cliente.Logradouro;
        clienteExistente.Numero = cliente.Numero;
        clienteExistente.Complemento = cliente.Complemento;
        clienteExistente.Bairro = cliente.Bairro;
        clienteExistente.Cidade = cliente.Cidade;
        clienteExistente.Uf = cliente.Uf;
        clienteExistente.LimiteCredito = cliente.LimiteCredito;
        clienteExistente.Observacoes = cliente.Observacoes;

        await _context.SaveChangesAsync();
        return clienteExistente;
    }

    /// <summary>
    /// Exclui um cliente (soft delete)
    /// </summary>
    public async Task ExcluirAsync(int id)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente != null)
        {
            cliente.Ativo = false;
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Verifica se um CPF/CNPJ já está em uso
    /// </summary>
    public async Task<bool> CpfCnpjExisteAsync(string cpfCnpj, int? clienteId = null)
    {
        var numerosSomenteNumeros = new string(cpfCnpj.Where(char.IsDigit).ToArray());
        
        var query = _context.Clientes.Where(c => 
            c.CpfCnpj.Replace(".", "").Replace("-", "").Replace("/", "") == numerosSomenteNumeros && 
            c.Ativo);
        
        if (clienteId.HasValue)
        {
            query = query.Where(c => c.Id != clienteId.Value);
        }

        return await query.AnyAsync();
    }

    /// <summary>
    /// Busca clientes por nome ou CPF/CNPJ
    /// </summary>
    public async Task<IEnumerable<Cliente>> BuscarAsync(string termo)
    {
        if (string.IsNullOrWhiteSpace(termo))
            return await ObterTodosAsync();

        var termoLower = termo.ToLower();
        var numerosSomenteNumeros = new string(termo.Where(char.IsDigit).ToArray());

        return await _context.Clientes
            .Where(c => c.Ativo && (
                c.Nome.ToLower().Contains(termoLower) ||
                (c.NomeFantasia != null && c.NomeFantasia.ToLower().Contains(termoLower)) ||
                c.CpfCnpj.Contains(numerosSomenteNumeros)
            ))
            .OrderBy(c => c.Nome)
            .ToListAsync();
    }

    /// <summary>
    /// Consulta dados do CNPJ na Receita Federal
    /// </summary>
    public async Task<DadosCnpj?> ConsultarCnpjAsync(string cnpj)
    {
        try
        {
            // Limpar CNPJ (manter apenas números)
            var cnpjLimpo = new string(cnpj.Where(char.IsDigit).ToArray());
            
            if (cnpjLimpo.Length != 14)
                return null;

            // Usar API pública da ReceitaWS
            var url = $"https://www.receitaws.com.br/v1/cnpj/{cnpjLimpo}";
            
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            var dados = JsonSerializer.Deserialize<ReceitaWsResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (dados == null || dados.Status == "ERROR")
                return null;

            return new DadosCnpj
            {
                Cnpj = dados.Cnpj ?? string.Empty,
                RazaoSocial = dados.Nome ?? string.Empty,
                NomeFantasia = dados.Fantasia ?? string.Empty,
                Situacao = dados.Situacao ?? string.Empty,
                Email = dados.Email ?? string.Empty,
                Telefone = dados.Telefone ?? string.Empty,
                Cep = dados.Cep ?? string.Empty,
                Logradouro = dados.Logradouro ?? string.Empty,
                Numero = dados.Numero ?? string.Empty,
                Complemento = dados.Complemento ?? string.Empty,
                Bairro = dados.Bairro ?? string.Empty,
                Cidade = dados.Municipio ?? string.Empty,
                Uf = dados.Uf ?? string.Empty
            };
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Classe para deserializar resposta da ReceitaWS
    /// </summary>
    private class ReceitaWsResponse
    {
        public string? Status { get; set; }
        public string? Cnpj { get; set; }
        public string? Nome { get; set; }
        public string? Fantasia { get; set; }
        public string? Situacao { get; set; }
        public string? Email { get; set; }
        public string? Telefone { get; set; }
        public string? Cep { get; set; }
        public string? Logradouro { get; set; }
        public string? Numero { get; set; }
        public string? Complemento { get; set; }
        public string? Bairro { get; set; }
        public string? Municipio { get; set; }
        public string? Uf { get; set; }
    }
}