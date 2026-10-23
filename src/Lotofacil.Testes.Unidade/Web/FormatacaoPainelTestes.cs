// <copyright file="FormatacaoPainelTestes.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using System.Globalization;
using Lotofacil.Dominio.Concursos;
using Lotofacil.Web.Components.Concursos;
using Lotofacil.Web.Components.Painel;

namespace Lotofacil.Testes.Unidade.Web;

/// <summary>
/// Testes dos formatos do Painel, na cultura pt-BR.
/// </summary>
public class FormatacaoPainelTestes
{
    /// <summary>
    /// Usa a cultura da aplicação.
    /// </summary>
    public FormatacaoPainelTestes()
    {
        CultureInfo.CurrentCulture = new CultureInfo("pt-BR");
    }

    /// <summary>
    /// Data do último sorteio gravado.
    /// </summary>
    [Fact]
    public void UltimoSorteio()
    {
        Assert.Equal("Último sorteio em 17/10/2026", FormatacaoPainel.UltimoSorteio(new DateOnly(2026, 10, 17)));
    }

    /// <summary>
    /// Estimativa em reais, ou não informada quando zero.
    /// </summary>
    [Fact]
    public void Estimativa()
    {
        Assert.Equal("Estimativa R$ 1.800.000,00", FormatacaoPainel.Estimativa(1_800_000m));
        Assert.Equal("Estimativa não informada", FormatacaoPainel.Estimativa(0m));
    }

    /// <summary>
    /// Detalhe dos jogos aguardando: nenhum, um concurso ou vários.
    /// </summary>
    [Fact]
    public void ConcursosAguardando()
    {
        Assert.Equal("Nenhum jogo aguardando sorteio", FormatacaoPainel.ConcursosAguardando([]));
        Assert.Equal("Concurso 3809", FormatacaoPainel.ConcursosAguardando([3809]));
        Assert.Equal("2 concursos", FormatacaoPainel.ConcursosAguardando([3809, 3810]));
    }

    /// <summary>
    /// Ganhadores de 15 acertos no singular, no plural ou acumulado.
    /// </summary>
    [Fact]
    public void Ganhadores15()
    {
        Assert.Equal("Acumulou", FormatacaoPainel.Ganhadores15(new Rateio(15, 0, 0m)));
        Assert.Equal("1 ganhador", FormatacaoPainel.Ganhadores15(new Rateio(15, 1, 1_712_408.63m)));
        Assert.Equal("2 ganhadores", FormatacaoPainel.Ganhadores15(new Rateio(15, 2, 856_204.31m)));
        Assert.Equal("1.024 ganhadores", FormatacaoPainel.Ganhadores15(new Rateio(15, 1024, 1_000m)));
    }

    /// <summary>
    /// Data com o dia da semana.
    /// </summary>
    [Fact]
    public void DataComDiaSemana()
    {
        Assert.Equal("sábado, 17/10/2026", FormatacaoConcurso.DataComDiaSemana(new DateOnly(2026, 10, 17)));
    }
}
