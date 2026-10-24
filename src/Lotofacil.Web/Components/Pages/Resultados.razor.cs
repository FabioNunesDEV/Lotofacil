// <copyright file="Resultados.razor.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Aplicacao.Concursos;
using Lotofacil.Aplicacao.Jogos;
using Lotofacil.Dominio;
using Lotofacil.Dominio.Concursos;
using Lotofacil.Web.Components.Concursos;
using Lotofacil.Web.Components.Jogos;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Lotofacil.Web.Components.Pages;

/// <summary>
/// Lista os concursos gravados, com paginação, busca por número e detalhe do rateio, e dispara a importação, a
/// atualização e o preenchimento da ordem do sorteio.
/// </summary>
public partial class Resultados : IDisposable
{
    /// <summary>
    /// Mensagem exibida quando a base não tem concursos.
    /// </summary>
    private const string MensagemBaseVazia =
        "Nenhum concurso gravado na base local. Use “Importar histórico” para carregar os concursos da Caixa.";

    /// <summary>
    /// Início da mensagem de falha da importação.
    /// </summary>
    private const string PrefixoFalhaImportacao = "A importação falhou e nada foi gravado.";

    /// <summary>
    /// Quantidades de concursos por página oferecidas ao usuário.
    /// </summary>
    private static readonly int[] OpcoesTamanhoPagina = [10, 25, 50];

    /// <summary>
    /// Números dos concursos com o detalhe aberto.
    /// </summary>
    private readonly HashSet<int> expandidos = [];

    /// <summary>
    /// Tabela de concursos, usada para recarregar os dados após a busca.
    /// </summary>
    private MudTable<Concurso> tabela = null!;

    /// <summary>
    /// Número buscado, ou <c>null</c> para listar todos.
    /// </summary>
    private int? numeroBusca;

    /// <summary>
    /// Mensagem exibida quando a tabela fica sem linhas.
    /// </summary>
    private string mensagemSemRegistros = MensagemBaseVazia;

    /// <summary>
    /// Texto do andamento da importação, da atualização ou do preenchimento da ordem, ou <c>null</c> quando nenhum
    /// está em andamento.
    /// </summary>
    private string? andamento;

    /// <summary>
    /// Quantidade de concursos sem a ordem do sorteio.
    /// </summary>
    private int semOrdem;

    /// <summary>
    /// Cancelamento do preenchimento da ordem em andamento, ou <c>null</c> quando ele não está rodando.
    /// </summary>
    private CancellationTokenSource? preenchimento;

    /// <summary>
    /// Indica que o usuário saiu da página; o preenchimento interrompido não mexe mais na tela.
    /// </summary>
    private bool descartada;

    /// <summary>
    /// Serviço de consulta dos concursos.
    /// </summary>
    [Inject]
    private ConsultaConcursosServico Servico { get; set; } = null!;

    /// <summary>
    /// Serviço de importação do histórico da Caixa.
    /// </summary>
    [Inject]
    private ImportacaoHistoricoServico Importacao { get; set; } = null!;

    /// <summary>
    /// Serviço de atualização dos concursos pela API da Caixa.
    /// </summary>
    [Inject]
    private AtualizacaoConcursosServico Atualizacao { get; set; } = null!;

    /// <summary>
    /// Serviço de preenchimento da ordem do sorteio pela API da Caixa.
    /// </summary>
    [Inject]
    private OrdemSorteioServico Ordem { get; set; } = null!;

    /// <summary>
    /// Serviço dos jogos, que confere os jogos dos concursos novos.
    /// </summary>
    [Inject]
    private JogosServico Jogos { get; set; } = null!;

    /// <summary>
    /// Serviço de diálogos, usado no aviso de jogos premiados.
    /// </summary>
    [Inject]
    private IDialogService Dialogos { get; set; } = null!;

    /// <summary>
    /// Mensagens rápidas exibidas ao usuário.
    /// </summary>
    [Inject]
    private ISnackbar Mensagens { get; set; } = null!;

    /// <summary>
    /// Log de erros da aplicação.
    /// </summary>
    [Inject]
    private ILogger<Resultados> Log { get; set; } = null!;

    /// <summary>
    /// Indica que a importação, a atualização ou o preenchimento está em andamento; os três botões ficam desabilitados.
    /// </summary>
    private bool Ocupada => this.andamento is not null;

    /// <summary>
    /// Interrompe o preenchimento da ordem ao sair da página.
    /// </summary>
    public void Dispose()
    {
        this.descartada = true;
        this.preenchimento?.Cancel();
    }

    /// <summary>
    /// Conta os concursos sem a ordem do sorteio, que habilitam o preenchimento.
    /// </summary>
    /// <returns>Tarefa da contagem.</returns>
    protected override async Task OnInitializedAsync()
    {
        await this.ContarSemOrdemAsync();
    }

    /// <summary>
    /// Devolve o rateio da faixa de 15 acertos.
    /// </summary>
    /// <param name="concurso">Concurso da linha.</param>
    /// <returns>Rateio de 15 acertos.</returns>
    private static Rateio Rateio15(Concurso concurso)
    {
        return concurso.ObterRateio(RegrasLotofacil.MaiorFaixaPremiada);
    }

    /// <summary>
    /// Carrega a página pedida pela tabela, ou somente o concurso buscado.
    /// </summary>
    /// <param name="estado">Página e tamanho de página pedidos pela tabela.</param>
    /// <param name="cancelamento">Sinal de cancelamento.</param>
    /// <returns>Concursos a exibir e total para o paginador.</returns>
    private async Task<TableData<Concurso>> CarregarAsync(TableState estado, CancellationToken cancelamento)
    {
        if (this.numeroBusca is int numero)
        {
            this.mensagemSemRegistros = $"Concurso {numero} não encontrado na base local";
            Concurso? concurso = await this.Servico.ObterAsync(numero, cancelamento);
            List<Concurso> encontrados = concurso is null ? [] : [concurso];

            return new TableData<Concurso> { Items = encontrados, TotalItems = encontrados.Count };
        }

        this.mensagemSemRegistros = MensagemBaseVazia;
        PaginaConcursos pagina = await this.Servico.ListarPaginaAsync(estado.Page, estado.PageSize, cancelamento);

        return new TableData<Concurso> { Items = pagina.Itens, TotalItems = pagina.Total };
    }

    /// <summary>
    /// Aplica ou limpa a busca e volta a tabela para a primeira página.
    /// </summary>
    /// <param name="numero">Número buscado, ou <c>null</c> quando o campo é limpo.</param>
    /// <returns>Tarefa da recarga.</returns>
    private async Task BuscarAsync(int? numero)
    {
        this.numeroBusca = numero;
        this.expandidos.Clear();

        // Trocar de página já recarrega os dados; na primeira página é preciso pedir a recarga.
        if (this.tabela.CurrentPage > 0)
        {
            this.tabela.NavigateTo(0);
        }
        else
        {
            await this.tabela.ReloadServerData();
        }
    }

    /// <summary>
    /// Importa o histórico da Caixa, informa o resultado e, se houver concursos novos, volta ao mais recente.
    /// </summary>
    /// <returns>Tarefa da importação.</returns>
    private async Task ImportarHistoricoAsync()
    {
        this.andamento = "Importando o histórico da Caixa…";
        try
        {
            ResultadoImportacao resultado = await this.Importacao.ImportarAsync();
            if (resultado.ConcursosGravados == 0)
            {
                this.Mensagens.Add(
                    $"A base local já tem todos os {resultado.ConcursosNoArquivo:N0} concursos do arquivo; nada foi gravado.",
                    Severity.Info);
            }
            else
            {
                this.Mensagens.Add(
                    $"Importação concluída: {resultado.ConcursosNoArquivo:N0} concursos no arquivo, "
                        + $"{resultado.ConcursosGravados:N0} gravados na base local.",
                    Severity.Success);
                await this.BuscarAsync(null);
            }
        }
        catch (FonteResultadosException erro)
        {
            this.Log.LogError(erro, "Falha na importação do histórico: {Mensagem}", erro.Message);
            this.MostrarFalha($"{PrefixoFalhaImportacao} {erro.Message}");
        }
        catch (Exception erro) when (erro is not OperationCanceledException)
        {
            this.Log.LogError(erro, "Falha inesperada na importação do histórico.");
            this.MostrarFalha($"{PrefixoFalhaImportacao} {AvisosAtualizacao.MotivoErroInesperado}");
        }
        finally
        {
            this.andamento = null;
            await this.ContarSemOrdemAsync();
        }
    }

    /// <summary>
    /// Busca na Caixa os concursos novos, mostrando o andamento, e informa o desfecho.
    /// </summary>
    /// <returns>Tarefa da atualização.</returns>
    private async Task AtualizarResultadosAsync()
    {
        this.andamento = AvisosAtualizacao.Consultando;
        try
        {
            var progresso = new Progress<AndamentoAtualizacao>(atual => this.MostrarAndamento(AvisosAtualizacao.Andamento(atual)));
            ResultadoAtualizacao resultado = await this.Atualizacao.AtualizarAsync(progresso);
            this.InformarAtualizacao(resultado);

            if (resultado.ConcursosGravados > 0)
            {
                await this.BuscarAsync(null);
                await this.AvisarJogosPremiadosAsync(resultado);
            }
        }
        catch (Exception erro) when (erro is not OperationCanceledException)
        {
            this.Log.LogError(erro, "Falha inesperada na atualização dos resultados.");
            this.MostrarFalha(AvisosAtualizacao.FalhaInesperada);
        }
        finally
        {
            this.andamento = null;
            await this.ContarSemOrdemAsync();
        }
    }

    /// <summary>
    /// Busca na Caixa a ordem do sorteio dos concursos sem ela, mostrando o andamento, e informa o desfecho.
    /// </summary>
    /// <returns>Tarefa do preenchimento.</returns>
    private async Task PreencherOrdemAsync()
    {
        using var cancelamento = new CancellationTokenSource();
        this.preenchimento = cancelamento;
        this.andamento = AvisosOrdemSorteio.Preparando;
        try
        {
            var progresso = new Progress<AndamentoOrdemSorteio>(
                atual => this.MostrarAndamento(AvisosOrdemSorteio.Andamento(atual, DateTime.Now)));
            ResultadoPreenchimentoOrdem resultado = await this.Ordem.PreencherAsync(progresso, cancelamento.Token);
            await this.InformarPreenchimentoAsync(resultado);
        }
        catch (OperationCanceledException) when (cancelamento.IsCancellationRequested)
        {
            // Interrompido antes de começar a busca: nada foi gravado e não há o que informar.
        }
        catch (Exception erro) when (erro is not OperationCanceledException)
        {
            this.Log.LogError(erro, "Falha inesperada no preenchimento da ordem do sorteio.");
            if (!this.descartada)
            {
                this.MostrarFalha(AvisosOrdemSorteio.FalhaInesperada);
            }
        }
        finally
        {
            this.preenchimento = null;
            this.andamento = null;
            await this.ContarSemOrdemAsync();
        }
    }

    /// <summary>
    /// Registra no log as falhas do preenchimento, mostra o desfecho e recarrega a lista se alguma ordem foi gravada.
    /// </summary>
    /// <param name="resultado">Desfecho do preenchimento.</param>
    /// <returns>Tarefa da recarga.</returns>
    private async Task InformarPreenchimentoAsync(ResultadoPreenchimentoOrdem resultado)
    {
        foreach (FalhaOrdemSorteio falha in resultado.Falhas)
        {
            this.Log.LogError(falha.Causa, "Ordem do sorteio não preenchida: {Mensagem}", falha.Mensagem);
        }

        // Depois de sair da página, a tabela já foi descartada e não há onde mostrar o desfecho.
        if (this.descartada)
        {
            return;
        }

        AvisoAtualizacao aviso = AvisosOrdemSorteio.Desfecho(resultado);
        if (aviso.Severidade == Severity.Error)
        {
            this.MostrarFalha(aviso.Texto);
        }
        else
        {
            this.Mensagens.Add(aviso.Texto, aviso.Severidade);
        }

        if (resultado.Preenchidos > 0)
        {
            await this.BuscarAsync(null);
        }
    }

    /// <summary>
    /// Pede a interrupção do preenchimento da ordem; ele para antes de buscar o próximo concurso.
    /// </summary>
    private void Interromper()
    {
        this.preenchimento?.Cancel();
    }

    /// <summary>
    /// Atualiza a quantidade de concursos sem a ordem do sorteio.
    /// </summary>
    /// <returns>Tarefa da contagem.</returns>
    private async Task ContarSemOrdemAsync()
    {
        this.semOrdem = await this.Ordem.ContarSemOrdemAsync();
    }

    /// <summary>
    /// Confere os jogos dos concursos gravados pela atualização e avisa os prêmios.
    /// </summary>
    /// <param name="resultado">Desfecho da atualização, com algum concurso gravado.</param>
    /// <returns>Tarefa do aviso.</returns>
    private async Task AvisarJogosPremiadosAsync(ResultadoAtualizacao resultado)
    {
        ConferenciaConcursos conferencia =
            await this.Jogos.ConferirConcursosAsync(resultado.PrimeiroGravado, resultado.UltimoNumeroNaBase);
        await AvisoJogosPremiados.MostrarAsync(
            this.Dialogos,
            this.Mensagens,
            conferencia,
            DateOnly.FromDateTime(DateTime.Today));
    }

    /// <summary>
    /// Mostra qual concurso está sendo buscado.
    /// </summary>
    /// <param name="texto">Texto do andamento.</param>
    private void MostrarAndamento(string texto)
    {
        _ = this.InvokeAsync(() =>
        {
            // O aviso chega de forma assíncrona; a operação pode já ter terminado.
            if (this.andamento is null || this.descartada)
            {
                return;
            }

            this.andamento = texto;
            this.StateHasChanged();
        });
    }

    /// <summary>
    /// Mostra a mensagem do desfecho da atualização e registra no log a falha, se houver.
    /// </summary>
    /// <param name="resultado">Desfecho da atualização.</param>
    private void InformarAtualizacao(ResultadoAtualizacao resultado)
    {
        AvisoAtualizacao aviso = AvisosAtualizacao.Desfecho(resultado);
        if (resultado.Falha is FonteResultadosException falha)
        {
            this.Log.LogError(falha, "Atualização dos resultados interrompida: {Mensagem}", falha.Message);
        }

        if (aviso.Severidade == Severity.Error)
        {
            this.MostrarFalha(aviso.Texto);
        }
        else
        {
            this.Mensagens.Add(aviso.Texto, aviso.Severidade);
        }
    }

    /// <summary>
    /// Mostra uma falha numa mensagem que só some quando o usuário fecha.
    /// </summary>
    /// <param name="mensagem">Mensagem completa da falha.</param>
    private void MostrarFalha(string mensagem)
    {
        this.Mensagens.Add(mensagem, Severity.Error, opcoes => opcoes.RequireInteraction = true);
    }

    /// <summary>
    /// Abre o detalhe do concurso clicado, ou fecha se já estiver aberto.
    /// </summary>
    /// <param name="clique">Dados do clique na linha.</param>
    private void AlternarDetalhe(TableRowClickEventArgs<Concurso> clique)
    {
        if (clique.Item is null)
        {
            return;
        }

        if (!this.expandidos.Remove(clique.Item.Numero))
        {
            this.expandidos.Add(clique.Item.Numero);
        }
    }
}
