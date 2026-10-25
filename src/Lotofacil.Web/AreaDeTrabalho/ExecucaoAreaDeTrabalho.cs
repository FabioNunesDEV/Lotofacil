// <copyright file="ExecucaoAreaDeTrabalho.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using System.Windows.Forms;

namespace Lotofacil.Web.AreaDeTrabalho;

/// <summary>
/// Execução do app instalado: sem console, com o navegador aberto e o ícone na bandeja até "Sair".
/// </summary>
public static class ExecucaoAreaDeTrabalho
{
    /// <summary>
    /// Indica se o app roda como instalado, isto é, fora do ambiente de desenvolvimento (lido antes de montar o host).
    /// </summary>
    /// <returns><c>true</c> fora do ambiente <c>Development</c>.</returns>
    public static bool Ativa()
    {
        string? ambiente = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");

        return !string.Equals(ambiente, Environments.Development, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Inicia o servidor, abre o navegador e mantém o ícone na bandeja; numa falha, mostra a mensagem e registra o erro.
    /// </summary>
    /// <param name="app">Aplicação já montada.</param>
    /// <param name="inicializar">Preparação da base e dos serviços antes de aceitar conexões.</param>
    /// <param name="endereco">Endereço local do app.</param>
    /// <returns>Tarefa concluída quando o app encerra.</returns>
    public static async Task ExecutarAsync(WebApplication app, Func<Task> inicializar, string endereco)
    {
        try
        {
            await inicializar();
            await app.StartAsync();
        }
        catch (Exception erro)
        {
            app.Logger.LogError(erro, "Falha ao iniciar o Lotofácil.");
            MessageBox.Show(
                FalhaInicializacao.Mensagem(erro, new Uri(endereco).Port),
                "Lotofácil",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        Navegador.Abrir(endereco);
        await IconeBandeja.ExecutarAsync(endereco, app.Lifetime.ApplicationStopping);
        await app.StopAsync();
    }
}
