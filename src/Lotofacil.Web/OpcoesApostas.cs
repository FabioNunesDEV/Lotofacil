// <copyright file="OpcoesApostas.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

namespace Lotofacil.Web;

/// <summary>
/// Valores das apostas usados nas telas, vindos da configuração.
/// </summary>
/// <param name="PrecoApostaMinima">Preço da aposta de 15 dezenas.</param>
public sealed record OpcoesApostas(decimal PrecoApostaMinima);
