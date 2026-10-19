// <copyright file="ConcursoRepositorio.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Dominio.Concursos;
using Microsoft.EntityFrameworkCore;

namespace Lotofacil.Infraestrutura.BancoDados;

/// <summary>
/// Grava e lê concursos na base SQLite, com um contexto do EF Core por operação.
/// </summary>
internal sealed class ConcursoRepositorio : IConcursoRepositorio
{
    /// <summary>
    /// Cria um contexto novo a cada operação; no Blazor Server um contexto por circuito seria compartilhado.
    /// </summary>
    private readonly IDbContextFactory<LotofacilContexto> fabricaContexto;

    /// <summary>
    /// Cria o repositório.
    /// </summary>
    /// <param name="fabricaContexto">Fábrica de contextos do EF Core.</param>
    public ConcursoRepositorio(IDbContextFactory<LotofacilContexto> fabricaContexto)
    {
        this.fabricaContexto = fabricaContexto;
    }

    /// <inheritdoc/>
    public async Task AdicionarAsync(IReadOnlyCollection<Concurso> concursos, CancellationToken cancelamento = default)
    {
        List<int> numeros = concursos.Select(concurso => concurso.Numero).ToList();
        List<int> repetidosNoLote = numeros.GroupBy(numero => numero)
            .Where(grupo => grupo.Count() > 1)
            .Select(grupo => grupo.Key)
            .ToList();
        if (repetidosNoLote.Count > 0)
        {
            throw new InvalidOperationException(
                $"O lote tem concursos repetidos: {string.Join(", ", repetidosNoLote)}. Nenhum concurso foi gravado.");
        }

        await using LotofacilContexto contexto = await this.fabricaContexto.CreateDbContextAsync(cancelamento);

        List<int> jaGravados = await contexto.Concursos
            .Where(concurso => numeros.Contains(concurso.Numero))
            .Select(concurso => concurso.Numero)
            .OrderBy(numero => numero)
            .ToListAsync(cancelamento);
        if (jaGravados.Count > 0)
        {
            throw new InvalidOperationException(
                $"Concursos já gravados na base local: {string.Join(", ", jaGravados)}. Nenhum concurso foi gravado.");
        }

        // SaveChanges grava tudo numa única transação.
        contexto.Concursos.AddRange(concursos);
        await contexto.SaveChangesAsync(cancelamento);
    }

    /// <inheritdoc/>
    public async Task<Concurso?> ObterAsync(int numero, CancellationToken cancelamento = default)
    {
        await using LotofacilContexto contexto = await this.fabricaContexto.CreateDbContextAsync(cancelamento);

        return await contexto.Concursos
            .AsNoTracking()
            .SingleOrDefaultAsync(concurso => concurso.Numero == numero, cancelamento);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Concurso>> ListarAsync(
        int ignorar,
        int quantidade,
        CancellationToken cancelamento = default)
    {
        await using LotofacilContexto contexto = await this.fabricaContexto.CreateDbContextAsync(cancelamento);

        return await contexto.Concursos
            .AsNoTracking()
            .OrderByDescending(concurso => concurso.Numero)
            .Skip(ignorar)
            .Take(quantidade)
            .ToListAsync(cancelamento);
    }

    /// <inheritdoc/>
    public async Task<int> ContarAsync(CancellationToken cancelamento = default)
    {
        await using LotofacilContexto contexto = await this.fabricaContexto.CreateDbContextAsync(cancelamento);

        return await contexto.Concursos.CountAsync(cancelamento);
    }
}
