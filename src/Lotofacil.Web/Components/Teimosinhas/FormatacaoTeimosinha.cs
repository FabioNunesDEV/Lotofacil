// <copyright file="FormatacaoTeimosinha.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Aplicacao.Teimosinhas;
using Lotofacil.Web.Components.Jogos;

namespace Lotofacil.Web.Components.Teimosinhas;

/// <summary>
/// Textos exibidos sobre as Teimosinhas.
/// </summary>
public static class FormatacaoTeimosinha
{
    /// <summary>
    /// Rótulo do switch que marca os jogos como Teimosinha.
    /// </summary>
    public const string RotuloSwitch = "Teimosinha (repetir em todo concurso)";

    /// <summary>
    /// Mensagem da página sem Teimosinhas ativas.
    /// </summary>
    public const string NenhumaAtiva =
        "Nenhuma Teimosinha ativa. Ligue “Teimosinha” ao salvar jogos em Meus jogos ou em Gerar jogos.";

    /// <summary>
    /// Mensagem depois de remover uma Teimosinha.
    /// </summary>
    public const string Removida = "Teimosinha removida.";

    /// <summary>
    /// Aviso dos jogos criados pelas Teimosinhas depois de uma atualização.
    /// </summary>
    /// <param name="criados">Quantidade e faixa de concursos; com quantidade maior que zero.</param>
    /// <returns>"Teimosinhas: 2 jogos criados para o concurso 3814." ou a variação com vários concursos.</returns>
    public static string Criados(JogosCriadosTeimosinha criados)
    {
        string jogos = criados.Quantidade == 1 ? "1 jogo criado" : $"{criados.Quantidade:N0} jogos criados";
        string concursos = criados.PrimeiroConcurso == criados.UltimoConcurso
            ? $"o concurso {criados.PrimeiroConcurso}"
            : $"os concursos {criados.PrimeiroConcurso} a {criados.UltimoConcurso}";

        return $"Teimosinhas: {jogos} para {concursos}.";
    }

    /// <summary>
    /// Concurso de início da Teimosinha.
    /// </summary>
    /// <param name="concurso">Menor início entre os jogos.</param>
    /// <returns>"Desde o concurso 3813".</returns>
    public static string Desde(int concurso)
    {
        return $"Desde o concurso {concurso}";
    }

    /// <summary>
    /// Quantidade de jogos e custo de cada concurso.
    /// </summary>
    /// <param name="quantidadeJogos">Quantidade de jogos da Teimosinha.</param>
    /// <param name="custo">Custo dos jogos em um concurso.</param>
    /// <returns>"2 jogos · R$ 59,50 por concurso".</returns>
    public static string JogosECusto(int quantidadeJogos, decimal custo)
    {
        return $"{FormatacaoJogo.QuantidadeJogos(quantidadeJogos)} · {FormatacaoJogo.Reais(custo)} por concurso";
    }

    /// <summary>
    /// Mensagem do cadastro manual de um jogo na Teimosinha.
    /// </summary>
    /// <param name="concurso">Concurso do jogo.</param>
    /// <param name="nome">Nome da Teimosinha.</param>
    /// <returns>"Jogo salvo para o concurso 3813 na Teimosinha Minhas fixas.".</returns>
    public static string JogoSalvo(int concurso, string nome)
    {
        return $"Jogo salvo para o concurso {concurso} na Teimosinha {nome}.";
    }

    /// <summary>
    /// Marca dos jogos criados por uma Teimosinha em Meus jogos.
    /// </summary>
    /// <param name="nome">Nome da Teimosinha.</param>
    /// <returns>"Teimosinha · Minhas fixas".</returns>
    public static string Marca(string nome)
    {
        return $"Teimosinha · {nome}";
    }
}
