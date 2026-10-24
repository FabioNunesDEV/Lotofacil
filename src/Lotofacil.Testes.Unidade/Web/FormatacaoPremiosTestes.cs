// <copyright file="FormatacaoPremiosTestes.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using System.Globalization;
using Lotofacil.Aplicacao.Jogos;
using Lotofacil.Dominio.Concursos;
using Lotofacil.Dominio.Jogos;
using Lotofacil.Web.Components.Jogos;

namespace Lotofacil.Testes.Unidade.Web;

/// <summary>
/// Testes dos textos do aviso de jogos premiados, na cultura pt-BR.
/// </summary>
public class FormatacaoPremiosTestes
{
    /// <summary>
    /// Concurso 3809, sorteado em 20/10/2026, com as dezenas padrão.
    /// </summary>
    private static readonly Concurso Concurso3809 = ConcursoFabrica.Criar(numero: 3809, data: new DateOnly(2026, 10, 20));

    /// <summary>
    /// Usa a cultura da aplicação.
    /// </summary>
    public FormatacaoPremiosTestes()
    {
        CultureInfo.CurrentCulture = new CultureInfo("pt-BR");
    }

    /// <summary>
    /// Resumo no plural e nos singulares.
    /// </summary>
    [Fact]
    public void Resumo()
    {
        Assert.Equal("3 de 5 jogos conferidos foram premiados.", FormatacaoPremios.Resumo(3, 5));
        Assert.Equal("1 de 4 jogos conferidos foi premiado.", FormatacaoPremios.Resumo(1, 4));
        Assert.Equal("1 de 1 jogo conferido foi premiado.", FormatacaoPremios.Resumo(1, 1));
    }

    /// <summary>
    /// Cabeçalho do concurso e prazo a receber ou encerrado (90 dias depois do sorteio, inclusive).
    /// </summary>
    [Fact]
    public void ConcursoEPrazo()
    {
        var grupo = new GrupoJogos(3809, Concurso3809, []);

        Assert.Equal("Concurso 3809 · 20/10/2026", FormatacaoPremios.Concurso(grupo));
        Assert.Equal("Receber até 18/01/2027", FormatacaoPremios.Prazo(grupo, new DateOnly(2026, 10, 24)));
        Assert.Equal("Receber até 18/01/2027", FormatacaoPremios.Prazo(grupo, new DateOnly(2027, 1, 18)));
        Assert.Equal("Prazo encerrado em 18/01/2027", FormatacaoPremios.Prazo(grupo, new DateOnly(2027, 1, 20)));
    }

    /// <summary>
    /// Linha do jogo de 15 dezenas e da aposta múltipla.
    /// </summary>
    [Fact]
    public void LinhaDoJogo()
    {
        int[] trezeAcertos = [.. ConcursoFabrica.DezenasPadrao.Take(13), 3, 6];
        var jogo15 = new Jogo(3809, trezeAcertos, DateTime.Now);
        var jogo16 = new Jogo(3809, [.. trezeAcertos, 8], DateTime.Now);

        Assert.Equal(
            "Jogo 02 · 13 acertos · R$ 35,00",
            FormatacaoPremios.Jogo(new JogoConferido(jogo15, 2, ConferenciaJogo.Conferir(jogo15, Concurso3809))));
        Assert.Equal(
            "Jogo 01 · 16 dezenas · 13 acertos · R$ 287,00",
            FormatacaoPremios.Jogo(new JogoConferido(jogo16, 1, ConferenciaJogo.Conferir(jogo16, Concurso3809))));
    }

    /// <summary>
    /// Total no plural e no singular.
    /// </summary>
    [Fact]
    public void Total()
    {
        Assert.Equal("Total: 3 jogos · R$ 49,00", FormatacaoPremios.Total(3, 49m));
        Assert.Equal("Total: 1 jogo · R$ 287,00", FormatacaoPremios.Total(1, 287m));
    }

    /// <summary>
    /// Aviso de nenhum prêmio com vários jogos e com um só.
    /// </summary>
    [Fact]
    public void NenhumPremio()
    {
        Assert.Equal(
            "Seus 4 jogos dos concursos novos foram conferidos; nenhum foi premiado.",
            FormatacaoPremios.NenhumPremio(4));
        Assert.Equal(
            "Seu jogo dos concursos novos foi conferido e não foi premiado.",
            FormatacaoPremios.NenhumPremio(1));
    }
}
