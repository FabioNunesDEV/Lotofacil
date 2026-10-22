// <copyright file="JogoTestes.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Dominio.Jogos;

namespace Lotofacil.Testes.Unidade.Dominio;

/// <summary>
/// Testes das regras de um jogo.
/// </summary>
public class JogoTestes
{
    /// <summary>
    /// Momento de cadastro usado nos testes.
    /// </summary>
    private static readonly DateTime Cadastro = new(2026, 10, 20, 17, 0, 0);

    /// <summary>
    /// As dezenas informadas fora de ordem ficam em ordem crescente, com os demais dados preservados.
    /// </summary>
    [Fact]
    public void GuardaAsDezenasEmOrdem()
    {
        int[] dezenas = [25, 3, 14, 1, 2, 4, 5, 7, 10, 11, 13, 18, 20, 21, 22];

        var jogo = new Jogo(3809, dezenas, Cadastro);

        Assert.Equal(dezenas.Order(), jogo.Dezenas);
        Assert.Equal(3809, jogo.NumeroConcurso);
        Assert.Equal(Cadastro, jogo.CadastradoEm);
    }

    /// <summary>
    /// Jogo com menos de 15 ou mais de 20 dezenas é rejeitado.
    /// </summary>
    /// <param name="quantidade">Quantidade de dezenas.</param>
    [Theory]
    [InlineData(14)]
    [InlineData(21)]
    public void RejeitaQuantidadeForaDoIntervalo(int quantidade)
    {
        var erro = Assert.Throws<ArgumentException>(() => new Jogo(3809, Enumerable.Range(1, quantidade), Cadastro));

        Assert.StartsWith($"O jogo deve ter de 15 a 20 dezenas; foram informadas {quantidade}.", erro.Message);
    }

    /// <summary>
    /// Jogos de 15 a 20 dezenas são aceitos e valem C(dezenas, 15) apostas de 15 dezenas.
    /// </summary>
    /// <param name="quantidade">Quantidade de dezenas.</param>
    /// <param name="apostas">Apostas de 15 dezenas equivalentes.</param>
    [Theory]
    [InlineData(15, 1)]
    [InlineData(16, 16)]
    [InlineData(17, 136)]
    [InlineData(20, 15_504)]
    public void AceitaApostaMultipla(int quantidade, int apostas)
    {
        var jogo = new Jogo(3809, Enumerable.Range(1, quantidade), Cadastro);

        Assert.Equal(quantidade, jogo.Dezenas.Count);
        Assert.Equal(apostas, jogo.QuantidadeApostas);
    }

    /// <summary>
    /// Jogo com dezena repetida é rejeitado.
    /// </summary>
    [Fact]
    public void RejeitaDezenaRepetida()
    {
        int[] dezenas = [1, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14];

        Assert.Throws<ArgumentException>(() => new Jogo(3809, dezenas, Cadastro));
    }

    /// <summary>
    /// Jogo com dezena fora do volante é rejeitado.
    /// </summary>
    /// <param name="dezena">Dezena inválida.</param>
    [Theory]
    [InlineData(0)]
    [InlineData(26)]
    public void RejeitaDezenaForaDoVolante(int dezena)
    {
        int[] dezenas = [dezena, .. Enumerable.Range(2, 14)];

        Assert.Throws<ArgumentOutOfRangeException>(() => new Jogo(3809, dezenas, Cadastro));
    }

    /// <summary>
    /// Número de concurso menor que 1 é rejeitado.
    /// </summary>
    [Fact]
    public void RejeitaConcursoInvalido()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Jogo(0, Enumerable.Range(1, 15), Cadastro));
    }
}
