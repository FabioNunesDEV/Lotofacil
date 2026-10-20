// <copyright file="ConferenciaJogoTestes.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Dominio.Concursos;
using Lotofacil.Dominio.Jogos;

namespace Lotofacil.Testes.Unidade.Dominio;

/// <summary>
/// Testes da conferência de um jogo contra o concurso sorteado.
/// </summary>
public class ConferenciaJogoTestes
{
    /// <summary>
    /// Momento de cadastro usado nos testes.
    /// </summary>
    private static readonly DateTime Cadastro = new(2026, 10, 20, 17, 0, 0);

    /// <summary>
    /// 13 acertos pagam o prêmio da faixa de 13 do rateio gravado.
    /// </summary>
    [Fact]
    public void FaixaFixaPeloRateioGravado()
    {
        ConferenciaJogo conferencia = ConferenciaJogo.Conferir(JogoComAcertos(13), ConcursoFabrica.Criar());

        Assert.Equal(13, conferencia.Acertos);
        Assert.Equal(35m, conferencia.Premio);
        Assert.True(conferencia.Premiado);
    }

    /// <summary>
    /// 14 acertos pagam o valor variável da faixa de 14 do concurso.
    /// </summary>
    [Fact]
    public void FaixaVariavelPeloRateioGravado()
    {
        ConferenciaJogo conferencia = ConferenciaJogo.Conferir(JogoComAcertos(14), ConcursoFabrica.Criar());

        Assert.Equal(1_502.34m, conferencia.Premio);
    }

    /// <summary>
    /// Concurso antigo com faixa fixa diferente da atual: vale o valor gravado, não o atual.
    /// </summary>
    [Fact]
    public void UsaOValorAntigoGravadoNaFaixaFixa()
    {
        List<Rateio> rateiosAntigos =
        [
            new(15, 1, 596_323.70m),
            new(14, 184, 1_388.95m),
            new(13, 6_232, 10m),
            new(12, 81_252, 4m),
            new(11, 478_188, 2m),
        ];
        Concurso concurso = ConcursoFabrica.Criar(rateios: rateiosAntigos);

        Assert.Equal(10m, ConferenciaJogo.Conferir(JogoComAcertos(13), concurso).Premio);
        Assert.Equal(2m, ConferenciaJogo.Conferir(JogoComAcertos(11), concurso).Premio);
    }

    /// <summary>
    /// 15 acertos pagam o prêmio principal do concurso.
    /// </summary>
    [Fact]
    public void QuinzeAcertos()
    {
        ConferenciaJogo conferencia = ConferenciaJogo.Conferir(JogoComAcertos(15), ConcursoFabrica.Criar());

        Assert.Equal(15, conferencia.Acertos);
        Assert.Equal(1_712_408.63m, conferencia.Premio);
    }

    /// <summary>
    /// Menos de 11 acertos não tem prêmio.
    /// </summary>
    [Fact]
    public void MenosDeOnzeAcertosSemPremio()
    {
        ConferenciaJogo conferencia = ConferenciaJogo.Conferir(JogoComAcertos(10), ConcursoFabrica.Criar());

        Assert.Equal(10, conferencia.Acertos);
        Assert.Equal(0m, conferencia.Premio);
        Assert.False(conferencia.Premiado);
    }

    /// <summary>
    /// Jogo de outro concurso não pode ser conferido.
    /// </summary>
    [Fact]
    public void RejeitaConcursoDeOutroNumero()
    {
        var jogo = new Jogo(3809, ConcursoFabrica.DezenasPadrao, Cadastro);

        Assert.Throws<ArgumentException>(() => ConferenciaJogo.Conferir(jogo, ConcursoFabrica.Criar(numero: 3808)));
    }

    /// <summary>
    /// Cria um jogo do concurso padrão (3807) com a quantidade de acertos pedida sobre as dezenas padrão.
    /// </summary>
    /// <param name="acertos">Quantidade de dezenas sorteadas no jogo.</param>
    /// <returns>Jogo criado.</returns>
    private static Jogo JogoComAcertos(int acertos)
    {
        int[] naoSorteadas = Enumerable.Range(1, 25).Except(ConcursoFabrica.DezenasPadrao).ToArray();
        IEnumerable<int> dezenas = ConcursoFabrica.DezenasPadrao.Take(acertos).Concat(naoSorteadas.Take(15 - acertos));

        return new Jogo(3807, dezenas, Cadastro);
    }
}
