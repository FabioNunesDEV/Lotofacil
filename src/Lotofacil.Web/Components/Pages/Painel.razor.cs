// <copyright file="Painel.razor.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Aplicacao.Concursos;
using Lotofacil.Aplicacao.Jogos;
using Lotofacil.Aplicacao.Painel;
using Lotofacil.Aplicacao.Teimosinhas;
using Lotofacil.Dominio;
using Lotofacil.Dominio.Concursos;
using Lotofacil.Web.Components.Concursos;
using Lotofacil.Web.Components.Jogos;
using Lotofacil.Web.Components.Teimosinhas;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Lotofacil.Web.Components.Pages;

/// <summary>
/// Página inicial com o resumo da base local e dos jogos, e os atalhos para atualizar e cadastrar.
/// </summary>
public partial class Painel
{
    /// <summary>
    /// Mensagem exibida quando a base não tem concursos.
    /// </summary>
    private const string MensagemBaseVazia =
        "Nenhum concurso gravado na base local. Use “Importar histórico” na página Resultados para carregar os concursos da Caixa.";

    /// <summary>
    /// Resumo exibido, ou <c>null</c> com a base vazia ou antes da primeira carga.
    /// </summary>
    private ResumoPainel? resumo;

    /// <summary>
    /// Indica que a primeira carga terminou.
    /// </summary>
    private bool carregado;

    /// <summary>
    /// Texto do andamento da atualização, ou <c>null</c> quando ela não está em andamento.
    /// </summary>
    private string? andamento;

    /// <summary>
    /// Serviço do resumo do Painel.
    /// </summary>
    [Inject]
    private PainelServico Servico { get; set; } = null!;

    /// <summary>
    /// Serviço de atualização dos concursos pela API da Caixa.
    /// </summary>
    [Inject]
    private AtualizacaoConcursosServico Atualizacao { get; set; } = null!;

    /// <summary>
    /// Serviço dos jogos, que confere os jogos dos concursos novos.
    /// </summary>
    [Inject]
    private JogosServico Jogos { get; set; } = null!;

    /// <summary>
    /// Serviço das Teimosinhas, que cria os jogos delas depois da atualização.
    /// </summary>
    [Inject]
    private TeimosinhaServico Teimosinhas { get; set; } = null!;

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
    private ILogger<Painel> Log { get; set; } = null!;

    /// <summary>
    /// Indica que a atualização está em andamento; o botão fica desabilitado.
    /// </summary>
    private bool Ocupada => this.andamento is not null;

    /// <summary>
    /// Rateio de 15 acertos do último concurso.
    /// </summary>
    private Rateio Rateio15 => this.resumo!.UltimoConcurso.ObterRateio(RegrasLotofacil.MaiorFaixaPremiada);

    /// <summary>
    /// Carrega o resumo ao abrir a página.
    /// </summary>
    /// <returns>Tarefa da carga.</returns>
    protected override async Task OnInitializedAsync()
    {
        await this.CarregarAsync();
        this.carregado = true;
    }

    /// <summary>
    /// Recarrega o resumo do serviço, com o prazo dos prêmios contado a partir de hoje.
    /// </summary>
    /// <returns>Tarefa da carga.</returns>
    private async Task CarregarAsync()
    {
        this.resumo = await this.Servico.ObterResumoAsync(DateOnly.FromDateTime(DateTime.Today));
    }

    /// <summary>
    /// Busca na Caixa os concursos novos, mostrando o andamento, informa o desfecho e recarrega o resumo.
    /// </summary>
    /// <returns>Tarefa da atualização.</returns>
    private async Task AtualizarResultadosAsync()
    {
        this.andamento = AvisosAtualizacao.Consultando;
        try
        {
            var progresso = new Progress<AndamentoAtualizacao>(this.MostrarAndamento);
            ResultadoAtualizacao resultado = await this.Atualizacao.AtualizarAsync(progresso);
            this.InformarAtualizacao(resultado);

            if (resultado.ConcursosGravados > 0)
            {
                // Os jogos das Teimosinhas vêm antes do aviso, para os criados em concursos já sorteados entrarem nele.
                await AvisoTeimosinhas.CriarJogosAsync(this.Teimosinhas, this.Mensagens);
                await this.CarregarAsync();
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
        }
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
    /// <param name="atual">Concurso sendo buscado e sua posição.</param>
    private void MostrarAndamento(AndamentoAtualizacao atual)
    {
        _ = this.InvokeAsync(() =>
        {
            // O aviso chega de forma assíncrona; a atualização pode já ter terminado.
            if (this.andamento is null)
            {
                return;
            }

            this.andamento = AvisosAtualizacao.Andamento(atual);
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
}
