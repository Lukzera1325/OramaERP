using Microsoft.AspNetCore.Mvc;
using Orama.Application.Services;
using Orama.Web.Models;

namespace Orama.Web.Controllers;

/// <summary>
/// Controller para gerenciamento de clientes
/// </summary>
public class ClientesController : BaseController
{
    private readonly IClienteService _clienteService;

    public ClientesController(IClienteService clienteService)
    {
        _clienteService = clienteService;
    }

    /// <summary>
    /// Lista todos os clientes
    /// </summary>
    public async Task<IActionResult> Index()
    {
        if (!TemPermissao("Clientes.Visualizar"))
        {
            return AccessDenied();
        }

        var clientes = await _clienteService.ObterTodosAsync();
        var viewModel = clientes.Select(ClienteViewModel.FromEntity);

        ViewData["Title"] = "Clientes";
        return View(viewModel);
    }

    /// <summary>
    /// Exibe detalhes de um cliente
    /// </summary>
    public async Task<IActionResult> Details(int id)
    {
        if (!TemPermissao("Clientes.Visualizar"))
        {
            return AccessDenied();
        }

        var cliente = await _clienteService.ObterPorIdAsync(id);
        if (cliente == null)
        {
            return NotFound();
        }

        var viewModel = ClienteViewModel.FromEntity(cliente);
        ViewData["Title"] = "Detalhes do Cliente";
        return View(viewModel);
    }

    /// <summary>
    /// Exibe formulário para criar novo cliente
    /// </summary>
    public IActionResult Create()
    {
        if (!TemPermissao("Clientes.Incluir"))
        {
            return AccessDenied();
        }

        ViewData["Title"] = "Novo Cliente";
        return View(new ClienteViewModel());
    }

    /// <summary>
    /// Processa criação de novo cliente
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ClienteViewModel model)
    {
        if (!TemPermissao("Clientes.Incluir"))
        {
            return AccessDenied();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var cliente = model.ToEntity();
            cliente.EmpresaId = UsuarioLogado?.EmpresaId ?? 0;
            await _clienteService.CriarAsync(cliente);
            AdicionarMensagemSucesso("Cliente criado com sucesso!");
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    /// <summary>
    /// Exibe formulário para editar cliente
    /// </summary>
    public async Task<IActionResult> Edit(int id)
    {
        if (!TemPermissao("Clientes.Alterar"))
        {
            return AccessDenied();
        }

        var cliente = await _clienteService.ObterPorIdAsync(id);
        if (cliente == null)
        {
            return NotFound();
        }

        var viewModel = ClienteViewModel.FromEntity(cliente);
        ViewData["Title"] = "Editar Cliente";
        return View(viewModel);
    }

    /// <summary>
    /// Processa edição de cliente
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ClienteViewModel model)
    {
        if (!TemPermissao("Clientes.Alterar"))
        {
            return AccessDenied();
        }

        if (id != model.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var cliente = model.ToEntity();
            cliente.EmpresaId = UsuarioLogado?.EmpresaId ?? 0;
            await _clienteService.AtualizarAsync(cliente);
            AdicionarMensagemSucesso("Cliente atualizado com sucesso!");
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    /// <summary>
    /// Exibe confirmação para excluir cliente
    /// </summary>
    public async Task<IActionResult> Delete(int id)
    {
        if (!TemPermissao("Clientes.Excluir"))
        {
            return AccessDenied();
        }

        var cliente = await _clienteService.ObterPorIdAsync(id);
        if (cliente == null)
        {
            return NotFound();
        }

        var viewModel = ClienteViewModel.FromEntity(cliente);
        ViewData["Title"] = "Excluir Cliente";
        return View(viewModel);
    }

    /// <summary>
    /// Processa exclusão de cliente
    /// </summary>
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (!TemPermissao("Clientes.Excluir"))
        {
            return AccessDenied();
        }

        try
        {
            await _clienteService.ExcluirAsync(id);
            AdicionarMensagemSucesso("Cliente excluído com sucesso!");
        }
        catch (Exception ex)
        {
            AdicionarMensagemErro($"Erro ao excluir cliente: {ex.Message}");
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Busca clientes via AJAX
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Buscar(string termo)
    {
        if (!TemPermissao("Clientes.Visualizar"))
        {
            return Json(new { success = false, message = "Sem permissão" });
        }

        try
        {
            var clientes = await _clienteService.BuscarAsync(termo ?? string.Empty);
            var resultado = clientes.Select(c => new
            {
                id = c.Id,
                nome = c.Nome,
                cpfCnpj = c.CpfCnpjFormatado,
                email = c.Email,
                telefone = c.Telefone
            });

            return Json(new { success = true, data = resultado });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Consulta CNPJ na Receita Federal via AJAX
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> ConsultarCnpj([FromBody] ConsultaCnpjRequest request)
    {
        if (!TemPermissao("Clientes.Incluir") && !TemPermissao("Clientes.Alterar"))
        {
            return Json(new { success = false, message = "Sem permissão" });
        }

        try
        {
            var dados = await _clienteService.ConsultarCnpjAsync(request.Cnpj);
            if (dados == null)
            {
                return Json(new { success = false, message = "CNPJ não encontrado ou inválido" });
            }

            return Json(new { success = true, data = dados });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"Erro ao consultar CNPJ: {ex.Message}" });
        }
    }

    /// <summary>
    /// Consulta CEP via AJAX
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> ConsultarCep([FromBody] ConsultaCepRequest request)
    {
        try
        {
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync($"https://viacep.com.br/ws/{request.Cep}/json/");
            
            if (!response.IsSuccessStatusCode)
            {
                return Json(new { success = false, message = "CEP não encontrado" });
            }

            var json = await response.Content.ReadAsStringAsync();
            var dados = System.Text.Json.JsonSerializer.Deserialize<ViaCepResponse>(json, new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (dados?.Erro == true)
            {
                return Json(new { success = false, message = "CEP não encontrado" });
            }

            return Json(new { 
                success = true, 
                data = new {
                    logradouro = dados?.Logradouro,
                    bairro = dados?.Bairro,
                    cidade = dados?.Localidade,
                    uf = dados?.Uf
                }
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"Erro ao consultar CEP: {ex.Message}" });
        }
    }
}

/// <summary>
/// Request para consulta de CNPJ
/// </summary>
public class ConsultaCnpjRequest
{
    public string Cnpj { get; set; } = string.Empty;
}

/// <summary>
/// Request para consulta de CEP
/// </summary>
public class ConsultaCepRequest
{
    public string Cep { get; set; } = string.Empty;
}

/// <summary>
/// Response da API ViaCEP
/// </summary>
public class ViaCepResponse
{
    public string? Cep { get; set; }
    public string? Logradouro { get; set; }
    public string? Complemento { get; set; }
    public string? Bairro { get; set; }
    public string? Localidade { get; set; }
    public string? Uf { get; set; }
    public bool Erro { get; set; }
}