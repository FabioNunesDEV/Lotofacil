// <copyright file="GeradorJogos.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Dominio.Estatisticas;

namespace Lotofacil.Dominio.Geracao;

/// <summary>
/// Gera jogos distintos com as dezenas fixas, sem as excluídas, sorteando as livres pelos pesos da estratégia.
/// </summary>
public sealed class GeradorJogos
{
    /// <summary>
    /// Tentativas por jogo pedido antes de desistir de achar jogos ainda não gerados.
    /// </summary>
    private const int TentativasPorJogo = 1_000;

    /// <summary>
    /// Fonte dos números aleatórios.
    /// </summary>
    private readonly Random aleatorio;

    /// <summary>
    /// Cria o gerador.
    /// </summary>
    /// <param name="aleatorio">Fonte dos números aleatórios.</param>
    public GeradorJogos(Random aleatorio)
    {
        this.aleatorio = aleatorio;
    }

    /// <summary>
    /// Gera os jogos pedidos, ou recusa quando as fixas e excluídas não permitem montar um jogo.
    /// </summary>
    /// <param name="parametros">Quantidade, tamanho, estratégia, fixas e excluídas.</param>
    /// <param name="estatisticas">Estatísticas das 25 dezenas, base dos pesos.</param>
    /// <returns>Os jogos gerados ou o motivo da recusa.</returns>
    public ResultadoGeracao Gerar(ParametrosGeracao parametros, IReadOnlyList<EstatisticaDezena> estatisticas)
    {
        int fixas = parametros.Fixas.Count;
        int dezenasPorJogo = parametros.DezenasPorJogo;
        if (fixas > dezenasPorJogo)
        {
            return ResultadoGeracao.Recusada($"Há {fixas} dezenas fixas, mais que as {dezenasPorJogo} dezenas por jogo.");
        }

        List<int> livres = Enumerable.Range(RegrasLotofacil.DezenaMinima, RegrasLotofacil.QuantidadeDezenas)
            .Except(parametros.Fixas)
            .Except(parametros.Excluidas)
            .ToList();
        int aSortear = dezenasPorJogo - fixas;
        if (livres.Count < aSortear)
        {
            return ResultadoGeracao.Recusada(
                $"Faltam dezenas livres: com {fixas} fixas e {parametros.Excluidas.Count} excluídas sobram "
                    + $"{livres.Count} livres, e cada jogo precisa de mais {aSortear}.");
        }

        int possiveis = Combinatoria.Combinacoes(livres.Count, aSortear);
        if (possiveis <= parametros.Quantidade)
        {
            List<IReadOnlyList<int>> todos = Combinar(livres, aSortear)
                .Select(escolhidas => Juntar(parametros.Fixas, escolhidas))
                .ToList();

            return new ResultadoGeracao(todos, possiveis < parametros.Quantidade ? possiveis : null, null);
        }

        return new ResultadoGeracao(this.Sortear(parametros, livres, estatisticas), null, null);
    }

    /// <summary>
    /// Enumera, em ordem, todas as formas de escolher k dezenas entre as livres.
    /// </summary>
    /// <param name="livres">Dezenas livres, em ordem crescente.</param>
    /// <param name="k">Quantidade a escolher.</param>
    /// <returns>Cada combinação, em ordem crescente.</returns>
    private static IEnumerable<List<int>> Combinar(IReadOnlyList<int> livres, int k)
    {
        if (k == 0)
        {
            yield return [];
            yield break;
        }

        for (int indice = 0; indice <= livres.Count - k; indice++)
        {
            List<int> restantes = livres.Skip(indice + 1).ToList();
            foreach (List<int> resto in Combinar(restantes, k - 1))
            {
                yield return [livres[indice], .. resto];
            }
        }
    }

    /// <summary>
    /// Junta as fixas às dezenas escolhidas, em ordem crescente.
    /// </summary>
    /// <param name="fixas">Dezenas fixas.</param>
    /// <param name="escolhidas">Dezenas escolhidas entre as livres.</param>
    /// <returns>As dezenas do jogo.</returns>
    private static IReadOnlyList<int> Juntar(IEnumerable<int> fixas, IEnumerable<int> escolhidas)
    {
        return fixas.Concat(escolhidas).Order().ToList();
    }

    /// <summary>
    /// Define com quais pesos e quantas dezenas sortear em cada etapa: uma etapa, ou duas na Equilibrada.
    /// </summary>
    /// <param name="estrategia">Estratégia escolhida.</param>
    /// <param name="estatisticas">Estatísticas das dezenas.</param>
    /// <param name="aSortear">Quantidade de dezenas a sortear entre as livres.</param>
    /// <returns>Pesos e quantidade de cada etapa.</returns>
    private static List<(IReadOnlyDictionary<int, int> Pesos, int Quantidade)> Etapas(
        EstrategiaGeracao estrategia,
        IReadOnlyList<EstatisticaDezena> estatisticas,
        int aSortear)
    {
        if (estrategia != EstrategiaGeracao.Equilibrada)
        {
            return [(PesosDezenas.Calcular(estatisticas, estrategia), aSortear)];
        }

        int pelasMaisSorteadas = (aSortear + 1) / 2;
        return
        [
            (PesosDezenas.Calcular(estatisticas, EstrategiaGeracao.MaisSorteadas), pelasMaisSorteadas),
            (PesosDezenas.Calcular(estatisticas, EstrategiaGeracao.MaisAtrasadas), aSortear - pelasMaisSorteadas),
        ];
    }

    /// <summary>
    /// Sorteia jogos até ter a quantidade pedida de jogos distintos.
    /// </summary>
    /// <param name="parametros">Parâmetros da geração.</param>
    /// <param name="livres">Dezenas livres.</param>
    /// <param name="estatisticas">Estatísticas das dezenas.</param>
    /// <returns>Os jogos sorteados.</returns>
    private List<IReadOnlyList<int>> Sortear(
        ParametrosGeracao parametros,
        IReadOnlyList<int> livres,
        IReadOnlyList<EstatisticaDezena> estatisticas)
    {
        int aSortear = parametros.DezenasPorJogo - parametros.Fixas.Count;
        List<(IReadOnlyDictionary<int, int> Pesos, int Quantidade)> etapas =
            Etapas(parametros.Estrategia, estatisticas, aSortear);

        List<IReadOnlyList<int>> jogos = [];
        HashSet<string> gerados = [];
        int tentativas = parametros.Quantidade * TentativasPorJogo;
        while (jogos.Count < parametros.Quantidade && tentativas-- > 0)
        {
            List<int> disponiveis = [.. livres];
            List<int> escolhidas = [];
            foreach ((IReadOnlyDictionary<int, int> pesos, int quantidade) in etapas)
            {
                for (int vez = 0; vez < quantidade; vez++)
                {
                    int dezena = this.SortearUma(disponiveis, pesos);
                    disponiveis.Remove(dezena);
                    escolhidas.Add(dezena);
                }
            }

            IReadOnlyList<int> jogo = Juntar(parametros.Fixas, escolhidas);
            if (gerados.Add(string.Join(',', jogo)))
            {
                jogos.Add(jogo);
            }
        }

        return jogos;
    }

    /// <summary>
    /// Sorteia uma dezena com chance proporcional ao peso.
    /// </summary>
    /// <param name="disponiveis">Dezenas ainda disponíveis.</param>
    /// <param name="pesos">Peso de cada dezena.</param>
    /// <returns>A dezena sorteada.</returns>
    private int SortearUma(IReadOnlyList<int> disponiveis, IReadOnlyDictionary<int, int> pesos)
    {
        int alvo = this.aleatorio.Next(disponiveis.Sum(dezena => pesos[dezena]));
        foreach (int dezena in disponiveis)
        {
            alvo -= pesos[dezena];
            if (alvo < 0)
            {
                return dezena;
            }
        }

        return disponiveis[^1];
    }
}
