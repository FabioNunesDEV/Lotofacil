// <copyright file="LimitePerfil.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Dominio.Estatisticas.Perfis;

namespace Lotofacil.Dominio.Geracao;

/// <summary>
/// Limites de um perfil pedidos na geração; um lado nulo não limita.
/// </summary>
/// <param name="Tipo">Perfil limitado.</param>
/// <param name="Minimo">Menor valor aceito, inclusive, ou <c>null</c> sem limite inferior.</param>
/// <param name="Maximo">Maior valor aceito, inclusive, ou <c>null</c> sem limite superior.</param>
public sealed record LimitePerfil(TipoPerfil Tipo, int? Minimo, int? Maximo)
{
    /// <summary>
    /// Indica que algum lado foi preenchido.
    /// </summary>
    public bool Ligado => this.Minimo is not null || this.Maximo is not null;

    /// <summary>
    /// Indica que o mínimo é maior que o máximo, o que nenhum valor atende.
    /// </summary>
    public bool Invertido => this.Minimo > this.Maximo;

    /// <summary>
    /// Indica se o valor está dentro dos limites.
    /// </summary>
    /// <param name="valor">Valor do perfil num jogo.</param>
    /// <returns><c>true</c> quando nenhum lado preenchido é violado.</returns>
    public bool Contem(int valor)
    {
        return (this.Minimo is null || valor >= this.Minimo) && (this.Maximo is null || valor <= this.Maximo);
    }
}
