// <copyright file="EstatisticasPerfis.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Dominio.Concursos;

namespace Lotofacil.Dominio.Estatisticas.Perfis;

/// <summary>
/// Perfis dos concursos: observado numa janela, teórico e atraso de cada linha sobre todo o histórico.
/// </summary>
public static class EstatisticasPerfis
{
    /// <summary>
    /// Chance teórica mínima, em percentual, para um valor da ponta ter linha própria.
    /// </summary>
    private const decimal ChanceMinimaDaLinha = 1m;

    /// <summary>
    /// Calcula os 7 perfis; lança <see cref="ArgumentException"/> com histórico vazio ou janela menor que 1.
    /// </summary>
    /// <param name="historico">Todos os concursos da base, em qualquer ordem.</param>
    /// <param name="ultimosConcursos">Quantidade de concursos mais recentes da janela, ou <c>null</c> para todos.</param>
    /// <returns>Os perfis na ordem de <see cref="TipoPerfil"/>.</returns>
    public static IReadOnlyList<DistribuicaoPerfil> Calcular(IReadOnlyList<ResultadoSorteio> historico, int? ultimosConcursos)
    {
        if (historico.Count == 0)
        {
            throw new ArgumentException("O histórico deve ter ao menos um concurso.", nameof(historico));
        }

        if (ultimosConcursos < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(ultimosConcursos),
                "A janela deve ter ao menos um concurso.");
        }

        List<ResultadoSorteio> ordenado = historico.OrderBy(resultado => resultado.Numero).ToList();
        int concursosNaJanela = Math.Min(ultimosConcursos ?? ordenado.Count, ordenado.Count);
        int inicioJanela = ordenado.Count - concursosNaJanela;

        return Enum.GetValues<TipoPerfil>()
            .Select(tipo => new DistribuicaoPerfil(tipo, CalcularFaixas(tipo, ordenado, inicioJanela)))
            .ToList();
    }

    /// <summary>
    /// Calcula as linhas de um perfil.
    /// </summary>
    /// <param name="tipo">Perfil.</param>
    /// <param name="ordenado">Todos os concursos, em ordem crescente de número.</param>
    /// <param name="inicioJanela">Posição, na lista ordenada, do primeiro concurso da janela.</param>
    /// <returns>Linhas com observado, teórico e atraso.</returns>
    private static List<FaixaPerfil> CalcularFaixas(TipoPerfil tipo, List<ResultadoSorteio> ordenado, int inicioJanela)
    {
        List<FaixaPerfil> faixas = AgruparPontas(tipo, FaixasBase(tipo));
        int[] concursosNaFaixa = new int[faixas.Count];
        int?[] ultimaVez = new int?[faixas.Count];
        int considerados = 0;

        // O primeiro concurso da base não tem anterior para contar as repetidas.
        int primeiraPosicao = tipo == TipoPerfil.Repetidas ? 1 : 0;
        for (int posicao = primeiraPosicao; posicao < ordenado.Count; posicao++)
        {
            IReadOnlyList<int>? anterior = posicao > 0 ? ordenado[posicao - 1].Dezenas : null;
            int valor = ValorPerfil.Calcular(tipo, ordenado[posicao].Dezenas, anterior);
            int indice = faixas.FindIndex(faixa => faixa.Contem(valor));

            ultimaVez[indice] = ordenado[posicao].Numero;
            if (posicao >= inicioJanela)
            {
                concursosNaFaixa[indice]++;
                considerados++;
            }
        }

        int ultimoConcurso = ordenado[^1].Numero;

        return faixas
            .Select((faixa, indice) => faixa with
            {
                Observado = considerados == 0 ? 0 : concursosNaFaixa[indice] * 100m / considerados,
                Atraso = ultimoConcurso - ultimaVez[indice],
            })
            .ToList();
    }

    /// <summary>
    /// Linhas antes do agrupamento: uma por valor possível ou, na soma, uma por faixa de 15.
    /// </summary>
    /// <param name="tipo">Perfil.</param>
    /// <returns>Linhas com o teórico preenchido, em ordem crescente.</returns>
    private static List<FaixaPerfil> FaixasBase(TipoPerfil tipo)
    {
        int menor = DistribuicaoTeorica.MenorValor(tipo);
        int maior = DistribuicaoTeorica.MaiorValor(tipo);
        int largura = tipo == TipoPerfil.Soma ? DistribuicaoTeorica.LarguraFaixaSoma : 1;
        int quantidade = (maior - menor + 1) / largura;

        return Enumerable.Range(0, quantidade)
            .Select(posicao =>
            {
                int minimo = menor + (posicao * largura);

                // A última faixa vai até o maior valor (na soma, 255–270).
                int maximo = posicao == quantidade - 1 ? maior : minimo + largura - 1;
                return CriarFaixa(tipo, minimo, maximo);
            })
            .ToList();
    }

    /// <summary>
    /// Soma as linhas das pontas com chance teórica abaixo de 1% à linha vizinha mais próxima com pelo menos 1%.
    /// </summary>
    /// <param name="tipo">Perfil.</param>
    /// <param name="faixas">Linhas base, em ordem crescente.</param>
    /// <returns>Linhas com as pontas agrupadas.</returns>
    private static List<FaixaPerfil> AgruparPontas(TipoPerfil tipo, List<FaixaPerfil> faixas)
    {
        int primeiraComum = faixas.FindIndex(faixa => faixa.Teorico >= ChanceMinimaDaLinha);
        int ultimaComum = faixas.FindLastIndex(faixa => faixa.Teorico >= ChanceMinimaDaLinha);
        if (primeiraComum < 0)
        {
            return faixas;
        }

        List<FaixaPerfil> agrupadas = faixas.GetRange(primeiraComum, ultimaComum - primeiraComum + 1);
        if (primeiraComum > 0)
        {
            agrupadas[0] = CriarFaixa(tipo, faixas[0].Minimo, agrupadas[0].Maximo) with { AbertaAbaixo = true };
        }

        if (ultimaComum < faixas.Count - 1)
        {
            FaixaPerfil vizinha = agrupadas[^1];
            agrupadas[^1] = CriarFaixa(tipo, vizinha.Minimo, faixas[^1].Maximo) with
            {
                AbertaAbaixo = vizinha.AbertaAbaixo,
                AbertaAcima = true,
            };
        }

        return agrupadas;
    }

    /// <summary>
    /// Cria uma linha com o teórico calculado, ainda sem observado nem atraso.
    /// </summary>
    /// <param name="tipo">Perfil.</param>
    /// <param name="minimo">Menor valor da linha.</param>
    /// <param name="maximo">Maior valor da linha.</param>
    /// <returns>Linha simples, sem ponta agrupada.</returns>
    private static FaixaPerfil CriarFaixa(TipoPerfil tipo, int minimo, int maximo)
    {
        return new FaixaPerfil(
            minimo,
            maximo,
            AbertaAbaixo: false,
            AbertaAcima: false,
            Observado: 0,
            DistribuicaoTeorica.Percentual(tipo, minimo, maximo),
            Atraso: null);
    }
}
