// <copyright file="IconeBandeja.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using System.Drawing;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace Lotofacil.Web.AreaDeTrabalho;

/// <summary>
/// Ícone do app na área de notificação do Windows, com "Abrir o Lotofácil" e "Sair".
/// </summary>
public static class IconeBandeja
{
    /// <summary>
    /// Intervalo da verificação do encerramento pedido de fora do ícone.
    /// </summary>
    private const int IntervaloVerificacaoMs = 500;

    /// <summary>
    /// Mostra o ícone numa thread própria do Windows Forms até o usuário escolher "Sair" ou o app encerrar.
    /// </summary>
    /// <param name="endereco">Endereço local aberto pelo menu e pelo clique duplo.</param>
    /// <param name="encerramento">Sinal de que o app está encerrando por outro motivo.</param>
    /// <returns>Tarefa concluída quando o ícone some.</returns>
    public static Task ExecutarAsync(string endereco, CancellationToken encerramento)
    {
        var concluido = new TaskCompletionSource();
        var thread = new Thread(() =>
        {
            Executar(endereco, encerramento);
            concluido.SetResult();
        });

        // O Windows Forms exige uma thread STA.
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();

        return concluido.Task;
    }

    /// <summary>
    /// Cria o ícone e roda o laço de mensagens do Windows até o fim.
    /// </summary>
    /// <param name="endereco">Endereço local do app.</param>
    /// <param name="encerramento">Sinal de encerramento vindo de fora do ícone.</param>
    private static void Executar(string endereco, CancellationToken encerramento)
    {
        using var contexto = new ApplicationContext();
        using var menu = new ContextMenuStrip();
        menu.Items.Add("Abrir o Lotofácil", null, (_, _) => Navegador.Abrir(endereco));
        menu.Items.Add("Sair", null, (_, _) => contexto.ExitThread());

        using var imagem = new Icon(Path.Combine(AppContext.BaseDirectory, "lotofacil.ico"), SystemInformation.SmallIconSize);
        using var icone = new NotifyIcon { Icon = imagem, Text = "Lotofácil", ContextMenuStrip = menu, Visible = true };
        icone.DoubleClick += (_, _) => Navegador.Abrir(endereco);

        using var verificacao = new Timer { Interval = IntervaloVerificacaoMs };
        verificacao.Tick += (_, _) =>
        {
            if (encerramento.IsCancellationRequested)
            {
                contexto.ExitThread();
            }
        };
        verificacao.Start();

        Application.Run(contexto);
        icone.Visible = false;
    }
}
