using Microsoft.EntityFrameworkCore;
using Orama.Domain.Entities;
using Orama.Infra.Data.Context;

namespace Orama.Application.Services;

/// <summary>
/// Serviço para gerenciamento de permissões
/// </summary>
public class PermissaoService : IPermissaoService
{
    private readonly OramaDbContext _context;

    public PermissaoService(OramaDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtém todas as permissões ativas
    /// </summary>
    public async Task<IEnumerable<Permissao>> ObterTodasAsync()
    {
        return await _context.Permissoes
            .Where(p => p.Ativo)
            .OrderBy(p => p.Modulo)
            .ThenBy(p => p.Acao)
            .ToListAsync();
    }

    /// <summary>
    /// Obtém uma permissão por ID
    /// </summary>
    public async Task<Permissao?> ObterPorIdAsync(int id)
    {
        return await _context.Permissoes
            .FirstOrDefaultAsync(p => p.Id == id && p.Ativo);
    }

    /// <summary>
    /// Obtém permissões por módulo
    /// </summary>
    public async Task<IEnumerable<Permissao>> ObterPorModuloAsync(string modulo)
    {
        return await _context.Permissoes
            .Where(p => p.Modulo.ToLower() == modulo.ToLower() && p.Ativo)
            .OrderBy(p => p.Acao)
            .ToListAsync();
    }

    /// <summary>
    /// Obtém todos os módulos disponíveis
    /// </summary>
    public async Task<IEnumerable<string>> ObterModulosAsync()
    {
        return await _context.Permissoes
            .Where(p => p.Ativo)
            .Select(p => p.Modulo)
            .Distinct()
            .OrderBy(m => m)
            .ToListAsync();
    }

    /// <summary>
    /// Cria uma nova permissão
    /// </summary>
    public async Task<Permissao> CriarAsync(Permissao permissao)
    {
        // Verificar se já existe uma permissão com o mesmo nome
        var permissaoExistente = await _context.Permissoes
            .FirstOrDefaultAsync(p => p.Nome.ToLower() == permissao.Nome.ToLower() && p.Ativo);
        if (permissaoExistente != null)
            throw new InvalidOperationException($"Já existe uma permissão com o nome '{permissao.Nome}'");

        _context.Permissoes.Add(permissao);
        await _context.SaveChangesAsync();
        return permissao;
    }

    /// <summary>
    /// Atualiza uma permissão existente
    /// </summary>
    public async Task<Permissao> AtualizarAsync(Permissao permissao)
    {
        var permissaoExistente = await _context.Permissoes.FindAsync(permissao.Id);
        if (permissaoExistente == null)
            throw new InvalidOperationException("Permissão não encontrada");

        // Verificar se já existe outra permissão com o mesmo nome
        var permissaoComMesmoNome = await _context.Permissoes
            .FirstOrDefaultAsync(p => p.Nome.ToLower() == permissao.Nome.ToLower() && p.Id != permissao.Id && p.Ativo);
        if (permissaoComMesmoNome != null)
            throw new InvalidOperationException($"Já existe outra permissão com o nome '{permissao.Nome}'");

        permissaoExistente.Nome = permissao.Nome;
        permissaoExistente.Modulo = permissao.Modulo;
        permissaoExistente.Acao = permissao.Acao;
        permissaoExistente.Descricao = permissao.Descricao;
        permissaoExistente.MarcarComoAtualizada();

        await _context.SaveChangesAsync();
        return permissaoExistente;
    }

    /// <summary>
    /// Exclui uma permissão (soft delete)
    /// </summary>
    public async Task<bool> ExcluirAsync(int id)
    {
        var permissao = await _context.Permissoes.FindAsync(id);
        if (permissao == null)
            return false;

        // Verificar se pode excluir
        if (!await PodeExcluirAsync(id))
            throw new InvalidOperationException("Não é possível excluir esta permissão pois ela está sendo utilizada por perfis");

        permissao.Ativo = false;
        permissao.MarcarComoAtualizada();
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Verifica se uma permissão pode ser excluída
    /// </summary>
    public async Task<bool> PodeExcluirAsync(int id)
    {
        var temPerfis = await _context.PerfilPermissoes
            .AnyAsync(pp => pp.PermissaoId == id && pp.Ativo);
        return !temPerfis;
    }

    /// <summary>
    /// Verifica se um usuário tem uma permissão específica
    /// </summary>
    public async Task<bool> UsuarioTemPermissaoAsync(int usuarioId, string nomePermissao)
    {
        var usuario = await _context.Usuarios
            .Include(u => u.Perfil)
                .ThenInclude(p => p.PerfilPermissoes)
                    .ThenInclude(pp => pp.Permissao)
            .FirstOrDefaultAsync(u => u.Id == usuarioId && u.Ativo);

        if (usuario == null)
            return false;

        // Super Admin tem todas as permissões
        if (usuario.IsSuperAdmin)
            return true;

        // Administrador tem todas as permissões
        if (usuario.Perfil?.Nome == "Administrador")
            return true;

        // Verificar se o perfil tem a permissão
        return usuario.Perfil?.PerfilPermissoes
            .Any(pp => pp.Permissao.Nome == nomePermissao && pp.Concedida && pp.Ativo) ?? false;
    }

    /// <summary>
    /// Obtém todas as permissões de um usuário
    /// </summary>
    public async Task<IEnumerable<string>> ObterPermissoesUsuarioAsync(int usuarioId)
    {
        var usuario = await _context.Usuarios
            .Include(u => u.Perfil)
                .ThenInclude(p => p.PerfilPermissoes)
                    .ThenInclude(pp => pp.Permissao)
            .FirstOrDefaultAsync(u => u.Id == usuarioId && u.Ativo);

        if (usuario == null)
            return Enumerable.Empty<string>();

        // Super Admin ou Administrador tem todas as permissões
        if (usuario.IsSuperAdmin || usuario.Perfil?.Nome == "Administrador")
        {
            return await _context.Permissoes
                .Where(p => p.Ativo)
                .Select(p => p.Nome)
                .ToListAsync();
        }

        // Retornar permissões do perfil
        return usuario.Perfil?.PerfilPermissoes
            .Where(pp => pp.Concedida && pp.Ativo && pp.Permissao.Ativo)
            .Select(pp => pp.Permissao.Nome)
            .ToList() ?? Enumerable.Empty<string>();
    }
}