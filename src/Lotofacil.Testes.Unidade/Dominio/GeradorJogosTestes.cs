// <copyright file="GeradorJogosTestes.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Dominio.Estatisticas;
using Lotofacil.Dominio.Estatisticas.Perfis;
using Lotofacil.Dominio.Geracao;

namespace Lotofacil.Testes.Unidade.Dominio;

/// <summary>
/// Testes do gerador de jogos.
/// </summary>
public class GeradorJogosTestes
{
    /// <summary>
    /// Estatísticas em que a dezena d tem 100 + d sorteios e atraso 25 − d: peso d em Mais sorteadas e 26 − d em
    /// Mais atrasadas.
    /// </summary>
    private static readonly List<EstatisticaDezena> Estatisticas = Enumerable.Range(1, 25)
        .Select(dezena => new EstatisticaDezena(dezena, 100 + dezena, 0m, 0m, 0d, 25 - dezena, null))
        .ToList();

    /// <summary>
    /// Último resultado usado no filtro de repetidas: 11 a 25.
    /// </summary>
    private static readonly List<int> UltimoResultado = Enumerable.Range(11, 15).ToList();

    /// <summary>
    /// Gera a quantidade pedida de jogos distintos, cada um com as dezenas certas e em ordem.
    /// </summary>
    [Fact]
    public void GeraJogosDistintosComOTamanhoPedido()
    {
        ResultadoGeracao resultado = Gerar(new ParametrosGeracao(10, 15, EstrategiaGeracao.Aleatoria, [], []));

        Assert.Null(resultado.Recusa);
        Assert.Null(resultado.CombinacoesPossiveis);
        Assert.Equal(10, resultado.Jogos.Count);
        Assert.Equal(10, resultado.Jogos.Select(jogo => string.Join(',', jogo)).Distinct().Count());
        Assert.All(resultado.Jogos, jogo =>
        {
            Assert.Equal(15, jogo.Distinct().Count());
            Assert.All(jogo, dezena => Assert.InRange(dezena, 1, 25));
            Assert.Equal(jogo.Order(), jogo);
        });
    }

    /// <summary>
    /// Em todas as estratégias, as fixas estão em todos os jogos e as excluídas em nenhum.
    /// </summary>
    /// <param name="estrategia">Estratégia testada.</param>
    [Theory]
    [InlineData(EstrategiaGeracao.Aleatoria)]
    [InlineData(EstrategiaGeracao.MaisSorteadas)]
    [InlineData(EstrategiaGeracao.MenosSorteadas)]
    [InlineData(EstrategiaGeracao.MaisAtrasadas)]
    [InlineData(EstrategiaGeracao.Equilibrada)]
    public void RespeitaFixasEExcluidas(EstrategiaGeracao estrategia)
    {
        ResultadoGeracao resultado = Gerar(new ParametrosGeracao(50, 16, estrategia, [5, 13], [1, 25]));

        Assert.Equal(50, resultado.Jogos.Count);
        Assert.Equal(50, resultado.Jogos.Select(jogo => string.Join(',', jogo)).Distinct().Count());
        Assert.All(resultado.Jogos, jogo =>
        {
            Assert.Equal(16, jogo.Count);
            Assert.Contains(5, jogo);
            Assert.Contains(13, jogo);
            Assert.DoesNotContain(1, jogo);
            Assert.DoesNotContain(25, jogo);
        });
    }

    /// <summary>
    /// Com o jogo inteiro fixo, só há um jogo possível, e o resultado avisa.
    /// </summary>
    [Fact]
    public void JogoInteiroFixo()
    {
        ResultadoGeracao resultado = Gerar(new ParametrosGeracao(10, 15, EstrategiaGeracao.Aleatoria, Enumerable.Range(1, 15), []));

        Assert.Equal([Enumerable.Range(1, 15).ToList()], resultado.Jogos);
        Assert.Equal(1, resultado.CombinacoesPossiveis);
    }

    /// <summary>
    /// Com 13 fixas e 8 excluídas sobram 4 livres para 2 vagas: os 6 jogos possíveis são gerados, com aviso.
    /// </summary>
    [Fact]
    public void PoucasCombinacoesGeraTodas()
    {
        ResultadoGeracao resultado = Gerar(
            new ParametrosGeracao(10, 15, EstrategiaGeracao.MaisSorteadas, Enumerable.Range(1, 13), Enumerable.Range(14, 8)));

        Assert.Equal(6, resultado.CombinacoesPossiveis);
        Assert.Equal(
            ["22,23", "22,24", "22,25", "23,24", "23,25", "24,25"],
            resultado.Jogos.Select(jogo => string.Join(',', jogo.Skip(13))));
        Assert.All(resultado.Jogos, jogo => Assert.Equal(Enumerable.Range(1, 13), jogo.Take(13)));
    }

    /// <summary>
    /// Quando as possíveis são exatamente a quantidade pedida, gera todas sem aviso.
    /// </summary>
    [Fact]
    public void PossiveisIguaisAQuantidadeSemAviso()
    {
        ResultadoGeracao resultado = Gerar(
            new ParametrosGeracao(6, 15, EstrategiaGeracao.Aleatoria, Enumerable.Range(1, 13), Enumerable.Range(14, 8)));

        Assert.Equal(6, resultado.Jogos.Count);
        Assert.Null(resultado.CombinacoesPossiveis);
    }

    /// <summary>
    /// Mais fixas que dezenas por jogo é recusado.
    /// </summary>
    [Fact]
    public void RecusaFixasDemais()
    {
        ResultadoGeracao resultado = Gerar(new ParametrosGeracao(10, 15, EstrategiaGeracao.Aleatoria, Enumerable.Range(1, 16), []));

        Assert.Empty(resultado.Jogos);
        Assert.Equal("Há 16 dezenas fixas, mais que as 15 dezenas por jogo.", resultado.Recusa);
    }

    /// <summary>
    /// Livres insuficientes para completar o jogo é recusado.
    /// </summary>
    [Fact]
    public void RecusaLivresInsuficientes()
    {
        ResultadoGeracao resultado = Gerar(
            new ParametrosGeracao(10, 15, EstrategiaGeracao.Aleatoria, Enumerable.Range(1, 5), Enumerable.Range(6, 12)));

        Assert.Empty(resultado.Jogos);
        Assert.Equal(
            "Faltam dezenas livres: com 5 fixas e 12 excluídas sobram 8 livres, e cada jogo precisa de mais 10.",
            resultado.Recusa);
    }

    /// <summary>
    /// Na Equilibrada com 3 fixas, as 12 dezenas a sortear são 6 pelos pesos de Mais sorteadas e 6 pelos de Mais
    /// atrasadas: os totais de peso de cada sorteio mostram qual tabela foi usada.
    /// </summary>
    [Fact]
    public void EquilibradaDivideAsDezenasLivres()
    {
        var aleatorio = new AleatorioZero();
        var gerador = new GeradorJogos(aleatorio);

        gerador.Gerar(new ParametrosGeracao(1, 15, EstrategiaGeracao.Equilibrada, [23, 24, 25], []), Estatisticas, UltimoResultado);

        // O sorteio fixo em zero escolhe sempre a menor dezena disponível: 1 a 6 pelos pesos d, 7 a 12 pelos 26 − d.
        List<int> esperados = [];
        for (int primeira = 1; primeira <= 6; primeira++)
        {
            esperados.Add(Enumerable.Range(primeira, 23 - primeira).Sum());
        }

        for (int primeira = 7; primeira <= 12; primeira++)
        {
            esperados.Add(Enumerable.Range(primeira, 23 - primeira).Sum(dezena => 26 - dezena));
        }

        Assert.Equal(esperados, aleatorio.Maximos);
    }

    /// <summary>
    /// Em Mais sorteadas, a dezena de peso 25 sai muito mais que a de peso 1.
    /// </summary>
    [Fact]
    public void PesoMaiorSaiMais()
    {
        var gerador = new GeradorJogos(new Random(42));

        ResultadoGeracao resultado = gerador.Gerar(new ParametrosGeracao(100, 15, EstrategiaGeracao.MaisSorteadas, [], []), Estatisticas, UltimoResultado);

        int comAMaisSorteada = resultado.Jogos.Count(jogo => jogo.Contains(25));
        int comAMenosSorteada = resultado.Jogos.Count(jogo => jogo.Contains(1));
        Assert.True(comAMaisSorteada > comAMenosSorteada * 3, $"25 em {comAMaisSorteada} jogos; 01 em {comAMenosSorteada}.");
    }

    /// <summary>
    /// Parâmetros fora das faixas são rejeitados.
    /// </summary>
    [Fact]
    public void RejeitaParametrosForaDaFaixa()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ParametrosGeracao(0, 15, EstrategiaGeracao.Aleatoria, [], []));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ParametrosGeracao(101, 15, EstrategiaGeracao.Aleatoria, [], []));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ParametrosGeracao(10, 14, EstrategiaGeracao.Aleatoria, [], []));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ParametrosGeracao(10, 21, EstrategiaGeracao.Aleatoria, [], []));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ParametrosGeracao(10, 15, EstrategiaGeracao.Aleatoria, [26], []));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ParametrosGeracao(10, 15, (EstrategiaGeracao)99, [], []));
        Assert.Throws<ArgumentException>(() => new ParametrosGeracao(10, 15, EstrategiaGeracao.Aleatoria, [3, 3], []));
        Assert.Throws<ArgumentException>(() => new ParametrosGeracao(10, 15, EstrategiaGeracao.Aleatoria, [7], [7]));
    }

    /// <summary>
    /// Limites fora da faixa possível para o tamanho do jogo são rejeitados; mínimo maior que o máximo é aceito.
    /// </summary>
    [Fact]
    public void ValidaAsFaixasDosFiltros()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ComFiltros(15, Limite(TipoPerfil.Pares, null, 16)));
        Assert.Throws<ArgumentOutOfRangeException>(() => ComFiltros(15, Limite(TipoPerfil.Pares, -1, null)));
        Assert.Throws<ArgumentOutOfRangeException>(() => ComFiltros(15, Limite(TipoPerfil.Soma, 119, null)));
        Assert.Throws<ArgumentOutOfRangeException>(() => ComFiltros(15, Limite(TipoPerfil.Soma, null, 271)));
        Assert.Throws<ArgumentOutOfRangeException>(() => ComFiltros(15, new FiltrosGeracao([], 0)));
        Assert.Throws<ArgumentOutOfRangeException>(() => ComFiltros(15, new FiltrosGeracao([], 16)));

        Assert.Equal(1, ComFiltros(20, Limite(TipoPerfil.Soma, 210, 310)).Filtros.Ligados);
        Assert.Equal(1, ComFiltros(18, Limite(TipoPerfil.Pares, 18, null)).Filtros.Ligados);
        Assert.Equal(1, ComFiltros(15, Limite(TipoPerfil.Soma, 210, 180)).Filtros.Ligados);
        Assert.Same(FiltrosGeracao.Nenhum, new ParametrosGeracao(10, 15, EstrategiaGeracao.Aleatoria, [], []).Filtros);
    }

    /// <summary>
    /// Todos os jogos atendem a pares de 7 a 8 e soma de 180 a 209.
    /// </summary>
    [Fact]
    public void JogosAtendemAosLimites()
    {
        var filtros = new FiltrosGeracao(
            [new LimitePerfil(TipoPerfil.Pares, 7, 8), new LimitePerfil(TipoPerfil.Soma, 180, 209)], null);

        ResultadoGeracao resultado = Gerar(new ParametrosGeracao(30, 15, EstrategiaGeracao.MaisSorteadas, [], [], filtros));

        Assert.Equal(30, resultado.Jogos.Count);
        Assert.True(resultado.ComFiltros);
        Assert.Null(resultado.JogosPedidos);
        Assert.All(resultado.Jogos, jogo =>
        {
            Assert.InRange(jogo.Count(dezena => dezena % 2 == 0), 7, 8);
            Assert.InRange(jogo.Sum(), 180, 209);
        });
    }

    /// <summary>
    /// O filtro de repetidas conta as dezenas do último resultado; em jogos de 18 conta o jogo inteiro.
    /// </summary>
    [Fact]
    public void RepetidasEmJogosDe18()
    {
        ResultadoGeracao resultado = Gerar(ComFiltros(18, Limite(TipoPerfil.Repetidas, 8, 10)));

        Assert.Equal(10, resultado.Jogos.Count);
        Assert.All(resultado.Jogos, jogo =>
        {
            Assert.Equal(18, jogo.Count);
            Assert.InRange(jogo.Count(UltimoResultado.Contains), 8, 10);
        });
    }

    /// <summary>
    /// Nenhum jogo passa da maior sequência permitida.
    /// </summary>
    /// <param name="permitida">Maior sequência permitida.</param>
    [Theory]
    [InlineData(9)]
    [InlineData(4)]
    public void RespeitaAMaiorSequencia(int permitida)
    {
        ResultadoGeracao resultado = Gerar(ComFiltros(15, new FiltrosGeracao([], permitida)));

        Assert.Equal(10, resultado.Jogos.Count);
        Assert.All(resultado.Jogos, jogo => Assert.True(FiltrosGeracao.MaiorSequencia(jogo) <= permitida));
    }

    /// <summary>
    /// Fixas de 01 a 10 já formam uma sequência de 10: com limite 9, nenhum jogo atende.
    /// </summary>
    [Fact]
    public void FixasQueFormamASequenciaSaoRecusadas()
    {
        ResultadoGeracao resultado = Gerar(
            new ParametrosGeracao(10, 15, EstrategiaGeracao.Aleatoria, Enumerable.Range(1, 10), [], new FiltrosGeracao([], 9)));

        Assert.Empty(resultado.Jogos);
        Assert.Equal(ResultadoGeracao.MensagemNenhumJogo, resultado.Recusa);
    }

    /// <summary>
    /// O volante só tem 12 pares: mínimo de 13 pares em jogos de 15 é recusado.
    /// </summary>
    [Fact]
    public void FiltroImpossivelERecusado()
    {
        ResultadoGeracao resultado = Gerar(ComFiltros(15, Limite(TipoPerfil.Pares, 13, null)));

        Assert.Empty(resultado.Jogos);
        Assert.Equal("Nenhum jogo atendeu aos filtros. Afrouxe os limites ou revise as fixas e excluídas.", resultado.Recusa);
    }

    /// <summary>
    /// Mínimo maior que o máximo recusa com o nome do filtro.
    /// </summary>
    [Fact]
    public void MinimoMaiorQueMaximoERecusado()
    {
        ResultadoGeracao resultado = Gerar(ComFiltros(15, Limite(TipoPerfil.Soma, 210, 180)));

        Assert.Empty(resultado.Jogos);
        Assert.Equal("No filtro de soma, o mínimo (210) é maior que o máximo (180).", resultado.Recusa);
    }

    /// <summary>
    /// Com 13 fixas e 8 excluídas há 6 jogos possíveis; só 2 têm soma de pelo menos 139.
    /// </summary>
    [Fact]
    public void PoucasCombinacoesComFiltros()
    {
        ResultadoGeracao resultado = Gerar(new ParametrosGeracao(
            10, 15, EstrategiaGeracao.Aleatoria, Enumerable.Range(1, 13), Enumerable.Range(14, 8), Limite(TipoPerfil.Soma, 139, null)));

        Assert.Equal(2, resultado.CombinacoesPossiveis);
        Assert.True(resultado.ComFiltros);
        Assert.Equal(["23,25", "24,25"], resultado.Jogos.Select(jogo => string.Join(',', jogo.Skip(13))));
    }

    /// <summary>
    /// Sem os pares de 02 a 16, sobram 136 jogos possíveis e só 6 têm no máximo 2 pares: o sorteio acha os 6 e
    /// informa a quantidade pedida.
    /// </summary>
    [Fact]
    public void FiltroQuaseImpossivelGeraMenosJogos()
    {
        ResultadoGeracao resultado = Gerar(new ParametrosGeracao(
            100, 15, EstrategiaGeracao.Aleatoria, [], [2, 4, 6, 8, 10, 12, 14, 16], Limite(TipoPerfil.Pares, null, 2)));

        Assert.Equal(6, resultado.Jogos.Count);
        Assert.Equal(100, resultado.JogosPedidos);
        Assert.Null(resultado.CombinacoesPossiveis);
    }

    /// <summary>
    /// Sem filtros, o resultado não marca filtros nem quantidade pedida.
    /// </summary>
    [Fact]
    public void SemFiltrosNaoMarcaFiltros()
    {
        ResultadoGeracao resultado = Gerar(new ParametrosGeracao(10, 15, EstrategiaGeracao.Aleatoria, [], []));

        Assert.False(resultado.ComFiltros);
        Assert.Null(resultado.JogosPedidos);
    }

    /// <summary>
    /// Cria filtros com um único limite.
    /// </summary>
    /// <param name="tipo">Perfil limitado.</param>
    /// <param name="minimo">Mínimo.</param>
    /// <param name="maximo">Máximo.</param>
    /// <returns>Os filtros.</returns>
    private static FiltrosGeracao Limite(TipoPerfil tipo, int? minimo, int? maximo)
    {
        return new FiltrosGeracao([new LimitePerfil(tipo, minimo, maximo)], null);
    }

    /// <summary>
    /// Cria parâmetros de 10 jogos aleatórios, sem fixas nem excluídas, com os filtros.
    /// </summary>
    /// <param name="dezenasPorJogo">Dezenas de cada jogo.</param>
    /// <param name="filtros">Filtros da geração.</param>
    /// <returns>Os parâmetros.</returns>
    private static ParametrosGeracao ComFiltros(int dezenasPorJogo, FiltrosGeracao filtros)
    {
        return new ParametrosGeracao(10, dezenasPorJogo, EstrategiaGeracao.Aleatoria, [], [], filtros);
    }

    /// <summary>
    /// Gera com um aleatório de semente fixa e as estatísticas de teste.
    /// </summary>
    /// <param name="parametros">Parâmetros da geração.</param>
    /// <returns>Resultado da geração.</returns>
    private static ResultadoGeracao Gerar(ParametrosGeracao parametros)
    {
        return new GeradorJogos(new Random(7)).Gerar(parametros, Estatisticas, UltimoResultado);
    }
}
