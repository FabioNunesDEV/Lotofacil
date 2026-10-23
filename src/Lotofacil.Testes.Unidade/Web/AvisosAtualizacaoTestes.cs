// <copyright file="AvisosAtualizacaoTestes.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using System.Globalization;
using Lotofacil.Aplicacao.Concursos;
using Lotofacil.Dominio.Concursos;
using Lotofacil.Web.Components.Concursos;
using MudBlazor;

namespace Lotofacil.Testes.Unidade.Web;

/// <summary>
/// Testes dos textos do andamento e do desfecho da atualização dos resultados.
/// </summary>
public class AvisosAtualizacaoTestes
{
    /// <summary>
    /// Usa a cultura da aplicação.
    /// </summary>
    public AvisosAtualizacaoTestes()
    {
        CultureInfo.CurrentCulture = new CultureInfo("pt-BR");
    }

    /// <summary>
    /// Andamento com o concurso buscado e a posição.
    /// </summary>
    [Fact]
    public void Andamento()
    {
        Assert.Equal(
            "Buscando o concurso 3810 (2 de 3)…",
            AvisosAtualizacao.Andamento(new AndamentoAtualizacao(3810, 2, 3)));
    }

    /// <summary>
    /// Base vazia pede a importação, como aviso.
    /// </summary>
    [Fact]
    public void BaseVazia()
    {
        Assert.Equal(
            new AvisoAtualizacao(
                "A base local está vazia. Use “Importar histórico” antes de atualizar os resultados.",
                Severity.Warning),
            AvisosAtualizacao.Desfecho(new ResultadoAtualizacao(null, 0, null)));
    }

    /// <summary>
    /// Sem concursos novos, informa o último gravado.
    /// </summary>
    [Fact]
    public void BaseJaAtualizada()
    {
        Assert.Equal(
            new AvisoAtualizacao("A base local já está atualizada: último concurso 3808.", Severity.Info),
            AvisosAtualizacao.Desfecho(new ResultadoAtualizacao(3808, 0, null)));
    }

    /// <summary>
    /// Um concurso novo usa o singular.
    /// </summary>
    [Fact]
    public void UmConcursoNovo()
    {
        Assert.Equal(
            new AvisoAtualizacao("Atualização concluída: 1 concurso novo gravado (3809).", Severity.Success),
            AvisosAtualizacao.Desfecho(new ResultadoAtualizacao(3808, 1, null)));
    }

    /// <summary>
    /// Vários concursos novos mostram o intervalo, sem separador de milhar nos números.
    /// </summary>
    [Fact]
    public void VariosConcursosNovos()
    {
        Assert.Equal(
            new AvisoAtualizacao("Atualização concluída: 3 concursos novos gravados (3809 a 3811).", Severity.Success),
            AvisosAtualizacao.Desfecho(new ResultadoAtualizacao(3808, 3, null)));
    }

    /// <summary>
    /// Falha depois de gravar informa o motivo e o que foi gravado, como erro.
    /// </summary>
    [Fact]
    public void FalhaDepoisDeGravar()
    {
        var falha = new FonteResultadosException("A Caixa respondeu com o código 503 ao buscar o concurso 3811.");

        Assert.Equal(
            new AvisoAtualizacao(
                "A atualização foi interrompida. A Caixa respondeu com o código 503 ao buscar o concurso 3811. "
                    + "Concursos gravados antes da falha: 2 (até o 3810).",
                Severity.Error),
            AvisosAtualizacao.Desfecho(new ResultadoAtualizacao(3808, 2, falha)));
    }

    /// <summary>
    /// Falha sem nada gravado mostra só o motivo.
    /// </summary>
    [Fact]
    public void FalhaSemGravar()
    {
        var falha = new FonteResultadosException("Tempo esgotado.");

        Assert.Equal(
            new AvisoAtualizacao("A atualização foi interrompida. Tempo esgotado.", Severity.Error),
            AvisosAtualizacao.Desfecho(new ResultadoAtualizacao(3808, 0, falha)));
    }

    /// <summary>
    /// Falha não prevista remete ao log.
    /// </summary>
    [Fact]
    public void FalhaInesperada()
    {
        Assert.Equal(
            "A atualização foi interrompida. Erro inesperado; os detalhes foram registrados no log.",
            AvisosAtualizacao.FalhaInesperada);
    }
}
