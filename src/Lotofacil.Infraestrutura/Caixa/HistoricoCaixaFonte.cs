// <copyright file="HistoricoCaixaFonte.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Dominio.Concursos;

namespace Lotofacil.Infraestrutura.Caixa;

/// <summary>
/// Baixa da Caixa o XLSX com o histórico completo da Lotofácil e o converte em concursos.
/// </summary>
internal sealed class HistoricoCaixaFonte : IFonteHistoricoConcursos
{
    /// <summary>
    /// Caminho do download, relativo à URL base; <c>modalidade</c> é "Lotofácil" codificado.
    /// </summary>
    private const string CaminhoDownload = "api/resultados/download?modalidade=Lotof%C3%A1cil";

    /// <summary>
    /// Cliente HTTP compartilhado, com o tempo limite da configuração.
    /// </summary>
    private readonly HttpClient cliente;

    /// <summary>
    /// Configuração da fonte.
    /// </summary>
    private readonly OpcoesFonteResultados opcoes;

    /// <summary>
    /// Cria a fonte do histórico.
    /// </summary>
    /// <param name="cliente">Cliente HTTP com o tempo limite já configurado.</param>
    /// <param name="opcoes">Configuração da fonte.</param>
    public HistoricoCaixaFonte(HttpClient cliente, OpcoesFonteResultados opcoes)
    {
        this.cliente = cliente;
        this.opcoes = opcoes;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Concurso>> ObterHistoricoAsync(CancellationToken cancelamento = default)
    {
        var endereco = new Uri($"{this.opcoes.UrlBase.TrimEnd('/')}/{CaminhoDownload}");
        byte[] arquivo;

        try
        {
            using HttpResponseMessage resposta = await this.cliente.GetAsync(endereco, cancelamento);
            if (!resposta.IsSuccessStatusCode)
            {
                throw new FonteResultadosException(
                    $"A Caixa respondeu com o código {(int)resposta.StatusCode} ao baixar o histórico.");
            }

            arquivo = await resposta.Content.ReadAsByteArrayAsync(cancelamento);
        }
        catch (HttpRequestException erro)
        {
            throw new FonteResultadosException("Não foi possível conectar à Caixa para baixar o histórico.", erro);
        }
        catch (TaskCanceledException erro) when (!cancelamento.IsCancellationRequested)
        {
            // Sem cancelamento pedido pelo chamador, o cancelamento veio do tempo limite do HttpClient.
            throw new FonteResultadosException(
                $"A Caixa não respondeu em {this.opcoes.TempoLimiteSegundos} segundos ao baixar o histórico.",
                erro);
        }

        using var conteudo = new MemoryStream(arquivo);
        return LeitorPlanilhaHistorico.Ler(conteudo);
    }
}
