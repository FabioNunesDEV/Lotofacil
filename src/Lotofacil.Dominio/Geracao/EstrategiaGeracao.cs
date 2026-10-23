// <copyright file="EstrategiaGeracao.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

namespace Lotofacil.Dominio.Geracao;

/// <summary>
/// Critério usado para sortear as dezenas livres de um jogo gerado.
/// </summary>
public enum EstrategiaGeracao
{
    /// <summary>
    /// Todas as dezenas livres com a mesma chance (Surpresinha).
    /// </summary>
    Aleatoria,

    /// <summary>
    /// Mais chance para as dezenas mais sorteadas na janela.
    /// </summary>
    MaisSorteadas,

    /// <summary>
    /// Mais chance para as dezenas menos sorteadas na janela.
    /// </summary>
    MenosSorteadas,

    /// <summary>
    /// Mais chance para as dezenas há mais tempo sem sair.
    /// </summary>
    MaisAtrasadas,

    /// <summary>
    /// Metade das dezenas livres pelas mais sorteadas e metade pelas mais atrasadas.
    /// </summary>
    Equilibrada,
}
