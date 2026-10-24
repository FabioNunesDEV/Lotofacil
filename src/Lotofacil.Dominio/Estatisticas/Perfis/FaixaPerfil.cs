// <copyright file="FaixaPerfil.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

namespace Lotofacil.Dominio.Estatisticas.Perfis;

/// <summary>
/// Uma linha de um perfil: os valores que ela cobre, o observado na janela, o teórico e o atraso.
/// </summary>
/// <param name="Minimo">Menor valor da linha, inclusive.</param>
/// <param name="Maximo">Maior valor da linha, inclusive (igual ao mínimo numa linha simples de quantidade).</param>
/// <param name="AbertaAbaixo">Indica que a linha agrupou os valores raros da ponta de baixo.</param>
/// <param name="AbertaAcima">Indica que a linha agrupou os valores raros da ponta de cima.</param>
/// <param name="Observado">Concursos da janela com valor na linha, de 0 a 100.</param>
/// <param name="Teorico">Chance de um jogo de 15 dezenas cair na linha, de 0 a 100.</param>
/// <param name="Atraso">Concursos desde a última vez que caiu na linha, ou <c>null</c> se nunca caiu.</param>
public sealed record FaixaPerfil(
    int Minimo,
    int Maximo,
    bool AbertaAbaixo,
    bool AbertaAcima,
    decimal Observado,
    decimal Teorico,
    int? Atraso)
{
    /// <summary>
    /// Indica se o valor cai na linha.
    /// </summary>
    /// <param name="valor">Valor do perfil num concurso.</param>
    /// <returns><c>true</c> quando o valor está entre o mínimo e o máximo.</returns>
    public bool Contem(int valor)
    {
        return valor >= this.Minimo && valor <= this.Maximo;
    }
}
