// <copyright file="Rateio.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

namespace Lotofacil.Dominio.Concursos;

/// <summary>
/// Resultado de uma faixa de premiação do concurso: quantos acertaram e quanto cada um recebe.
/// </summary>
public sealed class Rateio
{
    /// <summary>
    /// Cria o rateio de uma faixa, validando a faixa e os valores.
    /// </summary>
    /// <param name="faixa">Quantidade de acertos da faixa (11 a 15).</param>
    /// <param name="ganhadores">Quantidade de apostas ganhadoras.</param>
    /// <param name="valorPremio">Valor do prêmio de cada aposta ganhadora.</param>
    public Rateio(int faixa, int ganhadores, decimal valorPremio)
    {
        if (faixa < RegrasLotofacil.MenorFaixaPremiada || faixa > RegrasLotofacil.MaiorFaixaPremiada)
        {
            throw new ArgumentOutOfRangeException(
                nameof(faixa),
                $"A faixa de premiação deve estar entre 11 e 15 acertos; foi informada a faixa {faixa}.");
        }

        if (ganhadores < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(ganhadores),
                $"A quantidade de ganhadores da faixa de {faixa} acertos não pode ser negativa.");
        }

        if (valorPremio < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(valorPremio),
                $"O prêmio da faixa de {faixa} acertos não pode ser negativo.");
        }

        this.Faixa = faixa;
        this.Ganhadores = ganhadores;
        this.ValorPremio = valorPremio;
    }

    /// <summary>
    /// Quantidade de acertos da faixa (11 a 15).
    /// </summary>
    public int Faixa { get; private set; }

    /// <summary>
    /// Quantidade de apostas ganhadoras.
    /// </summary>
    public int Ganhadores { get; private set; }

    /// <summary>
    /// Valor do prêmio de cada aposta ganhadora.
    /// </summary>
    public decimal ValorPremio { get; private set; }
}
