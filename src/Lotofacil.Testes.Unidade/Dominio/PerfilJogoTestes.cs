// <copyright file="PerfilJogoTestes.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Dominio.Geracao;

namespace Lotofacil.Testes.Unidade.Dominio;

/// <summary>
/// Testes do perfil de um jogo.
/// </summary>
public class PerfilJogoTestes
{
    /// <summary>
    /// Jogo de 01 a 15 contra o resultado do concurso 3810.
    /// </summary>
    [Fact]
    public void PerfilContraOUltimoResultado()
    {
        int[] resultado3810 = [1, 5, 6, 7, 10, 11, 14, 15, 16, 17, 18, 19, 20, 23, 24];

        PerfilJogo perfil = PerfilJogo.Calcular([.. Enumerable.Range(1, 15)], resultado3810);

        Assert.Equal(new PerfilJogo(7, 8, 120, 8), perfil);
    }

    /// <summary>
    /// Jogo de 16 dezenas conta todas as dezenas.
    /// </summary>
    [Fact]
    public void PerfilDeApostaMultipla()
    {
        PerfilJogo perfil = PerfilJogo.Calcular([.. Enumerable.Range(10, 16)], [.. Enumerable.Range(1, 15)]);

        Assert.Equal(new PerfilJogo(8, 8, 280, 6), perfil);
    }
}
