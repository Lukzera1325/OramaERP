using Microsoft.AspNetCore.Mvc;
using Orama.Application.Services;
using Orama.Domain.Entities;
using Orama.Web.Models;

namespace Orama.Web.Controllers;

/// <summary>
/// Controller para gerenciamento de perfis de usuário
/// </summary>
public class PerfisController : BaseController
{
    private readonly IPerfilService _perfilService;
    private readonly IPermissaoService _permissaoService;
    private readonly IUsuarioService _usuarioService;

    public PerfisController(
        IPerfilService perfilService,
        IPermissaoService permissaoService,
        IUsuarioService usuarioService)
    {
        _perfilService = perfilService;
        _permissaoService = permissaoService;
        _usuarioService = usuarioService;
    }

    /// <summary>
    /// Lista todos os perfis
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        if (!TemPermissao("Usuarios.Visualizar"))
            return RedirectToAction("AccessDenied", "Auth");

        var perfis = await _perfilService.ObterTodosAsync();
        var viewModels = new List<PerfilViewModel>();

        foreach (var perfil in perfis)
        {
            var usuarios = await _usuarioService.ObterTodosAsync();
            var usuariosCount = usuarios.Count(u => u.PerfilId == perfil.Id);
            var permissoes = await _perfilService.ObterPermissoesAsync(perfil.Id);
            var podeExcluir = await _perfilService.PodeExcluirAsync(perfil.Id);

            viewModels.Add(new PerfilViewModel
            {
                Id = perfil.Id,
                Nome = perfil.Nome,
                Descricao = perfil.Descricao,
                DataCriacao = perfil.DataCriacao,
                DataAtualizacao = perfil.DataAtualizacao,
                Ativo = perfil.Ativo,
                QuantidadeUsuarios = usuariosCount,
                QuantidadePermissoes = permissoes.Count(),
                PodeExcluir = podeExcluir
            });
        }

        ViewData["Title"] = "Perfis de Usuário";
        return View(viewModels);
    }

    /// <summary>
    /// Exibe detalhes de um perfil
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        if (!TemPermissao("Usuarios.Visualizar"))
            return RedirectToAction("AccessDenied", "Auth");

        var perfil = await _perfilService.ObterPorIdAsync(id);
        if (perfil == null)
            return NotFound();

        var usuarios = await _usuarioService.ObterTodosAsync();
        var usuariosCount = usuarios.Count(u => u.PerfilId == perfil.Id);
        var permissoes = await _perfilService.ObterPermissoesAsync(perfil.Id);
        var podeExcluir = await _perfilService.PodeExcluirAsync(perfil.Id);

        var viewModel = new PerfilViewModel
        {
            Id = perfil.Id,
            Nome = perfil.Nome,
            Descricao = perfil.Descricao,
            DataCriacao = perfil.DataCriacao,
            DataAtualizacao = perfil.DataAtualizacao,
            Ativo = perfil.Ativo,
            QuantidadeUsuarios = usuariosCount,
            QuantidadePermissoes = permissoes.Count(),
            PodeExcluir = podeExcluir
        };

        ViewData["Title"] = $"Detalhes do Perfil - {perfil.Nome}";
        ViewBag.Permissoes = permissoes;
        return View(viewModel);
    }

    /// <summary>
    /// Exibe formulário para criar novo perfil
    /// </summary>
    [HttpGet]
    public IActionResult Create()
    {
        if (!TemPermissao("Usuarios.Incluir"))
            return RedirectToAction("AccessDenied", "Auth");

        ViewData["Title"] = "Novo Perfil";
        return View(new PerfilViewModel());
    }

    /// <summary>
    /// Processa criação de novo perfil
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PerfilViewModel model)
    {
        if (!TemPermissao("Usuarios.Incluir"))
            return RedirectToAction("AccessDenied", "Auth");

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Novo Perfil";
            return View(model);
        }

        try
        {
            var perfil = new Perfil
            {
                Nome = model.Nome,
                Descricao = model.Descricao
            };

            await _perfilService.CriarAsync(perfil);
            TempData["Success"] = "Perfil criado com sucesso!";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            ViewData["Title"] = "Novo Perfil";
            return View(model);
        }
        catch (Exception)
        {
            ModelState.AddModelError("", "Erro interno do servidor. Tente novamente.");
            ViewData["Title"] = "Novo Perfil";
            return View(model);
        }
    }

    /// <summary>
    /// Exibe formulário para editar perfil
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        if (!TemPermissao("Usuarios.Alterar"))
            return RedirectToAction("AccessDenied", "Auth");

        var perfil = await _perfilService.ObterPorIdAsync(id);
        if (perfil == null)
            return NotFound();

        var viewModel = new PerfilViewModel
        {
            Id = perfil.Id,
            Nome = perfil.Nome,
            Descricao = perfil.Descricao,
            Ativo = perfil.Ativo
        };

        ViewData["Title"] = $"Editar Perfil - {perfil.Nome}";
        return View(viewModel);
    }

    /// <summary>
    /// Processa edição de perfil
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, PerfilViewModel model)
    {
        if (!TemPermissao("Usuarios.Alterar"))
            return RedirectToAction("AccessDenied", "Auth");

        if (id != model.Id)
            return BadRequest();

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = $"Editar Perfil - {model.Nome}";
            return View(model);
        }

        try
        {
            var perfil = new Perfil
            {
                Id = model.Id,
                Nome = model.Nome,
                Descricao = model.Descricao
            };

            await _perfilService.AtualizarAsync(perfil);
            TempData["Success"] = "Perfil atualizado com sucesso!";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            ViewData["Title"] = $"Editar Perfil - {model.Nome}";
            return View(model);
        }
        catch (Exception)
        {
            ModelState.AddModelError("", "Erro interno do servidor. Tente novamente.");
            ViewData["Title"] = $"Editar Perfil - {model.Nome}";
            return View(model);
        }
    }

    /// <summary>
    /// Exibe confirmação para excluir perfil
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        if (!TemPermissao("Usuarios.Excluir"))
            return RedirectToAction("AccessDenied", "Auth");

        var perfil = await _perfilService.ObterPorIdAsync(id);
        if (perfil == null)
            return NotFound();

        var usuarios = await _usuarioService.ObterTodosAsync();
        var usuariosCount = usuarios.Count(u => u.PerfilId == perfil.Id);
        var permissoes = await _perfilService.ObterPermissoesAsync(perfil.Id);
        var podeExcluir = await _perfilService.PodeExcluirAsync(perfil.Id);

        var viewModel = new PerfilViewModel
        {
            Id = perfil.Id,
            Nome = perfil.Nome,
            Descricao = perfil.Descricao,
            DataCriacao = perfil.DataCriacao,
            DataAtualizacao = perfil.DataAtualizacao,
            Ativo = perfil.Ativo,
            QuantidadeUsuarios = usuariosCount,
            QuantidadePermissoes = permissoes.Count(),
            PodeExcluir = podeExcluir
        };

        ViewData["Title"] = $"Excluir Perfil - {perfil.Nome}";
        return View(viewModel);
    }

    /// <summary>
    /// Processa exclusão de perfil
    /// </summary>
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (!TemPermissao("Usuarios.Excluir"))
            return RedirectToAction("AccessDenied", "Auth");

        try
        {
            var sucesso = await _perfilService.ExcluirAsync(id);
            if (sucesso)
            {
                TempData["Success"] = "Perfil excluído com sucesso!";
            }
            else
            {
                TempData["Error"] = "Perfil não encontrado.";
            }
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }
        catch (Exception)
        {
            TempData["Error"] = "Erro interno do servidor. Tente novamente.";
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Gerencia permissões de um perfil
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Permissoes(int id)
    {
        if (!TemPermissao("Usuarios.Alterar"))
            return RedirectToAction("AccessDenied", "Auth");

        var perfil = await _perfilService.ObterPorIdAsync(id);
        if (perfil == null)
            return NotFound();

        var todasPermissoes = await _permissaoService.ObterTodasAsync();
        var permissoesPerfil = await _perfilService.ObterPermissoesAsync(id);
        var permissoesIds = permissoesPerfil.Select(p => p.Id).ToHashSet();

        var viewModel = new PerfilPermissoesViewModel
        {
            PerfilId = perfil.Id,
            NomePerfil = perfil.Nome,
            DescricaoPerfil = perfil.Descricao
        };

        // Agrupar por módulo
        var modulosGroup = todasPermissoes.GroupBy(p => p.Modulo);
        foreach (var grupo in modulosGroup.OrderBy(g => g.Key))
        {
            var moduloViewModel = new ModuloPermissoesViewModel
            {
                NomeModulo = grupo.Key
            };

            foreach (var permissao in grupo.OrderBy(p => p.Acao))
            {
                moduloViewModel.Permissoes.Add(new PermissaoCheckboxViewModel
                {
                    Id = permissao.Id,
                    Nome = permissao.Nome,
                    Acao = permissao.Acao,
                    Descricao = permissao.Descricao,
                    Selecionada = permissoesIds.Contains(permissao.Id)
                });
            }

            viewModel.Modulos.Add(moduloViewModel);
        }

        ViewData["Title"] = $"Permissões do Perfil - {perfil.Nome}";
        return View(viewModel);
    }

    /// <summary>
    /// Salva permissões de um perfil
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Permissoes(int id, List<int> permissoesSelecionadas)
    {
        if (!TemPermissao("Usuarios.Alterar"))
            return RedirectToAction("AccessDenied", "Auth");

        try
        {
            await _perfilService.AtualizarPermissoesAsync(id, permissoesSelecionadas ?? new List<int>());
            TempData["Success"] = "Permissões atualizadas com sucesso!";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Permissoes), new { id });
        }
        catch (Exception)
        {
            TempData["Error"] = "Erro interno do servidor. Tente novamente.";
            return RedirectToAction(nameof(Permissoes), new { id });
        }
    }
}