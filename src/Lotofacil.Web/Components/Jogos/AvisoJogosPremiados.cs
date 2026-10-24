// <copyright file="AvisoJogosPremiados.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Aplicacao.Jogos;
using MudBlazor;

namespace Lotofacil.Web.Components.Jogos;

/// <summary>
/// Mostra o resultado da conferência dos concursos novos: diálogo com os premiados, aviso sem prêmio ou nada.
/// </summary>
public static class AvisoJogosPremiados
{
    /// <summary>
    /// Título do diálogo.
    /// </summary>
    public const string Titulo = "Jogos premiados";

    /// <summary>
    /// Avisa o resultado da conferência.
    /// </summary>
    /// <param name="dialogos">Serviço de diálogos.</param>
    /// <param name="mensagens">Mensagens rápidas.</param>
    /// <param name="conferencia">Jogos conferidos dos concursos novos.</param>
    /// <param name="hoje">Data de referência do prazo para receber.</param>
    /// <returns>Tarefa da abertura do aviso.</returns>
    public static async Task MostrarAsync(
        IDialogService dialogos,
        ISnackbar mensagens,
        ConferenciaConcursos conferencia,
        DateOnly hoje)
    {
        if (conferencia.JogosConferidos == 0)
        {
            return;
        }

        if (conferencia.JogosPremiados == 0)
        {
            mensagens.Add(FormatacaoPremios.NenhumPremio(conferencia.JogosConferidos), Severity.Info);
            return;
        }

        var parametros = new DialogParameters<DialogoJogosPremiados>
        {
            { dialogo => dialogo.Conferencia, conferencia },
            { dialogo => dialogo.Hoje, hoje },
        };
        await dialogos.ShowAsync<DialogoJogosPremiados>(Titulo, parametros);
    }
}
