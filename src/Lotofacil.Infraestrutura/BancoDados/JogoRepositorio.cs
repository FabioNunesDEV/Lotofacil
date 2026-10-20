// <copyright file="JogoRepositorio.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Dominio.Jogos;
using Microsoft.EntityFrameworkCore;

namespace Lotofacil.Infraestrutura.BancoDados;

/// <summary>
/// Repositório dos jogos do usuário sobre o SQLite.
/// </summary>
internal sealed class JogoRepositorio : IJogoRepositorio
{
    /// <summary>
    /// Cria um contexto novo a cada operação, como no repositório de concursos.
    /// </summary>
    private readonly IDbContextFactory<LotofacilContexto> fabricaContexto;

    /// <summary>
    /// Cria o repositório.
    /// </summary>
    /// <param name="fabricaContexto">Fábrica de contextos do EF Core.</param>
    public JogoRepositorio(IDbContextFactory<LotofacilContexto> fabricaContexto)
    {
        this.fabricaContexto = fabricaContexto;
    }

    /// <inheritdoc/>
    public async Task AdicionarAsync(Jogo jogo, CancellationToken cancelamento = default)
    {
        await using LotofacilContexto contexto = await this.fabricaContexto.CreateDbContextAsync(cancelamento);

        contexto.Jogos.Add(jogo);
        await contexto.SaveChangesAsync(cancelamento);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Jogo>> ListarAsync(CancellationToken cancelamento = default)
    {
        await using LotofacilContexto contexto = await this.fabricaContexto.CreateDbContextAsync(cancelamento);

        // O identificador cresce a cada cadastro, então ordena na ordem em que os jogos foram cadastrados.
        return await contexto.Jogos
            .AsNoTracking()
            .OrderBy(jogo => jogo.Id)
            .ToListAsync(cancelamento);
    }

    /// <inheritdoc/>
    public async Task<bool> ExisteAsync(int numeroConcurso, IEnumerable<int> dezenas, CancellationToken cancelamento = default)
    {
        await using LotofacilContexto contexto = await this.fabricaContexto.CreateDbContextAsync(cancelamento);

        // Comparar a lista de dezenas no SQL exigiria depender do formato JSON da coluna; os jogos de um concurso
        // são poucos, então a comparação é feita em memória.
        List<Jogo> jogosDoConcurso = await contexto.Jogos
            .AsNoTracking()
            .Where(jogo => jogo.NumeroConcurso == numeroConcurso)
            .ToListAsync(cancelamento);
        List<int> procuradas = dezenas.Order().ToList();

        return jogosDoConcurso.Any(jogo => jogo.Dezenas.SequenceEqual(procuradas));
    }

    /// <inheritdoc/>
    public async Task ExcluirAsync(int id, CancellationToken cancelamento = default)
    {
        await using LotofacilContexto contexto = await this.fabricaContexto.CreateDbContextAsync(cancelamento);

        await contexto.Jogos.Where(jogo => jogo.Id == id).ExecuteDeleteAsync(cancelamento);
    }
}
