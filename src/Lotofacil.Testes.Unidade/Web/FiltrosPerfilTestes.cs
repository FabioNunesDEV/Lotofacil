// <copyright file="FiltrosPerfilTestes.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using System.Globalization;
using AngleSharp.Dom;
using Bunit;
using Lotofacil.Dominio.Estatisticas.Perfis;
using Lotofacil.Dominio.Geracao;
using Lotofacil.Web.Components.Geracao;

namespace Lotofacil.Testes.Unidade.Web;

/// <summary>
/// Testes da seção de filtros de perfil da geração.
/// </summary>
public class FiltrosPerfilTestes : ComponenteTesteBase
{
    /// <summary>
    /// Filtros recebidos pelo último aviso do componente.
    /// </summary>
    private FiltrosGeracao avisados = FiltrosGeracao.Nenhum;

    /// <summary>
    /// Usa a cultura da aplicação.
    /// </summary>
    public FiltrosPerfilTestes()
    {
        CultureInfo.CurrentCulture = new CultureInfo("pt-BR");
        CultureInfo.CurrentUICulture = new CultureInfo("pt-BR");
    }

    /// <summary>
    /// Ao abrir, nenhum filtro está ligado, os 7 perfis aparecem com os campos vazios e a sequência está desligada.
    /// </summary>
    [Fact]
    public void ValoresAoAbrir()
    {
        IRenderedComponent<FiltrosPerfil> secao = this.Renderizar(FiltrosGeracao.Nenhum, 15);

        Assert.Equal("Nenhum filtro ligado", secao.Find(".resumo-filtros").TextContent.Trim());
        Abrir(secao);
        Assert.Equal(
            ["Pares", "Primos", "Múltiplos de 3", "Fibonacci", "Soma", "Repetidas do 3810", "Na moldura"],
            secao.FindAll(".rotulo-filtro").Select(rotulo => rotulo.TextContent.Trim()));
        Assert.All(secao.FindAll(".linha-filtro input"), campo => Assert.True(string.IsNullOrEmpty(campo.GetAttribute("value"))));
        Assert.Empty(secao.FindAll(".dica-filtro"));
        Assert.Empty(secao.FindAll(".campo-sequencia"));
        Assert.Equal(
            "Os filtros só escolhem o perfil dos jogos; não mudam a chance de acerto.",
            secao.Find(".aviso-filtros").TextContent.Trim());
    }

    /// <summary>
    /// Preencher o mínimo e o máximo avisa os novos limites; com 15 dezenas aparece a dica de chance teórica.
    /// </summary>
    [Fact]
    public void PreencheOsLimitesEMostraADica()
    {
        IRenderedComponent<FiltrosPerfil> secao = this.Renderizar(FiltrosGeracao.Nenhum, 15);
        Abrir(secao);

        Linha(secao, TipoPerfil.Pares).QuerySelector(".campo-minimo input")!.Change("7");
        Assert.Equal([new LimitePerfil(TipoPerfil.Pares, 7, null)], this.avisados.Limites);

        secao.Render(parametros => parametros.Add(componente => componente.Filtros, this.avisados));
        Linha(secao, TipoPerfil.Pares).QuerySelector(".campo-maximo input")!.Change("8");
        Assert.Equal([new LimitePerfil(TipoPerfil.Pares, 7, 8)], this.avisados.Limites);

        secao.Render(parametros => parametros.Add(componente => componente.Filtros, this.avisados));
        Assert.Equal("Chance teórica: 57,17%", Linha(secao, TipoPerfil.Pares).QuerySelector(".dica-filtro")!.TextContent.Trim());
        Assert.Equal("1 filtro ligado", secao.Find(".resumo-filtros").TextContent.Trim());
    }

    /// <summary>
    /// Com 18 dezenas por jogo, a dica de chance teórica não aparece.
    /// </summary>
    [Fact]
    public void SemDicaCom18Dezenas()
    {
        var filtros = new FiltrosGeracao([new LimitePerfil(TipoPerfil.Pares, 9, 10)], null);

        IRenderedComponent<FiltrosPerfil> secao = this.Renderizar(filtros, 18);
        Abrir(secao);

        Assert.Equal("9", Linha(secao, TipoPerfil.Pares).QuerySelector(".campo-minimo input")!.GetAttribute("value"));
        Assert.Empty(secao.FindAll(".dica-filtro"));
    }

    /// <summary>
    /// Ligar "Evitar padrões populares" sugere 9 e mostra o campo com a explicação; desligar remove o filtro.
    /// </summary>
    [Fact]
    public void LigaEDesligaASequencia()
    {
        IRenderedComponent<FiltrosPerfil> secao = this.Renderizar(FiltrosGeracao.Nenhum, 15);
        Abrir(secao);

        secao.Find(".evitar-populares input").Change(true);
        Assert.Equal(9, this.avisados.MaiorSequenciaPermitida);

        secao.Render(parametros => parametros.Add(componente => componente.Filtros, this.avisados));
        Assert.Equal("9", secao.Find(".campo-sequencia input").GetAttribute("value"));
        Assert.StartsWith("Jogos com sequências longas, como 01 a 15", secao.Find(".aviso-populares").TextContent.Trim());
        Assert.Equal("1 filtro ligado", secao.Find(".resumo-filtros").TextContent.Trim());

        secao.Find(".campo-sequencia input").Change("4");
        Assert.Equal(4, this.avisados.MaiorSequenciaPermitida);

        secao.Find(".evitar-populares input").Change(false);
        Assert.Null(this.avisados.MaiorSequenciaPermitida);
    }

    /// <summary>
    /// "Limpar filtros" desliga tudo.
    /// </summary>
    [Fact]
    public void LimpaOsFiltros()
    {
        var filtros = new FiltrosGeracao(
            [new LimitePerfil(TipoPerfil.Pares, 7, 8), new LimitePerfil(TipoPerfil.Soma, null, 209), new LimitePerfil(TipoPerfil.Primos, 5, 6)],
            9);
        IRenderedComponent<FiltrosPerfil> secao = this.Renderizar(filtros, 15);

        Assert.Equal("4 filtros ligados", secao.Find(".resumo-filtros").TextContent.Trim());
        Abrir(secao);
        secao.Find(".botao-limpar-filtros").Click();

        Assert.Same(FiltrosGeracao.Nenhum, this.avisados);
    }

    /// <summary>
    /// Abre a seção recolhível.
    /// </summary>
    /// <param name="secao">Componente renderizado.</param>
    private static void Abrir(IRenderedComponent<FiltrosPerfil> secao)
    {
        secao.Find(".mud-expand-panel-header").Click();
    }

    /// <summary>
    /// Linha do perfil na seção.
    /// </summary>
    /// <param name="secao">Componente renderizado.</param>
    /// <param name="tipo">Perfil.</param>
    /// <returns>Elemento da linha.</returns>
    private static IElement Linha(IRenderedComponent<FiltrosPerfil> secao, TipoPerfil tipo)
    {
        return secao.Find($".linha-filtro[data-perfil='{tipo}']");
    }

    /// <summary>
    /// Renderiza a seção, guardando os filtros avisados.
    /// </summary>
    /// <param name="filtros">Filtros iniciais.</param>
    /// <param name="dezenasPorJogo">Dezenas por jogo.</param>
    /// <returns>Componente renderizado.</returns>
    private IRenderedComponent<FiltrosPerfil> Renderizar(FiltrosGeracao filtros, int dezenasPorJogo)
    {
        return this.Render<FiltrosPerfil>(parametros => parametros
            .Add(componente => componente.Filtros, filtros)
            .Add(componente => componente.FiltrosChanged, novos => this.avisados = novos)
            .Add(componente => componente.DezenasPorJogo, dezenasPorJogo)
            .Add(componente => componente.UltimoConcurso, 3810));
    }
}
