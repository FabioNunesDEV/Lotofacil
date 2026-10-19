// <copyright file="ArquiteturaTestes.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using System.Reflection;

namespace Lotofacil.Testes.Integracao;

/// <summary>
/// Verifica se cada projeto usa apenas as camadas permitidas pela arquitetura.
/// </summary>
public class ArquiteturaTestes
{
    /// <summary>
    /// Prefixo dos assemblies da solução.
    /// </summary>
    private const string PrefixoSolucao = "Lotofacil.";

    /// <summary>
    /// O projeto só referencia, entre os projetos da solução, as camadas permitidas.
    /// </summary>
    /// <param name="projeto">Nome do assembly verificado.</param>
    /// <param name="permitidos">Projetos da solução que ele pode referenciar.</param>
    [Theory]
    [InlineData("Lotofacil.Dominio")]
    [InlineData("Lotofacil.Aplicacao", "Lotofacil.Dominio")]
    [InlineData("Lotofacil.Infraestrutura", "Lotofacil.Dominio", "Lotofacil.Aplicacao")]
    [InlineData("Lotofacil.Web", "Lotofacil.Dominio", "Lotofacil.Aplicacao", "Lotofacil.Infraestrutura")]
    public void ProjetoReferenciaApenasCamadasPermitidas(string projeto, params string[] permitidos)
    {
        IEnumerable<string> referenciasDaSolucao = Assembly.Load(projeto)
            .GetReferencedAssemblies()
            .Select(referencia => referencia.Name ?? string.Empty)
            .Where(nome => nome.StartsWith(PrefixoSolucao, StringComparison.Ordinal));

        Assert.Empty(referenciasDaSolucao.Except(permitidos));
    }
}
