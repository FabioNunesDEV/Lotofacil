// <copyright file="ConfiguracaoInfraestrutura.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Dominio.Concursos;
using Lotofacil.Dominio.Configuracoes;
using Lotofacil.Dominio.Jogos;
using Lotofacil.Dominio.Teimosinhas;
using Lotofacil.Infraestrutura.BancoDados;
using Lotofacil.Infraestrutura.Caixa;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Lotofacil.Infraestrutura;

/// <summary>
/// Registra os serviços da infraestrutura e prepara a base local.
/// </summary>
public static class ConfiguracaoInfraestrutura
{
    /// <summary>
    /// Tempo que uma conexão com a Caixa fica no pool antes de ser renovada (acompanha mudanças de DNS).
    /// </summary>
    private static readonly TimeSpan VidaConexao = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Registra o acesso à base SQLite, os repositórios e a fonte de resultados da Caixa.
    /// </summary>
    /// <param name="servicos">Coleção de serviços da aplicação.</param>
    /// <param name="caminhoBanco">Caminho do arquivo SQLite.</param>
    /// <param name="opcoesFonte">Configuração do acesso à API da Caixa.</param>
    /// <returns>A mesma coleção de serviços.</returns>
    public static IServiceCollection AddInfraestrutura(
        this IServiceCollection servicos,
        string caminhoBanco,
        OpcoesFonteResultados opcoesFonte)
    {
        servicos.AddDbContextFactory<LotofacilContexto>(opcoes => opcoes.UseSqlite($"Data Source={caminhoBanco}"));
        servicos.AddSingleton<IConcursoRepositorio, ConcursoRepositorio>();
        servicos.AddSingleton<IJogoRepositorio, JogoRepositorio>();
        servicos.AddSingleton<IConfiguracaoApostasRepositorio, ConfiguracaoApostasRepositorio>();
        servicos.AddSingleton<ITeimosinhaRepositorio, TeimosinhaRepositorio>();

        // Um único cliente da Caixa para todas as fontes, para a pausa valer entre todas as requisições.
        servicos.AddSingleton(_ => new ClienteCaixa(CriarClienteHttp(opcoesFonte), opcoesFonte));
        servicos.AddSingleton<IFonteHistoricoConcursos, HistoricoCaixaFonte>();
        servicos.AddSingleton<IFonteConcursos, ConcursosCaixaFonte>();

        return servicos;
    }

    /// <summary>
    /// Cria o arquivo da base, se ainda não existir, e aplica as migrations pendentes.
    /// </summary>
    /// <param name="provedor">Provedor de serviços já montado.</param>
    /// <param name="cancelamento">Sinal de cancelamento.</param>
    /// <returns>Tarefa da inicialização.</returns>
    public static async Task InicializarBancoDadosAsync(
        this IServiceProvider provedor,
        CancellationToken cancelamento = default)
    {
        IDbContextFactory<LotofacilContexto> fabrica = provedor.GetRequiredService<IDbContextFactory<LotofacilContexto>>();
        await using LotofacilContexto contexto = await fabrica.CreateDbContextAsync(cancelamento);

        await contexto.Database.MigrateAsync(cancelamento);
    }

    /// <summary>
    /// Cria o cliente HTTP usado nas requisições à Caixa.
    /// </summary>
    /// <param name="opcoesFonte">Configuração com o tempo limite.</param>
    /// <returns>Cliente HTTP configurado.</returns>
    private static HttpClient CriarClienteHttp(OpcoesFonteResultados opcoesFonte)
    {
        var handler = new SocketsHttpHandler { PooledConnectionLifetime = VidaConexao };

        return new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(opcoesFonte.TempoLimiteSegundos) };
    }
}
