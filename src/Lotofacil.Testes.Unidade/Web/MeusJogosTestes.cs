// <copyright file="MeusJogosTestes.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using System.Globalization;
using AngleSharp.Dom;
using Bunit;
using Lotofacil.Aplicacao.Configuracoes;
using Lotofacil.Aplicacao.Jogos;
using Lotofacil.Dominio.Concursos;
using Lotofacil.Dominio.Configuracoes;
using Lotofacil.Dominio.Jogos;
using Lotofacil.Web.Components.Pages;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using NSubstitute;

namespace Lotofacil.Testes.Unidade.Web;

/// <summary>
/// Testes da página Meus jogos, com o serviço real sobre repositórios falsos em memória.
/// </summary>
public class MeusJogosTestes : ComponenteTesteBase
{
    /// <summary>
    /// Jogo do concurso 3808 com 11 acertos (prêmio de R$ 7,00 no rateio padrão).
    /// </summary>
    private static readonly int[] OnzeAcertos = [.. ConcursoFabrica.DezenasPadrao.Take(11), 3, 6, 8, 9];

    /// <summary>
    /// Jogo do concurso 3808 com 10 acertos (sem prêmio).
    /// </summary>
    private static readonly int[] DezAcertos = [.. ConcursoFabrica.DezenasPadrao.Take(10), 3, 6, 8, 9, 12];

    /// <summary>
    /// Concursos gravados no repositório falso.
    /// </summary>
    private readonly List<Concurso> concursos = [];

    /// <summary>
    /// Jogos gravados no repositório falso.
    /// </summary>
    private readonly List<Jogo> jogos = [];

    /// <summary>
    /// Repositório falso dos jogos.
    /// </summary>
    private readonly IJogoRepositorio repositorioJogos = Substitute.For<IJogoRepositorio>();

    /// <summary>
    /// Configurações com o preço padrão de R$ 3,50, sobre um repositório falso sem nada gravado.
    /// </summary>
    private readonly ConfiguracoesServico configuracoes =
        new(Substitute.For<IConfiguracaoApostasRepositorio>(), 3.50m);

    /// <summary>
    /// Provedor das listas suspensas, renderizado ao abrir o cadastro.
    /// </summary>
    private IRenderedComponent<MudPopoverProvider> popovers = null!;

    /// <summary>
    /// Registra o serviço de jogos sobre repositórios que usam as listas <see cref="jogos"/> e
    /// <see cref="concursos"/>, e o preço da aposta.
    /// </summary>
    public MeusJogosTestes()
    {
        IConcursoRepositorio repositorioConcursos = Substitute.For<IConcursoRepositorio>();
        repositorioConcursos.ObterUltimoNumeroAsync(Arg.Any<CancellationToken>())
            .Returns(_ => this.concursos.Count == 0 ? null : this.concursos.Max(concurso => concurso.Numero));
        repositorioConcursos.ObterAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(chamada => this.concursos.Find(concurso => concurso.Numero == chamada.ArgAt<int>(0)));

        this.repositorioJogos.ListarAsync(Arg.Any<CancellationToken>()).Returns(_ => this.jogos.ToList());
        this.repositorioJogos.ExisteAsync(Arg.Any<int>(), Arg.Any<IEnumerable<int>>(), Arg.Any<CancellationToken>())
            .Returns(chamada => this.jogos.Any(jogo =>
                jogo.NumeroConcurso == chamada.ArgAt<int>(0)
                && jogo.Dezenas.SequenceEqual(chamada.ArgAt<IEnumerable<int>>(1).Order())));
        this.repositorioJogos.AdicionarAsync(Arg.Any<Jogo>(), Arg.Any<CancellationToken>())
            .Returns(chamada =>
            {
                this.Gravar(chamada.ArgAt<Jogo>(0));
                return Task.CompletedTask;
            });
        this.repositorioJogos.ExcluirAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(chamada =>
            {
                this.jogos.RemoveAll(jogo => jogo.Id == chamada.ArgAt<int>(0));
                return Task.CompletedTask;
            });

        this.Services.AddSingleton(new JogosServico(this.repositorioJogos, repositorioConcursos));
        this.Services.AddSingleton(this.configuracoes);
    }

    /// <summary>
    /// Sem jogos, a página mostra o título e a mensagem própria.
    /// </summary>
    [Fact]
    public void SemJogosMostraMensagem()
    {
        IRenderedComponent<MeusJogos> pagina = this.RenderizarPagina();

        Assert.Equal("Meus jogos", Texto(pagina.Find("h1")));
        pagina.WaitForAssertion(() => Assert.Equal(
            "Nenhum jogo cadastrado. Use “Novo jogo” para cadastrar o primeiro.",
            Texto(pagina.Find(".sem-registros"))));
    }

    /// <summary>
    /// Os grupos aparecem do concurso maior para o menor, com situação, quantidade e custo ou prêmio.
    /// </summary>
    [Fact]
    public void MostraOsGruposPorConcurso()
    {
        this.MontarCenarioPadrao();

        IRenderedComponent<MeusJogos> pagina = this.RenderizarPagina();

        IReadOnlyList<IElement> grupos = pagina.FindAll(".grupo");
        Assert.Equal(["3810", "3808"], grupos.Select(grupo => grupo.GetAttribute("data-concurso")));

        IElement aguardando = grupos[0];
        Assert.Equal("Concurso 3810", Texto(aguardando.QuerySelector(".titulo-grupo")!));
        Assert.Equal(["Aguardando sorteio", "3 jogos", "Custo R$ 10,50"], Chips(aguardando.QuerySelector(".cabecalho-grupo")!));
        Assert.All(
            aguardando.QuerySelectorAll(".linha-jogo .resultado-jogo"),
            resultado => Assert.Equal("Aguardando", Texto(resultado)));

        IElement conferido = grupos[1];
        Assert.Equal("Concurso 3808 · 17/10/2026", Texto(conferido.QuerySelector(".titulo-grupo")!));
        Assert.Equal(["Conferido", "2 jogos", "Prêmio R$ 7,00"], Chips(conferido.QuerySelector(".cabecalho-grupo")!));
        Assert.Equal("Prescreve em 15/01/2027", Texto(conferido.QuerySelector(".prescricao")!));
        Assert.Equal(15, conferido.QuerySelectorAll(".resultado-oficial .bola").Length);
    }

    /// <summary>
    /// As linhas conferidas mostram número do jogo, acertos, prêmio e as dezenas acertadas destacadas.
    /// </summary>
    [Fact]
    public void MostraAsLinhasConferidas()
    {
        this.MontarCenarioPadrao();

        IRenderedComponent<MeusJogos> pagina = this.RenderizarPagina();

        IReadOnlyList<IElement> linhas = pagina.FindAll(".grupo[data-concurso='3808'] .linha-jogo");
        Assert.Equal("01", Texto(linhas[0].QuerySelector(".numero-jogo")!));
        Assert.Equal("11 acertos", Texto(linhas[0].QuerySelector(".mud-chip")!));
        Assert.Equal("R$ 7,00", Texto(linhas[0].QuerySelector(".premio")!));
        Assert.Equal(11, linhas[0].QuerySelectorAll(".bola.acerto").Length);
        Assert.Equal("02", Texto(linhas[1].QuerySelector(".numero-jogo")!));
        Assert.Equal("10 acertos", Texto(linhas[1].QuerySelector(".mud-chip")!));
        Assert.Equal("—", Texto(linhas[1].QuerySelector(".premio")!));
    }

    /// <summary>
    /// Os indicadores consideram todos os jogos.
    /// </summary>
    [Fact]
    public void MostraOsIndicadores()
    {
        this.MontarCenarioPadrao();

        IRenderedComponent<MeusJogos> pagina = this.RenderizarPagina();

        AssertIndicadores(pagina, "5", "2 concursos", "R$ 17,50", "5 apostas de 15 dezenas", "1", "R$ 7,00");
    }

    /// <summary>
    /// Com uma aposta de 16 dezenas, custo e investido somam as apostas de 15 que cada jogo vale, e a linha mostra a
    /// quantidade de dezenas.
    /// </summary>
    [Fact]
    public void ApostaMultiplaNoCustoENosIndicadores()
    {
        this.concursos.Add(ConcursoFabrica.Criar(numero: 3808));
        this.Gravar(new Jogo(3810, Enumerable.Range(1, 15), DateTime.Now));
        this.Gravar(new Jogo(3810, Enumerable.Range(2, 15), DateTime.Now));
        this.Gravar(new Jogo(3810, Enumerable.Range(1, 16), DateTime.Now));

        IRenderedComponent<MeusJogos> pagina = this.RenderizarPagina();

        IElement grupo = pagina.Find(".grupo[data-concurso='3810']");
        Assert.Equal(["Aguardando sorteio", "3 jogos", "Custo R$ 63,00"], Chips(grupo.QuerySelector(".cabecalho-grupo")!));
        IReadOnlyList<IElement> linhas = pagina.FindAll(".grupo[data-concurso='3810'] .linha-jogo");
        Assert.Empty(linhas[0].QuerySelectorAll(".quantidade-dezenas"));
        Assert.Equal("16 dezenas", Texto(linhas[2].QuerySelector(".quantidade-dezenas")!));
        Assert.Equal(16, linhas[2].QuerySelectorAll(".bola").Length);
        AssertIndicadores(pagina, "3", "1 concurso", "R$ 63,00", "18 apostas de 15 dezenas", "0", "R$ 0,00");
    }

    /// <summary>
    /// Com o preço salvo em Configurações, custo do grupo, investido e cadastro usam o preço novo.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task PrecoSalvoValeNosCustos()
    {
        this.concursos.Add(ConcursoFabrica.Criar(numero: 3808));
        this.Gravar(new Jogo(3810, Enumerable.Range(1, 15), DateTime.Now));
        this.Gravar(new Jogo(3810, Enumerable.Range(2, 15), DateTime.Now));
        this.Gravar(new Jogo(3810, Enumerable.Range(1, 16), DateTime.Now));
        await this.configuracoes.SalvarPrecoApostaMinimaAsync(4.00m, Xunit.TestContext.Current.CancellationToken);

        IRenderedComponent<MudDialogProvider> dialogos = this.Render<MudDialogProvider>();
        IRenderedComponent<MeusJogos> pagina = this.RenderizarPagina();

        IElement grupo = pagina.Find(".grupo[data-concurso='3810']");
        Assert.Equal(["Aguardando sorteio", "3 jogos", "Custo R$ 72,00"], Chips(grupo.QuerySelector(".cabecalho-grupo")!));
        AssertIndicadores(pagina, "3", "1 concurso", "R$ 72,00", "18 apostas de 15 dezenas", "0", "R$ 0,00");

        pagina.Find(".botao-novo").Click();

        dialogos.WaitForAssertion(() => Assert.Equal("0 de 15 dezenas · R$ 4,00", Texto(dialogos.Find(".contagem"))));
    }

    /// <summary>
    /// Os filtros escondem grupos sem mudar os indicadores.
    /// </summary>
    [Fact]
    public void FiltraOsGrupos()
    {
        this.MontarCenarioPadrao();
        IRenderedComponent<MeusJogos> pagina = this.RenderizarPagina();

        Filtro(pagina, "Premiados").Click();
        Assert.Equal(["3808"], pagina.FindAll(".grupo").Select(grupo => grupo.GetAttribute("data-concurso")));
        AssertIndicadores(pagina, "5", "2 concursos", "R$ 17,50", "5 apostas de 15 dezenas", "1", "R$ 7,00");

        Filtro(pagina, "Aguardando sorteio").Click();
        Assert.Equal(["3810"], pagina.FindAll(".grupo").Select(grupo => grupo.GetAttribute("data-concurso")));

        Filtro(pagina, "Conferidos").Click();
        Assert.Equal(["3808"], pagina.FindAll(".grupo").Select(grupo => grupo.GetAttribute("data-concurso")));
    }

    /// <summary>
    /// O cadastro sugere o próximo concurso e 15 dezenas, conta as dezenas com o custo, só salva com 15 e recarrega
    /// a lista.
    /// </summary>
    [Fact]
    public void CadastraUmJogo()
    {
        this.MontarCenarioPadrao();
        IRenderedComponent<MudDialogProvider> dialogos = this.Render<MudDialogProvider>();
        IRenderedComponent<MudSnackbarProvider> mensagens = this.Render<MudSnackbarProvider>();
        IRenderedComponent<MeusJogos> pagina = this.RenderizarPagina();

        pagina.Find(".botao-novo").Click();

        dialogos.WaitForAssertion(() => Assert.Equal("3809", dialogos.Find(".campo-concurso input").GetAttribute("value")));
        Assert.Equal("15", dialogos.Find(".campo-dezenas-por-jogo input").GetAttribute("value"));
        Assert.Equal("0 de 15 dezenas · R$ 3,50", Texto(dialogos.Find(".contagem")));
        Assert.True(dialogos.Find(".botao-salvar").HasAttribute("disabled"));

        MarcarDezenas(dialogos, Enumerable.Range(1, 14));
        Assert.Equal("14 de 15 dezenas · R$ 3,50", Texto(dialogos.Find(".contagem")));
        Assert.True(dialogos.Find(".botao-salvar").HasAttribute("disabled"));

        MarcarDezenas(dialogos, [15]);
        Assert.Equal("15 de 15 dezenas · R$ 3,50", Texto(dialogos.Find(".contagem")));
        Assert.False(dialogos.Find(".botao-salvar").HasAttribute("disabled"));
        Assert.True(Dezena(dialogos, 16).HasAttribute("disabled"));

        dialogos.Find(".botao-salvar").Click();

        mensagens.WaitForAssertion(() => Assert.Contains("Jogo salvo para o concurso 3809.", MensagensExibidas(mensagens)));
        pagina.WaitForAssertion(() => Assert.Equal(
            ["3810", "3809", "3808"],
            pagina.FindAll(".grupo").Select(grupo => grupo.GetAttribute("data-concurso"))));
        Assert.Empty(dialogos.FindAll(".campo-concurso"));
    }

    /// <summary>
    /// Com 17 dezenas por jogo, a contagem mostra o custo da aposta e o volante permite marcar até 17.
    /// </summary>
    [Fact]
    public void CadastroDeApostaDeDezesseteDezenas()
    {
        this.MontarCenarioPadrao();
        IRenderedComponent<MudDialogProvider> dialogos = this.AbrirCadastro();

        this.EscolherDezenasPorJogo(dialogos, 17);
        MarcarDezenas(dialogos, Enumerable.Range(1, 12));

        Assert.Equal("12 de 17 dezenas · R$ 476,00", Texto(dialogos.Find(".contagem")));
        Assert.True(dialogos.Find(".botao-salvar").HasAttribute("disabled"));

        MarcarDezenas(dialogos, Enumerable.Range(13, 5));
        Assert.Equal("17 de 17 dezenas · R$ 476,00", Texto(dialogos.Find(".contagem")));
        Assert.False(dialogos.Find(".botao-salvar").HasAttribute("disabled"));
        Assert.True(Dezena(dialogos, 18).HasAttribute("disabled"));
    }

    /// <summary>
    /// Reduzir a quantidade mantém as marcações, bloqueia novas e só libera o Salvar ao chegar à nova quantidade.
    /// </summary>
    [Fact]
    public void ReduzirAQuantidadeMantemAsMarcacoes()
    {
        this.MontarCenarioPadrao();
        IRenderedComponent<MudDialogProvider> dialogos = this.AbrirCadastro();
        this.EscolherDezenasPorJogo(dialogos, 17);
        MarcarDezenas(dialogos, Enumerable.Range(1, 17));

        this.EscolherDezenasPorJogo(dialogos, 15);

        Assert.Equal("17 de 15 dezenas · R$ 3,50", Texto(dialogos.Find(".contagem")));
        Assert.Equal(17, dialogos.FindAll(".volante .marcada").Count);
        Assert.True(dialogos.Find(".botao-salvar").HasAttribute("disabled"));
        Assert.True(Dezena(dialogos, 18).HasAttribute("disabled"));

        MarcarDezenas(dialogos, [16, 17]);
        Assert.Equal("15 de 15 dezenas · R$ 3,50", Texto(dialogos.Find(".contagem")));
        Assert.False(dialogos.Find(".botao-salvar").HasAttribute("disabled"));
    }

    /// <summary>
    /// Aberta pelo atalho do Painel, a página abre o cadastro e tira o pedido do endereço.
    /// </summary>
    [Fact]
    public void AtalhoDoPainelAbreOCadastro()
    {
        this.MontarCenarioPadrao();
        NavigationManager navegacao = this.Services.GetRequiredService<NavigationManager>();
        navegacao.NavigateTo("meus-jogos?novo=true");
        IRenderedComponent<MudDialogProvider> dialogos = this.Render<MudDialogProvider>();

        this.RenderizarPagina();

        dialogos.WaitForAssertion(() => Assert.Equal("3809", dialogos.Find(".campo-concurso input").GetAttribute("value")));
        Assert.Equal($"{navegacao.BaseUri}meus-jogos", navegacao.Uri);
    }

    /// <summary>
    /// Sem o parâmetro, a página não abre o cadastro.
    /// </summary>
    [Fact]
    public void SemOParametroNaoAbreOCadastro()
    {
        this.MontarCenarioPadrao();
        IRenderedComponent<MudDialogProvider> dialogos = this.Render<MudDialogProvider>();

        this.RenderizarPagina();

        Assert.Empty(dialogos.FindAll(".campo-concurso"));
    }

    /// <summary>
    /// O jogo salvo pelo cadastro aberto pelo atalho aparece na lista.
    /// </summary>
    [Fact]
    public void SalvaPeloAtalhoDoPainel()
    {
        this.MontarCenarioPadrao();
        this.Services.GetRequiredService<NavigationManager>().NavigateTo("meus-jogos?novo=true");
        IRenderedComponent<MudDialogProvider> dialogos = this.Render<MudDialogProvider>();
        IRenderedComponent<MudSnackbarProvider> mensagens = this.Render<MudSnackbarProvider>();
        IRenderedComponent<MeusJogos> pagina = this.RenderizarPagina();
        dialogos.WaitForAssertion(() => Assert.NotEmpty(dialogos.FindAll(".campo-concurso")));

        MarcarDezenas(dialogos, Enumerable.Range(1, 15));
        dialogos.Find(".botao-salvar").Click();

        mensagens.WaitForAssertion(() => Assert.Contains("Jogo salvo para o concurso 3809.", MensagensExibidas(mensagens)));
        pagina.WaitForAssertion(() => Assert.Equal(
            ["3810", "3809", "3808"],
            pagina.FindAll(".grupo").Select(grupo => grupo.GetAttribute("data-concurso"))));
    }

    /// <summary>
    /// Aposta de 20 dezenas é salva direto, sem pedido de confirmação, e aparece com a quantidade de dezenas.
    /// </summary>
    [Fact]
    public void SalvaApostaDeVinteDezenasSemConfirmacao()
    {
        this.MontarCenarioPadrao();
        this.popovers = this.Render<MudPopoverProvider>();
        IRenderedComponent<MudSnackbarProvider> mensagens = this.Render<MudSnackbarProvider>();
        IRenderedComponent<MudDialogProvider> dialogos = this.Render<MudDialogProvider>();
        IRenderedComponent<MeusJogos> pagina = this.RenderizarPagina();
        pagina.Find(".botao-novo").Click();
        dialogos.WaitForAssertion(() => Assert.NotEmpty(dialogos.FindAll(".campo-concurso")));

        this.EscolherDezenasPorJogo(dialogos, 20);
        MarcarDezenas(dialogos, Enumerable.Range(1, 20));
        Assert.Equal("20 de 20 dezenas · R$ 54.264,00", Texto(dialogos.Find(".contagem")));
        dialogos.Find(".botao-salvar").Click();

        mensagens.WaitForAssertion(() => Assert.Contains("Jogo salvo para o concurso 3809.", MensagensExibidas(mensagens)));
        Assert.Empty(dialogos.FindAll(".mud-dialog"));
        Assert.Equal(20, this.jogos.Single(jogo => jogo.NumeroConcurso == 3809).Dezenas.Count);
        pagina.WaitForAssertion(() => Assert.Contains(
            "20 dezenas",
            Texto(pagina.Find(".grupo[data-concurso='3809'] .linha-jogo"))));
    }

    /// <summary>
    /// Concurso já sorteado é recusado com a mensagem, e o diálogo continua aberto.
    /// </summary>
    [Fact]
    public void RecusaConcursoJaSorteado()
    {
        this.MontarCenarioPadrao();
        IRenderedComponent<MudDialogProvider> dialogos = this.Render<MudDialogProvider>();
        IRenderedComponent<MudSnackbarProvider> mensagens = this.Render<MudSnackbarProvider>();
        IRenderedComponent<MeusJogos> pagina = this.RenderizarPagina();
        pagina.Find(".botao-novo").Click();
        dialogos.WaitForElement(".campo-concurso input");

        dialogos.Find(".campo-concurso input").Change("3808");
        MarcarDezenas(dialogos, Enumerable.Range(1, 15));
        dialogos.Find(".botao-salvar").Click();

        mensagens.WaitForAssertion(() => Assert.Contains(
            "Só é possível cadastrar jogos para concursos depois do 3808.",
            MensagensExibidas(mensagens)));
        Assert.Single(dialogos.FindAll(".campo-concurso"));
        Assert.Equal(5, this.jogos.Count);
    }

    /// <summary>
    /// O mesmo jogo no mesmo concurso é recusado com a mensagem.
    /// </summary>
    [Fact]
    public void RecusaJogoDuplicado()
    {
        this.MontarCenarioPadrao();
        IRenderedComponent<MudDialogProvider> dialogos = this.Render<MudDialogProvider>();
        IRenderedComponent<MudSnackbarProvider> mensagens = this.Render<MudSnackbarProvider>();
        IRenderedComponent<MeusJogos> pagina = this.RenderizarPagina();
        pagina.Find(".botao-novo").Click();
        dialogos.WaitForElement(".campo-concurso input");

        dialogos.Find(".campo-concurso input").Change("3810");
        MarcarDezenas(dialogos, Enumerable.Range(1, 15));
        dialogos.Find(".botao-salvar").Click();

        mensagens.WaitForAssertion(() => Assert.Contains(
            "Este jogo já está cadastrado para o concurso 3810.",
            MensagensExibidas(mensagens)));
        Assert.Equal(5, this.jogos.Count);
    }

    /// <summary>
    /// Com a base vazia, não há concurso sugerido e salvar pede a importação do histórico.
    /// </summary>
    [Fact]
    public void RecusaComBaseVazia()
    {
        IRenderedComponent<MudDialogProvider> dialogos = this.Render<MudDialogProvider>();
        IRenderedComponent<MudSnackbarProvider> mensagens = this.Render<MudSnackbarProvider>();
        IRenderedComponent<MeusJogos> pagina = this.RenderizarPagina();
        pagina.Find(".botao-novo").Click();
        dialogos.WaitForElement(".campo-concurso input");
        Assert.True(string.IsNullOrEmpty(dialogos.Find(".campo-concurso input").GetAttribute("value")));

        dialogos.Find(".campo-concurso input").Change("1");
        MarcarDezenas(dialogos, Enumerable.Range(1, 15));
        dialogos.Find(".botao-salvar").Click();

        mensagens.WaitForAssertion(() => Assert.Contains(
            "Importe o histórico na página Resultados antes de cadastrar jogos.",
            MensagensExibidas(mensagens)));
        Assert.Empty(this.jogos);
    }

    /// <summary>
    /// Cada grupo tem o botão "Exportar", que abre o diálogo com o texto dos jogos daquele concurso.
    /// </summary>
    [Fact]
    public void ExportaOsJogosDoConcurso()
    {
        this.MontarCenarioPadrao();
        IRenderedComponent<MudDialogProvider> dialogos = this.Render<MudDialogProvider>();
        IRenderedComponent<MeusJogos> pagina = this.RenderizarPagina();

        Assert.Equal(2, pagina.FindAll(".grupo .botao-exportar").Count);
        pagina.Find(".grupo[data-concurso='3810'] .botao-exportar").Click();

        dialogos.WaitForAssertion(() => Assert.Equal("Exportar · concurso 3810", Texto(dialogos.Find(".mud-dialog-title"))));
        Assert.Equal(
            "Lotofácil · concurso 3810\n3 jogos · R$ 10,50\n\n"
                + "01) 01 02 03 04 05 06 07 08 09 10 11 12 13 14 15\n"
                + "02) 02 03 04 05 06 07 08 09 10 11 12 13 14 15 16\n"
                + "03) 03 04 05 06 07 08 09 10 11 12 13 14 15 16 17",
            dialogos.Find(".texto-exportacao textarea").GetAttribute("value")!.Replace(' ', ' '));
        Assert.Equal("concurso-3810.txt", dialogos.Find("a.botao-baixar").GetAttribute("download"));
    }

    /// <summary>
    /// Clicar num jogo conferido abre a conferência com o volante e os detalhes.
    /// </summary>
    [Fact]
    public void AbreAConferenciaDeUmJogoConferido()
    {
        this.MontarCenarioPadrao();
        IRenderedComponent<MudDialogProvider> dialogos = this.Render<MudDialogProvider>();
        IRenderedComponent<MeusJogos> pagina = this.RenderizarPagina();

        pagina.FindAll(".grupo[data-concurso='3808'] .linha-jogo")[0].Click();

        dialogos.WaitForAssertion(() => Assert.Contains("Jogo 01 · concurso 3808", dialogos.Find(".mud-dialog-title").TextContent));
        Assert.Equal(
            ["17/10/2026", "15 dezenas", "11 de 15", "11 acertos", "R$ 7,00", "15/01/2027"],
            dialogos.FindAll(".detalhes dd").Select(Texto));
        Assert.Equal(11, dialogos.FindAll(".volante .acerto").Count);
        Assert.Equal(4, dialogos.FindAll(".volante .erro").Count);
        Assert.Equal(4, dialogos.FindAll(".volante .nao-marcada").Count);
        Assert.Empty(dialogos.FindAll(".premios-faixa"));
    }

    /// <summary>
    /// A conferência de uma aposta de 16 dezenas com 13 acertos mostra o tamanho da aposta, o prêmio total e os
    /// prêmios por faixa.
    /// </summary>
    [Fact]
    public void AbreAConferenciaDeUmaApostaMultipla()
    {
        this.concursos.Add(ConcursoFabrica.Criar(numero: 3808));
        this.Gravar(new Jogo(3808, [.. ConcursoFabrica.DezenasPadrao.Take(13), 3, 6, 8], DateTime.Now));
        IRenderedComponent<MudDialogProvider> dialogos = this.Render<MudDialogProvider>();
        IRenderedComponent<MeusJogos> pagina = this.RenderizarPagina();

        pagina.Find(".grupo[data-concurso='3808'] .linha-jogo").Click();

        dialogos.WaitForAssertion(() => Assert.Contains("Jogo 01 · concurso 3808", dialogos.Find(".mud-dialog-title").TextContent));
        Assert.Equal(
            ["17/10/2026", "16 dezenas · 16 apostas", "13 de 15", "13 acertos", "R$ 287,00", "15/01/2027"],
            dialogos.FindAll(".detalhes dd").Select(Texto));
        Assert.Equal(
            ["3 × 13 acertos · R$ 35,00", "13 × 12 acertos · R$ 14,00"],
            dialogos.FindAll(".premios-faixa li").Select(Texto));
        Assert.Equal(13, dialogos.FindAll(".volante .acerto").Count);
        Assert.Equal(3, dialogos.FindAll(".volante .erro").Count);
        Assert.Equal(2, dialogos.FindAll(".volante .nao-marcada").Count);
    }

    /// <summary>
    /// Jogo aguardando sorteio não abre a conferência.
    /// </summary>
    [Fact]
    public void JogoAguardandoNaoAbreConferencia()
    {
        this.MontarCenarioPadrao();
        IRenderedComponent<MudDialogProvider> dialogos = this.Render<MudDialogProvider>();
        IRenderedComponent<MeusJogos> pagina = this.RenderizarPagina();

        pagina.FindAll(".grupo[data-concurso='3810'] .linha-jogo")[0].Click();

        Assert.Empty(dialogos.FindAll(".mud-dialog"));
    }

    /// <summary>
    /// Excluir pede confirmação; confirmado, o jogo sai, os demais são renumerados e os indicadores mudam.
    /// </summary>
    [Fact]
    public void ExcluiComConfirmacao()
    {
        this.MontarCenarioPadrao();
        IRenderedComponent<MudDialogProvider> dialogos = this.Render<MudDialogProvider>();
        IRenderedComponent<MudSnackbarProvider> mensagens = this.Render<MudSnackbarProvider>();
        IRenderedComponent<MeusJogos> pagina = this.RenderizarPagina();

        pagina.FindAll(".grupo[data-concurso='3810'] .botao-excluir")[0].Click();

        dialogos.WaitForAssertion(() => Assert.Contains(
            "Excluir o jogo 01 do concurso 3810?",
            dialogos.Find(".mud-dialog").TextContent));
        Botao(dialogos, "Excluir").Click();

        mensagens.WaitForAssertion(() => Assert.Contains("Jogo excluído.", MensagensExibidas(mensagens)));
        pagina.WaitForAssertion(() => Assert.Equal(
            ["01", "02"],
            pagina.FindAll(".grupo[data-concurso='3810'] .numero-jogo").Select(Texto)));
        Assert.Equal("4", Texto(pagina.Find(".jogos-salvos .valor")));
        Assert.Empty(dialogos.FindAll(".mud-dialog"));
    }

    /// <summary>
    /// Cancelar a confirmação mantém o jogo.
    /// </summary>
    [Fact]
    public void CancelarExclusaoMantemOJogo()
    {
        this.MontarCenarioPadrao();
        IRenderedComponent<MudDialogProvider> dialogos = this.Render<MudDialogProvider>();
        IRenderedComponent<MeusJogos> pagina = this.RenderizarPagina();

        pagina.FindAll(".grupo[data-concurso='3810'] .botao-excluir")[0].Click();
        dialogos.WaitForElement(".mud-dialog");
        Botao(dialogos, "Cancelar").Click();

        dialogos.WaitForAssertion(() => Assert.Empty(dialogos.FindAll(".mud-dialog")));
        Assert.Equal(5, this.jogos.Count);
        Assert.Equal(3, pagina.FindAll(".grupo[data-concurso='3810'] .linha-jogo").Count);
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
    /// Textos dos chips de um elemento.
    /// </summary>
    /// <param name="elemento">Elemento com chips.</param>
    /// <returns>Textos dos chips.</returns>
    private static List<string> Chips(IElement elemento)
    {
        return elemento.QuerySelectorAll(".mud-chip").Select(Texto).ToList();
    }

    /// <summary>
    /// Confere os valores e detalhes dos quatro indicadores.
    /// </summary>
    /// <param name="pagina">Página renderizada.</param>
    /// <param name="jogos">Jogos salvos.</param>
    /// <param name="concursos">Detalhe dos jogos salvos.</param>
    /// <param name="investido">Valor investido.</param>
    /// <param name="apostas">Detalhe do investido.</param>
    /// <param name="premiados">Jogos premiados.</param>
    /// <param name="premios">Total em prêmios.</param>
    private static void AssertIndicadores(
        IRenderedComponent<MeusJogos> pagina,
        string jogos,
        string concursos,
        string investido,
        string apostas,
        string premiados,
        string premios)
    {
        Assert.Equal(jogos, Texto(pagina.Find(".jogos-salvos .valor")));
        Assert.Equal(concursos, Texto(pagina.Find(".jogos-salvos .detalhe")));
        Assert.Equal(investido, Texto(pagina.Find(".investido .valor")));
        Assert.Equal(apostas, Texto(pagina.Find(".investido .detalhe")));
        Assert.Equal(premiados, Texto(pagina.Find(".premiados .valor")));
        Assert.Equal(premios, Texto(pagina.Find(".total-premios .valor")));
    }

    /// <summary>
    /// Encontra o chip de filtro pelo texto.
    /// </summary>
    /// <param name="pagina">Página renderizada.</param>
    /// <param name="texto">Texto do chip.</param>
    /// <returns>Elemento do chip.</returns>
    private static IElement Filtro(IRenderedComponent<MeusJogos> pagina, string texto)
    {
        return pagina.FindAll(".filtros .mud-chip").Single(chip => Texto(chip) == texto);
    }

    /// <summary>
    /// Encontra uma dezena do volante do diálogo.
    /// </summary>
    /// <param name="dialogos">Provedor de diálogos renderizado.</param>
    /// <param name="dezena">Dezena procurada.</param>
    /// <returns>Botão da dezena.</returns>
    private static IElement Dezena(IRenderedComponent<MudDialogProvider> dialogos, int dezena)
    {
        return dialogos.FindAll(".volante button").Single(botao => Texto(botao) == dezena.ToString("00"));
    }

    /// <summary>
    /// Clica nas dezenas do volante do diálogo, esperando cada clique ser aplicado antes do seguinte.
    /// </summary>
    /// <param name="dialogos">Provedor de diálogos renderizado.</param>
    /// <param name="dezenas">Dezenas a marcar ou desmarcar.</param>
    private static void MarcarDezenas(IRenderedComponent<MudDialogProvider> dialogos, IEnumerable<int> dezenas)
    {
        foreach (int dezena in dezenas)
        {
            string? antes = Dezena(dialogos, dezena).GetAttribute("aria-pressed");
            Dezena(dialogos, dezena).Click();

            // Com a máquina carregada, o próximo clique pode chegar antes de o volante receber a marcação anterior.
            dialogos.WaitForAssertion(() => Assert.NotEqual(antes, Dezena(dialogos, dezena).GetAttribute("aria-pressed")));
        }
    }

    /// <summary>
    /// Encontra um botão do diálogo pelo texto.
    /// </summary>
    /// <param name="dialogos">Provedor de diálogos renderizado.</param>
    /// <param name="texto">Texto do botão.</param>
    /// <returns>Botão encontrado.</returns>
    private static IElement Botao(IRenderedComponent<MudDialogProvider> dialogos, string texto)
    {
        return dialogos.FindAll(".mud-dialog button").Single(botao => Texto(botao) == texto);
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
    /// Escolhe a quantidade de dezenas por jogo no cadastro, abrindo a lista e clicando na opção, e confere que o
    /// campo passa a exibir a quantidade escolhida.
    /// </summary>
    /// <param name="dialogos">Provedor de diálogos renderizado.</param>
    /// <param name="quantidade">Quantidade de dezenas.</param>
    private void EscolherDezenasPorJogo(IRenderedComponent<MudDialogProvider> dialogos, int quantidade)
    {
        dialogos.Find(".campo-dezenas-por-jogo").MouseDown();
        this.popovers.WaitForAssertion(() => Assert.NotEmpty(this.popovers.FindAll(".mud-popover-open .mud-list-item")));
        this.popovers.FindAll(".mud-popover-open .mud-list-item")
            .Single(opcao => Texto(opcao) == quantidade.ToString())
            .Click();

        dialogos.WaitForAssertion(() => Assert.Equal(
            quantidade.ToString(),
            Texto(dialogos.Find(".campo-dezenas-por-jogo div[role='combobox']"))));
    }

    /// <summary>
    /// Monta o cenário: concurso 3808 gravado, com dois jogos (11 e 10 acertos), e três jogos aguardando o 3810.
    /// </summary>
    private void MontarCenarioPadrao()
    {
        this.concursos.Add(ConcursoFabrica.Criar(numero: 3808));
        this.Gravar(new Jogo(3808, OnzeAcertos, DateTime.Now));
        this.Gravar(new Jogo(3808, DezAcertos, DateTime.Now));
        this.Gravar(new Jogo(3810, Enumerable.Range(1, 15), DateTime.Now));
        this.Gravar(new Jogo(3810, Enumerable.Range(2, 15), DateTime.Now));
        this.Gravar(new Jogo(3810, Enumerable.Range(3, 15), DateTime.Now));
    }

    /// <summary>
    /// Grava o jogo na lista com um identificador novo, como o banco faria.
    /// </summary>
    /// <param name="jogo">Jogo a gravar.</param>
    private void Gravar(Jogo jogo)
    {
        typeof(Jogo).GetProperty(nameof(Jogo.Id))!.SetValue(jogo, this.jogos.Count == 0 ? 1 : this.jogos.Max(item => item.Id) + 1);
        this.jogos.Add(jogo);
    }

    /// <summary>
    /// Renderiza a página e abre o diálogo de cadastro.
    /// </summary>
    /// <returns>Provedor de diálogos com o cadastro aberto.</returns>
    private IRenderedComponent<MudDialogProvider> AbrirCadastro()
    {
        this.popovers = this.Render<MudPopoverProvider>();
        IRenderedComponent<MudDialogProvider> dialogos = this.Render<MudDialogProvider>();
        IRenderedComponent<MeusJogos> pagina = this.RenderizarPagina();
        pagina.Find(".botao-novo").Click();
        dialogos.WaitForAssertion(() => Assert.NotEmpty(dialogos.FindAll(".campo-concurso")));

        return dialogos;
    }

    /// <summary>
    /// Renderiza a página na cultura pt-BR, a mesma da aplicação, e espera a primeira carga.
    /// </summary>
    /// <returns>Página renderizada.</returns>
    private IRenderedComponent<MeusJogos> RenderizarPagina()
    {
        CultureInfo.CurrentCulture = new CultureInfo("pt-BR");
        CultureInfo.CurrentUICulture = new CultureInfo("pt-BR");

        IRenderedComponent<MeusJogos> pagina = this.Render<MeusJogos>();
        pagina.WaitForAssertion(() => Assert.NotEmpty(pagina.FindAll(".indicador")));

        return pagina;
    }
}
