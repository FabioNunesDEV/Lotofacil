// <copyright file="FormatacaoGeracao.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Aplicacao.Jogos;
using Lotofacil.Dominio.Estatisticas.Perfis;
using Lotofacil.Dominio.Geracao;
using Lotofacil.Web.Components.Estatisticas;
using Lotofacil.Web.Components.Jogos;

namespace Lotofacil.Web.Components.Geracao;

/// <summary>
/// Formatos dos textos da página Gerar jogos, na cultura atual.
/// </summary>
public static class FormatacaoGeracao
{
    /// <summary>
    /// Frase do topo da seção de filtros, abaixo do aviso.
    /// </summary>
    public const string InstrucaoFiltros = "Mín. e Máx.: quantas dezenas do jogo entram em cada grupo. Deixe vazio para não limitar.";

    /// <summary>
    /// Nome da estratégia exibido na lista de opções.
    /// </summary>
    /// <param name="estrategia">Estratégia.</param>
    /// <returns>Nome da estratégia.</returns>
    public static string Nome(EstrategiaGeracao estrategia)
    {
        return estrategia switch
        {
            EstrategiaGeracao.Aleatoria => "Aleatória (Surpresinha)",
            EstrategiaGeracao.MaisSorteadas => "Mais sorteadas",
            EstrategiaGeracao.MenosSorteadas => "Menos sorteadas",
            EstrategiaGeracao.MaisAtrasadas => "Mais atrasadas",
            _ => "Equilibrada",
        };
    }

    /// <summary>
    /// Descrição da estratégia exibida abaixo da escolha.
    /// </summary>
    /// <param name="estrategia">Estratégia.</param>
    /// <returns>Descrição da estratégia.</returns>
    public static string Descricao(EstrategiaGeracao estrategia)
    {
        return estrategia switch
        {
            EstrategiaGeracao.Aleatoria => "Todas as dezenas livres têm a mesma chance.",
            EstrategiaGeracao.MaisSorteadas => "Mais chance para as dezenas mais sorteadas na janela.",
            EstrategiaGeracao.MenosSorteadas => "Mais chance para as dezenas menos sorteadas na janela.",
            EstrategiaGeracao.MaisAtrasadas => "Mais chance para as dezenas há mais tempo sem sair (todo o histórico).",
            _ => "Metade das dezenas livres pelas mais sorteadas na janela, metade pelas mais atrasadas.",
        };
    }

    /// <summary>
    /// Custo da geração pedida no formulário.
    /// </summary>
    /// <param name="quantidade">Quantidade de jogos.</param>
    /// <param name="dezenasPorJogo">Dezenas por jogo.</param>
    /// <param name="custo">Custo total.</param>
    /// <returns>Texto "10 jogos de 15 dezenas · R$ 35,00".</returns>
    public static string Custo(int quantidade, int dezenasPorJogo, decimal custo)
    {
        return $"{FormatacaoJogo.QuantidadeJogos(quantidade)} de {dezenasPorJogo} dezenas · {FormatacaoJogo.Reais(custo)}";
    }

    /// <summary>
    /// Subtítulo do cartão dos jogos gerados.
    /// </summary>
    /// <param name="quantidade">Quantidade de jogos na lista.</param>
    /// <param name="custo">Custo dos jogos da lista.</param>
    /// <returns>Texto do subtítulo.</returns>
    public static string SubtituloLista(int quantidade, decimal custo)
    {
        return quantidade == 0
            ? "Nenhum jogo gerado ainda"
            : $"{FormatacaoJogo.QuantidadeJogos(quantidade)} · {FormatacaoJogo.Reais(custo)}";
    }

    /// <summary>
    /// Pares e ímpares do jogo.
    /// </summary>
    /// <param name="perfil">Perfil do jogo.</param>
    /// <returns>Texto "7 pares · 8 ímpares".</returns>
    public static string Paridade(PerfilJogo perfil)
    {
        return $"{perfil.Pares} pares · {perfil.Impares} ímpares";
    }

    /// <summary>
    /// Soma das dezenas do jogo.
    /// </summary>
    /// <param name="perfil">Perfil do jogo.</param>
    /// <returns>Texto "Soma 120".</returns>
    public static string Soma(PerfilJogo perfil)
    {
        return $"Soma {perfil.Soma}";
    }

    /// <summary>
    /// Repetidas do último concurso gravado.
    /// </summary>
    /// <param name="perfil">Perfil do jogo.</param>
    /// <param name="concurso">Número do último concurso gravado.</param>
    /// <returns>Texto "8 repetidas do 3810".</returns>
    public static string Repetidas(PerfilJogo perfil, int concurso)
    {
        return perfil.Repetidas == 1 ? $"1 repetida do {concurso}" : $"{perfil.Repetidas} repetidas do {concurso}";
    }

    /// <summary>
    /// Aviso de que há menos jogos diferentes possíveis que a quantidade pedida.
    /// </summary>
    /// <param name="possiveis">Jogos diferentes possíveis (que atendem aos filtros).</param>
    /// <param name="comFiltros">Indica que a geração tinha algum filtro ligado.</param>
    /// <returns>Texto do aviso.</returns>
    public static string AvisoCombinacoes(int possiveis, bool comFiltros)
    {
        string condicao = comFiltros ? "com essas fixas, excluídas e filtros" : "com essas fixas e excluídas";

        return possiveis == 1
            ? $"Só existe 1 jogo diferente {condicao}; foi gerado 1."
            : $"Só existem {possiveis} jogos diferentes {condicao}; foram gerados {possiveis}.";
    }

    /// <summary>
    /// Aviso de que o sorteio com filtros achou menos jogos que o pedido.
    /// </summary>
    /// <param name="gerados">Jogos achados.</param>
    /// <param name="pedidos">Jogos pedidos.</param>
    /// <returns>Texto "Foram gerados 3 dos 10 jogos pedidos: poucos jogos atendem aos filtros.".</returns>
    public static string AvisoFiltros(int gerados, int pedidos)
    {
        string inicio = gerados == 1 ? "Foi gerado 1" : $"Foram gerados {gerados}";

        return $"{inicio} dos {pedidos} jogos pedidos: poucos jogos atendem aos filtros.";
    }

    /// <summary>
    /// Resumo exibido no cabeçalho da seção de filtros.
    /// </summary>
    /// <param name="ligados">Quantidade de filtros ligados.</param>
    /// <returns>"Nenhum filtro ligado", "1 filtro ligado" ou "3 filtros ligados".</returns>
    public static string FiltrosLigados(int ligados)
    {
        return ligados switch
        {
            0 => "Nenhum filtro ligado",
            1 => "1 filtro ligado",
            _ => $"{ligados} filtros ligados",
        };
    }

    /// <summary>
    /// Rótulo da linha de um perfil na seção de filtros.
    /// </summary>
    /// <param name="tipo">Perfil.</param>
    /// <param name="ultimoConcurso">Número do último concurso gravado, base das repetidas.</param>
    /// <returns>Rótulo (ex.: "Pares", "Repetidas do 3810").</returns>
    public static string RotuloFiltro(TipoPerfil tipo, int ultimoConcurso)
    {
        return tipo switch
        {
            TipoPerfil.Pares => "Pares",
            TipoPerfil.Primos => "Primos",
            TipoPerfil.MultiplosDe3 => "Múltiplos de 3",
            TipoPerfil.Fibonacci => "Fibonacci",
            TipoPerfil.Soma => "Soma",
            TipoPerfil.Repetidas => $"Repetidas do {ultimoConcurso}",
            _ => "Na moldura",
        };
    }

    /// <summary>
    /// Explicação exibida embaixo do nome de um filtro.
    /// </summary>
    /// <param name="tipo">Perfil.</param>
    /// <param name="ultimoConcurso">Número do último concurso gravado, base das repetidas.</param>
    /// <param name="dezenasPorJogo">Dezenas por jogo, que definem a menor e a maior soma.</param>
    /// <returns>Explicação (ex.: "12 pares no volante").</returns>
    public static string ExplicacaoFiltro(TipoPerfil tipo, int ultimoConcurso, int dezenasPorJogo)
    {
        return tipo switch
        {
            TipoPerfil.Pares => "12 pares no volante",
            TipoPerfil.Primos => "02 03 05 07 11 13 17 19 23",
            TipoPerfil.MultiplosDe3 => "03 06 09 12 15 18 21 24",
            TipoPerfil.Fibonacci => "01 02 03 05 08 13 21",
            TipoPerfil.Soma => $"Soma das dezenas do jogo (de {FiltrosGeracao.MenorLimite(tipo, dezenasPorJogo)} a {FiltrosGeracao.MaiorLimite(tipo, dezenasPorJogo)})",
            TipoPerfil.Repetidas => $"Dezenas do jogo que saíram no {ultimoConcurso}",
            _ => "As 16 dezenas da borda do volante (miolo: 07 08 09 12 13 14 17 18 19)",
        };
    }

    /// <summary>
    /// Valor mais comum do perfil entre as combinações de 15 dezenas, para completar a explicação.
    /// </summary>
    /// <param name="tipo">Perfil.</param>
    /// <returns>Texto " · mais comum: 7 ou 8 (57%)" ou, na soma, " · mais comum: 180 a 209 (59%)".</returns>
    public static string MaisComum(TipoPerfil tipo)
    {
        (int minimo, int maximo) = DistribuicaoTeorica.FaixaMaisComum(tipo);
        decimal percentual = DistribuicaoTeorica.Percentual(tipo, minimo, maximo);
        string separador = tipo == TipoPerfil.Soma ? "a" : "ou";

        return $" · mais comum: {minimo} {separador} {maximo} ({Math.Round(percentual):0}%)";
    }

    /// <summary>
    /// Dica da parte das combinações de 15 dezenas que fica dentro dos limites; um lado vazio usa o extremo do perfil.
    /// </summary>
    /// <param name="limite">Limites do perfil.</param>
    /// <returns>Texto "Chance teórica: 57,17%".</returns>
    public static string ChanceTeorica(LimitePerfil limite)
    {
        int minimo = limite.Minimo ?? DistribuicaoTeorica.MenorValor(limite.Tipo);
        int maximo = limite.Maximo ?? DistribuicaoTeorica.MaiorValor(limite.Tipo);
        decimal percentual = minimo > maximo ? 0m : DistribuicaoTeorica.Percentual(limite.Tipo, minimo, maximo);

        return $"Chance teórica: {FormatacaoEstatistica.Percentual(percentual)}";
    }

    /// <summary>
    /// Mensagem do desfecho do salvamento.
    /// </summary>
    /// <param name="resultado">Gravados, duplicados ou o motivo da recusa.</param>
    /// <param name="concurso">Número do concurso.</param>
    /// <param name="teimosinha">Nome da Teimosinha que recebeu os jogos, ou <c>null</c>.</param>
    /// <returns>Texto da mensagem.</returns>
    public static string Salvamento(ResultadoCadastroVarios resultado, int concurso, string? teimosinha = null)
    {
        if (resultado.Recusa is string recusa)
        {
            return recusa;
        }

        if (resultado.Gravados == 0)
        {
            return $"Nenhum jogo salvo: todos já estavam cadastrados para o concurso {concurso}.";
        }

        string salvos = resultado.Gravados == 1 ? "1 jogo salvo" : $"{resultado.Gravados:N0} jogos salvos";
        string destino = teimosinha is null ? $"para o concurso {concurso}" : $"para o concurso {concurso} na Teimosinha {teimosinha}";
        return resultado.Duplicados switch
        {
            0 => $"{salvos} {destino}.",
            1 => $"{salvos} {destino}; 1 já estava cadastrado.",
            _ => $"{salvos} {destino}; {resultado.Duplicados:N0} já estavam cadastrados.",
        };
    }
}
