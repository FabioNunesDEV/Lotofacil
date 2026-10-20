// <copyright file="PlanilhaCaixa.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using System.Globalization;
using System.IO.Compression;
using System.Security;
using System.Text;

namespace Lotofacil.Testes.Integracao.Caixa;

/// <summary>
/// Monta arquivos XLSX no mesmo formato do histórico da Caixa e abre a amostra recortada do arquivo real.
/// </summary>
internal static class PlanilhaCaixa
{
    /// <summary>
    /// Títulos do cabeçalho do arquivo real, na ordem das colunas.
    /// </summary>
    public static readonly string[] CabecalhoPadrao =
    [
        "Concurso", "Data Sorteio",
        "Bola1", "Bola2", "Bola3", "Bola4", "Bola5", "Bola6", "Bola7", "Bola8",
        "Bola9", "Bola10", "Bola11", "Bola12", "Bola13", "Bola14", "Bola15",
        "Ganhadores 15 acertos", "Cidade / UF", "Rateio 15 acertos",
        "Ganhadores 14 acertos", "Rateio 14 acertos", "Ganhadores 13 acertos", "Rateio 13 acertos",
        "Ganhadores 12 acertos", "Rateio 12 acertos", "Ganhadores 11 acertos", "Rateio 11 acertos",
        "Acumulado 15 acertos", "Arrecadacao Total", "Estimativa Prêmio",
        "Acumulado sorteio especial Lotofácil da Independência", "Observação",
    ];

    /// <summary>
    /// Caminho da amostra do arquivo real: cabeçalho e concursos 1, 2 e 3808.
    /// </summary>
    private static readonly string CaminhoAmostra = Path.Combine(AppContext.BaseDirectory, "Arquivos", "lotofacil-amostra.xlsx");

    /// <summary>
    /// Cria os valores de uma linha válida, como aparecem no arquivo da Caixa.
    /// </summary>
    /// <param name="numero">Número do concurso.</param>
    /// <returns>As 33 células da linha.</returns>
    public static string[] CriarLinha(int numero)
    {
        return
        [
            numero.ToString(CultureInfo.InvariantCulture), "17/10/2026",
            "01", "02", "04", "05", "07", "10", "11", "13", "14", "18", "20", "21", "22", "24", "25",
            "1", "SAO PAULO/SP", "R$1.712.408,63",
            "287", "R$1.502,34", "9874", "R$35,00",
            "121045", "R$14,00", "653211", "R$7,00",
            "R$0,00", "R$24.873.112,57", "R$1.800.000,00",
            "R$120.500.300,25", string.Empty,
        ];
    }

    /// <summary>
    /// Monta um XLSX com o cabeçalho em <c>sharedStrings</c> e as linhas de dados com valores inline e sem
    /// referência de coluna, como no arquivo da Caixa.
    /// </summary>
    /// <param name="linhas">Linhas de dados; cada uma é a lista de textos das células.</param>
    /// <param name="cabecalho">Títulos do cabeçalho; o padrão quando <c>null</c>.</param>
    /// <returns>Conteúdo do arquivo XLSX.</returns>
    public static byte[] Gerar(IEnumerable<string[]> linhas, string[]? cabecalho = null)
    {
        string[] titulos = cabecalho ?? CabecalhoPadrao;

        using var conteudo = new MemoryStream();
        using (var pacote = new ZipArchive(conteudo, ZipArchiveMode.Create, leaveOpen: true))
        {
            GravarEntrada(pacote, "xl/sharedStrings.xml", GerarTextosCompartilhados(titulos));
            GravarEntrada(pacote, "xl/worksheets/sheet1.xml", GerarPlanilha(titulos.Length, linhas));
        }

        return conteudo.ToArray();
    }

    /// <summary>
    /// Abre a amostra recortada do arquivo real da Caixa.
    /// </summary>
    /// <returns>Conteúdo da amostra.</returns>
    public static MemoryStream AbrirAmostra()
    {
        return new MemoryStream(File.ReadAllBytes(CaminhoAmostra));
    }

    /// <summary>
    /// Gera o <c>sharedStrings.xml</c> com os títulos do cabeçalho.
    /// </summary>
    /// <param name="titulos">Títulos do cabeçalho.</param>
    /// <returns>XML gerado.</returns>
    private static string GerarTextosCompartilhados(string[] titulos)
    {
        var xml = new StringBuilder();
        xml.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
        xml.Append("<sst xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\">");
        foreach (string titulo in titulos)
        {
            xml.Append("<si><t>").Append(SecurityElement.Escape(titulo)).Append("</t></si>");
        }

        xml.Append("</sst>");
        return xml.ToString();
    }

    /// <summary>
    /// Gera o <c>sheet1.xml</c>: cabeçalho com referência de coluna e dados sem referência.
    /// </summary>
    /// <param name="colunasCabecalho">Quantidade de títulos do cabeçalho.</param>
    /// <param name="linhas">Linhas de dados.</param>
    /// <returns>XML gerado.</returns>
    private static string GerarPlanilha(int colunasCabecalho, IEnumerable<string[]> linhas)
    {
        var xml = new StringBuilder();
        xml.Append("<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\"?>");
        xml.Append("<x:worksheet xmlns:x=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\">");
        xml.Append("<x:dimension ref=\"A1:AG1\" /><x:sheetData>");

        xml.Append("<x:row r=\"1\">");
        for (int indice = 0; indice < colunasCabecalho; indice++)
        {
            xml.Append("<x:c r=\"").Append(LetraColuna(indice)).Append("1\" t=\"s\"><x:v>")
                .Append(indice).Append("</x:v></x:c>");
        }

        xml.Append("</x:row>");

        foreach (string[] linha in linhas)
        {
            xml.Append("<x:row>");
            foreach (string valor in linha)
            {
                string tipo = valor.Length > 0 && valor.All(char.IsAsciiDigit) ? "n" : "str";
                xml.Append("<x:c t=\"").Append(tipo).Append("\"><x:v>")
                    .Append(SecurityElement.Escape(valor)).Append("</x:v></x:c>");
            }

            xml.Append("</x:row>");
        }

        xml.Append("</x:sheetData></x:worksheet>");
        return xml.ToString();
    }

    /// <summary>
    /// Converte o índice da coluna (a partir de zero) na letra do Excel: 0 → A, 26 → AA.
    /// </summary>
    /// <param name="indice">Índice da coluna.</param>
    /// <returns>Letra da coluna.</returns>
    private static string LetraColuna(int indice)
    {
        string letras = string.Empty;
        for (int numero = indice + 1; numero > 0; numero = (numero - 1) / 26)
        {
            letras = (char)('A' + ((numero - 1) % 26)) + letras;
        }

        return letras;
    }

    /// <summary>
    /// Grava um texto como entrada do pacote.
    /// </summary>
    /// <param name="pacote">Pacote ZIP em criação.</param>
    /// <param name="caminho">Caminho da entrada.</param>
    /// <param name="texto">Conteúdo da entrada.</param>
    private static void GravarEntrada(ZipArchive pacote, string caminho, string texto)
    {
        using Stream destino = pacote.CreateEntry(caminho).Open();
        using var escritor = new StreamWriter(destino, new UTF8Encoding(false));
        escritor.Write(texto);
    }
}
