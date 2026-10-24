// <copyright file="TeimosinhaTestes.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Dominio.Teimosinhas;

namespace Lotofacil.Testes.Unidade.Dominio;

/// <summary>
/// Testes das regras da Teimosinha.
/// </summary>
public class TeimosinhaTestes
{
    /// <summary>
    /// Dezenas 01 a 15.
    /// </summary>
    private static readonly int[] De01a15 = [.. Enumerable.Range(1, 15)];

    /// <summary>
    /// Dezenas 11 a 25.
    /// </summary>
    private static readonly int[] De11a25 = [.. Enumerable.Range(11, 15)];

    /// <summary>
    /// A Teimosinha nasce ativa, com o nome aparado e o primeiro concurso como último criado.
    /// </summary>
    [Fact]
    public void NasceAtivaComONomeAparado()
    {
        var teimosinha = new Teimosinha("  Minhas fixas ", 3813);

        Assert.Equal("Minhas fixas", teimosinha.Nome);
        Assert.True(teimosinha.Ativa);
        Assert.Equal(3813, teimosinha.UltimoConcursoCriado);
        Assert.Empty(teimosinha.Jogos);
        Assert.Null(teimosinha.Desde);
    }

    /// <summary>
    /// Nome vazio ou com mais de 40 caracteres é recusado; 40 é aceito.
    /// </summary>
    /// <param name="tamanho">Quantidade de letras do nome.</param>
    /// <param name="aceito">Se o nome deve ser aceito.</param>
    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(40, true)]
    [InlineData(41, false)]
    public void ValidaOTamanhoDoNome(int tamanho, bool aceito)
    {
        string nome = new('a', tamanho);

        if (aceito)
        {
            Assert.Equal(nome, Teimosinha.NormalizarNome(nome));
        }
        else
        {
            var erro = Assert.Throws<ArgumentException>(() => new Teimosinha($"  {nome}  ", 3813));
            Assert.StartsWith("O nome da Teimosinha deve ter de 1 a 40 caracteres.", erro.Message);
        }
    }

    /// <summary>
    /// A comparação de nome não diferencia maiúsculas nem considera espaços nas pontas.
    /// </summary>
    [Fact]
    public void ComparaONomeSemMaiusculas()
    {
        var teimosinha = new Teimosinha("Minhas fixas", 3813);

        Assert.True(teimosinha.NomeIgual(" minhas FIXAS "));
        Assert.False(teimosinha.NomeIgual("Minhas"));
    }

    /// <summary>
    /// Os jogos guardam as dezenas em ordem e o início; jogo repetido é recusado.
    /// </summary>
    [Fact]
    public void AcrescentaJogosERecusaRepetido()
    {
        var teimosinha = new Teimosinha("Minhas fixas", 3813);

        teimosinha.Acrescentar(De11a25.Reverse(), 3813);
        teimosinha.Acrescentar(De01a15, 3815);

        Assert.Equal(De11a25, teimosinha.Jogos[0].Dezenas);
        Assert.Equal(3813, teimosinha.Desde);
        Assert.True(teimosinha.Contem(De01a15.Reverse()));
        var erro = Assert.Throws<ArgumentException>(() => teimosinha.Acrescentar(De01a15, 3816));
        Assert.StartsWith("O jogo já faz parte da Teimosinha Minhas fixas.", erro.Message);
        Assert.Equal(2, teimosinha.Jogos.Count);
    }

    /// <summary>
    /// Dezenas fora das regras de um jogo são recusadas.
    /// </summary>
    [Fact]
    public void RecusaDezenasInvalidas()
    {
        var teimosinha = new Teimosinha("Minhas fixas", 3813);

        Assert.ThrowsAny<ArgumentException>(() => teimosinha.Acrescentar(Enumerable.Range(1, 14), 3813));
        Assert.Empty(teimosinha.Jogos);
    }

    /// <summary>
    /// Para um concurso, valem só os jogos com início até ele.
    /// </summary>
    [Fact]
    public void JogosAteOConcurso()
    {
        var teimosinha = new Teimosinha("Minhas fixas", 3813);
        teimosinha.Acrescentar(De01a15, 3813);
        teimosinha.Acrescentar(De11a25, 3816);

        Assert.Single(teimosinha.JogosAte(3815));
        Assert.Equal(2, teimosinha.JogosAte(3816).Count);
    }

    /// <summary>
    /// O último concurso criado só avança, e remover desativa.
    /// </summary>
    [Fact]
    public void RegistraCriacaoERemove()
    {
        var teimosinha = new Teimosinha("Minhas fixas", 3815);

        teimosinha.RegistrarCriacao(3814);
        Assert.Equal(3815, teimosinha.UltimoConcursoCriado);
        teimosinha.RegistrarCriacao(3817);
        Assert.Equal(3817, teimosinha.UltimoConcursoCriado);

        teimosinha.Remover();
        Assert.False(teimosinha.Ativa);
    }
}
