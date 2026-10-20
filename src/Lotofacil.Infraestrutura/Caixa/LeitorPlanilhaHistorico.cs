// <copyright file="LeitorPlanilhaHistorico.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Xml;
using Lotofacil.Dominio;
using Lotofacil.Dominio.Concursos;

namespace Lotofacil.Infraestrutura.Caixa;

/// <summary>
/// Lê o XLSX do histórico da Caixa e converte cada linha em concurso, validando o arquivo inteiro.
/// </summary>
/// <remarks>
/// As células de dados do arquivo não trazem referência de coluna, por isso as colunas são lidas por posição.
/// </remarks>
internal static class LeitorPlanilhaHistorico
{
    /// <summary>
    /// Mensagem para conteúdo que não é uma planilha XLSX legível.
    /// </summary>
    public const string MensagemArquivoInvalido = "O arquivo recebido da Caixa não é uma planilha XLSX válida.";

    /// <summary>
    /// Mensagem para planilha sem nenhuma linha de concurso.
    /// </summary>
    public const string MensagemSemConcursos = "A planilha da Caixa não tem nenhum concurso.";

    /// <summary>
    /// Posição (a partir de zero) da coluna do número do concurso.
    /// </summary>
    private const int ColunaConcurso = 0;

    /// <summary>
    /// Posição da coluna da data do sorteio.
    /// </summary>
    private const int ColunaData = 1;

    /// <summary>
    /// Posição da coluna da primeira dezena (Bola1); as 15 dezenas são consecutivas.
    /// </summary>
    private const int ColunaPrimeiraDezena = 2;

    /// <summary>
    /// Posição da coluna do acumulado da faixa de 15 acertos.
    /// </summary>
    private const int ColunaAcumulado15 = 28;

    /// <summary>
    /// Posição da coluna da arrecadação total.
    /// </summary>
    private const int ColunaArrecadacao = 29;

    /// <summary>
    /// Posição da coluna da estimativa do próximo concurso.
    /// </summary>
    private const int ColunaEstimativa = 30;

    /// <summary>
    /// Posição da coluna do acumulado da Lotofácil da Independência.
    /// </summary>
    private const int ColunaAcumuladoIndependencia = 31;

    /// <summary>
    /// Caminho da planilha dentro do pacote XLSX.
    /// </summary>
    private const string CaminhoPlanilha = "xl/worksheets/sheet1.xml";

    /// <summary>
    /// Caminho dos textos compartilhados (usados pelo cabeçalho) dentro do pacote XLSX.
    /// </summary>
    private const string CaminhoTextosCompartilhados = "xl/sharedStrings.xml";

    /// <summary>
    /// Títulos esperados no cabeçalho, por posição; <c>null</c> nas colunas ignoradas, que não são conferidas.
    /// </summary>
    private static readonly string?[] TitulosEsperados =
    [
        "Concurso", "Data Sorteio",
        "Bola1", "Bola2", "Bola3", "Bola4", "Bola5", "Bola6", "Bola7", "Bola8",
        "Bola9", "Bola10", "Bola11", "Bola12", "Bola13", "Bola14", "Bola15",
        "Ganhadores 15 acertos", null, "Rateio 15 acertos",
        "Ganhadores 14 acertos", "Rateio 14 acertos", "Ganhadores 13 acertos", "Rateio 13 acertos",
        "Ganhadores 12 acertos", "Rateio 12 acertos", "Ganhadores 11 acertos", "Rateio 11 acertos",
        "Acumulado 15 acertos", "Arrecadacao Total", "Estimativa Prêmio",
        "Acumulado sorteio especial Lotofácil da Independência", null,
    ];

    /// <summary>
    /// Títulos das colunas ignoradas, usados apenas na mensagem quando o cabeçalho tem colunas a menos.
    /// </summary>
    private static readonly Dictionary<int, string> TitulosIgnorados = new()
    {
        [18] = "Cidade / UF",
        [32] = "Observação",
    };

    /// <summary>
    /// Posições das colunas de ganhadores e de prêmio de cada faixa.
    /// </summary>
    private static readonly (int Faixa, int ColunaGanhadores, int ColunaPremio)[] ColunasFaixas =
    [
        (15, 17, 19),
        (14, 20, 21),
        (13, 22, 23),
        (12, 24, 25),
        (11, 26, 27),
    ];

    /// <summary>
    /// Cultura dos valores em reais do arquivo (<c>1.388,95</c>).
    /// </summary>
    private static readonly CultureInfo CulturaReais = CultureInfo.GetCultureInfo("pt-BR");

    /// <summary>
    /// Lê todos os concursos do arquivo; lança <see cref="FonteResultadosException"/> no primeiro problema.
    /// </summary>
    /// <param name="conteudo">Conteúdo do arquivo XLSX, com suporte a busca.</param>
    /// <returns>Concursos na ordem das linhas do arquivo.</returns>
    public static IReadOnlyList<Concurso> Ler(Stream conteudo)
    {
        List<List<string>> linhas = LerLinhas(conteudo);
        if (linhas.Count == 0)
        {
            throw new FonteResultadosException(MensagemArquivoInvalido);
        }

        ConferirCabecalho(linhas[0]);

        List<Concurso> concursos = [];
        HashSet<int> numerosLidos = [];
        for (int indice = 1; indice < linhas.Count; indice++)
        {
            List<string> celulas = linhas[indice];
            if (celulas.All(string.IsNullOrWhiteSpace))
            {
                continue;
            }

            // A linha da planilha começa em 1, no cabeçalho.
            int linha = indice + 1;
            Concurso concurso = ConverterLinha(celulas, linha);
            if (!numerosLidos.Add(concurso.Numero))
            {
                throw new FonteResultadosException(
                    $"Linha {linha}: o concurso {concurso.Numero} aparece mais de uma vez na planilha.");
            }

            concursos.Add(concurso);
        }

        if (concursos.Count == 0)
        {
            throw new FonteResultadosException(MensagemSemConcursos);
        }

        return concursos;
    }

    /// <summary>
    /// Abre o pacote e lê o texto de cada célula da planilha, linha a linha, por posição.
    /// </summary>
    /// <param name="conteudo">Conteúdo do arquivo XLSX.</param>
    /// <returns>Linhas da planilha, cada uma com os textos das células.</returns>
    private static List<List<string>> LerLinhas(Stream conteudo)
    {
        try
        {
            using var pacote = new ZipArchive(conteudo, ZipArchiveMode.Read, leaveOpen: true);
            ZipArchiveEntry planilha = pacote.GetEntry(CaminhoPlanilha)
                ?? throw new FonteResultadosException(MensagemArquivoInvalido);
            List<string> textosCompartilhados = LerTextosCompartilhados(pacote.GetEntry(CaminhoTextosCompartilhados));

            using Stream fluxo = planilha.Open();
            using XmlReader leitor = XmlReader.Create(fluxo);
            List<List<string>> linhas = [];
            List<string>? linhaAtual = null;
            while (leitor.Read())
            {
                if (leitor.NodeType != XmlNodeType.Element)
                {
                    continue;
                }

                if (leitor.LocalName == "row")
                {
                    linhaAtual = [];
                    linhas.Add(linhaAtual);
                }
                else if (leitor.LocalName == "c" && linhaAtual is not null)
                {
                    linhaAtual.Add(LerCelula(leitor, textosCompartilhados));
                }
            }

            return linhas;
        }
        catch (Exception erro) when (erro is InvalidDataException or XmlException)
        {
            throw new FonteResultadosException(MensagemArquivoInvalido, erro);
        }
    }

    /// <summary>
    /// Lê a lista de textos compartilhados, referenciados pelas células do tipo <c>s</c>.
    /// </summary>
    /// <param name="entrada">Entrada do pacote, ou <c>null</c> se o arquivo não tiver textos compartilhados.</param>
    /// <returns>Textos na ordem do arquivo.</returns>
    private static List<string> LerTextosCompartilhados(ZipArchiveEntry? entrada)
    {
        List<string> textos = [];
        if (entrada is null)
        {
            return textos;
        }

        using Stream fluxo = entrada.Open();
        using XmlReader leitor = XmlReader.Create(fluxo);
        while (leitor.Read())
        {
            if (leitor.NodeType == XmlNodeType.Element && leitor.LocalName == "si")
            {
                textos.Add(LerTexto(leitor));
            }
        }

        return textos;
    }

    /// <summary>
    /// Lê o texto de uma célula, buscando nos textos compartilhados quando ela é do tipo <c>s</c>.
    /// </summary>
    /// <param name="leitor">Leitor posicionado no elemento da célula.</param>
    /// <param name="textosCompartilhados">Textos compartilhados do arquivo.</param>
    /// <returns>Texto da célula; vazio se ela não tiver valor.</returns>
    private static string LerCelula(XmlReader leitor, List<string> textosCompartilhados)
    {
        string? tipo = leitor.GetAttribute("t");
        string valor = LerTexto(leitor);
        if (tipo != "s")
        {
            return valor;
        }

        bool indiceValido = int.TryParse(valor, NumberStyles.None, CultureInfo.InvariantCulture, out int indice)
            && indice < textosCompartilhados.Count;
        if (!indiceValido)
        {
            throw new FonteResultadosException(MensagemArquivoInvalido);
        }

        return textosCompartilhados[indice];
    }

    /// <summary>
    /// Junta o conteúdo dos elementos <c>v</c> e <c>t</c> dentro do elemento atual.
    /// </summary>
    /// <param name="leitor">Leitor posicionado no elemento; ao final, fica no fim desse elemento.</param>
    /// <returns>Texto encontrado.</returns>
    private static string LerTexto(XmlReader leitor)
    {
        var texto = new StringBuilder();
        using XmlReader elemento = leitor.ReadSubtree();
        elemento.Read();
        while (!elemento.EOF)
        {
            bool temTexto = elemento.NodeType == XmlNodeType.Element
                && (elemento.LocalName == "v" || elemento.LocalName == "t");
            if (temTexto)
            {
                // Já avança para o nó seguinte ao fim do elemento.
                texto.Append(elemento.ReadElementContentAsString());
            }
            else
            {
                elemento.Read();
            }
        }

        return texto.ToString();
    }

    /// <summary>
    /// Confere se o cabeçalho tem as 33 colunas e os títulos esperados nas colunas usadas.
    /// </summary>
    /// <param name="cabecalho">Textos da primeira linha.</param>
    private static void ConferirCabecalho(List<string> cabecalho)
    {
        for (int indice = 0; indice < TitulosEsperados.Length; indice++)
        {
            string? esperado = TitulosEsperados[indice];
            bool existe = indice < cabecalho.Count;
            string encontrado = existe ? cabecalho[indice].Trim() : string.Empty;

            if (!existe || (esperado is not null && encontrado != esperado))
            {
                string titulo = esperado ?? TitulosIgnorados[indice];
                throw new FonteResultadosException(
                    $"O layout da planilha da Caixa mudou: a coluna {indice + 1} deveria ser “{titulo}” e é “{encontrado}”.");
            }
        }
    }

    /// <summary>
    /// Converte uma linha da planilha em concurso.
    /// </summary>
    /// <param name="celulas">Textos das células da linha.</param>
    /// <param name="linha">Número da linha na planilha, usado nas mensagens.</param>
    /// <returns>Concurso da linha.</returns>
    private static Concurso ConverterLinha(List<string> celulas, int linha)
    {
        int numero = LerInteiro(celulas, ColunaConcurso, $"Linha {linha}:");
        string prefixo = $"Linha {linha} (concurso {numero}):";

        DateOnly data = LerData(celulas, ColunaData, prefixo);
        List<int> dezenas = Enumerable.Range(ColunaPrimeiraDezena, RegrasLotofacil.DezenasSorteadas)
            .Select(coluna => LerInteiro(celulas, coluna, prefixo))
            .ToList();
        List<(int Faixa, int Ganhadores, decimal Premio)> faixas = ColunasFaixas
            .Select(colunas => (
                colunas.Faixa,
                LerInteiro(celulas, colunas.ColunaGanhadores, prefixo),
                LerReais(celulas, colunas.ColunaPremio, prefixo)))
            .ToList();
        decimal acumulado15 = LerReais(celulas, ColunaAcumulado15, prefixo);
        decimal arrecadacao = LerReais(celulas, ColunaArrecadacao, prefixo);
        decimal estimativa = LerReais(celulas, ColunaEstimativa, prefixo);
        decimal acumuladoIndependencia = LerReais(celulas, ColunaAcumuladoIndependencia, prefixo);

        try
        {
            List<Rateio> rateios = faixas
                .Select(faixa => new Rateio(faixa.Faixa, faixa.Ganhadores, faixa.Premio))
                .ToList();

            return new Concurso(
                numero,
                data,
                dezenas,
                null,
                rateios,
                arrecadacao,
                acumulado15,
                estimativa,
                acumuladoIndependencia);
        }
        catch (ArgumentException erro)
        {
            throw new FonteResultadosException($"{prefixo} {MensagemSemParametro(erro)}", erro);
        }
    }

    /// <summary>
    /// Lê um número inteiro sem sinal, com ou sem zeros à esquerda.
    /// </summary>
    /// <param name="celulas">Textos das células da linha.</param>
    /// <param name="coluna">Posição da coluna.</param>
    /// <param name="prefixo">Início da mensagem de erro (linha e concurso).</param>
    /// <returns>Valor lido.</returns>
    private static int LerInteiro(List<string> celulas, int coluna, string prefixo)
    {
        string texto = Celula(celulas, coluna);
        if (!int.TryParse(texto, NumberStyles.None, CultureInfo.InvariantCulture, out int valor))
        {
            throw ValorInvalido(prefixo, coluna, texto, "um número inteiro");
        }

        return valor;
    }

    /// <summary>
    /// Lê uma data no formato <c>dd/MM/aaaa</c>.
    /// </summary>
    /// <param name="celulas">Textos das células da linha.</param>
    /// <param name="coluna">Posição da coluna.</param>
    /// <param name="prefixo">Início da mensagem de erro (linha e concurso).</param>
    /// <returns>Data lida.</returns>
    private static DateOnly LerData(List<string> celulas, int coluna, string prefixo)
    {
        string texto = Celula(celulas, coluna);
        if (!DateOnly.TryParseExact(texto, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly data))
        {
            throw ValorInvalido(prefixo, coluna, texto, "uma data no formato dd/mm/aaaa");
        }

        return data;
    }

    /// <summary>
    /// Lê um valor em reais no formato da Caixa (<c>R$1.388,95</c>), sem perder os centavos.
    /// </summary>
    /// <param name="celulas">Textos das células da linha.</param>
    /// <param name="coluna">Posição da coluna.</param>
    /// <param name="prefixo">Início da mensagem de erro (linha e concurso).</param>
    /// <returns>Valor lido.</returns>
    private static decimal LerReais(List<string> celulas, int coluna, string prefixo)
    {
        string texto = Celula(celulas, coluna);
        string semMoeda = texto.StartsWith("R$", StringComparison.Ordinal) ? texto[2..].Trim() : texto;
        const NumberStyles estilo = NumberStyles.AllowThousands | NumberStyles.AllowDecimalPoint;
        if (!decimal.TryParse(semMoeda, estilo, CulturaReais, out decimal valor))
        {
            throw ValorInvalido(prefixo, coluna, texto, "um valor em reais");
        }

        return valor;
    }

    /// <summary>
    /// Devolve o texto de uma célula, sem espaços nas pontas; vazio se a linha não tiver essa coluna.
    /// </summary>
    /// <param name="celulas">Textos das células da linha.</param>
    /// <param name="coluna">Posição da coluna.</param>
    /// <returns>Texto da célula.</returns>
    private static string Celula(List<string> celulas, int coluna)
    {
        return coluna < celulas.Count ? celulas[coluna].Trim() : string.Empty;
    }

    /// <summary>
    /// Cria o erro de valor que não pode ser lido.
    /// </summary>
    /// <param name="prefixo">Início da mensagem (linha e concurso).</param>
    /// <param name="coluna">Posição da coluna.</param>
    /// <param name="texto">Texto encontrado na célula.</param>
    /// <param name="tipoEsperado">Descrição do tipo esperado (ex.: "um valor em reais").</param>
    /// <returns>Exceção com a mensagem para o usuário.</returns>
    private static FonteResultadosException ValorInvalido(string prefixo, int coluna, string texto, string tipoEsperado)
    {
        return new FonteResultadosException(
            $"{prefixo} a coluna “{TitulosEsperados[coluna]}” tem o valor “{texto}”, que não é {tipoEsperado}.");
    }

    /// <summary>
    /// Devolve a mensagem da validação do domínio sem o sufixo <c>(Parameter '...')</c> que o .NET acrescenta.
    /// </summary>
    /// <param name="erro">Erro de validação do domínio.</param>
    /// <returns>Mensagem para o usuário.</returns>
    private static string MensagemSemParametro(ArgumentException erro)
    {
        string sufixo = $" (Parameter '{erro.ParamName}')";
        bool temSufixo = erro.ParamName is not null && erro.Message.EndsWith(sufixo, StringComparison.Ordinal);

        return temSufixo ? erro.Message[..^sufixo.Length] : erro.Message;
    }
}
