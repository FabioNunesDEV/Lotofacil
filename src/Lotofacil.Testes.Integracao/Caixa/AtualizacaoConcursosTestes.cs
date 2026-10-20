// <copyright file="AtualizacaoConcursosTestes.cs" company="Fábio Nunes">
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
/// Atualização de ponta a ponta: API JSON falsa, leitura das respostas e gravação em SQLite em memória.
/// </summary>
public sealed class AtualizacaoConcursosTestes : IAsyncLifetime
{
    /// <summary>
    /// Endereço do último concurso na API falsa.
    /// </summary>
    private const string EnderecoUltimo = "https://exemplo.local/api/lotofacil/";

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
    /// Rede falsa que responde como a API da Caixa.
    /// </summary>
    private readonly RespostaFalsaHandler handler;

    /// <summary>
    /// Serviço testado, com a fonte da Caixa sobre a rede falsa.
    /// </summary>
    private readonly AtualizacaoConcursosServico servico;

    /// <summary>
    /// Números de concurso que a API falsa responde com o código 503.
    /// </summary>
    private readonly HashSet<int> concursosComFalha = [];

    /// <summary>
    /// Último concurso divulgado pela API falsa.
    /// </summary>
    private int ultimoDivulgado = 3808;

    /// <summary>
    /// Monta o serviço com a fonte da Caixa e o repositório sobre o banco em memória.
    /// </summary>
    public AtualizacaoConcursosTestes()
    {
        this.provedor = new ServiceCollection()
            .AddDbContextFactory<LotofacilContexto>(opcoes => opcoes.UseSqlite(this.conexao))
            .BuildServiceProvider();
        this.repositorio = new ConcursoRepositorio(this.provedor.GetRequiredService<IDbContextFactory<LotofacilContexto>>());

        this.handler = new RespostaFalsaHandler((requisicao, _) => Task.FromResult(this.Responder(requisicao)));
        var cliente = new ClienteCaixa(new HttpClient(this.handler), new OpcoesFonteResultados("https://exemplo.local", 60, 0));
        this.servico = new AtualizacaoConcursosServico(new ConcursosCaixaFonte(cliente), this.repositorio);
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
    /// Os concursos novos são buscados em ordem e gravados com a ordem do sorteio.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task GravaOsNovosComOrdemDoSorteio()
    {
        await this.GravarConcurso3808Async();
        this.ultimoDivulgado = 3811;

        ResultadoAtualizacao resultado = await this.servico.AtualizarAsync(null, TestContext.Current.CancellationToken);

        Assert.Equal(new ResultadoAtualizacao(3808, 3, null), resultado);
        Assert.Equal(
            [EnderecoUltimo, $"{EnderecoUltimo}3809", $"{EnderecoUltimo}3810", $"{EnderecoUltimo}3811"],
            this.handler.Enderecos.Select(endereco => endereco.OriginalString));
        for (int numero = 3809; numero <= 3811; numero++)
        {
            Concurso? gravado = await this.repositorio.ObterAsync(numero, TestContext.Current.CancellationToken);
            Assert.NotNull(gravado?.OrdemSorteio);
        }
    }

    /// <summary>
    /// Uma falha no meio mantém os já gravados, e a atualização seguinte continua de onde parou.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task InterrupcaoPreservaOsGravadosEContinuaDepois()
    {
        await this.GravarConcurso3808Async();
        this.ultimoDivulgado = 3812;
        this.concursosComFalha.Add(3811);

        ResultadoAtualizacao interrompida = await this.servico.AtualizarAsync(null, TestContext.Current.CancellationToken);

        Assert.Equal(2, interrompida.ConcursosGravados);
        Assert.Equal("A Caixa respondeu com o código 503 ao buscar o concurso 3811.", interrompida.Falha?.Message);
        Assert.Equal(3810, await this.repositorio.ObterUltimoNumeroAsync(TestContext.Current.CancellationToken));

        this.concursosComFalha.Clear();
        this.handler.Enderecos.Clear();
        ResultadoAtualizacao seguinte = await this.servico.AtualizarAsync(null, TestContext.Current.CancellationToken);

        Assert.Equal(new ResultadoAtualizacao(3810, 2, null), seguinte);
        Assert.Equal($"{EnderecoUltimo}3811", this.handler.Enderecos[1].OriginalString);
        Assert.Equal(3812, await this.repositorio.ObterUltimoNumeroAsync(TestContext.Current.CancellationToken));
    }

    /// <summary>
    /// Com a base vazia, nenhuma requisição é feita.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task BaseVaziaNaoFazRequisicao()
    {
        ResultadoAtualizacao resultado = await this.servico.AtualizarAsync(null, TestContext.Current.CancellationToken);

        Assert.True(resultado.BaseVazia);
        Assert.Empty(this.handler.Enderecos);
    }

    /// <summary>
    /// Grava na base o concurso 3808 da resposta real.
    /// </summary>
    /// <returns>Tarefa da gravação.</returns>
    private async Task GravarConcurso3808Async()
    {
        Concurso concurso = LeitorConcursoJson.LerConcurso(RespostaConcursoCaixa.ComNumero(3808), 3808);
        await this.repositorio.AdicionarAsync([concurso], TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Responde como a API da Caixa: último concurso sem número, ou o concurso pedido.
    /// </summary>
    /// <param name="requisicao">Requisição recebida.</param>
    /// <returns>Resposta da API falsa.</returns>
    private HttpResponseMessage Responder(HttpRequestMessage requisicao)
    {
        string numeroPedido = requisicao.RequestUri!.Segments[^1];
        if (!int.TryParse(numeroPedido, out int numero))
        {
            return RespostaFalsaHandler.CriarResposta(HttpStatusCode.OK, RespostaConcursoCaixa.ComNumero(this.ultimoDivulgado));
        }

        if (this.concursosComFalha.Contains(numero))
        {
            return RespostaFalsaHandler.CriarResposta(HttpStatusCode.ServiceUnavailable);
        }

        return RespostaFalsaHandler.CriarResposta(HttpStatusCode.OK, RespostaConcursoCaixa.ComNumero(numero));
    }
}
