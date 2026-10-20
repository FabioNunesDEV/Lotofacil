// <copyright file="Estatisticas.razor.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Aplicacao.Estatisticas;
using Lotofacil.Dominio.Estatisticas;
using Lotofacil.Web.Components.Estatisticas;
using Microsoft.AspNetCore.Components;

namespace Lotofacil.Web.Components.Pages;

/// <summary>
/// Estatísticas por dezena na janela escolhida: mapa de calor, rankings e tabela.
/// </summary>
public partial class Estatisticas
{
    /// <summary>
    /// Aviso fixo de que estatística não é previsão.
    /// </summary>
    private const string Aviso = "As estatísticas descrevem o passado. Cada combinação de 15 dezenas tem a mesma chance "
        + "(1 em 3.268.760) em todo concurso; os números servem como critério de escolha, não como previsão.";

    /// <summary>
    /// Mensagem exibida quando a base não tem concursos.
    /// </summary>
    private const string MensagemBaseVazia =
        "Nenhum concurso gravado na base local. Use “Importar histórico” na página Resultados.";

    /// <summary>
    /// Valor do chip "Todos": a janela é todo o histórico.
    /// </summary>
    private const int TodasAsJanelas = 0;

    /// <summary>
    /// Janela selecionada: quantidade de concursos mais recentes, ou <see cref="TodasAsJanelas"/>.
    /// </summary>
    private int janela = TodasAsJanelas;

    /// <summary>
    /// Estatísticas calculadas, ou <c>null</c> antes do cálculo ou com a base vazia.
    /// </summary>
    private EstatisticasDezenas? estatisticas;

    /// <summary>
    /// Indica que o primeiro cálculo já terminou.
    /// </summary>
    private bool carregado;

    /// <summary>
    /// Serviço de estatísticas.
    /// </summary>
    [Inject]
    private EstatisticasServico Servico { get; set; } = null!;

    /// <summary>
    /// Intervalo de concursos considerado, exibido abaixo do título.
    /// </summary>
    private string Intervalo => this.estatisticas is EstatisticasDezenas dados
        ? $"Concursos {dados.PrimeiroConcurso} a {dados.UltimoConcurso} · "
            + $"{FormatacaoEstatistica.Quantidade(dados.ConcursosNaJanela)} concursos"
        : "Calculadas a partir da base local";

    /// <summary>
    /// Calcula as estatísticas sobre todo o histórico ao abrir a página.
    /// </summary>
    /// <returns>Tarefa do cálculo.</returns>
    protected override async Task OnInitializedAsync()
    {
        await this.CalcularAsync();
        this.carregado = true;
    }

    /// <summary>
    /// Troca a janela e recalcula.
    /// </summary>
    /// <param name="novaJanela">Quantidade de concursos mais recentes, ou <see cref="TodasAsJanelas"/>.</param>
    /// <returns>Tarefa do cálculo.</returns>
    private async Task TrocarJanelaAsync(int novaJanela)
    {
        this.janela = novaJanela;
        await this.CalcularAsync();
    }

    /// <summary>
    /// Calcula as estatísticas na janela selecionada.
    /// </summary>
    /// <returns>Tarefa do cálculo.</returns>
    private async Task CalcularAsync()
    {
        int? ultimos = this.janela == TodasAsJanelas ? null : this.janela;
        this.estatisticas = await this.Servico.CalcularAsync(ultimos);
    }
}
