// <copyright file="MeusJogos.razor.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Aplicacao.Jogos;
using Lotofacil.Web.Components.Concursos;
using Lotofacil.Web.Components.Jogos;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Lotofacil.Web.Components.Pages;

/// <summary>
/// Jogos do usuário agrupados por concurso, com cadastro, conferência, filtros, indicadores e exclusão.
/// </summary>
public partial class MeusJogos
{
    /// <summary>
    /// Mensagem exibida quando não há jogos cadastrados.
    /// </summary>
    private const string MensagemSemJogos = "Nenhum jogo cadastrado. Use “Novo jogo” para cadastrar o primeiro.";

    /// <summary>
    /// Grupos de jogos, do concurso de maior número para o menor.
    /// </summary>
    private IReadOnlyList<GrupoJogos> grupos = [];

    /// <summary>
    /// Filtro selecionado.
    /// </summary>
    private FiltroJogos filtro = FiltroJogos.Todos;

    /// <summary>
    /// Indica que a primeira carga terminou.
    /// </summary>
    private bool carregado;

    /// <summary>
    /// Filtros da lista de jogos.
    /// </summary>
    public enum FiltroJogos
    {
        /// <summary>
        /// Todos os grupos.
        /// </summary>
        Todos,

        /// <summary>
        /// Grupos de concursos ainda não sorteados.
        /// </summary>
        Aguardando,

        /// <summary>
        /// Grupos de concursos sorteados.
        /// </summary>
        Conferidos,

        /// <summary>
        /// Grupos com algum jogo premiado.
        /// </summary>
        Premiados,
    }

    /// <summary>
    /// Serviço de jogos.
    /// </summary>
    [Inject]
    private JogosServico Servico { get; set; } = null!;

    /// <summary>
    /// Preço da aposta, usado no custo e no valor investido.
    /// </summary>
    [Inject]
    private OpcoesApostas Opcoes { get; set; } = null!;

    /// <summary>
    /// Serviço de diálogos do MudBlazor.
    /// </summary>
    [Inject]
    private IDialogService Dialogos { get; set; } = null!;

    /// <summary>
    /// Mensagens rápidas exibidas ao usuário.
    /// </summary>
    [Inject]
    private ISnackbar Mensagens { get; set; } = null!;

    /// <summary>
    /// Grupos exibidos com o filtro selecionado.
    /// </summary>
    private IEnumerable<GrupoJogos> GruposVisiveis => this.filtro switch
    {
        FiltroJogos.Aguardando => this.grupos.Where(grupo => !grupo.Conferido),
        FiltroJogos.Conferidos => this.grupos.Where(grupo => grupo.Conferido),
        FiltroJogos.Premiados => this.grupos.Where(grupo => grupo.Premiado),
        _ => this.grupos,
    };

    /// <summary>
    /// Total de jogos cadastrados.
    /// </summary>
    private int TotalJogos => this.grupos.Sum(grupo => grupo.Jogos.Count);

    /// <summary>
    /// Soma das apostas de 15 dezenas que todos os jogos valem, usada no valor investido.
    /// </summary>
    private int TotalApostas => this.grupos.Sum(grupo => grupo.QuantidadeApostas);

    /// <summary>
    /// Quantidade de concursos com jogos.
    /// </summary>
    private int TotalConcursos => this.grupos.Count;

    /// <summary>
    /// Quantidade de jogos premiados.
    /// </summary>
    private int JogosPremiados => this.grupos.Sum(grupo => grupo.Jogos.Count(jogo => jogo.Conferencia?.Premiado == true));

    /// <summary>
    /// Soma dos prêmios de todos os jogos.
    /// </summary>
    private decimal TotalPremios => this.grupos.Sum(grupo => grupo.PremioTotal);

    /// <summary>
    /// Carrega os jogos ao abrir a página.
    /// </summary>
    /// <returns>Tarefa da carga.</returns>
    protected override async Task OnInitializedAsync()
    {
        await this.CarregarAsync();
        this.carregado = true;
    }

    /// <summary>
    /// Título do grupo: número do concurso e, se sorteado, a data.
    /// </summary>
    /// <param name="grupo">Grupo de jogos.</param>
    /// <returns>Título do grupo.</returns>
    private static string TituloGrupo(GrupoJogos grupo)
    {
        return grupo.Concurso is null
            ? $"Concurso {grupo.NumeroConcurso}"
            : $"Concurso {grupo.NumeroConcurso} · {FormatacaoConcurso.Data(grupo.Concurso.Data)}";
    }

    /// <summary>
    /// Valor do grupo: custo enquanto aguarda, prêmio total depois de conferido.
    /// </summary>
    /// <param name="grupo">Grupo de jogos.</param>
    /// <returns>Texto do valor.</returns>
    private string ValorGrupo(GrupoJogos grupo)
    {
        if (!grupo.Conferido)
        {
            return $"Custo {FormatacaoJogo.Reais(grupo.QuantidadeApostas * this.Opcoes.PrecoApostaMinima)}";
        }

        return grupo.PremioTotal > 0 ? $"Prêmio {FormatacaoJogo.Reais(grupo.PremioTotal)}" : "Sem prêmio";
    }

    /// <summary>
    /// Troca o filtro da lista.
    /// </summary>
    /// <param name="novoFiltro">Filtro escolhido.</param>
    private void TrocarFiltro(FiltroJogos novoFiltro)
    {
        this.filtro = novoFiltro;
    }

    /// <summary>
    /// Recarrega os jogos do serviço.
    /// </summary>
    /// <returns>Tarefa da carga.</returns>
    private async Task CarregarAsync()
    {
        this.grupos = await this.Servico.ListarAsync();
    }

    /// <summary>
    /// Abre o cadastro de jogo e recarrega a lista se um jogo foi salvo.
    /// </summary>
    /// <returns>Tarefa do cadastro.</returns>
    private async Task NovoJogoAsync()
    {
        var parametros = new DialogParameters<CadastroJogo>
        {
            { cadastro => cadastro.ConcursoSugerido, await this.Servico.ObterProximoConcursoAsync() },
        };
        IDialogReference dialogo = await this.Dialogos.ShowAsync<CadastroJogo>("Novo jogo", parametros);
        DialogResult? resultado = await dialogo.Result;

        if (resultado is { Canceled: false })
        {
            await this.CarregarAsync();
        }
    }

    /// <summary>
    /// Abre a conferência de um jogo de concurso já sorteado.
    /// </summary>
    /// <param name="grupo">Grupo do jogo.</param>
    /// <param name="jogo">Jogo clicado.</param>
    /// <returns>Tarefa da abertura.</returns>
    private async Task AbrirConferenciaAsync(GrupoJogos grupo, JogoConferido jogo)
    {
        if (grupo.Concurso is null)
        {
            return;
        }

        var parametros = new DialogParameters<DialogoConferencia>
        {
            { conferencia => conferencia.Jogo, jogo },
            { conferencia => conferencia.Concurso, grupo.Concurso },
        };
        string titulo = $"Jogo {FormatacaoJogo.NumeroJogo(jogo.Numero)} · concurso {grupo.NumeroConcurso}";
        await this.Dialogos.ShowAsync<DialogoConferencia>(titulo, parametros);
    }

    /// <summary>
    /// Pede confirmação e exclui o jogo.
    /// </summary>
    /// <param name="grupo">Grupo do jogo.</param>
    /// <param name="jogo">Jogo a excluir.</param>
    /// <returns>Tarefa da exclusão.</returns>
    private async Task ExcluirAsync(GrupoJogos grupo, JogoConferido jogo)
    {
        bool? confirmado = await this.Dialogos.ShowMessageBoxAsync(
            "Excluir jogo",
            $"Excluir o jogo {FormatacaoJogo.NumeroJogo(jogo.Numero)} do concurso {grupo.NumeroConcurso}?",
            yesText: "Excluir",
            cancelText: "Cancelar");
        if (confirmado != true)
        {
            return;
        }

        await this.Servico.ExcluirAsync(jogo.Jogo.Id);
        this.Mensagens.Add("Jogo excluído.", Severity.Success);
        await this.CarregarAsync();
    }
}
