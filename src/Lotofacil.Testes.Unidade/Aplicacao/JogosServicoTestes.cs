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
    /// Só entram os concursos do intervalo, em ordem crescente, com os números dos jogos de Meus jogos e as contas
    /// do aviso; os grupos premiados ficam só com os jogos premiados.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task ConfereOsConcursosDoIntervalo()
    {
        int[] dezAcertos = [.. ConcursoFabrica.DezenasPadrao.Take(10), 3, 6, 8, 9, 12];
        int[] onzeAcertos = [.. ConcursoFabrica.DezenasPadrao.Take(11), 3, 6, 8, 9];
        var antigo = new Jogo(3808, ConcursoFabrica.DezenasPadrao, DateTime.Now);
        var semPremio = new Jogo(3809, dezAcertos, DateTime.Now);
        var comOnze = new Jogo(3809, onzeAcertos, DateTime.Now);
        var comQuinze = new Jogo(3810, ConcursoFabrica.DezenasPadrao, DateTime.Now);
        var aguardando = new Jogo(3811, DezenasJogo, DateTime.Now);
        this.jogos.ListarAsync(Arg.Any<CancellationToken>()).Returns([antigo, comQuinze, semPremio, aguardando, comOnze]);
        this.concursos.ObterAsync(3809, Arg.Any<CancellationToken>()).Returns(ConcursoFabrica.Criar(numero: 3809));
        this.concursos.ObterAsync(3810, Arg.Any<CancellationToken>()).Returns(ConcursoFabrica.Criar(numero: 3810));

        ConferenciaConcursos conferencia = await this.CriarServico().ConferirConcursosAsync(3809, 3810, TestContext.Current.CancellationToken);

        Assert.Equal([3809, 3810], conferencia.Grupos.Select(grupo => grupo.NumeroConcurso));
        Assert.Equal([semPremio, comOnze], conferencia.Grupos[0].Jogos.Select(jogo => jogo.Jogo));
        Assert.Equal(3, conferencia.JogosConferidos);
        Assert.Equal(2, conferencia.JogosPremiados);
        Assert.Equal(1_712_408.63m + 7m, conferencia.PremioTotal);

        IReadOnlyList<GrupoJogos> premiados = conferencia.GruposPremiados;
        Assert.Equal([3809, 3810], premiados.Select(grupo => grupo.NumeroConcurso));
        JogoConferido onze = Assert.Single(premiados[0].Jogos);
        Assert.Equal(2, onze.Numero);
        Assert.Equal(11, onze.Conferencia?.Acertos);
        await this.concursos.DidNotReceive().ObterAsync(3808, Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Sem jogos no intervalo, a conferência vem vazia e zerada.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task ConferenciaSemJogosNoIntervalo()
    {
        this.jogos.ListarAsync(Arg.Any<CancellationToken>()).Returns([new Jogo(3808, DezenasJogo, DateTime.Now)]);

        ConferenciaConcursos conferencia = await this.CriarServico().ConferirConcursosAsync(3809, 3809, TestContext.Current.CancellationToken);

        Assert.Empty(conferencia.Grupos);
        Assert.Equal(0, conferencia.JogosConferidos);
        Assert.Equal(0, conferencia.JogosPremiados);
        Assert.Equal(0m, conferencia.PremioTotal);
        Assert.Empty(conferencia.GruposPremiados);
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
    /// O prazo para receber vai até a data da prescrição, inclusive.
    /// </summary>
    /// <param name="ano">Ano de hoje.</param>
    /// <param name="mes">Mês de hoje.</param>
    /// <param name="dia">Dia de hoje.</param>
    /// <param name="esperado">Se o grupo está dentro do prazo.</param>
    [Theory]
    [InlineData(2026, 10, 19, true)]
    [InlineData(2027, 1, 17, true)]
    [InlineData(2027, 1, 18, false)]
    public void DentroDoPrazoAteAPrescricao(int ano, int mes, int dia, bool esperado)
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

        Assert.Equal(esperado, grupo.DentroDoPrazo(new DateOnly(ano, mes, dia)));
    }

    /// <summary>
    /// Grupo de concurso ainda não sorteado não está dentro do prazo.
    /// </summary>
    [Fact]
    public void GrupoAguardandoNaoEstaDentroDoPrazo()
    {
        var grupo = new GrupoJogos(3810, null, []);

        Assert.False(grupo.DentroDoPrazo(new DateOnly(2026, 10, 22)));
    }

    /// <summary>
    /// Vários jogos sem duplicados são todos gravados.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task CadastraVariosJogos()
    {
        this.concursos.ObterUltimoNumeroAsync(Arg.Any<CancellationToken>()).Returns(3810);
        List<IReadOnlyCollection<int>> lista = [DezenasJogo, [.. Enumerable.Range(2, 15)], [.. Enumerable.Range(1, 16)]];

        ResultadoCadastroVarios resultado = await this.CriarServico().CadastrarVariosAsync(3811, lista, TestContext.Current.CancellationToken);

        Assert.Equal(new ResultadoCadastroVarios(3, 0, null), resultado);
        await this.jogos.Received(3).AdicionarAsync(Arg.Is<Jogo>(jogo => jogo.NumeroConcurso == 3811), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Os jogos já cadastrados para o concurso são recusados e os demais gravados.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task CadastraVariosRecusandoDuplicados()
    {
        this.concursos.ObterUltimoNumeroAsync(Arg.Any<CancellationToken>()).Returns(3810);
        this.jogos.ExisteAsync(3811, Arg.Is<IEnumerable<int>>(dezenas => dezenas.SequenceEqual(DezenasJogo)), Arg.Any<CancellationToken>())
            .Returns(true);
        List<IReadOnlyCollection<int>> lista = [DezenasJogo, [.. Enumerable.Range(2, 15)]];

        ResultadoCadastroVarios resultado = await this.CriarServico().CadastrarVariosAsync(3811, lista, TestContext.Current.CancellationToken);

        Assert.Equal(new ResultadoCadastroVarios(1, 1, null), resultado);
        await this.jogos.Received(1).AdicionarAsync(
            Arg.Is<Jogo>(jogo => jogo.Dezenas.SequenceEqual(Enumerable.Range(2, 15))),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Todos duplicados: nada é gravado.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task CadastraVariosTodosDuplicados()
    {
        this.concursos.ObterUltimoNumeroAsync(Arg.Any<CancellationToken>()).Returns(3810);
        this.jogos.ExisteAsync(3811, Arg.Any<IEnumerable<int>>(), Arg.Any<CancellationToken>()).Returns(true);

        ResultadoCadastroVarios resultado = await this.CriarServico().CadastrarVariosAsync(
            3811,
            [DezenasJogo, [.. Enumerable.Range(2, 15)]],
            TestContext.Current.CancellationToken);

        Assert.Equal(new ResultadoCadastroVarios(0, 2, null), resultado);
        await this.jogos.DidNotReceiveWithAnyArgs().AdicionarAsync(null!, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Base vazia e concurso já sorteado recusam todos os jogos, com as mensagens do cadastro.
    /// </summary>
    /// <param name="ultimoGravado">Último concurso gravado, ou nulo com a base vazia.</param>
    /// <param name="mensagem">Motivo esperado.</param>
    /// <returns>Tarefa do teste.</returns>
    [Theory]
    [InlineData(null, "Importe o histórico na página Resultados antes de cadastrar jogos.")]
    [InlineData(3811, "Só é possível cadastrar jogos para concursos depois do 3811.")]
    public async Task CadastraVariosRecusaConcurso(int? ultimoGravado, string mensagem)
    {
        this.concursos.ObterUltimoNumeroAsync(Arg.Any<CancellationToken>()).Returns(ultimoGravado);

        ResultadoCadastroVarios resultado = await this.CriarServico().CadastrarVariosAsync(
            3811,
            [DezenasJogo],
            TestContext.Current.CancellationToken);

        Assert.Equal(new ResultadoCadastroVarios(0, 0, mensagem), resultado);
        await this.jogos.DidNotReceiveWithAnyArgs().AdicionarAsync(null!, TestContext.Current.CancellationToken);
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
