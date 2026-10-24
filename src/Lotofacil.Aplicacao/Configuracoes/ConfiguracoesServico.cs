// <copyright file="ConfiguracoesServico.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Dominio.Configuracoes;

namespace Lotofacil.Aplicacao.Configuracoes;

/// <summary>
/// Mantém o preço da aposta mínima em uso: o gravado pelo usuário ou, sem gravação, o padrão da configuração.
/// </summary>
public class ConfiguracoesServico
{
    /// <summary>
    /// Repositório da configuração das apostas.
    /// </summary>
    private readonly IConfiguracaoApostasRepositorio repositorio;

    /// <summary>
    /// Preço usado enquanto nada foi gravado.
    /// </summary>
    private readonly decimal precoPadrao;

    /// <summary>
    /// Configuração gravada; a troca da referência é atômica, então as telas leem sem trava.
    /// </summary>
    private ConfiguracaoApostas? gravada;

    /// <summary>
    /// Cria o serviço das configurações.
    /// </summary>
    /// <param name="repositorio">Repositório da configuração das apostas.</param>
    /// <param name="precoPadrao">Preço da aposta mínima usado enquanto nada foi gravado.</param>
    public ConfiguracoesServico(IConfiguracaoApostasRepositorio repositorio, decimal precoPadrao)
    {
        this.repositorio = repositorio;
        this.precoPadrao = precoPadrao;
    }

    /// <summary>
    /// Preço da aposta de 15 dezenas em uso.
    /// </summary>
    public decimal PrecoApostaMinima => this.gravada?.PrecoApostaMinima ?? this.precoPadrao;

    /// <summary>
    /// Lê a configuração gravada; chamado uma vez na inicialização, depois de preparar a base.
    /// </summary>
    /// <param name="cancelamento">Sinal de cancelamento.</param>
    /// <returns>Tarefa da leitura.</returns>
    public async Task CarregarAsync(CancellationToken cancelamento = default)
    {
        this.gravada = await this.repositorio.ObterAsync(cancelamento);
    }

    /// <summary>
    /// Grava o novo preço da aposta mínima, que passa a valer na hora.
    /// </summary>
    /// <param name="preco">Preço da aposta de 15 dezenas.</param>
    /// <param name="cancelamento">Sinal de cancelamento.</param>
    /// <returns><c>false</c>, sem gravar, se o preço for inválido.</returns>
    public async Task<bool> SalvarPrecoApostaMinimaAsync(decimal preco, CancellationToken cancelamento = default)
    {
        if (!ConfiguracaoApostas.PrecoValido(preco))
        {
            return false;
        }

        var configuracao = new ConfiguracaoApostas(preco);
        await this.repositorio.SalvarAsync(configuracao, cancelamento);
        this.gravada = configuracao;

        return true;
    }
}
