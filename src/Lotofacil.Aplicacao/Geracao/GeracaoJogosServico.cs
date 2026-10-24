// <copyright file="GeracaoJogosServico.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Aplicacao.Estatisticas;
using Lotofacil.Aplicacao.Jogos;
using Lotofacil.Dominio.Concursos;
using Lotofacil.Dominio.Estatisticas;
using Lotofacil.Dominio.Geracao;

namespace Lotofacil.Aplicacao.Geracao;

/// <summary>
/// Gera jogos a partir das estatísticas da janela pedida e salva os escolhidos pelo serviço de jogos.
/// </summary>
public class GeracaoJogosServico
{
    /// <summary>
    /// Mensagem de recusa quando a base não tem concursos.
    /// </summary>
    public const string MensagemBaseVazia =
        "Nenhum concurso gravado na base local. Use “Importar histórico” na página Resultados para carregar os concursos da Caixa.";

    /// <summary>
    /// Repositório dos concursos.
    /// </summary>
    private readonly IConcursoRepositorio concursos;

    /// <summary>
    /// Serviço das estatísticas das dezenas.
    /// </summary>
    private readonly EstatisticasServico estatisticas;

    /// <summary>
    /// Serviço dos jogos, que grava com as regras do cadastro manual.
    /// </summary>
    private readonly JogosServico jogos;

    /// <summary>
    /// Fonte dos números aleatórios do gerador.
    /// </summary>
    private readonly Random aleatorio;

    /// <summary>
    /// Cria o serviço de geração.
    /// </summary>
    /// <param name="concursos">Repositório dos concursos.</param>
    /// <param name="estatisticas">Serviço das estatísticas das dezenas.</param>
    /// <param name="jogos">Serviço dos jogos.</param>
    /// <param name="aleatorio">Fonte dos números aleatórios.</param>
    public GeracaoJogosServico(
        IConcursoRepositorio concursos,
        EstatisticasServico estatisticas,
        JogosServico jogos,
        Random aleatorio)
    {
        this.concursos = concursos;
        this.estatisticas = estatisticas;
        this.jogos = jogos;
        this.aleatorio = aleatorio;
    }

    /// <summary>
    /// Obtém o último concurso gravado e o total de concursos.
    /// </summary>
    /// <param name="cancelamento">Sinal de cancelamento.</param>
    /// <returns>O contexto, ou <c>null</c> com a base vazia.</returns>
    public async Task<ContextoGeracao?> ObterContextoAsync(CancellationToken cancelamento = default)
    {
        int? ultimoNumero = await this.concursos.ObterUltimoNumeroAsync(cancelamento);
        if (ultimoNumero is not int numero)
        {
            return null;
        }

        Concurso? ultimo = await this.concursos.ObterAsync(numero, cancelamento);
        if (ultimo is null)
        {
            return null;
        }

        int total = await this.concursos.ContarAsync(cancelamento);

        return new ContextoGeracao(numero, ultimo.Dezenas, total);
    }

    /// <summary>
    /// Gera os jogos com as estatísticas da janela, com o perfil de cada um em relação ao último concurso gravado.
    /// </summary>
    /// <param name="parametros">Quantidade, tamanho, estratégia, fixas, excluídas e filtros.</param>
    /// <param name="ultimosConcursos">Janela da frequência, ou <c>null</c> para todo o histórico.</param>
    /// <param name="cancelamento">Sinal de cancelamento.</param>
    /// <returns>Os jogos gerados, o aviso de poucas combinações ou o motivo da recusa.</returns>
    public async Task<JogosGerados> GerarAsync(
        ParametrosGeracao parametros,
        int? ultimosConcursos,
        CancellationToken cancelamento = default)
    {
        ContextoGeracao? contexto = await this.ObterContextoAsync(cancelamento);
        EstatisticasDezenas? dados = await this.estatisticas.CalcularAsync(ultimosConcursos, cancelamento);
        if (contexto is null || dados is null)
        {
            return new JogosGerados([], null, MensagemBaseVazia);
        }

        ResultadoGeracao resultado =
            new GeradorJogos(this.aleatorio).Gerar(parametros, dados.Dezenas, contexto.UltimoResultado);
        List<JogoGerado> gerados = resultado.Jogos
            .Select(jogo => new JogoGerado(jogo, PerfilJogo.Calcular(jogo, contexto.UltimoResultado)))
            .ToList();

        return new JogosGerados(
            gerados,
            resultado.CombinacoesPossiveis,
            resultado.Recusa,
            resultado.ComFiltros,
            resultado.JogosPedidos);
    }

    /// <summary>
    /// Salva os jogos para o concurso pelas regras do cadastro manual.
    /// </summary>
    /// <param name="numeroConcurso">Número do concurso.</param>
    /// <param name="jogosGerados">Dezenas de cada jogo.</param>
    /// <param name="cancelamento">Sinal de cancelamento.</param>
    /// <returns>Quantos foram gravados e quantos eram duplicados, ou o motivo da recusa.</returns>
    public Task<ResultadoCadastroVarios> SalvarAsync(
        int numeroConcurso,
        IReadOnlyList<IReadOnlyCollection<int>> jogosGerados,
        CancellationToken cancelamento = default)
    {
        return this.jogos.CadastrarVariosAsync(numeroConcurso, jogosGerados, cancelamento);
    }
}
