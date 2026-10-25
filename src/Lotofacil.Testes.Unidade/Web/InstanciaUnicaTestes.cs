// <copyright file="InstanciaUnicaTestes.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Web.AreaDeTrabalho;

namespace Lotofacil.Testes.Unidade.Web;

/// <summary>
/// Testes da execução única do app instalado.
/// </summary>
public class InstanciaUnicaTestes
{
    /// <summary>
    /// A primeira execução é a primeira; a segunda, com a primeira aberta, não; depois de fechar, volta a ser.
    /// </summary>
    [Fact]
    public void SegundaExecucaoNaoEAPrimeira()
    {
        string nome = $"Lotofacil.Testes.{Guid.NewGuid():N}";

        using (var primeira = new InstanciaUnica(nome))
        {
            Assert.True(primeira.Primeira);
            using var segunda = new InstanciaUnica(nome);
            Assert.False(segunda.Primeira);
        }

        using var depois = new InstanciaUnica(nome);
        Assert.True(depois.Primeira);
    }
}
