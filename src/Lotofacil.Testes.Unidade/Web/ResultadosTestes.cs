// <copyright file="ResultadosTestes.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using System.Globalization;
using AngleSharp.Dom;
using Bunit;
using Lotofacil.Aplicacao.Concursos;
using Lotofacil.Aplicacao.Jogos;
using Lotofacil.Dominio.Concursos;
using Lotofacil.Dominio.Jogos;
using Lotofacil.Web.Components.Pages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MudBlazor;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Lotofacil.Testes.Unidade.Web;

/// <summary>
/// Testes da página Resultados, com os serviços reais sobre um repositório e uma fonte falsos em memória.
/// </summary>
public class ResultadosTestes : ComponenteTesteBase
{
    /// <summary>
    /// Jogo com 13 acertos nas dezenas padrão dos concursos de teste.
    /// </summary>
    private static readonly int[] TrezeAcertos = [.. ConcursoFabrica.DezenasPadrao.Take(13), 3, 6];

    /// <summary>
    /// Jogo com 9 acertos nas dezenas padrão dos concursos de teste.
    /// </summary>
    private static readonly int[] NoveAcertos = [.. ConcursoFabrica.DezenasPadrao.Take(9), 3, 6, 8, 9, 12, 15];

    /// <summary>
    /// Jogos cadastrados na base falsa.
    /// </summary>
    private readonly List<Jogo> jogos = [];

    /// <summary>
    /// Concursos devolvidos pelo repositório falso.
    /// </summary>
    private readonly List<Concurso> concursos = [];

    /// <summary>
    /// Repositório falso, que lê e grava na lista <see cref="concursos"/>.
    /// </summary>
    private readonly IConcursoRepositorio repositorio = Substitute.For<IConcursoRepositorio>();

    /// <summary>
    /// Fonte falsa do histórico da Caixa.
    /// </summary>
    private readonly IFonteHistoricoConcursos fonte = Substitute.For<IFonteHistoricoConcursos>();

    /// <summary>
    /// Fonte falsa dos concursos individuais da Caixa.
    /// </summary>
    private readonly IFonteConcursos fonteConcursos = Substitute.For<IFonteConcursos>();

    /// <summary>
    /// Logger da página, para conferir o registro das falhas.
    /// </summary>
    private readonly LoggerFalso<Resultados> log = new();

    /// <summary>
    /// Registra os serviços de consulta, importação e atualização com um repositório que usa a lista
    /// <see cref="concursos"/>.
    /// </summary>
    public ResultadosTestes()
    {
        this.repositorio.ObterUltimoNumeroAsync(Arg.Any<CancellationToken>())
            .Returns(_ => this.concursos.Count == 0 ? null : this.concursos.Max(concurso => concurso.Numero));
        this.fonteConcursos.ObterConcursoAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(chamada => ConcursoFabrica.Criar(numero: chamada.ArgAt<int>(0)));
        this.repositorio.ListarAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(chamada => this.concursos
                .OrderByDescending(concurso => concurso.Numero)
                .Skip(chamada.ArgAt<int>(0))
                .Take(chamada.ArgAt<int>(1))
                .ToList());
        this.repositorio.ContarAsync(Arg.Any<CancellationToken>()).Returns(_ => this.concursos.Count);
        this.repositorio.ObterAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(chamada => this.concursos.Find(concurso => concurso.Numero == chamada.ArgAt<int>(0)));
        this.repositorio.ListarNumerosAsync(Arg.Any<CancellationToken>())
            .Returns(_ => (IReadOnlySet<int>)this.concursos.Select(concurso => concurso.Numero).ToHashSet());
        this.repositorio.AdicionarAsync(Arg.Any<IReadOnlyCollection<Concurso>>(), Arg.Any<CancellationToken>())
            .Returns(chamada =>
            {
                this.concursos.AddRange(chamada.ArgAt<IReadOnlyCollection<Concurso>>(0));
                return Task.CompletedTask;
            });

        IJogoRepositorio repositorioJogos = Substitute.For<IJogoRepositorio>();
        repositorioJogos.ListarAsync(Arg.Any<CancellationToken>()).Returns(_ => this.jogos.ToList());

        this.Services.AddSingleton(new ConsultaConcursosServico(this.repositorio));
        this.Services.AddSingleton(new JogosServico(repositorioJogos, this.repositorio));
        this.Services.AddSingleton(new ImportacaoHistoricoServico(this.fonte, this.repositorio));
        this.Services.AddSingleton(new AtualizacaoConcursosServico(this.fonteConcursos, this.repositorio));
        this.Services.AddSingleton<ILogger<Resultados>>(this.log);
    }

    /// <summary>
    /// A página exibe o seu título.
    /// </summary>
    [Fact]
    public void ExibeTitulo()
    {
        IRenderedComponent<Resultados> pagina = this.RenderizarPagina();

        Assert.Equal("Resultados", pagina.Find("h1").TextContent.Trim());
    }

    /// <summary>
    /// A primeira página mostra os 10 concursos mais recentes e o total da base.
    /// </summary>
    [Fact]
    public void PrimeiraPaginaMostraOsDezMaisRecentes()
    {
        this.AdicionarConcursos(23);

        IRenderedComponent<Resultados> pagina = this.RenderizarPagina();

        pagina.WaitForAssertion(() => Assert.Equal(Enumerable.Range(14, 10).Reverse(), NumerosExibidos(pagina)));
        Assert.Contains(pagina.FindAll(".mud-table-pagination-caption"), legenda => Texto(legenda) == "1–10 de 23");
    }

    /// <summary>
    /// Avançar a página mostra os 10 concursos seguintes.
    /// </summary>
    [Fact]
    public void ProximaPaginaMostraOsConcursosSeguintes()
    {
        this.AdicionarConcursos(23);
        IRenderedComponent<Resultados> pagina = this.RenderizarPagina();
        pagina.WaitForAssertion(() => Assert.Equal(10, NumerosExibidos(pagina).Count));

        pagina.FindAll(".mud-table-pagination-actions button")[2].Click();

        pagina.WaitForAssertion(() => Assert.Equal(Enumerable.Range(4, 10).Reverse(), NumerosExibidos(pagina)));
    }

    /// <summary>
    /// A linha mostra data, dezenas com dois dígitos, ganhadores, prêmio em reais e a situação "Saiu".
    /// </summary>
    [Fact]
    public void LinhaDeConcursoComGanhador()
    {
        this.concursos.Add(ConcursoFabrica.Criar(
            numero: 3804,
            rateios: ConcursoFabrica.CriarRateios(ganhadores15: 2, premio15: 801_455.92m)));

        IRenderedComponent<Resultados> pagina = this.RenderizarPagina();

        IElement linha = pagina.WaitForElement("tr.linha-clicavel");
        IHtmlCollection<IElement> celulas = linha.QuerySelectorAll("td");
        Assert.Equal("3804", Texto(celulas[0]));
        Assert.Equal("17/10/2026", Texto(celulas[1]));
        Assert.Equal(
            ConcursoFabrica.DezenasPadrao.Select(dezena => dezena.ToString("00")),
            celulas[2].QuerySelectorAll(".bola").Select(Texto));
        Assert.Equal("2", Texto(celulas[3]));
        Assert.Equal("R$ 801.455,92", Texto(celulas[4]));
        Assert.Equal("Saiu", Texto(celulas[5]));
    }

    /// <summary>
    /// Sem ganhador de 15 acertos, o prêmio aparece como "—" e a situação como "Acumulou".
    /// </summary>
    [Fact]
    public void LinhaDeConcursoSemGanhador()
    {
        this.concursos.Add(ConcursoFabrica.Criar(
            rateios: ConcursoFabrica.CriarRateios(ganhadores15: 0, premio15: 0m)));

        IRenderedComponent<Resultados> pagina = this.RenderizarPagina();

        IHtmlCollection<IElement> celulas = pagina.WaitForElement("tr.linha-clicavel").QuerySelectorAll("td");
        Assert.Equal("0", Texto(celulas[3]));
        Assert.Equal("—", Texto(celulas[4]));
        Assert.Equal("Acumulou", Texto(celulas[5]));
    }

    /// <summary>
    /// Clicar na linha abre o rateio das faixas de 15 a 11; clicar de novo fecha.
    /// </summary>
    [Fact]
    public void CliqueNaLinhaAbreEFechaODetalhe()
    {
        this.concursos.Add(ConcursoFabrica.Criar());
        IRenderedComponent<Resultados> pagina = this.RenderizarPagina();

        pagina.WaitForElement("tr.linha-clicavel").Click();

        IElement detalhe = pagina.Find(".detalhe-concurso");
        List<string[]> linhasRateio = detalhe.QuerySelectorAll("tr")
            .Select(linha => linha.QuerySelectorAll("td").Select(Texto).ToArray())
            .Where(celulas => celulas.Length > 0)
            .ToList();
        Assert.Equal(
            [
                ["15 acertos", "1", "R$ 1.712.408,63"],
                ["14 acertos", "287", "R$ 1.502,34"],
                ["13 acertos", "9.874", "R$ 35,00"],
                ["12 acertos", "121.045", "R$ 14,00"],
                ["11 acertos", "653.211", "R$ 7,00"],
            ],
            linhasRateio);
        Assert.Contains("R$ 24.873.112,50", Texto(detalhe.QuerySelector(".arrecadacao")!));

        pagina.Find("tr.linha-clicavel").Click();

        Assert.Empty(pagina.FindAll(".detalhe-concurso"));
    }

    /// <summary>
    /// O detalhe mostra a ordem do sorteio quando ela é conhecida.
    /// </summary>
    [Fact]
    public void DetalheMostraOrdemDoSorteio()
    {
        int[] ordem = [13, 2, 24, 7, 20, 11, 1, 25, 18, 5, 22, 14, 10, 21, 4];
        this.concursos.Add(ConcursoFabrica.Criar(ordemSorteio: ordem));
        IRenderedComponent<Resultados> pagina = this.RenderizarPagina();

        pagina.WaitForElement("tr.linha-clicavel").Click();

        Assert.Equal(
            ordem.Select(dezena => dezena.ToString("00")),
            pagina.FindAll(".ordem-sorteio .bola").Select(Texto));
    }

    /// <summary>
    /// Concurso antigo: arrecadação "Não informada" e sem ordem do sorteio.
    /// </summary>
    [Fact]
    public void DetalheDeConcursoAntigo()
    {
        this.concursos.Add(ConcursoFabrica.Criar(numero: 1, arrecadacao: 0m, ordemSorteio: null));
        IRenderedComponent<Resultados> pagina = this.RenderizarPagina();

        pagina.WaitForElement("tr.linha-clicavel").Click();

        Assert.Contains("Não informada", Texto(pagina.Find(".arrecadacao")));
        Assert.Empty(pagina.FindAll(".ordem-sorteio"));
    }

    /// <summary>
    /// Buscar um número existente mostra só esse concurso.
    /// </summary>
    [Fact]
    public void BuscaMostraSomenteOConcursoEncontrado()
    {
        this.AdicionarConcursos(3807);
        IRenderedComponent<Resultados> pagina = this.RenderizarPagina();
        pagina.WaitForAssertion(() => Assert.Equal(10, NumerosExibidos(pagina).Count));

        pagina.Find(".campo-busca input").Change("3805");

        pagina.WaitForAssertion(() => Assert.Equal([3805], NumerosExibidos(pagina)));
    }

    /// <summary>
    /// Buscar um número que não está na base mostra a mensagem de não encontrado.
    /// </summary>
    [Fact]
    public void BuscaDeConcursoInexistenteMostraMensagem()
    {
        this.AdicionarConcursos(23);
        IRenderedComponent<Resultados> pagina = this.RenderizarPagina();
        pagina.WaitForAssertion(() => Assert.Equal(10, NumerosExibidos(pagina).Count));

        pagina.Find(".campo-busca input").Change("9999");

        pagina.WaitForAssertion(() => Assert.Equal(
            "Concurso 9999 não encontrado na base local",
            Texto(pagina.Find(".sem-registros"))));
        Assert.Empty(NumerosExibidos(pagina));
    }

    /// <summary>
    /// Limpar a busca volta à lista completa, a partir do mais recente.
    /// </summary>
    [Fact]
    public void LimparBuscaVoltaParaListaCompleta()
    {
        this.AdicionarConcursos(23);
        IRenderedComponent<Resultados> pagina = this.RenderizarPagina();
        pagina.WaitForAssertion(() => Assert.Equal(10, NumerosExibidos(pagina).Count));
        pagina.Find(".campo-busca input").Change("5");
        pagina.WaitForAssertion(() => Assert.Equal([5], NumerosExibidos(pagina)));

        pagina.Find(".campo-busca input").Change(string.Empty);

        pagina.WaitForAssertion(() => Assert.Equal(Enumerable.Range(14, 10).Reverse(), NumerosExibidos(pagina)));
    }

    /// <summary>
    /// Com a base vazia, a página mostra a mensagem própria no lugar da lista.
    /// </summary>
    [Fact]
    public void BaseVaziaMostraMensagem()
    {
        IRenderedComponent<Resultados> pagina = this.RenderizarPagina();

        pagina.WaitForAssertion(() => Assert.Equal(
            "Nenhum concurso gravado na base local. Use “Importar histórico” para carregar os concursos da Caixa.",
            Texto(pagina.Find(".sem-registros"))));
    }

    /// <summary>
    /// Importação com concursos novos: mensagem com separador de milhar e lista a partir do mais recente.
    /// </summary>
    [Fact]
    public void ImportacaoComConcursosNovos()
    {
        this.DefinirHistorico(3808);
        IRenderedComponent<MudSnackbarProvider> mensagens = this.Render<MudSnackbarProvider>();
        IRenderedComponent<Resultados> pagina = this.RenderizarPagina();
        pagina.WaitForElement(".sem-registros");

        pagina.Find(".botao-importar").Click();

        mensagens.WaitForAssertion(() => Assert.Contains(
            "Importação concluída: 3.808 concursos no arquivo, 3.808 gravados na base local.",
            MensagensExibidas(mensagens)));
        pagina.WaitForAssertion(() => Assert.Equal(Enumerable.Range(3799, 10).Reverse(), NumerosExibidos(pagina)));
    }

    /// <summary>
    /// Sem concursos novos, a mensagem informa que nada foi gravado.
    /// </summary>
    [Fact]
    public void ImportacaoSemConcursosNovos()
    {
        this.AdicionarConcursos(3808);
        this.DefinirHistorico(3808);
        IRenderedComponent<MudSnackbarProvider> mensagens = this.Render<MudSnackbarProvider>();
        IRenderedComponent<Resultados> pagina = this.RenderizarPagina();
        pagina.WaitForAssertion(() => Assert.Equal(10, NumerosExibidos(pagina).Count));

        pagina.Find(".botao-importar").Click();

        mensagens.WaitForAssertion(() => Assert.Contains(
            "A base local já tem todos os 3.808 concursos do arquivo; nada foi gravado.",
            MensagensExibidas(mensagens)));
        this.repositorio.DidNotReceiveWithAnyArgs().AdicionarAsync(default!, Xunit.TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Enquanto a importação não termina, os dois botões ficam desabilitados e a página indica o andamento.
    /// </summary>
    [Fact]
    public void ImportacaoEmAndamento()
    {
        var download = new TaskCompletionSource<IReadOnlyList<Concurso>>();
        this.fonte.ObterHistoricoAsync(Arg.Any<CancellationToken>()).Returns(download.Task);
        IRenderedComponent<Resultados> pagina = this.RenderizarPagina();

        pagina.Find(".botao-importar").Click();

        Assert.True(pagina.Find(".botao-importar").HasAttribute("disabled"));
        Assert.True(pagina.Find(".botao-atualizar").HasAttribute("disabled"));
        Assert.Equal("Importando o histórico da Caixa…", Texto(pagina.Find(".andamento")));

        download.SetResult([ConcursoFabrica.Criar(numero: 1)]);

        pagina.WaitForAssertion(() => Assert.False(pagina.Find(".botao-importar").HasAttribute("disabled")));
        Assert.False(pagina.Find(".botao-atualizar").HasAttribute("disabled"));
        Assert.Empty(pagina.FindAll(".andamento"));
    }

    /// <summary>
    /// Falha da fonte: mensagem com o motivo, botão habilitado de novo e registro no log de erros.
    /// </summary>
    [Fact]
    public void FalhaNaImportacao()
    {
        var falha = new FonteResultadosException("A Caixa respondeu com o código 503 ao baixar o histórico.");
        this.fonte.ObterHistoricoAsync(Arg.Any<CancellationToken>()).ThrowsAsync(falha);
        IRenderedComponent<MudSnackbarProvider> mensagens = this.Render<MudSnackbarProvider>();
        IRenderedComponent<Resultados> pagina = this.RenderizarPagina();

        pagina.Find(".botao-importar").Click();

        mensagens.WaitForAssertion(() => Assert.Contains(
            "A importação falhou e nada foi gravado. A Caixa respondeu com o código 503 ao baixar o histórico.",
            MensagensExibidas(mensagens)));
        pagina.WaitForAssertion(() => Assert.False(pagina.Find(".botao-importar").HasAttribute("disabled")));
        Assert.Contains((LogLevel.Error, (Exception?)falha), this.log.Registros);
    }

    /// <summary>
    /// Falha não prevista (por exemplo, na gravação): mensagem genérica e registro no log de erros.
    /// </summary>
    [Fact]
    public void FalhaInesperadaNaImportacao()
    {
        this.DefinirHistorico(3);
        var falha = new InvalidOperationException("Banco bloqueado.");
        this.repositorio.AdicionarAsync(Arg.Any<IReadOnlyCollection<Concurso>>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(falha);
        IRenderedComponent<MudSnackbarProvider> mensagens = this.Render<MudSnackbarProvider>();
        IRenderedComponent<Resultados> pagina = this.RenderizarPagina();

        pagina.Find(".botao-importar").Click();

        mensagens.WaitForAssertion(() => Assert.Contains(
            "A importação falhou e nada foi gravado. Erro inesperado; os detalhes foram registrados no log.",
            MensagensExibidas(mensagens)));
        Assert.Contains((LogLevel.Error, (Exception?)falha), this.log.Registros);
    }

    /// <summary>
    /// Atualização com vários concursos novos: mensagem com o intervalo e lista a partir do mais recente.
    /// </summary>
    [Fact]
    public void AtualizacaoComConcursosNovos()
    {
        this.AdicionarConcursos(3808);
        this.fonteConcursos.ObterNumeroUltimoConcursoAsync(Arg.Any<CancellationToken>()).Returns(3811);
        IRenderedComponent<MudSnackbarProvider> mensagens = this.Render<MudSnackbarProvider>();
        IRenderedComponent<Resultados> pagina = this.RenderizarPagina();
        pagina.WaitForAssertion(() => Assert.Equal(10, NumerosExibidos(pagina).Count));

        pagina.Find(".botao-atualizar").Click();

        mensagens.WaitForAssertion(() => Assert.Contains(
            "Atualização concluída: 3 concursos novos gravados (3809 a 3811).",
            MensagensExibidas(mensagens)));
        pagina.WaitForAssertion(() => Assert.Equal(3811, NumerosExibidos(pagina)[0]));
    }

    /// <summary>
    /// Atualização com um único concurso novo usa o singular.
    /// </summary>
    [Fact]
    public void AtualizacaoComUmConcursoNovo()
    {
        this.AdicionarConcursos(3808);
        this.fonteConcursos.ObterNumeroUltimoConcursoAsync(Arg.Any<CancellationToken>()).Returns(3809);
        IRenderedComponent<MudSnackbarProvider> mensagens = this.Render<MudSnackbarProvider>();
        IRenderedComponent<Resultados> pagina = this.RenderizarPagina();

        pagina.Find(".botao-atualizar").Click();

        mensagens.WaitForAssertion(() => Assert.Contains(
            "Atualização concluída: 1 concurso novo gravado (3809).",
            MensagensExibidas(mensagens)));
    }

    /// <summary>
    /// Sem concursos novos, a página informa que a base já está atualizada.
    /// </summary>
    [Fact]
    public void AtualizacaoComBaseJaAtualizada()
    {
        this.AdicionarConcursos(3808);
        this.fonteConcursos.ObterNumeroUltimoConcursoAsync(Arg.Any<CancellationToken>()).Returns(3808);
        IRenderedComponent<MudSnackbarProvider> mensagens = this.Render<MudSnackbarProvider>();
        IRenderedComponent<Resultados> pagina = this.RenderizarPagina();

        pagina.Find(".botao-atualizar").Click();

        mensagens.WaitForAssertion(() => Assert.Contains(
            "A base local já está atualizada: último concurso 3808.",
            MensagensExibidas(mensagens)));
    }

    /// <summary>
    /// Com a base vazia, a página pede a importação antes e a Caixa não é consultada.
    /// </summary>
    [Fact]
    public void AtualizacaoComBaseVazia()
    {
        IRenderedComponent<MudSnackbarProvider> mensagens = this.Render<MudSnackbarProvider>();
        IRenderedComponent<Resultados> pagina = this.RenderizarPagina();

        pagina.Find(".botao-atualizar").Click();

        mensagens.WaitForAssertion(() => Assert.Contains(
            "A base local está vazia. Use “Importar histórico” antes de atualizar os resultados.",
            MensagensExibidas(mensagens)));
        this.fonteConcursos.DidNotReceiveWithAnyArgs().ObterNumeroUltimoConcursoAsync(Xunit.TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Durante a atualização, a página mostra o concurso sendo buscado e desabilita os dois botões.
    /// </summary>
    [Fact]
    public void AtualizacaoEmAndamento()
    {
        this.AdicionarConcursos(3808);
        this.fonteConcursos.ObterNumeroUltimoConcursoAsync(Arg.Any<CancellationToken>()).Returns(3811);
        var busca3810 = new TaskCompletionSource<Concurso>();
        this.fonteConcursos.ObterConcursoAsync(3810, Arg.Any<CancellationToken>()).Returns(busca3810.Task);
        IRenderedComponent<Resultados> pagina = this.RenderizarPagina();

        pagina.Find(".botao-atualizar").Click();

        pagina.WaitForAssertion(() => Assert.Equal("Buscando o concurso 3810 (2 de 3)…", Texto(pagina.Find(".andamento"))));
        Assert.True(pagina.Find(".botao-atualizar").HasAttribute("disabled"));
        Assert.True(pagina.Find(".botao-importar").HasAttribute("disabled"));

        busca3810.SetResult(ConcursoFabrica.Criar(numero: 3810));

        pagina.WaitForAssertion(() => Assert.Empty(pagina.FindAll(".andamento")));
        Assert.False(pagina.Find(".botao-atualizar").HasAttribute("disabled"));
    }

    /// <summary>
    /// Interrupção depois de gravar: mensagem com a falha e o que foi gravado, lista recarregada e log.
    /// </summary>
    [Fact]
    public void AtualizacaoInterrompidaDepoisDeGravar()
    {
        this.AdicionarConcursos(3808);
        this.fonteConcursos.ObterNumeroUltimoConcursoAsync(Arg.Any<CancellationToken>()).Returns(3812);
        var falha = new FonteResultadosException("A Caixa respondeu com o código 503 ao buscar o concurso 3811.");
        this.fonteConcursos.ObterConcursoAsync(3811, Arg.Any<CancellationToken>()).ThrowsAsync(falha);
        IRenderedComponent<MudSnackbarProvider> mensagens = this.Render<MudSnackbarProvider>();
        IRenderedComponent<Resultados> pagina = this.RenderizarPagina();

        pagina.Find(".botao-atualizar").Click();

        mensagens.WaitForAssertion(() => Assert.Contains(
            "A atualização foi interrompida. A Caixa respondeu com o código 503 ao buscar o concurso 3811. "
                + "Concursos gravados antes da falha: 2 (até o 3810).",
            MensagensExibidas(mensagens)));
        pagina.WaitForAssertion(() => Assert.Equal(3810, NumerosExibidos(pagina)[0]));
        Assert.Contains((LogLevel.Error, (Exception?)falha), this.log.Registros);
    }

    /// <summary>
    /// Interrupção depois de gravar o 3809 e o 3810: os jogos desses concursos são conferidos e o premiado do 3809
    /// aparece no diálogo; o jogo premiado do 3808, que já estava na base, e o do 3811 não entram.
    /// </summary>
    [Fact]
    public void AtualizacaoInterrompidaAvisaOsPremiados()
    {
        this.AdicionarConcursos(3808);
        this.jogos.Add(new Jogo(3808, TrezeAcertos, DateTime.Now));
        this.jogos.Add(new Jogo(3809, TrezeAcertos, DateTime.Now));
        this.jogos.Add(new Jogo(3811, TrezeAcertos, DateTime.Now));
        this.fonteConcursos.ObterNumeroUltimoConcursoAsync(Arg.Any<CancellationToken>()).Returns(3812);
        var falha = new FonteResultadosException("A Caixa respondeu com o código 503 ao buscar o concurso 3811.");
        this.fonteConcursos.ObterConcursoAsync(3811, Arg.Any<CancellationToken>()).ThrowsAsync(falha);
        IRenderedComponent<MudSnackbarProvider> mensagens = this.Render<MudSnackbarProvider>();
        IRenderedComponent<MudDialogProvider> dialogos = this.Render<MudDialogProvider>();
        IRenderedComponent<Resultados> pagina = this.RenderizarPagina();

        pagina.Find(".botao-atualizar").Click();

        dialogos.WaitForAssertion(() => Assert.NotEmpty(dialogos.FindAll(".resumo-premios")));
        Assert.Contains(
            "A atualização foi interrompida. A Caixa respondeu com o código 503 ao buscar o concurso 3811. "
                + "Concursos gravados antes da falha: 2 (até o 3810).",
            MensagensExibidas(mensagens));
        Assert.Equal("1 de 1 jogo conferido foi premiado.", Texto(dialogos.Find(".resumo-premios")));
        Assert.Equal(["Concurso 3809 · 17/10/2026"], dialogos.FindAll(".titulo-concurso").Select(Texto));
        Assert.Equal("Jogo 01 · 13 acertos · R$ 35,00", Texto(dialogos.Find(".jogo-premiado")));
    }

    /// <summary>
    /// Jogos dos concursos novos sem prêmio: aviso rápido, sem diálogo.
    /// </summary>
    [Fact]
    public void AtualizacaoComJogosSemPremio()
    {
        this.AdicionarConcursos(3808);
        this.jogos.AddRange(
        [
            new Jogo(3809, NoveAcertos, DateTime.Now),
            new Jogo(3809, [.. ConcursoFabrica.DezenasPadrao.Skip(6), 3, 6, 8, 9, 12, 15], DateTime.Now),
            new Jogo(3810, NoveAcertos, DateTime.Now),
            new Jogo(3810, [.. ConcursoFabrica.DezenasPadrao.Skip(6), 3, 6, 8, 9, 12, 15], DateTime.Now),
        ]);
        this.fonteConcursos.ObterNumeroUltimoConcursoAsync(Arg.Any<CancellationToken>()).Returns(3810);
        IRenderedComponent<MudSnackbarProvider> mensagens = this.Render<MudSnackbarProvider>();
        IRenderedComponent<MudDialogProvider> dialogos = this.Render<MudDialogProvider>();
        IRenderedComponent<Resultados> pagina = this.RenderizarPagina();

        pagina.Find(".botao-atualizar").Click();

        mensagens.WaitForAssertion(() => Assert.Equal(
            [
                "Atualização concluída: 2 concursos novos gravados (3809 a 3810).",
                "Seus 4 jogos dos concursos novos foram conferidos; nenhum foi premiado.",
            ],
            MensagensExibidas(mensagens)));
        Assert.Empty(dialogos.FindAll(".mud-dialog"));
    }

    /// <summary>
    /// Sem concurso gravado, nenhum jogo é conferido, mesmo havendo jogo premiado de concurso antigo.
    /// </summary>
    [Fact]
    public void BaseAtualizadaNaoConfereJogos()
    {
        this.AdicionarConcursos(3808);
        this.jogos.Add(new Jogo(3808, TrezeAcertos, DateTime.Now));
        this.fonteConcursos.ObterNumeroUltimoConcursoAsync(Arg.Any<CancellationToken>()).Returns(3808);
        IRenderedComponent<MudSnackbarProvider> mensagens = this.Render<MudSnackbarProvider>();
        IRenderedComponent<MudDialogProvider> dialogos = this.Render<MudDialogProvider>();
        IRenderedComponent<Resultados> pagina = this.RenderizarPagina();

        pagina.Find(".botao-atualizar").Click();

        mensagens.WaitForAssertion(() => Assert.Equal(
            ["A base local já está atualizada: último concurso 3808."],
            MensagensExibidas(mensagens)));
        Assert.Empty(dialogos.FindAll(".mud-dialog"));
    }

    /// <summary>
    /// Falha não prevista durante a atualização: mensagem genérica e registro no log.
    /// </summary>
    [Fact]
    public void FalhaInesperadaNaAtualizacao()
    {
        this.AdicionarConcursos(3808);
        this.fonteConcursos.ObterNumeroUltimoConcursoAsync(Arg.Any<CancellationToken>()).Returns(3809);
        var falha = new InvalidOperationException("Banco bloqueado.");
        this.repositorio.AdicionarAsync(Arg.Any<IReadOnlyCollection<Concurso>>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(falha);
        IRenderedComponent<MudSnackbarProvider> mensagens = this.Render<MudSnackbarProvider>();
        IRenderedComponent<Resultados> pagina = this.RenderizarPagina();

        pagina.Find(".botao-atualizar").Click();

        mensagens.WaitForAssertion(() => Assert.Contains(
            "A atualização foi interrompida. Erro inesperado; os detalhes foram registrados no log.",
            MensagensExibidas(mensagens)));
        Assert.Contains((LogLevel.Error, (Exception?)falha), this.log.Registros);
    }

    /// <summary>
    /// Textos das mensagens rápidas exibidas.
    /// </summary>
    /// <param name="mensagens">Provedor de mensagens renderizado.</param>
    /// <returns>Textos das mensagens.</returns>
    private static List<string> MensagensExibidas(IRenderedComponent<MudSnackbarProvider> mensagens)
    {
        return mensagens.FindAll(".mud-snackbar-content-message").Select(Texto).ToList();
    }

    /// <summary>
    /// Números dos concursos nas linhas exibidas, de cima para baixo.
    /// </summary>
    /// <param name="pagina">Página renderizada.</param>
    /// <returns>Números exibidos.</returns>
    private static List<int> NumerosExibidos(IRenderedComponent<Resultados> pagina)
    {
        return pagina.FindAll("tr.linha-clicavel")
            .Select(linha => int.Parse(Texto(linha.QuerySelector("td")!), CultureInfo.InvariantCulture))
            .ToList();
    }

    /// <summary>
    /// Texto do elemento sem espaços nas pontas e com espaço comum no lugar do espaço inseparável do "R$".
    /// </summary>
    /// <param name="elemento">Elemento lido.</param>
    /// <returns>Texto normalizado.</returns>
    private static string Texto(IElement elemento)
    {
        return elemento.TextContent.Replace(' ', ' ').Trim();
    }

    /// <summary>
    /// Adiciona os concursos de 1 até o número informado.
    /// </summary>
    /// <param name="ultimo">Número do último concurso.</param>
    private void AdicionarConcursos(int ultimo)
    {
        this.concursos.AddRange(Enumerable.Range(1, ultimo).Select(numero => ConcursoFabrica.Criar(numero: numero)));
    }

    /// <summary>
    /// Faz a fonte do histórico devolver os concursos de 1 até o número informado.
    /// </summary>
    /// <param name="ultimo">Número do último concurso do histórico.</param>
    private void DefinirHistorico(int ultimo)
    {
        List<Concurso> historico = Enumerable.Range(1, ultimo).Select(numero => ConcursoFabrica.Criar(numero: numero)).ToList();
        this.fonte.ObterHistoricoAsync(Arg.Any<CancellationToken>()).Returns(historico);
    }

    /// <summary>
    /// Renderiza a página na cultura pt-BR, a mesma da aplicação.
    /// </summary>
    /// <returns>Página renderizada.</returns>
    private IRenderedComponent<Resultados> RenderizarPagina()
    {
        CultureInfo.CurrentCulture = new CultureInfo("pt-BR");
        CultureInfo.CurrentUICulture = new CultureInfo("pt-BR");

        return this.Render<Resultados>();
    }
}
