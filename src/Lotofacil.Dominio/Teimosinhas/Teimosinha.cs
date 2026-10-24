// <copyright file="Teimosinha.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

namespace Lotofacil.Dominio.Teimosinhas;

/// <summary>
/// Grupo de jogos com nome repetido em todo concurso, até o usuário remover.
/// </summary>
public sealed class Teimosinha
{
    /// <summary>
    /// Tamanho máximo do nome.
    /// </summary>
    public const int TamanhoMaximoNome = 40;

    /// <summary>
    /// Jogos da Teimosinha, na ordem em que foram acrescentados.
    /// </summary>
    private readonly List<JogoTeimosinha> jogos = [];

    /// <summary>
    /// Cria uma Teimosinha ativa, sem jogos; lança <see cref="ArgumentException"/> se o nome for inválido.
    /// </summary>
    /// <param name="nome">Nome da Teimosinha.</param>
    /// <param name="primeiroConcurso">Concurso cujos jogos são criados ao registrar.</param>
    public Teimosinha(string nome, int primeiroConcurso)
    {
        this.Nome = NormalizarNome(nome);
        this.Ativa = true;
        this.UltimoConcursoCriado = primeiroConcurso;
    }

    /// <summary>
    /// Usado apenas pelo EF Core para recriar a Teimosinha a partir do banco.
    /// </summary>
    private Teimosinha()
    {
        this.Nome = string.Empty;
    }

    /// <summary>
    /// Identificador na base; zero antes de gravada.
    /// </summary>
    public int Id { get; private set; }

    /// <summary>
    /// Nome da Teimosinha, sem espaços nas pontas.
    /// </summary>
    public string Nome { get; private set; }

    /// <summary>
    /// Indica que a Teimosinha ainda cria jogos; falso depois de removida.
    /// </summary>
    public bool Ativa { get; private set; }

    /// <summary>
    /// Último concurso para o qual os jogos da Teimosinha foram criados.
    /// </summary>
    public int UltimoConcursoCriado { get; private set; }

    /// <summary>
    /// Jogos da Teimosinha, na ordem em que foram acrescentados.
    /// </summary>
    public IReadOnlyList<JogoTeimosinha> Jogos => this.jogos;

    /// <summary>
    /// Menor concurso de início entre os jogos, ou <c>null</c> sem jogos.
    /// </summary>
    public int? Desde => this.jogos.Count == 0 ? null : this.jogos.Min(jogo => jogo.ConcursoInicial);

    /// <summary>
    /// Apara o nome e confere o tamanho; lança <see cref="ArgumentException"/> se ficar vazio ou longo demais.
    /// </summary>
    /// <param name="nome">Nome informado.</param>
    /// <returns>Nome sem espaços nas pontas.</returns>
    public static string NormalizarNome(string nome)
    {
        string aparado = nome.Trim();
        if (aparado.Length == 0 || aparado.Length > TamanhoMaximoNome)
        {
            throw new ArgumentException("O nome da Teimosinha deve ter de 1 a 40 caracteres.", nameof(nome));
        }

        return aparado;
    }

    /// <summary>
    /// Indica se o nome é o desta Teimosinha, sem diferenciar maiúsculas e minúsculas.
    /// </summary>
    /// <param name="nome">Nome a comparar.</param>
    /// <returns><c>true</c> se for o mesmo nome.</returns>
    public bool NomeIgual(string nome)
    {
        return string.Equals(this.Nome, nome.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Indica se a Teimosinha já tem um jogo com as mesmas dezenas.
    /// </summary>
    /// <param name="dezenas">Dezenas, em qualquer ordem.</param>
    /// <returns><c>true</c> se o jogo já faz parte da Teimosinha.</returns>
    public bool Contem(IEnumerable<int> dezenas)
    {
        List<int> ordenadas = dezenas.Order().ToList();

        return this.jogos.Any(jogo => jogo.Dezenas.SequenceEqual(ordenadas));
    }

    /// <summary>
    /// Acrescenta um jogo; lança <see cref="ArgumentException"/> se as dezenas forem inválidas ou já fizerem parte da
    /// Teimosinha.
    /// </summary>
    /// <param name="dezenas">As 15 a 20 dezenas, em qualquer ordem.</param>
    /// <param name="concursoInicial">Primeiro concurso em que o jogo vale.</param>
    public void Acrescentar(IEnumerable<int> dezenas, int concursoInicial)
    {
        var jogo = new JogoTeimosinha(dezenas, concursoInicial);
        if (this.Contem(jogo.Dezenas))
        {
            throw new ArgumentException($"O jogo já faz parte da Teimosinha {this.Nome}.", nameof(dezenas));
        }

        this.jogos.Add(jogo);
    }

    /// <summary>
    /// Jogos que valem para o concurso: os com início até ele.
    /// </summary>
    /// <param name="concurso">Número do concurso.</param>
    /// <returns>Jogos que valem no concurso.</returns>
    public IReadOnlyList<JogoTeimosinha> JogosAte(int concurso)
    {
        return this.jogos.Where(jogo => jogo.ConcursoInicial <= concurso).ToList();
    }

    /// <summary>
    /// Registra que os jogos foram criados até o concurso informado.
    /// </summary>
    /// <param name="concurso">Último concurso com os jogos criados.</param>
    public void RegistrarCriacao(int concurso)
    {
        this.UltimoConcursoCriado = Math.Max(this.UltimoConcursoCriado, concurso);
    }

    /// <summary>
    /// Remove a Teimosinha: ela deixa de criar jogos, e os já criados continuam.
    /// </summary>
    public void Remover()
    {
        this.Ativa = false;
    }
}
