// <copyright file="FiltrosPerfil.razor.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Dominio;
using Lotofacil.Dominio.Estatisticas.Perfis;
using Lotofacil.Dominio.Geracao;
using Microsoft.AspNetCore.Components;

namespace Lotofacil.Web.Components.Geracao;

/// <summary>
/// Seção recolhível com os limites por perfil e a maior sequência permitida da geração.
/// </summary>
public partial class FiltrosPerfil
{
    /// <summary>
    /// Texto fixo da seção.
    /// </summary>
    private const string AvisoFiltros = "Os filtros só escolhem o perfil dos jogos; não mudam a chance de acerto.";

    /// <summary>
    /// Explicação do filtro de sequência.
    /// </summary>
    private const string AvisoPopulares = "Jogos com sequências longas, como 01 a 15, são muito escolhidos; evitá-los só "
        + "pode reduzir a divisão do prêmio se o jogo for premiado.";

    /// <summary>
    /// Filtros atuais.
    /// </summary>
    [Parameter]
    public FiltrosGeracao Filtros { get; set; } = FiltrosGeracao.Nenhum;

    /// <summary>
    /// Avisa os novos filtros depois de uma alteração.
    /// </summary>
    [Parameter]
    public EventCallback<FiltrosGeracao> FiltrosChanged { get; set; }

    /// <summary>
    /// Dezenas por jogo, que definem a faixa dos limites e se a dica de chance aparece.
    /// </summary>
    [Parameter]
    public int DezenasPorJogo { get; set; } = RegrasLotofacil.MinimoDezenasPorJogo;

    /// <summary>
    /// Número do último concurso gravado, exibido no rótulo das repetidas.
    /// </summary>
    [Parameter]
    public int UltimoConcurso { get; set; }

    /// <summary>
    /// Indica se a maior sequência está sendo filtrada.
    /// </summary>
    private bool EvitarPadroesPopulares => this.Filtros.MaiorSequenciaPermitida is not null;

    /// <summary>
    /// Limite atual do perfil.
    /// </summary>
    /// <param name="tipo">Perfil.</param>
    /// <returns>O limite, ou <c>null</c> sem filtro no perfil.</returns>
    private LimitePerfil? Limite(TipoPerfil tipo)
    {
        return this.Filtros.Limites.FirstOrDefault(limite => limite.Tipo == tipo);
    }

    /// <summary>
    /// Explicação do filtro, com o valor mais comum só em jogos de 15 dezenas.
    /// </summary>
    /// <param name="tipo">Perfil.</param>
    /// <returns>Texto exibido embaixo do nome do filtro.</returns>
    private string Explicacao(TipoPerfil tipo)
    {
        string explicacao = FormatacaoGeracao.ExplicacaoFiltro(tipo, this.UltimoConcurso, this.DezenasPorJogo);

        return this.DezenasPorJogo == RegrasLotofacil.DezenasSorteadas ? explicacao + FormatacaoGeracao.MaisComum(tipo) : explicacao;
    }

    /// <summary>
    /// Troca o mínimo do perfil, mantendo o máximo.
    /// </summary>
    /// <param name="tipo">Perfil.</param>
    /// <param name="minimo">Novo mínimo, ou <c>null</c>.</param>
    /// <returns>Tarefa do aviso.</returns>
    private Task AlterarMinimoAsync(TipoPerfil tipo, int? minimo)
    {
        return this.AvisarAsync(this.Filtros.ComLimite(new LimitePerfil(tipo, minimo, this.Limite(tipo)?.Maximo)));
    }

    /// <summary>
    /// Troca o máximo do perfil, mantendo o mínimo.
    /// </summary>
    /// <param name="tipo">Perfil.</param>
    /// <param name="maximo">Novo máximo, ou <c>null</c>.</param>
    /// <returns>Tarefa do aviso.</returns>
    private Task AlterarMaximoAsync(TipoPerfil tipo, int? maximo)
    {
        return this.AvisarAsync(this.Filtros.ComLimite(new LimitePerfil(tipo, this.Limite(tipo)?.Minimo, maximo)));
    }

    /// <summary>
    /// Liga o filtro de sequência com o limite padrão, ou o desliga.
    /// </summary>
    /// <param name="ligado">Novo estado de "Evitar padrões populares".</param>
    /// <returns>Tarefa do aviso.</returns>
    private Task AlterarEvitarAsync(bool ligado)
    {
        int? permitida = ligado ? Math.Min(FiltrosGeracao.MaiorSequenciaPadrao, this.DezenasPorJogo) : null;

        return this.AvisarAsync(this.Filtros.ComMaiorSequencia(permitida));
    }

    /// <summary>
    /// Troca a maior sequência permitida.
    /// </summary>
    /// <param name="permitida">Nova maior sequência.</param>
    /// <returns>Tarefa do aviso.</returns>
    private Task AlterarSequenciaAsync(int permitida)
    {
        return this.AvisarAsync(this.Filtros.ComMaiorSequencia(Math.Clamp(permitida, 1, this.DezenasPorJogo)));
    }

    /// <summary>
    /// Desliga todos os filtros.
    /// </summary>
    /// <returns>Tarefa do aviso.</returns>
    private Task LimparAsync()
    {
        return this.AvisarAsync(FiltrosGeracao.Nenhum);
    }

    /// <summary>
    /// Avisa os novos filtros.
    /// </summary>
    /// <param name="filtros">Filtros alterados.</param>
    /// <returns>Tarefa do aviso.</returns>
    private Task AvisarAsync(FiltrosGeracao filtros)
    {
        return this.FiltrosChanged.InvokeAsync(filtros);
    }
}
