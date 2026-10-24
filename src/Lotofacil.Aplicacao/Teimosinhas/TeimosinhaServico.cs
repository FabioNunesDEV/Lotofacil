// <copyright file="TeimosinhaServico.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Aplicacao.Jogos;
using Lotofacil.Dominio.Jogos;
using Lotofacil.Dominio.Teimosinhas;

namespace Lotofacil.Aplicacao.Teimosinhas;

/// <summary>
/// Registra as Teimosinhas e cria os jogos delas em cada concurso, até o próximo.
/// </summary>
public class TeimosinhaServico
{
    /// <summary>
    /// Repositório das Teimosinhas.
    /// </summary>
    private readonly ITeimosinhaRepositorio teimosinhas;

    /// <summary>
    /// Repositório dos jogos.
    /// </summary>
    private readonly IJogoRepositorio jogos;

    /// <summary>
    /// Serviço dos jogos, com a verificação do concurso e o próximo concurso.
    /// </summary>
    private readonly JogosServico servicoJogos;

    /// <summary>
    /// Cria o serviço das Teimosinhas.
    /// </summary>
    /// <param name="teimosinhas">Repositório das Teimosinhas.</param>
    /// <param name="jogos">Repositório dos jogos.</param>
    /// <param name="servicoJogos">Serviço dos jogos.</param>
    public TeimosinhaServico(ITeimosinhaRepositorio teimosinhas, IJogoRepositorio jogos, JogosServico servicoJogos)
    {
        this.teimosinhas = teimosinhas;
        this.jogos = jogos;
        this.servicoJogos = servicoJogos;
    }

    /// <summary>
    /// Grava jogos para o concurso como jogos da Teimosinha com o nome informado: cria a Teimosinha se não houver
    /// ativa com esse nome, ou acrescenta os jogos a ela. Jogos já cadastrados no concurso ou já na Teimosinha são
    /// recusados; se nada for gravado, a Teimosinha não muda.
    /// </summary>
    /// <param name="nome">Nome da Teimosinha.</param>
    /// <param name="concurso">Concurso dos jogos, que é o início deles na Teimosinha.</param>
    /// <param name="dezenasJogos">Dezenas de cada jogo (15 a 20 por jogo).</param>
    /// <param name="cancelamento">Sinal de cancelamento.</param>
    /// <returns>Quantos jogos foram gravados e recusados, ou o motivo de nada ter sido gravado.</returns>
    public async Task<ResultadoSalvarTeimosinha> SalvarAsync(
        string nome,
        int concurso,
        IReadOnlyList<IReadOnlyCollection<int>> dezenasJogos,
        CancellationToken cancelamento = default)
    {
        string nomeNormalizado;
        try
        {
            nomeNormalizado = Teimosinha.NormalizarNome(nome);
        }
        catch (ArgumentException)
        {
            return new ResultadoSalvarTeimosinha(0, 0, "O nome da Teimosinha deve ter de 1 a 40 caracteres.", nome.Trim());
        }

        string? recusa = await this.servicoJogos.VerificarConcursoAsync(concurso, cancelamento);
        if (recusa is not null)
        {
            return new ResultadoSalvarTeimosinha(0, 0, recusa, nomeNormalizado);
        }

        IReadOnlyList<Teimosinha> ativas = await this.teimosinhas.ListarAtivasAsync(cancelamento);
        Teimosinha teimosinha = ativas.FirstOrDefault(ativa => ativa.NomeIgual(nomeNormalizado))
            ?? new Teimosinha(nomeNormalizado, concurso);

        List<List<int>> aceitos = [];
        string? motivoUnico = null;
        foreach (IReadOnlyCollection<int> dezenas in dezenasJogos)
        {
            List<int> ordenadas = Jogo.OrdenarEValidar(dezenas);
            if (teimosinha.Contem(ordenadas) || aceitos.Exists(aceito => aceito.SequenceEqual(ordenadas)))
            {
                motivoUnico = $"Este jogo já faz parte da Teimosinha {teimosinha.Nome}.";
            }
            else if (await this.jogos.ExisteAsync(concurso, ordenadas, cancelamento))
            {
                motivoUnico = $"Este jogo já está cadastrado para o concurso {concurso}.";
            }
            else
            {
                aceitos.Add(ordenadas);
            }
        }

        int duplicados = dezenasJogos.Count - aceitos.Count;
        if (aceitos.Count == 0)
        {
            string motivo = dezenasJogos.Count == 1 && motivoUnico is not null
                ? motivoUnico
                : $"Nenhum jogo salvo: todos já estavam cadastrados para o concurso {concurso} ou na Teimosinha {teimosinha.Nome}.";

            return new ResultadoSalvarTeimosinha(0, duplicados, motivo, teimosinha.Nome);
        }

        foreach (List<int> dezenas in aceitos)
        {
            teimosinha.Acrescentar(dezenas, concurso);
        }

        await this.teimosinhas.SalvarAsync(teimosinha, cancelamento);

        DateTime agora = DateTime.Now;
        foreach (List<int> dezenas in aceitos)
        {
            await this.jogos.AdicionarAsync(new Jogo(concurso, dezenas, agora, teimosinha.Id), cancelamento);
        }

        return new ResultadoSalvarTeimosinha(aceitos.Count, duplicados, null, teimosinha.Nome);
    }

    /// <summary>
    /// Cria, para cada Teimosinha ativa, os jogos de cada concurso depois do último criado até o próximo concurso,
    /// pulando os já cadastrados. Pode ser chamado várias vezes sem duplicar.
    /// </summary>
    /// <param name="cancelamento">Sinal de cancelamento.</param>
    /// <returns>Quantidade de jogos criados e a faixa de concursos.</returns>
    public async Task<JogosCriadosTeimosinha> CriarJogosAsync(CancellationToken cancelamento = default)
    {
        if (await this.servicoJogos.ObterProximoConcursoAsync(cancelamento) is not int proximo)
        {
            return JogosCriadosTeimosinha.Nenhum;
        }

        DateTime agora = DateTime.Now;
        int quantidade = 0;
        int primeiro = int.MaxValue;
        int ultimo = 0;
        foreach (Teimosinha teimosinha in await this.teimosinhas.ListarAtivasAsync(cancelamento))
        {
            if (teimosinha.UltimoConcursoCriado >= proximo)
            {
                continue;
            }

            for (int concurso = teimosinha.UltimoConcursoCriado + 1; concurso <= proximo; concurso++)
            {
                foreach (JogoTeimosinha jogo in teimosinha.JogosAte(concurso))
                {
                    if (await this.jogos.ExisteAsync(concurso, jogo.Dezenas, cancelamento))
                    {
                        continue;
                    }

                    await this.jogos.AdicionarAsync(new Jogo(concurso, jogo.Dezenas, agora, teimosinha.Id), cancelamento);
                    quantidade++;
                    primeiro = Math.Min(primeiro, concurso);
                    ultimo = Math.Max(ultimo, concurso);
                }
            }

            teimosinha.RegistrarCriacao(proximo);
            await this.teimosinhas.SalvarAsync(teimosinha, cancelamento);
        }

        return quantidade == 0 ? JogosCriadosTeimosinha.Nenhum : new JogosCriadosTeimosinha(quantidade, primeiro, ultimo);
    }

    /// <summary>
    /// Lista as Teimosinhas ativas, com os jogos, em ordem de nome.
    /// </summary>
    /// <param name="cancelamento">Sinal de cancelamento.</param>
    /// <returns>Teimosinhas ativas.</returns>
    public Task<IReadOnlyList<Teimosinha>> ListarAtivasAsync(CancellationToken cancelamento = default)
    {
        return this.teimosinhas.ListarAtivasAsync(cancelamento);
    }

    /// <summary>
    /// Lista o nome de todas as Teimosinhas, inclusive as removidas, para marcar os jogos criados por elas.
    /// </summary>
    /// <param name="cancelamento">Sinal de cancelamento.</param>
    /// <returns>Nome por identificador.</returns>
    public Task<IReadOnlyDictionary<int, string>> ListarNomesAsync(CancellationToken cancelamento = default)
    {
        return this.teimosinhas.ListarNomesAsync(cancelamento);
    }

    /// <summary>
    /// Remove a Teimosinha: ela deixa de criar jogos, e os já criados continuam; não faz nada se ela não existir.
    /// </summary>
    /// <param name="id">Identificador da Teimosinha.</param>
    /// <param name="cancelamento">Sinal de cancelamento.</param>
    /// <returns>Tarefa da remoção.</returns>
    public async Task RemoverAsync(int id, CancellationToken cancelamento = default)
    {
        Teimosinha? teimosinha = await this.teimosinhas.ObterAsync(id, cancelamento);
        if (teimosinha is null)
        {
            return;
        }

        teimosinha.Remover();
        await this.teimosinhas.SalvarAsync(teimosinha, cancelamento);
    }
}
