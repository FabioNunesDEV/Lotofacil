// <copyright file="PremioFaixa.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

namespace Lotofacil.Dominio.Jogos;

/// <summary>
/// Prêmios que um jogo recebe em uma faixa: quantos e o valor de cada um pelo rateio do concurso.
/// </summary>
/// <param name="Faixa">Quantidade de acertos da faixa (11 a 15).</param>
/// <param name="Quantidade">Quantidade de prêmios da faixa.</param>
/// <param name="ValorPremio">Valor de cada prêmio da faixa no rateio gravado do concurso.</param>
public sealed record PremioFaixa(int Faixa, int Quantidade, decimal ValorPremio)
{
    /// <summary>
    /// Soma dos prêmios da faixa.
    /// </summary>
    public decimal Total => this.Quantidade * this.ValorPremio;
}
