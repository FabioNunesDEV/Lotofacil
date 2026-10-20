// <copyright file="LoggerFalso.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Microsoft.Extensions.Logging;

namespace Lotofacil.Testes.Unidade.Web;

/// <summary>
/// Logger que guarda o nível e a exceção de cada registro, para os testes conferirem o que foi logado.
/// </summary>
/// <typeparam name="T">Categoria do log.</typeparam>
internal sealed class LoggerFalso<T> : ILogger<T>
{
    /// <summary>
    /// Registros recebidos, na ordem.
    /// </summary>
    public List<(LogLevel Nivel, Exception? Erro)> Registros { get; } = [];

    /// <inheritdoc/>
    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull
    {
        return null;
    }

    /// <inheritdoc/>
    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    /// <inheritdoc/>
    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        this.Registros.Add((logLevel, exception));
    }
}
