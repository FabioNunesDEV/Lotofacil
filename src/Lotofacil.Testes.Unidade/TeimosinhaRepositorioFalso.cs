// <copyright file="TeimosinhaRepositorioFalso.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Dominio.Teimosinhas;

namespace Lotofacil.Testes.Unidade;

/// <summary>
/// Repositório das Teimosinhas em memória, que dá um identificador a cada Teimosinha nova, como o banco.
/// </summary>
internal sealed class TeimosinhaRepositorioFalso : ITeimosinhaRepositorio
{
    /// <summary>
    /// Teimosinhas gravadas, ativas e removidas.
    /// </summary>
    public List<Teimosinha> Todas { get; } = [];

    /// <inheritdoc/>
    public Task<IReadOnlyList<Teimosinha>> ListarAtivasAsync(CancellationToken cancelamento = default)
    {
        return Task.FromResult<IReadOnlyList<Teimosinha>>(
            this.Todas.Where(teimosinha => teimosinha.Ativa).OrderBy(teimosinha => teimosinha.Nome).ToList());
    }

    /// <inheritdoc/>
    public Task<Teimosinha?> ObterAsync(int id, CancellationToken cancelamento = default)
    {
        return Task.FromResult(this.Todas.Find(teimosinha => teimosinha.Id == id));
    }

    /// <inheritdoc/>
    public Task<IReadOnlyDictionary<int, string>> ListarNomesAsync(CancellationToken cancelamento = default)
    {
        return Task.FromResult<IReadOnlyDictionary<int, string>>(
            this.Todas.ToDictionary(teimosinha => teimosinha.Id, teimosinha => teimosinha.Nome));
    }

    /// <inheritdoc/>
    public Task SalvarAsync(Teimosinha teimosinha, CancellationToken cancelamento = default)
    {
        if (teimosinha.Id == 0)
        {
            // O identificador tem set privado; o banco o preenche ao inserir, e o falso faz o mesmo.
            typeof(Teimosinha).GetProperty(nameof(Teimosinha.Id))!.SetValue(teimosinha, this.Todas.Count + 1);
            this.Todas.Add(teimosinha);
        }

        return Task.CompletedTask;
    }
}
