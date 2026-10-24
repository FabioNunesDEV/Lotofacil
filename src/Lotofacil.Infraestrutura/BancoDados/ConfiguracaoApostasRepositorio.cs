// <copyright file="ConfiguracaoApostasRepositorio.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Dominio.Configuracoes;
using Microsoft.EntityFrameworkCore;

namespace Lotofacil.Infraestrutura.BancoDados;

/// <summary>
/// Repositório da configuração das apostas sobre o SQLite.
/// </summary>
internal sealed class ConfiguracaoApostasRepositorio : IConfiguracaoApostasRepositorio
{
    /// <summary>
    /// Cria um contexto novo a cada operação, como nos demais repositórios.
    /// </summary>
    private readonly IDbContextFactory<LotofacilContexto> fabricaContexto;

    /// <summary>
    /// Cria o repositório.
    /// </summary>
    /// <param name="fabricaContexto">Fábrica de contextos do EF Core.</param>
    public ConfiguracaoApostasRepositorio(IDbContextFactory<LotofacilContexto> fabricaContexto)
    {
        this.fabricaContexto = fabricaContexto;
    }

    /// <inheritdoc/>
    public async Task<ConfiguracaoApostas?> ObterAsync(CancellationToken cancelamento = default)
    {
        await using LotofacilContexto contexto = await this.fabricaContexto.CreateDbContextAsync(cancelamento);

        return await contexto.ConfiguracoesApostas
            .AsNoTracking()
            .SingleOrDefaultAsync(configuracao => configuracao.Id == ConfiguracaoApostas.IdUnico, cancelamento);
    }

    /// <inheritdoc/>
    public async Task SalvarAsync(ConfiguracaoApostas configuracao, CancellationToken cancelamento = default)
    {
        await using LotofacilContexto contexto = await this.fabricaContexto.CreateDbContextAsync(cancelamento);

        ConfiguracaoApostas? gravada = await contexto.ConfiguracoesApostas
            .SingleOrDefaultAsync(item => item.Id == ConfiguracaoApostas.IdUnico, cancelamento);
        if (gravada is null)
        {
            contexto.ConfiguracoesApostas.Add(configuracao);
        }
        else
        {
            contexto.Entry(gravada).CurrentValues.SetValues(configuracao);
        }

        await contexto.SaveChangesAsync(cancelamento);
    }
}
