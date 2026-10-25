// <copyright file="CaminhoConfiguradoTestes.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Web;

namespace Lotofacil.Testes.Unidade.Web;

/// <summary>
/// Testes da resolução dos caminhos da configuração.
/// </summary>
public class CaminhoConfiguradoTestes
{
    /// <summary>
    /// Caminho relativo fica dentro da pasta da aplicação.
    /// </summary>
    [Fact]
    public void RelativoFicaNaPastaDaAplicacao()
    {
        Assert.Equal(@"C:\App\lotofacil.db", CaminhoConfigurado.Resolver(@"C:\App", "lotofacil.db", "padrao.db"));
    }

    /// <summary>
    /// Caminho absoluto, como o gravado pelo instalador, ignora a pasta da aplicação.
    /// </summary>
    [Fact]
    public void AbsolutoIgnoraAPastaDaAplicacao()
    {
        Assert.Equal(@"D:\Loteria\log", CaminhoConfigurado.Resolver(@"C:\App", @"D:\Loteria\log", "log"));
    }

    /// <summary>
    /// Sem valor na configuração, usa o padrão dentro da pasta da aplicação.
    /// </summary>
    /// <param name="configurado">Valor ausente ou vazio.</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void SemValorUsaOPadrao(string? configurado)
    {
        Assert.Equal(@"C:\App\log", CaminhoConfigurado.Resolver(@"C:\App", configurado, "log"));
    }
}
