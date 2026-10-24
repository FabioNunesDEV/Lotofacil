// <copyright file="AvisoJogosPremiadosTestes.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using System.Globalization;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using Lotofacil.Aplicacao.Jogos;
using Lotofacil.Dominio.Concursos;
using Lotofacil.Dominio.Jogos;
using Lotofacil.Web.Components.Jogos;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;

namespace Lotofacil.Testes.Unidade.Web;

/// <summary>
/// Testes do aviso de jogos premiados e do diálogo "Jogos premiados".
/// </summary>
public class AvisoJogosPremiadosTestes : ComponenteTesteBase
{
    /// <summary>
    /// Dezenas que não saem nos concursos de teste (as dezenas padrão são as sorteadas).
    /// </summary>
    private static readonly int[] NaoSorteadas = [3, 6, 8, 9, 12, 15, 16, 17, 19, 23];

    /// <summary>
    /// Concurso 3809, sorteado em 20/10/2026.
    /// </summary>
    private static readonly Concurso Concurso3809 = ConcursoFabrica.Criar(numero: 3809, data: new DateOnly(2026, 10, 20));

    /// <summary>
    /// Concurso 3810, sorteado em 21/10/2026.
    /// </summary>
    private static readonly Concurso Concurso3810 = ConcursoFabrica.Criar(numero: 3810, data: new DateOnly(2026, 10, 21));

    /// <summary>
    /// Provedor dos diálogos.
    /// </summary>
    private readonly IRenderedComponent<MudDialogProvider> dialogos;

    /// <summary>
    /// Provedor das mensagens rápidas.
    /// </summary>
    private readonly IRenderedComponent<MudSnackbarProvider> mensagens;

    /// <summary>
    /// Renderiza os provedores na cultura da aplicação.
    /// </summary>
    public AvisoJogosPremiadosTestes()
    {
        CultureInfo.CurrentCulture = new CultureInfo("pt-BR");
        CultureInfo.CurrentUICulture = new CultureInfo("pt-BR");
        this.dialogos = this.Render<MudDialogProvider>();
        this.mensagens = this.Render<MudSnackbarProvider>();
    }

    /// <summary>
    /// Dois concursos premiados: resumo, concursos com data e prazo, jogos e total, como no cenário da spec.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task DialogoComDoisConcursos()
    {
        var conferencia = new ConferenciaConcursos(
        [
            Grupo(Concurso3809, Jogo(3809, 10), Jogo(3809, 13), Jogo(3809, 9), Jogo(3809, 8), Jogo(3809, 11)),
            Grupo(Concurso3810, Jogo(3810, 11)),
        ]);

        await this.MostrarAsync(conferencia, new DateOnly(2026, 10, 24));

        this.dialogos.WaitForAssertion(() => Assert.NotEmpty(this.dialogos.FindAll(".resumo-premios")));
        Assert.Equal("Jogos premiados", Texto(this.dialogos.Find(".mud-dialog-title")));
        Assert.Equal("3 de 6 jogos conferidos foram premiados.", Texto(this.dialogos.Find(".resumo-premios")));
        Assert.Equal(
            ["Concurso 3809 · 20/10/2026", "Concurso 3810 · 21/10/2026"],
            this.dialogos.FindAll(".titulo-concurso").Select(Texto));
        Assert.Equal(["Receber até 18/01/2027", "Receber até 19/01/2027"], this.dialogos.FindAll(".prazo").Select(Texto));
        Assert.Equal(
            ["Jogo 02 · 13 acertos · R$ 35,00", "Jogo 05 · 11 acertos · R$ 7,00"],
            this.dialogos.Find(".concurso-premiado[data-concurso='3809']").QuerySelectorAll(".jogo-premiado").Select(Texto));
        Assert.Equal(
            ["Jogo 01 · 11 acertos · R$ 7,00"],
            this.dialogos.Find(".concurso-premiado[data-concurso='3810']").QuerySelectorAll(".jogo-premiado").Select(Texto));
        Assert.Equal("Total: 3 jogos · R$ 49,00", Texto(this.dialogos.Find(".total-premios")));
    }

    /// <summary>
    /// Concurso sem prêmio fica fora; aposta múltipla mostra a quantidade de dezenas; prazo encerrado.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task ApostaMultiplaEPrazoEncerrado()
    {
        int[] dezesseis = [.. ConcursoFabrica.DezenasPadrao.Take(13), 3, 6, 8];
        var conferencia = new ConferenciaConcursos(
        [
            Grupo(Concurso3809, new Jogo(3809, dezesseis, DateTime.Now)),
            Grupo(Concurso3810, Jogo(3810, 10)),
        ]);

        await this.MostrarAsync(conferencia, new DateOnly(2027, 1, 20));

        this.dialogos.WaitForAssertion(() => Assert.NotEmpty(this.dialogos.FindAll(".jogo-premiado")));
        Assert.Equal(["Concurso 3809 · 20/10/2026"], this.dialogos.FindAll(".titulo-concurso").Select(Texto));
        Assert.Equal("Prazo encerrado em 18/01/2027", Texto(this.dialogos.Find(".prazo")));
        Assert.Equal("Jogo 01 · 16 dezenas · 13 acertos · R$ 287,00", Texto(this.dialogos.Find(".jogo-premiado")));
        Assert.Equal("Total: 1 jogo · R$ 287,00", Texto(this.dialogos.Find(".total-premios")));
    }

    /// <summary>
    /// "Ver em Meus jogos" fecha o diálogo e navega; "Fechar" só fecha.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task BotoesDoDialogo()
    {
        var conferencia = new ConferenciaConcursos([Grupo(Concurso3809, Jogo(3809, 11))]);
        var navegacao = this.Services.GetRequiredService<BunitNavigationManager>();

        await this.MostrarAsync(conferencia, new DateOnly(2026, 10, 24));
        this.dialogos.WaitForAssertion(() => Assert.NotEmpty(this.dialogos.FindAll(".botao-fechar")));
        this.dialogos.Find(".botao-fechar").Click();
        this.dialogos.WaitForAssertion(() => Assert.Empty(this.dialogos.FindAll(".mud-dialog")));
        Assert.Equal("http://localhost/", navegacao.Uri);

        await this.MostrarAsync(conferencia, new DateOnly(2026, 10, 24));
        this.dialogos.WaitForAssertion(() => Assert.NotEmpty(this.dialogos.FindAll(".botao-ver-jogos")));
        this.dialogos.Find(".botao-ver-jogos").Click();
        this.dialogos.WaitForAssertion(() => Assert.Empty(this.dialogos.FindAll(".mud-dialog")));
        Assert.Equal("http://localhost/meus-jogos", navegacao.Uri);
    }

    /// <summary>
    /// Jogos conferidos sem prêmio: aviso rápido, sem diálogo.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task NenhumPremio()
    {
        var conferencia = new ConferenciaConcursos(
        [
            Grupo(Concurso3809, Jogo(3809, 10), Jogo(3809, 9)),
            Grupo(Concurso3810, Jogo(3810, 8), Jogo(3810, 10)),
        ]);

        await this.MostrarAsync(conferencia, new DateOnly(2026, 10, 24));

        this.mensagens.WaitForAssertion(() => Assert.Contains(
            "Seus 4 jogos dos concursos novos foram conferidos; nenhum foi premiado.",
            this.mensagens.FindAll(".mud-snackbar-content-message").Select(Texto)));
        Assert.Empty(this.dialogos.FindAll(".mud-dialog"));
    }

    /// <summary>
    /// Sem jogos nos concursos novos, nada é exibido.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task SemJogos()
    {
        await this.MostrarAsync(new ConferenciaConcursos([]), new DateOnly(2026, 10, 24));

        Assert.Empty(this.dialogos.FindAll(".mud-dialog"));
        Assert.Empty(this.mensagens.FindAll(".mud-snackbar-content-message"));
    }

    /// <summary>
    /// Cria um jogo de 15 dezenas com a quantidade de acertos pedida nos concursos de teste.
    /// </summary>
    /// <param name="concurso">Número do concurso.</param>
    /// <param name="acertos">Acertos do jogo.</param>
    /// <returns>O jogo.</returns>
    private static Jogo Jogo(int concurso, int acertos)
    {
        return new Jogo(concurso, [.. ConcursoFabrica.DezenasPadrao.Take(acertos), .. NaoSorteadas.Take(15 - acertos)], DateTime.Now);
    }

    /// <summary>
    /// Cria o grupo conferido de um concurso, numerando os jogos na ordem dada.
    /// </summary>
    /// <param name="concurso">Concurso sorteado.</param>
    /// <param name="jogos">Jogos do concurso.</param>
    /// <returns>O grupo.</returns>
    private static GrupoJogos Grupo(Concurso concurso, params Jogo[] jogos)
    {
        List<JogoConferido> conferidos = jogos
            .Select((jogo, indice) => new JogoConferido(jogo, indice + 1, ConferenciaJogo.Conferir(jogo, concurso)))
            .ToList();

        return new GrupoJogos(concurso.Numero, concurso, conferidos);
    }

    /// <summary>
    /// Texto de um elemento, com espaços não separáveis trocados por espaços comuns.
    /// </summary>
    /// <param name="elemento">Elemento da página.</param>
    /// <returns>Texto sem espaços nas pontas.</returns>
    private static string Texto(IElement elemento)
    {
        return elemento.TextContent.Replace(' ', ' ').Trim();
    }

    /// <summary>
    /// Chama o aviso com os serviços do contexto, no despachante dos componentes.
    /// </summary>
    /// <param name="conferencia">Jogos conferidos.</param>
    /// <param name="hoje">Data de referência.</param>
    /// <returns>Tarefa do aviso.</returns>
    private Task MostrarAsync(ConferenciaConcursos conferencia, DateOnly hoje)
    {
        IDialogService servicoDialogos = this.Services.GetRequiredService<IDialogService>();
        ISnackbar servicoMensagens = this.Services.GetRequiredService<ISnackbar>();

        return this.dialogos.InvokeAsync(() => AvisoJogosPremiados.MostrarAsync(servicoDialogos, servicoMensagens, conferencia, hoje));
    }
}
