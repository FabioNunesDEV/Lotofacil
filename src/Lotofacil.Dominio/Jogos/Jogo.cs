// <copyright file="Jogo.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

namespace Lotofacil.Dominio.Jogos;

/// <summary>
/// Jogo do usuário: as dezenas apostadas em um concurso.
/// </summary>
public sealed class Jogo
{
    /// <summary>
    /// Cria um jogo válido; lança <see cref="ArgumentException"/> se alguma regra não for atendida.
    /// </summary>
    /// <param name="numeroConcurso">Número do concurso a que o jogo se destina, maior que zero.</param>
    /// <param name="dezenas">As 15 dezenas do jogo, em qualquer ordem.</param>
    /// <param name="cadastradoEm">Momento do cadastro.</param>
    public Jogo(int numeroConcurso, IEnumerable<int> dezenas, DateTime cadastradoEm)
    {
        if (numeroConcurso <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(numeroConcurso), "O número do concurso deve ser maior que zero.");
        }

        List<int> dezenasOrdenadas = dezenas.Order().ToList();
        if (dezenasOrdenadas.Count != RegrasLotofacil.DezenasPorJogo)
        {
            throw new ArgumentException(
                $"O jogo deve ter 15 dezenas; foram informadas {dezenasOrdenadas.Count}.",
                nameof(dezenas));
        }

        if (dezenasOrdenadas.Any(dezena => dezena < RegrasLotofacil.DezenaMinima || dezena > RegrasLotofacil.DezenaMaxima))
        {
            throw new ArgumentOutOfRangeException(nameof(dezenas), "As dezenas do jogo vão de 1 a 25.");
        }

        if (dezenasOrdenadas.Distinct().Count() != dezenasOrdenadas.Count)
        {
            throw new ArgumentException("O jogo não pode ter dezena repetida.", nameof(dezenas));
        }

        this.NumeroConcurso = numeroConcurso;
        this.Dezenas = dezenasOrdenadas;
        this.CadastradoEm = cadastradoEm;
    }

    /// <summary>
    /// Usado apenas pelo EF Core para recriar o jogo a partir do banco.
    /// </summary>
    private Jogo()
    {
        this.Dezenas = [];
    }

    /// <summary>
    /// Identificador do jogo na base; zero antes de gravado.
    /// </summary>
    public int Id { get; private set; }

    /// <summary>
    /// Número do concurso a que o jogo se destina.
    /// </summary>
    public int NumeroConcurso { get; private set; }

    /// <summary>
    /// As 15 dezenas do jogo, em ordem crescente.
    /// </summary>
    public IReadOnlyList<int> Dezenas { get; private set; }

    /// <summary>
    /// Momento do cadastro, que define a ordem dos jogos no concurso.
    /// </summary>
    public DateTime CadastradoEm { get; private set; }
}
