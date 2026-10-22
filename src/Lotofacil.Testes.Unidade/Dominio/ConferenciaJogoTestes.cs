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
        Assert.Empty(conferencia.Premios);
    }

    /// <summary>
    /// Jogo de 15 dezenas tem um único prêmio, na faixa dos acertos.
    /// </summary>
    [Fact]
    public void QuinzeDezenasTemUmPremioNaFaixaDosAcertos()
    {
        ConferenciaJogo conferencia = ConferenciaJogo.Conferir(JogoComAcertos(12), ConcursoFabrica.Criar());

        Assert.Equal([new PremioFaixa(12, 1, 14m)], conferencia.Premios);
    }

    /// <summary>
    /// 16 dezenas com 13 acertos: 3 prêmios de 13 e 13 prêmios de 12, somando R$ 287,00.
    /// </summary>
    [Fact]
    public void ApostaMultiplaPremiadaEmVariasFaixas()
    {
        ConferenciaJogo conferencia = ConferenciaJogo.Conferir(JogoComAcertos(13, 16), ConcursoFabrica.Criar());

        Assert.Equal(13, conferencia.Acertos);
        Assert.Equal([new PremioFaixa(13, 3, 35m), new PremioFaixa(12, 13, 14m)], conferencia.Premios);
        Assert.Equal(287m, conferencia.Premio);
        Assert.True(conferencia.Premiado);
    }

    /// <summary>
    /// 16 dezenas com 15 acertos: 1 prêmio de 15 e 15 prêmios de 14.
    /// </summary>
    [Fact]
    public void ApostaMultiplaComQuinzeAcertos()
    {
        ConferenciaJogo conferencia = ConferenciaJogo.Conferir(JogoComAcertos(15, 16), ConcursoFabrica.Criar());

        Assert.Equal([new PremioFaixa(15, 1, 1_712_408.63m), new PremioFaixa(14, 15, 1_502.34m)], conferencia.Premios);
        Assert.Equal(1_712_408.63m + (15 * 1_502.34m), conferencia.Premio);
    }

    /// <summary>
    /// 20 dezenas com 11 acertos: 126 prêmios de 11, somando R$ 882,00.
    /// </summary>
    [Fact]
    public void VinteDezenasComOnzeAcertos()
    {
        ConferenciaJogo conferencia = ConferenciaJogo.Conferir(JogoComAcertos(11, 20), ConcursoFabrica.Criar());

        Assert.Equal([new PremioFaixa(11, 126, 7m)], conferencia.Premios);
        Assert.Equal(882m, conferencia.Premio);
    }

    /// <summary>
    /// Faixas de 11 a 15 aparecem da maior para a menor, só as que têm prêmio.
    /// </summary>
    [Fact]
    public void FaixasEmOrdemDecrescenteSoComPremio()
    {
        ConferenciaJogo conferencia = ConferenciaJogo.Conferir(JogoComAcertos(14, 20), ConcursoFabrica.Criar());

        // 14 acertos e 6 erros: C(14, k) × C(6, 15 − k) para k de 14 a 11.
        Assert.Equal([14, 13, 12, 11], conferencia.Premios.Select(premio => premio.Faixa));
        Assert.Equal([6, 210, 1_820, 5_460], conferencia.Premios.Select(premio => premio.Quantidade));
    }

    /// <summary>
    /// Aposta múltipla com menos de 11 acertos não tem prêmio.
    /// </summary>
    [Fact]
    public void ApostaMultiplaComDezAcertosSemPremio()
    {
        ConferenciaJogo conferencia = ConferenciaJogo.Conferir(JogoComAcertos(10, 20), ConcursoFabrica.Criar());

        Assert.Equal(10, conferencia.Acertos);
        Assert.False(conferencia.Premiado);
        Assert.Equal(0m, conferencia.Premio);
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
    /// <param name="quantidadeDezenas">Quantidade de dezenas do jogo.</param>
    /// <returns>Jogo criado.</returns>
    private static Jogo JogoComAcertos(int acertos, int quantidadeDezenas = 15)
    {
        int[] naoSorteadas = Enumerable.Range(1, 25).Except(ConcursoFabrica.DezenasPadrao).ToArray();
        IEnumerable<int> dezenas = ConcursoFabrica.DezenasPadrao
            .Take(acertos)
            .Concat(naoSorteadas.Take(quantidadeDezenas - acertos));

        return new Jogo(3807, dezenas, Cadastro);
    }
}
