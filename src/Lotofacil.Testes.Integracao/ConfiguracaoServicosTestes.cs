// <copyright file="ConfiguracaoServicosTestes.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Aplicacao.Concursos;
using Lotofacil.Dominio.Concursos;
using Lotofacil.Infraestrutura;
using Lotofacil.Infraestrutura.Caixa;
using Microsoft.Extensions.DependencyInjection;

namespace Lotofacil.Testes.Integracao;

/// <summary>
/// Verifica se os serviços que usam a Caixa são resolvidos com o registro feito como no <c>Program.cs</c>.
/// </summary>
public class ConfiguracaoServicosTestes
{
    /// <summary>
    /// Os serviços de importação e de atualização são resolvidos, com as fontes da Caixa registradas pela
    /// infraestrutura.
    /// </summary>
    [Fact]
    public void ResolveImportacaoEAtualizacaoComFontesDaCaixa()
    {
        string caminhoBanco = Path.Combine(Path.GetTempPath(), "lotofacil-nao-criado.db");
        using ServiceProvider provedor = new ServiceCollection()
            .AddInfraestrutura(caminhoBanco, new OpcoesFonteResultados("https://exemplo.local", 60, 0))
            .AddSingleton<ImportacaoHistoricoServico>()
            .AddSingleton<AtualizacaoConcursosServico>()
            .BuildServiceProvider();

        Assert.NotNull(provedor.GetRequiredService<ImportacaoHistoricoServico>());
        Assert.NotNull(provedor.GetRequiredService<AtualizacaoConcursosServico>());
        Assert.IsType<HistoricoCaixaFonte>(provedor.GetRequiredService<IFonteHistoricoConcursos>());
        Assert.IsType<ConcursosCaixaFonte>(provedor.GetRequiredService<IFonteConcursos>());
    }
}
