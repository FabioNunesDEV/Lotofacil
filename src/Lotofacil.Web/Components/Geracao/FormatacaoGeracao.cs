// <copyright file="FormatacaoGeracao.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Aplicacao.Jogos;
using Lotofacil.Dominio.Geracao;
using Lotofacil.Web.Components.Jogos;

namespace Lotofacil.Web.Components.Geracao;

/// <summary>
/// Formatos dos textos da página Gerar jogos, na cultura atual.
/// </summary>
public static class FormatacaoGeracao
{
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
    /// <param name="possiveis">Jogos diferentes possíveis.</param>
    /// <returns>Texto do aviso.</returns>
    public static string AvisoCombinacoes(int possiveis)
    {
        return possiveis == 1
            ? "Só existe 1 jogo diferente com essas fixas e excluídas; foi gerado 1."
            : $"Só existem {possiveis} jogos diferentes com essas fixas e excluídas; foram gerados {possiveis}.";
    }

    /// <summary>
    /// Mensagem do desfecho do salvamento.
    /// </summary>
    /// <param name="resultado">Gravados, duplicados ou o motivo da recusa.</param>
    /// <param name="concurso">Número do concurso.</param>
    /// <returns>Texto da mensagem.</returns>
    public static string Salvamento(ResultadoCadastroVarios resultado, int concurso)
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
        return resultado.Duplicados switch
        {
            0 => $"{salvos} para o concurso {concurso}.",
            1 => $"{salvos} para o concurso {concurso}; 1 já estava cadastrado.",
            _ => $"{salvos} para o concurso {concurso}; {resultado.Duplicados:N0} já estavam cadastrados.",
        };
    }
}
