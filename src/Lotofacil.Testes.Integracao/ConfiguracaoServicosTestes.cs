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
/// Verifica se os serviços da importação são resolvidos com o registro feito como no <c>Program.cs</c>.
/// </summary>
public class ConfiguracaoServicosTestes
{
    /// <summary>
    /// O serviço de importação é resolvido, com a fonte da Caixa registrada pela infraestrutura.
    /// </summary>
    [Fact]
    public void ResolveImportacaoComFonteDaCaixa()
    {
        string caminhoBanco = Path.Combine(Path.GetTempPath(), "lotofacil-nao-criado.db");
        using ServiceProvider provedor = new ServiceCollection()
            .AddInfraestrutura(caminhoBanco, new OpcoesFonteResultados("https://exemplo.local", 60))
            .AddSingleton<ImportacaoHistoricoServico>()
            .BuildServiceProvider();

        Assert.NotNull(provedor.GetRequiredService<ImportacaoHistoricoServico>());
        Assert.IsType<HistoricoCaixaFonte>(provedor.GetRequiredService<IFonteHistoricoConcursos>());
    }
}
