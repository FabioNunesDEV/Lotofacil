// <copyright file="LeitorPlanilhaHistoricoTestes.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using System.IO.Compression;
using System.Text;
using Lotofacil.Dominio.Concursos;
using Lotofacil.Infraestrutura.Caixa;

namespace Lotofacil.Testes.Integracao.Caixa;

/// <summary>
/// Testes da leitura do XLSX do histórico da Caixa, com a amostra do arquivo real e arquivos gerados.
/// </summary>
public class LeitorPlanilhaHistoricoTestes
{
    /// <summary>
    /// A amostra do arquivo real é lida com os valores exatos de cada concurso.
    /// </summary>
    [Fact]
    public void LeAmostraDoArquivoReal()
    {
        using MemoryStream amostra = PlanilhaCaixa.AbrirAmostra();

        IReadOnlyList<Concurso> concursos = LeitorPlanilhaHistorico.Ler(amostra);

        Assert.Equal([1, 2, 3808], concursos.Select(concurso => concurso.Numero));

        Concurso concurso2 = concursos[1];
        Assert.Equal(new DateOnly(2003, 10, 6), concurso2.Data);
        Assert.Equal([1, 4, 5, 6, 7, 9, 11, 12, 13, 15, 16, 19, 20, 23, 24], concurso2.Dezenas);
        Assert.Null(concurso2.OrdemSorteio);
        Assert.Equal(
            [(11, 478_188, 2m), (12, 81_252, 4m), (13, 6_232, 10m), (14, 184, 1_388.95m), (15, 1, 596_323.70m)],
            concurso2.Rateios.Select(rateio => (rateio.Faixa, rateio.Ganhadores, rateio.ValorPremio)));
        Assert.Equal(0m, concurso2.Acumulado15);
        Assert.Equal(0m, concurso2.Arrecadacao);
        Assert.Equal(0m, concurso2.EstimativaProximo);
        Assert.Equal(0m, concurso2.AcumuladoIndependencia);

        Concurso concurso3808 = concursos[2];
        Assert.Equal(new DateOnly(2026, 10, 19), concurso3808.Data);
        Assert.Equal(1_456_047.72m, concurso3808.ObterRateio(15).ValorPremio);
        Assert.Equal(17_703_532.00m, concurso3808.Arrecadacao);
        Assert.Equal(2_000_000.00m, concurso3808.EstimativaProximo);
        Assert.Equal(12_676_875.24m, concurso3808.AcumuladoIndependencia);
    }

    /// <summary>
    /// Uma linha gerada no formato da Caixa vira o concurso com todos os valores, sem as colunas ignoradas.
    /// </summary>
    [Fact]
    public void LeLinhaCompleta()
    {
        IReadOnlyList<Concurso> concursos = Ler([PlanilhaCaixa.CriarLinha(3807)]);

        Concurso concurso = Assert.Single(concursos);
        Assert.Equal(3807, concurso.Numero);
        Assert.Equal(new DateOnly(2026, 10, 17), concurso.Data);
        Assert.Equal([1, 2, 4, 5, 7, 10, 11, 13, 14, 18, 20, 21, 22, 24, 25], concurso.Dezenas);
        Assert.Null(concurso.OrdemSorteio);
        Assert.Equal(
            [(11, 653_211, 7m), (12, 121_045, 14m), (13, 9_874, 35m), (14, 287, 1_502.34m), (15, 1, 1_712_408.63m)],
            concurso.Rateios.Select(rateio => (rateio.Faixa, rateio.Ganhadores, rateio.ValorPremio)));
        Assert.Equal(0m, concurso.Acumulado15);
        Assert.Equal(24_873_112.57m, concurso.Arrecadacao);
        Assert.Equal(1_800_000m, concurso.EstimativaProximo);
        Assert.Equal(120_500_300.25m, concurso.AcumuladoIndependencia);
    }

    /// <summary>
    /// Números com ou sem zeros à esquerda são aceitos.
    /// </summary>
    [Fact]
    public void AceitaNumerosSemZeroAEsquerda()
    {
        string[] linha = PlanilhaCaixa.CriarLinha(1);
        linha[2] = "1";
        linha[3] = "002";

        Concurso concurso = Assert.Single(Ler([linha]));

        Assert.Equal(1, concurso.Dezenas[0]);
        Assert.Equal(2, concurso.Dezenas[1]);
    }

    /// <summary>
    /// Linhas sem nenhuma célula preenchida são ignoradas.
    /// </summary>
    [Fact]
    public void IgnoraLinhasVazias()
    {
        string[] vazia = new string[PlanilhaCaixa.CabecalhoPadrao.Length];
        Array.Fill(vazia, string.Empty);

        IReadOnlyList<Concurso> concursos = Ler([PlanilhaCaixa.CriarLinha(1), vazia, PlanilhaCaixa.CriarLinha(2), []]);

        Assert.Equal([1, 2], concursos.Select(concurso => concurso.Numero));
    }

    /// <summary>
    /// Títulos diferentes nas colunas ignoradas (Cidade / UF e Observação) não impedem a leitura.
    /// </summary>
    [Fact]
    public void NaoConfereTitulosDasColunasIgnoradas()
    {
        string[] cabecalho = [.. PlanilhaCaixa.CabecalhoPadrao];
        cabecalho[18] = "Cidade";
        cabecalho[32] = "Obs";

        IReadOnlyList<Concurso> concursos = Ler([PlanilhaCaixa.CriarLinha(1)], cabecalho);

        Assert.Single(concursos);
    }

    /// <summary>
    /// Uma linha que não atende às regras do concurso interrompe a leitura, com a mensagem do domínio sem o
    /// nome do parâmetro.
    /// </summary>
    [Fact]
    public void DezenaRepetidaInformaLinhaEConcurso()
    {
        List<string[]> linhas = Enumerable.Range(1, 14).Select(PlanilhaCaixa.CriarLinha).ToList();
        linhas[13][3] = "01";

        var erro = Assert.Throws<FonteResultadosException>(() => Ler(linhas));

        Assert.Equal("Linha 15 (concurso 14): A dezena 01 foi informada mais de uma vez.", erro.Message);
    }

    /// <summary>
    /// Valor em reais ilegível informa a linha, o concurso, a coluna e o valor.
    /// </summary>
    [Fact]
    public void ValorEmReaisIlegivel()
    {
        string[] linha = PlanilhaCaixa.CriarLinha(1);
        linha[21] = "abc";

        var erro = Assert.Throws<FonteResultadosException>(() => Ler([linha]));

        Assert.Equal(
            "Linha 2 (concurso 1): a coluna “Rateio 14 acertos” tem o valor “abc”, que não é um valor em reais.",
            erro.Message);
    }

    /// <summary>
    /// Data fora do formato dd/MM/aaaa é rejeitada.
    /// </summary>
    [Fact]
    public void DataIlegivel()
    {
        string[] linha = PlanilhaCaixa.CriarLinha(1);
        linha[1] = "2026-10-17";

        var erro = Assert.Throws<FonteResultadosException>(() => Ler([linha]));

        Assert.Equal(
            "Linha 2 (concurso 1): a coluna “Data Sorteio” tem o valor “2026-10-17”, que não é uma data no formato dd/mm/aaaa.",
            erro.Message);
    }

    /// <summary>
    /// Quando o próprio número do concurso não pode ser lido, a mensagem traz só a linha.
    /// </summary>
    [Fact]
    public void NumeroDoConcursoIlegivel()
    {
        string[] linha = PlanilhaCaixa.CriarLinha(1);
        linha[0] = "x1";

        var erro = Assert.Throws<FonteResultadosException>(() => Ler([linha]));

        Assert.Equal("Linha 2: a coluna “Concurso” tem o valor “x1”, que não é um número inteiro.", erro.Message);
    }

    /// <summary>
    /// Título diferente numa coluna usada indica mudança de layout.
    /// </summary>
    [Fact]
    public void LayoutAlterado()
    {
        string[] cabecalho = [.. PlanilhaCaixa.CabecalhoPadrao];
        cabecalho[29] = "Arrecadação";

        var erro = Assert.Throws<FonteResultadosException>(() => Ler([PlanilhaCaixa.CriarLinha(1)], cabecalho));

        Assert.Equal(
            "O layout da planilha da Caixa mudou: a coluna 30 deveria ser “Arrecadacao Total” e é “Arrecadação”.",
            erro.Message);
    }

    /// <summary>
    /// Cabeçalho com colunas a menos indica mudança de layout, mesmo que a coluna faltante seja ignorada.
    /// </summary>
    [Fact]
    public void CabecalhoComColunasAMenos()
    {
        string[] cabecalho = PlanilhaCaixa.CabecalhoPadrao[..32];

        var erro = Assert.Throws<FonteResultadosException>(() => Ler([PlanilhaCaixa.CriarLinha(1)], cabecalho));

        Assert.Equal(
            "O layout da planilha da Caixa mudou: a coluna 33 deveria ser “Observação” e é “”.",
            erro.Message);
    }

    /// <summary>
    /// Planilha só com o cabeçalho não tem concursos.
    /// </summary>
    [Fact]
    public void PlanilhaSemConcursos()
    {
        var erro = Assert.Throws<FonteResultadosException>(() => Ler([]));

        Assert.Equal("A planilha da Caixa não tem nenhum concurso.", erro.Message);
    }

    /// <summary>
    /// O mesmo número de concurso em duas linhas é rejeitado na segunda.
    /// </summary>
    [Fact]
    public void ConcursoRepetido()
    {
        List<string[]> linhas = Enumerable.Range(1, 100).Select(PlanilhaCaixa.CriarLinha).ToList();
        linhas.Add(PlanilhaCaixa.CriarLinha(100));

        var erro = Assert.Throws<FonteResultadosException>(() => Ler(linhas));

        Assert.Equal("Linha 102: o concurso 100 aparece mais de uma vez na planilha.", erro.Message);
    }

    /// <summary>
    /// Conteúdo que não é XLSX (por exemplo, uma página HTML) é rejeitado.
    /// </summary>
    [Fact]
    public void ConteudoQueNaoEXlsx()
    {
        using var html = new MemoryStream(Encoding.UTF8.GetBytes("<html><body>Manutenção</body></html>"));

        var erro = Assert.Throws<FonteResultadosException>(() => LeitorPlanilhaHistorico.Ler(html));

        Assert.Equal(LeitorPlanilhaHistorico.MensagemArquivoInvalido, erro.Message);
    }

    /// <summary>
    /// Um ZIP sem a planilha esperada é rejeitado.
    /// </summary>
    [Fact]
    public void PacoteSemPlanilha()
    {
        using var conteudo = new MemoryStream();
        using (var pacote = new ZipArchive(conteudo, ZipArchiveMode.Create, leaveOpen: true))
        {
            pacote.CreateEntry("xl/workbook.xml");
        }

        conteudo.Position = 0;

        var erro = Assert.Throws<FonteResultadosException>(() => LeitorPlanilhaHistorico.Ler(conteudo));

        Assert.Equal(LeitorPlanilhaHistorico.MensagemArquivoInvalido, erro.Message);
    }

    /// <summary>
    /// Gera um XLSX com as linhas informadas e lê os concursos.
    /// </summary>
    /// <param name="linhas">Linhas de dados.</param>
    /// <param name="cabecalho">Cabeçalho; o padrão quando <c>null</c>.</param>
    /// <returns>Concursos lidos.</returns>
    private static IReadOnlyList<Concurso> Ler(IEnumerable<string[]> linhas, string[]? cabecalho = null)
    {
        using var conteudo = new MemoryStream(PlanilhaCaixa.Gerar(linhas, cabecalho));

        return LeitorPlanilhaHistorico.Ler(conteudo);
    }
}
