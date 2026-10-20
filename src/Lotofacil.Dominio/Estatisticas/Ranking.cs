// <copyright file="Ranking.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

namespace Lotofacil.Dominio.Estatisticas;

/// <summary>
/// As primeiras dezenas por um critério e quantas outras empatam com a última delas.
/// </summary>
/// <param name="Itens">As primeiras dezenas, em ordem do critério e, no empate, da dezena.</param>
/// <param name="Empatadas">Quantas dezenas fora dos itens têm o mesmo valor do último item.</param>
public sealed record Ranking(IReadOnlyList<EstatisticaDezena> Itens, int Empatadas)
{
    /// <summary>
    /// Quantidade de dezenas exibidas em cada ranking.
    /// </summary>
    public const int Tamanho = 5;

    /// <summary>
    /// Monta o ranking ordenando pelo valor e, no empate, pela dezena menor.
    /// </summary>
    /// <param name="dezenas">Estatísticas das 25 dezenas.</param>
    /// <param name="valor">Valor usado na ordenação.</param>
    /// <param name="decrescente">Se o maior valor vem primeiro.</param>
    /// <returns>Ranking com as primeiras dezenas.</returns>
    internal static Ranking Criar(
        IEnumerable<EstatisticaDezena> dezenas,
        Func<EstatisticaDezena, int> valor,
        bool decrescente)
    {
        IOrderedEnumerable<EstatisticaDezena> porValor = decrescente
            ? dezenas.OrderByDescending(valor)
            : dezenas.OrderBy(valor);
        List<EstatisticaDezena> ordenadas = porValor.ThenBy(estatistica => estatistica.Dezena).ToList();

        List<EstatisticaDezena> itens = ordenadas.Take(Tamanho).ToList();
        int valorDoUltimo = valor(itens[^1]);
        int empatadas = ordenadas.Skip(Tamanho).Count(estatistica => valor(estatistica) == valorDoUltimo);

        return new Ranking(itens, empatadas);
    }
}
