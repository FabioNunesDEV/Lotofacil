// <copyright file="FormatacaoExportacao.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Aplicacao.Jogos;
using Lotofacil.Dominio;

namespace Lotofacil.Web.Components.Jogos;

/// <summary>
/// Texto da exportação dos jogos de um concurso, nome do arquivo e endereço de download.
/// </summary>
public static class FormatacaoExportacao
{
    /// <summary>
    /// Quebra de linha do texto, a do Windows, para o arquivo abrir certo no Bloco de Notas.
    /// </summary>
    private const string QuebraDeLinha = "\r\n";

    /// <summary>
    /// Monta o texto com o cabeçalho do concurso e um jogo por linha, sem conferência.
    /// </summary>
    /// <param name="grupo">Jogos do concurso.</param>
    /// <param name="precoApostaMinima">Preço da aposta de 15 dezenas.</param>
    /// <returns>Texto da exportação.</returns>
    public static string Texto(GrupoJogos grupo, decimal precoApostaMinima)
    {
        decimal custo = grupo.QuantidadeApostas * precoApostaMinima;
        List<string> linhas =
        [
            $"Lotofácil · concurso {grupo.NumeroConcurso}",
            $"{FormatacaoJogo.QuantidadeJogos(grupo.Jogos.Count)} · {FormatacaoJogo.Reais(custo)}",
            string.Empty,
        ];
        linhas.AddRange(grupo.Jogos.Select(LinhaJogo));

        return string.Join(QuebraDeLinha, linhas);
    }

    /// <summary>
    /// Nome do arquivo baixado.
    /// </summary>
    /// <param name="numeroConcurso">Número do concurso.</param>
    /// <returns>Nome como "concurso-3813.txt".</returns>
    public static string NomeArquivo(int numeroConcurso)
    {
        return $"concurso-{numeroConcurso}.txt";
    }

    /// <summary>
    /// Endereço que entrega o texto como arquivo, sem passar pelo servidor.
    /// </summary>
    /// <param name="texto">Texto da exportação.</param>
    /// <returns>Data URL de texto em UTF-8.</returns>
    public static string EnderecoDownload(string texto)
    {
        return $"data:text/plain;charset=utf-8,{Uri.EscapeDataString(texto)}";
    }

    /// <summary>
    /// Linha de um jogo: número, dezenas e a quantidade quando for aposta múltipla.
    /// </summary>
    /// <param name="jogo">Jogo do concurso.</param>
    /// <returns>Texto como "01) 01 02 04 ..." ou "03) ... (16 dezenas)".</returns>
    private static string LinhaJogo(JogoConferido jogo)
    {
        IReadOnlyList<int> dezenas = jogo.Jogo.Dezenas;
        string linha = $"{FormatacaoJogo.NumeroJogo(jogo.Numero)}) {string.Join(' ', dezenas.Select(dezena => dezena.ToString("00")))}";

        return dezenas.Count > RegrasLotofacil.MinimoDezenasPorJogo
            ? $"{linha} ({FormatacaoJogo.QuantidadeDezenas(dezenas.Count)})"
            : linha;
    }
}
