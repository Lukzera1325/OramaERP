using Microsoft.AspNetCore.Mvc;
using Orama.Application.Services;
using Orama.Web.Models;
using System.Text.Json;

namespace Orama.Web.Controllers;

/// <summary>
/// Controller responsável pela autenticação de usuários
/// </summary>
public class AuthController : Controller
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Exibe a tela de login
    /// </summary>
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        // Se já estiver logado, redirecionar
        if (UsuarioLogado != null)
        {
            return RedirectToAction("Index", "Home");
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    /// <summary>
    /// Processa o login do usuário
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var usuario = await _authService.AutenticarAsync(model.Email, model.Senha);

        if (usuario == null)
        {
            ModelState.AddModelError(string.Empty, "Email ou senha inválidos.");
            return View(model);
        }

        // Obter permissões do usuário
        var permissoes = await _authService.ObterPermissoesUsuarioAsync(usuario.Id);
        
        // Obter empresas do usuário
        var empresas = await _authService.ObterEmpresasUsuarioAsync(usuario.Id);
        var empresasDisponiveis = empresas.Select(e => new EmpresaSimplificada
        {
            Id = e.Id,
            Nome = e.NomeFantasia ?? e.RazaoSocial,
            Cnpj = e.Cnpj
        }).ToList();

        // Se o usuário tem apenas uma empresa, selecionar automaticamente
        var empresaPadrao = empresasDisponiveis.FirstOrDefault();
        if (empresaPadrao == null)
        {
            ModelState.AddModelError(string.Empty, "Usuário não possui acesso a nenhuma empresa.");
            return View(model);
        }

        // Se o usuário tem múltiplas empresas, redirecionar para seleção
        if (empresasDisponiveis.Count > 1)
        {
            var selecionarEmpresaModel = new SelecionarEmpresaViewModel
            {
                Empresas = empresasDisponiveis,
                NomeUsuario = usuario.Nome,
                ReturnUrl = model.ReturnUrl
            };

            // Salvar dados temporários na sessão
            HttpContext.Session.SetString("UsuarioTempLogin", JsonSerializer.Serialize(new
            {
                Id = usuario.Id,
                Nome = usuario.Nome,
                Email = usuario.Email,
                Perfil = usuario.Perfil.Nome,
                UltimoLogin = usuario.UltimoLogin,
                Permissoes = permissoes,
                IsSuperAdmin = usuario.IsSuperAdmin,
                EmpresasDisponiveis = empresasDisponiveis
            }));

            return View("SelecionarEmpresa", selecionarEmpresaModel);
        }

        // Criar objeto do usuário logado
        var usuarioLogado = new UsuarioLogadoViewModel
        {
            Id = usuario.Id,
            Nome = usuario.Nome,
            Email = usuario.Email,
            Perfil = usuario.Perfil.Nome,
            UltimoLogin = usuario.UltimoLogin,
            Permissoes = permissoes,
            IsSuperAdmin = usuario.IsSuperAdmin,
            EmpresaId = empresaPadrao.Id,
            EmpresaNome = empresaPadrao.Nome,
            EmpresasDisponiveis = empresasDisponiveis
        };

        // Salvar na sessão
        HttpContext.Session.SetString("UsuarioLogado", JsonSerializer.Serialize(usuarioLogado));

        // Redirecionar
        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
        {
            return Redirect(model.ReturnUrl);
        }

        return RedirectToAction("Index", "Home");
    }

    /// <summary>
    /// Processa a seleção de empresa
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SelecionarEmpresa(SelecionarEmpresaViewModel model)
    {
        if (!model.EmpresaSelecionada.HasValue)
        {
            ModelState.AddModelError(string.Empty, "Selecione uma empresa para continuar.");
            return View(model);
        }

        // Recuperar dados temporários da sessão
        var usuarioTempJson = HttpContext.Session.GetString("UsuarioTempLogin");
        if (string.IsNullOrEmpty(usuarioTempJson))
        {
            return RedirectToAction("Login");
        }

        var usuarioTemp = JsonSerializer.Deserialize<JsonElement>(usuarioTempJson);
        var empresasJson = usuarioTemp.GetProperty("EmpresasDisponiveis").GetRawText();
        var empresasDisponiveis = JsonSerializer.Deserialize<List<EmpresaSimplificada>>(empresasJson);

        var empresaSelecionada = empresasDisponiveis?.FirstOrDefault(e => e.Id == model.EmpresaSelecionada.Value);
        if (empresaSelecionada == null)
        {
            ModelState.AddModelError(string.Empty, "Empresa selecionada inválida.");
            return View(model);
        }

        // Criar objeto do usuário logado com empresa selecionada
        var usuarioLogado = new UsuarioLogadoViewModel
        {
            Id = usuarioTemp.GetProperty("Id").GetInt32(),
            Nome = usuarioTemp.GetProperty("Nome").GetString() ?? "",
            Email = usuarioTemp.GetProperty("Email").GetString() ?? "",
            Perfil = usuarioTemp.GetProperty("Perfil").GetString() ?? "",
            UltimoLogin = usuarioTemp.GetProperty("UltimoLogin").GetDateTime(),
            Permissoes = JsonSerializer.Deserialize<IEnumerable<string>>(
                usuarioTemp.GetProperty("Permissoes").GetRawText()) ?? new List<string>(),
            IsSuperAdmin = usuarioTemp.GetProperty("IsSuperAdmin").GetBoolean(),
            EmpresaId = empresaSelecionada.Id,
            EmpresaNome = empresaSelecionada.Nome,
            EmpresasDisponiveis = empresasDisponiveis ?? new List<EmpresaSimplificada>()
        };

        // Salvar na sessão e limpar dados temporários
        HttpContext.Session.SetString("UsuarioLogado", JsonSerializer.Serialize(usuarioLogado));
        HttpContext.Session.Remove("UsuarioTempLogin");

        // Redirecionar
        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
        {
            return Redirect(model.ReturnUrl);
        }

        return RedirectToAction("Index", "Home");
    }

    /// <summary>
    /// Realiza o logout do usuário
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }

    /// <summary>
    /// Página de acesso negado
    /// </summary>
    public IActionResult AccessDenied()
    {
        return View();
    }

    /// <summary>
    /// Propriedade para acessar o usuário logado
    /// </summary>
    private UsuarioLogadoViewModel? UsuarioLogado
    {
        get
        {
            var usuarioJson = HttpContext.Session.GetString("UsuarioLogado");
            if (string.IsNullOrEmpty(usuarioJson))
                return null;

            try
            {
                return JsonSerializer.Deserialize<UsuarioLogadoViewModel>(usuarioJson);
            }
            catch
            {
                return null;
            }
        }
    }
}