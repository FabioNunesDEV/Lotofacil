// <copyright file="PainelServicoTestes.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Aplicacao.Estatisticas;
using Lotofacil.Aplicacao.Jogos;
using Lotofacil.Aplicacao.Painel;
using Lotofacil.Dominio.Concursos;
using Lotofacil.Dominio.Estatisticas;
using Lotofacil.Dominio.Jogos;
using NSubstitute;

namespace Lotofacil.Testes.Unidade.Aplicacao;

/// <summary>
/// Testes do serviço do Painel.
/// </summary>
public class PainelServicoTestes
{
    /// <summary>
    /// Data de referência em que os prêmios do concurso 3808 (sorteado em 17/10/2026) ainda estão no prazo.
    /// </summary>
    private static readonly DateOnly Hoje = new(2026, 10, 22);

    /// <summary>
    /// Dezenas de um jogo que acerta 10 dezenas do resultado padrão, sem prêmio.
    /// </summary>
    private static readonly int[] DezAcertos = [1, 2, 4, 5, 7, 10, 11, 13, 14, 18, 3, 6, 8, 9, 12];

    /// <summary>
    /// Repositório falso dos concursos.
    /// </summary>
    private readonly IConcursoRepositorio concursos = Substitute.For<IConcursoRepositorio>();

    /// <summary>
    /// Repositório falso dos jogos.
    /// </summary>
    private readonly IJogoRepositorio jogos = Substitute.For<IJogoRepositorio>();

    /// <summary>
    /// Último concurso gravado nos testes.
    /// </summary>
    private readonly Concurso ultimo = ConcursoFabrica.Criar(3808);

    /// <summary>
    /// Com a base vazia, não há resumo.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task BaseVaziaDevolveNulo()
    {
        this.concursos.ObterUltimoNumeroAsync(Arg.Any<CancellationToken>()).Returns((int?)null);

        ResumoPainel? resumo = await this.CriarServico().ObterResumoAsync(Hoje, TestContext.Current.CancellationToken);

        Assert.Null(resumo);
    }

    /// <summary>
    /// O resumo traz o total de concursos, o último concurso e o próximo.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task ResumeOsConcursos()
    {
        this.PrepararBase();

        ResumoPainel? resumo = await this.CriarServico().ObterResumoAsync(Hoje, TestContext.Current.CancellationToken);

        Assert.NotNull(resumo);
        Assert.Equal(3808, resumo.TotalConcursos);
        Assert.Same(this.ultimo, resumo.UltimoConcurso);
        Assert.Equal(3809, resumo.ProximoConcurso);
    }

    /// <summary>
    /// Os rankings são os mesmos das estatísticas sobre todo o histórico.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task EstatisticasSobreTodoOHistorico()
    {
        this.PrepararBase();
        EstatisticasDezenas? esperadas = await new EstatisticasServico(this.concursos)
            .CalcularAsync(null, TestContext.Current.CancellationToken);

        ResumoPainel? resumo = await this.CriarServico().ObterResumoAsync(Hoje, TestContext.Current.CancellationToken);

        Assert.NotNull(resumo);
        Assert.NotNull(esperadas);
        Assert.Equal(esperadas.TotalConcursos, resumo.Estatisticas.ConcursosNaJanela);
        Assert.Equal(esperadas.MaisSorteadas, resumo.Estatisticas.MaisSorteadas, CompararRanking);
        Assert.Equal(esperadas.MaisAtrasadas, resumo.Estatisticas.MaisAtrasadas, CompararRanking);
    }

    /// <summary>
    /// Os jogos do último concurso vêm conferidos.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task JogosDoUltimoConcursoConferidos()
    {
        this.PrepararBase(
            new Jogo(3808, ConcursoFabrica.DezenasPadrao, DateTime.Now),
            new Jogo(3808, DezAcertos, DateTime.Now),
            new Jogo(3810, Enumerable.Range(1, 15), DateTime.Now));

        ResumoPainel? resumo = await this.CriarServico().ObterResumoAsync(Hoje, TestContext.Current.CancellationToken);

        Assert.NotNull(resumo);
        GrupoJogos? grupo = resumo.JogosUltimoConcurso;
        Assert.NotNull(grupo);
        Assert.Equal(3808, grupo.NumeroConcurso);
        Assert.Equal([15, 10], grupo.Jogos.Select(jogo => jogo.Conferencia?.Acertos));
    }

    /// <summary>
    /// Sem jogos no último concurso, o grupo é nulo.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task SemJogosNoUltimoConcurso()
    {
        this.PrepararBase(new Jogo(3810, Enumerable.Range(1, 15), DateTime.Now));

        ResumoPainel? resumo = await this.CriarServico().ObterResumoAsync(Hoje, TestContext.Current.CancellationToken);

        Assert.NotNull(resumo);
        Assert.Null(resumo.JogosUltimoConcurso);
    }

    /// <summary>
    /// Os jogos aguardando somam os jogos de concursos não sorteados, com os números em ordem.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task JogosAguardandoEmVariosConcursos()
    {
        this.PrepararBase(
            new Jogo(3810, Enumerable.Range(1, 15), DateTime.Now),
            new Jogo(3809, Enumerable.Range(1, 15), DateTime.Now),
            new Jogo(3809, Enumerable.Range(2, 15), DateTime.Now),
            new Jogo(3808, ConcursoFabrica.DezenasPadrao, DateTime.Now));

        ResumoPainel? resumo = await this.CriarServico().ObterResumoAsync(Hoje, TestContext.Current.CancellationToken);

        Assert.NotNull(resumo);
        Assert.Equal(3, resumo.JogosAguardando);
        Assert.Equal([3809, 3810], resumo.ConcursosAguardando);
    }

    /// <summary>
    /// Sem jogos aguardando, a quantidade é zero e não há concursos.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task SemJogosAguardando()
    {
        this.PrepararBase(new Jogo(3808, ConcursoFabrica.DezenasPadrao, DateTime.Now));

        ResumoPainel? resumo = await this.CriarServico().ObterResumoAsync(Hoje, TestContext.Current.CancellationToken);

        Assert.NotNull(resumo);
        Assert.Equal(0, resumo.JogosAguardando);
        Assert.Empty(resumo.ConcursosAguardando);
    }

    /// <summary>
    /// Os prêmios a receber somam só os concursos dentro do prazo (sorteio de 17/10/2026 prescreve em 15/01/2027).
    /// </summary>
    /// <param name="ano">Ano de hoje.</param>
    /// <param name="mes">Mês de hoje.</param>
    /// <param name="dia">Dia de hoje.</param>
    /// <param name="esperado">Prêmios a receber.</param>
    /// <returns>Tarefa do teste.</returns>
    [Theory]
    [InlineData(2027, 1, 15, 1_712_408.63)]
    [InlineData(2027, 1, 16, 0)]
    public async Task PremiosAReceberSomenteNoPrazo(int ano, int mes, int dia, decimal esperado)
    {
        this.PrepararBase(
            new Jogo(3808, ConcursoFabrica.DezenasPadrao, DateTime.Now),
            new Jogo(3808, DezAcertos, DateTime.Now));

        ResumoPainel? resumo = await this.CriarServico().ObterResumoAsync(
            new DateOnly(ano, mes, dia),
            TestContext.Current.CancellationToken);

        Assert.NotNull(resumo);
        Assert.Equal(esperado, resumo.PremiosAReceber);
    }

    /// <summary>
    /// Compara dois rankings pelas dezenas, valores e empatadas.
    /// </summary>
    /// <param name="esperado">Ranking esperado.</param>
    /// <param name="atual">Ranking obtido.</param>
    /// <returns><c>true</c> se forem iguais.</returns>
    private static bool CompararRanking(Ranking esperado, Ranking atual)
    {
        return esperado.Empatadas == atual.Empatadas && esperado.Itens.SequenceEqual(atual.Itens);
    }

    /// <summary>
    /// Prepara uma base com os concursos 1 a 3808 e os jogos informados.
    /// </summary>
    /// <param name="jogosCadastrados">Jogos cadastrados.</param>
    private void PrepararBase(params Jogo[] jogosCadastrados)
    {
        List<ResultadoSorteio> historico = Enumerable.Range(1, 3808)
            .Select(numero => new ResultadoSorteio(
                numero,
                numero % 2 == 0 ? ConcursoFabrica.DezenasPadrao : Enumerable.Range(1, 15).ToArray()))
            .ToList();
        this.concursos.ObterUltimoNumeroAsync(Arg.Any<CancellationToken>()).Returns(3808);
        this.concursos.ContarAsync(Arg.Any<CancellationToken>()).Returns(3808);
        this.concursos.ObterAsync(3808, Arg.Any<CancellationToken>()).Returns(this.ultimo);
        this.concursos.ListarResultadosAsync(Arg.Any<CancellationToken>()).Returns(historico);
        this.jogos.ListarAsync(Arg.Any<CancellationToken>()).Returns(jogosCadastrados);
    }

    /// <summary>
    /// Cria o serviço com os repositórios falsos.
    /// </summary>
    /// <returns>O serviço do Painel.</returns>
    private PainelServico CriarServico()
    {
        return new PainelServico(
            this.concursos,
            new EstatisticasServico(this.concursos),
            new JogosServico(this.jogos, this.concursos));
    }
}
