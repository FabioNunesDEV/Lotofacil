// <copyright file="IConfiguracaoApostasRepositorio.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

namespace Lotofacil.Dominio.Configuracoes;

/// <summary>
/// Acesso à configuração das apostas gravada na base local.
/// </summary>
public interface IConfiguracaoApostasRepositorio
{
    /// <summary>
    /// Lê a configuração gravada.
    /// </summary>
    /// <param name="cancelamento">Sinal de cancelamento.</param>
    /// <returns>A configuração, ou <c>null</c> se nada foi gravado.</returns>
    Task<ConfiguracaoApostas?> ObterAsync(CancellationToken cancelamento = default);

    /// <summary>
    /// Grava a configuração, inserindo a linha única ou substituindo a existente.
    /// </summary>
    /// <param name="configuracao">Configuração a gravar.</param>
    /// <param name="cancelamento">Sinal de cancelamento.</param>
    /// <returns>Tarefa da gravação.</returns>
    Task SalvarAsync(ConfiguracaoApostas configuracao, CancellationToken cancelamento = default);
}
