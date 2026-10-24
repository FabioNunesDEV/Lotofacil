// <copyright file="AvisoTeimosinhas.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Aplicacao.Teimosinhas;
using MudBlazor;

namespace Lotofacil.Web.Components.Teimosinhas;

/// <summary>
/// Cria os jogos das Teimosinhas depois de uma atualização e avisa quantos foram criados; usado por Resultados e
/// Painel.
/// </summary>
public static class AvisoTeimosinhas
{
    /// <summary>
    /// Cria os jogos das Teimosinhas até o próximo concurso e, se algum foi criado, mostra o aviso.
    /// </summary>
    /// <param name="servico">Serviço das Teimosinhas.</param>
    /// <param name="mensagens">Mensagens rápidas da página.</param>
    /// <returns>Tarefa da criação.</returns>
    public static async Task CriarJogosAsync(TeimosinhaServico servico, ISnackbar mensagens)
    {
        JogosCriadosTeimosinha criados = await servico.CriarJogosAsync();
        if (criados.Quantidade > 0)
        {
            mensagens.Add(FormatacaoTeimosinha.Criados(criados), Severity.Info);
        }
    }
}
