using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Orama.Web.Models;
using System.Text.Json;

namespace Orama.Web.Controllers;

/// <summary>
/// Controller base com funcionalidades comuns de autenticação e autorização
/// </summary>
public abstract class BaseController : Controller
{
    /// <summary>
    /// Usuário atualmente logado no sistema
    /// </summary>
    protected UsuarioLogadoViewModel? UsuarioLogado
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

    /// <summary>
    /// Verifica se o usuário está autenticado
    /// </summary>
    protected bool UsuarioAutenticado => UsuarioLogado != null;

    /// <summary>
    /// Verifica se o usuário tem uma permissão específica
    /// </summary>
    protected bool TemPermissao(string nomePermissao)
    {
        if (UsuarioLogado == null)
            return false;

        // Administrador tem todas as permissões
        if (UsuarioLogado.Perfil == "Administrador")
            return true;

        return UsuarioLogado.Permissoes.Contains(nomePermissao);
    }

    /// <summary>
    /// Executa antes de cada action para verificar autenticação
    /// </summary>
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        // Verificar se precisa de autenticação (exceto AuthController)
        if (GetType() != typeof(AuthController) && !UsuarioAutenticado)
        {
            var returnUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
            context.Result = RedirectToAction("Login", "Auth", new { returnUrl });
            return;
        }

        // Disponibilizar usuário logado para as views
        if (UsuarioLogado != null)
        {
            ViewBag.UsuarioLogado = UsuarioLogado;
        }

        base.OnActionExecuting(context);
    }

    /// <summary>
    /// Retorna erro de acesso negado
    /// </summary>
    protected IActionResult AccessDenied()
    {
        return RedirectToAction("AccessDenied", "Auth");
    }

    /// <summary>
    /// Adiciona mensagem de sucesso
    /// </summary>
    protected void AdicionarMensagemSucesso(string mensagem)
    {
        TempData["MensagemSucesso"] = mensagem;
    }

    /// <summary>
    /// Adiciona mensagem de erro
    /// </summary>
    protected void AdicionarMensagemErro(string mensagem)
    {
        TempData["MensagemErro"] = mensagem;
    }

    /// <summary>
    /// Adiciona mensagem de aviso
    /// </summary>
    protected void AdicionarMensagemAviso(string mensagem)
    {
        TempData["MensagemAviso"] = mensagem;
    }

    /// <summary>
    /// Obtém o ID da empresa do usuário logado
    /// </summary>
    protected int ObterEmpresaId()
    {
        return UsuarioLogado?.EmpresaId ?? 1; // Fallback para empresa ID 1
    }
}