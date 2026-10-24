// <copyright file="FormatacaoExportacaoTestes.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using System.Globalization;
using Lotofacil.Aplicacao.Jogos;
using Lotofacil.Dominio.Concursos;
using Lotofacil.Dominio.Jogos;
using Lotofacil.Web.Components.Jogos;

namespace Lotofacil.Testes.Unidade.Web;

/// <summary>
/// Testes do texto da exportação dos jogos, na cultura pt-BR.
/// </summary>
public class FormatacaoExportacaoTestes
{
    /// <summary>
    /// Usa a cultura da aplicação.
    /// </summary>
    public FormatacaoExportacaoTestes()
    {
        CultureInfo.CurrentCulture = new CultureInfo("pt-BR");
    }

    /// <summary>
    /// Texto do cenário da spec: três jogos, um de 16 dezenas, com custo de R$ 63,00.
    /// </summary>
    [Fact]
    public void TextoDosJogos()
    {
        GrupoJogos grupo = Grupo(
            3813,
            null,
            [1, 2, 4, 5, 7, 10, 11, 13, 14, 18, 20, 21, 22, 24, 25],
            [2, 3, 5, 6, 8, 9, 12, 15, 16, 17, 19, 20, 21, 23, 25],
            [25, 1, 3, 4, 6, 7, 8, 10, 11, 12, 14, 15, 17, 18, 22, 24]);

        string texto = FormatacaoExportacao.Texto(grupo, 3.50m);

        Assert.Equal(
            "Lotofácil · concurso 3813\r\n"
                + "3 jogos · R$ 63,00\r\n"
                + "\r\n"
                + "01) 01 02 04 05 07 10 11 13 14 18 20 21 22 24 25\r\n"
                + "02) 02 03 05 06 08 09 12 15 16 17 19 20 21 23 25\r\n"
                + "03) 01 03 04 06 07 08 10 11 12 14 15 17 18 22 24 25 (16 dezenas)",
            Normalizar(texto));
    }

    /// <summary>
    /// Um jogo só, no singular.
    /// </summary>
    [Fact]
    public void UmJogo()
    {
        GrupoJogos grupo = Grupo(3814, null, [.. Enumerable.Range(1, 15)]);

        string[] linhas = FormatacaoExportacao.Texto(grupo, 3.50m).Split("\r\n");

        Assert.Equal("1 jogo · R$ 3,50", Normalizar(linhas[1]));
        Assert.Equal("01) 01 02 03 04 05 06 07 08 09 10 11 12 13 14 15", linhas[3]);
    }

    /// <summary>
    /// Concurso sorteado com jogo premiado: o texto não traz conferência.
    /// </summary>
    [Fact]
    public void ConcursoSorteadoSemConferencia()
    {
        Concurso sorteado = ConcursoFabrica.Criar(numero: 3808);
        GrupoJogos grupo = Grupo(3808, sorteado, ConcursoFabrica.DezenasPadrao);

        string texto = FormatacaoExportacao.Texto(grupo, 3.50m);

        Assert.Equal(4, texto.Split("\r\n").Length);
        Assert.DoesNotContain("acerto", texto);
        Assert.DoesNotContain("Prêmio", texto);
        Assert.False(texto.EndsWith("\r\n", StringComparison.Ordinal));
    }

    /// <summary>
    /// Nome do arquivo e endereço de download que devolve o mesmo texto.
    /// </summary>
    [Fact]
    public void NomeEEnderecoDoArquivo()
    {
        const string texto = "Lotofácil · concurso 3813\r\n1 jogo · R$ 3,50";

        string endereco = FormatacaoExportacao.EnderecoDownload(texto);

        Assert.Equal("concurso-3813.txt", FormatacaoExportacao.NomeArquivo(3813));
        Assert.StartsWith("data:text/plain;charset=utf-8,", endereco, StringComparison.Ordinal);
        Assert.Equal(texto, Uri.UnescapeDataString(endereco["data:text/plain;charset=utf-8,".Length..]));
    }

    /// <summary>
    /// Cria o grupo de um concurso com os jogos na ordem dada.
    /// </summary>
    /// <param name="numero">Número do concurso.</param>
    /// <param name="concurso">Concurso sorteado, ou <c>null</c> enquanto aguarda.</param>
    /// <param name="jogos">Dezenas de cada jogo.</param>
    /// <returns>O grupo.</returns>
    private static GrupoJogos Grupo(int numero, Concurso? concurso, params int[][] jogos)
    {
        List<JogoConferido> conferidos = jogos
            .Select((dezenas, indice) =>
            {
                var jogo = new Jogo(numero, dezenas, DateTime.Now);
                return new JogoConferido(jogo, indice + 1, concurso is null ? null : ConferenciaJogo.Conferir(jogo, concurso));
            })
            .ToList();

        return new GrupoJogos(numero, concurso, conferidos);
    }

    /// <summary>
    /// Troca o espaço não separável do formato de moeda por espaço comum.
    /// </summary>
    /// <param name="texto">Texto formatado.</param>
    /// <returns>Texto com espaços comuns.</returns>
    private static string Normalizar(string texto)
    {
        return texto.Replace(' ', ' ');
    }
}
