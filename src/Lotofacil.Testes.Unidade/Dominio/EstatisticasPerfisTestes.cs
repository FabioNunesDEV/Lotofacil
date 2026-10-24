// <copyright file="EstatisticasPerfisTestes.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Dominio.Concursos;
using Lotofacil.Dominio.Estatisticas.Perfis;

namespace Lotofacil.Testes.Unidade.Dominio;

/// <summary>
/// Testes do cálculo dos perfis dos concursos: linhas, observado, teórico e atraso.
/// </summary>
public class EstatisticasPerfisTestes
{
    /// <summary>
    /// Os 7 perfis vêm na ordem de exibição.
    /// </summary>
    [Fact]
    public void DevolveOsSetePerfisNaOrdem()
    {
        IReadOnlyList<DistribuicaoPerfil> perfis = EstatisticasPerfis.Calcular(Historico(1, 10, pares: 7), null);

        Assert.Equal(
            [
                TipoPerfil.Pares,
                TipoPerfil.Primos,
                TipoPerfil.MultiplosDe3,
                TipoPerfil.Fibonacci,
                TipoPerfil.Soma,
                TipoPerfil.Repetidas,
                TipoPerfil.Moldura,
            ],
            perfis.Select(perfil => perfil.Tipo));
    }

    /// <summary>
    /// Pares: "≤ 4", 5 a 9 e "≥ 10", com os teóricos agrupados de 1,27% e 2,87%.
    /// </summary>
    [Fact]
    public void LinhasDePares()
    {
        IReadOnlyList<FaixaPerfil> faixas = Faixas(TipoPerfil.Pares, Historico(1, 10, pares: 7), null);

        Assert.Equal(
            [(2, 4, true, false), (5, 5, false, false), (6, 6, false, false), (7, 7, false, false),
                (8, 8, false, false), (9, 9, false, false), (10, 12, false, true)],
            faixas.Select(Limites));
        Assert.Equal(1.27m, Math.Round(faixas[0].Teorico, 2));
        Assert.Equal(31.18m, Math.Round(faixas[3].Teorico, 2));
        Assert.Equal(2.87m, Math.Round(faixas[^1].Teorico, 2));
    }

    /// <summary>
    /// Soma: "≤ 164", quatro faixas de 15 e "≥ 225", com os teóricos agrupados de 4,55% e 5,13%.
    /// </summary>
    [Fact]
    public void LinhasDaSoma()
    {
        IReadOnlyList<FaixaPerfil> faixas = Faixas(TipoPerfil.Soma, Historico(1, 10, pares: 7), null);

        Assert.Equal(
            [(120, 164, true, false), (165, 179, false, false), (180, 194, false, false), (195, 209, false, false),
                (210, 224, false, false), (225, 270, false, true)],
            faixas.Select(Limites));
        Assert.Equal(4.55m, Math.Round(faixas[0].Teorico, 2));
        Assert.Equal(29.07m, Math.Round(faixas[2].Teorico, 2));
        Assert.Equal(29.67m, Math.Round(faixas[3].Teorico, 2));
        Assert.Equal(5.13m, Math.Round(faixas[^1].Teorico, 2));
    }

    /// <summary>
    /// Fibonacci: só a ponta de baixo é agrupada; "7 de Fibonacci" tem chance de pelo menos 1%.
    /// </summary>
    [Fact]
    public void LinhasDeFibonacci()
    {
        IReadOnlyList<FaixaPerfil> faixas = Faixas(TipoPerfil.Fibonacci, Historico(1, 10, pares: 7), null);

        Assert.Equal(
            [(0, 2, true, false), (3, 3, false, false), (4, 4, false, false), (5, 5, false, false),
                (6, 6, false, false), (7, 7, false, false)],
            faixas.Select(Limites));
    }

    /// <summary>
    /// Os teóricos de cada perfil somam 100% e não dependem da janela.
    /// </summary>
    [Fact]
    public void TeoricosSomamCemENaoDependemDaJanela()
    {
        List<ResultadoSorteio> historico = Historico(1, 30, pares: 7);
        IReadOnlyList<DistribuicaoPerfil> todos = EstatisticasPerfis.Calcular(historico, null);
        IReadOnlyList<DistribuicaoPerfil> ultimos10 = EstatisticasPerfis.Calcular(historico, 10);

        Assert.All(todos, perfil => Assert.Equal(100m, Math.Round(perfil.Faixas.Sum(faixa => faixa.Teorico), 10)));
        Assert.Equal(
            todos.SelectMany(perfil => perfil.Faixas.Select(faixa => faixa.Teorico)),
            ultimos10.SelectMany(perfil => perfil.Faixas.Select(faixa => faixa.Teorico)));
    }

    /// <summary>
    /// Com a janela de 10 concursos e 3 deles com 7 pares, a linha "7 pares" tem 30,00%.
    /// </summary>
    [Fact]
    public void ObservadoNaJanela()
    {
        // Os concursos 1 a 20 têm 7 pares; dos 10 últimos (21 a 30), só 3 têm 7 pares e os outros 8.
        List<ResultadoSorteio> historico =
        [
            .. Historico(1, 20, pares: 7),
            .. Historico(21, 23, pares: 7),
            .. Historico(24, 30, pares: 8),
        ];

        IReadOnlyList<FaixaPerfil> faixas = Faixas(TipoPerfil.Pares, historico, 10);

        Assert.Equal(30m, faixas.Single(faixa => faixa.Contem(7)).Observado);
        Assert.Equal(70m, faixas.Single(faixa => faixa.Contem(8)).Observado);
        Assert.Equal(0m, faixas.Single(faixa => faixa.Contem(6)).Observado);
    }

    /// <summary>
    /// Com os concursos 1 a 10 e a janela "Todos", as repetidas consideram só os concursos 2 a 10.
    /// </summary>
    [Fact]
    public void RepetidasSemOPrimeiroConcurso()
    {
        // Concursos alternando 01–15 e 06–20: cada um repete 10 dezenas do anterior.
        List<ResultadoSorteio> historico = Enumerable.Range(1, 10)
            .Select(numero => new ResultadoSorteio(
                numero,
                numero % 2 == 1 ? Enumerable.Range(1, 15).ToArray() : Enumerable.Range(6, 15).ToArray()))
            .ToList();

        IReadOnlyList<FaixaPerfil> faixas = Faixas(TipoPerfil.Repetidas, historico, null);

        Assert.Equal(100m, faixas.Single(faixa => faixa.Contem(10)).Observado);
        Assert.Equal(100m, faixas.Sum(faixa => faixa.Observado));
    }

    /// <summary>
    /// As repetidas somam 100% sobre 9 concursos quando os valores se dividem.
    /// </summary>
    [Fact]
    public void RepetidasDivididasSobreNoveConcursos()
    {
        // O concurso 2 repete 10 dezenas do 1; os concursos 3 a 10 repetem as 15 do anterior.
        List<ResultadoSorteio> historico =
        [
            new(1, Enumerable.Range(1, 15).ToArray()),
            .. Enumerable.Range(2, 9).Select(numero => new ResultadoSorteio(numero, Enumerable.Range(6, 15).ToArray())),
        ];

        IReadOnlyList<FaixaPerfil> faixas = Faixas(TipoPerfil.Repetidas, historico, null);

        Assert.Equal(100m / 9, faixas.Single(faixa => faixa.Contem(10)).Observado);
        Assert.Equal(800m / 9, faixas.Single(faixa => faixa.Contem(15)).Observado);
    }

    /// <summary>
    /// Com um único concurso, as repetidas não têm concursos considerados e ficam zeradas.
    /// </summary>
    [Fact]
    public void RepetidasComUmSoConcurso()
    {
        IReadOnlyList<FaixaPerfil> faixas = Faixas(TipoPerfil.Repetidas, Historico(1, 1, pares: 7), null);

        Assert.All(faixas, faixa => Assert.Equal(0m, faixa.Observado));
        Assert.All(faixas, faixa => Assert.Null(faixa.Atraso));
    }

    /// <summary>
    /// A faixa do último concurso tem atraso 0.
    /// </summary>
    [Fact]
    public void AtrasoDaFaixaDoUltimoConcurso()
    {
        List<ResultadoSorteio> historico = [.. Historico(3801, 3809, pares: 7), .. Historico(3810, 3810, pares: 8)];

        IReadOnlyList<FaixaPerfil> faixas = Faixas(TipoPerfil.Pares, historico, null);

        Assert.Equal(0, faixas.Single(faixa => faixa.Contem(8)).Atraso);
        Assert.Equal(1, faixas.Single(faixa => faixa.Contem(7)).Atraso);
    }

    /// <summary>
    /// O atraso é sobre todo o histórico: "≥ 10 pares" no 3768 dá 42 também com a janela de 10.
    /// </summary>
    [Fact]
    public void AtrasoNaoDependeDaJanela()
    {
        List<ResultadoSorteio> historico =
        [
            .. Historico(3700, 3767, pares: 7),
            .. Historico(3768, 3768, pares: 11),
            .. Historico(3769, 3810, pares: 7),
        ];

        FaixaPerfil todos = Faixas(TipoPerfil.Pares, historico, null)[^1];
        FaixaPerfil ultimos10 = Faixas(TipoPerfil.Pares, historico, 10)[^1];

        Assert.Equal(42, todos.Atraso);
        Assert.Equal(42, ultimos10.Atraso);
        Assert.Equal(0m, ultimos10.Observado);
    }

    /// <summary>
    /// Faixa que nunca saiu fica sem atraso.
    /// </summary>
    [Fact]
    public void FaixaQueNuncaSaiu()
    {
        FaixaPerfil faixa = Faixas(TipoPerfil.Pares, Historico(1, 10, pares: 7), null)[^1];

        Assert.Null(faixa.Atraso);
        Assert.Equal(0m, faixa.Observado);
    }

    /// <summary>
    /// Histórico vazio ou janela menor que 1 são rejeitados.
    /// </summary>
    [Fact]
    public void RejeitaHistoricoVazioEJanelaInvalida()
    {
        Assert.Throws<ArgumentException>(() => EstatisticasPerfis.Calcular([], null));
        Assert.Throws<ArgumentOutOfRangeException>(() => EstatisticasPerfis.Calcular(Historico(1, 3, pares: 7), 0));
    }

    /// <summary>
    /// Linhas de um perfil.
    /// </summary>
    /// <param name="tipo">Perfil.</param>
    /// <param name="historico">Concursos.</param>
    /// <param name="ultimosConcursos">Janela, ou <c>null</c> para todos.</param>
    /// <returns>Linhas do perfil.</returns>
    private static IReadOnlyList<FaixaPerfil> Faixas(
        TipoPerfil tipo,
        IReadOnlyList<ResultadoSorteio> historico,
        int? ultimosConcursos)
    {
        return EstatisticasPerfis.Calcular(historico, ultimosConcursos).Single(perfil => perfil.Tipo == tipo).Faixas;
    }

    /// <summary>
    /// Limites e pontas agrupadas de uma linha.
    /// </summary>
    /// <param name="faixa">Linha.</param>
    /// <returns>Mínimo, máximo e as indicações de ponta.</returns>
    private static (int Minimo, int Maximo, bool AbertaAbaixo, bool AbertaAcima) Limites(FaixaPerfil faixa)
    {
        return (faixa.Minimo, faixa.Maximo, faixa.AbertaAbaixo, faixa.AbertaAcima);
    }

    /// <summary>
    /// Concursos com as mesmas dezenas: a quantidade pedida de pares e o resto de ímpares, das menores.
    /// </summary>
    /// <param name="primeiro">Número do primeiro concurso.</param>
    /// <param name="ultimo">Número do último concurso.</param>
    /// <param name="pares">Quantidade de dezenas pares.</param>
    /// <returns>Concursos do intervalo.</returns>
    private static List<ResultadoSorteio> Historico(int primeiro, int ultimo, int pares)
    {
        int[] dezenas =
        [
            .. Enumerable.Range(1, 12).Select(posicao => posicao * 2).Take(pares),
            .. Enumerable.Range(0, 13).Select(posicao => (posicao * 2) + 1).Take(15 - pares),
        ];

        return Enumerable.Range(primeiro, ultimo - primeiro + 1)
            .Select(numero => new ResultadoSorteio(numero, dezenas))
            .ToList();
    }
}
