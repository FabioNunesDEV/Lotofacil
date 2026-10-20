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
    /// <param name="dezenas">As 15 dezenas do jogo.</param>
    /// <param name="cancelamento">Sinal de cancelamento.</param>
    /// <returns>Se o jogo foi gravado e a mensagem para o usuário.</returns>
    public async Task<ResultadoCadastro> CadastrarAsync(
        int numeroConcurso,
        IReadOnlyCollection<int> dezenas,
        CancellationToken cancelamento = default)
    {
        int? ultimoGravado = await this.concursos.ObterUltimoNumeroAsync(cancelamento);
        if (ultimoGravado is null)
        {
            return new ResultadoCadastro(false, "Importe o histórico na página Resultados antes de cadastrar jogos.");
        }

        if (numeroConcurso <= ultimoGravado)
        {
            return new ResultadoCadastro(
                false,
                $"Só é possível cadastrar jogos para concursos depois do {ultimoGravado}.");
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
}
