// <copyright file="ConcursoTestes.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Dominio.Concursos;

namespace Lotofacil.Testes.Unidade.Dominio;

/// <summary>
/// Testes dos dados e das regras de validade do concurso.
/// </summary>
public class ConcursoTestes
{
    /// <summary>
    /// Todos os dados informados ficam disponíveis no concurso.
    /// </summary>
    [Fact]
    public void ConcursoCompletoMantemOsDados()
    {
        int[] ordem = [13, 2, 24, 7, 20, 11, 1, 25, 18, 5, 22, 14, 10, 21, 4];

        Concurso concurso = new(
            3807,
            new DateOnly(2026, 10, 17),
            ConcursoFabrica.DezenasPadrao,
            ordem,
            ConcursoFabrica.CriarRateios(ganhadores15: 2, premio15: 801_455.92m),
            24_873_112.50m,
            1_000.10m,
            1_800_000m,
            120_500_300.25m);

        Assert.Equal(3807, concurso.Numero);
        Assert.Equal(new DateOnly(2026, 10, 17), concurso.Data);
        Assert.Equal(ConcursoFabrica.DezenasPadrao, concurso.Dezenas);
        Assert.Equal(ordem, concurso.OrdemSorteio);
        Assert.Equal([11, 12, 13, 14, 15], concurso.Rateios.Select(rateio => rateio.Faixa));
        Assert.Equal(2, concurso.ObterRateio(15).Ganhadores);
        Assert.Equal(801_455.92m, concurso.ObterRateio(15).ValorPremio);
        Assert.Equal(24_873_112.50m, concurso.Arrecadacao);
        Assert.Equal(1_000.10m, concurso.Acumulado15);
        Assert.Equal(1_800_000m, concurso.EstimativaProximo);
        Assert.Equal(120_500_300.25m, concurso.AcumuladoIndependencia);
    }

    /// <summary>
    /// Dezenas informadas fora de ordem ficam em ordem crescente.
    /// </summary>
    [Fact]
    public void DezenasFicamEmOrdemCrescente()
    {
        Concurso concurso = ConcursoFabrica.Criar(dezenas: [25, 3, 14, 1, 2, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13]);

        Assert.Equal([1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 25], concurso.Dezenas);
    }

    /// <summary>
    /// Sem a ordem do sorteio o concurso é válido e indica que ela não é conhecida.
    /// </summary>
    [Fact]
    public void ConcursoSemOrdemDoSorteioEhValido()
    {
        Concurso concurso = ConcursoFabrica.Criar(ordemSorteio: null);

        Assert.Null(concurso.OrdemSorteio);
    }

    /// <summary>
    /// Número zero ou negativo é rejeitado.
    /// </summary>
    /// <param name="numero">Número inválido.</param>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void NumeroDeveSerMaiorQueZero(int numero)
    {
        var erro = Assert.ThrowsAny<ArgumentException>(() => ConcursoFabrica.Criar(numero: numero));

        Assert.Contains("O número do concurso deve ser maior que zero.", erro.Message);
    }

    /// <summary>
    /// Quantidade de dezenas diferente de 15 é rejeitada.
    /// </summary>
    /// <param name="quantidade">Quantidade de dezenas informada.</param>
    [Theory]
    [InlineData(14)]
    [InlineData(16)]
    public void ExigeQuinzeDezenas(int quantidade)
    {
        var erro = Assert.ThrowsAny<ArgumentException>(
            () => ConcursoFabrica.Criar(dezenas: Enumerable.Range(1, quantidade)));

        Assert.Contains($"O concurso deve ter 15 dezenas; foram informadas {quantidade}.", erro.Message);
    }

    /// <summary>
    /// Dezena repetida é rejeitada informando qual é.
    /// </summary>
    [Fact]
    public void RejeitaDezenaRepetida()
    {
        int[] dezenas = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 7];

        var erro = Assert.ThrowsAny<ArgumentException>(() => ConcursoFabrica.Criar(dezenas: dezenas));

        Assert.Contains("A dezena 07 foi informada mais de uma vez.", erro.Message);
    }

    /// <summary>
    /// Dezena fora do intervalo de 1 a 25 é rejeitada.
    /// </summary>
    /// <param name="dezenaInvalida">Dezena fora do volante.</param>
    [Theory]
    [InlineData(0)]
    [InlineData(26)]
    public void RejeitaDezenaForaDoIntervalo(int dezenaInvalida)
    {
        int[] dezenas = [dezenaInvalida, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15];

        var erro = Assert.ThrowsAny<ArgumentException>(() => ConcursoFabrica.Criar(dezenas: dezenas));

        Assert.Contains($"As dezenas vão de 1 a 25; foi informada a dezena {dezenaInvalida}.", erro.Message);
    }

    /// <summary>
    /// Ordem do sorteio com dezena que não foi sorteada, ou com dezenas faltando, é rejeitada.
    /// </summary>
    /// <param name="ordem">Ordem do sorteio inválida.</param>
    [Theory]
    [InlineData(new[] { 3, 2, 24, 7, 20, 11, 1, 25, 18, 5, 22, 14, 10, 21, 4 })]
    [InlineData(new[] { 13, 2, 24, 7, 20, 11, 1, 25, 18, 5, 22, 14, 10, 21 })]
    [InlineData(new[] { 13, 2, 24, 7, 20, 11, 1, 25, 18, 5, 22, 14, 10, 21, 4, 4 })]
    public void RejeitaOrdemDoSorteioComDezenasDiferentes(int[] ordem)
    {
        var erro = Assert.ThrowsAny<ArgumentException>(() => ConcursoFabrica.Criar(ordemSorteio: ordem));

        Assert.Contains("A ordem do sorteio não corresponde às dezenas sorteadas.", erro.Message);
    }

    /// <summary>
    /// Um concurso gravado sem ordem passa a ter a ordem definida, sem mudar as dezenas.
    /// </summary>
    [Fact]
    public void DefineAOrdemDoSorteioDepoisDeCriado()
    {
        int[] ordem = [13, 2, 24, 7, 20, 11, 1, 25, 18, 5, 22, 14, 10, 21, 4];
        Concurso concurso = ConcursoFabrica.Criar(ordemSorteio: null);

        concurso.DefinirOrdemSorteio(ordem);

        Assert.Equal(ordem, concurso.OrdemSorteio);
        Assert.Equal(ConcursoFabrica.DezenasPadrao, concurso.Dezenas);
    }

    /// <summary>
    /// Definir uma ordem com dezenas diferentes das sorteadas é rejeitado e o concurso continua sem ordem.
    /// </summary>
    /// <param name="ordem">Ordem do sorteio inválida.</param>
    [Theory]
    [InlineData(new[] { 3, 2, 24, 7, 20, 11, 1, 25, 18, 5, 22, 14, 10, 21, 4 })]
    [InlineData(new[] { 13, 2, 24, 7, 20, 11, 1, 25, 18, 5, 22, 14, 10, 21 })]
    public void RejeitaDefinirOrdemComDezenasDiferentes(int[] ordem)
    {
        Concurso concurso = ConcursoFabrica.Criar(ordemSorteio: null);

        var erro = Assert.ThrowsAny<ArgumentException>(() => concurso.DefinirOrdemSorteio(ordem));

        Assert.Contains("A ordem do sorteio não corresponde às dezenas sorteadas.", erro.Message);
        Assert.Null(concurso.OrdemSorteio);
    }

    /// <summary>
    /// Rateio sem uma das faixas é rejeitado.
    /// </summary>
    [Fact]
    public void RejeitaRateioSemFaixa()
    {
        List<Rateio> rateios = ConcursoFabrica.CriarRateios();
        rateios.RemoveAll(rateio => rateio.Faixa == 13);

        var erro = Assert.ThrowsAny<ArgumentException>(() => ConcursoFabrica.Criar(rateios: rateios));

        Assert.Contains("O rateio deve ter exatamente uma entrada para cada faixa de 11 a 15 acertos.", erro.Message);
    }

    /// <summary>
    /// Rateio com a mesma faixa duas vezes é rejeitado.
    /// </summary>
    [Fact]
    public void RejeitaRateioComFaixaRepetida()
    {
        List<Rateio> rateios = ConcursoFabrica.CriarRateios();
        rateios.Add(new Rateio(14, 1, 10m));

        var erro = Assert.ThrowsAny<ArgumentException>(() => ConcursoFabrica.Criar(rateios: rateios));

        Assert.Contains("O rateio deve ter exatamente uma entrada para cada faixa de 11 a 15 acertos.", erro.Message);
    }

    /// <summary>
    /// Faixa fora de 11 a 15 é rejeitada.
    /// </summary>
    /// <param name="faixa">Faixa inválida.</param>
    [Theory]
    [InlineData(10)]
    [InlineData(16)]
    public void RejeitaFaixaForaDoIntervalo(int faixa)
    {
        var erro = Assert.ThrowsAny<ArgumentException>(() => new Rateio(faixa, 1, 10m));

        Assert.Contains($"A faixa de premiação deve estar entre 11 e 15 acertos; foi informada a faixa {faixa}.", erro.Message);
    }

    /// <summary>
    /// Quantidade de ganhadores negativa é rejeitada.
    /// </summary>
    [Fact]
    public void RejeitaGanhadoresNegativos()
    {
        var erro = Assert.ThrowsAny<ArgumentException>(() => new Rateio(14, -1, 10m));

        Assert.Contains("A quantidade de ganhadores da faixa de 14 acertos não pode ser negativa.", erro.Message);
    }

    /// <summary>
    /// Prêmio negativo é rejeitado.
    /// </summary>
    [Fact]
    public void RejeitaPremioNegativo()
    {
        var erro = Assert.ThrowsAny<ArgumentException>(() => new Rateio(12, 10, -0.01m));

        Assert.Contains("O prêmio da faixa de 12 acertos não pode ser negativo.", erro.Message);
    }

    /// <summary>
    /// Cada valor monetário do concurso negativo é rejeitado com a mensagem que o identifica.
    /// </summary>
    /// <param name="campo">Valor que recebe o número negativo.</param>
    /// <param name="mensagem">Mensagem esperada.</param>
    [Theory]
    [InlineData("arrecadacao", "O valor da arrecadação não pode ser negativo.")]
    [InlineData("acumulado15", "O valor acumulado da faixa de 15 acertos não pode ser negativo.")]
    [InlineData("estimativaProximo", "O valor da estimativa do próximo concurso não pode ser negativo.")]
    [InlineData("acumuladoIndependencia", "O valor acumulado da Lotofácil da Independência não pode ser negativo.")]
    public void RejeitaValorNegativo(string campo, string mensagem)
    {
        const decimal Negativo = -1m;

        var erro = Assert.ThrowsAny<ArgumentException>(() => ConcursoFabrica.Criar(
            arrecadacao: campo == "arrecadacao" ? Negativo : 0m,
            acumulado15: campo == "acumulado15" ? Negativo : 0m,
            estimativaProximo: campo == "estimativaProximo" ? Negativo : 0m,
            acumuladoIndependencia: campo == "acumuladoIndependencia" ? Negativo : 0m));

        Assert.Contains(mensagem, erro.Message);
        Assert.Equal(campo, erro.ParamName);
    }

    /// <summary>
    /// Concurso antigo, com valores zerados, é válido.
    /// </summary>
    [Fact]
    public void AceitaValoresZerados()
    {
        Concurso concurso = ConcursoFabrica.Criar(
            arrecadacao: 0m,
            acumulado15: 0m,
            estimativaProximo: 0m,
            acumuladoIndependencia: 0m);

        Assert.Equal(0m, concurso.Arrecadacao);
        Assert.Equal(0m, concurso.EstimativaProximo);
    }
}
