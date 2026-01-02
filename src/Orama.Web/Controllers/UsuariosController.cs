using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Orama.Application.Services;
using Orama.Domain.Entities;
using Orama.Infra.Data.Context;
using Orama.Web.Models;

namespace Orama.Web.Controllers;

/// <summary>
/// Controller para gerenciamento de usuários
/// </summary>
public class UsuariosController : BaseController
{
    private readonly IUsuarioService _usuarioService;
    private readonly OramaDbContext _context;

    public UsuariosController(IUsuarioService usuarioService, OramaDbContext context)
    {
        _usuarioService = usuarioService;
        _context = context;
    }

    /// <summary>
    /// Lista todos os usuários
    /// </summary>
    public async Task<IActionResult> Index()
    {
        // Verificar permissão
        if (!TemPermissao("Usuarios.Visualizar"))
        {
            return AccessDenied();
        }

        var usuarios = await _usuarioService.ObterTodosAsync();
        var viewModel = usuarios.Select(u => new UsuarioViewModel
        {
            Id = u.Id,
            Nome = u.Nome,
            Email = u.Email,
            Telefone = u.Telefone,
            PerfilId = u.PerfilId,
            NomePerfil = u.Perfil.Nome,
            UltimoLogin = u.UltimoLogin,
            DataCriacao = u.DataCriacao,
            Ativo = u.Ativo
        });

        ViewData["Title"] = "Usuários";
        return View(viewModel);
    }

    /// <summary>
    /// Exibe detalhes de um usuário
    /// </summary>
    public async Task<IActionResult> Details(int id)
    {
        if (!TemPermissao("Usuarios.Visualizar"))
        {
            return AccessDenied();
        }

        var usuario = await _usuarioService.ObterPorIdAsync(id);
        if (usuario == null)
        {
            return NotFound();
        }

        var viewModel = new UsuarioViewModel
        {
            Id = usuario.Id,
            Nome = usuario.Nome,
            Email = usuario.Email,
            Telefone = usuario.Telefone,
            PerfilId = usuario.PerfilId,
            NomePerfil = usuario.Perfil.Nome,
            UltimoLogin = usuario.UltimoLogin,
            DataCriacao = usuario.DataCriacao,
            Ativo = usuario.Ativo
        };

        ViewData["Title"] = "Detalhes do Usuário";
        return View(viewModel);
    }

    /// <summary>
    /// Exibe formulário para criar novo usuário
    /// </summary>
    public async Task<IActionResult> Create()
    {
        if (!TemPermissao("Usuarios.Incluir"))
        {
            return AccessDenied();
        }

        await CarregarPerfis();
        ViewData["Title"] = "Novo Usuário";
        return View(new UsuarioViewModel());
    }

    /// <summary>
    /// Processa criação de novo usuário
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UsuarioViewModel model)
    {
        if (!TemPermissao("Usuarios.Incluir"))
        {
            return AccessDenied();
        }

        // Validar senha obrigatória para novo usuário
        if (string.IsNullOrWhiteSpace(model.Senha))
        {
            ModelState.AddModelError("Senha", "Senha é obrigatória para novo usuário");
        }

        if (!ModelState.IsValid)
        {
            await CarregarPerfis();
            return View(model);
        }

        try
        {
            var usuario = new Usuario
            {
                Nome = model.Nome,
                Email = model.Email,
                Telefone = model.Telefone,
                PerfilId = model.PerfilId,
                Senha = model.Senha!
            };

            await _usuarioService.CriarAsync(usuario);
            AdicionarMensagemSucesso("Usuário criado com sucesso!");
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await CarregarPerfis();
            return View(model);
        }
    }

    /// <summary>
    /// Exibe formulário para editar usuário
    /// </summary>
    public async Task<IActionResult> Edit(int id)
    {
        if (!TemPermissao("Usuarios.Alterar"))
        {
            return AccessDenied();
        }

        var usuario = await _usuarioService.ObterPorIdAsync(id);
        if (usuario == null)
        {
            return NotFound();
        }

        var viewModel = new UsuarioViewModel
        {
            Id = usuario.Id,
            Nome = usuario.Nome,
            Email = usuario.Email,
            Telefone = usuario.Telefone,
            PerfilId = usuario.PerfilId,
            Ativo = usuario.Ativo
        };

        await CarregarPerfis();
        ViewData["Title"] = "Editar Usuário";
        return View(viewModel);
    }

    /// <summary>
    /// Processa edição de usuário
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UsuarioViewModel model)
    {
        if (!TemPermissao("Usuarios.Alterar"))
        {
            return AccessDenied();
        }

        if (id != model.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            await CarregarPerfis();
            return View(model);
        }

        try
        {
            var usuario = new Usuario
            {
                Id = model.Id,
                Nome = model.Nome,
                Email = model.Email,
                Telefone = model.Telefone,
                PerfilId = model.PerfilId,
                Senha = model.Senha ?? string.Empty
            };

            await _usuarioService.AtualizarAsync(usuario);
            AdicionarMensagemSucesso("Usuário atualizado com sucesso!");
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await CarregarPerfis();
            return View(model);
        }
    }

    /// <summary>
    /// Exibe confirmação para excluir usuário
    /// </summary>
    public async Task<IActionResult> Delete(int id)
    {
        if (!TemPermissao("Usuarios.Excluir"))
        {
            return AccessDenied();
        }

        var usuario = await _usuarioService.ObterPorIdAsync(id);
        if (usuario == null)
        {
            return NotFound();
        }

        var viewModel = new UsuarioViewModel
        {
            Id = usuario.Id,
            Nome = usuario.Nome,
            Email = usuario.Email,
            NomePerfil = usuario.Perfil.Nome,
            DataCriacao = usuario.DataCriacao
        };

        ViewData["Title"] = "Excluir Usuário";
        return View(viewModel);
    }

    /// <summary>
    /// Processa exclusão de usuário
    /// </summary>
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (!TemPermissao("Usuarios.Excluir"))
        {
            return AccessDenied();
        }

        try
        {
            await _usuarioService.ExcluirAsync(id);
            AdicionarMensagemSucesso("Usuário excluído com sucesso!");
        }
        catch (Exception ex)
        {
            AdicionarMensagemErro($"Erro ao excluir usuário: {ex.Message}");
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Carrega lista de perfis para dropdown
    /// </summary>
    private async Task CarregarPerfis()
    {
        var perfis = await _context.Perfis
            .Where(p => p.Ativo)
            .OrderBy(p => p.Nome)
            .ToListAsync();

        ViewBag.Perfis = new SelectList(perfis, "Id", "Nome");
    }
}