// <copyright file="DistribuicaoPerfil.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

namespace Lotofacil.Dominio.Estatisticas.Perfis;

/// <summary>
/// As linhas de um perfil, em ordem crescente de valor.
/// </summary>
/// <param name="Tipo">Perfil.</param>
/// <param name="Faixas">Linhas do perfil, com as pontas raras agrupadas.</param>
public sealed record DistribuicaoPerfil(TipoPerfil Tipo, IReadOnlyList<FaixaPerfil> Faixas);
