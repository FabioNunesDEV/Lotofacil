// <copyright file="MensagemValidacao.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

namespace Lotofacil.Infraestrutura.Caixa;

/// <summary>
/// Prepara as mensagens de validação do domínio para exibição ao usuário.
/// </summary>
internal static class MensagemValidacao
{
    /// <summary>
    /// Devolve a mensagem da validação sem o sufixo <c>(Parameter '...')</c> que o .NET acrescenta.
    /// </summary>
    /// <param name="erro">Erro de validação do domínio.</param>
    /// <returns>Mensagem para o usuário.</returns>
    public static string SemParametro(ArgumentException erro)
    {
        string sufixo = $" (Parameter '{erro.ParamName}')";
        bool temSufixo = erro.ParamName is not null && erro.Message.EndsWith(sufixo, StringComparison.Ordinal);

        return temSufixo ? erro.Message[..^sufixo.Length] : erro.Message;
    }
}
