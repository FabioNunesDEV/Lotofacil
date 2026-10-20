// <copyright file="FonteResultadosException.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

namespace Lotofacil.Dominio.Concursos;

/// <summary>
/// Falha ao obter resultados da fonte oficial; a mensagem é a que o usuário vê.
/// </summary>
public sealed class FonteResultadosException : Exception
{
    /// <summary>
    /// Cria a exceção com a mensagem para o usuário.
    /// </summary>
    /// <param name="mensagem">Descrição da falha para o usuário.</param>
    public FonteResultadosException(string mensagem)
        : base(mensagem)
    {
    }

    /// <summary>
    /// Cria a exceção com a mensagem para o usuário e a causa técnica.
    /// </summary>
    /// <param name="mensagem">Descrição da falha para o usuário.</param>
    /// <param name="causa">Exceção que originou a falha.</param>
    public FonteResultadosException(string mensagem, Exception causa)
        : base(mensagem, causa)
    {
    }
}
