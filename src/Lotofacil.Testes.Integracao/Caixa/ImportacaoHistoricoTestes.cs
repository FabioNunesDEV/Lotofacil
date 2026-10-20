// <copyright file="ImportacaoHistoricoTestes.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using System.Net;
using Lotofacil.Aplicacao.Concursos;
using Lotofacil.Dominio.Concursos;
using Lotofacil.Infraestrutura.BancoDados;
using Lotofacil.Infraestrutura.Caixa;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Lotofacil.Testes.Integracao.Caixa;

/// <summary>
/// Importação de ponta a ponta: download com rede falsa, leitura do XLSX e gravação em SQLite em memória.
/// </summary>
public sealed class ImportacaoHistoricoTestes : IAsyncLifetime
{
    /// <summary>
    /// Conexão mantida aberta durante o teste; o banco em memória deixa de existir quando ela fecha.
    /// </summary>
    private readonly SqliteConnection conexao = new("Data Source=:memory:");

    /// <summary>
    /// Provedor com a fábrica de contextos ligada à conexão em memória.
    /// </summary>
    private readonly ServiceProvider provedor;

    /// <summary>
    /// Repositório real sobre o banco em memória.
    /// </summary>
    private readonly ConcursoRepositorio repositorio;

    /// <summary>
    /// Serviço testado, com a fonte da Caixa sobre a rede falsa.
    /// </summary>
    private readonly ImportacaoHistoricoServico servico;

    /// <summary>
    /// Conteúdo devolvido pela rede falsa a cada download.
    /// </summary>
    private byte[] arquivo = [];

    /// <summary>
    /// Monta o serviço com a fonte da Caixa e o repositório sobre o banco em memória.
    /// </summary>
    public ImportacaoHistoricoTestes()
    {
        this.provedor = new ServiceCollection()
            .AddDbContextFactory<LotofacilContexto>(opcoes => opcoes.UseSqlite(this.conexao))
            .BuildServiceProvider();
        this.repositorio = new ConcursoRepositorio(this.provedor.GetRequiredService<IDbContextFactory<LotofacilContexto>>());

        var handler = new RespostaFalsaHandler(_ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(this.arquivo),
        }));
        var fonte = new HistoricoCaixaFonte(new HttpClient(handler), new OpcoesFonteResultados("https://exemplo.local", 60));
        this.servico = new ImportacaoHistoricoServico(fonte, this.repositorio);
    }

    /// <summary>
    /// Abre a conexão e cria a estrutura do banco pelas migrations.
    /// </summary>
    /// <returns>Tarefa da preparação.</returns>
    public async ValueTask InitializeAsync()
    {
        await this.conexao.OpenAsync(TestContext.Current.CancellationToken);
        IDbContextFactory<LotofacilContexto> fabrica = this.provedor.GetRequiredService<IDbContextFactory<LotofacilContexto>>();
        await using LotofacilContexto contexto = await fabrica.CreateDbContextAsync(TestContext.Current.CancellationToken);
        await contexto.Database.MigrateAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Fecha a conexão, descartando o banco em memória.
    /// </summary>
    /// <returns>Tarefa da limpeza.</returns>
    public async ValueTask DisposeAsync()
    {
        await this.provedor.DisposeAsync();
        await this.conexao.DisposeAsync();
    }

    /// <summary>
    /// Com a base vazia, todos os concursos são gravados; importar de novo não grava nada.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task GravaTudoEReimportarNaoDuplica()
    {
        this.arquivo = PlanilhaCaixa.Gerar(Enumerable.Range(1, 5).Select(PlanilhaCaixa.CriarLinha));

        ResultadoImportacao primeira = await this.servico.ImportarAsync(TestContext.Current.CancellationToken);
        ResultadoImportacao segunda = await this.servico.ImportarAsync(TestContext.Current.CancellationToken);

        Assert.Equal(new ResultadoImportacao(5, 5), primeira);
        Assert.Equal(new ResultadoImportacao(5, 0), segunda);
        Assert.Equal(5, await this.repositorio.ContarAsync(TestContext.Current.CancellationToken));
    }

    /// <summary>
    /// Com parte dos concursos na base, só os novos são gravados e os existentes ficam como estavam.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task GravaSomenteOsNovosSemAlterarOsExistentes()
    {
        string[] linhaAntiga = PlanilhaCaixa.CriarLinha(1);
        linhaAntiga[29] = "R$1,00";
        this.arquivo = PlanilhaCaixa.Gerar([linhaAntiga]);
        await this.servico.ImportarAsync(TestContext.Current.CancellationToken);

        this.arquivo = PlanilhaCaixa.Gerar(Enumerable.Range(1, 3).Select(PlanilhaCaixa.CriarLinha));
        ResultadoImportacao resultado = await this.servico.ImportarAsync(TestContext.Current.CancellationToken);

        Assert.Equal(new ResultadoImportacao(3, 2), resultado);
        Concurso? existente = await this.repositorio.ObterAsync(1, TestContext.Current.CancellationToken);
        Assert.Equal(1m, existente?.Arrecadacao);
        Assert.Equal(3, await this.repositorio.ContarAsync(TestContext.Current.CancellationToken));
    }

    /// <summary>
    /// Uma linha inválida faz a importação falhar sem gravar nenhum concurso, nem os das linhas válidas.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task LinhaInvalidaNaoGravaNada()
    {
        List<string[]> linhas = Enumerable.Range(1, 5).Select(PlanilhaCaixa.CriarLinha).ToList();
        linhas[4][2] = "26";
        this.arquivo = PlanilhaCaixa.Gerar(linhas);

        var erro = await Assert.ThrowsAsync<FonteResultadosException>(
            () => this.servico.ImportarAsync(TestContext.Current.CancellationToken));

        Assert.StartsWith("Linha 6 (concurso 5):", erro.Message);
        Assert.Equal(0, await this.repositorio.ContarAsync(TestContext.Current.CancellationToken));
    }
}
