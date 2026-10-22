// <copyright file="JogosServicoTestes.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Aplicacao.Jogos;
using Lotofacil.Dominio.Concursos;
using Lotofacil.Dominio.Jogos;
using NSubstitute;

namespace Lotofacil.Testes.Unidade.Aplicacao;

/// <summary>
/// Testes do serviço de jogos.
/// </summary>
public class JogosServicoTestes
{
    /// <summary>
    /// Dezenas de um jogo usado nos testes de cadastro.
    /// </summary>
    private static readonly int[] DezenasJogo = Enumerable.Range(1, 15).ToArray();

    /// <summary>
    /// Repositório falso dos jogos.
    /// </summary>
    private readonly IJogoRepositorio jogos = Substitute.For<IJogoRepositorio>();

    /// <summary>
    /// Repositório falso dos concursos.
    /// </summary>
    private readonly IConcursoRepositorio concursos = Substitute.For<IConcursoRepositorio>();

    /// <summary>
    /// Jogo para concurso futuro e sem duplicado é gravado com a mensagem de confirmação.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task CadastraJogoParaConcursoFuturo()
    {
        this.concursos.ObterUltimoNumeroAsync(Arg.Any<CancellationToken>()).Returns(3808);

        ResultadoCadastro resultado = await this.CriarServico().CadastrarAsync(3809, DezenasJogo, TestContext.Current.CancellationToken);

        Assert.Equal(new ResultadoCadastro(true, "Jogo salvo para o concurso 3809."), resultado);
        await this.jogos.Received(1).AdicionarAsync(
            Arg.Is<Jogo>(jogo => jogo.NumeroConcurso == 3809 && jogo.Dezenas.SequenceEqual(DezenasJogo)),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Aposta de 20 dezenas é gravada como qualquer outro jogo.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task CadastraApostaDeVinteDezenas()
    {
        int[] vinteDezenas = Enumerable.Range(1, 20).ToArray();
        this.concursos.ObterUltimoNumeroAsync(Arg.Any<CancellationToken>()).Returns(3808);

        ResultadoCadastro resultado = await this.CriarServico().CadastrarAsync(3809, vinteDezenas, TestContext.Current.CancellationToken);

        Assert.Equal(new ResultadoCadastro(true, "Jogo salvo para o concurso 3809."), resultado);
        await this.jogos.Received(1).AdicionarAsync(
            Arg.Is<Jogo>(jogo => jogo.Dezenas.SequenceEqual(vinteDezenas)),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Com a base vazia, o cadastro pede a importação do histórico.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task RecusaComBaseVazia()
    {
        this.concursos.ObterUltimoNumeroAsync(Arg.Any<CancellationToken>()).Returns((int?)null);

        ResultadoCadastro resultado = await this.CriarServico().CadastrarAsync(1, DezenasJogo, TestContext.Current.CancellationToken);

        Assert.Equal(
            new ResultadoCadastro(false, "Importe o histórico na página Resultados antes de cadastrar jogos."),
            resultado);
        await this.jogos.DidNotReceiveWithAnyArgs().AdicionarAsync(default!, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Concurso igual ou anterior ao último gravado é recusado.
    /// </summary>
    /// <param name="numeroConcurso">Concurso do jogo.</param>
    /// <returns>Tarefa do teste.</returns>
    [Theory]
    [InlineData(3808)]
    [InlineData(100)]
    public async Task RecusaConcursoJaSorteado(int numeroConcurso)
    {
        this.concursos.ObterUltimoNumeroAsync(Arg.Any<CancellationToken>()).Returns(3808);

        ResultadoCadastro resultado = await this.CriarServico().CadastrarAsync(numeroConcurso, DezenasJogo, TestContext.Current.CancellationToken);

        Assert.Equal(
            new ResultadoCadastro(false, "Só é possível cadastrar jogos para concursos depois do 3808."),
            resultado);
        await this.jogos.DidNotReceiveWithAnyArgs().AdicionarAsync(default!, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// O mesmo jogo no mesmo concurso é recusado.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task RecusaJogoDuplicado()
    {
        this.concursos.ObterUltimoNumeroAsync(Arg.Any<CancellationToken>()).Returns(3808);
        this.jogos.ExisteAsync(3809, Arg.Any<IEnumerable<int>>(), Arg.Any<CancellationToken>()).Returns(true);

        ResultadoCadastro resultado = await this.CriarServico().CadastrarAsync(3809, DezenasJogo, TestContext.Current.CancellationToken);

        Assert.Equal(new ResultadoCadastro(false, "Este jogo já está cadastrado para o concurso 3809."), resultado);
        await this.jogos.DidNotReceiveWithAnyArgs().AdicionarAsync(default!, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// A busca de duplicado usa as dezenas do jogo inteiro: 16 dezenas iguais são recusadas.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task RecusaApostaMultiplaDuplicada()
    {
        int[] dezesseis = [16, .. DezenasJogo];
        this.concursos.ObterUltimoNumeroAsync(Arg.Any<CancellationToken>()).Returns(3808);
        this.jogos.ExisteAsync(
            3809,
            Arg.Is<IEnumerable<int>>(dezenas => dezenas.SequenceEqual(Enumerable.Range(1, 16))),
            Arg.Any<CancellationToken>()).Returns(true);

        ResultadoCadastro resultado = await this.CriarServico().CadastrarAsync(3809, dezesseis, TestContext.Current.CancellationToken);

        Assert.Equal(new ResultadoCadastro(false, "Este jogo já está cadastrado para o concurso 3809."), resultado);
        await this.jogos.DidNotReceiveWithAnyArgs().AdicionarAsync(default!, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// O concurso sugerido é o seguinte ao último gravado; com a base vazia, não há sugestão.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task SugereOProximoConcurso()
    {
        this.concursos.ObterUltimoNumeroAsync(Arg.Any<CancellationToken>()).Returns(3808, (int?)null);

        Assert.Equal(3809, await this.CriarServico().ObterProximoConcursoAsync(TestContext.Current.CancellationToken));
        Assert.Null(await this.CriarServico().ObterProximoConcursoAsync(TestContext.Current.CancellationToken));
    }

    /// <summary>
    /// Os jogos são agrupados por concurso, do maior para o menor, numerados na ordem de cadastro, e só os
    /// grupos de concursos sorteados são conferidos.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task ListaAgrupadoEConferido()
    {
        Concurso concurso3808 = ConcursoFabrica.Criar(numero: 3808);
        var premiado = new Jogo(3808, ConcursoFabrica.DezenasPadrao, DateTime.Now);
        var aguardando = new Jogo(3810, DezenasJogo, DateTime.Now);
        int[] onzeAcertos = [.. ConcursoFabrica.DezenasPadrao.Take(11), 3, 6, 8, 9];
        var comOnze = new Jogo(3808, onzeAcertos, DateTime.Now);
        this.jogos.ListarAsync(Arg.Any<CancellationToken>()).Returns([premiado, aguardando, comOnze]);
        this.concursos.ObterAsync(3808, Arg.Any<CancellationToken>()).Returns(concurso3808);
        this.concursos.ObterAsync(3810, Arg.Any<CancellationToken>()).Returns((Concurso?)null);

        IReadOnlyList<GrupoJogos> grupos = await this.CriarServico().ListarAsync(TestContext.Current.CancellationToken);

        Assert.Equal([3810, 3808], grupos.Select(grupo => grupo.NumeroConcurso));

        GrupoJogos grupo3810 = grupos[0];
        Assert.False(grupo3810.Conferido);
        Assert.Null(grupo3810.Jogos.Single().Conferencia);
        Assert.Null(grupo3810.Prescricao);

        GrupoJogos grupo3808 = grupos[1];
        Assert.True(grupo3808.Conferido);
        Assert.Equal([1, 2], grupo3808.Jogos.Select(jogo => jogo.Numero));
        Assert.Equal([premiado, comOnze], grupo3808.Jogos.Select(jogo => jogo.Jogo));
        Assert.Equal(15, grupo3808.Jogos[0].Conferencia?.Acertos);
        Assert.Equal(11, grupo3808.Jogos[1].Conferencia?.Acertos);
        Assert.Equal(1_712_408.63m + 7m, grupo3808.PremioTotal);
        Assert.True(grupo3808.Premiado);
    }

    /// <summary>
    /// A quantidade de apostas do grupo soma as apostas de 15 dezenas de cada jogo: 1 + 1 + 16.
    /// </summary>
    [Fact]
    public void QuantidadeApostasDoGrupo()
    {
        List<JogoConferido> jogos =
        [
            new(new Jogo(3810, DezenasJogo, DateTime.Now), 1, null),
            new(new Jogo(3810, Enumerable.Range(2, 15), DateTime.Now), 2, null),
            new(new Jogo(3810, Enumerable.Range(1, 16), DateTime.Now), 3, null),
        ];

        var grupo = new GrupoJogos(3810, null, jogos);

        Assert.Equal(18, grupo.QuantidadeApostas);
    }

    /// <summary>
    /// A prescrição é 90 dias depois do sorteio.
    /// </summary>
    [Fact]
    public void PrescricaoNoventaDiasAposOSorteio()
    {
        Concurso concurso = new(
            3808,
            new DateOnly(2026, 10, 19),
            ConcursoFabrica.DezenasPadrao,
            null,
            ConcursoFabrica.CriarRateios(),
            0m,
            0m,
            0m,
            0m);

        var grupo = new GrupoJogos(3808, concurso, []);

        Assert.Equal(new DateOnly(2027, 1, 17), grupo.Prescricao);
    }

    /// <summary>
    /// Excluir repassa o identificador ao repositório.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task ExcluiPeloIdentificador()
    {
        await this.CriarServico().ExcluirAsync(42, TestContext.Current.CancellationToken);

        await this.jogos.Received(1).ExcluirAsync(42, Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Cria o serviço com os repositórios falsos.
    /// </summary>
    /// <returns>Serviço testado.</returns>
    private JogosServico CriarServico()
    {
        return new JogosServico(this.jogos, this.concursos);
    }
}
