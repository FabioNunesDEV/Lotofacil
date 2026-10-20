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
    /// Cliente da API da Caixa.
    /// </summary>
    private readonly ClienteCaixa cliente;

    /// <summary>
    /// Cria a fonte do histórico.
    /// </summary>
    /// <param name="cliente">Cliente da API da Caixa.</param>
    public HistoricoCaixaFonte(ClienteCaixa cliente)
    {
        this.cliente = cliente;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Concurso>> ObterHistoricoAsync(CancellationToken cancelamento = default)
    {
        byte[] arquivo = await this.cliente.ObterAsync(CaminhoDownload, "baixar o histórico", cancelamento);

        using var conteudo = new MemoryStream(arquivo);
        return LeitorPlanilhaHistorico.Ler(conteudo);
    }
}
