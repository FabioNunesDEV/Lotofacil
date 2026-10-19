// <copyright file="ConfiguracaoLog.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Serilog;
using Serilog.Events;

namespace Lotofacil.Web;

/// <summary>
/// Monta a configuração do Serilog que grava apenas erros em arquivo, um arquivo por dia.
/// </summary>
public static class ConfiguracaoLog
{
    /// <summary>
    /// Formato de cada linha do arquivo; a exceção, quando houver, vem nas linhas seguintes.
    /// </summary>
    private const string FormatoLinha =
        "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}";

    /// <summary>
    /// Cria a configuração do log de erros.
    /// </summary>
    /// <param name="pasta">Pasta onde os arquivos são gravados; é criada se não existir.</param>
    /// <returns>Configuração pronta para criar o logger.</returns>
    public static LoggerConfiguration Criar(string pasta)
    {
        string caminhoArquivo = Path.Combine(pasta, "lotofacil-.log");

        return new LoggerConfiguration()
            .MinimumLevel.Is(LogEventLevel.Error)
            .WriteTo.File(caminhoArquivo, rollingInterval: RollingInterval.Day, outputTemplate: FormatoLinha);
    }
}
