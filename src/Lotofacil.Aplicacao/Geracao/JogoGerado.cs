// <copyright file="JogoGerado.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Dominio.Geracao;

namespace Lotofacil.Aplicacao.Geracao;

/// <summary>
/// Jogo gerado, ainda não salvo, com o seu perfil.
/// </summary>
/// <param name="Dezenas">Dezenas do jogo, em ordem crescente.</param>
/// <param name="Perfil">Pares, ímpares, soma e repetidas do último concurso gravado.</param>
public sealed record JogoGerado(IReadOnlyList<int> Dezenas, PerfilJogo Perfil);
