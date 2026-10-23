// <copyright file="AvisosAtualizacao.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Aplicacao.Concursos;
using Lotofacil.Dominio.Concursos;
using MudBlazor;

namespace Lotofacil.Web.Components.Concursos;

/// <summary>
/// Textos do andamento e do desfecho da atualização dos resultados, usados pelas páginas Resultados e Painel.
/// </summary>
public static class AvisosAtualizacao
{
    /// <summary>
    /// Andamento exibido antes de saber quantos concursos serão buscados.
    /// </summary>
    public const string Consultando = "Consultando o último concurso da Caixa…";

    /// <summary>
    /// Motivo exibido quando a falha não foi prevista; o detalhe vai para o log.
    /// </summary>
    public const string MotivoErroInesperado = "Erro inesperado; os detalhes foram registrados no log.";

    /// <summary>
    /// Início da mensagem de falha da atualização.
    /// </summary>
    private const string PrefixoFalha = "A atualização foi interrompida.";

    /// <summary>
    /// Mensagem completa de uma falha não prevista na atualização.
    /// </summary>
    public static string FalhaInesperada => $"{PrefixoFalha} {MotivoErroInesperado}";

    /// <summary>
    /// Texto do andamento: qual concurso está sendo buscado.
    /// </summary>
    /// <param name="andamento">Concurso sendo buscado e sua posição.</param>
    /// <returns>Texto do andamento.</returns>
    public static string Andamento(AndamentoAtualizacao andamento)
    {
        return $"Buscando o concurso {andamento.Numero} ({andamento.Posicao} de {andamento.Total})…";
    }

    /// <summary>
    /// Mensagem do desfecho da atualização.
    /// </summary>
    /// <param name="resultado">Desfecho da atualização.</param>
    /// <returns>Texto e gravidade da mensagem.</returns>
    public static AvisoAtualizacao Desfecho(ResultadoAtualizacao resultado)
    {
        int gravados = resultado.ConcursosGravados;
        int ultimo = resultado.UltimoNumeroNaBase;

        if (resultado.BaseVazia)
        {
            return new AvisoAtualizacao(
                "A base local está vazia. Use “Importar histórico” antes de atualizar os resultados.",
                Severity.Warning);
        }

        if (resultado.Falha is FonteResultadosException falha)
        {
            string mensagem = $"{PrefixoFalha} {falha.Message}";
            if (gravados > 0)
            {
                mensagem += $" Concursos gravados antes da falha: {gravados:N0} (até o {ultimo}).";
            }

            return new AvisoAtualizacao(mensagem, Severity.Error);
        }

        if (gravados == 0)
        {
            return new AvisoAtualizacao($"A base local já está atualizada: último concurso {ultimo}.", Severity.Info);
        }

        if (gravados == 1)
        {
            return new AvisoAtualizacao($"Atualização concluída: 1 concurso novo gravado ({ultimo}).", Severity.Success);
        }

        return new AvisoAtualizacao(
            $"Atualização concluída: {gravados:N0} concursos novos gravados ({resultado.PrimeiroGravado} a {ultimo}).",
            Severity.Success);
    }
}
