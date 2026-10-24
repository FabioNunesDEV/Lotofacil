// <copyright file="ResultadoPreenchimentoOrdem.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

namespace Lotofacil.Aplicacao.Concursos;

/// <summary>
/// Desfecho do preenchimento da ordem do sorteio.
/// </summary>
/// <param name="Total">Concursos sem ordem no início do preenchimento.</param>
/// <param name="Preenchidos">Concursos com a ordem gravada.</param>
/// <param name="Falhas">Concursos pulados, na ordem em que falharam.</param>
/// <param name="Desfecho">Como o preenchimento terminou.</param>
public sealed record ResultadoPreenchimentoOrdem(
    int Total,
    int Preenchidos,
    IReadOnlyList<FalhaOrdemSorteio> Falhas,
    DesfechoPreenchimento Desfecho)
{
    /// <summary>
    /// Concursos pulados por falha.
    /// </summary>
    public int SemOrdem => this.Falhas.Count;

    /// <summary>
    /// Concursos que continuam sem ordem: os pulados e os que não chegaram a ser buscados.
    /// </summary>
    public int Restantes => this.Total - this.Preenchidos;
}
