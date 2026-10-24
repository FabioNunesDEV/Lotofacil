// <copyright file="ConfiguracaoApostas.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

namespace Lotofacil.Dominio.Configuracoes;

/// <summary>
/// Valores das apostas definidos pelo usuário; a base guarda uma única linha.
/// </summary>
public sealed class ConfiguracaoApostas
{
    /// <summary>
    /// Identificador da linha única.
    /// </summary>
    public const int IdUnico = 1;

    /// <summary>
    /// Menor preço aceito para a aposta mínima.
    /// </summary>
    public const decimal PrecoMinimo = 0.01m;

    /// <summary>
    /// Maior preço aceito para a aposta mínima.
    /// </summary>
    public const decimal PrecoMaximo = 100.00m;

    /// <summary>
    /// Cria a configuração; lança <see cref="ArgumentOutOfRangeException"/> se o preço for inválido.
    /// </summary>
    /// <param name="precoApostaMinima">Preço da aposta de 15 dezenas.</param>
    public ConfiguracaoApostas(decimal precoApostaMinima)
    {
        if (!PrecoValido(precoApostaMinima))
        {
            throw new ArgumentOutOfRangeException(
                nameof(precoApostaMinima),
                "O preço deve estar entre R$ 0,01 e R$ 100,00, com até dois decimais.");
        }

        this.Id = IdUnico;
        this.PrecoApostaMinima = precoApostaMinima;
    }

    /// <summary>
    /// Usado apenas pelo EF Core para recriar a configuração a partir do banco.
    /// </summary>
    private ConfiguracaoApostas()
    {
    }

    /// <summary>
    /// Identificador da linha na base; sempre <see cref="IdUnico"/>.
    /// </summary>
    public int Id { get; private set; }

    /// <summary>
    /// Preço da aposta de 15 dezenas.
    /// </summary>
    public decimal PrecoApostaMinima { get; private set; }

    /// <summary>
    /// Indica se o preço está entre <see cref="PrecoMinimo"/> e <see cref="PrecoMaximo"/>, com até dois decimais.
    /// </summary>
    /// <param name="preco">Preço a verificar.</param>
    /// <returns><c>true</c> se o preço for aceito.</returns>
    public static bool PrecoValido(decimal preco)
    {
        return preco >= PrecoMinimo && preco <= PrecoMaximo && decimal.Round(preco, 2) == preco;
    }
}
