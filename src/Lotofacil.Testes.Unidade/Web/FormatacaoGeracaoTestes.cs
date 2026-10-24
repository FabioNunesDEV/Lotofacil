// <copyright file="FormatacaoGeracaoTestes.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using System.Globalization;
using Lotofacil.Aplicacao.Jogos;
using Lotofacil.Dominio.Estatisticas.Perfis;
using Lotofacil.Dominio.Geracao;
using Lotofacil.Web.Components.Geracao;

namespace Lotofacil.Testes.Unidade.Web;

/// <summary>
/// Testes dos formatos da página Gerar jogos, na cultura pt-BR.
/// </summary>
public class FormatacaoGeracaoTestes
{
    /// <summary>
    /// Usa a cultura da aplicação.
    /// </summary>
    public FormatacaoGeracaoTestes()
    {
        CultureInfo.CurrentCulture = new CultureInfo("pt-BR");
    }

    /// <summary>
    /// Nome e descrição de cada estratégia.
    /// </summary>
    [Fact]
    public void NomesEDescricoes()
    {
        Assert.Equal("Aleatória (Surpresinha)", FormatacaoGeracao.Nome(EstrategiaGeracao.Aleatoria));
        Assert.Equal("Mais sorteadas", FormatacaoGeracao.Nome(EstrategiaGeracao.MaisSorteadas));
        Assert.Equal("Menos sorteadas", FormatacaoGeracao.Nome(EstrategiaGeracao.MenosSorteadas));
        Assert.Equal("Mais atrasadas", FormatacaoGeracao.Nome(EstrategiaGeracao.MaisAtrasadas));
        Assert.Equal("Equilibrada", FormatacaoGeracao.Nome(EstrategiaGeracao.Equilibrada));
        Assert.Equal("Todas as dezenas livres têm a mesma chance.", FormatacaoGeracao.Descricao(EstrategiaGeracao.Aleatoria));
        Assert.Equal(
            "Mais chance para as dezenas mais sorteadas na janela.",
            FormatacaoGeracao.Descricao(EstrategiaGeracao.MaisSorteadas));
        Assert.Equal(
            "Mais chance para as dezenas menos sorteadas na janela.",
            FormatacaoGeracao.Descricao(EstrategiaGeracao.MenosSorteadas));
        Assert.Equal(
            "Mais chance para as dezenas há mais tempo sem sair (todo o histórico).",
            FormatacaoGeracao.Descricao(EstrategiaGeracao.MaisAtrasadas));
        Assert.Equal(
            "Metade das dezenas livres pelas mais sorteadas na janela, metade pelas mais atrasadas.",
            FormatacaoGeracao.Descricao(EstrategiaGeracao.Equilibrada));
    }

    /// <summary>
    /// Custo da geração e subtítulo da lista, no singular e no plural.
    /// </summary>
    [Fact]
    public void CustoESubtitulo()
    {
        Assert.Equal("10 jogos de 15 dezenas · R$ 35,00", FormatacaoGeracao.Custo(10, 15, 35m));
        Assert.Equal("5 jogos de 17 dezenas · R$ 2.380,00", FormatacaoGeracao.Custo(5, 17, 2_380m));
        Assert.Equal("1 jogo de 15 dezenas · R$ 3,50", FormatacaoGeracao.Custo(1, 15, 3.5m));
        Assert.Equal("Nenhum jogo gerado ainda", FormatacaoGeracao.SubtituloLista(0, 0m));
        Assert.Equal("9 jogos · R$ 31,50", FormatacaoGeracao.SubtituloLista(9, 31.5m));
        Assert.Equal("1 jogo · R$ 3,50", FormatacaoGeracao.SubtituloLista(1, 3.5m));
    }

    /// <summary>
    /// Perfil do jogo.
    /// </summary>
    [Fact]
    public void Perfil()
    {
        var perfil = new PerfilJogo(7, 8, 120, 8);

        Assert.Equal("7 pares · 8 ímpares", FormatacaoGeracao.Paridade(perfil));
        Assert.Equal("Soma 120", FormatacaoGeracao.Soma(perfil));
        Assert.Equal("8 repetidas do 3810", FormatacaoGeracao.Repetidas(perfil, 3810));
        Assert.Equal("1 repetida do 3810", FormatacaoGeracao.Repetidas(perfil with { Repetidas = 1 }, 3810));
    }

    /// <summary>
    /// Aviso de poucas combinações no singular e no plural, sem e com filtros.
    /// </summary>
    [Fact]
    public void AvisoCombinacoes()
    {
        Assert.Equal(
            "Só existe 1 jogo diferente com essas fixas e excluídas; foi gerado 1.",
            FormatacaoGeracao.AvisoCombinacoes(1, false));
        Assert.Equal(
            "Só existem 6 jogos diferentes com essas fixas e excluídas; foram gerados 6.",
            FormatacaoGeracao.AvisoCombinacoes(6, false));
        Assert.Equal(
            "Só existem 2 jogos diferentes com essas fixas, excluídas e filtros; foram gerados 2.",
            FormatacaoGeracao.AvisoCombinacoes(2, true));
        Assert.Equal(
            "Só existe 1 jogo diferente com essas fixas, excluídas e filtros; foi gerado 1.",
            FormatacaoGeracao.AvisoCombinacoes(1, true));
    }

    /// <summary>
    /// Aviso de jogos a menos por causa dos filtros, no plural e no singular.
    /// </summary>
    [Fact]
    public void AvisoFiltros()
    {
        Assert.Equal(
            "Foram gerados 3 dos 10 jogos pedidos: poucos jogos atendem aos filtros.",
            FormatacaoGeracao.AvisoFiltros(3, 10));
        Assert.Equal(
            "Foi gerado 1 dos 10 jogos pedidos: poucos jogos atendem aos filtros.",
            FormatacaoGeracao.AvisoFiltros(1, 10));
    }

    /// <summary>
    /// Resumo dos filtros ligados.
    /// </summary>
    [Fact]
    public void FiltrosLigados()
    {
        Assert.Equal("Nenhum filtro ligado", FormatacaoGeracao.FiltrosLigados(0));
        Assert.Equal("1 filtro ligado", FormatacaoGeracao.FiltrosLigados(1));
        Assert.Equal("2 filtros ligados", FormatacaoGeracao.FiltrosLigados(2));
    }

    /// <summary>
    /// Rótulos das linhas de filtro, com o último concurso nas repetidas.
    /// </summary>
    [Fact]
    public void RotulosDosFiltros()
    {
        Assert.Equal(
            ["Pares", "Primos", "Múltiplos de 3", "Fibonacci", "Soma", "Repetidas do 3810", "Na moldura"],
            Enum.GetValues<TipoPerfil>().Select(tipo => FormatacaoGeracao.RotuloFiltro(tipo, 3810)));
    }

    /// <summary>
    /// Dica de chance teórica para 15 dezenas; um lado vazio vai até o extremo do perfil.
    /// </summary>
    [Fact]
    public void ChanceTeorica()
    {
        Assert.Equal("Chance teórica: 57,17%", FormatacaoGeracao.ChanceTeorica(new LimitePerfil(TipoPerfil.Pares, 7, 8)));
        Assert.Equal("Chance teórica: 58,73%", FormatacaoGeracao.ChanceTeorica(new LimitePerfil(TipoPerfil.Soma, 180, 209)));
        Assert.Equal("Chance teórica: 100,00%", FormatacaoGeracao.ChanceTeorica(new LimitePerfil(TipoPerfil.Pares, 0, null)));
        Assert.Equal("Chance teórica: 0,00%", FormatacaoGeracao.ChanceTeorica(new LimitePerfil(TipoPerfil.Pares, 13, null)));
        Assert.Equal("Chance teórica: 0,00%", FormatacaoGeracao.ChanceTeorica(new LimitePerfil(TipoPerfil.Soma, 210, 180)));
    }

    /// <summary>
    /// Mensagens do salvamento.
    /// </summary>
    /// <param name="gravados">Jogos gravados.</param>
    /// <param name="duplicados">Jogos duplicados.</param>
    /// <param name="esperado">Mensagem esperada.</param>
    [Theory]
    [InlineData(10, 0, "10 jogos salvos para o concurso 3811.")]
    [InlineData(1, 0, "1 jogo salvo para o concurso 3811.")]
    [InlineData(8, 2, "8 jogos salvos para o concurso 3811; 2 já estavam cadastrados.")]
    [InlineData(1, 1, "1 jogo salvo para o concurso 3811; 1 já estava cadastrado.")]
    [InlineData(0, 3, "Nenhum jogo salvo: todos já estavam cadastrados para o concurso 3811.")]
    public void Salvamento(int gravados, int duplicados, string esperado)
    {
        Assert.Equal(esperado, FormatacaoGeracao.Salvamento(new ResultadoCadastroVarios(gravados, duplicados, null), 3811));
    }

    /// <summary>
    /// Recusa do concurso é repassada como está.
    /// </summary>
    [Fact]
    public void SalvamentoRecusado()
    {
        const string Recusa = "Só é possível cadastrar jogos para concursos depois do 3810.";

        Assert.Equal(Recusa, FormatacaoGeracao.Salvamento(new ResultadoCadastroVarios(0, 0, Recusa), 3810));
    }
}
