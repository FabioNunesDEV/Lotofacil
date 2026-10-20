// <copyright file="EstatisticaDezena.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

namespace Lotofacil.Dominio.Estatisticas;

/// <summary>
/// Números de uma dezena: frequência na janela da análise e atraso sobre todo o histórico.
/// </summary>
/// <param name="Dezena">Dezena (1 a 25).</param>
/// <param name="Sorteios">Em quantos concursos da janela a dezena saiu.</param>
/// <param name="Percentual">Sorteios em relação aos concursos da janela, de 0 a 100.</param>
/// <param name="Diferenca">Sorteios menos o esperado para a janela.</param>
/// <param name="Desvio">Diferença em desvios-padrão.</param>
/// <param name="Atraso">Concursos desde a última vez que saiu, ou <c>null</c> se nunca saiu.</param>
/// <param name="UltimaVez">Último concurso em que saiu, ou <c>null</c> se nunca saiu.</param>
public sealed record EstatisticaDezena(
    int Dezena,
    int Sorteios,
    decimal Percentual,
    decimal Diferenca,
    double Desvio,
    int? Atraso,
    int? UltimaVez);
