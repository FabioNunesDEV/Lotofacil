// <copyright file="TeimosinhaServicoTestes.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Aplicacao.Jogos;
using Lotofacil.Aplicacao.Teimosinhas;
using Lotofacil.Dominio.Concursos;
using Lotofacil.Dominio.Jogos;
using Lotofacil.Dominio.Teimosinhas;
using NSubstitute;

namespace Lotofacil.Testes.Unidade.Aplicacao;

/// <summary>
/// Testes do serviço das Teimosinhas, sobre repositórios falsos em memória.
/// </summary>
public class TeimosinhaServicoTestes
{
    /// <summary>
    /// Dezenas 01 a 15.
    /// </summary>
    private static readonly int[] De01a15 = [.. Enumerable.Range(1, 15)];

    /// <summary>
    /// Dezenas 11 a 25.
    /// </summary>
    private static readonly int[] De11a25 = [.. Enumerable.Range(11, 15)];

    /// <summary>
    /// Dezenas 06 a 20.
    /// </summary>
    private static readonly int[] De06a20 = [.. Enumerable.Range(6, 15)];

    /// <summary>
    /// Jogos gravados no repositório falso.
    /// </summary>
    private readonly List<Jogo> jogos = [];

    /// <summary>
    /// Repositório falso das Teimosinhas.
    /// </summary>
    private readonly TeimosinhaRepositorioFalso teimosinhas = new();

    /// <summary>
    /// Repositório falso dos concursos.
    /// </summary>
    private readonly IConcursoRepositorio concursos = Substitute.For<IConcursoRepositorio>();

    /// <summary>
    /// Serviço testado.
    /// </summary>
    private readonly TeimosinhaServico servico;

    /// <summary>
    /// Monta o serviço sobre os repositórios falsos, com o último concurso gravado 3812.
    /// </summary>
    public TeimosinhaServicoTestes()
    {
        IJogoRepositorio repositorioJogos = Substitute.For<IJogoRepositorio>();
        repositorioJogos.ExisteAsync(Arg.Any<int>(), Arg.Any<IEnumerable<int>>(), Arg.Any<CancellationToken>())
            .Returns(chamada => this.jogos.Any(jogo =>
                jogo.NumeroConcurso == chamada.ArgAt<int>(0)
                && jogo.Dezenas.SequenceEqual(chamada.ArgAt<IEnumerable<int>>(1).Order())));
        repositorioJogos.AdicionarAsync(Arg.Any<Jogo>(), Arg.Any<CancellationToken>())
            .Returns(chamada =>
            {
                this.jogos.Add(chamada.ArgAt<Jogo>(0));
                return Task.CompletedTask;
            });
        this.DefinirUltimoConcurso(3812);

        this.servico = new TeimosinhaServico(
            this.teimosinhas,
            repositorioJogos,
            new JogosServico(repositorioJogos, this.concursos));
    }

    /// <summary>
    /// Um nome novo cria a Teimosinha com o jogo, e o jogo do concurso fica ligado a ela.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task SalvaNovaTeimosinha()
    {
        ResultadoSalvarTeimosinha resultado =
            await this.servico.SalvarAsync(" Minhas fixas ", 3813, [De01a15], TestContext.Current.CancellationToken);

        Assert.Equal(new ResultadoSalvarTeimosinha(1, 0, null, "Minhas fixas"), resultado);
        Teimosinha teimosinha = Assert.Single(this.teimosinhas.Todas);
        Assert.Equal("Minhas fixas", teimosinha.Nome);
        Assert.Equal(3813, teimosinha.UltimoConcursoCriado);
        Assert.Equal(3813, Assert.Single(teimosinha.Jogos).ConcursoInicial);
        Jogo jogo = Assert.Single(this.jogos);
        Assert.Equal((3813, teimosinha.Id), (jogo.NumeroConcurso, jogo.TeimosinhaId));
        Assert.True(teimosinha.Id > 0);
    }

    /// <summary>
    /// O nome de uma ativa, em outra caixa, acrescenta o jogo a ela com o próprio início.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task NomeExistenteAcrescenta()
    {
        await this.servico.SalvarAsync("Minhas fixas", 3813, [De01a15], TestContext.Current.CancellationToken);

        ResultadoSalvarTeimosinha resultado =
            await this.servico.SalvarAsync("minhas FIXAS", 3814, [De11a25], TestContext.Current.CancellationToken);

        Assert.Equal(new ResultadoSalvarTeimosinha(1, 0, null, "Minhas fixas"), resultado);
        Teimosinha teimosinha = Assert.Single(this.teimosinhas.Todas);
        Assert.Equal([3813, 3814], teimosinha.Jogos.Select(jogo => jogo.ConcursoInicial));
        Assert.Equal(3813, teimosinha.UltimoConcursoCriado);
    }

    /// <summary>
    /// Um jogo que já faz parte da Teimosinha é recusado, e nada muda.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task RecusaJogoJaNaTeimosinha()
    {
        await this.servico.SalvarAsync("Minhas fixas", 3813, [De01a15], TestContext.Current.CancellationToken);

        ResultadoSalvarTeimosinha resultado =
            await this.servico.SalvarAsync("Minhas fixas", 3814, [De01a15], TestContext.Current.CancellationToken);

        Assert.Equal("Este jogo já faz parte da Teimosinha Minhas fixas.", resultado.Recusa);
        Assert.Single(Assert.Single(this.teimosinhas.Todas).Jogos);
        Assert.Single(this.jogos);
    }

    /// <summary>
    /// Um jogo já cadastrado no concurso é recusado com a mensagem do cadastro, sem criar a Teimosinha.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task RecusaDuplicadoNoConcursoSemCriarATeimosinha()
    {
        this.jogos.Add(new Jogo(3813, De01a15, DateTime.Now));

        ResultadoSalvarTeimosinha resultado =
            await this.servico.SalvarAsync("Minhas fixas", 3813, [De01a15], TestContext.Current.CancellationToken);

        Assert.Equal("Este jogo já está cadastrado para o concurso 3813.", resultado.Recusa);
        Assert.Empty(this.teimosinhas.Todas);
    }

    /// <summary>
    /// Concurso já sorteado é recusado com a mensagem do cadastro, sem criar a Teimosinha.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task RecusaConcursoPassado()
    {
        ResultadoSalvarTeimosinha resultado =
            await this.servico.SalvarAsync("Minhas fixas", 3812, [De01a15], TestContext.Current.CancellationToken);

        Assert.Equal("Só é possível cadastrar jogos para concursos depois do 3812.", resultado.Recusa);
        Assert.Empty(this.teimosinhas.Todas);
        Assert.Empty(this.jogos);
    }

    /// <summary>
    /// Com vários jogos, os duplicados são recusados e os demais gravados na Teimosinha.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task SalvaVariosComDuplicado()
    {
        this.jogos.Add(new Jogo(3813, De06a20, DateTime.Now));

        ResultadoSalvarTeimosinha resultado = await this.servico.SalvarAsync(
            "Geradas",
            3813,
            [De01a15, De06a20, De11a25],
            TestContext.Current.CancellationToken);

        Assert.Equal(new ResultadoSalvarTeimosinha(2, 1, null, "Geradas"), resultado);
        Assert.Equal(2, Assert.Single(this.teimosinhas.Todas).Jogos.Count);
        Assert.Equal(3, this.jogos.Count);
    }

    /// <summary>
    /// Com o próximo concurso andando um, os jogos são criados para ele.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task CriaOsJogosDoProximoConcurso()
    {
        await this.servico.SalvarAsync("Minhas fixas", 3813, [De01a15, De11a25], TestContext.Current.CancellationToken);
        this.DefinirUltimoConcurso(3813);

        JogosCriadosTeimosinha criados = await this.servico.CriarJogosAsync(TestContext.Current.CancellationToken);

        Assert.Equal(new JogosCriadosTeimosinha(2, 3814, 3814), criados);
        Assert.Equal(2, this.jogos.Count(jogo => jogo.NumeroConcurso == 3814));
        Assert.Equal(3814, Assert.Single(this.teimosinhas.Todas).UltimoConcursoCriado);
    }

    /// <summary>
    /// Concursos pulados também ganham os jogos, e uma segunda chamada não cria nada.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task PreencheConcursosPuladosSemDuplicar()
    {
        await this.servico.SalvarAsync("Minhas fixas", 3813, [De01a15], TestContext.Current.CancellationToken);
        this.DefinirUltimoConcurso(3815);

        JogosCriadosTeimosinha criados = await this.servico.CriarJogosAsync(TestContext.Current.CancellationToken);
        JogosCriadosTeimosinha deNovo = await this.servico.CriarJogosAsync(TestContext.Current.CancellationToken);

        Assert.Equal(new JogosCriadosTeimosinha(3, 3814, 3816), criados);
        Assert.Equal([3813, 3814, 3815, 3816], this.jogos.Select(jogo => jogo.NumeroConcurso));
        Assert.Equal(JogosCriadosTeimosinha.Nenhum, deNovo);
    }

    /// <summary>
    /// Um jogo já cadastrado no concurso é pulado, e os demais são criados.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task PulaJogoJaCadastrado()
    {
        await this.servico.SalvarAsync("Minhas fixas", 3813, [De01a15, De11a25], TestContext.Current.CancellationToken);
        this.jogos.Add(new Jogo(3814, De01a15, DateTime.Now));
        this.DefinirUltimoConcurso(3813);

        JogosCriadosTeimosinha criados = await this.servico.CriarJogosAsync(TestContext.Current.CancellationToken);

        Assert.Equal(1, criados.Quantidade);
        Assert.Equal(2, this.jogos.Count(jogo => jogo.NumeroConcurso == 3814));
    }

    /// <summary>
    /// Um jogo acrescentado com início posterior só entra a partir do próprio início.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task JogoAcrescentadoComInicioPosterior()
    {
        await this.servico.SalvarAsync("Minhas fixas", 3813, [De01a15], TestContext.Current.CancellationToken);
        await this.servico.SalvarAsync("Minhas fixas", 3816, [De11a25], TestContext.Current.CancellationToken);
        this.DefinirUltimoConcurso(3815);

        await this.servico.CriarJogosAsync(TestContext.Current.CancellationToken);

        Assert.Equal(
            [(3813, 1), (3814, 1), (3815, 1), (3816, 2)],
            this.jogos.GroupBy(jogo => jogo.NumeroConcurso).OrderBy(grupo => grupo.Key).Select(grupo => (grupo.Key, grupo.Count())));
    }

    /// <summary>
    /// Uma Teimosinha removida não cria jogos, e os já criados continuam.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task RemovidaNaoCriaJogos()
    {
        await this.servico.SalvarAsync("Minhas fixas", 3813, [De01a15], TestContext.Current.CancellationToken);
        int id = this.teimosinhas.Todas[0].Id;
        this.DefinirUltimoConcurso(3813);

        await this.servico.RemoverAsync(id, TestContext.Current.CancellationToken);
        JogosCriadosTeimosinha criados = await this.servico.CriarJogosAsync(TestContext.Current.CancellationToken);

        Assert.Equal(JogosCriadosTeimosinha.Nenhum, criados);
        Assert.Single(this.jogos);
        Assert.Empty(await this.servico.ListarAtivasAsync(TestContext.Current.CancellationToken));
        Assert.Equal("Minhas fixas", (await this.servico.ListarNomesAsync(TestContext.Current.CancellationToken))[id]);
    }

    /// <summary>
    /// Com a base vazia, nada é criado.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task BaseVaziaNaoCria()
    {
        this.concursos.ObterUltimoNumeroAsync(Arg.Any<CancellationToken>()).Returns((int?)null);

        Assert.Equal(JogosCriadosTeimosinha.Nenhum, await this.servico.CriarJogosAsync(TestContext.Current.CancellationToken));
    }

    /// <summary>
    /// Define o último concurso gravado na base falsa.
    /// </summary>
    /// <param name="numero">Número do último concurso.</param>
    private void DefinirUltimoConcurso(int numero)
    {
        this.concursos.ObterUltimoNumeroAsync(Arg.Any<CancellationToken>()).Returns(numero);
    }
}
