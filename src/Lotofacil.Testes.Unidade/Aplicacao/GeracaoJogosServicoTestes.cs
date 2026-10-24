// <copyright file="GeracaoJogosServicoTestes.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Aplicacao.Estatisticas;
using Lotofacil.Aplicacao.Geracao;
using Lotofacil.Aplicacao.Jogos;
using Lotofacil.Dominio.Concursos;
using Lotofacil.Dominio.Estatisticas.Perfis;
using Lotofacil.Dominio.Geracao;
using Lotofacil.Dominio.Jogos;
using NSubstitute;

namespace Lotofacil.Testes.Unidade.Aplicacao;

/// <summary>
/// Testes do serviço de geração de jogos.
/// </summary>
public class GeracaoJogosServicoTestes
{
    /// <summary>
    /// Fixas que deixam só uma dezena a sortear entre as livres 15 a 25.
    /// </summary>
    private static readonly int[] Fixas1a14 = [.. Enumerable.Range(1, 14)];

    /// <summary>
    /// Repositório falso dos concursos.
    /// </summary>
    private readonly IConcursoRepositorio concursos = Substitute.For<IConcursoRepositorio>();

    /// <summary>
    /// Repositório falso dos jogos.
    /// </summary>
    private readonly IJogoRepositorio jogos = Substitute.For<IJogoRepositorio>();

    /// <summary>
    /// Aleatório que escolhe sempre a primeira dezena disponível.
    /// </summary>
    private readonly AleatorioZero aleatorio = new();

    /// <summary>
    /// Com a base vazia, não há contexto e a geração é recusada.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task BaseVazia()
    {
        this.concursos.ObterUltimoNumeroAsync(Arg.Any<CancellationToken>()).Returns((int?)null);
        this.concursos.ListarResultadosAsync(Arg.Any<CancellationToken>()).Returns([]);
        GeracaoJogosServico servico = this.CriarServico();

        Assert.Null(await servico.ObterContextoAsync(TestContext.Current.CancellationToken));
        JogosGerados gerados = await servico.GerarAsync(Parametros(), null, TestContext.Current.CancellationToken);
        Assert.Empty(gerados.Jogos);
        Assert.Equal(GeracaoJogosServico.MensagemBaseVazia, gerados.Recusa);
    }

    /// <summary>
    /// O contexto traz o último concurso, o resultado dele, o total e o próximo concurso.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task Contexto()
    {
        this.PrepararHistorico();

        ContextoGeracao? contexto = await this.CriarServico().ObterContextoAsync(TestContext.Current.CancellationToken);

        Assert.NotNull(contexto);
        Assert.Equal(20, contexto.UltimoConcurso);
        Assert.Equal(21, contexto.ProximoConcurso);
        Assert.Equal(20, contexto.TotalConcursos);
        Assert.Equal(Enumerable.Range(11, 15), contexto.UltimoResultado);
    }

    /// <summary>
    /// A janela pedida chega às estatísticas: os pesos de Mais sorteadas mudam com ela.
    /// </summary>
    /// <param name="janela">Janela da análise.</param>
    /// <param name="pesoTotal">Soma dos pesos das livres 15 a 25 nessa janela.</param>
    /// <returns>Tarefa do teste.</returns>
    [Theory]
    [InlineData(10, 121)]
    [InlineData(null, 31)]
    public async Task UsaAJanelaPedida(int? janela, int pesoTotal)
    {
        this.PrepararHistorico();

        await this.CriarServico().GerarAsync(Parametros(), janela, TestContext.Current.CancellationToken);

        // Nos últimos 10, as dezenas 11 a 25 saíram 10 vezes (peso 11); em todo o histórico, 11 a 15 saíram 20 vezes
        // (peso 21) e as demais 10 (peso 1).
        Assert.Equal([pesoTotal], this.aleatorio.Maximos);
    }

    /// <summary>
    /// Cada jogo vem com o perfil em relação ao último concurso gravado.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task CalculaOPerfilContraOUltimoConcurso()
    {
        this.PrepararHistorico();

        JogosGerados gerados = await this.CriarServico().GerarAsync(Parametros(), null, TestContext.Current.CancellationToken);

        JogoGerado jogo = Assert.Single(gerados.Jogos);
        Assert.Equal(Enumerable.Range(1, 15), jogo.Dezenas);
        Assert.Equal(new PerfilJogo(7, 8, 120, 5), jogo.Perfil);
        Assert.Null(gerados.Recusa);
    }

    /// <summary>
    /// Recusa do gerador chega ao resultado.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task RepassaARecusaDoGerador()
    {
        this.PrepararHistorico();
        var parametros = new ParametrosGeracao(10, 15, EstrategiaGeracao.Aleatoria, Enumerable.Range(1, 16), []);

        JogosGerados gerados = await this.CriarServico().GerarAsync(parametros, null, TestContext.Current.CancellationToken);

        Assert.Equal("Há 16 dezenas fixas, mais que as 15 dezenas por jogo.", gerados.Recusa);
    }

    /// <summary>
    /// O filtro de repetidas usa o último concurso gravado (11 a 25): com as fixas 01 a 14, todo jogo tem 5
    /// repetidas. O aleatório controlado só acha um jogo, e o resultado informa a quantidade pedida.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task FiltroDeRepetidasUsaOUltimoConcurso()
    {
        this.PrepararHistorico();
        GeracaoJogosServico servico = this.CriarServico();

        JogosGerados cinco = await servico.GerarAsync(ComRepetidas(5, 2), null, TestContext.Current.CancellationToken);
        JogosGerados quatro = await servico.GerarAsync(ComRepetidas(4, 1), null, TestContext.Current.CancellationToken);

        JogoGerado jogo = Assert.Single(cinco.Jogos);
        Assert.Equal(Enumerable.Range(1, 15), jogo.Dezenas);
        Assert.True(cinco.ComFiltros);
        Assert.Equal(2, cinco.JogosPedidos);
        Assert.Empty(quatro.Jogos);
        Assert.Equal(ResultadoGeracao.MensagemNenhumJogo, quatro.Recusa);
    }

    /// <summary>
    /// Salvar grava pelo serviço de jogos.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task SalvaPeloServicoDeJogos()
    {
        this.PrepararHistorico();

        ResultadoCadastroVarios resultado = await this.CriarServico().SalvarAsync(
            21,
            [[.. Enumerable.Range(1, 15)], [.. Enumerable.Range(2, 15)]],
            TestContext.Current.CancellationToken);

        Assert.Equal(new ResultadoCadastroVarios(2, 0, null), resultado);
        await this.jogos.Received(2).AdicionarAsync(Arg.Is<Jogo>(jogo => jogo.NumeroConcurso == 21), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Um jogo de 15 dezenas com as fixas 01 a 14, pela estratégia Mais sorteadas.
    /// </summary>
    /// <returns>Os parâmetros.</returns>
    private static ParametrosGeracao Parametros()
    {
        return new ParametrosGeracao(1, 15, EstrategiaGeracao.MaisSorteadas, Fixas1a14, []);
    }

    /// <summary>
    /// Jogos de 15 dezenas com as fixas 01 a 14 e exatamente a quantidade de repetidas pedida.
    /// </summary>
    /// <param name="repetidas">Repetidas exigidas.</param>
    /// <param name="quantidade">Quantidade de jogos.</param>
    /// <returns>Os parâmetros.</returns>
    private static ParametrosGeracao ComRepetidas(int repetidas, int quantidade)
    {
        var filtros = new FiltrosGeracao([new LimitePerfil(TipoPerfil.Repetidas, repetidas, repetidas)], null);

        return new ParametrosGeracao(quantidade, 15, EstrategiaGeracao.MaisSorteadas, Fixas1a14, [], filtros);
    }

    /// <summary>
    /// Base com 20 concursos: 1 a 10 sortearam 01 a 15; 11 a 20 sortearam 11 a 25.
    /// </summary>
    private void PrepararHistorico()
    {
        List<ResultadoSorteio> historico = Enumerable.Range(1, 20)
            .Select(numero => new ResultadoSorteio(numero, [.. Enumerable.Range(numero <= 10 ? 1 : 11, 15)]))
            .ToList();
        this.concursos.ListarResultadosAsync(Arg.Any<CancellationToken>()).Returns(historico);
        this.concursos.ObterUltimoNumeroAsync(Arg.Any<CancellationToken>()).Returns(20);
        this.concursos.ContarAsync(Arg.Any<CancellationToken>()).Returns(20);
        this.concursos.ObterAsync(20, Arg.Any<CancellationToken>())
            .Returns(ConcursoFabrica.Criar(numero: 20, dezenas: Enumerable.Range(11, 15)));
    }

    /// <summary>
    /// Cria o serviço com os repositórios falsos e o aleatório controlado.
    /// </summary>
    /// <returns>O serviço de geração.</returns>
    private GeracaoJogosServico CriarServico()
    {
        return new GeracaoJogosServico(
            this.concursos,
            new EstatisticasServico(this.concursos),
            new JogosServico(this.jogos, this.concursos),
            this.aleatorio);
    }
}
