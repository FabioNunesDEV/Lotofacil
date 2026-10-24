// <copyright file="JogoTeimosinha.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Dominio.Jogos;

namespace Lotofacil.Dominio.Teimosinhas;

/// <summary>
/// Jogo de uma Teimosinha: as dezenas repetidas em todo concurso a partir do início.
/// </summary>
public sealed class JogoTeimosinha
{
    /// <summary>
    /// Cria o jogo; lança <see cref="ArgumentException"/> se as dezenas não atenderem às regras de um jogo.
    /// </summary>
    /// <param name="dezenas">As 15 a 20 dezenas, em qualquer ordem.</param>
    /// <param name="concursoInicial">Primeiro concurso em que o jogo vale.</param>
    public JogoTeimosinha(IEnumerable<int> dezenas, int concursoInicial)
    {
        this.Dezenas = Jogo.OrdenarEValidar(dezenas);
        this.ConcursoInicial = concursoInicial;
    }

    /// <summary>
    /// Usado apenas pelo EF Core para recriar o jogo a partir do banco.
    /// </summary>
    private JogoTeimosinha()
    {
        this.Dezenas = [];
    }

    /// <summary>
    /// Identificador na base; zero antes de gravado.
    /// </summary>
    public int Id { get; private set; }

    /// <summary>
    /// As 15 a 20 dezenas, em ordem crescente.
    /// </summary>
    public IReadOnlyList<int> Dezenas { get; private set; }

    /// <summary>
    /// Primeiro concurso em que o jogo vale.
    /// </summary>
    public int ConcursoInicial { get; private set; }
}
