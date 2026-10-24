// <copyright file="PainelTestes.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using System.Globalization;
using AngleSharp.Dom;
using Bunit;
using Lotofacil.Aplicacao.Concursos;
using Lotofacil.Aplicacao.Estatisticas;
using Lotofacil.Aplicacao.Jogos;
using Lotofacil.Aplicacao.Painel;
using Lotofacil.Aplicacao.Teimosinhas;
using Lotofacil.Dominio.Concursos;
using Lotofacil.Dominio.Estatisticas;
using Lotofacil.Dominio.Jogos;
using Lotofacil.Dominio.Teimosinhas;
using Lotofacil.Web.Components.Pages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MudBlazor;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Lotofacil.Testes.Unidade.Web;

/// <summary>
/// Testes da página Painel.
/// </summary>
public class PainelTestes : ComponenteTesteBase
{
    /// <summary>
    /// Dezenas de um jogo que acerta 13 dezenas do resultado padrão.
    /// </summary>
    private static readonly int[] TrezeAcertos = [.. ConcursoFabrica.DezenasPadrao.Take(13), 3, 6];

    /// <summary>
    /// Dezenas de um jogo que acerta 9 dezenas do resultado padrão.
    /// </summary>
    private static readonly int[] NoveAcertos = [.. ConcursoFabrica.DezenasPadrao.Take(9), 3, 6, 8, 9, 12, 15];

    /// <summary>
    /// Concursos gravados na base falsa.
    /// </summary>
    private readonly List<Concurso> concursos = [];

    /// <summary>
    /// Jogos gravados na base falsa.
    /// </summary>
    private readonly List<Jogo> jogos = [];

    /// <summary>
    /// Teimosinhas gravadas em memória.
    /// </summary>
    private readonly TeimosinhaRepositorioFalso teimosinhas = new();

    /// <summary>
    /// Repositório falso dos concursos.
    /// </summary>
    private readonly IConcursoRepositorio repositorio = Substitute.For<IConcursoRepositorio>();

    /// <summary>
    /// Fonte falsa dos concursos da API da Caixa.
    /// </summary>
    private readonly IFonteConcursos fonteConcursos = Substitute.For<IFonteConcursos>();

    /// <summary>
    /// Log falso, para conferir os erros registrados.
    /// </summary>
    private readonly LoggerFalso<Painel> log = new();

    /// <summary>
    /// Prepara os serviços da página sobre as listas em memória.
    /// </summary>
    public PainelTestes()
    {
        this.repositorio.ObterUltimoNumeroAsync(Arg.Any<CancellationToken>())
            .Returns(_ => this.concursos.Count == 0 ? null : this.concursos.Max(concurso => concurso.Numero));
        this.repositorio.ObterAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(chamada => this.concursos.Find(concurso => concurso.Numero == chamada.ArgAt<int>(0)));
        this.repositorio.ContarAsync(Arg.Any<CancellationToken>()).Returns(_ => this.concursos.Count);
        this.repositorio.ListarResultadosAsync(Arg.Any<CancellationToken>())
            .Returns(_ => this.Historico());
        this.repositorio.AdicionarAsync(Arg.Any<IReadOnlyCollection<Concurso>>(), Arg.Any<CancellationToken>())
            .Returns(chamada =>
            {
                this.concursos.AddRange(chamada.ArgAt<IReadOnlyCollection<Concurso>>(0));
                return Task.CompletedTask;
            });
        this.fonteConcursos.ObterConcursoAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(chamada => ConcursoFabrica.Criar(numero: chamada.ArgAt<int>(0)));

        IJogoRepositorio repositorioJogos = Substitute.For<IJogoRepositorio>();
        repositorioJogos.ListarAsync(Arg.Any<CancellationToken>()).Returns(_ => this.jogos.ToList());
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

        var estatisticas = new EstatisticasServico(this.repositorio);
        var servicoJogos = new JogosServico(repositorioJogos, this.repositorio);
        this.Services.AddSingleton(new PainelServico(this.repositorio, estatisticas, servicoJogos));
        this.Services.AddSingleton(servicoJogos);
        this.Services.AddSingleton(new TeimosinhaServico(this.teimosinhas, repositorioJogos, servicoJogos));
        this.Services.AddSingleton(new AtualizacaoConcursosServico(this.fonteConcursos, this.repositorio));
        this.Services.AddSingleton<ILogger<Painel>>(this.log);
    }

    /// <summary>
    /// O cabeçalho tem título, subtítulo e os atalhos "Gerar jogos" e "Novo jogo" (Meus jogos com o cadastro aberto).
    /// </summary>
    [Fact]
    public void CabecalhoComAtalhos()
    {
        IRenderedComponent<Painel> pagina = this.RenderizarPagina();

        Assert.Equal("Painel", Texto(pagina.Find("h1")));
        Assert.Equal("Resumo da base local e dos seus jogos", Texto(pagina.Find(".subtitulo")));
        Assert.Equal("Atualizar resultados", Texto(pagina.Find(".botao-atualizar")));
        IElement novoJogo = pagina.Find(".botao-novo");
        Assert.Equal("Novo jogo", Texto(novoJogo));
        Assert.Equal("meus-jogos?novo=true", novoJogo.GetAttribute("href"));
        IElement gerarJogos = pagina.Find(".botao-gerar");
        Assert.Equal("Gerar jogos", Texto(gerarJogos));
        Assert.Equal("gerar-jogos", gerarJogos.GetAttribute("href"));
    }

    /// <summary>
    /// Com a base vazia, mostra a mensagem com o link para Resultados, sem indicadores nem cartões.
    /// </summary>
    [Fact]
    public void BaseVazia()
    {
        IRenderedComponent<Painel> pagina = this.RenderizarPagina();

        pagina.WaitForAssertion(() => Assert.StartsWith(
            "Nenhum concurso gravado na base local. Use “Importar histórico” na página Resultados para carregar os concursos da Caixa.",
            Texto(pagina.Find(".sem-registros"))));
        Assert.Equal("resultados", pagina.Find(".link-resultados").GetAttribute("href"));
        Assert.Empty(pagina.FindAll(".indicador"));
        Assert.Empty(pagina.FindAll(".ultimo-resultado"));
        Assert.Empty(pagina.FindAll(".jogos-ultimo-concurso"));
        Assert.Single(pagina.FindAll(".botao-atualizar"));
        Assert.Single(pagina.FindAll(".botao-novo"));
    }

    /// <summary>
    /// Indicadores com concursos na base e jogos aguardando um concurso.
    /// </summary>
    [Fact]
    public void Indicadores()
    {
        this.AdicionarConcursos(3808);
        this.jogos.Add(new Jogo(3809, Enumerable.Range(1, 15), DateTime.Now));
        this.jogos.Add(new Jogo(3809, Enumerable.Range(2, 15), DateTime.Now));
        this.jogos.Add(new Jogo(3809, Enumerable.Range(3, 15), DateTime.Now));

        IRenderedComponent<Painel> pagina = this.RenderizarComConteudo();

        AssertIndicador(pagina, ".concursos-na-base", "3.808", "Último sorteio em 17/10/2026");
        AssertIndicador(pagina, ".proximo-concurso", "3809", "Estimativa R$ 1.800.000,00");
        AssertIndicador(pagina, ".jogos-aguardando", "3", "Concurso 3809");
        AssertIndicador(pagina, ".premios-a-receber", "R$ 0,00", "Prazo: 90 dias após o sorteio");
    }

    /// <summary>
    /// Jogos aguardando em vários concursos mostram a quantidade de concursos.
    /// </summary>
    [Fact]
    public void JogosAguardandoEmVariosConcursos()
    {
        this.AdicionarConcursos(3808);
        this.jogos.Add(new Jogo(3809, Enumerable.Range(1, 15), DateTime.Now));
        this.jogos.Add(new Jogo(3809, Enumerable.Range(2, 15), DateTime.Now));
        this.jogos.Add(new Jogo(3810, Enumerable.Range(1, 15), DateTime.Now));

        IRenderedComponent<Painel> pagina = this.RenderizarComConteudo();

        AssertIndicador(pagina, ".jogos-aguardando", "3", "2 concursos");
    }

    /// <summary>
    /// Sem jogos de concursos não sorteados, o indicador mostra zero.
    /// </summary>
    [Fact]
    public void SemJogosAguardando()
    {
        this.AdicionarConcursos(3808);
        this.jogos.Add(new Jogo(3808, Enumerable.Range(1, 15), DateTime.Now));

        IRenderedComponent<Painel> pagina = this.RenderizarComConteudo();

        AssertIndicador(pagina, ".jogos-aguardando", "0", "Nenhum jogo aguardando sorteio");
    }

    /// <summary>
    /// Estimativa zero aparece como não informada.
    /// </summary>
    [Fact]
    public void EstimativaNaoInformada()
    {
        this.AdicionarConcursos(3807);
        this.concursos.Add(ConcursoFabrica.Criar(numero: 3808, estimativaProximo: 0m));

        IRenderedComponent<Painel> pagina = this.RenderizarComConteudo();

        AssertIndicador(pagina, ".proximo-concurso", "3809", "Estimativa não informada");
    }

    /// <summary>
    /// Prêmio de concurso sorteado hoje entra nos prêmios a receber.
    /// </summary>
    [Fact]
    public void PremioDentroDoPrazo()
    {
        this.AdicionarConcursos(3807);
        this.concursos.Add(CriarConcurso(3808, DateOnly.FromDateTime(DateTime.Today)));
        this.jogos.Add(new Jogo(3808, ConcursoFabrica.DezenasPadrao, DateTime.Now));

        IRenderedComponent<Painel> pagina = this.RenderizarComConteudo();

        AssertIndicador(pagina, ".premios-a-receber", "R$ 1.712.408,63", "Prazo: 90 dias após o sorteio");
    }

    /// <summary>
    /// Prêmio de concurso sorteado há mais de 90 dias não entra nos prêmios a receber.
    /// </summary>
    [Fact]
    public void PremioPrescrito()
    {
        this.AdicionarConcursos(3807);
        this.concursos.Add(CriarConcurso(3808, DateOnly.FromDateTime(DateTime.Today).AddDays(-91)));
        this.jogos.Add(new Jogo(3808, ConcursoFabrica.DezenasPadrao, DateTime.Now));

        IRenderedComponent<Painel> pagina = this.RenderizarComConteudo();

        AssertIndicador(pagina, ".premios-a-receber", "R$ 0,00", "Prazo: 90 dias após o sorteio");
    }

    /// <summary>
    /// O último resultado mostra número, data com dia da semana, ganhadores, dezenas e o detalhe do concurso.
    /// </summary>
    [Fact]
    public void UltimoResultadoComGanhadores()
    {
        int[] ordem = [.. ConcursoFabrica.DezenasPadrao.Reverse()];
        this.AdicionarConcursos(3807);
        this.concursos.Add(ConcursoFabrica.Criar(
            numero: 3808,
            ordemSorteio: ordem,
            rateios: ConcursoFabrica.CriarRateios(ganhadores15: 2, premio15: 856_204.31m)));

        IRenderedComponent<Painel> pagina = this.RenderizarComConteudo();

        IElement cartao = pagina.Find(".ultimo-resultado");
        Assert.Equal("Último resultado · concurso 3808", Texto(cartao.QuerySelector(".titulo-ultimo")!));
        Assert.Equal("sábado, 17/10/2026", Texto(cartao.QuerySelector(".data-sorteio")!));
        Assert.Equal("2 ganhadores", Texto(cartao.QuerySelector(".ganhadores")!));
        Assert.Equal(
            ConcursoFabrica.DezenasPadrao.Select(dezena => dezena.ToString("00")),
            cartao.QuerySelectorAll(".dezenas-sorteadas .bola").Select(Texto));
        Assert.Equal(5, cartao.QuerySelectorAll(".rateio tbody tr").Length);
        Assert.Equal(
            ordem.Select(dezena => dezena.ToString("00")),
            cartao.QuerySelectorAll(".ordem-sorteio .bola").Select(Texto));
    }

    /// <summary>
    /// Último concurso sem ganhador de 15 acertos mostra "Acumulou" e, sem ordem do sorteio, não a exibe.
    /// </summary>
    [Fact]
    public void UltimoResultadoAcumulado()
    {
        this.AdicionarConcursos(3807);
        this.concursos.Add(ConcursoFabrica.Criar(numero: 3808, rateios: ConcursoFabrica.CriarRateios(ganhadores15: 0, premio15: 0m)));

        IRenderedComponent<Painel> pagina = this.RenderizarComConteudo();

        IElement cartao = pagina.Find(".ultimo-resultado");
        Assert.Equal("Acumulou", Texto(cartao.QuerySelector(".ganhadores")!));
        Assert.Empty(cartao.QuerySelectorAll(".ordem-sorteio"));
    }

    /// <summary>
    /// Os destaques são os rankings de todo o histórico, iguais aos das Estatísticas, com link para elas.
    /// </summary>
    [Fact]
    public void DestaquesIguaisAsEstatisticas()
    {
        this.AdicionarConcursos(3808);
        var esperadas = EstatisticasDezenas.Calcular(this.Historico(), null);

        IRenderedComponent<Painel> pagina = this.RenderizarComConteudo();

        Assert.Equal("Mais sorteadas", Texto(pagina.Find(".mais-sorteadas .mud-typography")));
        Assert.Equal(DezenasDoRanking(esperadas.MaisSorteadas), DezenasExibidas(pagina, ".mais-sorteadas"));
        Assert.Equal("Há mais tempo sem sair", Texto(pagina.Find(".mais-atrasadas .mud-typography")));
        Assert.Equal(DezenasDoRanking(esperadas.MaisAtrasadas), DezenasExibidas(pagina, ".mais-atrasadas"));
        Assert.Empty(pagina.FindAll(".menos-sorteadas"));
        Assert.Equal("estatisticas", pagina.Find(".link-estatisticas").GetAttribute("href"));
    }

    /// <summary>
    /// Os jogos do último concurso aparecem conferidos, na ordem de cadastro, com link para Meus jogos.
    /// </summary>
    [Fact]
    public void JogosDoUltimoConcursoConferidos()
    {
        this.AdicionarConcursos(3808);
        this.jogos.Add(new Jogo(3808, TrezeAcertos, DateTime.Now));
        this.jogos.Add(new Jogo(3808, NoveAcertos, DateTime.Now));
        this.jogos.Add(new Jogo(3809, Enumerable.Range(1, 15), DateTime.Now));

        IRenderedComponent<Painel> pagina = this.RenderizarComConteudo();

        IElement cartao = pagina.Find(".jogos-ultimo-concurso");
        Assert.Equal("Seus jogos no concurso 3808", Texto(cartao.QuerySelector(".titulo-jogos")!));
        IHtmlCollection<IElement> linhas = cartao.QuerySelectorAll(".linha-jogo");
        Assert.Equal(2, linhas.Length);
        Assert.Equal("01", Texto(linhas[0].QuerySelector(".numero-jogo")!));
        Assert.Equal("13 acertos", Texto(linhas[0].QuerySelector(".acertos")!));
        Assert.Equal("R$ 35,00", Texto(linhas[0].QuerySelector(".premio")!));
        Assert.Equal(13, linhas[0].QuerySelectorAll(".bola.acerto").Length);
        Assert.Equal("02", Texto(linhas[1].QuerySelector(".numero-jogo")!));
        Assert.Equal("9 acertos", Texto(linhas[1].QuerySelector(".acertos")!));
        Assert.Equal("—", Texto(linhas[1].QuerySelector(".premio")!));
        Assert.Equal("meus-jogos", cartao.QuerySelector(".link-meus-jogos")!.GetAttribute("href"));
    }

    /// <summary>
    /// Aposta múltipla no último concurso mostra a quantidade de dezenas.
    /// </summary>
    [Fact]
    public void ApostaMultiplaNoUltimoConcurso()
    {
        this.AdicionarConcursos(3808);
        this.jogos.Add(new Jogo(3808, Enumerable.Range(1, 16), DateTime.Now));

        IRenderedComponent<Painel> pagina = this.RenderizarComConteudo();

        Assert.Equal("16 dezenas", Texto(pagina.Find(".jogos-ultimo-concurso .quantidade-dezenas")));
    }

    /// <summary>
    /// Sem jogos no último concurso, o cartão avisa.
    /// </summary>
    [Fact]
    public void NenhumJogoNoUltimoConcurso()
    {
        this.AdicionarConcursos(3808);
        this.jogos.Add(new Jogo(3809, Enumerable.Range(1, 15), DateTime.Now));

        IRenderedComponent<Painel> pagina = this.RenderizarComConteudo();

        Assert.Equal("Nenhum jogo cadastrado para o concurso 3808.", Texto(pagina.Find(".sem-jogos")));
        Assert.Empty(pagina.FindAll(".jogos-ultimo-concurso .linha-jogo"));
    }

    /// <summary>
    /// Atualização com concurso novo informa o desfecho e recarrega o Painel.
    /// </summary>
    [Fact]
    public void AtualizacaoComConcursoNovoRecarrega()
    {
        this.AdicionarConcursos(3808);
        this.fonteConcursos.ObterNumeroUltimoConcursoAsync(Arg.Any<CancellationToken>()).Returns(3809);
        IRenderedComponent<MudSnackbarProvider> mensagens = this.Render<MudSnackbarProvider>();
        IRenderedComponent<Painel> pagina = this.RenderizarComConteudo();

        pagina.Find(".botao-atualizar").Click();

        mensagens.WaitForAssertion(() => Assert.Contains(
            "Atualização concluída: 1 concurso novo gravado (3809).",
            MensagensExibidas(mensagens)));
        pagina.WaitForAssertion(() => Assert.Equal(
            "Último resultado · concurso 3809",
            Texto(pagina.Find(".titulo-ultimo"))));
        AssertIndicador(pagina, ".proximo-concurso", "3810", "Estimativa R$ 1.800.000,00");
        AssertIndicador(pagina, ".concursos-na-base", "3.809", "Último sorteio em 17/10/2026");
    }

    /// <summary>
    /// Atualização pelo Painel cria os jogos da Teimosinha para o próximo concurso e avisa.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task AtualizacaoCriaJogosDaTeimosinha()
    {
        this.AdicionarConcursos(3808);
        var teimosinha = new Teimosinha("Fixas", 3809);
        teimosinha.Acrescentar(NoveAcertos, 3809);
        teimosinha.Acrescentar(Enumerable.Range(11, 15), 3809);
        await this.teimosinhas.SalvarAsync(teimosinha, Xunit.TestContext.Current.CancellationToken);
        this.fonteConcursos.ObterNumeroUltimoConcursoAsync(Arg.Any<CancellationToken>()).Returns(3809);
        IRenderedComponent<MudSnackbarProvider> mensagens = this.Render<MudSnackbarProvider>();
        IRenderedComponent<Painel> pagina = this.RenderizarComConteudo();

        pagina.Find(".botao-atualizar").Click();

        mensagens.WaitForAssertion(() => Assert.Contains(
            "Teimosinhas: 2 jogos criados para o concurso 3810.",
            MensagensExibidas(mensagens)));
        Assert.Equal(2, this.jogos.Count(jogo => jogo.NumeroConcurso == 3810 && jogo.TeimosinhaId == teimosinha.Id));
    }

    /// <summary>
    /// Atualização pelo Painel com jogo premiado no concurso novo abre o diálogo "Jogos premiados"; o jogo premiado
    /// do 3808, que já estava na base, não entra.
    /// </summary>
    [Fact]
    public void AtualizacaoComJogoPremiadoAbreODialogo()
    {
        this.AdicionarConcursos(3808);
        this.jogos.Add(new Jogo(3808, TrezeAcertos, DateTime.Now));
        this.jogos.Add(new Jogo(3809, NoveAcertos, DateTime.Now));
        this.jogos.Add(new Jogo(3809, TrezeAcertos, DateTime.Now));
        this.fonteConcursos.ObterNumeroUltimoConcursoAsync(Arg.Any<CancellationToken>()).Returns(3809);
        IRenderedComponent<MudSnackbarProvider> mensagens = this.Render<MudSnackbarProvider>();
        IRenderedComponent<MudDialogProvider> dialogos = this.Render<MudDialogProvider>();
        IRenderedComponent<Painel> pagina = this.RenderizarComConteudo();

        pagina.Find(".botao-atualizar").Click();

        dialogos.WaitForAssertion(() => Assert.NotEmpty(dialogos.FindAll(".resumo-premios")));
        Assert.Contains("Atualização concluída: 1 concurso novo gravado (3809).", MensagensExibidas(mensagens));
        Assert.Equal("1 de 2 jogos conferidos foi premiado.", Texto(dialogos.Find(".resumo-premios")));
        Assert.Equal("Jogo 02 · 13 acertos · R$ 35,00", Texto(dialogos.Find(".jogo-premiado")));
        Assert.Equal("Total: 1 jogo · R$ 35,00", Texto(dialogos.Find(".total-premios")));
    }

    /// <summary>
    /// Atualização sem concursos novos informa que a base já está atualizada.
    /// </summary>
    [Fact]
    public void AtualizacaoComBaseJaAtualizada()
    {
        this.AdicionarConcursos(3808);
        this.fonteConcursos.ObterNumeroUltimoConcursoAsync(Arg.Any<CancellationToken>()).Returns(3808);
        IRenderedComponent<MudSnackbarProvider> mensagens = this.Render<MudSnackbarProvider>();
        IRenderedComponent<Painel> pagina = this.RenderizarComConteudo();

        pagina.Find(".botao-atualizar").Click();

        mensagens.WaitForAssertion(() => Assert.Contains(
            "A base local já está atualizada: último concurso 3808.",
            MensagensExibidas(mensagens)));
    }

    /// <summary>
    /// Durante a atualização, o andamento é exibido e o botão fica desabilitado.
    /// </summary>
    [Fact]
    public void AtualizacaoEmAndamento()
    {
        this.AdicionarConcursos(3808);
        this.fonteConcursos.ObterNumeroUltimoConcursoAsync(Arg.Any<CancellationToken>()).Returns(3811);
        var busca3810 = new TaskCompletionSource<Concurso>();
        this.fonteConcursos.ObterConcursoAsync(3810, Arg.Any<CancellationToken>()).Returns(busca3810.Task);
        IRenderedComponent<Painel> pagina = this.RenderizarComConteudo();

        pagina.Find(".botao-atualizar").Click();

        pagina.WaitForAssertion(() => Assert.Equal("Buscando o concurso 3810 (2 de 3)…", Texto(pagina.Find(".andamento"))));
        Assert.True(pagina.Find(".botao-atualizar").HasAttribute("disabled"));

        busca3810.SetResult(ConcursoFabrica.Criar(numero: 3810));

        pagina.WaitForAssertion(() => Assert.Empty(pagina.FindAll(".andamento")));
        Assert.False(pagina.Find(".botao-atualizar").HasAttribute("disabled"));
    }

    /// <summary>
    /// Falha não prevista na atualização é informada e registrada no log.
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
        IRenderedComponent<Painel> pagina = this.RenderizarComConteudo();

        pagina.Find(".botao-atualizar").Click();

        mensagens.WaitForAssertion(() => Assert.Contains(
            "A atualização foi interrompida. Erro inesperado; os detalhes foram registrados no log.",
            MensagensExibidas(mensagens)));
        Assert.Contains((LogLevel.Error, (Exception?)falha), this.log.Registros);
    }

    /// <summary>
    /// Confere o valor e o detalhe de um indicador.
    /// </summary>
    /// <param name="pagina">Página renderizada.</param>
    /// <param name="seletor">Classe do indicador.</param>
    /// <param name="valor">Valor esperado.</param>
    /// <param name="detalhe">Detalhe esperado.</param>
    private static void AssertIndicador(IRenderedComponent<Painel> pagina, string seletor, string valor, string detalhe)
    {
        IElement indicador = pagina.Find($".indicador{seletor}");
        Assert.Equal(valor, Texto(indicador.QuerySelector(".valor")!));
        Assert.Equal(detalhe, Texto(indicador.QuerySelector(".detalhe")!));
    }

    /// <summary>
    /// Dezenas de um ranking no formato exibido.
    /// </summary>
    /// <param name="ranking">Ranking calculado.</param>
    /// <returns>Dezenas com dois dígitos.</returns>
    private static List<string> DezenasDoRanking(Ranking ranking)
    {
        return ranking.Itens.Select(item => item.Dezena.ToString("00")).ToList();
    }

    /// <summary>
    /// Dezenas exibidas num ranking da página.
    /// </summary>
    /// <param name="pagina">Página renderizada.</param>
    /// <param name="classe">Classe do ranking.</param>
    /// <returns>Textos das dezenas.</returns>
    private static List<string> DezenasExibidas(IRenderedComponent<Painel> pagina, string classe)
    {
        return pagina.FindAll($"{classe} .bola").Select(Texto).ToList();
    }

    /// <summary>
    /// Mensagens rápidas exibidas.
    /// </summary>
    /// <param name="mensagens">Provedor de mensagens renderizado.</param>
    /// <returns>Textos das mensagens.</returns>
    private static List<string> MensagensExibidas(IRenderedComponent<MudSnackbarProvider> mensagens)
    {
        return mensagens.FindAll(".mud-snackbar-content-message").Select(Texto).ToList();
    }

    /// <summary>
    /// Texto de um elemento, com espaços não separáveis trocados por espaços comuns.
    /// </summary>
    /// <param name="elemento">Elemento da página.</param>
    /// <returns>Texto sem espaços nas pontas.</returns>
    private static string Texto(IElement elemento)
    {
        return elemento.TextContent.Replace(' ', ' ').Trim();
    }

    /// <summary>
    /// Cria um concurso com o resultado padrão sorteado na data informada.
    /// </summary>
    /// <param name="numero">Número do concurso.</param>
    /// <param name="data">Data do sorteio.</param>
    /// <returns>O concurso.</returns>
    private static Concurso CriarConcurso(int numero, DateOnly data)
    {
        return new Concurso(
            numero,
            data,
            ConcursoFabrica.DezenasPadrao,
            null,
            ConcursoFabrica.CriarRateios(),
            0m,
            0m,
            1_800_000m,
            0m);
    }

    /// <summary>
    /// Grava os concursos de 1 até o último, alternando dois resultados para variar os rankings.
    /// </summary>
    /// <param name="ultimo">Número do último concurso.</param>
    private void AdicionarConcursos(int ultimo)
    {
        this.concursos.AddRange(Enumerable.Range(1, ultimo).Select(numero => ConcursoFabrica.Criar(
            numero: numero,
            dezenas: numero % 3 == 0 && numero != ultimo ? Enumerable.Range(1, 15) : null)));
    }

    /// <summary>
    /// Resultados gravados, como o repositório devolve para as estatísticas.
    /// </summary>
    /// <returns>Número e dezenas de cada concurso.</returns>
    private List<ResultadoSorteio> Historico()
    {
        return this.concursos
            .OrderBy(concurso => concurso.Numero)
            .Select(concurso => new ResultadoSorteio(concurso.Numero, concurso.Dezenas))
            .ToList();
    }

    /// <summary>
    /// Renderiza a página na cultura da aplicação.
    /// </summary>
    /// <returns>Página renderizada.</returns>
    private IRenderedComponent<Painel> RenderizarPagina()
    {
        CultureInfo.CurrentCulture = new CultureInfo("pt-BR");
        CultureInfo.CurrentUICulture = new CultureInfo("pt-BR");

        return this.Render<Painel>();
    }

    /// <summary>
    /// Renderiza a página e espera os indicadores aparecerem.
    /// </summary>
    /// <returns>Página renderizada.</returns>
    private IRenderedComponent<Painel> RenderizarComConteudo()
    {
        IRenderedComponent<Painel> pagina = this.RenderizarPagina();
        pagina.WaitForAssertion(() => Assert.NotEmpty(pagina.FindAll(".indicador")));

        return pagina;
    }
}
