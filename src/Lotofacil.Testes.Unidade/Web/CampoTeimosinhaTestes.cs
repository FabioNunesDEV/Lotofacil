// <copyright file="CampoTeimosinhaTestes.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Bunit;
using Lotofacil.Aplicacao.Jogos;
using Lotofacil.Aplicacao.Teimosinhas;
using Lotofacil.Dominio.Concursos;
using Lotofacil.Dominio.Jogos;
using Lotofacil.Dominio.Teimosinhas;
using Lotofacil.Web.Components.Teimosinhas;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Lotofacil.Testes.Unidade.Web;

/// <summary>
/// Testes do campo que marca os jogos como Teimosinha.
/// </summary>
public class CampoTeimosinhaTestes : ComponenteTesteBase
{
    /// <summary>
    /// Registra o serviço das Teimosinhas com duas ativas.
    /// </summary>
    public CampoTeimosinhaTestes()
    {
        ITeimosinhaRepositorio repositorio = Substitute.For<ITeimosinhaRepositorio>();
        repositorio.ListarAtivasAsync(Arg.Any<CancellationToken>())
            .Returns([new Teimosinha("Geradas", 3813), new Teimosinha("Minhas fixas", 3813)]);
        IJogoRepositorio jogos = Substitute.For<IJogoRepositorio>();
        this.Services.AddSingleton(new TeimosinhaServico(
            repositorio,
            jogos,
            new JogosServico(jogos, Substitute.For<IConcursoRepositorio>())));
    }

    /// <summary>
    /// Desligado, o campo só mostra o switch.
    /// </summary>
    [Fact]
    public void DesligadoSoMostraOSwitch()
    {
        IRenderedComponent<CampoTeimosinha> campo = this.Render<CampoTeimosinha>();

        Assert.Contains("Teimosinha (repetir em todo concurso)", campo.Find(".switch-teimosinha").TextContent);
        Assert.Empty(campo.FindAll(".nome-teimosinha"));
    }

    /// <summary>
    /// Ligado, o campo do nome aparece ligado às sugestões com os nomes das ativas.
    /// </summary>
    [Fact]
    public void LigadoMostraONomeComSugestoes()
    {
        IRenderedComponent<CampoTeimosinha> campo = this.Render<CampoTeimosinha>(parametros => parametros
            .Add(componente => componente.Ligada, true));

        string? lista = campo.Find(".nome-teimosinha input").GetAttribute("list");
        Assert.NotNull(lista);
        Assert.Equal(
            ["Geradas", "Minhas fixas"],
            campo.FindAll($"datalist#{lista} option").Select(opcao => opcao.GetAttribute("value")));
    }

    /// <summary>
    /// O switch e o nome digitado são repassados a quem usa o campo.
    /// </summary>
    [Fact]
    public void RepassaSwitchENome()
    {
        bool ligada = false;
        string? nome = null;
        IRenderedComponent<CampoTeimosinha> campo = this.Render<CampoTeimosinha>(parametros => parametros
            .Add(componente => componente.LigadaChanged, valor => ligada = valor)
            .Add(componente => componente.NomeChanged, valor => nome = valor));

        campo.Find(".switch-teimosinha input").Change(true);
        Assert.True(ligada);

        campo.Render(parametros => parametros.Add(componente => componente.Ligada, true));
        campo.Find(".nome-teimosinha input").Input("Minhas");
        Assert.Equal("Minhas", nome);
    }

    /// <summary>
    /// O salvamento só fica completo com o switch desligado ou com um nome.
    /// </summary>
    /// <param name="ligada">Estado do switch.</param>
    /// <param name="nome">Nome digitado.</param>
    /// <param name="esperado">Se está completo.</param>
    [Theory]
    [InlineData(false, null, true)]
    [InlineData(true, null, false)]
    [InlineData(true, "  ", false)]
    [InlineData(true, "Minhas fixas", true)]
    public void Completo(bool ligada, string? nome, bool esperado)
    {
        Assert.Equal(esperado, CampoTeimosinha.Completo(ligada, nome));
    }
}
