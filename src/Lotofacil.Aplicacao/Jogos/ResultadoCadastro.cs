// <copyright file="ResultadoCadastro.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

namespace Lotofacil.Aplicacao.Jogos;

/// <summary>
/// Desfecho do cadastro de um jogo, com a mensagem para o usuário.
/// </summary>
/// <param name="Sucesso">Indica se o jogo foi gravado.</param>
/// <param name="Mensagem">Mensagem de confirmação ou o motivo da recusa.</param>
public sealed record ResultadoCadastro(bool Sucesso, string Mensagem);
