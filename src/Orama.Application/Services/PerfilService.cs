using Microsoft.EntityFrameworkCore;
using Orama.Domain.Entities;
using Orama.Infra.Data.Context;

namespace Orama.Application.Services;

/// <summary>
/// Serviço para gerenciamento de perfis
/// </summary>
public class PerfilService : IPerfilService
{
    private readonly OramaDbContext _context;

    public PerfilService(OramaDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtém todos os perfis ativos
    /// </summary>
    public async Task<IEnumerable<Perfil>> ObterTodosAsync()
    {
        return await _context.Perfis
            .Where(p => p.Ativo)
            .OrderBy(p => p.Nome)
            .ToListAsync();
    }

    /// <summary>
    /// Obtém um perfil por ID
    /// </summary>
    public async Task<Perfil?> ObterPorIdAsync(int id)
    {
        return await _context.Perfis
            .Include(p => p.PerfilPermissoes)
                .ThenInclude(pp => pp.Permissao)
            .FirstOrDefaultAsync(p => p.Id == id && p.Ativo);
    }

    /// <summary>
    /// Obtém um perfil por nome
    /// </summary>
    public async Task<Perfil?> ObterPorNomeAsync(string nome)
    {
        return await _context.Perfis
            .FirstOrDefaultAsync(p => p.Nome.ToLower() == nome.ToLower() && p.Ativo);
    }

    /// <summary>
    /// Cria um novo perfil
    /// </summary>
    public async Task<Perfil> CriarAsync(Perfil perfil)
    {
        // Verificar se já existe um perfil com o mesmo nome
        var perfilExistente = await ObterPorNomeAsync(perfil.Nome);
        if (perfilExistente != null)
            throw new InvalidOperationException($"Já existe um perfil com o nome '{perfil.Nome}'");

        _context.Perfis.Add(perfil);
        await _context.SaveChangesAsync();
        return perfil;
    }

    /// <summary>
    /// Atualiza um perfil existente
    /// </summary>
    public async Task<Perfil> AtualizarAsync(Perfil perfil)
    {
        var perfilExistente = await _context.Perfis.FindAsync(perfil.Id);
        if (perfilExistente == null)
            throw new InvalidOperationException("Perfil não encontrado");

        // Verificar se já existe outro perfil com o mesmo nome
        var perfilComMesmoNome = await _context.Perfis
            .FirstOrDefaultAsync(p => p.Nome.ToLower() == perfil.Nome.ToLower() && p.Id != perfil.Id && p.Ativo);
        if (perfilComMesmoNome != null)
            throw new InvalidOperationException($"Já existe outro perfil com o nome '{perfil.Nome}'");

        perfilExistente.Nome = perfil.Nome;
        perfilExistente.Descricao = perfil.Descricao;
        perfilExistente.MarcarComoAtualizada();

        await _context.SaveChangesAsync();
        return perfilExistente;
    }

    /// <summary>
    /// Exclui um perfil (soft delete)
    /// </summary>
    public async Task<bool> ExcluirAsync(int id)
    {
        var perfil = await _context.Perfis.FindAsync(id);
        if (perfil == null)
            return false;

        // Verificar se pode excluir
        if (!await PodeExcluirAsync(id))
            throw new InvalidOperationException("Não é possível excluir este perfil pois existem usuários vinculados a ele");

        perfil.Ativo = false;
        perfil.MarcarComoAtualizada();
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Verifica se um perfil pode ser excluído (não tem usuários vinculados)
    /// </summary>
    public async Task<bool> PodeExcluirAsync(int id)
    {
        var temUsuarios = await _context.Usuarios
            .AnyAsync(u => u.PerfilId == id && u.Ativo);
        return !temUsuarios;
    }

    /// <summary>
    /// Obtém as permissões de um perfil
    /// </summary>
    public async Task<IEnumerable<Permissao>> ObterPermissoesAsync(int perfilId)
    {
        return await _context.PerfilPermissoes
            .Where(pp => pp.PerfilId == perfilId && pp.Concedida && pp.Ativo)
            .Select(pp => pp.Permissao)
            .Where(p => p.Ativo)
            .OrderBy(p => p.Modulo)
            .ThenBy(p => p.Acao)
            .ToListAsync();
    }

    /// <summary>
    /// Atualiza as permissões de um perfil
    /// </summary>
    public async Task AtualizarPermissoesAsync(int perfilId, IEnumerable<int> permissaoIds)
    {
        // Verificar se o perfil existe
        var perfil = await _context.Perfis.FindAsync(perfilId);
        if (perfil == null)
            throw new InvalidOperationException("Perfil não encontrado");

        // Remover todas as permissões atuais do perfil
        var permissoesAtuais = await _context.PerfilPermissoes
            .Where(pp => pp.PerfilId == perfilId)
            .ToListAsync();
        _context.PerfilPermissoes.RemoveRange(permissoesAtuais);

        // Adicionar as novas permissões
        foreach (var permissaoId in permissaoIds)
        {
            var perfilPermissao = new PerfilPermissao
            {
                PerfilId = perfilId,
                PermissaoId = permissaoId,
                Concedida = true
            };
            _context.PerfilPermissoes.Add(perfilPermissao);
        }

        await _context.SaveChangesAsync();
    }
}