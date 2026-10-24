// <copyright file="TeimosinhaRepositorio.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Dominio.Teimosinhas;
using Microsoft.EntityFrameworkCore;

namespace Lotofacil.Infraestrutura.BancoDados;

/// <summary>
/// Repositório das Teimosinhas sobre o SQLite.
/// </summary>
internal sealed class TeimosinhaRepositorio : ITeimosinhaRepositorio
{
    /// <summary>
    /// Cria um contexto novo a cada operação, como nos demais repositórios.
    /// </summary>
    private readonly IDbContextFactory<LotofacilContexto> fabricaContexto;

    /// <summary>
    /// Cria o repositório.
    /// </summary>
    /// <param name="fabricaContexto">Fábrica de contextos do EF Core.</param>
    public TeimosinhaRepositorio(IDbContextFactory<LotofacilContexto> fabricaContexto)
    {
        this.fabricaContexto = fabricaContexto;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Teimosinha>> ListarAtivasAsync(CancellationToken cancelamento = default)
    {
        await using LotofacilContexto contexto = await this.fabricaContexto.CreateDbContextAsync(cancelamento);

        List<Teimosinha> ativas = await contexto.Teimosinhas
            .AsNoTracking()
            .Where(teimosinha => teimosinha.Ativa)
            .ToListAsync(cancelamento);

        // A ordenação do SQLite diferencia maiúsculas; a lista é pequena, então ordena em memória.
        return ativas.OrderBy(teimosinha => teimosinha.Nome, StringComparer.CurrentCultureIgnoreCase).ToList();
    }

    /// <inheritdoc/>
    public async Task<Teimosinha?> ObterAsync(int id, CancellationToken cancelamento = default)
    {
        await using LotofacilContexto contexto = await this.fabricaContexto.CreateDbContextAsync(cancelamento);

        return await contexto.Teimosinhas
            .AsNoTracking()
            .SingleOrDefaultAsync(teimosinha => teimosinha.Id == id, cancelamento);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyDictionary<int, string>> ListarNomesAsync(CancellationToken cancelamento = default)
    {
        await using LotofacilContexto contexto = await this.fabricaContexto.CreateDbContextAsync(cancelamento);

        return await contexto.Teimosinhas
            .AsNoTracking()
            .ToDictionaryAsync(teimosinha => teimosinha.Id, teimosinha => teimosinha.Nome, cancelamento);
    }

    /// <inheritdoc/>
    public async Task SalvarAsync(Teimosinha teimosinha, CancellationToken cancelamento = default)
    {
        await using LotofacilContexto contexto = await this.fabricaContexto.CreateDbContextAsync(cancelamento);

        // Jogos novos têm identificador zero e são inseridos; os já gravados são atualizados.
        if (teimosinha.Id == 0)
        {
            contexto.Teimosinhas.Add(teimosinha);
        }
        else
        {
            contexto.Teimosinhas.Update(teimosinha);
        }

        await contexto.SaveChangesAsync(cancelamento);
    }
}
