// <copyright file="AvisoAtualizacao.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using MudBlazor;

namespace Lotofacil.Web.Components.Concursos;

/// <summary>
/// Mensagem do desfecho da atualização dos resultados, com a gravidade para exibição.
/// </summary>
/// <param name="Texto">Mensagem para o usuário.</param>
/// <param name="Severidade">Gravidade da mensagem; <see cref="Severity.Error"/> indica falha.</param>
public sealed record AvisoAtualizacao(string Texto, Severity Severidade);
