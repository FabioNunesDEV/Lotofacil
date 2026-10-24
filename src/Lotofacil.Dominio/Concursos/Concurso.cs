// <copyright file="Concurso.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

namespace Lotofacil.Dominio.Concursos;

/// <summary>
/// Resultado de um concurso da Lotofácil: dezenas sorteadas, rateio das faixas e valores do concurso.
/// </summary>
public sealed class Concurso
{
    /// <summary>
    /// Rateio das faixas de 11 a 15 acertos, em ordem de faixa.
    /// </summary>
    private readonly List<Rateio> rateios = [];

    /// <summary>
    /// Cria um concurso válido; lança <see cref="ArgumentException"/> se alguma regra não for atendida.
    /// </summary>
    /// <param name="numero">Número do concurso, maior que zero.</param>
    /// <param name="data">Data do sorteio.</param>
    /// <param name="dezenas">As 15 dezenas sorteadas, em qualquer ordem.</param>
    /// <param name="ordemSorteio">As mesmas dezenas na ordem do sorteio, ou <c>null</c> se não for conhecida.</param>
    /// <param name="rateios">Rateio com uma entrada para cada faixa de 11 a 15 acertos.</param>
    /// <param name="arrecadacao">Arrecadação total; zero quando não informada.</param>
    /// <param name="acumulado15">Valor acumulado para a faixa de 15 acertos.</param>
    /// <param name="estimativaProximo">Estimativa de prêmio do próximo concurso.</param>
    /// <param name="acumuladoIndependencia">Valor acumulado para a Lotofácil da Independência.</param>
    public Concurso(
        int numero,
        DateOnly data,
        IEnumerable<int> dezenas,
        IEnumerable<int>? ordemSorteio,
        IEnumerable<Rateio> rateios,
        decimal arrecadacao,
        decimal acumulado15,
        decimal estimativaProximo,
        decimal acumuladoIndependencia)
    {
        if (numero <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(numero), "O número do concurso deve ser maior que zero.");
        }

        List<int> dezenasOrdenadas = dezenas.Order().ToList();
        ValidarDezenas(dezenasOrdenadas);

        List<int>? ordem = ordemSorteio?.ToList();
        if (ordem is not null)
        {
            ValidarOrdem(ordem, dezenasOrdenadas, nameof(ordemSorteio));
        }

        List<Rateio> rateiosOrdenados = rateios.OrderBy(rateio => rateio.Faixa).ToList();
        ValidarFaixas(rateiosOrdenados);

        ValidarNaoNegativo(arrecadacao, "da arrecadação", nameof(arrecadacao));
        ValidarNaoNegativo(acumulado15, "acumulado da faixa de 15 acertos", nameof(acumulado15));
        ValidarNaoNegativo(estimativaProximo, "da estimativa do próximo concurso", nameof(estimativaProximo));
        ValidarNaoNegativo(acumuladoIndependencia, "acumulado da Lotofácil da Independência", nameof(acumuladoIndependencia));

        this.Numero = numero;
        this.Data = data;
        this.Dezenas = dezenasOrdenadas;
        this.OrdemSorteio = ordem;
        this.rateios = rateiosOrdenados;
        this.Arrecadacao = arrecadacao;
        this.Acumulado15 = acumulado15;
        this.EstimativaProximo = estimativaProximo;
        this.AcumuladoIndependencia = acumuladoIndependencia;
    }

    /// <summary>
    /// Usado apenas pelo EF Core para recriar o concurso a partir do banco.
    /// </summary>
    private Concurso()
    {
        this.Dezenas = [];
    }

    /// <summary>
    /// Número do concurso.
    /// </summary>
    public int Numero { get; private set; }

    /// <summary>
    /// Data do sorteio.
    /// </summary>
    public DateOnly Data { get; private set; }

    /// <summary>
    /// As 15 dezenas sorteadas, em ordem crescente.
    /// </summary>
    public IReadOnlyList<int> Dezenas { get; private set; }

    /// <summary>
    /// As dezenas na ordem em que foram sorteadas, ou <c>null</c> quando a ordem não é conhecida.
    /// </summary>
    public IReadOnlyList<int>? OrdemSorteio { get; private set; }

    /// <summary>
    /// Rateio das faixas de 11 a 15 acertos, em ordem crescente de faixa.
    /// </summary>
    public IReadOnlyList<Rateio> Rateios => this.rateios;

    /// <summary>
    /// Arrecadação total; zero quando a Caixa não informou o valor.
    /// </summary>
    public decimal Arrecadacao { get; private set; }

    /// <summary>
    /// Valor acumulado para a faixa de 15 acertos do próximo concurso.
    /// </summary>
    public decimal Acumulado15 { get; private set; }

    /// <summary>
    /// Estimativa de prêmio do próximo concurso.
    /// </summary>
    public decimal EstimativaProximo { get; private set; }

    /// <summary>
    /// Valor acumulado para a Lotofácil da Independência.
    /// </summary>
    public decimal AcumuladoIndependencia { get; private set; }

    /// <summary>
    /// Devolve o rateio de uma faixa de premiação.
    /// </summary>
    /// <param name="faixa">Quantidade de acertos (11 a 15).</param>
    /// <returns>Rateio da faixa.</returns>
    public Rateio ObterRateio(int faixa)
    {
        return this.rateios.Find(rateio => rateio.Faixa == faixa)
            ?? throw new ArgumentOutOfRangeException(nameof(faixa), $"Não há faixa de premiação com {faixa} acertos.");
    }

    /// <summary>
    /// Define a ordem do sorteio de um concurso gravado sem ela; lança <see cref="ArgumentException"/> se a ordem não
    /// tiver as mesmas dezenas sorteadas.
    /// </summary>
    /// <param name="ordem">As dezenas na ordem em que foram sorteadas.</param>
    public void DefinirOrdemSorteio(IEnumerable<int> ordem)
    {
        List<int> ordemSorteio = ordem.ToList();
        ValidarOrdem(ordemSorteio, this.Dezenas, nameof(ordem));

        this.OrdemSorteio = ordemSorteio;
    }

    /// <summary>
    /// Confere se a ordem do sorteio tem exatamente as dezenas sorteadas.
    /// </summary>
    /// <param name="ordem">Dezenas na ordem do sorteio.</param>
    /// <param name="dezenasOrdenadas">Dezenas sorteadas, em ordem crescente.</param>
    /// <param name="parametro">Nome do parâmetro, para a exceção.</param>
    private static void ValidarOrdem(List<int> ordem, IReadOnlyList<int> dezenasOrdenadas, string parametro)
    {
        if (!ordem.Order().SequenceEqual(dezenasOrdenadas))
        {
            throw new ArgumentException("A ordem do sorteio não corresponde às dezenas sorteadas.", parametro);
        }
    }

    /// <summary>
    /// Confere se há 15 dezenas, sem repetição e dentro do volante.
    /// </summary>
    /// <param name="dezenasOrdenadas">Dezenas em ordem crescente.</param>
    private static void ValidarDezenas(List<int> dezenasOrdenadas)
    {
        if (dezenasOrdenadas.Count != RegrasLotofacil.DezenasSorteadas)
        {
            throw new ArgumentException(
                $"O concurso deve ter 15 dezenas; foram informadas {dezenasOrdenadas.Count}.",
                "dezenas");
        }

        foreach (int dezena in dezenasOrdenadas)
        {
            if (dezena < RegrasLotofacil.DezenaMinima || dezena > RegrasLotofacil.DezenaMaxima)
            {
                throw new ArgumentOutOfRangeException(
                    "dezenas",
                    $"As dezenas vão de 1 a 25; foi informada a dezena {dezena}.");
            }
        }

        // Com a lista ordenada, uma dezena repetida fica ao lado da outra.
        for (int indice = 1; indice < dezenasOrdenadas.Count; indice++)
        {
            if (dezenasOrdenadas[indice] == dezenasOrdenadas[indice - 1])
            {
                throw new ArgumentException(
                    $"A dezena {dezenasOrdenadas[indice]:00} foi informada mais de uma vez.",
                    "dezenas");
            }
        }
    }

    /// <summary>
    /// Confere se o rateio tem exatamente uma entrada para cada faixa de 11 a 15 acertos.
    /// </summary>
    /// <param name="rateiosOrdenados">Rateios em ordem crescente de faixa.</param>
    private static void ValidarFaixas(List<Rateio> rateiosOrdenados)
    {
        IEnumerable<int> faixasEsperadas = Enumerable.Range(
            RegrasLotofacil.MenorFaixaPremiada,
            RegrasLotofacil.MaiorFaixaPremiada - RegrasLotofacil.MenorFaixaPremiada + 1);

        if (!rateiosOrdenados.Select(rateio => rateio.Faixa).SequenceEqual(faixasEsperadas))
        {
            throw new ArgumentException(
                "O rateio deve ter exatamente uma entrada para cada faixa de 11 a 15 acertos.",
                "rateios");
        }
    }

    /// <summary>
    /// Rejeita um valor monetário negativo.
    /// </summary>
    /// <param name="valor">Valor verificado.</param>
    /// <param name="descricao">Descrição do valor usada na mensagem (ex.: "da arrecadação").</param>
    /// <param name="parametro">Nome do parâmetro do construtor.</param>
    private static void ValidarNaoNegativo(decimal valor, string descricao, string parametro)
    {
        if (valor < 0)
        {
            throw new ArgumentOutOfRangeException(parametro, $"O valor {descricao} não pode ser negativo.");
        }
    }
}
