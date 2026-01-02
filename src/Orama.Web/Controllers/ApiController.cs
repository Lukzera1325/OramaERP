using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Orama.Web.Controllers;

/// <summary>
/// Controller para APIs auxiliares (CNPJ, CEP, etc.)
/// </summary>
[ApiController]
[Route("api")]
public class ApiController : BaseController
{
    private readonly HttpClient _httpClient;

    public ApiController(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Consulta dados de CNPJ
    /// </summary>
    [HttpGet("cnpj/{cnpj}")]
    public async Task<IActionResult> ConsultarCnpj(string cnpj)
    {
        try
        {
            // Limpar CNPJ
            var cnpjLimpo = new string(cnpj.Where(char.IsDigit).ToArray());
            
            if (cnpjLimpo.Length != 14)
            {
                return BadRequest(new { erro = "CNPJ deve ter 14 dígitos" });
            }

            // Tentar múltiplas APIs como fallback
            var resultado = await TentarConsultarCnpj(cnpjLimpo);
            
            if (resultado != null)
            {
                return Ok(resultado);
            }

            return NotFound(new { erro = "CNPJ não encontrado" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { erro = "Erro interno do servidor", detalhes = ex.Message });
        }
    }

    private async Task<object?> TentarConsultarCnpj(string cnpj)
    {
        // Lista de APIs para tentar
        var apis = new[]
        {
            $"https://receitaws.com.br/v1/cnpj/{cnpj}",
            $"https://www.receitaws.com.br/v1/cnpj/{cnpj}",
            $"https://publica.cnpj.ws/cnpj/{cnpj}"
        };

        foreach (var apiUrl in apis)
        {
            try
            {
                _httpClient.Timeout = TimeSpan.FromSeconds(10);
                var response = await _httpClient.GetAsync(apiUrl);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    
                    // Tentar parsear como ReceitaWS primeiro
                    if (apiUrl.Contains("receitaws"))
                    {
                        var data = JsonSerializer.Deserialize<ReceitaWsResponse>(content, new JsonSerializerOptions 
                        { 
                            PropertyNameCaseInsensitive = true 
                        });
                        
                        if (data?.Status == "OK")
                        {
                            return new
                            {
                                status = "OK",
                                nome = data.Nome,
                                fantasia = data.Fantasia,
                                email = data.Email,
                                telefone = data.Telefone,
                                cep = data.Cep,
                                logradouro = data.Logradouro,
                                numero = data.Numero,
                                complemento = data.Complemento,
                                bairro = data.Bairro,
                                municipio = data.Municipio,
                                uf = data.Uf,
                                situacao = data.Situacao,
                                atividade_principal = data.AtividadePrincipal?.FirstOrDefault()?.Text,
                                // Debug info
                                debug = new
                                {
                                    api_usada = "receitaws",
                                    dados_originais = new
                                    {
                                        cep_original = data.Cep,
                                        logradouro_original = data.Logradouro,
                                        bairro_original = data.Bairro,
                                        municipio_original = data.Municipio,
                                        uf_original = data.Uf
                                    }
                                }
                            };
                        }
                    }
                    else if (apiUrl.Contains("publica.cnpj.ws"))
                    {
                        // API alternativa - formato diferente
                        var data = JsonSerializer.Deserialize<CnpjWsResponse>(content, new JsonSerializerOptions 
                        { 
                            PropertyNameCaseInsensitive = true 
                        });
                        
                        if (data?.Status == 200)
                        {
                            return new
                            {
                                status = "OK",
                                nome = data.Company?.Name,
                                fantasia = data.Alias,
                                email = data.Emails?.FirstOrDefault()?.Address,
                                telefone = data.Phones?.FirstOrDefault()?.Number,
                                cep = data.Address?.Zip,
                                logradouro = data.Address?.Street,
                                numero = data.Address?.Number,
                                complemento = data.Address?.Details,
                                bairro = data.Address?.District,
                                municipio = data.Address?.City,
                                uf = data.Address?.State,
                                situacao = data.Status == 200 ? "ATIVA" : "INATIVA"
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log do erro e continua para próxima API
                Console.WriteLine($"Erro ao consultar {apiUrl}: {ex.Message}");
                continue;
            }
        }

        return null;
    }
}

// Classes para deserialização das APIs
public class ReceitaWsResponse
{
    public string Status { get; set; } = "";
    public string Nome { get; set; } = "";
    public string Fantasia { get; set; } = "";
    public string Email { get; set; } = "";
    public string Telefone { get; set; } = "";
    public string Cep { get; set; } = "";
    public string Logradouro { get; set; } = "";
    public string Numero { get; set; } = "";
    public string Complemento { get; set; } = "";
    public string Bairro { get; set; } = "";
    public string Municipio { get; set; } = "";
    public string Uf { get; set; } = "";
    public string Situacao { get; set; } = "";
    public List<AtividadeResponse>? AtividadePrincipal { get; set; }
}

public class AtividadeResponse
{
    public string Text { get; set; } = "";
}

public class CnpjWsResponse
{
    public int Status { get; set; }
    public CompanyResponse? Company { get; set; }
    public string Alias { get; set; } = "";
    public List<EmailResponse>? Emails { get; set; }
    public List<PhoneResponse>? Phones { get; set; }
    public AddressResponse? Address { get; set; }
}

public class CompanyResponse
{
    public string Name { get; set; } = "";
}

public class EmailResponse
{
    public string Address { get; set; } = "";
}

public class PhoneResponse
{
    public string Number { get; set; } = "";
}

public class AddressResponse
{
    public string Zip { get; set; } = "";
    public string Street { get; set; } = "";
    public string Number { get; set; } = "";
    public string Details { get; set; } = "";
    public string District { get; set; } = "";
    public string City { get; set; } = "";
    public string State { get; set; } = "";
}