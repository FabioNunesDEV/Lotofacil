// <copyright file="ParametrosGeracao.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

namespace Lotofacil.Dominio.Geracao;

/// <summary>
/// O que o usuário pediu para gerar: quantidade, tamanho dos jogos, estratégia, dezenas fixas e excluídas.
/// </summary>
public sealed class ParametrosGeracao
{
    /// <summary>
    /// Menor quantidade de jogos por geração.
    /// </summary>
    public const int QuantidadeMinima = 1;

    /// <summary>
    /// Maior quantidade de jogos por geração.
    /// </summary>
    public const int QuantidadeMaxima = 100;

    /// <summary>
    /// Quantidade de jogos sugerida ao abrir a geração.
    /// </summary>
    public const int QuantidadePadrao = 10;

    /// <summary>
    /// Cria os parâmetros, validando as faixas.
    /// </summary>
    /// <param name="quantidade">Quantidade de jogos (1 a 100).</param>
    /// <param name="dezenasPorJogo">Dezenas de cada jogo (15 a 20).</param>
    /// <param name="estrategia">Critério do sorteio das dezenas livres.</param>
    /// <param name="fixas">Dezenas presentes em todos os jogos.</param>
    /// <param name="excluidas">Dezenas ausentes de todos os jogos.</param>
    /// <param name="filtros">Filtros de perfil e de sequência, ou <c>null</c> sem filtros.</param>
    public ParametrosGeracao(
        int quantidade,
        int dezenasPorJogo,
        EstrategiaGeracao estrategia,
        IEnumerable<int> fixas,
        IEnumerable<int> excluidas,
        FiltrosGeracao? filtros = null)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(quantidade, QuantidadeMinima);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(quantidade, QuantidadeMaxima);
        ArgumentOutOfRangeException.ThrowIfLessThan(dezenasPorJogo, RegrasLotofacil.MinimoDezenasPorJogo);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(dezenasPorJogo, RegrasLotofacil.MaximoDezenasPorJogo);
        if (!Enum.IsDefined(estrategia))
        {
            throw new ArgumentOutOfRangeException(nameof(estrategia), "Estratégia desconhecida.");
        }

        List<int> fixasOrdenadas = ValidarDezenas(fixas, nameof(fixas));
        List<int> excluidasOrdenadas = ValidarDezenas(excluidas, nameof(excluidas));
        if (fixasOrdenadas.Intersect(excluidasOrdenadas).Any())
        {
            throw new ArgumentException("Uma dezena não pode ser fixa e excluída ao mesmo tempo.", nameof(excluidas));
        }

        filtros ??= FiltrosGeracao.Nenhum;
        ValidarFiltros(filtros, dezenasPorJogo);

        this.Quantidade = quantidade;
        this.DezenasPorJogo = dezenasPorJogo;
        this.Estrategia = estrategia;
        this.Fixas = fixasOrdenadas;
        this.Excluidas = excluidasOrdenadas;
        this.Filtros = filtros;
    }

    /// <summary>
    /// Quantidade de jogos pedida.
    /// </summary>
    public int Quantidade { get; }

    /// <summary>
    /// Dezenas de cada jogo.
    /// </summary>
    public int DezenasPorJogo { get; }

    /// <summary>
    /// Critério do sorteio das dezenas livres.
    /// </summary>
    public EstrategiaGeracao Estrategia { get; }

    /// <summary>
    /// Dezenas fixas, em ordem crescente.
    /// </summary>
    public IReadOnlyList<int> Fixas { get; }

    /// <summary>
    /// Dezenas excluídas, em ordem crescente.
    /// </summary>
    public IReadOnlyList<int> Excluidas { get; }

    /// <summary>
    /// Filtros de perfil e de sequência.
    /// </summary>
    public FiltrosGeracao Filtros { get; }

    /// <summary>
    /// Confere que os limites estão na faixa possível para o tamanho do jogo; mínimo maior que o máximo é aceito,
    /// para o gerador recusar com mensagem.
    /// </summary>
    /// <param name="filtros">Filtros informados.</param>
    /// <param name="dezenasPorJogo">Dezenas de cada jogo.</param>
    private static void ValidarFiltros(FiltrosGeracao filtros, int dezenasPorJogo)
    {
        foreach (LimitePerfil limite in filtros.Limites)
        {
            int menor = FiltrosGeracao.MenorLimite(limite.Tipo, dezenasPorJogo);
            int maior = FiltrosGeracao.MaiorLimite(limite.Tipo, dezenasPorJogo);
            if (limite.Minimo < menor || limite.Minimo > maior || limite.Maximo < menor || limite.Maximo > maior)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(filtros), $"Limite {FiltrosGeracao.NomeNaMensagem(limite.Tipo)} fora da faixa de {menor} a {maior}.");
            }
        }

        if (filtros.MaiorSequenciaPermitida < 1 || filtros.MaiorSequenciaPermitida > dezenasPorJogo)
        {
            throw new ArgumentOutOfRangeException(nameof(filtros), "A maior sequência vai de 1 às dezenas por jogo.");
        }
    }

    /// <summary>
    /// Confere que as dezenas estão no volante e não se repetem.
    /// </summary>
    /// <param name="dezenas">Dezenas informadas.</param>
    /// <param name="parametro">Nome do parâmetro, para a mensagem.</param>
    /// <returns>As dezenas em ordem crescente.</returns>
    private static List<int> ValidarDezenas(IEnumerable<int> dezenas, string parametro)
    {
        List<int> ordenadas = dezenas.Order().ToList();
        if (ordenadas.Any(dezena => dezena < RegrasLotofacil.DezenaMinima || dezena > RegrasLotofacil.DezenaMaxima))
        {
            throw new ArgumentOutOfRangeException(parametro, "As dezenas vão de 1 a 25.");
        }

        if (ordenadas.Distinct().Count() != ordenadas.Count)
        {
            throw new ArgumentException("Há dezena repetida.", parametro);
        }

        return ordenadas;
    }
}
