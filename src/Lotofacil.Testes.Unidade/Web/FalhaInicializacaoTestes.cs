// <copyright file="FalhaInicializacaoTestes.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using System.Net.Sockets;
using Lotofacil.Web.AreaDeTrabalho;
using Microsoft.AspNetCore.Connections;

namespace Lotofacil.Testes.Unidade.Web;

/// <summary>
/// Testes das mensagens de falha ao iniciar o app instalado.
/// </summary>
public class FalhaInicializacaoTestes
{
    /// <summary>
    /// Porta ocupada, como o servidor informa (dentro de outra exceção), tem mensagem própria.
    /// </summary>
    [Fact]
    public void PortaEmUsoDentroDeOutraExcecao()
    {
        var erro = new IOException(
            "Failed to bind to address http://127.0.0.1:5080: address already in use.",
            new AddressInUseException("Only one usage of each socket address is normally permitted."));

        Assert.Equal(
            "Não foi possível iniciar o Lotofácil: a porta 5080 já está em uso por outro programa.",
            FalhaInicializacao.Mensagem(erro, 5080));
    }

    /// <summary>
    /// Porta ocupada também é reconhecida pelo erro do socket.
    /// </summary>
    [Fact]
    public void PortaEmUsoPeloSocket()
    {
        var erro = new SocketException((int)SocketError.AddressAlreadyInUse);

        Assert.Equal(
            "Não foi possível iniciar o Lotofácil: a porta 5081 já está em uso por outro programa.",
            FalhaInicializacao.Mensagem(erro, 5081));
    }

    /// <summary>
    /// Outras falhas, como a pasta dos dados sem permissão, trazem o motivo do erro.
    /// </summary>
    [Fact]
    public void OutraFalhaTrazOMotivo()
    {
        var erro = new UnauthorizedAccessException(@"Access to the path 'D:\Loteria' is denied.");

        Assert.Equal(
            @"Não foi possível iniciar o Lotofácil: Access to the path 'D:\Loteria' is denied.",
            FalhaInicializacao.Mensagem(erro, 5080));
    }
}
