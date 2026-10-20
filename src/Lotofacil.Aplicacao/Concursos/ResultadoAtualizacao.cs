// <copyright file="ResultadoAtualizacao.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Dominio.Concursos;

namespace Lotofacil.Aplicacao.Concursos;

/// <summary>
/// Desfecho de uma atualização da base pela fonte de concursos, concluída ou interrompida.
/// </summary>
/// <param name="UltimoNumeroAntes">Último concurso gravado antes da atualização, ou <c>null</c> com a base vazia.</param>
/// <param name="ConcursosGravados">Quantidade de concursos novos gravados.</param>
/// <param name="Falha">Falha da fonte que interrompeu a atualização, ou <c>null</c> se ela terminou.</param>
public sealed record ResultadoAtualizacao(int? UltimoNumeroAntes, int ConcursosGravados, FonteResultadosException? Falha)
{
    /// <summary>
    /// Indica que a base estava vazia e nada foi buscado.
    /// </summary>
    public bool BaseVazia => this.UltimoNumeroAntes is null;

    /// <summary>
    /// Número do primeiro concurso gravado nesta atualização (válido quando algum foi gravado).
    /// </summary>
    public int PrimeiroGravado => (this.UltimoNumeroAntes ?? 0) + 1;

    /// <summary>
    /// Número do último concurso da base ao final da atualização.
    /// </summary>
    public int UltimoNumeroNaBase => (this.UltimoNumeroAntes ?? 0) + this.ConcursosGravados;
}
