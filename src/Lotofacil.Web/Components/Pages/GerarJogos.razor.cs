// <copyright file="GerarJogos.razor.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Aplicacao.Geracao;
using Lotofacil.Aplicacao.Jogos;
using Lotofacil.Dominio;
using Lotofacil.Dominio.Geracao;
using Lotofacil.Dominio.Jogos;
using Lotofacil.Web.Components.Geracao;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Lotofacil.Web.Components.Pages;

/// <summary>
/// Gera jogos pelas estatísticas, com fixas e excluídas, e salva os escolhidos para um concurso futuro.
/// </summary>
public partial class GerarJogos
{
    /// <summary>
    /// Aviso fixo de que a estratégia não aumenta a chance de acerto.
    /// </summary>
    private const string Aviso = "Nenhuma estratégia aumenta a chance de acerto: cada jogo de 15 dezenas tem 1 chance "
        + "em 3.268.760 em todo concurso. As estratégias só organizam a escolha das dezenas pelo critério escolhido.";

    /// <summary>
    /// Valor da janela que representa todo o histórico.
    /// </summary>
    private const int TodasAsJanelas = 0;

    /// <summary>
    /// Último concurso e total da base, ou <c>null</c> com a base vazia ou antes da carga.
    /// </summary>
    private ContextoGeracao? contexto;

    /// <summary>
    /// Indica que a primeira carga terminou.
    /// </summary>
    private bool carregado;

    /// <summary>
    /// Indica que uma geração ou um salvamento está em andamento.
    /// </summary>
    private bool gerando;

    /// <summary>
    /// Concurso dos jogos.
    /// </summary>
    private int? numeroConcurso;

    /// <summary>
    /// Quantidade de jogos pedida.
    /// </summary>
    private int quantidade = ParametrosGeracao.QuantidadePadrao;

    /// <summary>
    /// Dezenas de cada jogo.
    /// </summary>
    private int dezenasPorJogo = RegrasLotofacil.MinimoDezenasPorJogo;

    /// <summary>
    /// Estratégia escolhida.
    /// </summary>
    private EstrategiaGeracao estrategia = EstrategiaGeracao.Aleatoria;

    /// <summary>
    /// Janela da frequência: quantidade de concursos recentes, ou <see cref="TodasAsJanelas"/>.
    /// </summary>
    private int janela = TodasAsJanelas;

    /// <summary>
    /// Dezenas fixas.
    /// </summary>
    private IReadOnlyCollection<int> fixas = [];

    /// <summary>
    /// Dezenas excluídas.
    /// </summary>
    private IReadOnlyCollection<int> excluidas = [];

    /// <summary>
    /// Filtros de perfil e de sequência.
    /// </summary>
    private FiltrosGeracao filtros = FiltrosGeracao.Nenhum;

    /// <summary>
    /// Jogos gerados ainda não salvos.
    /// </summary>
    private List<JogoGerado> gerados = [];

    /// <summary>
    /// Serviço de geração.
    /// </summary>
    [Inject]
    private GeracaoJogosServico Servico { get; set; } = null!;

    /// <summary>
    /// Preço da aposta, usado nos custos.
    /// </summary>
    [Inject]
    private OpcoesApostas Opcoes { get; set; } = null!;

    /// <summary>
    /// Mensagens rápidas exibidas ao usuário.
    /// </summary>
    [Inject]
    private ISnackbar Mensagens { get; set; } = null!;

    /// <summary>
    /// Custo da geração pedida no formulário.
    /// </summary>
    private decimal CustoGeracao =>
        this.quantidade * Jogo.CalcularQuantidadeApostas(this.dezenasPorJogo) * this.Opcoes.PrecoApostaMinima;

    /// <summary>
    /// Custo dos jogos da lista.
    /// </summary>
    private decimal CustoLista =>
        this.gerados.Sum(jogo => Jogo.CalcularQuantidadeApostas(jogo.Dezenas.Count)) * this.Opcoes.PrecoApostaMinima;

    /// <summary>
    /// Carrega o contexto e sugere o próximo concurso.
    /// </summary>
    /// <returns>Tarefa da carga.</returns>
    protected override async Task OnInitializedAsync()
    {
        this.contexto = await this.Servico.ObterContextoAsync();
        this.numeroConcurso = this.contexto?.ProximoConcurso;
        this.carregado = true;
    }

    /// <summary>
    /// Nome exibido de uma janela.
    /// </summary>
    /// <param name="valor">Quantidade de concursos, ou <see cref="TodasAsJanelas"/>.</param>
    /// <returns>"Todos (3.810)" ou "Últimos 10".</returns>
    private string NomeJanela(int valor)
    {
        return valor == TodasAsJanelas ? $"Todos ({this.contexto?.TotalConcursos ?? 0:N0})" : $"Últimos {valor}";
    }

    /// <summary>
    /// Volta todas as dezenas a livres.
    /// </summary>
    private void Limpar()
    {
        this.fixas = [];
        this.excluidas = [];
    }

    /// <summary>
    /// Traz os limites dos filtros para a faixa possível depois de trocar as dezenas por jogo.
    /// </summary>
    private void AjustarFiltros()
    {
        this.filtros = this.filtros.AjustarPara(this.dezenasPorJogo);
    }

    /// <summary>
    /// Gera os jogos com os parâmetros do formulário, substituindo a lista, ou mostra a recusa.
    /// </summary>
    /// <returns>Tarefa da geração.</returns>
    private async Task GerarAsync()
    {
        this.gerando = true;
        try
        {
            var parametros = new ParametrosGeracao(
                Math.Clamp(this.quantidade, ParametrosGeracao.QuantidadeMinima, ParametrosGeracao.QuantidadeMaxima),
                this.dezenasPorJogo,
                this.estrategia,
                this.fixas,
                this.excluidas,
                this.filtros);
            JogosGerados resultado = await this.Servico.GerarAsync(
                parametros,
                this.janela == TodasAsJanelas ? null : this.janela);

            if (resultado.Recusa is string recusa)
            {
                this.Mensagens.Add(recusa, Severity.Warning);
                return;
            }

            this.gerados = [.. resultado.Jogos];
            if (resultado.CombinacoesPossiveis is int possiveis)
            {
                this.Mensagens.Add(FormatacaoGeracao.AvisoCombinacoes(possiveis, resultado.ComFiltros), Severity.Info);
            }

            if (resultado.JogosPedidos is int pedidos)
            {
                this.Mensagens.Add(FormatacaoGeracao.AvisoFiltros(resultado.Jogos.Count, pedidos), Severity.Info);
            }
        }
        finally
        {
            this.gerando = false;
        }
    }

    /// <summary>
    /// Tira um jogo da lista.
    /// </summary>
    /// <param name="jogo">Jogo a remover.</param>
    private void Remover(JogoGerado jogo)
    {
        this.gerados.Remove(jogo);
    }

    /// <summary>
    /// Salva os jogos da lista para o concurso do formulário e mostra o resumo.
    /// </summary>
    /// <returns>Tarefa do salvamento.</returns>
    private async Task SalvarAsync()
    {
        if (this.numeroConcurso is not int concurso)
        {
            return;
        }

        this.gerando = true;
        try
        {
            List<IReadOnlyCollection<int>> jogos = this.gerados.Select(jogo => (IReadOnlyCollection<int>)jogo.Dezenas).ToList();
            ResultadoCadastroVarios resultado = await this.Servico.SalvarAsync(concurso, jogos);

            bool gravou = resultado.Gravados > 0;
            this.Mensagens.Add(
                FormatacaoGeracao.Salvamento(resultado, concurso),
                gravou ? Severity.Success : Severity.Warning);
            if (gravou)
            {
                this.gerados = [];
            }
        }
        finally
        {
            this.gerando = false;
        }
    }
}
