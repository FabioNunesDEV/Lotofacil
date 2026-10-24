// <copyright file="FiltrosGeracaoTestes.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Dominio.Estatisticas.Perfis;
using Lotofacil.Dominio.Geracao;

namespace Lotofacil.Testes.Unidade.Dominio;

/// <summary>
/// Testes dos filtros da geração.
/// </summary>
public class FiltrosGeracaoTestes
{
    /// <summary>
    /// Último resultado usado nas repetidas: 11 a 25.
    /// </summary>
    private static readonly List<int> UltimoResultado = Enumerable.Range(11, 15).ToList();

    /// <summary>
    /// A maior sequência conta as dezenas consecutivas, em qualquer ordem de entrada.
    /// </summary>
    [Fact]
    public void CalculaAMaiorSequencia()
    {
        Assert.Equal(15, FiltrosGeracao.MaiorSequencia(Enumerable.Range(1, 15)));
        Assert.Equal(1, FiltrosGeracao.MaiorSequencia([1, 3, 5, 7, 9, 11, 13, 15, 17, 19, 21, 23, 25]));
        Assert.Equal(3, FiltrosGeracao.MaiorSequencia([11, 5, 4, 10, 6]));
        Assert.Equal(0, FiltrosGeracao.MaiorSequencia([]));
    }

    /// <summary>
    /// Um lado vazio não limita; os dois lados são inclusivos.
    /// </summary>
    [Fact]
    public void LimiteComUmSoLado()
    {
        var soMinimo = new LimitePerfil(TipoPerfil.Pares, 7, null);
        var soMaximo = new LimitePerfil(TipoPerfil.Pares, null, 8);
        var dois = new LimitePerfil(TipoPerfil.Pares, 7, 8);

        Assert.False(soMinimo.Contem(6));
        Assert.True(soMinimo.Contem(7));
        Assert.True(soMinimo.Contem(12));
        Assert.True(soMaximo.Contem(0));
        Assert.False(soMaximo.Contem(9));
        Assert.True(dois.Contem(8));
        Assert.False(dois.Invertido);
        Assert.True(new LimitePerfil(TipoPerfil.Soma, 210, 180).Invertido);
        Assert.False(new LimitePerfil(TipoPerfil.Soma, null, null).Ligado);
    }

    /// <summary>
    /// Só os limites preenchidos ficam, na ordem dos perfis, e a sequência conta como filtro ligado.
    /// </summary>
    [Fact]
    public void GuardaOsLimitesLigados()
    {
        var filtros = new FiltrosGeracao(
            [new LimitePerfil(TipoPerfil.Soma, null, 209), new LimitePerfil(TipoPerfil.Primos, null, null), new LimitePerfil(TipoPerfil.Pares, 7, 8)],
            9);

        Assert.Equal([TipoPerfil.Pares, TipoPerfil.Soma], filtros.Limites.Select(limite => limite.Tipo));
        Assert.Equal(3, filtros.Ligados);
        Assert.Equal(0, FiltrosGeracao.Nenhum.Ligados);
        Assert.Throws<ArgumentException>(() => new FiltrosGeracao(
            [new LimitePerfil(TipoPerfil.Pares, 7, null), new LimitePerfil(TipoPerfil.Pares, null, 8)], null));
    }

    /// <summary>
    /// O jogo 01 a 15 tem 7 pares, soma 120, 5 repetidas de 11 a 25 e sequência de 15.
    /// </summary>
    [Fact]
    public void AtendeAosLimitesEASequencia()
    {
        List<int> jogo = Enumerable.Range(1, 15).ToList();

        Assert.True(FiltrosGeracao.Nenhum.Atende(jogo, UltimoResultado));
        Assert.True(Filtro(TipoPerfil.Pares, 7, 8).Atende(jogo, UltimoResultado));
        Assert.False(Filtro(TipoPerfil.Pares, 8, null).Atende(jogo, UltimoResultado));
        Assert.True(Filtro(TipoPerfil.Soma, null, 120).Atende(jogo, UltimoResultado));
        Assert.True(Filtro(TipoPerfil.Repetidas, 5, 5).Atende(jogo, UltimoResultado));
        Assert.False(Filtro(TipoPerfil.Repetidas, 6, null).Atende(jogo, UltimoResultado));
        Assert.True(new FiltrosGeracao([], 15).Atende(jogo, UltimoResultado));
        Assert.False(new FiltrosGeracao([], 14).Atende(jogo, UltimoResultado));
    }

    /// <summary>
    /// Num jogo de 18 dezenas, o perfil conta todas as dezenas do jogo.
    /// </summary>
    [Fact]
    public void JogoDe18ContadoInteiro()
    {
        List<int> jogo = Enumerable.Range(1, 18).ToList();

        Assert.True(Filtro(TipoPerfil.Pares, 9, 9).Atende(jogo, UltimoResultado));
        Assert.True(Filtro(TipoPerfil.Soma, 171, 171).Atende(jogo, UltimoResultado));
    }

    /// <summary>
    /// As faixas dos limites dependem das dezenas por jogo.
    /// </summary>
    [Fact]
    public void FaixasDosLimites()
    {
        Assert.Equal(120, FiltrosGeracao.MenorLimite(TipoPerfil.Soma, 15));
        Assert.Equal(270, FiltrosGeracao.MaiorLimite(TipoPerfil.Soma, 15));
        Assert.Equal(210, FiltrosGeracao.MenorLimite(TipoPerfil.Soma, 20));
        Assert.Equal(310, FiltrosGeracao.MaiorLimite(TipoPerfil.Soma, 20));
        Assert.Equal(0, FiltrosGeracao.MenorLimite(TipoPerfil.Pares, 18));
        Assert.Equal(18, FiltrosGeracao.MaiorLimite(TipoPerfil.Moldura, 18));
    }

    /// <summary>
    /// Cada perfil tem o nome usado nas mensagens.
    /// </summary>
    [Fact]
    public void NomesNasMensagens()
    {
        Assert.Equal("de pares", FiltrosGeracao.NomeNaMensagem(TipoPerfil.Pares));
        Assert.Equal("de soma", FiltrosGeracao.NomeNaMensagem(TipoPerfil.Soma));
        Assert.Equal("da moldura", FiltrosGeracao.NomeNaMensagem(TipoPerfil.Moldura));
        Assert.All(Enum.GetValues<TipoPerfil>(), tipo => Assert.NotEmpty(FiltrosGeracao.NomeNaMensagem(tipo)));
    }

    /// <summary>
    /// Trocar um limite mantém os outros e a sequência; limite vazio desliga o perfil.
    /// </summary>
    [Fact]
    public void TrocaLimitesESequencia()
    {
        FiltrosGeracao filtros = new FiltrosGeracao([new LimitePerfil(TipoPerfil.Soma, 180, 209)], 9)
            .ComLimite(new LimitePerfil(TipoPerfil.Pares, 7, null))
            .ComLimite(new LimitePerfil(TipoPerfil.Pares, 7, 8));

        Assert.Equal([new LimitePerfil(TipoPerfil.Pares, 7, 8), new LimitePerfil(TipoPerfil.Soma, 180, 209)], filtros.Limites);
        Assert.Equal(9, filtros.MaiorSequenciaPermitida);
        Assert.Equal(2, filtros.ComLimite(new LimitePerfil(TipoPerfil.Soma, null, null)).Ligados);
        Assert.Null(filtros.ComMaiorSequencia(null).MaiorSequenciaPermitida);
        Assert.Equal(2, filtros.ComMaiorSequencia(null).Limites.Count);
    }

    /// <summary>
    /// Ao trocar as dezenas por jogo, os limites voltam para a faixa possível.
    /// </summary>
    [Fact]
    public void AjustaParaAsDezenasPorJogo()
    {
        var filtros = new FiltrosGeracao(
            [new LimitePerfil(TipoPerfil.Pares, 16, 18), new LimitePerfil(TipoPerfil.Soma, 200, 300)], 18);

        FiltrosGeracao ajustados = filtros.AjustarPara(15);

        Assert.Equal(
            [new LimitePerfil(TipoPerfil.Pares, 15, 15), new LimitePerfil(TipoPerfil.Soma, 200, 270)],
            ajustados.Limites);
        Assert.Equal(15, ajustados.MaiorSequenciaPermitida);
        Assert.Equal(new LimitePerfil(TipoPerfil.Soma, 210, 300), filtros.AjustarPara(20).Limites[1]);
        Assert.Null(FiltrosGeracao.Nenhum.AjustarPara(20).MaiorSequenciaPermitida);
    }

    /// <summary>
    /// Cria filtros com um único limite.
    /// </summary>
    /// <param name="tipo">Perfil limitado.</param>
    /// <param name="minimo">Mínimo.</param>
    /// <param name="maximo">Máximo.</param>
    /// <returns>Os filtros.</returns>
    private static FiltrosGeracao Filtro(TipoPerfil tipo, int? minimo, int? maximo)
    {
        return new FiltrosGeracao([new LimitePerfil(tipo, minimo, maximo)], null);
    }
}
