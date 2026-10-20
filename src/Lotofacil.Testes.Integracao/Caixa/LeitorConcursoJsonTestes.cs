// <copyright file="LeitorConcursoJsonTestes.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using System.Text;
using Lotofacil.Dominio.Concursos;
using Lotofacil.Infraestrutura.Caixa;
using Newtonsoft.Json.Linq;

namespace Lotofacil.Testes.Integracao.Caixa;

/// <summary>
/// Testes da leitura das respostas JSON da API da Caixa, com a resposta real do concurso 3808 e variações dela.
/// </summary>
public class LeitorConcursoJsonTestes
{
    /// <summary>
    /// Mensagem de formato inesperado para o concurso 3808.
    /// </summary>
    private const string FormatoInesperado3808 = "A resposta da Caixa para o concurso 3808 não está no formato esperado.";

    /// <summary>
    /// A resposta real vira o concurso com valores exatos e ordem do sorteio.
    /// </summary>
    [Fact]
    public void LeAmostraReal()
    {
        Concurso concurso = LeitorConcursoJson.LerConcurso(RespostaConcursoCaixa.EmBytes(RespostaConcursoCaixa.LerAmostra()), 3808);

        Assert.Equal(3808, concurso.Numero);
        Assert.Equal(new DateOnly(2026, 10, 19), concurso.Data);
        Assert.Equal([1, 4, 7, 10, 12, 13, 14, 15, 17, 18, 19, 20, 21, 22, 24], concurso.Dezenas);
        Assert.Equal([13, 10, 22, 19, 12, 1, 7, 14, 4, 24, 21, 15, 18, 20, 17], concurso.OrdemSorteio);
        Assert.Equal(
            [(11, 441_683, 7m), (12, 78_238, 14m), (13, 6_009, 35m), (14, 159, 1_920.13m), (15, 1, 1_456_047.72m)],
            concurso.Rateios.Select(rateio => (rateio.Faixa, rateio.Ganhadores, rateio.ValorPremio)));
        Assert.Equal(17_703_532.00m, concurso.Arrecadacao);
        Assert.Equal(0m, concurso.Acumulado15);
        Assert.Equal(2_000_000m, concurso.EstimativaProximo);
        Assert.Equal(12_676_875.24m, concurso.AcumuladoIndependencia);
    }

    /// <summary>
    /// O número do último concurso é lido da resposta.
    /// </summary>
    [Fact]
    public void LeNumeroDoUltimoConcurso()
    {
        int numero = LeitorConcursoJson.LerNumero(RespostaConcursoCaixa.ComNumero(3811));

        Assert.Equal(3811, numero);
    }

    /// <summary>
    /// Resposta do último concurso sem número tem mensagem própria.
    /// </summary>
    [Fact]
    public void UltimoConcursoSemNumero()
    {
        JObject resposta = RespostaConcursoCaixa.LerAmostra();
        resposta.Remove("numero");

        var erro = Assert.Throws<FonteResultadosException>(() => LeitorConcursoJson.LerNumero(RespostaConcursoCaixa.EmBytes(resposta)));

        Assert.Equal("A resposta da Caixa para o último concurso não está no formato esperado.", erro.Message);
    }

    /// <summary>
    /// Ordem do sorteio vazia ou ausente deixa o concurso sem ordem.
    /// </summary>
    /// <param name="ausente">Se o campo é removido em vez de esvaziado.</param>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void OrdemDoSorteioVaziaOuAusente(bool ausente)
    {
        JObject resposta = RespostaConcursoCaixa.LerAmostra();
        if (ausente)
        {
            resposta.Remove("dezenasSorteadasOrdemSorteio");
        }
        else
        {
            resposta["dezenasSorteadasOrdemSorteio"] = new JArray();
        }

        Concurso concurso = LeitorConcursoJson.LerConcurso(RespostaConcursoCaixa.EmBytes(resposta), 3808);

        Assert.Null(concurso.OrdemSorteio);
    }

    /// <summary>
    /// Faixa com descrição que não corresponde à posição indica formato inesperado.
    /// </summary>
    [Fact]
    public void DescricaoDeFaixaInesperada()
    {
        JObject resposta = RespostaConcursoCaixa.LerAmostra();
        resposta["listaRateioPremio"]![0]!["descricaoFaixa"] = "14 acertos";

        var erro = Assert.Throws<FonteResultadosException>(() => Ler(resposta));

        Assert.Equal(FormatoInesperado3808, erro.Message);
    }

    /// <summary>
    /// Resposta com número diferente do pedido é rejeitada.
    /// </summary>
    [Fact]
    public void NumeroDiferenteDoPedido()
    {
        var erro = Assert.Throws<FonteResultadosException>(
            () => LeitorConcursoJson.LerConcurso(RespostaConcursoCaixa.ComNumero(3808), 3809));

        Assert.Equal("A Caixa devolveu o concurso 3808 ao buscar o concurso 3809.", erro.Message);
    }

    /// <summary>
    /// Rateio vazio indica concurso ainda sem rateio divulgado.
    /// </summary>
    [Fact]
    public void RateioVazio()
    {
        JObject resposta = RespostaConcursoCaixa.LerAmostra();
        resposta["listaRateioPremio"] = new JArray();

        var erro = Assert.Throws<FonteResultadosException>(() => Ler(resposta));

        Assert.Equal(
            "O concurso 3808 já foi sorteado, mas a Caixa ainda não divulgou o rateio. Tente atualizar mais tarde.",
            erro.Message);
    }

    /// <summary>
    /// Todas as faixas sem ganhadores também indicam rateio ainda não divulgado.
    /// </summary>
    [Fact]
    public void RateioTodoZerado()
    {
        JObject resposta = RespostaConcursoCaixa.LerAmostra();
        foreach (JToken faixa in resposta["listaRateioPremio"]!)
        {
            faixa["numeroDeGanhadores"] = 0;
            faixa["valorPremio"] = 0;
        }

        var erro = Assert.Throws<FonteResultadosException>(() => Ler(resposta));

        Assert.StartsWith("O concurso 3808 já foi sorteado, mas a Caixa ainda não divulgou o rateio.", erro.Message);
    }

    /// <summary>
    /// Conteúdo que não é JSON indica formato inesperado.
    /// </summary>
    [Fact]
    public void ConteudoQueNaoEJson()
    {
        byte[] html = Encoding.UTF8.GetBytes("<html><body>Manutenção</body></html>");

        var erro = Assert.Throws<FonteResultadosException>(() => LeitorConcursoJson.LerConcurso(html, 3808));

        Assert.Equal(FormatoInesperado3808, erro.Message);
    }

    /// <summary>
    /// Campo obrigatório ausente ou com tipo errado indica formato inesperado.
    /// </summary>
    /// <param name="campo">Campo alterado.</param>
    /// <param name="remover">Se o campo é removido em vez de receber um texto inválido.</param>
    [Theory]
    [InlineData("valorArrecadado", true)]
    [InlineData("listaDezenas", true)]
    [InlineData("dataApuracao", true)]
    [InlineData("valorArrecadado", false)]
    [InlineData("dataApuracao", false)]
    public void CampoAusenteOuInvalido(string campo, bool remover)
    {
        JObject resposta = RespostaConcursoCaixa.LerAmostra();
        if (remover)
        {
            resposta.Remove(campo);
        }
        else
        {
            resposta[campo] = "abc";
        }

        var erro = Assert.Throws<FonteResultadosException>(() => Ler(resposta));

        Assert.Equal(FormatoInesperado3808, erro.Message);
    }

    /// <summary>
    /// Concurso que não atende às regras do domínio informa o número e o motivo, sem o nome do parâmetro.
    /// </summary>
    [Fact]
    public void RegraDoDominio()
    {
        JObject resposta = RespostaConcursoCaixa.LerAmostra();
        resposta["listaDezenas"]![1] = "01";

        var erro = Assert.Throws<FonteResultadosException>(() => Ler(resposta));

        Assert.Equal("Concurso 3808: A dezena 01 foi informada mais de uma vez.", erro.Message);
    }

    /// <summary>
    /// Lê o concurso 3808 a partir do JSON informado.
    /// </summary>
    /// <param name="resposta">JSON da resposta.</param>
    /// <returns>Concurso lido.</returns>
    private static Concurso Ler(JObject resposta)
    {
        return LeitorConcursoJson.LerConcurso(RespostaConcursoCaixa.EmBytes(resposta), 3808);
    }
}
