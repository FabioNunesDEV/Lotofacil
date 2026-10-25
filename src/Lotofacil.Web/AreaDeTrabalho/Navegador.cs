// <copyright file="Navegador.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;

namespace Lotofacil.Web.AreaDeTrabalho;

/// <summary>
/// Abre o endereço do app no navegador padrão do Windows.
/// </summary>
public static class Navegador
{
    /// <summary>
    /// Abre o endereço; sem navegador padrão, mostra o endereço para abrir à mão.
    /// </summary>
    /// <param name="endereco">Endereço local do app.</param>
    public static void Abrir(string endereco)
    {
        try
        {
            Process.Start(new ProcessStartInfo(endereco) { UseShellExecute = true });
        }
        catch (Win32Exception)
        {
            MessageBox.Show(
                $"Não foi possível abrir o navegador. Abra o endereço {endereco} no seu navegador.",
                "Lotofácil",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
    }
}
