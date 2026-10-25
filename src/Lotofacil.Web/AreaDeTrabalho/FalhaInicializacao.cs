// <copyright file="FalhaInicializacao.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using System.Net.Sockets;
using Microsoft.AspNetCore.Connections;

namespace Lotofacil.Web.AreaDeTrabalho;

/// <summary>
/// Mensagem exibida quando o app instalado não consegue iniciar.
/// </summary>
public static class FalhaInicializacao
{
    /// <summary>
    /// Começo comum de todas as mensagens.
    /// </summary>
    public const string Inicio = "Não foi possível iniciar o Lotofácil: ";

    /// <summary>
    /// Monta a mensagem a partir do erro; a porta ocupada, que pode vir dentro de outra exceção, tem texto próprio.
    /// </summary>
    /// <param name="erro">Erro da inicialização.</param>
    /// <param name="porta">Porta configurada do app.</param>
    /// <returns>Texto da mensagem.</returns>
    public static string Mensagem(Exception erro, int porta)
    {
        for (Exception? atual = erro; atual is not null; atual = atual.InnerException)
        {
            if (atual is AddressInUseException or SocketException { SocketErrorCode: SocketError.AddressAlreadyInUse })
            {
                return $"{Inicio}a porta {porta} já está em uso por outro programa.";
            }
        }

        return Inicio + erro.Message;
    }
}
