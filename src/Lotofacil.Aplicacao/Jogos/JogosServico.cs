// <copyright file="JogosServico.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Dominio.Concursos;
using Lotofacil.Dominio.Jogos;

namespace Lotofacil.Aplicacao.Jogos;

/// <summary>
/// Cadastra, lista conferidos e exclui os jogos do usuário.
/// </summary>
public class JogosServico
{
    /// <summary>
    /// Repositório dos jogos.
    /// </summary>
    private readonly IJogoRepositorio jogos;

    /// <summary>
    /// Repositório dos concursos, usado para saber o último gravado e conferir os jogos.
    /// </summary>
    private readonly IConcursoRepositorio concursos;

    /// <summary>
    /// Cria o serviço de jogos.
    /// </summary>
    /// <param name="jogos">Repositório dos jogos.</param>
    /// <param name="concursos">Repositório dos concursos.</param>
    public JogosServico(IJogoRepositorio jogos, IConcursoRepositorio concursos)
    {
        this.jogos = jogos;
        this.concursos = concursos;
    }

    /// <summary>
    /// Cadastra um jogo para um concurso futuro, recusando base vazia, concurso já sorteado e jogo duplicado.
    /// </summary>
    /// <param name="numeroConcurso">Número do concurso.</param>
    /// <param name="dezenas">As 15 a 20 dezenas do jogo.</param>
    /// <param name="cancelamento">Sinal de cancelamento.</param>
    /// <returns>Se o jogo foi gravado e a mensagem para o usuário.</returns>
    public async Task<ResultadoCadastro> CadastrarAsync(
        int numeroConcurso,
        IReadOnlyCollection<int> dezenas,
        CancellationToken cancelamento = default)
    {
        string? recusa = await this.VerificarConcursoAsync(numeroConcurso, cancelamento);
        if (recusa is not null)
        {
            return new ResultadoCadastro(false, recusa);
        }

        var jogo = new Jogo(numeroConcurso, dezenas, DateTime.Now);
        if (await this.jogos.ExisteAsync(numeroConcurso, jogo.Dezenas, cancelamento))
        {
            return new ResultadoCadastro(false, $"Este jogo já está cadastrado para o concurso {numeroConcurso}.");
        }

        await this.jogos.AdicionarAsync(jogo, cancelamento);
        return new ResultadoCadastro(true, $"Jogo salvo para o concurso {numeroConcurso}.");
    }

    /// <summary>
    /// Cadastra vários jogos para um concurso futuro: recusa tudo com base vazia ou concurso já sorteado, recusa os
    /// duplicados e grava os demais.
    /// </summary>
    /// <param name="numeroConcurso">Número do concurso.</param>
    /// <param name="jogos">Dezenas de cada jogo (15 a 20 por jogo).</param>
    /// <param name="cancelamento">Sinal de cancelamento.</param>
    /// <returns>Quantos jogos foram gravados e quantos eram duplicados, ou o motivo da recusa.</returns>
    public async Task<ResultadoCadastroVarios> CadastrarVariosAsync(
        int numeroConcurso,
        IReadOnlyList<IReadOnlyCollection<int>> jogos,
        CancellationToken cancelamento = default)
    {
        string? recusa = await this.VerificarConcursoAsync(numeroConcurso, cancelamento);
        if (recusa is not null)
        {
            return new ResultadoCadastroVarios(0, 0, recusa);
        }

        DateTime agora = DateTime.Now;
        int gravados = 0;
        int duplicados = 0;
        foreach (IReadOnlyCollection<int> dezenas in jogos)
        {
            var jogo = new Jogo(numeroConcurso, dezenas, agora);
            if (await this.jogos.ExisteAsync(numeroConcurso, jogo.Dezenas, cancelamento))
            {
                duplicados++;
                continue;
            }

            await this.jogos.AdicionarAsync(jogo, cancelamento);
            gravados++;
        }

        return new ResultadoCadastroVarios(gravados, duplicados, null);
    }

    /// <summary>
    /// Sugere o concurso de um jogo novo: o seguinte ao último gravado.
    /// </summary>
    /// <param name="cancelamento">Sinal de cancelamento.</param>
    /// <returns>Último concurso gravado + 1, ou <c>null</c> com a base vazia.</returns>
    public async Task<int?> ObterProximoConcursoAsync(CancellationToken cancelamento = default)
    {
        int? ultimoGravado = await this.concursos.ObterUltimoNumeroAsync(cancelamento);

        return ultimoGravado + 1;
    }

    /// <summary>
    /// Lista os jogos agrupados por concurso, do maior número para o menor, conferindo os de concursos sorteados.
    /// </summary>
    /// <param name="cancelamento">Sinal de cancelamento.</param>
    /// <returns>Grupos de jogos.</returns>
    public async Task<IReadOnlyList<GrupoJogos>> ListarAsync(CancellationToken cancelamento = default)
    {
        IReadOnlyList<Jogo> todos = await this.jogos.ListarAsync(cancelamento);
        List<GrupoJogos> grupos = [];

        foreach (IGrouping<int, Jogo> doConcurso in todos.GroupBy(jogo => jogo.NumeroConcurso).OrderByDescending(grupo => grupo.Key))
        {
            Concurso? concurso = await this.concursos.ObterAsync(doConcurso.Key, cancelamento);
            List<JogoConferido> conferidos = doConcurso
                .Select((jogo, indice) => new JogoConferido(
                    jogo,
                    indice + 1,
                    concurso is null ? null : ConferenciaJogo.Conferir(jogo, concurso)))
                .ToList();

            grupos.Add(new GrupoJogos(doConcurso.Key, concurso, conferidos));
        }

        return grupos;
    }

    /// <summary>
    /// Exclui um jogo.
    /// </summary>
    /// <param name="id">Identificador do jogo.</param>
    /// <param name="cancelamento">Sinal de cancelamento.</param>
    /// <returns>Tarefa da exclusão.</returns>
    public Task ExcluirAsync(int id, CancellationToken cancelamento = default)
    {
        return this.jogos.ExcluirAsync(id, cancelamento);
    }

    /// <summary>
    /// Confere se é possível cadastrar jogos para o concurso: base com concursos e concurso ainda não sorteado.
    /// </summary>
    /// <param name="numeroConcurso">Número do concurso.</param>
    /// <param name="cancelamento">Sinal de cancelamento.</param>
    /// <returns>O motivo da recusa, ou <c>null</c> quando o concurso aceita jogos.</returns>
    private async Task<string?> VerificarConcursoAsync(int numeroConcurso, CancellationToken cancelamento)
    {
        int? ultimoGravado = await this.concursos.ObterUltimoNumeroAsync(cancelamento);
        if (ultimoGravado is null)
        {
            return "Importe o histórico na página Resultados antes de cadastrar jogos.";
        }

        return numeroConcurso <= ultimoGravado
            ? $"Só é possível cadastrar jogos para concursos depois do {ultimoGravado}."
            : null;
    }
}
