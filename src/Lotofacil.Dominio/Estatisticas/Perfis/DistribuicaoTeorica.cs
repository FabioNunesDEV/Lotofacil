// <copyright file="DistribuicaoTeorica.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

namespace Lotofacil.Dominio.Estatisticas.Perfis;

/// <summary>
/// Quantas das 3.268.760 combinações de 15 dezenas têm cada valor de cada perfil.
/// </summary>
public static class DistribuicaoTeorica
{
    /// <summary>
    /// Largura das faixas de soma, a partir da menor soma possível.
    /// </summary>
    public const int LarguraFaixaSoma = 15;

    /// <summary>
    /// Quantidade de combinações de 15 dezenas entre 25.
    /// </summary>
    public static readonly int TotalCombinacoes =
        Combinatoria.Combinacoes(RegrasLotofacil.QuantidadeDezenas, RegrasLotofacil.DezenasSorteadas);

    /// <summary>
    /// Combinações de cada perfil, indexadas pelo valor (quantidade de dezenas ou soma).
    /// </summary>
    private static readonly IReadOnlyDictionary<TipoPerfil, int[]> CombinacoesPorValor =
        Enum.GetValues<TipoPerfil>().ToDictionary(tipo => tipo, CalcularCombinacoes);

    /// <summary>
    /// Quantidade de combinações de 15 dezenas com o valor no perfil.
    /// </summary>
    /// <param name="tipo">Perfil.</param>
    /// <param name="valor">Valor do perfil.</param>
    /// <returns>Quantidade de combinações; zero para um valor impossível.</returns>
    public static int Combinacoes(TipoPerfil tipo, int valor)
    {
        int[] combinacoes = CombinacoesPorValor[tipo];

        return valor >= 0 && valor < combinacoes.Length ? combinacoes[valor] : 0;
    }

    /// <summary>
    /// Chance de um jogo de 15 dezenas ter o valor do perfil entre os limites.
    /// </summary>
    /// <param name="tipo">Perfil.</param>
    /// <param name="minimo">Menor valor, inclusive.</param>
    /// <param name="maximo">Maior valor, inclusive.</param>
    /// <returns>Percentual de 0 a 100.</returns>
    public static decimal Percentual(TipoPerfil tipo, int minimo, int maximo)
    {
        int combinacoes = Enumerable.Range(minimo, maximo - minimo + 1).Sum(valor => Combinacoes(tipo, valor));

        return combinacoes * 100m / TotalCombinacoes;
    }

    /// <summary>
    /// Menor valor possível do perfil.
    /// </summary>
    /// <param name="tipo">Perfil.</param>
    /// <returns>Menor valor com alguma combinação.</returns>
    public static int MenorValor(TipoPerfil tipo)
    {
        return Array.FindIndex(CombinacoesPorValor[tipo], combinacoes => combinacoes > 0);
    }

    /// <summary>
    /// Maior valor possível do perfil.
    /// </summary>
    /// <param name="tipo">Perfil.</param>
    /// <returns>Maior valor com alguma combinação.</returns>
    public static int MaiorValor(TipoPerfil tipo)
    {
        return Array.FindLastIndex(CombinacoesPorValor[tipo], combinacoes => combinacoes > 0);
    }

    /// <summary>
    /// Par de valores vizinhos com mais combinações; na soma, par de faixas vizinhas de <see cref="LarguraFaixaSoma"/>.
    /// </summary>
    /// <param name="tipo">Perfil.</param>
    /// <returns>Menor e maior valor do par; no empate, o par de menor valor.</returns>
    public static (int Minimo, int Maximo) FaixaMaisComum(TipoPerfil tipo)
    {
        int menor = MenorValor(tipo);
        int maior = MaiorValor(tipo);
        int largura = tipo == TipoPerfil.Soma ? LarguraFaixaSoma : 1;
        int quantidadeFaixas = (maior - menor + 1) / largura;

        (int Minimo, int Maximo) maisComum = (menor, menor);
        decimal maiorPercentual = -1m;
        for (int posicao = 0; posicao < quantidadeFaixas - 1; posicao++)
        {
            int minimo = menor + (posicao * largura);

            // O último par vai até o maior valor, como a última faixa da soma (255–270).
            int maximo = posicao == quantidadeFaixas - 2 ? maior : minimo + (2 * largura) - 1;
            decimal percentual = Percentual(tipo, minimo, maximo);
            if (percentual > maiorPercentual)
            {
                maisComum = (minimo, maximo);
                maiorPercentual = percentual;
            }
        }

        return maisComum;
    }

    /// <summary>
    /// Calcula as combinações de cada valor de um perfil.
    /// </summary>
    /// <param name="tipo">Perfil.</param>
    /// <returns>Combinações indexadas pelo valor.</returns>
    private static int[] CalcularCombinacoes(TipoPerfil tipo)
    {
        return tipo switch
        {
            TipoPerfil.Pares => Hipergeometrica(ValorPerfil.Pares.Count),
            TipoPerfil.Primos => Hipergeometrica(ValorPerfil.Primos.Count),
            TipoPerfil.MultiplosDe3 => Hipergeometrica(ValorPerfil.MultiplosDe3.Count),
            TipoPerfil.Fibonacci => Hipergeometrica(ValorPerfil.Fibonacci.Count),
            TipoPerfil.Soma => CombinacoesPorSoma(),
            TipoPerfil.Repetidas => Hipergeometrica(RegrasLotofacil.DezenasSorteadas),
            TipoPerfil.Moldura => Hipergeometrica(ValorPerfil.Moldura.Count),
            _ => throw new ArgumentOutOfRangeException(nameof(tipo)),
        };
    }

    /// <summary>
    /// Combinações com k dezenas de um conjunto de K dezenas: C(K, k) × C(25 − K, 15 − k).
    /// </summary>
    /// <param name="tamanhoConjunto">Quantidade de dezenas do conjunto (K).</param>
    /// <returns>Combinações indexadas por k, de 0 a 15.</returns>
    private static int[] Hipergeometrica(int tamanhoConjunto)
    {
        int foraDoConjunto = RegrasLotofacil.QuantidadeDezenas - tamanhoConjunto;

        return Enumerable.Range(0, RegrasLotofacil.DezenasSorteadas + 1)
            .Select(k => Combinatoria.Combinacoes(tamanhoConjunto, k)
                * Combinatoria.Combinacoes(foraDoConjunto, RegrasLotofacil.DezenasSorteadas - k))
            .ToArray();
    }

    /// <summary>
    /// Combinações de 15 dezenas com cada soma, por programação dinâmica.
    /// </summary>
    /// <returns>Combinações indexadas pela soma, de 0 a 270.</returns>
    private static int[] CombinacoesPorSoma()
    {
        int quantidade = RegrasLotofacil.DezenasSorteadas;
        int somaMaxima = Enumerable.Range(RegrasLotofacil.DezenaMaxima - quantidade + 1, quantidade).Sum();

        // maneiras[c, s]: quantos subconjuntos de c dezenas, entre as já vistas, somam s.
        int[,] maneiras = new int[quantidade + 1, somaMaxima + 1];
        maneiras[0, 0] = 1;
        for (int dezena = RegrasLotofacil.DezenaMinima; dezena <= RegrasLotofacil.DezenaMaxima; dezena++)
        {
            // Percorrer de cima para baixo usa cada dezena uma só vez.
            for (int c = Math.Min(dezena, quantidade); c >= 1; c--)
            {
                for (int soma = somaMaxima; soma >= dezena; soma--)
                {
                    maneiras[c, soma] += maneiras[c - 1, soma - dezena];
                }
            }
        }

        return Enumerable.Range(0, somaMaxima + 1).Select(soma => maneiras[quantidade, soma]).ToArray();
    }
}
