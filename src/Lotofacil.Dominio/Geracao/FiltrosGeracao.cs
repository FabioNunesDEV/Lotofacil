// <copyright file="FiltrosGeracao.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Dominio.Estatisticas.Perfis;

namespace Lotofacil.Dominio.Geracao;

/// <summary>
/// Filtros opcionais da geração: limites por perfil e maior sequência de dezenas seguidas permitida.
/// </summary>
public sealed class FiltrosGeracao
{
    /// <summary>
    /// Maior sequência sugerida ao ligar "Evitar padrões populares": recusa só sequências de 10 ou mais dezenas,
    /// cerca de 1% das combinações de 15.
    /// </summary>
    public const int MaiorSequenciaPadrao = 9;

    /// <summary>
    /// Filtros sem nada ligado.
    /// </summary>
    public static readonly FiltrosGeracao Nenhum = new([], null);

    /// <summary>
    /// Cria os filtros, guardando só os limites com algum lado preenchido, na ordem dos perfis.
    /// </summary>
    /// <param name="limites">Limites por perfil; no máximo um por perfil.</param>
    /// <param name="maiorSequenciaPermitida">
    /// Maior quantidade de dezenas seguidas aceita num jogo, ou <c>null</c> com "Evitar padrões populares" desligado.
    /// </param>
    public FiltrosGeracao(IEnumerable<LimitePerfil> limites, int? maiorSequenciaPermitida)
    {
        List<LimitePerfil> ligados = limites.Where(limite => limite.Ligado).OrderBy(limite => limite.Tipo).ToList();
        if (ligados.Select(limite => limite.Tipo).Distinct().Count() != ligados.Count)
        {
            throw new ArgumentException("Há mais de um limite para o mesmo perfil.", nameof(limites));
        }

        this.Limites = ligados;
        this.MaiorSequenciaPermitida = maiorSequenciaPermitida;
    }

    /// <summary>
    /// Limites ligados, na ordem dos perfis.
    /// </summary>
    public IReadOnlyList<LimitePerfil> Limites { get; }

    /// <summary>
    /// Maior quantidade de dezenas seguidas aceita, ou <c>null</c> sem esse filtro.
    /// </summary>
    public int? MaiorSequenciaPermitida { get; }

    /// <summary>
    /// Quantidade de filtros ligados, contando a maior sequência.
    /// </summary>
    public int Ligados => this.Limites.Count + (this.MaiorSequenciaPermitida is null ? 0 : 1);

    /// <summary>
    /// Menor limite aceito para o perfil num jogo de n dezenas: 0 nas quantidades, 1 + 2 + … + n na soma.
    /// </summary>
    /// <param name="tipo">Perfil.</param>
    /// <param name="dezenasPorJogo">Dezenas do jogo (n).</param>
    /// <returns>Menor valor possível do perfil.</returns>
    public static int MenorLimite(TipoPerfil tipo, int dezenasPorJogo)
    {
        return tipo == TipoPerfil.Soma ? dezenasPorJogo * (dezenasPorJogo + 1) / 2 : 0;
    }

    /// <summary>
    /// Maior limite aceito para o perfil num jogo de n dezenas: n nas quantidades, a soma das n maiores na soma.
    /// </summary>
    /// <param name="tipo">Perfil.</param>
    /// <param name="dezenasPorJogo">Dezenas do jogo (n).</param>
    /// <returns>Maior valor possível do perfil.</returns>
    public static int MaiorLimite(TipoPerfil tipo, int dezenasPorJogo)
    {
        if (tipo != TipoPerfil.Soma)
        {
            return dezenasPorJogo;
        }

        return dezenasPorJogo * ((2 * RegrasLotofacil.DezenaMaxima) - dezenasPorJogo + 1) / 2;
    }

    /// <summary>
    /// Calcula a maior quantidade de dezenas seguidas (consecutivas) de um jogo.
    /// </summary>
    /// <param name="dezenas">Dezenas do jogo.</param>
    /// <returns>Tamanho da maior sequência; zero sem dezenas.</returns>
    public static int MaiorSequencia(IEnumerable<int> dezenas)
    {
        int maior = 0;
        int atual = 0;
        int anterior = int.MinValue;
        foreach (int dezena in dezenas.Order())
        {
            atual = dezena == anterior + 1 ? atual + 1 : 1;
            maior = Math.Max(maior, atual);
            anterior = dezena;
        }

        return maior;
    }

    /// <summary>
    /// Nome do filtro usado nas mensagens ("No filtro de pares, ...").
    /// </summary>
    /// <param name="tipo">Perfil.</param>
    /// <returns>Nome com a preposição.</returns>
    public static string NomeNaMensagem(TipoPerfil tipo)
    {
        return tipo switch
        {
            TipoPerfil.Pares => "de pares",
            TipoPerfil.Primos => "de primos",
            TipoPerfil.MultiplosDe3 => "de múltiplos de 3",
            TipoPerfil.Fibonacci => "de Fibonacci",
            TipoPerfil.Soma => "de soma",
            TipoPerfil.Repetidas => "de repetidas",
            TipoPerfil.Moldura => "da moldura",
            _ => throw new ArgumentOutOfRangeException(nameof(tipo)),
        };
    }

    /// <summary>
    /// Cria filtros iguais a estes, com o limite do perfil trocado.
    /// </summary>
    /// <param name="limite">Novo limite do perfil; com os dois lados vazios, o perfil deixa de ser filtrado.</param>
    /// <returns>Os novos filtros.</returns>
    public FiltrosGeracao ComLimite(LimitePerfil limite)
    {
        IEnumerable<LimitePerfil> outros = this.Limites.Where(existente => existente.Tipo != limite.Tipo);

        return new FiltrosGeracao(outros.Append(limite), this.MaiorSequenciaPermitida);
    }

    /// <summary>
    /// Cria filtros iguais a estes, com a maior sequência permitida trocada.
    /// </summary>
    /// <param name="maiorSequenciaPermitida">Nova maior sequência, ou <c>null</c> para desligar o filtro.</param>
    /// <returns>Os novos filtros.</returns>
    public FiltrosGeracao ComMaiorSequencia(int? maiorSequenciaPermitida)
    {
        return new FiltrosGeracao(this.Limites, maiorSequenciaPermitida);
    }

    /// <summary>
    /// Cria filtros com cada limite trazido para a faixa possível de um jogo de n dezenas.
    /// </summary>
    /// <param name="dezenasPorJogo">Dezenas do jogo (n).</param>
    /// <returns>Os filtros ajustados.</returns>
    public FiltrosGeracao AjustarPara(int dezenasPorJogo)
    {
        List<LimitePerfil> ajustados = this.Limites
            .Select(limite =>
            {
                int menor = MenorLimite(limite.Tipo, dezenasPorJogo);
                int maior = MaiorLimite(limite.Tipo, dezenasPorJogo);
                return new LimitePerfil(limite.Tipo, Ajustar(limite.Minimo, menor, maior), Ajustar(limite.Maximo, menor, maior));
            })
            .ToList();

        return new FiltrosGeracao(ajustados, Ajustar(this.MaiorSequenciaPermitida, 1, dezenasPorJogo));
    }

    /// <summary>
    /// Indica se o jogo atende a todos os filtros ligados.
    /// </summary>
    /// <param name="jogo">Dezenas do jogo.</param>
    /// <param name="ultimoResultado">Dezenas do último concurso gravado, base das repetidas.</param>
    /// <returns><c>true</c> quando nenhum filtro recusa o jogo.</returns>
    public bool Atende(IReadOnlyCollection<int> jogo, IReadOnlyCollection<int> ultimoResultado)
    {
        if (this.MaiorSequenciaPermitida is int permitida && MaiorSequencia(jogo) > permitida)
        {
            return false;
        }

        return this.Limites.All(limite => limite.Contem(ValorPerfil.Calcular(limite.Tipo, jogo, ultimoResultado)));
    }

    /// <summary>
    /// Traz um valor opcional para dentro da faixa.
    /// </summary>
    /// <param name="valor">Valor, ou <c>null</c>.</param>
    /// <param name="menor">Menor valor aceito.</param>
    /// <param name="maior">Maior valor aceito.</param>
    /// <returns>O valor ajustado, ou <c>null</c>.</returns>
    private static int? Ajustar(int? valor, int menor, int maior)
    {
        return valor is int definido ? Math.Clamp(definido, menor, maior) : null;
    }
}
