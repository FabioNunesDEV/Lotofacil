// <copyright file="CaminhoConfigurado.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

namespace Lotofacil.Web;

/// <summary>
/// Resolve os caminhos da configuração (base e log), que podem ser relativos à pasta da aplicação ou absolutos.
/// </summary>
public static class CaminhoConfigurado
{
    /// <summary>
    /// Caminho final de um valor da configuração.
    /// </summary>
    /// <param name="pastaAplicacao">Pasta da aplicação, base dos caminhos relativos.</param>
    /// <param name="configurado">Valor da configuração, ou <c>null</c> se ausente.</param>
    /// <param name="padrao">Caminho relativo usado quando a configuração não tem valor.</param>
    /// <returns>O caminho absoluto configurado ou o relativo combinado com a pasta da aplicação.</returns>
    public static string Resolver(string pastaAplicacao, string? configurado, string padrao)
    {
        string caminho = string.IsNullOrWhiteSpace(configurado) ? padrao : configurado;

        return Path.Combine(pastaAplicacao, caminho);
    }
}
