// <copyright file="ConferenciaJogo.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Dominio.Concursos;

namespace Lotofacil.Dominio.Jogos;

/// <summary>
/// Resultado de um jogo em um concurso sorteado: acertos e prêmios por faixa pelo rateio gravado do concurso.
/// </summary>
/// <param name="Acertos">Quantidade de dezenas do jogo que foram sorteadas.</param>
/// <param name="Premios">Prêmios por faixa, da maior para a menor, somente as faixas com prêmio.</param>
public sealed record ConferenciaJogo(int Acertos, IReadOnlyList<PremioFaixa> Premios)
{
    /// <summary>
    /// Indica se o jogo recebeu prêmio em alguma faixa.
    /// </summary>
    public bool Premiado => this.Premios.Count > 0;

    /// <summary>
    /// Soma dos prêmios de todas as faixas, ou zero sem prêmio.
    /// </summary>
    public decimal Premio => this.Premios.Sum(premio => premio.Total);

    /// <summary>
    /// Confere o jogo contra o concurso, usando sempre o rateio gravado desse concurso.
    /// </summary>
    /// <param name="jogo">Jogo conferido.</param>
    /// <param name="concurso">Concurso a que o jogo se destina.</param>
    /// <returns>Acertos e prêmios do jogo.</returns>
    public static ConferenciaJogo Conferir(Jogo jogo, Concurso concurso)
    {
        if (jogo.NumeroConcurso != concurso.Numero)
        {
            throw new ArgumentException(
                $"O jogo é do concurso {jogo.NumeroConcurso} e não pode ser conferido com o concurso {concurso.Numero}.",
                nameof(concurso));
        }

        int acertos = jogo.Dezenas.Count(dezena => concurso.Dezenas.Contains(dezena));
        int erros = jogo.Dezenas.Count - acertos;
        List<PremioFaixa> premios = [];

        // Uma aposta de n dezenas equivale a todas as apostas de 15 formadas com elas; as que fazem k acertos
        // combinam k das dezenas acertadas com 15 − k das que não saíram.
        for (int faixa = RegrasLotofacil.MaiorFaixaPremiada; faixa >= RegrasLotofacil.MenorFaixaPremiada; faixa--)
        {
            int quantidade = Combinatoria.Combinacoes(acertos, faixa)
                * Combinatoria.Combinacoes(erros, RegrasLotofacil.DezenasSorteadas - faixa);
            if (quantidade > 0)
            {
                premios.Add(new PremioFaixa(faixa, quantidade, concurso.ObterRateio(faixa).ValorPremio));
            }
        }

        return new ConferenciaJogo(acertos, premios);
    }
}
