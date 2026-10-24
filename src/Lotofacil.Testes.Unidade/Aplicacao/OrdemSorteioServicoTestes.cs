// <copyright file="OrdemSorteioServicoTestes.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Aplicacao.Concursos;
using Lotofacil.Dominio.Concursos;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Lotofacil.Testes.Unidade.Aplicacao;

/// <summary>
/// Testes do serviço de preenchimento da ordem do sorteio.
/// </summary>
public class OrdemSorteioServicoTestes
{
    /// <summary>
    /// Ordem devolvida pela fonte falsa para qualquer concurso.
    /// </summary>
    private static readonly int[] Ordem = [18, 20, 25, 23, 10, 11, 24, 14, 6, 2, 13, 9, 5, 16, 3];

    /// <summary>
    /// Quatro esperas zeradas, para os testes do limite de consultas não esperarem de verdade.
    /// </summary>
    private static readonly TimeSpan[] EsperasZeradas = [TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero];

    /// <summary>
    /// Fonte falsa dos concursos individuais.
    /// </summary>
    private readonly IFonteConcursos fonte = Substitute.For<IFonteConcursos>();

    /// <summary>
    /// Repositório falso dos concursos.
    /// </summary>
    private readonly IConcursoRepositorio repositorio = Substitute.For<IConcursoRepositorio>();

    /// <summary>
    /// Números com a ordem gravada pelo repositório falso, na ordem das gravações.
    /// </summary>
    private readonly List<int> gravados = [];

    /// <summary>
    /// Números pedidos à fonte falsa, na ordem dos pedidos.
    /// </summary>
    private readonly List<int> pedidos = [];

    /// <summary>
    /// Faz a fonte falsa devolver uma ordem válida e o repositório falso anotar cada gravação.
    /// </summary>
    public OrdemSorteioServicoTestes()
    {
        this.fonte.ObterOrdemSorteioAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(chamada =>
            {
                chamada.ArgAt<CancellationToken>(1).ThrowIfCancellationRequested();
                this.pedidos.Add(chamada.ArgAt<int>(0));
                return Ordem;
            });
        this.repositorio.GravarOrdemSorteioAsync(Arg.Any<int>(), Arg.Any<IReadOnlyList<int>>(), Arg.Any<CancellationToken>())
            .Returns(chamada =>
            {
                this.gravados.Add(chamada.ArgAt<int>(0));
                return Task.CompletedTask;
            });
    }

    /// <summary>
    /// Só os concursos sem ordem são buscados, em ordem crescente, com o andamento de cada um.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task PreencheOsConcursosSemOrdemEmOrdem()
    {
        this.DefinirSemOrdem(1, 2, 3808);
        var andamento = new AndamentoAnotado();

        ResultadoPreenchimentoOrdem resultado =
            await this.CriarServico().PreencherAsync(andamento, TestContext.Current.CancellationToken);

        Assert.Equal([1, 2, 3808], this.pedidos);
        Assert.Equal([1, 2, 3808], this.gravados);
        Assert.Equal(
            [
                new AndamentoOrdemSorteio(1, 1, 3, null),
                new AndamentoOrdemSorteio(2, 2, 3, null),
                new AndamentoOrdemSorteio(3808, 3, 3, null),
            ],
            andamento.Recebidos);
        Assert.Equal(3, resultado.Total);
        Assert.Equal(3, resultado.Preenchidos);
        Assert.Empty(resultado.Falhas);
        Assert.Equal(DesfechoPreenchimento.Concluido, resultado.Desfecho);
        Assert.Equal(0, resultado.Restantes);
    }

    /// <summary>
    /// Falha de comunicação, resposta sem ordem e ordem divergente são puladas, e a busca segue.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task PulaAsFalhasESegue()
    {
        this.DefinirSemOrdem(1499, 1500, 1501, 1502);
        this.fonte.ObterOrdemSorteioAsync(1500, Arg.Any<CancellationToken>())
            .ThrowsAsync(new FonteResultadosException("A Caixa respondeu com o código 503 ao buscar a ordem do concurso 1500."));
        this.fonte.ObterOrdemSorteioAsync(1501, Arg.Any<CancellationToken>()).Returns((IReadOnlyList<int>?)null);
        this.repositorio.GravarOrdemSorteioAsync(1502, Arg.Any<IReadOnlyList<int>>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new ArgumentException("A ordem do sorteio não corresponde às dezenas sorteadas."));

        ResultadoPreenchimentoOrdem resultado =
            await this.CriarServico().PreencherAsync(null, TestContext.Current.CancellationToken);

        Assert.Equal([1499], this.gravados);
        Assert.Equal(1, resultado.Preenchidos);
        Assert.Equal(3, resultado.SemOrdem);
        Assert.Equal(3, resultado.Restantes);
        Assert.Equal(
            [
                (1500, "A Caixa respondeu com o código 503 ao buscar a ordem do concurso 1500."),
                (1501, "A Caixa não informou a ordem do sorteio do concurso 1501."),
                (1502, "A ordem do sorteio informada pela Caixa para o concurso 1502 não corresponde às dezenas gravadas."),
            ],
            resultado.Falhas.Select(falha => (falha.Numero, falha.Mensagem)));
        Assert.IsType<FonteResultadosException>(resultado.Falhas[0].Causa);
        Assert.Equal(DesfechoPreenchimento.Concluido, resultado.Desfecho);
    }

    /// <summary>
    /// Cinco falhas seguidas encerram a busca sem pedir o concurso seguinte.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task CincoFalhasSeguidasEncerram()
    {
        this.DefinirSemOrdem(1, 2, 3, 4, 5, 6, 7);
        this.fonte.ObterOrdemSorteioAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(IReadOnlyList<int>? (chamada) =>
            {
                this.pedidos.Add(chamada.ArgAt<int>(0));
                throw new FonteResultadosException($"Não foi possível conectar à Caixa para buscar a ordem do concurso {chamada.ArgAt<int>(0)}.");
            });

        ResultadoPreenchimentoOrdem resultado =
            await this.CriarServico().PreencherAsync(null, TestContext.Current.CancellationToken);

        Assert.Equal([1, 2, 3, 4, 5], this.pedidos);
        Assert.Equal(DesfechoPreenchimento.EncerradoPorFalhas, resultado.Desfecho);
        Assert.Equal(0, resultado.Preenchidos);
        Assert.Equal(7, resultado.Restantes);
        Assert.Equal(
            "Não foi possível conectar à Caixa para buscar a ordem do concurso 5.",
            resultado.Falhas[^1].Mensagem);
    }

    /// <summary>
    /// Um concurso preenchido zera a contagem de falhas seguidas.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task SucessoZeraAContagemDeFalhas()
    {
        this.DefinirSemOrdem(1, 2, 3, 4, 5, 6, 7, 8, 9);
        foreach (int numero in new[] { 1, 2, 3, 4, 6, 7, 8, 9 })
        {
            this.fonte.ObterOrdemSorteioAsync(numero, Arg.Any<CancellationToken>()).Returns((IReadOnlyList<int>?)null);
        }

        ResultadoPreenchimentoOrdem resultado =
            await this.CriarServico().PreencherAsync(null, TestContext.Current.CancellationToken);

        Assert.Equal(DesfechoPreenchimento.Concluido, resultado.Desfecho);
        Assert.Equal([5], this.gravados);
        Assert.Equal(8, resultado.SemOrdem);
    }

    /// <summary>
    /// O cancelamento interrompe a busca sem lançar, mantendo as ordens já gravadas.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task CancelamentoInterrompeSemLancar()
    {
        this.DefinirSemOrdem(1, 2, 3, 4);
        using var interromper = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        this.repositorio.GravarOrdemSorteioAsync(Arg.Any<int>(), Arg.Any<IReadOnlyList<int>>(), Arg.Any<CancellationToken>())
            .Returns(chamada =>
            {
                this.gravados.Add(chamada.ArgAt<int>(0));
                if (this.gravados.Count == 2)
                {
                    interromper.Cancel();
                }

                return Task.CompletedTask;
            });

        ResultadoPreenchimentoOrdem resultado = await this.CriarServico().PreencherAsync(null, interromper.Token);

        Assert.Equal(DesfechoPreenchimento.Interrompido, resultado.Desfecho);
        Assert.Equal(2, resultado.Preenchidos);
        Assert.Equal(2, resultado.Restantes);
        Assert.Equal([1, 2], this.pedidos);
    }

    /// <summary>
    /// Um erro não previsto é lançado, e as ordens gravadas antes dele ficam.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task ErroInesperadoELancado()
    {
        this.DefinirSemOrdem(18, 19, 20, 21);
        this.repositorio.GravarOrdemSorteioAsync(20, Arg.Any<IReadOnlyList<int>>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Falha na gravação."));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => this.CriarServico().PreencherAsync(null, TestContext.Current.CancellationToken));

        Assert.Equal([18, 19], this.gravados);
    }

    /// <summary>
    /// Com o limite de consultas, o mesmo concurso é buscado de novo depois da espera, sem contar como falha.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task LimiteDeConsultasEsperaETentaDeNovo()
    {
        this.DefinirSemOrdem(37, 38, 39);
        int tentativas38 = 0;
        this.fonte.ObterOrdemSorteioAsync(38, Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                tentativas38++;
                return tentativas38 <= 2 ? throw Limite(38) : (IReadOnlyList<int>?)Ordem;
            });
        var andamento = new AndamentoAnotado();

        ResultadoPreenchimentoOrdem resultado =
            await this.CriarServico().PreencherAsync(andamento, TestContext.Current.CancellationToken);

        Assert.Equal([37, 38, 39], this.gravados);
        Assert.Empty(resultado.Falhas);
        Assert.Equal(DesfechoPreenchimento.Concluido, resultado.Desfecho);
        Assert.Equal(
            [
                new AndamentoOrdemSorteio(37, 1, 3, null),
                new AndamentoOrdemSorteio(38, 2, 3, null),
                new AndamentoOrdemSorteio(38, 2, 3, TimeSpan.Zero),
                new AndamentoOrdemSorteio(38, 2, 3, null),
                new AndamentoOrdemSorteio(38, 2, 3, TimeSpan.Zero),
                new AndamentoOrdemSorteio(38, 2, 3, null),
                new AndamentoOrdemSorteio(39, 3, 3, null),
            ],
            andamento.Recebidos);
    }

    /// <summary>
    /// As esperas são usadas na ordem configurada, e o limite que continua depois da última encerra a busca.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task LimitePersistenteEncerra()
    {
        this.DefinirSemOrdem(37, 38, 39);
        this.fonte.ObterOrdemSorteioAsync(38, Arg.Any<CancellationToken>()).ThrowsAsync(Limite(38));
        var andamento = new AndamentoAnotado();
        TimeSpan[] esperas = [TimeSpan.FromTicks(1), TimeSpan.FromTicks(2), TimeSpan.FromTicks(3), TimeSpan.FromTicks(4)];
        var servico = new OrdemSorteioServico(this.fonte, this.repositorio, esperas);

        ResultadoPreenchimentoOrdem resultado = await servico.PreencherAsync(andamento, TestContext.Current.CancellationToken);

        Assert.Equal(DesfechoPreenchimento.EncerradoPorLimite, resultado.Desfecho);
        Assert.Equal([37], this.gravados);
        Assert.Equal(1, resultado.Preenchidos);
        Assert.Equal(2, resultado.Restantes);
        Assert.Equal("A Caixa respondeu com o código 429 ao buscar a ordem do concurso 38.", resultado.Falhas[^1].Mensagem);
        Assert.Equal(esperas, andamento.Recebidos.Where(item => item.Espera is not null).Select(item => item.Espera!.Value));
        await this.fonte.Received(5).ObterOrdemSorteioAsync(38, Arg.Any<CancellationToken>());
        await this.fonte.DidNotReceive().ObterOrdemSorteioAsync(39, Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Interromper durante a espera para na hora, sem esperar o fim da pausa.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task InterromperDuranteAEspera()
    {
        this.DefinirSemOrdem(38, 39);
        this.fonte.ObterOrdemSorteioAsync(38, Arg.Any<CancellationToken>()).ThrowsAsync(Limite(38));
        using var interromper = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        var andamento = new AndamentoAnotado(item =>
        {
            if (item.Espera is not null)
            {
                interromper.Cancel();
            }
        });
        var servico = new OrdemSorteioServico(this.fonte, this.repositorio, [TimeSpan.FromHours(1)]);

        ResultadoPreenchimentoOrdem resultado = await servico.PreencherAsync(andamento, interromper.Token);

        Assert.Equal(DesfechoPreenchimento.Interrompido, resultado.Desfecho);
        Assert.Empty(this.gravados);
        Assert.Equal(2, resultado.Restantes);
    }

    /// <summary>
    /// Cria a falha de limite de consultas da Caixa para um concurso.
    /// </summary>
    /// <param name="numero">Número do concurso.</param>
    /// <returns>Falha com o código 429.</returns>
    private static FonteResultadosException Limite(int numero)
    {
        return new FonteResultadosException($"A Caixa respondeu com o código 429 ao buscar a ordem do concurso {numero}.", true);
    }

    /// <summary>
    /// Faz o repositório falso informar os concursos sem ordem.
    /// </summary>
    /// <param name="numeros">Números dos concursos sem ordem, em ordem crescente.</param>
    private void DefinirSemOrdem(params int[] numeros)
    {
        this.repositorio.ListarNumerosSemOrdemAsync(Arg.Any<CancellationToken>()).Returns(numeros);
    }

    /// <summary>
    /// Cria o serviço com a fonte e o repositório falsos.
    /// </summary>
    /// <returns>Serviço testado.</returns>
    private OrdemSorteioServico CriarServico()
    {
        return new OrdemSorteioServico(this.fonte, this.repositorio, EsperasZeradas);
    }

    /// <summary>
    /// Anota os andamentos recebidos, sem a troca de thread do <see cref="Progress{T}"/>.
    /// </summary>
    private sealed class AndamentoAnotado : IProgress<AndamentoOrdemSorteio>
    {
        /// <summary>
        /// Ação executada a cada andamento, ou <c>null</c>.
        /// </summary>
        private readonly Action<AndamentoOrdemSorteio>? aoReceber;

        /// <summary>
        /// Cria o anotador de andamentos.
        /// </summary>
        /// <param name="aoReceber">Ação executada a cada andamento, ou <c>null</c>.</param>
        public AndamentoAnotado(Action<AndamentoOrdemSorteio>? aoReceber = null)
        {
            this.aoReceber = aoReceber;
        }

        /// <summary>
        /// Andamentos recebidos, na ordem.
        /// </summary>
        public List<AndamentoOrdemSorteio> Recebidos { get; } = [];

        /// <inheritdoc/>
        public void Report(AndamentoOrdemSorteio value)
        {
            this.Recebidos.Add(value);
            this.aoReceber?.Invoke(value);
        }
    }
}
