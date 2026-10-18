// <copyright file="ConfiguracaoLogTestes.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Web;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Lotofacil.Testes.Integracao;

/// <summary>
/// Verifica a gravação do log de erros em arquivo com a configuração usada pela aplicação.
/// </summary>
public sealed class ConfiguracaoLogTestes : IDisposable
{
    /// <summary>
    /// Pasta temporária e ainda inexistente onde o log do teste é gravado.
    /// </summary>
    private readonly string pasta = Path.Combine(Path.GetTempPath(), "lotofacil-testes", Guid.NewGuid().ToString("N"));

    /// <summary>
    /// Só o erro é gravado, com a exceção, num arquivo do dia criado na pasta configurada.
    /// </summary>
    [Fact]
    public void GravaSomenteErrosNoArquivoDoDia()
    {
        using (ILoggerFactory fabrica = LoggerFactory.Create(
            construtor => construtor.AddSerilog(ConfiguracaoLog.Criar(this.pasta).CreateLogger(), dispose: true)))
        {
            ILogger<ConfiguracaoLogTestes> logger = fabrica.CreateLogger<ConfiguracaoLogTestes>();
            logger.LogInformation("mensagem informativa");
            logger.LogWarning("mensagem de aviso");
            logger.LogError(new InvalidOperationException("falha simulada"), "erro registrado");
        }

        string arquivoEsperado = Path.Combine(this.pasta, $"lotofacil-{DateTime.Now:yyyyMMdd}.log");
        Assert.Equal([arquivoEsperado], Directory.GetFiles(this.pasta));

        string conteudo = File.ReadAllText(arquivoEsperado);
        Assert.Contains("[ERR] Lotofacil.Testes.Integracao.ConfiguracaoLogTestes: erro registrado", conteudo);
        Assert.Contains("System.InvalidOperationException: falha simulada", conteudo);
        Assert.DoesNotContain("mensagem informativa", conteudo);
        Assert.DoesNotContain("mensagem de aviso", conteudo);
    }

    /// <summary>
    /// Remove a pasta temporária do teste.
    /// </summary>
    public void Dispose()
    {
        if (Directory.Exists(this.pasta))
        {
            Directory.Delete(this.pasta, recursive: true);
        }
    }
}
