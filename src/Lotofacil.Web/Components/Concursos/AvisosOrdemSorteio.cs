// <copyright file="AvisosOrdemSorteio.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Aplicacao.Concursos;
using MudBlazor;

namespace Lotofacil.Web.Components.Concursos;

/// <summary>
/// Textos do andamento e do desfecho do preenchimento da ordem do sorteio, usados pela página Resultados.
/// </summary>
public static class AvisosOrdemSorteio
{
    /// <summary>
    /// Mensagem exibida quando, ao acionar o preenchimento, nenhum concurso está sem ordem.
    /// </summary>
    public const string TodosComOrdem = "Todos os concursos já têm a ordem do sorteio.";

    /// <summary>
    /// Andamento exibido antes de saber quantos concursos serão buscados.
    /// </summary>
    public const string Preparando = "Procurando os concursos sem a ordem do sorteio…";

    /// <summary>
    /// Mensagem completa de uma falha não prevista no preenchimento.
    /// </summary>
    public static string FalhaInesperada => $"O preenchimento foi interrompido. {AvisosAtualizacao.MotivoErroInesperado}";

    /// <summary>
    /// Texto do andamento: de qual concurso a ordem está sendo buscada ou, numa pausa pedida pela Caixa, quando será
    /// a nova tentativa.
    /// </summary>
    /// <param name="andamento">Concurso, posição e espera.</param>
    /// <param name="agora">Horário local de agora, base da nova tentativa.</param>
    /// <returns>Texto do andamento.</returns>
    public static string Andamento(AndamentoOrdemSorteio andamento, DateTime agora)
    {
        string posicao = $"({andamento.Posicao:N0} de {andamento.Total:N0})";
        if (andamento.Espera is TimeSpan espera)
        {
            return $"A Caixa pediu uma pausa. Nova tentativa do concurso {andamento.Numero} às {agora + espera:HH:mm} {posicao}…";
        }

        return $"Buscando a ordem do sorteio do concurso {andamento.Numero} {posicao}…";
    }

    /// <summary>
    /// Mensagem do desfecho do preenchimento.
    /// </summary>
    /// <param name="resultado">Desfecho do preenchimento.</param>
    /// <returns>Texto e gravidade da mensagem.</returns>
    public static AvisoAtualizacao Desfecho(ResultadoPreenchimentoOrdem resultado)
    {
        if (resultado.Total == 0)
        {
            return new AvisoAtualizacao(TodosComOrdem, Severity.Info);
        }

        string preenchidos = Concursos(resultado.Preenchidos);
        string restantes = ContinuamSemOrdem(resultado.Restantes);
        string situacao = $"Ordem preenchida em {preenchidos}; {restantes}.";

        switch (resultado.Desfecho)
        {
            case DesfechoPreenchimento.EncerradoPorFalhas:
                return new AvisoAtualizacao(
                    $"Preenchimento encerrado após {OrdemSorteioServico.FalhasSeguidasParaEncerrar} falhas seguidas. "
                        + $"{resultado.Falhas[^1].Mensagem} {situacao}",
                    Severity.Error);

            case DesfechoPreenchimento.EncerradoPorLimite:
                return new AvisoAtualizacao(
                    $"Preenchimento encerrado: a Caixa continua limitando as consultas. {resultado.Falhas[^1].Mensagem} {situacao}",
                    Severity.Error);

            case DesfechoPreenchimento.Interrompido:
                return new AvisoAtualizacao(
                    $"Preenchimento interrompido: ordem preenchida em {preenchidos}; {restantes}.",
                    Severity.Info);
        }

        if (resultado.SemOrdem == 0)
        {
            return new AvisoAtualizacao($"Ordem do sorteio preenchida em {preenchidos}.", Severity.Success);
        }

        string semOrdem = resultado.SemOrdem == 1
            ? "1 concurso ficou sem ordem e pode ser buscado de novo"
            : $"{resultado.SemOrdem:N0} concursos ficaram sem ordem e podem ser buscados de novo";

        return new AvisoAtualizacao($"Ordem do sorteio preenchida em {preenchidos}; {semOrdem}.", Severity.Warning);
    }

    /// <summary>
    /// Quantidade de concursos por extenso, com separador de milhar.
    /// </summary>
    /// <param name="quantidade">Quantidade de concursos.</param>
    /// <returns>"1 concurso" ou "&lt;n&gt; concursos".</returns>
    private static string Concursos(int quantidade)
    {
        return quantidade == 1 ? "1 concurso" : $"{quantidade:N0} concursos";
    }

    /// <summary>
    /// Quantos concursos continuam sem ordem, com o verbo concordando com a quantidade.
    /// </summary>
    /// <param name="quantidade">Concursos que continuam sem ordem.</param>
    /// <returns>"1 concurso continua sem ordem" ou "&lt;n&gt; concursos continuam sem ordem".</returns>
    private static string ContinuamSemOrdem(int quantidade)
    {
        return quantidade == 1 ? "1 concurso continua sem ordem" : $"{quantidade:N0} concursos continuam sem ordem";
    }
}
