// <copyright file="AvisosOrdemSorteioTestes.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using System.Globalization;
using Lotofacil.Aplicacao.Concursos;
using Lotofacil.Web.Components.Concursos;
using MudBlazor;

namespace Lotofacil.Testes.Unidade.Web;

/// <summary>
/// Testes dos textos do andamento e do desfecho do preenchimento da ordem do sorteio.
/// </summary>
public class AvisosOrdemSorteioTestes
{
    /// <summary>
    /// Usa a cultura da aplicação.
    /// </summary>
    public AvisosOrdemSorteioTestes()
    {
        CultureInfo.CurrentCulture = new CultureInfo("pt-BR");
    }

    /// <summary>
    /// Andamento com o número sem separador e a posição e o total com separador de milhar.
    /// </summary>
    [Fact]
    public void Andamento()
    {
        Assert.Equal(
            "Buscando a ordem do sorteio do concurso 1234 (1.234 de 3.808)…",
            AvisosOrdemSorteio.Andamento(new AndamentoOrdemSorteio(1234, 1234, 3808, null), new DateTime(2026, 10, 24, 18, 16, 0)));
    }

    /// <summary>
    /// Na pausa pedida pela Caixa, o andamento mostra o horário da nova tentativa.
    /// </summary>
    [Fact]
    public void AndamentoDuranteAEspera()
    {
        var andamento = new AndamentoOrdemSorteio(38, 38, 3808, TimeSpan.FromMinutes(1));

        Assert.Equal(
            "A Caixa pediu uma pausa. Nova tentativa do concurso 38 às 18:17 (38 de 3.808)…",
            AvisosOrdemSorteio.Andamento(andamento, new DateTime(2026, 10, 24, 18, 16, 20)));
    }

    /// <summary>
    /// Encerramento pelo limite de consultas traz a última falha e é erro.
    /// </summary>
    [Fact]
    public void EncerradoPorLimite()
    {
        FalhaOrdemSorteio[] falhas =
            [new FalhaOrdemSorteio(38, "A Caixa respondeu com o código 429 ao buscar a ordem do concurso 38.", null)];
        var resultado = new ResultadoPreenchimentoOrdem(3808, 37, falhas, DesfechoPreenchimento.EncerradoPorLimite);

        Assert.Equal(
            new AvisoAtualizacao(
                "Preenchimento encerrado: a Caixa continua limitando as consultas. A Caixa respondeu com o código 429 ao "
                    + "buscar a ordem do concurso 38. Ordem preenchida em 37 concursos; 3.771 concursos continuam sem ordem.",
                Severity.Error),
            AvisosOrdemSorteio.Desfecho(resultado));
    }

    /// <summary>
    /// Tudo preenchido, no plural e no singular.
    /// </summary>
    /// <param name="preenchidos">Concursos preenchidos.</param>
    /// <param name="esperado">Mensagem esperada.</param>
    [Theory]
    [InlineData(3808, "Ordem do sorteio preenchida em 3.808 concursos.")]
    [InlineData(1, "Ordem do sorteio preenchida em 1 concurso.")]
    public void TudoPreenchido(int preenchidos, string esperado)
    {
        Assert.Equal(
            new AvisoAtualizacao(esperado, Severity.Success),
            AvisosOrdemSorteio.Desfecho(Resultado(preenchidos, preenchidos, falhas: 0)));
    }

    /// <summary>
    /// Com falhas, o resumo avisa quantos ficaram sem ordem, no plural e no singular.
    /// </summary>
    /// <param name="falhas">Concursos que falharam.</param>
    /// <param name="esperado">Mensagem esperada.</param>
    [Theory]
    [InlineData(2, "Ordem do sorteio preenchida em 3.806 concursos; 2 concursos ficaram sem ordem e podem ser buscados de novo.")]
    [InlineData(1, "Ordem do sorteio preenchida em 3.807 concursos; 1 concurso ficou sem ordem e pode ser buscado de novo.")]
    public void ComFalhas(int falhas, string esperado)
    {
        Assert.Equal(
            new AvisoAtualizacao(esperado, Severity.Warning),
            AvisosOrdemSorteio.Desfecho(Resultado(3808, 3808 - falhas, falhas)));
    }

    /// <summary>
    /// Interrupção pelo usuário informa o preenchido e o que continua sem ordem.
    /// </summary>
    /// <param name="preenchidos">Concursos preenchidos antes de interromper.</param>
    /// <param name="esperado">Mensagem esperada.</param>
    [Theory]
    [InlineData(500, "Preenchimento interrompido: ordem preenchida em 500 concursos; 3.308 concursos continuam sem ordem.")]
    [InlineData(3807, "Preenchimento interrompido: ordem preenchida em 3.807 concursos; 1 concurso continua sem ordem.")]
    public void Interrompido(int preenchidos, string esperado)
    {
        Assert.Equal(
            new AvisoAtualizacao(esperado, Severity.Info),
            AvisosOrdemSorteio.Desfecho(Resultado(3808, preenchidos, falhas: 0, interrompido: true)));
    }

    /// <summary>
    /// Encerramento por falhas seguidas traz a última falha e é erro.
    /// </summary>
    [Fact]
    public void EncerradoPorFalhas()
    {
        FalhaOrdemSorteio[] falhas = Enumerable.Range(1, 5)
            .Select(numero => new FalhaOrdemSorteio(
                numero,
                $"Não foi possível conectar à Caixa para buscar a ordem do concurso {numero}.",
                null))
            .ToArray();
        var resultado = new ResultadoPreenchimentoOrdem(3808, 0, falhas, DesfechoPreenchimento.EncerradoPorFalhas);

        Assert.Equal(
            new AvisoAtualizacao(
                "Preenchimento encerrado após 5 falhas seguidas. Não foi possível conectar à Caixa para buscar a ordem do "
                    + "concurso 5. Ordem preenchida em 0 concursos; 3.808 concursos continuam sem ordem.",
                Severity.Error),
            AvisosOrdemSorteio.Desfecho(resultado));
    }

    /// <summary>
    /// Sem concursos sem ordem ao acionar, a mensagem diz que todos já têm a ordem.
    /// </summary>
    [Fact]
    public void NenhumSemOrdem()
    {
        Assert.Equal(
            new AvisoAtualizacao("Todos os concursos já têm a ordem do sorteio.", Severity.Info),
            AvisosOrdemSorteio.Desfecho(Resultado(0, 0, falhas: 0)));
    }

    /// <summary>
    /// Mensagem de erro inesperado.
    /// </summary>
    [Fact]
    public void FalhaInesperada()
    {
        Assert.Equal(
            "O preenchimento foi interrompido. Erro inesperado; os detalhes foram registrados no log.",
            AvisosOrdemSorteio.FalhaInesperada);
    }

    /// <summary>
    /// Cria um desfecho com falhas genéricas.
    /// </summary>
    /// <param name="total">Concursos sem ordem no início.</param>
    /// <param name="preenchidos">Concursos preenchidos.</param>
    /// <param name="falhas">Quantidade de falhas.</param>
    /// <param name="interrompido">Se o usuário interrompeu.</param>
    /// <returns>Desfecho do preenchimento.</returns>
    private static ResultadoPreenchimentoOrdem Resultado(int total, int preenchidos, int falhas, bool interrompido = false)
    {
        FalhaOrdemSorteio[] lista = Enumerable.Range(1, falhas)
            .Select(numero => new FalhaOrdemSorteio(numero, "Falha.", null))
            .ToArray();

        DesfechoPreenchimento desfecho = interrompido ? DesfechoPreenchimento.Interrompido : DesfechoPreenchimento.Concluido;

        return new ResultadoPreenchimentoOrdem(total, preenchidos, lista, desfecho);
    }
}
