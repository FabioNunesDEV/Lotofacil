// <copyright file="ConcursosCaixaFonte.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Dominio.Concursos;

namespace Lotofacil.Infraestrutura.Caixa;

/// <summary>
/// Busca concursos individuais da Lotofácil na API JSON da Caixa.
/// </summary>
internal sealed class ConcursosCaixaFonte : IFonteConcursos
{
    /// <summary>
    /// Caminho da API da Lotofácil, relativo à URL base; sem número, devolve o último concurso.
    /// </summary>
    private const string CaminhoLotofacil = "api/lotofacil/";

    /// <summary>
    /// Cliente da API da Caixa.
    /// </summary>
    private readonly ClienteCaixa cliente;

    /// <summary>
    /// Cria a fonte de concursos.
    /// </summary>
    /// <param name="cliente">Cliente da API da Caixa.</param>
    public ConcursosCaixaFonte(ClienteCaixa cliente)
    {
        this.cliente = cliente;
    }

    /// <inheritdoc/>
    public async Task<int> ObterNumeroUltimoConcursoAsync(CancellationToken cancelamento = default)
    {
        byte[] resposta = await this.cliente.ObterAsync(CaminhoLotofacil, "buscar o último concurso", cancelamento);

        return LeitorConcursoJson.LerNumero(resposta);
    }

    /// <inheritdoc/>
    public async Task<Concurso> ObterConcursoAsync(int numero, CancellationToken cancelamento = default)
    {
        byte[] resposta = await this.cliente.ObterAsync(
            $"{CaminhoLotofacil}{numero}",
            $"buscar o concurso {numero}",
            cancelamento);

        return LeitorConcursoJson.LerConcurso(resposta, numero);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<int>?> ObterOrdemSorteioAsync(int numero, CancellationToken cancelamento = default)
    {
        byte[] resposta = await this.cliente.ObterAsync(
            $"{CaminhoLotofacil}{numero}",
            $"buscar a ordem do concurso {numero}",
            cancelamento);

        return LeitorConcursoJson.LerOrdemSorteio(resposta, numero);
    }
}
