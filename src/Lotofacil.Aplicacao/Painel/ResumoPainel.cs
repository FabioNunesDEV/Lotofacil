// <copyright file="ResumoPainel.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Aplicacao.Jogos;
using Lotofacil.Dominio.Concursos;
using Lotofacil.Dominio.Estatisticas;

namespace Lotofacil.Aplicacao.Painel;

/// <summary>
/// Resumo da base local e dos jogos exibido no Painel.
/// </summary>
/// <param name="TotalConcursos">Quantidade de concursos gravados.</param>
/// <param name="UltimoConcurso">Último concurso gravado.</param>
/// <param name="Estatisticas">Estatísticas das dezenas sobre todo o histórico.</param>
/// <param name="JogosUltimoConcurso">Jogos do último concurso gravado, conferidos, ou <c>null</c> se não houver.</param>
/// <param name="JogosAguardando">Quantidade de jogos de concursos ainda não sorteados.</param>
/// <param name="ConcursosAguardando">Números dos concursos com jogos aguardando sorteio, do menor para o maior.</param>
/// <param name="PremiosAReceber">Soma dos prêmios dos jogos ainda dentro do prazo para receber.</param>
public sealed record ResumoPainel(
    int TotalConcursos,
    Concurso UltimoConcurso,
    EstatisticasDezenas Estatisticas,
    GrupoJogos? JogosUltimoConcurso,
    int JogosAguardando,
    IReadOnlyList<int> ConcursosAguardando,
    decimal PremiosAReceber)
{
    /// <summary>
    /// Número do próximo concurso: o seguinte ao último gravado.
    /// </summary>
    public int ProximoConcurso => this.UltimoConcurso.Numero + 1;
}
